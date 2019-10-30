using System;
using System.Collections;
using System.Collections.Generic;
using MVVM.Binding.Utils;
using MVVM.Core.ViewModels;
using MVVM.Extension.Services;
using MVVM.Extension.Views.Common;
using UniRx;
using UnityEditor;
using UnityEngine;
using Utils.Reflection;

namespace MVVM.Binding.Binding
{
    public interface IDataBindingBaseProvider
    {
        IEnumerable<IDataBindingBase> Bindings { get;  } 
    }
    public interface IDataBindingBase
    {
        ViewBase View { get; }
        void Unbind();
        void Rebind();
    }

    public abstract class DataBindingBase :
        MonoBehaviour,
        IDataBinding,
        IConnectionTestEventReciver, 
        IDataBindingBaseProvider,
        IDataBindingBase
    {
        public static Func<Type, bool> PreSelectedViewFindTest = t =>
            t.GetGenericTypeDefinition() == typeof(ViewBase<>) ||
            t.GetGenericTypeDefinition() == typeof(ScreenViewBase<>);

        public readonly BindingManager Manager;
        protected bool _keepConnectionAliveOnDisable = false;

        [SerializeField] private ViewBase _view;

        [HideInInspector] public ViewModelBase _viewModel;


        [HideInInspector] public string ViewModelName;

        public ViewBase View => PreSelectedView;
        public ViewBase PreSelectedView
        {
            get => _view;
            set
            {
                _view = value;
                if (_view != null)
                    ViewModelName = value.GetType().GetGenericTypeRecursive(PreSelectedViewFindTest)
                        .GenericTypeArguments[0]
                        .ToString();
            }
        }

        public ViewModelBase ViewModelSrc => _viewModel;

        protected ReactiveProperty<bool> Initialized { get; } = new ReactiveProperty<bool>();

        public virtual bool KeepConnectionAliveOnDisable => _keepConnectionAliveOnDisable;


        protected DataBindingBase()
        {
            Manager = new BindingManager(this);            
        }
        
        public virtual void PreConnectionTest()
        {
        }

        public virtual void PostConnectionTest()
        {
        }

        public virtual void RegisterDataBinding()
        {
#if UNITY_EDITOR
            BindingMonitor.RegisterBinding(this);
#endif
        }


        public virtual void UnregisterDataBinding()
        {
#if UNITY_EDITOR
            BindingMonitor.UnRegisterBinding(this);
#endif
        }

        protected virtual void OnValidate()
        {
            UpdateBindings();
            // FormerlyBindedAsHandler();
        }

        protected virtual void FormerlyBindedAsHandler()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, DateTime.Now.ToString());
            var vms = ViewModelProvider.SGetViewModels();
            var index = GetIndexOfViewModel(ViewModelName, vms);
            ViewModelName = vms[index];
#endif
        }

        private void Reset()
        {
            FindViewModel();
            UpdateBindings();
        }

        private void FindViewModel()
        {
            if (!(PreSelectedView is null)) return;

            if (ViewFinder.FindRelativeView(gameObject, ViewFinder.FindRelativeViewUpward, out var v))
                PreSelectedView = v;
            else
                Debug.unityLogger.LogWarning(
                    $"No valid view found for me ({$"{transform.parent?.name ?? ""}/{name}"}), will resolve using ViewModelProvider",
                    this);

            // if (_view == null)
            //     PreSelectedView = gameObject.FindRelativeViewUpward();

            // if (_view == null)
            //     Debug.LogWarning(
            //         $"No valid view found for me ({$"{transform.parent?.name ?? ""}/{name}"}), will resolve using ViewModelProvider",
            // this);
        }

        public virtual void UpdateBindings()
        {
        }

        private IEnumerator ResolveViewModel()
        {
            if (_viewModel != null)
                yield return null;

            if (_view == null)
                FindViewModel();

            if (_view != null)
            {
                if (_view.Initialized.Value == false)
                    while (_view.Initialized.Value == false)
                        yield return null;
                var vm = _view.ViewModel;
                if (vm == null || vm.GetType().FullName.Equals(ViewModelName, StringComparison.OrdinalIgnoreCase) ==
                    false)
                {
                    Debug.LogError(
                        $"{name}: ViewModel type of referenced View is '{vm?.GetType().Name}' expected: '{ViewModelName}'",
                        this);
                }
                else
                {
                    _viewModel = vm;
                    yield break;
                }
            }

            if (ViewModelProvider.IsViewModelTypeNameValid(ViewModelName))
                _viewModel = ViewModelProvider.Instance.GetViewModelBehaviour(ViewModelName);

            if (_viewModel == null)
                Debug.LogErrorFormat("ViewModel Null: {0}", name);
        }

        private IEnumerator Start()
        {
            yield return MainThreadDispatcher.StartCoroutine(Initialize());
        }
        
        protected virtual IEnumerator Initialize()
        {
            yield return MainThreadDispatcher.StartCoroutine(ResolveViewModel());
            RegisterDataBinding();
            Initialized.Value = true;
        }


        protected virtual void OnDestroy()
        {
            UnregisterDataBinding();
        }

        public static int GetIndexOfBindablePropertyInfo(BindablePropertyInfo name, Type ownerType,
            List<BindablePropertyInfo> propertyInfos)
        {
            Func<BindablePropertyInfo, int> finder = bp =>
                propertyInfos.FindIndex(p =>
                    !(bp is null) && p.PropertyName == bp.PropertyName && p.PropertyType == bp.PropertyType);
            var rValue = finder.Invoke(name);
            if (rValue > -1) return rValue;

            var properties = ownerType.GetProperties();
            BindablePropertyInfo newName = null;
            foreach (var property in properties)
            {
                var customAttribute =
                    (FormerlyBindedAs) Attribute.GetCustomAttribute(property, typeof(FormerlyBindedAs));
                if (customAttribute is null || customAttribute.OldName != name.PropertyName) continue;
                newName = new BindablePropertyInfo(property.Name, name.IsStatic ? property.PropertyType.Name :
                    property.PropertyType.TryGetGenericTypeRecursive(out var gt) ? gt.GetGenericArguments()[0].Name :
                    "");
                break;
            }

            return finder.Invoke(newName);
        }

        public static int GetIndexOfViewModel(string name, List<string> viewModels)
        {
            var rValue = viewModels.IndexOf(name);

            if (rValue > -1) return rValue;

            bool IsEqual(string lhs, string rhs)
            {
                if (lhs.Contains("."))
                    return lhs == rhs;
                return rhs.EndsWith(lhs);
            }

            var newName = "";
            foreach (var vm in viewModels)
            {
                var vmType = ViewModelProvider.GetViewModelType(vm);
                var customAttribute = (FormerlyBindedAs) Attribute.GetCustomAttribute(vmType, typeof(FormerlyBindedAs));
                if (customAttribute is null || !IsEqual(customAttribute.OldName, name)) continue;
                newName = vmType.ToString();
            }

            return viewModels.IndexOf(newName);
        }
        
        public void Unbind()
        {
            UnregisterDataBinding();
            Initialized.Value = false;
            _viewModel = null;
        }

        public void Rebind()
        {
            MainThreadDispatcher.StartCoroutine(Initialize());
        }

        public class BindingManager
        {
            private readonly DataBindingBase _binding;

            public BindingManager(DataBindingBase binding)
            {
                _binding = binding;
            }

            public void Unbind()
            {
                _binding.UnregisterDataBinding();
                _binding.Initialized.Value = false;
                _binding._viewModel = null;
            }

            public void Rebind()
            {
                MainThreadDispatcher.StartCoroutine(_binding.Initialize());
            }
        }

        public IEnumerable<IDataBindingBase> Bindings => new[] {this};
    }
}