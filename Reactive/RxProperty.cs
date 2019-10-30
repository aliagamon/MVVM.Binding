using System;
using UniRx;

namespace MVVM.Binding.Reactive
{
    public class RxProperty<T> : UniRx.ReactiveProperty<T>, IBoxedSubscribe
    {
        public RxProperty()
        {
        }

        public RxProperty(T i) : base(i)
        {
        }

        public IDisposable NonGenericSubscribe(Action<object> onNext)
        {
            return this.SkipLatestValueOnSubscribe().Subscribe(obj => onNext.Invoke(obj));
        }
    }

    public static class RxPropertyExtensions
    {
        public static IReadOnlyReactiveProperty<T> ToRxProperty<T>(this IObservable<T> source)
        {
            return new RxReadOnlyProperty<T>(source);
        }

        public static IReadOnlyReactiveProperty<T> ToRxProperty<T>(this IObservable<T> source, T initialValue)
        {
            return new RxReadOnlyProperty<T>(source, initialValue);
        }

        public static RxReadOnlyProperty<T> ToRxReadOnlyProperty<T>(this IObservable<T> source)
        {
            return new RxReadOnlyProperty<T>(source);
        } 
    }
    
}
