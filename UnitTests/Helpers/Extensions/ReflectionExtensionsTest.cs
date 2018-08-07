using Biskuits.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnitTests.Helpers.Extensions
{
    [TestFixture]
    class ReflectionExtensionsTest
    {
        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [Test]
        public void TestGetPropertyType()
        {
            var list = new List<int>() { 1, 2, 3 };

            // プロパティの型を取得します
            var type = list.GetPropertyType(o => o.Count);

            // 正しい型を取得できたことを確認します
            Assert.That(type, Is.EqualTo(list.Count.GetType()));
        }

        /// <summary>
        /// メソッド利用に失敗する流れをテストします
        /// </summary>
        [Test]
        public void TestGetPropertyType_Fail()
        {
            var list = new List<int>() { 1, 2, 3 };

            // プロパティの型の取得失敗を確認します
            Assert.Throws<ArgumentException>(() => list.GetPropertyType(o => o));
        }
    }
}
