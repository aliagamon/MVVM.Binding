using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVM.Binding.Converters;
using MVVM.Extension.Services;
using MVVM.Extension.Views.Common;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Utils.Reflection;
using Utils.Unity.Runtime;

namespace MVVM.Binding.Binding
{
    [AddComponentMenu("MVVM/Binding/One Way Binder")]
    public class OneWayDataBinding
        : DataBindingBase
    {
        
        
        public DataBindingConnection Connection { get { return _connection; } }

        protected DataBindingConnection _connection;

        [HideInInspector]
        public List<BindablePropertyInfo> SrcProps = new List<BindablePropertyInfo>();

        [HideInInspector]
        public List<BindablePropertyInfo> DstProps = new List<BindablePropertyInfo>();


        [HideInInspector]
        public BindablePropertyInfo SrcPropertyName = null;

        [HideInInspector]
        public BindablePropertyInfo DstPropertyName = null;

        [SerializeField]
        public UnityEngine.Component _dstView;

        [SerializeField,HideInInspector]
        private UnityEvent _propertySet;

        [SerializeField,HideInInspector]
        private UnityEvent _propertyChanged;

        [SerializeField]
        [HideInInspector]
        protected ValueConverterBase[] Converters;

        [HideInInspector]
        protected string PropertyPath = null;

        private IValueConverter[] IConverters => Array.ConvertAll(Converters, x => (IValueConverter)x);

        bool _isStartup = true;

        protected void Awake()
        {
            Initialized.Where(b => b == true).SubscribeWithState(this, (_, self) =>
            {
                self._connection.OnSrcUpdated();
                self._propertySet.Invoke();
            });
        }

        public override void RegisterDataBinding()
        {
            base.RegisterDataBinding();

            if (_viewModel == null)
            {
                Debug.LogErrorFormat("Binding Error | Could not Find ViewModel {0} for Property {1}", ViewModelName, SrcPropertyName.PropertyName);

                return;
            }
            if (_connection == null)
            {
                _connection = new DataBindingConnection(
                    gameObject, SrcPropertyName.ToBindTarget(_viewModel, true,
                        PropertyPath), DstPropertyName.ToBindTarget(_dstView), IConverters);
                _connection.PropertyChanged += OnPropertyChanged;
            }

            _connection.Bind();
        }

        private void OnPropertyChanged()
        {
            _propertyChanged.Invoke();
        }

        public override void UnregisterDataBinding()
        {
            base.UnregisterDataBinding();

            _connection?.Unbind();
            _connection = null;
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            if (_dstView != null)
            {
                var props = _dstView.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                DstProps = props.Where(prop => prop.GetSetMethod(false) != null
                                               && prop.GetSetMethod(false) != null
                                               && !prop.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any()
                ).Select(e => new BindablePropertyInfo(e.Name, e.PropertyType.Name, true)).ToList();

            }

            if (ViewModelProvider.IsViewModelTypeNameValid(ViewModelName))
            {
                var props = ViewModelProvider.GetViewModelType(ViewModelName).GetProperties();
                //                SrcProps = props.Where(prop =>
                //                        prop.PropertyType.IsAssignableToGenericType(typeof(Reactive.ReactiveProperty<>))
                //                        && !prop.GetCustomAttributes(typeof(ObsoleteAttribute), true)
                //                            .Any())
                SrcProps = SrcPropsSearch(props).Select(e => new BindablePropertyInfo(e.Name,
                    e.PropertyType.IsGenericType
                        ? e.PropertyType.GetGenericArguments()[0].Name
                        : e.PropertyType.BaseType.GetGenericArguments()[0].Name)).ToList();
                SrcProps.AddRange(GetExtraViewModelProperties(props));
            }
        }

        public virtual IEnumerable<PropertyInfo> SrcPropsSearch(IEnumerable<PropertyInfo> props) =>
            props.Where(prop =>
                prop.PropertyType.IsAssignableToGenericType(typeof(Reactive.RxProperty<>))
                || prop.PropertyType.IsAssignableToGenericType(typeof(Reactive.RxReadOnlyProperty<>))
                && !prop.GetCustomAttributes(typeof(ObsoleteAttribute), true)
                    .Any());

        protected virtual IEnumerable<BindablePropertyInfo> GetExtraViewModelProperties(PropertyInfo[] props)
        {
            return props.Where(p => new []
                                    {
                                        typeof(Reactive.RxCommand<>), typeof(Reactive.RxAsyncCommand<>)
                                    }.Any(t=>p.PropertyType.IsAssignableToGenericType(t))
                                    && !p.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any()
            ).Select(e => new BindablePropertyInfo(e.Name, "Can Execute")).Concat(props.Where(p =>
                new []
                {
                    typeof(Reactive.RxCommand<>), typeof(Reactive.RxAsyncCommand<>),
                    typeof(Reactive.RxProperty<>), typeof(Reactive.RxReadOnlyProperty<>),
                    typeof(Reactive.RxCollection<>)
                    
                }.All(t => !p.PropertyType.IsAssignableToGenericType(t))
                && !p.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any()).Select(e =>
                new BindablePropertyInfo(e.Name, e.PropertyType.Name, true)));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_connection != null)
                _connection.Dispose();
        }


        protected override void FormerlyBindedAsHandler()
        {
            base.FormerlyBindedAsHandler();
#if UNITY_EDITOR
            var indx = ViewModelProvider.IsViewModelTypeNameValid(ViewModelName)
                ? GetIndexOfBindablePropertyInfo(SrcPropertyName,
                    ViewModelProvider.GetViewModelType(ViewModelName), SrcProps)
                : -1;
            SrcPropertyName = indx > -1 ? SrcProps[indx] : SrcPropertyName;
            indx = !(_dstView is null)
                ? GetIndexOfBindablePropertyInfo(DstPropertyName, _dstView.GetType(), DstProps)
                : -1;
            DstPropertyName = indx > -1 ? DstProps[indx] : DstPropertyName;
#endif
        }


    }
}
