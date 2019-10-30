using System;
using System.Reflection;
using System.Linq;
using UnityEngine.Events;

namespace MVVM.Binding.Binding.DataBindingTestCases
{
    public class TwoWayDataBindingTestCase : OneWayDataBindingTestCase
    {
        private readonly TwoWayDataBinding _binder;

        public TwoWayDataBindingTestCase(DataBindingBase binder) : base(binder)
        {
            _binder = binder as TwoWayDataBinding;
        }

        public override bool ExecuteTest()
        {
            if (!base.ExecuteTest()) return false;

            if(_binder is null)
            {
                LogNotMineError();
                return false;
            }

            bool ValidateDstChangeEvent(Type ownerType, string dstEventName)
            {
                var propInfo = ownerType.GetProperty(dstEventName, BindingFlags.Instance | BindingFlags.Public);
                return !(propInfo is null) && propInfo.PropertyType.IsSubclassOf(typeof(UnityEventBase)) &&
                       !propInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any();
            }
            var validateDstChangeEvent = ValidateDstChangeEvent(_binder._dstView.GetType(), _binder._dstChangedEventName);
            if(!validateDstChangeEvent) Errors.Push("Destination change event is not valid");
            return validateDstChangeEvent;
        }
    }
}
