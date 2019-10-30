using MVVM.Extension.Views.Common;
using UnityEngine;

namespace MVVM.Binding.View
{
    public class CanvasView : ViewBase
    {
        public float Alpha
        {
            get
            {
                return cg.alpha;
            }
            set
            {
                cg.alpha = value;
            }
        }

        public CanvasGroup cg
        {
            get
            {
                if (_cg == null)
                    _cg = GetComponent<CanvasGroup>();
                if (_cg == null)
                    _cg = gameObject.AddComponent<CanvasGroup>();
                return _cg;
            }
        }

        private CanvasGroup _cg;

        public override bool Visibility
        {
            get => base.Visibility;
            set
            {
                base.Visibility = value;
                cg.blocksRaycasts = value;
            }
        }
    }
}