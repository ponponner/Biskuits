using Biskuits.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnitTests.Helpers.Extensions
{
    [TestFixture]
    class ReflectionExtensionsTest
    {
        [Test]
        public void TestGetPropertyType_OK()
        {
            var list = new List<int>() { 1, 2, 3 };
            var type = list.GetPropertyType(o => o.Count);
            Assert.That(type, Is.EqualTo(list.Count.GetType()));
        }

        [Test]
        public void TestGetPropertyType_NG()
        {
            var list = new List<int>() { 1, 2, 3 };
            Assert.Throws<ArgumentException>(() => list.GetPropertyType(o => o));
        }
    }
}
