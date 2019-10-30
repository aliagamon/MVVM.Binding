using MVVM.Extension.Views.Common;

namespace MVVM.Binding.Converters
{
    public abstract class InjectableValueConverterBase : ValueConverterBase
    {
        protected virtual void Awake()
        {
            MonoInitializer.Instance.ResolveDependencies(this);
        }
    }
}