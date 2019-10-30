using System;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.Binding.View
{
    [RequireComponent(typeof(Image))]
    public class AlphaAsVisibilityHandler : VisibilityHandler
    {
        [SerializeField] [Range(0, 1)] private float _visableAlpha;
        [SerializeField] [Range(0, 1)] private float _unvisableAlpha;
        private Image _this;

        private void Awake()
        {
            _this = GetComponent<Image>();
        }

        public override bool Visible
        {
            set
            {
                var thisColor = _this.color;
                thisColor.a = value ? _visableAlpha : _unvisableAlpha;
                _this.color = thisColor;
            }
        }
    }
}