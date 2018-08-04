using Biskuits.Helpers.Extensions;
using Biskuits.Helpers.Observables;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;

namespace UnitTests.Helpers.Extensions
{
    [TestFixture]
    class ReactiveExtensionsPlusTest
    {
        class ObservableModel : ObservableObject
        {
            public string Name
            {
                get { return _Name; }
                set { SetProperty(ref _Name, value); }
            }
            string _Name = "Apple";

            public int Qty
            {
                get { return _Qty; }
                set { SetProperty(ref _Qty, value); }
            }
            int _Qty = 0;
        }

        class Observer<TEventArgs>
        {
            public TEventArgs LastEventArgs;
            public readonly CompositeDisposable Subscriptions = new CompositeDisposable();

            ~Observer()
            {
                Subscriptions.Dispose();
            }

            public void OnObservedEvent(EventPattern<TEventArgs> e)
            {
                LastEventArgs = e.EventArgs;
                Console.WriteLine($"Event Occurred(EventArgsType={typeof(TEventArgs)}).");
            }
        }

        class ModelList<TItem> : ObservableCollection<TItem>
        {
        }

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [Test]
        public void TestAddTo()
        {
            var count = 10;
            var tuples = new List<Tuple<ObservableModel, Observer<PropertyChangedEventArgs>>>();
            CompositeDisposable disposables = new CompositeDisposable();

            for (var i = 0; i < count; i++)
            {
                // 被観測者, 観測者を生成します
                var model = new ObservableModel();
                var observer = new Observer<PropertyChangedEventArgs>();
                tuples.Add(new Tuple<ObservableModel, Observer<PropertyChangedEventArgs>>(model, observer));

                // イベントを購読します
                model
                    .ObserveEvent<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
                    .Subscribe(observer.OnObservedEvent)
                    .AddTo(disposables);

                // イベントを送受信することで, 以上の動作を確認します
                model.Qty = 8;
                Assert.That(observer.LastEventArgs, Is.Not.Null);
            }

            // イベント購読をまとめて解除します
            disposables.Dispose();

            for (var i = 0; i < count; i++)
            {
                (var model, var observer) = tuples[i];

                // イベントを受信できないことで, 以上の動作を確認します
                observer.LastEventArgs = null;
                model.Qty = 16;
                Assert.That(observer.LastEventArgs, Is.Null);
            }
        }

        // AddToWeaklyを実装する目的を見失ったため廃止した
        //[Test]
        //public void TestAddToWeakly_Unsubscribe()
        //{
        //    var model = new ObservableModel();
        //    var modelWeakReference = new WeakReference(model);
        //    var observer = new Observer<PropertyChangedEventArgs>();
        //
        //    var subscriptions = new CompositeDisposable();
        //    model
        //        .ObserveEvent<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
        //        .SubscribeWeakly(observer.OnObservedEvent)
        //        .AddToWeakly(subscriptions);
        //
        //    // イベントを発信および受信できることを確認する
        //    model.Qty = 8;
        //    Assert.That(observer.LastEventArgs, Is.Not.Null);
        //
        //    // イベント購読解除できることを確認する
        //    subscriptions.Dispose();
        //    observer.LastEventArgs = null;
        //    model.Qty = 16;
        //    Assert.That(observer.LastEventArgs, Is.Null);
        //}

        // AddToWeaklyを実装する目的を見失ったため廃止した
        //[Test]
        //public void TestAddToWeakly_Weakly()
        //{
        //    var model = new ObservableModel();
        //    var modelWeakReference = new WeakReference(model);
        //    var observer = new Observer<PropertyChangedEventArgs>();
        //
        //    var subscription =
        //        model
        //        .ObserveEvent<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
        //        .SubscribeWeakly(observer.OnObservedEvent);
        //    var subscriptionWeakReference = new WeakReference(subscription);
        //
        //    var subscriptions = new CompositeDisposable();
        //    subscription.AddToWeakly(subscriptions);
        //
        //    // イベントを発信および受信できることを確認する
        //    model.Qty = 8;
        //    Assert.That(observer.LastEventArgs, Is.Not.Null);
        //
        //    // 発信元は、イベント購読解除を行わずとも破棄されえることを確認する
        //    model = null;
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    Assert.That(modelWeakReference.IsAlive, Is.False);
        //
        //    // イベント購読解除が空実行されることを確認する
        //    subscriptions.Dispose();
        //
        //    // オブジェクトの延命
        //    //Console.WriteLine(observer);
        //    //Console.WriteLine(subscription);
        //}

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [Test]
        public void TestObserveEvent()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            // イベントを購読します
            var subscription =
                model
                .ObserveEvent<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
                .Subscribe(e => observer.OnObservedEvent(e));
            observer.Subscriptions.Add(subscription);

            // イベントを送受信することで, 以上の動作を確認します
            model.Qty = 8;
            Assert.That(observer.LastEventArgs, Is.Not.Null);

