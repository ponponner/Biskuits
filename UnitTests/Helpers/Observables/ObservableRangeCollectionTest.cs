using Biskuits.Helpers.Observables;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace UnitTests.Helpers.ObservableCollections
{
    [TestFixture]
    class ObservableRangeCollectionTest
    {
        [Test]
        public void TestNew_WithArgument()
        {
            var collection = new List<int>() { 1, 2, 3 };
            var orc = new ObservableRangeCollection<int>(collection);

            Assert.That(orc, Is.EqualTo(collection));
        }

        [Test]
        public void TestAddRange_Reset()
        {
            var collection_01 = new List<int>() { 1, 2, 3 };
            var collection_02 = new List<int>() { 4, 5 };
            var collection_marged = new List<int>();
            collection_marged.AddRange(collection_01);
            collection_marged.AddRange(collection_02);

            var orc = new ObservableRangeCollection<int>(collection_01);
            var isEventOccurred = false;
            orc.CollectionChanged += (sender, e) =>
            {
                isEventOccurred = true;
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            };

            orc.AddRange(collection_02, NotifyCollectionChangedAction.Reset);
            Assert.That(orc, Is.EqualTo(collection_marged));
            Assert.That(isEventOccurred);
        }

        [Test]
        public void TestAddRange()
        {
            var collection_01 = new List<int>() { 1, 2, 3 };
            var collection_02 = new List<int>() { 4, 5 };
            var collection_marged = new List<int>();
            collection_marged.AddRange(collection_01);
            collection_marged.AddRange(collection_02);

            var orc = new ObservableRangeCollection<int>(collection_01);
            var orcCount = orc.Count;
            var isEventOccurred = false;
            orc.CollectionChanged += (sender, e) =>
            {
                isEventOccurred = true;
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
                Assert.That(e.NewItems, Is.EqualTo(collection_02));
                Assert.That(e.NewStartingIndex, Is.EqualTo(orcCount));
                Assert.That(e.OldItems, Is.EqualTo(null));
                Assert.That(e.OldStartingIndex, Is.EqualTo(-1));
            };

            orc.AddRange(collection_02);
            Assert.That(orc, Is.EqualTo(collection_marged));
            Assert.That(isEventOccurred);
        }

        [Test]
        public void TestRemoveRange()
        {
            var collection_01 = new List<int>() { 1, 3, 5 };
            var collection_02 = new List<int>() { 4, 2 };
            var collection_marged = new List<int>();
            collection_marged.AddRange(collection_01);
            collection_marged.AddRange(collection_02);

            var orc = new ObservableRangeCollection<int>(collection_marged);
            var isEventOccurred = false;
            orc.CollectionChanged += (sender, e) =>
            {
                isEventOccurred = true;
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                Assert.That(e.NewItems, Is.EqualTo(null));
                Assert.That(e.NewStartingIndex, Is.EqualTo(-1));
                Assert.That(e.OldItems, Is.EqualTo(null));
                Assert.That(e.OldStartingIndex, Is.EqualTo(-1));
            };

            orc.RemoveRange(collection_02);
            Assert.That(orc, Is.EqualTo(collection_01));
            Assert.That(isEventOccurred);
        }

        [Test]
        public void TestReplaceAll_01()
        {
            var collection_01 = new List<int>() { 1, 2, 3 };
            var collection_02 = new List<int>() { 4, 5 };

            var orc = new ObservableRangeCollection<int>(collection_01);
            var isEventOccurred = false;
            orc.CollectionChanged += (sender, e) =>
            {
                isEventOccurred = true;
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                Assert.That(e.NewItems, Is.EqualTo(null));
                Assert.That(e.NewStartingIndex, Is.EqualTo(-1));
                Assert.That(e.OldItems, Is.EqualTo(null));
                Assert.That(e.OldStartingIndex, Is.EqualTo(-1));
            };

            orc.ReplaceAll(collection_02);
            Assert.That(orc, Is.EqualTo(collection_02));
            Assert.That(isEventOccurred);
        }

        [Test]
        public void TestReplaceAll_02()
        {
            var collection_01 = new List<int>() { 1, 2, 3 };
            var collection_02 = new List<int>() {};

            var orc = new ObservableRangeCollection<int>(collection_01);
            var isEventOccurred = false;
            orc.CollectionChanged += (sender, e) =>
            {
                isEventOccurred = true;
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                Assert.That(e.NewItems, Is.EqualTo(null));
                Assert.That(e.NewStartingIndex, Is.EqualTo(-1));
                Assert.That(e.OldItems, Is.EqualTo(null));
                Assert.That(e.OldStartingIndex, Is.EqualTo(-1));
            };

            orc.ReplaceAll(collection_02);
            Assert.That(orc, Is.EqualTo(collection_02));
            Assert.That(isEventOccurred);
        }

    }
}
