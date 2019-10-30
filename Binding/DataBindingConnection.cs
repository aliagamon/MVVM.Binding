using System;
using System.ComponentModel;
using System.Linq.Expressions;
using MVVM.Binding.Converters;
using UnityEngine;

namespace MVVM.Binding.Binding
{
    [Serializable]
    public class DataBindingConnection : DataBindingConnectionBase
    {
        public event Action PropertyChanged;

        public bool isDisposed = false;

        private readonly IValueConverter[] _converters;
        private IDisposable _subscription = null;

        public DataBindingConnection(GameObject owner, BindTarget src, BindTarget dst,
            IValueConverter[] converters = null) : base(owner, src, dst)
        {
            _converters = converters;

            PropertyChanged = OnSrcUpdated;

            BindingMonitor.RegisterConnection(this);
        }

        public void DstUpdated()
        {
            if (_converters != null && _converters.Length > 0)
                SrcTarget.SetValue(_converters.ChainConvertBack(DstTarget.GetValue(), SrcTarget.property.PropertyType, null));
            else if (SrcTarget.property.PropertyType == DstTarget.property.PropertyType)
                SrcTarget.SetValue(DstTarget.GetValue());
            else
                SrcTarget.SetValue(Convert.ChangeType(DstTarget.GetValue(), SrcTarget.property.PropertyType));
        }

        internal void ClearHandler()
        {
            PropertyChanged = null;
            IsBound = false;
        }

        internal void Unbind()
        {
            if (!IsBound) return;
            if (SrcTarget.IsReactive)
            {
                if (!(_subscription is null))
                {
                    _subscription.Dispose();
                    _subscription = null;
                }
            }
//            else
//                (SrcTarget.propertyOwner as INotifyPropertyChanged).PropertyChanged -= PropertyChangedHandler;

            IsBound = false;
        }

        public static string GetName<T>(Expression<Func<T>> e)
        {
            var member = (MemberExpression) e.Body;
            return member.Member.Name;
        }

        internal void Bind()
        {
            if (IsBound) return;
            if (SrcTarget.IsReactive)
            {
//                var methodInfo = SrcTarget.propertyOwner.GetType().GetMethod("NonGenericSubscribe",BindingFlags.NonPublic|BindingFlags.Instance);
//                _subscription = (IDisposable) methodInfo.Invoke(SrcTarget.propertyOwner,
//                    new[]
//                    {
//                        new Action<object>(o => PropertyChangedHandler(SrcTarget.propertyOwner,
//                            new PropertyChangedEventArgs(SrcTarget.propertyName)))
//                    });
                _subscription = SrcTarget.ReactiveBind(PropertyChangedHandler);
            }

//            else
//                (SrcTarget.propertyOwner as INotifyPropertyChanged).PropertyChanged += PropertyChangedHandler;
            IsBound = true;
        }

        public void OnSrcUpdated()
        {
            try
            {
                if (_converters != null && _converters.Length > 0)
                    DstTarget.SetValue(_converters.ChainConvert(SrcTarget.GetValue(), DstTarget.property.PropertyType, null));
                else if (SrcTarget.property.PropertyType == DstTarget.property.PropertyType)
                    DstTarget.SetValue(SrcTarget.GetValue());
                else if (SrcTarget.GetValue() is IConvertible)
                    DstTarget.SetValue(Convert.ChangeType(SrcTarget.GetValue(), DstTarget.property.PropertyType));
                else
                    DstTarget.SetValue(SrcTarget.GetValue());
            }
            catch (Exception e)
            {
                Debug.LogError($"Data binding error in: {OwnerObject.name}: {e}", OwnerObject);

                if (e.InnerException != null)
                    Debug.LogErrorFormat("Inner Exception: {0}", e.InnerException);
            }
        }

        public void SetHandler(Action handler)
        {
            PropertyChanged = handler;
        }

        public static object GetOwner<T>(Expression<Func<T>> e)
        {
            var member = (MemberExpression) e.Body;
            return member.Expression.Type;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(SrcTarget.propertyName))
                PropertyChanged?.Invoke();
        }

        public override void Dispose()
        {
            BindingMonitor.UnRegisterConnection(this);

            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing && SrcTarget.propertyOwner != null)
            {
                var notifyPropertyChanged = SrcTarget.propertyOwner as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged -= PropertyChangedHandler;
                }
            }

            isDisposed = true;
        }
    }
}
