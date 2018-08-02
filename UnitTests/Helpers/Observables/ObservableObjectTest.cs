using Biskuits.Helpers.Observables;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace UnitTests.Helpers.Observables
{
    [TestFixture]
    class BindableBaseTest
    {
        class Model : ObservableObject
        {
            static PropertyChangedEventArgs Prop10Pcea = new PropertyChangedEventArgs(nameof(Prop10));
            static PropertyChangedEventArgs Prop20Pcea = new PropertyChangedEventArgs(nameof(Prop20));
            public int Prop00
            {
                get { return _Prop00; }
                set { SetProperty(ref _Prop00, value, new PropertyChangedEventArgs(null)); }
            }
            public int Prop10
            {
                get { return _Prop10; }
                set { SetProperty(ref _Prop10, value, Prop10Pcea); }
            }
            public int Prop11
            {
                get { return _Prop11; }
                set { SetProperty(ref _Prop11, value); }
            }
            public int Prop20
            {
                get { return _Prop20; }
                set { SetProperty(ref _Prop20, value, () => { SetValueAction(nameof(Prop20)); }, Prop20Pcea); }
            }
            public int Prop21
            {
                get { return _Prop21; }
                set { SetProperty(ref _Prop21, value, () => { SetValueAction(nameof(Prop21)); }); }
            }
            int _Prop00 = 0;
            int _Prop10 = 0;
            int _Prop11 = 0;
            int _Prop20 = 0;
            int _Prop21 = 0;

            public int SetValueActionCalledCount = 0;
            public int EventFiredCount = 0;
            public string LastChangedPropertyName { get; private set; } = "YetEventFired";

            public Model()
            {
                // イベント発火の確認のため、リスナーを登録する。
                this.PropertyChanged += (sender, args) =>
                {
                    Console.WriteLine($"PropertyChangedEventが発火しました({args.PropertyName ?? "null"})。");

                    // イベント発火回数を記録する。
                    EventFiredCount += 1;

                    // 変化のあったプロパティ名を記憶する。
                    LastChangedPropertyName = args.PropertyName;
                };
            }

            void SetValueAction(string propertyName)
            {
                Console.WriteLine($"SetValue中でアクションが発火しました({propertyName})。");

                // アクション発火回数を記録する。
                SetValueActionCalledCount += 1;
            }
        }

        [TestCase("Prop00")]
        public void TestSetProperty00(string propertyName)
        {
            var model = new Model();
            var propertyInfo = model.GetType().GetProperty(propertyName);
            int oldValue = (int)propertyInfo.GetValue(model);
            int desiredValue = oldValue + 1;

            // プロパティに値をセットする
            propertyInfo.SetValue(model, desiredValue);

            // プロパティの値を確認する
            // イベント発火回数を確認する
            // イベント引数を確認する
            Assert.That((int)propertyInfo.GetValue(model), Is.EqualTo(desiredValue));
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            Assert.That(model.LastChangedPropertyName, Is.Null);

            // プロパティに値をセットする
            propertyInfo.SetValue(model, desiredValue);

            // イベント発火回数を確認する（イベントが発火していないことを確認する）
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
        }

        [TestCase("Prop10")]
        [TestCase("Prop11")]
        public void TestSetProperty10(string propertyName)
        {
            var model = new Model();
            var propertyInfo = model.GetType().GetProperty(propertyName);
            int oldValue = (int)propertyInfo.GetValue(model);
            int desiredValue = oldValue + 1;

            // プロパティに値をセットする
            propertyInfo.SetValue(model, desiredValue);

            // プロパティの値を確認する
            // イベント発火回数を確認する
            // イベント引数を確認する
            Assert.That((int)propertyInfo.GetValue(model), Is.EqualTo(desiredValue));
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            Assert.That(model.LastChangedPropertyName, Is.EqualTo(propertyName));

            // プロパティに値をセットする
            propertyInfo.SetValue(model, desiredValue);

            // イベント発火回数を確認する（イベントが発火していないことを確認する）
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
        }

        [TestCase("Prop20")]
        [TestCase("Prop21")]
        public void TestSetProperty20(string propertyName)
        {
            var model = new Model();
            var propertyInfo = model.GetType().GetProperty(propertyName);
            int oldValue = (int)propertyInfo.GetValue(model);
            int desiredValue = oldValue + 1;

            // プロパティに値をセットする
            propertyInfo.SetValue(model, desiredValue);

            // プロパティの値を確認する
            // イベント発火回数を確認する
            // イベント引数を確認する
            // アクションの実行回数を確認する
            Assert.That((int)propertyInfo.GetValue(model), Is.EqualTo(desiredValue));
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            Assert.That(model.LastChangedPropertyName, Is.EqualTo(propertyName));
            Assert.That(model.SetValueActionCalledCount, Is.EqualTo(1));

            // プロパティに値をセットする
            propertyInfo.SetValue(model, desiredValue);

            // イベント発火回数を確認する（イベントが発火していないことを確認する）
            // アクションの実行回数を確認する（アクションが発火していないことを確認する）
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            Assert.That(model.SetValueActionCalledCount, Is.EqualTo(1));
        }
    }
}
