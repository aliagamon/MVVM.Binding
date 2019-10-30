using System;
using System.Linq;
using MVVM.Binding.Binding;
using MVVM.Binding.Reactive;
using MVVM.Core.ViewModels;
using MVVM.Extension.Views.Common;
using UniRx;
using UniRx.Async;
using UnityEngine;
using Utils.Linq;

namespace MVVM.Binding.View
{
    public class ViewModelPropertyView<TViewModel> : ViewBase<TViewModel>
        where TViewModel : ViewModelBase
    {
        private IDataBindingBase[] _childBindings;

        public new TViewModel ViewModel
        {
            get => base.ViewModel;
            set => Initialize(value);
        }

        protected override async UniTask OnInitializeAsync()
        {
            _childBindings = _childBindings ?? GetComponentsInChildren<IDataBindingBaseProvider>(true).SelectMany(p=>p.Bindings).Where(b=>b.View == this).ToArray();
            await base.OnInitializeAsync();
            _childBindings.ForEach(b=>
            {
                b.Unbind();
                b.Rebind();
            });
        
        }
    }
}