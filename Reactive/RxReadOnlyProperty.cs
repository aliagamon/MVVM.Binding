using System;
using UniRx;

namespace MVVM.Binding.Reactive
{
    public class RxReadOnlyProperty<T> : UniRx.ReadOnlyReactiveProperty<T>, IBoxedSubscribe
    {

        public IDisposable NonGenericSubscribe(Action<object> onNext)
        {
            return this.SkipLatestValueOnSubscribe().Subscribe(obj => onNext.Invoke(obj));
        }

        public RxReadOnlyProperty(IObservable<T> source) : base(source)
        {
        }

        public RxReadOnlyProperty(IObservable<T> source, bool distinctUntilChanged) : base(source, distinctUntilChanged)
        {
        }

        public RxReadOnlyProperty(IObservable<T> source, T initialValue) : base(source, initialValue)
        {
        }

        public RxReadOnlyProperty(IObservable<T> source, T initialValue, bool distinctUntilChanged) : base(source, initialValue, distinctUntilChanged)
        {
        }
    }
}