            // イベント購読を解除します
            observer.Subscriptions.Dispose();

            // イベントを受信できないことで, 以上の動作を確認します
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [Test]
        public void TestObservePropertyChanged()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            // イベントを購読します
            var subscription =
                model
                .ObservePropertyChanged()
                .Subscribe(e => observer.OnObservedEvent(e));
            observer.Subscriptions.Add(subscription);

            // イベントを送受信することで, 以上の動作を確認します
            model.Qty = 8;
            Assert.That(observer.LastEventArgs.PropertyName, Is.EqualTo(nameof(model.Qty)));

            // イベント購読を解除します
            observer.Subscriptions.Dispose();

            // イベントを受信できないことで, 以上の動作を確認します
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        /// <summary>
        /// メソッド利用に失敗する流れをテストします
        /// </summary>
        [Test]
        public void TestObservePropertyChanged_Fail()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            // イベント購読が失敗することを確認します
            Assert.Throws<ArgumentException>(() => { model.ObservePropertyChanged(o => "notPropertyName"); });
            Assert.Throws<ArgumentException>(() => { model.ObservePropertyChanged(o => o.GetType()); });
        }

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [Test]
        public void TestObserveCollectionChanged()
        {
            // 被観測者, 観測者を生成します
            var modelList = new ModelList<int>() { 1, 2, 3 };
            var observer = new Observer<NotifyCollectionChangedEventArgs>();

            // イベントを購読します
            var subscription =
                modelList
                .ObserveCollectionChanged()
                .Subscribe(observer.OnObservedEvent);
            observer.Subscriptions.Add(subscription);

            // イベントを送受信することで, 以上の動作を確認します
            modelList.Add(8);
            Assert.That(observer.LastEventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            // イベント購読を解除します
            observer.Subscriptions.Dispose();

            // イベントを受信できないことで, 以上の動作を確認します
            observer.LastEventArgs = null;
            modelList.Add(16);
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        /// <summary>
        /// メソッド利用時の一連の流れをテストします
        /// </summary>
        [Test]
        public void TestSubscribeWeakly()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            // イベントを購読します
            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);
            observer.Subscriptions.Add(subscription);

            // イベントを送受信することで, 以上の動作を確認します
            model.Qty = 8;
            Assert.That(observer.LastEventArgs.PropertyName, Is.EqualTo(nameof(model.Qty)));

            // イベント購読を解除します
            observer.Subscriptions.Dispose();

            // イベントを受信できないことで, 以上の動作を確認します
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        /// <summary>
        /// ハンドラへの弱参照性を確認します
        /// </summary>
        [Test]
        public void TestSubscribeWeakly_ReferenceWeakness()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();
            var observerWeakReference = new WeakReference(observer);

            // イベントを購読します
            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);

            // 観測者を破棄. 回収します
            observer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 観測者が回収されたことで, ハンドラへの弱参照性を確認する
            Assert.That(observerWeakReference.IsAlive, Is.False);

            // オブジェクトを延命します
            Console.WriteLine(model);
            Console.WriteLine(subscription);
        }

        /// <summary>
        /// Subscriptionから被観測者への参照が, 強参照であることを確認します
        /// </summary>
        [Test]
        public void TestSubscribeWeakly_ReferenceStrongness()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var modelWeakReference = new WeakReference(model);
            var observer = new Observer<PropertyChangedEventArgs>();

            // イベントを購読します
            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);

            // 被観測者, 観測者への参照を破棄し, GCを実行します
            model = null;
            observer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 被観測者が破棄されないことを確認します
            Assert.That(modelWeakReference.IsAlive, Is.True);

            // イベントを購読解除します
            subscription.Dispose();

            // 被観測者が破棄されることで, Subscriptionから被観測者への参照が強参照でることを確認します
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.That(modelWeakReference.IsAlive, Is.False);

            // オブジェクトを延命します
            Console.WriteLine(subscription);
        }

        /// <summary>
        /// 自動での購読解除をテストします
        /// </summary>
        [Test]
        public void TestSubscribeWeakly_AutoUnsubscribe()
        {
            // 被観測者, 観測者を生成します
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();
            var observerWeakReference = new WeakReference(observer);

            // イベントを購読します
            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);
            var subscriptionWeakReference = new WeakReference(subscription);

            // サブスクリプションを観測者に持たせておき, 観測者が破棄されるとき自動的に購読解除されるようにします
            observer.Subscriptions.Add(subscription);

            // サブスクリプションへの参照を破棄します
            subscription = null;

            // 観測者を破棄します
            observer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 被観測者, サブスクリプションが回収されたことで, 自動での購読解除を確認します
            Assert.That(observerWeakReference.IsAlive, Is.False);
            Assert.That(subscriptionWeakReference.IsAlive, Is.False);

            // オブジェクトを延命します
            Console.WriteLine(model);
        }

    }
}
