using System;
using UniRx;

namespace MVVM.Binding.Reactive
{
    public class RxAsyncCommand<T> : UniRx.AsyncReactiveCommand<T>, IBoxedSubscribe
    {
        public RxAsyncCommand()
            : base()
        { }
        public RxAsyncCommand(IObservable<bool> canExecuteSource)
            : base(canExecuteSource)
        {
        }

        public IDisposable NonGenericSubscribe(Action<object> onNext)
        {
            return CanExecute.SkipLatestValueOnSubscribe().Subscribe(obj => onNext.Invoke(obj));
        }
    }
    
    public class RxAsyncCommand : RxAsyncCommand<Unit>
    {
        public RxAsyncCommand()
            : base()
        { }

        /// <summary>
        /// CanExecute is changed from canExecute sequence.
        /// </summary>
        public RxAsyncCommand(IObservable<bool> canExecuteSource)
            : base(canExecuteSource)
        {
        }

        internal IDisposable NonGenericSubscribe(Action<object> onNext)
        {
            return CanExecute.Subscribe(obj => onNext.Invoke(obj));
        }

        public IObservable<Unit> ExecuteAsync()
        {
            return ExecuteAsync(Unit.Default);
        }

        public IDisposable Execute()
        {
            return base.Execute(Unit.Default);
        }
    }
}