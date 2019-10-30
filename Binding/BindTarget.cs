using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MVVM.Binding.Reactive;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Utils.Reflection;

namespace MVVM.Binding.Binding
{
    [Serializable]
    public class BindTarget
    {
        private object _command;

        public PropertyInfo property;

        public string propertyName;

        public object propertyOwner;

        public string propertyPath;

        private BindTarget()
        {
        }

        public bool IsReactive { get; private set; }

        public bool IsCommand { get; private set; }

        public static BindTarget Create(object propOwner, string propName, string path = null,
            UnityEvent dstChangedEvent = null, bool isReactive = false, bool isCommand = false)
        {
            var result = new BindTarget();
            if (isReactive)
            {
                var propertyInfo = propOwner.GetType().GetProperty(propName);

                result.propertyOwner = propertyInfo?.GetValue(propOwner);
                if (result.propertyOwner == null && propertyInfo != null)
                {
                    Debug.LogWarning($"{propOwner.GetType().Name} :: {propName} property is null, can't bind");
                    return null;
                }

                if (isCommand)
                {
                    result._command = result.propertyOwner;
                    result.propertyOwner = result.propertyOwner.GetType()
                        .GetPropertyRecursive("CanExecute")
                        .GetValue(result.propertyOwner);
                }
            }
            else
            {
                result.propertyOwner = propOwner;
            }

            if (result.propertyOwner == null)
            {
                Debug.LogErrorFormat("Could not find propertyOwner (ViewModel?) for Property {0}", propName);
                return null;
            }

            result.propertyName = propName;
            result.propertyPath = path;
            result.IsReactive = isReactive;
            result.IsCommand = isCommand;


            result.property = result.propertyOwner.GetType().GetProperties()
                .FirstOrDefault(p => p.Name == (isReactive ? "Value" : result.propertyName));

            if (dstChangedEvent != null)
                dstChangedEvent.AddListener(() => { });

            return result;
        }

        public object GetValue()
        {
            if (string.IsNullOrEmpty(propertyPath))
                return property != null ? property.GetValue(propertyOwner, null) : null;

            var parentProp = property.GetValue(propertyOwner, null);
            var parts = propertyPath.Split('.');

            var owner = parentProp;
            PropertyInfo prop = null;

            foreach (var part in parts)
            {
                prop = owner.GetType().GetProperty(propertyPath);
                owner = prop.GetValue(owner, null);
            }

            return owner;
        }

        public void SetValue(object src)
        {
            if (property == null) return;

            if (string.IsNullOrEmpty(propertyPath))
            {
                property.SetValue(propertyOwner, src, null);
            }
            else
            {
                var parentProp = property.GetValue(propertyOwner, null);
                var parts = propertyPath.Split('.');

                var owner = parentProp;
                PropertyInfo prop = null;

                foreach (var part in parts) prop = owner.GetType().GetProperty(propertyPath);

                prop.SetValue(owner, src, null);
            }
        }

        public IDisposable ReactiveBind(PropertyChangedEventHandler handler)
        {
//            MethodInfo methodInfo;
//            if (IsCommand)
//                methodInfo = _command.GetType()
//                    .GetMethod("NonGenericSubscribe", BindingFlags.NonPublic | BindingFlags.Instance);
//            else
//                methodInfo = propertyOwner.GetType()
//                    .GetMethod("NonGenericSubscribe", BindingFlags.NonPublic | BindingFlags.Instance);
//
//            return (IDisposable) methodInfo.Invoke(IsCommand ? _command : propertyOwner,
//                new[]
//                {
//                    new Action<object>(o => handler(propertyOwner,
//                        new PropertyChangedEventArgs(propertyName)))
//                });
            var boxedSubscribe = IsCommand ? _command as IBoxedSubscribe : propertyOwner as IBoxedSubscribe;
            return boxedSubscribe?.NonGenericSubscribe(o =>
                handler(propertyOwner, new PropertyChangedEventArgs(propertyName)));
        }

        public IDisposable ReactiveCollectionBind(Action<CollectionAddEvent> addHandler,
            Action<CollectionRemoveEvent> removeHandler, Action<CollectionReplaceEvent> replaceHandler,
            Action<CollectionMoveEvent> moveHandle, Action<Unit> resetHandler)
        {
            var compositeDisposable = new CompositeDisposable(5);
            if (propertyOwner is IBoxedCollectionSubscribe unboxed)
            {
                compositeDisposable.Add(unboxed.NonGenericSubscribeAdd(o => addHandler.Invoke((CollectionAddEvent) o)));
                compositeDisposable.Add(
                    unboxed.NonGenericSubscribeRemove(o => removeHandler.Invoke((CollectionRemoveEvent) o)));
                compositeDisposable.Add(
                    unboxed.NonGenericSubscribeReplace(o => replaceHandler.Invoke((CollectionReplaceEvent) o)));
                compositeDisposable.Add(
                    unboxed.NonGenericSubscribeMove(o => moveHandle.Invoke((CollectionMoveEvent) o)));
                compositeDisposable.Add(unboxed.NonGenericSubscribeReset(o => resetHandler.Invoke((Unit) o)));
            }

            return compositeDisposable;
        }
    }
}