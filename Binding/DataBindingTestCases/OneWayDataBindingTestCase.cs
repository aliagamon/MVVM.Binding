using System;
using System.Linq;
using UnityEngine;
using MVVM.Extension.Services;
using MVVM.Extension.Views.Common;
using Utils.Reflection;
namespace MVVM.Binding.Binding.DataBindingTestCases
{
    public class OneWayDataBindingTestCase : DataBindingTestCaseBase
    {
        private readonly OneWayDataBinding _binder;

        public OneWayDataBindingTestCase(DataBindingBase binder) : base(binder)
        {
            _binder = binder as OneWayDataBinding;
        }

        public override bool ExecuteTest()
        {

            if(!base.ExecuteTest()) return false;

            if(_binder is null)
            {
                LogNotMineError();
                return false;
            }

            bool ValidateSrcProperty(Type ownerType, BindablePropertyInfo property)
            {
                var propInfo = ownerType.GetProperty(property.PropertyName);
                if (propInfo == null)
                    return false;

                var genericType = (property.IsStatic
                    ? propInfo.PropertyType
                    : propInfo.PropertyType.GetGenericTypeRecursive()?.GetGenericArguments()[0])?.Name;
                return !propInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any() &&
                       property.PropertyType == genericType;
            }

            bool ValidateDstProperty(Type ownerType, BindablePropertyInfo property)
            {
                var propInfo = ownerType.GetProperty(property.PropertyName);
                return !(propInfo is null) && propInfo.PropertyType.Name == property.PropertyType;
            }

            var validateSrcProperty = ValidateSrcProperty(ViewModelProvider.GetViewModelType(_binder.ViewModelName), _binder.SrcPropertyName);
            if(!validateSrcProperty) Errors.Push("Source property is not valid");
            var validateDstView = !(_binder._dstView is null);
            if(!validateDstView) Errors.Push("Destination view is not valid");
            var validateDstProperty = validateDstView && ValidateDstProperty(_binder._dstView.GetType(), _binder.DstPropertyName);
            if(!validateDstProperty) Errors.Push("Destination property is not valid");
            return validateSrcProperty &&
                    validateDstView &&
                    validateDstProperty;
        }
    }
}
