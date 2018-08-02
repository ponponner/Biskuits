using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;

namespace Biskuits.Helpers.Extensions
{
    public static class ReactiveExtensionsPlus
    {
        // ==================================================
        // ==================================================
        // ==================================================

        // 文言の統一
        //   ストリームの購読
        //   ストリームの購読解除
        //   ストリーム送信端末
        //   ストリーム受信端末

        // 参考サイト
        //   ReactiveExtensionおよびその応用、拡張メソッドについて
        //               https://qiita.com/Temarin/items/be5f9cea260580327700
        //   ReactiveExtensionでの弱参照イベントについて
        //               https://www.codeproject.com/Tips/1078183/Weak-events-in-NET-using-Reactive-Extensions-Rx


        // ObserveEvent ===============================

        /// <example><code>
        /// model
        ///     .ObserveEvent<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
        ///     .Subscribe(e => Console.WriteLine($"SomeEvent occurred."));
        /// </code></example>
        public static IObservable<EventPattern<TEventArgs>> ObserveEvent<TEventArgs>(this object source, string eventName)
        {
            return Observable.FromEventPattern<TEventArgs>(source, eventName);
        }


        // ObservePropertyChanged ================================

        /// <example><code>
        /// model
        ///     .ObservePropertyChanged()
        ///     .Subscribe(e => Console.WriteLine($"SomePropertyChangedEvent occurred."));
        /// </code></example>
        public static IObservable<EventPattern<PropertyChangedEventArgs>> ObservePropertyChanged(this INotifyPropertyChanged source)
        {
            return source.ObserveEvent<PropertyChangedEventArgs>(nameof(source.PropertyChanged));
        }

        /// <example><code>
        /// model
        ///     .ObservePropertyChanged(o => o.SomeProperty)
        ///     .Subscribe(e => Console.WriteLine($"PropertyChangedEvent of \"{e.EventArgs.PropertyName}\" occurred."));
        /// </code></example>
        public static IObservable<EventPattern<PropertyChangedEventArgs>> ObservePropertyChanged<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyExpr)
            where TSource : INotifyPropertyChanged
        {
            Contract.Requires(propertyExpr != null);

            var body = propertyExpr.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("式がプロパティを参照していません。", "propertyExpr");

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("式がプロパティを参照していません。", "propertyExpr");

            string propertyName = propertyInfo.Name;

            return source
                .ObserveEvent<PropertyChangedEventArgs>(nameof(source.PropertyChanged))
                .Where(e => e.EventArgs.PropertyName == propertyName);
        }


        // ObserveCollectionChanged ==============================

        /// <example><code>
        /// modelList
        ///     .ObserveCollectionChanged()
        ///     .Subscribe(e => Console.WriteLine($"CollectionChangedEvent({e.EventArgs.Action}) occurred."));
        /// </code></example>
        public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>> ObserveCollectionChanged(this INotifyCollectionChanged source)
        {
            return source.ObserveEvent<NotifyCollectionChangedEventArgs>(nameof(source.CollectionChanged));
        }


        // SubscribeWeakly =======================================

        /// <summary>
        /// この拡張メソッドにより<paramref name="observable"/>を購読します。
        /// その際、与えられたハンドラ<paramref name="onNext"/>を弱参照でラップし、ハンドラを強参照しないようにします。
        /// </summary>
        public static IDisposable SubscribeWeakly<TEventArgs>(this IObservable<EventPattern<TEventArgs>> observable, Action<EventPattern<TEventArgs>> onNext)
        {
            var onNextWeakReference = new WeakReference<Action<EventPattern<TEventArgs>>>(onNext);

            IDisposable subscribe = null;
            subscribe = observable.Subscribe(e => StaticEventHandlerForSubscribeWeakly(onNextWeakReference, e, () => subscribe));
            return subscribe;
        }

        /// <summary>
        /// このメソッドはIObservable&lt;EventPattern&lt;<typeparamref name="TEventArgs"/>&gt;&gt;のハンドラから強参照され、そしてまた
        /// 弱参照でラップされたハンドラの呼び出しを行う、ハンドラの間を取り持つハンドラです。
        /// </summary>
        private static void StaticEventHandlerForSubscribeWeakly<TEventArgs>(WeakReference<Action<EventPattern<TEventArgs>>> onNextWeakReference, EventPattern<TEventArgs> eventPattern, Func<IDisposable> getSubscribe)
        {
            Action<EventPattern<TEventArgs>> currentOnNext;

            if (onNextWeakReference.TryGetTarget(out currentOnNext))
            {
                currentOnNext(eventPattern);
            }
            else
            {
                getSubscribe.Invoke().Dispose();
            }
        }

        /// <summary>
        /// この拡張メソッドにより<paramref name="observable"/>を購読します。
        /// その際、与えられたハンドラ<paramref name="onNext"/>を弱参照でラップし、ハンドラを強参照しないようにします。
        /// </summary>
        public static IDisposable SubscribeWeakly02<TEventArgs>(this IObservable<EventPattern<TEventArgs>> observable, Action<EventPattern<TEventArgs>> onNext)
        {
            IDisposable subscribe = null;
            subscribe = observable.Subscribe(e => StaticEventHandlerForSubscribeWeakly02(onNext, e, () => subscribe.Dispose()));
            return subscribe;
        }

        /// <summary>
        /// このメソッドはIObservable&lt;EventPattern&lt;<typeparamref name="TEventArgs"/>&gt;&gt;のハンドラから強参照され、そしてまた
        /// 弱参照でラップされたハンドラの呼び出しを行う、ハンドラの間を取り持つハンドラです。
        /// </summary>
        private static void StaticEventHandlerForSubscribeWeakly02<TEventArgs>(Action<EventPattern<TEventArgs>> onNext, EventPattern<TEventArgs> eventPattern, Action unsubscribe)
        {
            Console.WriteLine($"onNext:{onNext==null}");
            if (onNext != null)
            {
                onNext(eventPattern);
            }
            else
            {
                unsubscribe();
            }
        }


        // AddTo CompositeDisposable ========================

        /// <summary>
        /// この拡張メソッドは<see cref="IObservable{T}"/>を購読した後などに
        /// 利用するもので、<see cref="CompositeDisposable"/>に<see cref="IDisposable"/>を追加します。
        /// </summary>
        /// <example><code>
        /// model
        ///     .AsFromEventPattern<PropertyChangedEventArgs>(nameof(model.PropertyChanged))
        ///     .Subscribe(e => Console.WriteLine($"SomeEvent occurred."))
        ///     .AddTo(disposables);
        /// </code></example>
        public static void AddTo<TSource>(this TSource source, CompositeDisposable compositeDisposable)
            where TSource : IDisposable
        {
            compositeDisposable.Add(source);
        }

        public static void AddToWeakly<TSource>(this TSource source, CompositeDisposable compositeDisposable)
            where TSource : IDisposable
        {
            compositeDisposable.Add(source);
        }

        class WeakDisposable : IDisposable
        {
            public WeakReference<IDisposable> WeakReference;
            public WeakDisposable(IDisposable target)
            {
                WeakReference = new WeakReference<IDisposable>(target);
            }
            public void Dispose()
            {
                IDisposable target;
                if (WeakReference.TryGetTarget(out target))
                {
                    target.Dispose();
                    WeakReference.SetTarget(null);
                }
            }
        }

    }
}
