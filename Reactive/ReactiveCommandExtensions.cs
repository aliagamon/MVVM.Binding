using System;

namespace MVVM.Binding.Reactive
{
    public static class MVVMReactiveCommandExtensions
    {
        public static RxCommand ToMVVMReactiveCommand(this IObservable<bool> canExecuteSource, bool initialValue = true)
        {
            return new RxCommand(canExecuteSource, initialValue);
        }
    }
}