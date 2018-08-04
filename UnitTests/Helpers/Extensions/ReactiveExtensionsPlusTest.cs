using Biskuits.Helpers.Extensions;
using Biskuits.Helpers.Observables;
using NUnit.Framework;
using System;
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

        [Test]
        public void TestAddTo()
        {
            var model01 = new ObservableModel();
            var model02 = new ObservableModel();
            var observer01 = new Observer<PropertyChangedEventArgs>();
            var observer02 = new Observer<PropertyChangedEventArgs>();

            CompositeDisposable disposables = new CompositeDisposable();

            model01
                .ObserveEvent<PropertyChangedEventArgs>(nameof(model01.PropertyChanged))
                .Subscribe(observer01.OnObservedEvent)
                .AddTo(disposables);
            model02
                .ObserveEvent<PropertyChangedEventArgs>(nameof(model01.PropertyChanged))
                .Subscribe(observer02.OnObservedEvent)
                .AddTo(disposables);

            // イベントを発信および受信できることを確認する
            model01.Qty = 8;
            model02.Qty = 8;
            Assert.That(observer01.LastEventArgs, Is.Not.Null);
            Assert.That(observer02.LastEventArgs, Is.Not.Null);

            // イベントをまとめて購読解除できることを確認する
            disposables.Dispose();
            observer01.LastEventArgs = null;
            observer02.LastEventArgs = null;
            model01.Qty = 16;
            model02.Qty = 16;
            Assert.That(observer01.LastEventArgs, Is.Null);
            Assert.That(observer02.LastEventArgs, Is.Null);
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

        [Test]
        public void TestObserveEvent()
        {
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            model
                .ObserveEvent<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
                .Subscribe(e => observer.OnObservedEvent(e))
                .AddTo(observer.Subscriptions);

            // イベントを発信および受信できることを確認する
            model.Qty = 8;
            Assert.That(observer.LastEventArgs, Is.Not.Null);

            // イベントを購読解除できることを確認する
            observer.Subscriptions.Dispose();
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        [Test]
        public void TestObservePropertyChanged_01()
        {
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            var subscription =
                model
                .ObservePropertyChanged()
                .Subscribe(e => observer.OnObservedEvent(e));
            observer.Subscriptions.Add(subscription);

            // イベントを発信および受信できることを確認する
            // イベント引数が正しいことを確認する
            model.Qty = 8;
            Assert.That(observer.LastEventArgs.PropertyName, Is.EqualTo(nameof(model.Qty)));

            // イベントを購読解除できることを確認する
            observer.Subscriptions.Dispose();
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        [Test]
        public void TestObservePropertyChanged_02()
        {
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            // イベント購読が失敗することを確認する
            Assert.Throws<ArgumentException>(() => { model.ObservePropertyChanged(o => "notPropertyName"); });
            Assert.Throws<ArgumentException>(() => { model.ObservePropertyChanged(o => o.GetType()); });

            // イベント購読が成功することを確認する
            var subscription =
                model
                .ObservePropertyChanged(o => o.Qty)
                .Subscribe(e => observer.OnObservedEvent(e));
            observer.Subscriptions.Add(subscription);

            // イベントを発信および受信できることを確認する
            // イベント引数が正しいことを確認する
            model.Qty = 8;
            Assert.That(observer.LastEventArgs.PropertyName, Is.EqualTo(nameof(model.Qty)));

            // イベントを購読解除できることを確認する
            observer.Subscriptions.Dispose();
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        [Test]
        public void TestObserveCollectionChanged()
        {
            var modelList = new ModelList<int>() { 1, 2, 3 };
            var observer = new Observer<NotifyCollectionChangedEventArgs>();

            var subscription =
                modelList
                .ObserveCollectionChanged()
                .Subscribe(observer.OnObservedEvent);
            observer.Subscriptions.Add(subscription);

            // イベントを発信および受信できることを確認する
            // イベント引数が正しいことを確認する
            modelList.Add(8);
            Assert.That(observer.LastEventArgs.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

            // イベントを購読解除できることを確認する
            observer.Subscriptions.Dispose();
            observer.LastEventArgs = null;
            modelList.Add(16);
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        // 基本的なテストケース
        [Test]
        public void TestSubscribeWeakly01_BasicalTests()
        {
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();

            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);
            observer.Subscriptions.Add(subscription);

            // イベントを受信できることを確認する
            // イベント引数が正しいことを確認する
            model.Qty = 8;
            Assert.That(observer.LastEventArgs.PropertyName, Is.EqualTo(nameof(model.Qty)));

            // イベントを購読解除できることを確認する
            observer.Subscriptions.Dispose();
            observer.LastEventArgs = null;
            model.Qty = 16;
            Assert.That(observer.LastEventArgs, Is.Null);
        }

        // 弱参照を確認するテストケース
        [Test]
        public void TestSubscribeWeakly02_TestWeakness()
        {
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();
            var observerWeakReference = new WeakReference(observer);

            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);
            observer.Subscriptions.Add(subscription);

            // イベント受信元に対する強参照が存在しなければ、
            // イベント購読中であってもGCに回収されえることを確認する
            // （言い換えると、イベントハンドラの参照方法が弱参照であることを確認する）
            observer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.That(observerWeakReference.IsAlive, Is.False);
        }

        // subscriptionが発信元に対して強参照であることを確認するテストケース
        [Test]
        public void TestSubscribeWeakly03()
        {
            var model = new ObservableModel();
            var modelWeakReference = new WeakReference(model);
            var observer = new Observer<PropertyChangedEventArgs>();

            var subscription = model
            .ObservePropertyChanged()
            .SubscribeWeakly(observer.OnObservedEvent);

            var checkIsModelAlive = new Func<bool>(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return modelWeakReference.IsAlive;
            });

            // 参照を破棄する
            model = null;
            observer = null;
            Assert.That(checkIsModelAlive.Invoke(), Is.True);

            // イベント購読解除を行わないと、発信元が破棄されないことを確認する
            subscription.Dispose();
            Assert.That(checkIsModelAlive.Invoke(), Is.False);
        }

        // subscriptionの自動破棄について確認するテストケース
        [Test]
        public void TestSubscribeWeakly04()
        {
            var model = new ObservableModel();
            var modelWeakReference = new WeakReference(model);
            var observer = new Observer<PropertyChangedEventArgs>();

            var subscription =
                model
                .ObservePropertyChanged()
                .SubscribeWeakly(observer.OnObservedEvent);

            // 購読元が破棄されるとき、購読解除されるようにしておく
            observer.Subscriptions.Add(subscription);

            // 購読元が破棄されるとき、購読解除されるようにしておくことで、
            // 同系列のテスト03で破棄されなかったタイミングで発信元が破棄されることを確認する
            model = null;
            observer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.That(modelWeakReference.IsAlive, Is.False);
        }

        // subscriptionが発信元に対して強参照であることを確認するテストケース（Subscribeの場合でも）
        [Test]
        public void TestSubscribeWeakly05()
        {
            var model = new ObservableModel();
            var observer = new Observer<PropertyChangedEventArgs>();
            var observerWeakReference = new WeakReference(observer);

            // イベントを弱購読ではなく"強購読"する
            var subscription =
                model
                .ObservePropertyChanged()
                // 強参照として購読する
                //.SubscribeWeakly(observer.OnObservedEvent);
                .Subscribe(observer.OnObservedEvent);
            observer.Subscriptions.Add(subscription);

            // イベント受信元に対する強参照が削除されても、
            // イベント購読中であればGCに回収されないことを確認する
            // （言い換えると、イベントハンドラの参照方法が強参照であることを確認する）
            observer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.That(observerWeakReference.IsAlive, Is.True);
        }

    }
}
