namespace MVVM.Binding.Reactive
{
   interface IBoxedSubscribe
   {
       System.IDisposable NonGenericSubscribe(System.Action<object> onNext);
   }
}
