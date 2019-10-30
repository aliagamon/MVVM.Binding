using System;
using UniRx;

namespace MVVM.Binding.Reactive
{
    public class RxCommand<T> : UniRx.ReactiveCommand<T>, IBoxedSubscribe
    {
        public RxCommand()
            : base()
        { }
        public RxCommand(IObservable<bool> canExecuteSource, bool initialValue = true)
            : base(canExecuteSource, initialValue)
        {
        }

        public IDisposable NonGenericSubscribe(Action<object> onNext)
        {
            return CanExecute.SkipLatestValueOnSubscribe().Subscribe(obj => onNext.Invoke(obj));
        }
    }

    public class RxCommand : RxCommand<Unit>
    {
        public RxCommand()
            : base()
        { }

        /// <summary>
        /// CanExecute is changed from canExecute sequence.
        /// </summary>
        public RxCommand(IObservable<bool> canExecuteSource, bool initialValue = true)
            : base(canExecuteSource, initialValue)
        {
        }

        /// <summary>Push null to subscribers.</summary>
        public bool Execute()
        {
            return Execute(Unit.Default);
        }

        /// <summary>Force push parameter to subscribers.</summary>
        public void ForceExecute()
        {
            ForceExecute(Unit.Default);
        }
        internal IDisposable NonGenericSubscribe(Action<object> onNext)
        {
            return CanExecute.Subscribe(obj => onNext.Invoke(obj));
        }
    }


}
