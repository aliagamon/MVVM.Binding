using System;
using System.Collections.Generic;
using UniRx;

namespace MVVM.Binding.Reactive
{
    public class RxCollection<T> : UniRx.ReactiveCollection<T>, IBoxedCollectionSubscribe
    {
        public RxCollection()
        {
            
        }
        
        public RxCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public RxCollection(List<T> list) : base(list)
        {
        }

        public IDisposable NonGenericSubscribeAdd(Action<object> onNext)
        {
            return ObserveAdd().Subscribe(e => onNext.Invoke(new CollectionAddEvent(e.Index, e.Value)));
        }

        public IDisposable NonGenericSubscribeRemove(Action<object> onNext)
        {
            return ObserveRemove().Subscribe(e => onNext.Invoke(new CollectionRemoveEvent(e.Index, e.Value)));
        }

        public IDisposable NonGenericSubscribeReplace(Action<object> onNext)
        {
            return ObserveReplace().Subscribe(e =>
                onNext.Invoke(new CollectionReplaceEvent(e.Index, e.OldValue, e.NewValue)));
        }

        public IDisposable NonGenericSubscribeMove(Action<object> onNext)
        {
            return ObserveMove()
                .Subscribe(e => onNext.Invoke(new CollectionMoveEvent(e.OldIndex, e.NewIndex, e.Value)));
        }

        public IDisposable NonGenericSubscribeReset(Action<object> onNext)
        {
            return ObserveReset()
                .Subscribe(o => onNext.Invoke(o));
        }
    }

    public static class RxCollectionExtension
    {
        public static RxCollection<T> ToRxCollection<T>(this IEnumerable<T> source)
        {
            return new RxCollection<T>(source);
        }
    }
}
