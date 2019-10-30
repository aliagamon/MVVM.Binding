using System;
using System.Linq;
using MVVM.Extension.Services;
using MVVM.Binding.Reactive;
using Utils.Reflection;

namespace MVVM.Binding.Binding.DataBindingTestCases
{
    public class CollectionViewSourceTestCase : DataBindingTestCaseBase
    {
        private readonly CollectionViewSource _binder;

        public CollectionViewSourceTestCase(DataBindingBase binder) : base(binder)
        {
            _binder = binder as CollectionViewSource;
        }

        public override bool ExecuteTest()
        {
            if (_binder is null)
            {
                LogNotMineError();
                return false;
            }

            bool ValidateSrcCollection(Type ownerType, BindablePropertyInfo property)
            {
                var propInfo = ownerType.GetProperty(property.PropertyName);
                return !(propInfo is null) &&
                        propInfo.PropertyType.IsAssignableToGenericType(typeof(RxCollection<>)) &&
                        !propInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any();
            }
            var validateSrcCollection = ValidateSrcCollection(ViewModelProvider.GetViewModelType(_binder.ViewModelName), _binder.SrcCollectionName);
            if(!validateSrcCollection) Errors.Push("Source collection is not valid");
            return validateSrcCollection;
        }
    }
}
