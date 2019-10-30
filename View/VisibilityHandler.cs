using MVVM.Core.ViewModels;
using MVVM.Extension.Views.Common;
using UniRx.Async;
using UnityEngine;

namespace MVVM.Binding.View
{
    public class VisibilityHandler : MonoBehaviour
    {
        [SerializeField]
        private GameObject _notSelfTarget;

        public virtual bool Visible
        {
            set => (_notSelfTarget ? _notSelfTarget : gameObject).SetActive(value);
        }
    }
}