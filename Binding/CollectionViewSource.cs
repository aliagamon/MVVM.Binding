using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using MVVM.Binding.Model;
using MVVM.Binding.Reactive;
using MVVM.Core.ViewModels;
using MVVM.Extension.Services;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Utils.Reflection;

namespace MVVM.Binding.Binding
{
    [AddComponentMenu("MVVM/Binding/Collections/Source Binder")]
    public class CollectionViewSource : DataBindingBase
    {
        INotifyCollectionChanged srcCollection;

        [HideInInspector]
        public List<BindablePropertyInfo> SrcCollections = new List<BindablePropertyInfo>();

        [HideInInspector]
        public BindablePropertyInfo SrcCollectionName;
        [SerializeField] private UnityEvent _collectionBinded;

        public bool IsBound => isBound;


        public Action<int, object> OnElementAdded;
        public Action<int, object> OnElementRemoved;
        public Action<int, object, object> OnElementReplaced;
        public Action<int, int> OnElementMove;
        public Action OnCollectionReset;

        public Action<IViewModel> OnSelectedItemUpdated;

        public IViewModel SelectedItem
        {
            get { return _selectedItem; }

            set
            {
                if (value != _selectedItem)
                {
                    _selectedItem = value;

                    OnSelectedItemUpdated?.Invoke(value);
                }
            }
        }

        IViewModel _selectedItem;

        BindTarget src;

        private IDisposable _subscription = null;

        bool isBound = false;

        public override bool KeepConnectionAliveOnDisable => true;

        public int Count
        {
            get
            {
                return (src.GetValue() as IList).Count;
            }
        }

        public string SelectedItemPropName;

        public object this[int key]
        {
            get
            {
                var list = (src.GetValue() as IList);
                return list[key];
            }
            set
            {
                (src.GetValue() as IList)[key] = value;
            }
        }

        public override void RegisterDataBinding()
        {
            base.RegisterDataBinding();
            if (isBound) return;
            if (_viewModel == null)
            {
                Debug.LogErrorFormat("Binding Error | Could not Find ViewModel {0} for collection {1}", ViewModelName,
                    SrcCollectionName);

                return;
            }

            src = SrcCollectionName.ToBindTarget(_viewModel, true);
            if (!(src is null))
            {
                _subscription = src.ReactiveCollectionBind(CollectionAdd, CollectionRemove, CollectionReplace,
                    CollectionMove, CollectionReset);

                var items = (IList) src.propertyOwner; 
                for (int i = 0; i < items.Count; i++)
                {
                    CollectionAdd(new CollectionAddEvent(i, items[i]));
                }
                isBound = true;
                _collectionBinded.Invoke();
            }
        }

        public override void UnregisterDataBinding()
        {
            base.UnregisterDataBinding();
            if (!isBound) return;
            if (!(_subscription is null))
            {
                _subscription.Dispose();
                _subscription = null;
            }
            if (srcCollection != null)
                srcCollection.CollectionChanged -= CollectionChanged;
            OnCollectionReset?.Invoke();
            isBound = false;
        }

        protected virtual void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
//            switch (e.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    OnElementsAdded?.Invoke(e.NewStartingIndex, e.NewItems);
//                    break;
//                case NotifyCollectionChangedAction.Move:
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    OnElementsRemoved?.Invoke(e.OldStartingIndex, e.OldItems);
//                    break;
//                case NotifyCollectionChangedAction.Replace:
//                    OnElementUpdated.Invoke(e.NewStartingIndex, e.NewItems);
//                    break;
//                case NotifyCollectionChangedAction.Reset:
//                    OnCollectionReset?.Invoke(e.NewStartingIndex, e.NewItems);
//                    break;
//                default:
//                    break;
//            }

            OnSelectedItemUpdated?.Invoke(SelectedItem);
        }

        protected virtual void CollectionAdd(Reactive.CollectionAddEvent e) => OnElementAdded?.Invoke(e.Index, e.Value);

        protected virtual void CollectionRemove(Reactive.CollectionRemoveEvent e) => OnElementRemoved?.Invoke(e.Index, e.Value);

        protected virtual void CollectionReplace(Reactive.CollectionReplaceEvent e) => OnElementReplaced?.Invoke(e.Index, e.OldValue, e.NewValue);
        protected virtual void CollectionMove(Reactive.CollectionMoveEvent e) => OnElementMove?.Invoke(e.OldIndex, e.NewIndex);

        protected virtual void CollectionReset(Unit e) => OnCollectionReset?.Invoke();

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            if (ViewModelProvider.IsViewModelTypeNameValid(ViewModelName))
            {
                var props = ViewModelProvider.GetViewModelType(ViewModelName).GetProperties();

                SrcCollections = props.Where(prop =>
                        prop.PropertyType.IsAssignableToGenericType(typeof(RxCollection<>))
                        && !prop.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any()
                    ).Select(e => new BindablePropertyInfo(e.Name, e.PropertyType.GenericTypeArguments[0].Name)).ToList();


            }
        }
    }
}
