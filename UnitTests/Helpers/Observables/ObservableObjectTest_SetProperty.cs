using Biskuits.Helpers.Observables;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace UnitTests.Helpers.Observables
{
    [TestFixture]
    partial class ObservableObjectTest
    {
        class ModelForSetPropertyTest : ObservableObject
        {
            /// プロパティ名の命名規則
            /// - Prop1*: <seealso cref="ObservableObject.SetProperty{T}(ref T, T, string)"/>のテストケース用プロパティ
            /// - Prop2*: <seealso cref="ObservableObject.SetProperty{T}(ref T, T, PropertyChangedEventArgs)"/>のテストケース用プロパティ
            /// - Prop3*: <seealso cref="ObservableObject.SetProperty{T}(ref T, T, Action, string)"/>のテストケース用プロパティ
            /// - Prop4*: <seealso cref="ObservableObject.SetProperty{T}(ref T, T, Action, PropertyChangedEventArgs)"/>のテストケース用プロパティ

            public static char CheckChar = 'X'; // "X": 指定した文字列が使われたかチェックするための文字

            public static PropertyChangedEventArgs Prop21Pcea = new PropertyChangedEventArgs($"{CheckChar}{nameof(Prop21)}");
            public static PropertyChangedEventArgs Prop41Pcea = new PropertyChangedEventArgs($"{CheckChar}{nameof(Prop41)}");

            /// <summary>
            /// <seealso cref="ObservableObject.SetProperty{T}(ref T, T, string)"/>のテストケース用プロパティ
            /// </summary>
            public int Prop11
            {
                get { return _Prop11; }
                set { SetProperty(ref _Prop11, value); }
            }
            public int Prop12
            {
                get { return _Prop12; }
                set { SetProperty(ref _Prop12, value, $"{CheckChar}{nameof(ModelForSetPropertyTest.Prop12)}"); }
            }
            int _Prop11 = 0;
            int _Prop12 = 0;

            /// <summary>
            /// <seealso cref="ObservableObject.SetProperty{T}(ref T, T, PropertyChangedEventArgs)"/>のテストケース用プロパティ
            /// </summary>
            public int Prop21
            {
                get { return _Prop21; }
                set { SetProperty(ref _Prop21, value, Prop21Pcea); }
            }
            public int Prop22
            {
                get { return _Prop22; }
                set { SetProperty(ref _Prop22, value, new PropertyChangedEventArgs($"{CheckChar}{nameof(Prop22)}")); }
            }
            public int Prop23
            {
                get { return _Prop23; }
                set { SetProperty(ref _Prop23, value, new PropertyChangedEventArgs(null)); }
            }
            int _Prop21 = 0;
            int _Prop22 = 0;
            int _Prop23 = 0;

            /// <summary>
            /// <seealso cref="ObservableObject.SetProperty{T}(ref T, T, Action, string)"/>のテストケース用プロパティ
            /// </summary>
            public int Prop31
            {
                get { return _Prop31; }
                set { SetProperty(ref _Prop31, value, () => { OnSetProperty(nameof(Prop31)); }); }
            }
            public int Prop32
            {
                get { return _Prop32; }
                set { SetProperty(ref _Prop32, value, () => { OnSetProperty(nameof(Prop32)); }, $"{CheckChar}{nameof(Prop32)}"); }
            }
            int _Prop31 = 0;
            int _Prop32 = 0;

            /// <summary>
            /// <seealso cref="ObservableObject.SetProperty{T}(ref T, T, Action, PropertyChangedEventArgs)"/>のテストケース用プロパティ
            /// </summary>
            public int Prop41
            {
                get { return _Prop41; }
                set { SetProperty(ref _Prop41, value, () => { OnSetProperty(nameof(Prop41)); }, Prop41Pcea); }
            }
            public int Prop42
            {
                get { return _Prop42; }
                set { SetProperty(ref _Prop42, value, () => { OnSetProperty(nameof(Prop42)); }, new PropertyChangedEventArgs($"{CheckChar}{nameof(Prop42)}")); }
            }
            int _Prop41 = 0;
            int _Prop42 = 0;

            public int EventFiredCount { get; private set; } = 0;
            public int ActionFiredCount { get; private set; } = 0;
            public string LastChangedPropertyName { get; private set; } = "EventFiredYet";

            public ModelForSetPropertyTest()
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

            void OnSetProperty(string propertyName)
            {
                // アクション発火の旨をコンソール出力します
                Console.WriteLine($"SetPropertyのアクションが発火しました({propertyName})。");

                // アクション発火回数を記憶します
                ActionFiredCount += 1;
            }
        }

        /// <summary>
        /// テストケース (プロパティ名, イベント引数が持つべきプロパティ名, 値セット時のアクションの有無)
        /// </summary>
        static readonly object[] SetPropertyTestCases =
        {
            // Prop1*
            new object[] {nameof(ModelForSetPropertyTest.Prop11), nameof(ModelForSetPropertyTest.Prop11), false },
            new object[] {nameof(ModelForSetPropertyTest.Prop12), $"{ModelForSetPropertyTest.CheckChar}{nameof(ModelForSetPropertyTest.Prop12)}", false },
            // Prop2*
            new object[] {nameof(ModelForSetPropertyTest.Prop21), ModelForSetPropertyTest.Prop21Pcea.PropertyName, false },
            new object[] {nameof(ModelForSetPropertyTest.Prop22), $"{ModelForSetPropertyTest.CheckChar}{nameof(ModelForSetPropertyTest.Prop22)}", false },
            new object[] {nameof(ModelForSetPropertyTest.Prop23), null, false },
            // Prop3*
            new object[] {nameof(ModelForSetPropertyTest.Prop31), nameof(ModelForSetPropertyTest.Prop31), true },
            new object[] {nameof(ModelForSetPropertyTest.Prop32), $"{ModelForSetPropertyTest.CheckChar}{nameof(ModelForSetPropertyTest.Prop32)}", true },
            // Prop4*
            new object[] {nameof(ModelForSetPropertyTest.Prop41), ModelForSetPropertyTest.Prop41Pcea.PropertyName, true },
            new object[] {nameof(ModelForSetPropertyTest.Prop42), $"{ModelForSetPropertyTest.CheckChar}{nameof(ModelForSetPropertyTest.Prop42)}", true },
        };

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [TestCaseSource(nameof(SetPropertyTestCases))]
        public void TestSetProperty(string propertyName, string eventArgPropertyName, bool hasAction)
        {
            // モデルを生成します
            var model = new ModelForSetPropertyTest();

            // 諸情報を記憶します
            var propertyInfo = model.GetType().GetProperty(propertyName);
            int oldValue = (int)propertyInfo.GetValue(model);
            int desiredValue = oldValue + 1;

            // プロパティに希望値をセットします
            propertyInfo.SetValue(model, desiredValue);

            // プロパティの値, イベント発火, イベント引数, アクション実行 を確認します
            Assert.That((int)propertyInfo.GetValue(model), Is.EqualTo(desiredValue));
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            Assert.That(model.LastChangedPropertyName, Is.EqualTo(eventArgPropertyName));
            if (hasAction)
                Assert.That(model.ActionFiredCount, Is.EqualTo(1));

            // プロパティに希望値をセットします (同じ値のためセットされず, イベント, アクションは不発となります)
            propertyInfo.SetValue(model, desiredValue);

            // イベントの不発, アクション不発 を確認します
            Assert.That(model.EventFiredCount, Is.EqualTo(1));
            if (hasAction)
                Assert.That(model.ActionFiredCount, Is.EqualTo(1));
        }
    }
}
