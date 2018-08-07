using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Biskuits.Helpers.Observables
{
    /// <summary>
    /// <seealso cref="ObservableObject">のメソッドの外部実装群を表します.
    /// <seealso cref="ObservableObject">を継承できない場合などで利用します.
    /// </summary>
    public static class ObservableObjectExtImpl
    {
        public static void RaisePropertyChanged(
            PropertyChangedEventHandler handler,
            object sender,
            PropertyChangedEventArgs args)
        {
            handler?.Invoke(sender, args);
        }

        public static bool SetProperty<T>(
            ref T storage,
            T value,
            Action<PropertyChangedEventArgs> raisePropertyChanged,
            PropertyChangedEventArgs args)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            raisePropertyChanged(args);

            return true;
        }

        public static bool SetProperty<T>(
            ref T storage,
            T value,
            Action onChanged,
            Action<PropertyChangedEventArgs> raisePropertyChanged,
            PropertyChangedEventArgs args)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            onChanged?.Invoke();
            raisePropertyChanged(args);

            return true;
        }
    }
}
