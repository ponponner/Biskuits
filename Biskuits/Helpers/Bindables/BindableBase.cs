using Biskuits.Helpers.ExternalImplements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Biskuits.Helpers.Bindables
{
    /// <summary>
    /// このクラスは単純なモデルのための<see cref="INotifyPropertyChanged"/>実装を表します。
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティの値が変更されたとき、このイベントが発火します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChangedイベントを発火させます。
        /// </summary>
        /// <param name="args">PropertyChangedEventArgsです。</param>
        protected void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            BindableBaseExtImpl.RaisePropertyChanged(PropertyChanged, this, args);
        }

        /// <summary>
        /// PropertyChangedイベントを発火させます。
        /// </summary>
        /// <param name="propertyName">リスナーに通知するプロパティ名。この値はオプションであり、
        /// <see cref="CallerMemberName"/>をサポートするコンパイラから自動的に提供されえます。</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティの値が既にセットされているものでないかを確認し、
        /// 必要な場合にのみプロパティをセットしリスナーに通知します。
        /// </summary>
        /// <typeparam name="T">プロパティのタイプ。</typeparam>
        /// <param name="storage">getterとsetterの両方を伴うプロパティに対する参照。</param>
        /// <param name="value">プロパティにセットする値。</param>
        /// <param name="args">PropertyChangedEventArgsです。</param>
        /// <returns>値が変更された場合は真であり、既に値がセットされている場合は偽です。</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, PropertyChangedEventArgs args)
        {
            return BindableBaseExtImpl.SetProperty(ref storage, value, RaisePropertyChanged, args);
        }

        /// <summary>
        /// プロパティの値が既にセットされているものでないかを確認し、
        /// 必要な場合にのみプロパティをセットしリスナーに通知します。
        /// </summary>
        /// <typeparam name="T">プロパティのタイプ。</typeparam>
        /// <param name="storage">getterとsetterの両方を伴うプロパティに対する参照。</param>
        /// <param name="value">プロパティにセットする値。</param>
        /// <param name="propertyName">リスナーに通知するプロパティ名。この値はオプションであり、
        /// <see cref="CallerMemberName"/>をサポートするコンパイラから自動的に提供されえます。</param>
        /// <returns>値が変更された場合は真であり、既に値がセットされている場合は偽です。</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref storage, value, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティの値が既にセットされているものでないかを確認し、
        /// 必要な場合にのみプロパティをセットしリスナーに通知します。
        /// </summary>
        /// <typeparam name="T">プロパティのタイプ。</typeparam>
        /// <param name="storage">getterとsetterの両方を伴うプロパティに対する参照。</param>
        /// <param name="value">プロパティにセットする値。</param>
        /// <param name="onChanged">アクションは値が変更された後に呼び出されます。</param>
        /// <param name="args">PropertyChangedEventArgsです。</param>
        /// <returns>値が変更された場合は真であり、既に値がセットされている場合は偽です。</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, PropertyChangedEventArgs args)
        {
            return BindableBaseExtImpl.SetProperty(ref storage, value, onChanged, RaisePropertyChanged, args);
        }

        /// <summary>
        /// プロパティの値が既にセットされているものでないかを確認し、
        /// 必要な場合にのみプロパティをセットしリスナーに通知します。
        /// </summary>
        /// <typeparam name="T">プロパティのタイプ。</typeparam>
        /// <param name="storage">getterとsetterの両方を伴うプロパティに対する参照。</param>
        /// <param name="value">プロパティにセットする値。</param>
        /// <param name="onChanged">アクションは値が変更された後に呼び出されます。</param>
        /// <param name="propertyName">リスナーに通知するプロパティ名。この値はオプションであり、
        /// <see cref="CallerMemberName"/>をサポートするコンパイラから自動的に提供されえます。</param>
        /// <returns>値が変更された場合は真であり、既に値がセットされている場合は偽です。</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref storage, value, onChanged, new PropertyChangedEventArgs(propertyName));
        }
    }
}
