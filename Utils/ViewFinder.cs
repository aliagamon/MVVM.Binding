using UnityEngine;
using Utils.Unity.Runtime;
using MVVM.Extension.Views.Common;

namespace MVVM.Binding.Utils
{
    public static class ViewFinder
    {
        public delegate ViewBase ViewBaseFinder(GameObject go);
        public static ViewBase FindRelativeViewUpward(GameObject go)
        {
            return go.GetComponentInMeOrParent<ViewBase>(true);
        }

        public static ViewBase FindRelativeViewDownward(GameObject go)
        {
            return go.GetComponentInMeOrChildren<ViewBase>(true);
        }

        public static bool FindRelativeView(GameObject go, ViewBaseFinder finder, out ViewBase view)
        {
            view = finder.Invoke(go);
            if (view is null) return false;
            return true;
        }
    }
}
