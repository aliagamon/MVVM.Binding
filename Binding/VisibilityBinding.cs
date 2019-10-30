using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MVVM.Binding.Binding
{
    [AddComponentMenu("MVVM/Binding/Visibility Binder")]
    public class VisibilityBinding : OneWayDataBinding
    {
        public override bool KeepConnectionAliveOnDisable => true;

        protected override IEnumerable<BindablePropertyInfo> GetExtraViewModelProperties(PropertyInfo[] props)
        {
            return new BindablePropertyInfo[0];
        }
    }
}
