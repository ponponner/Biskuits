using Biskuits.Helpers.Observables;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace UnitTests.Helpers.Observables
{
    [TestFixture]
    partial class ObservableObjectTest
    {
        class ModelForRaisePropertyChangedTest : ObservableObject
        {
            /// プロパティ名の命名規則
            /// - Prop1*: <seealso cref="ObservableObject.RaisePropertyChanged(string)"/>のテストケース用プロパティ
            /// - Prop2*: <seealso cref="ObservableObject.RaisePropertyChanged(PropertyChangedEventArgs)"/>のテストケース用プロパティ

            public static char CheckChar = 'X'; // "X": 指定した文字列が使われたかチェックするための文字

            public static PropertyChangedEventArgs Prop21Pcea = new PropertyChangedEventArgs($"{CheckChar}{nameof(Prop21)}");

            /// <summary>
            /// <seealso cref="ObservableObject.RaisePropertyChanged(string)"/>
            /// </summary>
            public int Prop11
            {
                get { return _Prop11; }
                set
                {
                    _Prop11 = value;
                    RaisePropertyChanged();
                }
            }
            public int Prop12
            {
                get { return _Prop12; }
                set
                {
                    _Prop12 = value;
                    RaisePropertyChanged($"{CheckChar}{nameof(Prop12)}");
                }
            }
            int _Prop11 = 0;
            int _Prop12 = 0;

            /// <summary>
            /// <seealso cref="ObservableObject.RaisePropertyChanged(PropertyChangedEventArgs)"/>
            /// </summary>
            public int Prop21
            {
                get { return _Prop21; }
                set
                {
                    _Prop21 = value;
                    RaisePropertyChanged(Prop21Pcea);
                }
            }
            public int Prop22
            {
                get { return _Prop22; }
                set
                {
                    _Prop22 = value;
                    RaisePropertyChanged(new PropertyChangedEventArgs($"{CheckChar}{nameof(Prop22)}"));
                }
            }
            public int Prop23
            {
                get { return _Prop23; }
                set
                {
                    _Prop23 = value;
                    RaisePropertyChanged(new PropertyChangedEventArgs(null));
                }
            }
            int _Prop21 = 0;
            int _Prop22 = 0;
            int _Prop23 = 0;

            public int EventFiredCount = 0;
            public string LastChangedPropertyName { get; private set; } = "EventFiredYet";

            public ModelForRaisePropertyChangedTest()
            {
                // イベントを購読し, その発火について記憶します
                this.PropertyChanged += (sender, args) =>
                {
                    // イベント発火の旨をコンソール出力します
                    Console.WriteLine($"ProgressChangedEventが発火しました({args.PropertyName ?? "null"})。");

                    // イベント発火回数を記憶します
                    EventFiredCount += 1;

                    // イベント引数が持つプロパティ名を記憶します
                    LastChangedPropertyName = args.PropertyName;
                };
            }
        }

        /// <summary>
        /// テストケース (プロパティ名, イベント引数が持つべきプロパティ名)
        /// </summary>
        static readonly object[] RaisePropertyChangedTestCases =
        {
            // Prop1*
            new object[] {nameof(ModelForSetPropertyTest.Prop11), nameof(ModelForRaisePropertyChangedTest.Prop11) },
            new object[] {nameof(ModelForSetPropertyTest.Prop12), $"{ModelForRaisePropertyChangedTest.CheckChar}{nameof(ModelForSetPropertyTest.Prop12)}" },
            // Prop2*
            new object[] {nameof(ModelForSetPropertyTest.Prop21), ModelForRaisePropertyChangedTest.Prop21Pcea.PropertyName },
            new object[] {nameof(ModelForSetPropertyTest.Prop22), $"{ModelForRaisePropertyChangedTest.CheckChar}{nameof(ModelForSetPropertyTest.Prop22)}" },
            new object[] {nameof(ModelForSetPropertyTest.Prop23), null},
        };

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [TestCaseSource(nameof(RaisePropertyChangedTestCases))]
        public void TestRaisePropertyChanged(string propertyName, string eventArgPropertyName)
        {
            // モデルを生成します
            var model = new ModelForSetPropertyTest();

            // 諸情報を記憶します
            var propertyInfo = model.GetType().GetProperty(propertyName);
            int oldValue = (int)propertyInfo.GetValue(model);
            int desiredValue = oldValue + 1;

            // プロパティに希望値をセットします
            propertyInfo.SetValue(model, desiredValue);

            // プロパティの値, イベント発火, イベント引数 を確認します
            Assert.That((int)propertyInfo.GetValue(model), Is.EqualTo(desiredValue));
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            Assert.That(model.LastChangedPropertyName, Is.EqualTo(eventArgPropertyName));

            // プロパティに希望値をセットします (同じ値のためセットされず, イベントは不発となります)
            propertyInfo.SetValue(model, desiredValue);

            // イベントの不発を確認します
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
        }
    }
}
