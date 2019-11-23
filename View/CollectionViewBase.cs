using System;
using System.Collections.Generic;
using System.Linq;
using MVVM.Binding.Binding;
using MVVM.Core.ViewModels;
using MVVM.Extension.Views.Common;
using UnityEngine;
using UnityEngine.Events;
using Utils.Linq;
using Logger = MVVM.Core.Services.Logger;

namespace MVVM.Binding.View
{
    namespace View
    {
        [RequireComponent(typeof(CollectionViewSource))]
        [AddComponentMenu("MVVM/Binding/Collections/Collection View")]
        public class CollectionViewBase : MonoBehaviour
        {
            [SerializeField] private UnityEvent _collectionChanged;

            [SerializeField] protected ViewBase _listItemPrefab;

            [SerializeField] private bool _reversedOrder;

            [SerializeField] protected CollectionViewSource _src;

            public List<ViewBase> InstantiatedItems = new List<ViewBase>();

            [SerializeField] protected bool UseStaticItems;

            // Use this for initialization
            protected void Awake()
            {
                if (_src != null)
                {
                    _src.OnElementAdded += AddElement;
                    _src.OnElementRemoved += RemoveElement;
                    _src.OnElementReplaced += ReplaceElement;
                    _src.OnElementMove += MoveElement;
                    _src.OnCollectionReset += ResetView;
                }

                _src.OnSelectedItemUpdated += UpdateSelectedItem;

                foreach (Transform child in transform)
                {
                    if (UseStaticItems)
                    {
                        var v = child.GetComponent<ViewBase>();
                        if (!(v is null))
                        {
                            if (_reversedOrder)
                                InstantiatedItems.Insert(0, v);
                            else
                                InstantiatedItems.Add(v);
                            continue;
                        }
                    }

                    Destroy(child.gameObject);
                }
            }

            protected virtual void MoveElement(int oldIndex, int newIndex)
            {
                var item = InstantiatedItems[oldIndex];
                InstantiatedItems.RemoveAt(oldIndex);
                InstantiatedItems.Insert(newIndex, item);
                if (item is ICollectionItemOnMoveListener ml)
                    ml.OnMoveInCollection();
                OnCollectionChanged();
            }

            protected virtual void ReplaceElement(int index, object oldValue, object newValue)
            {
                var item = InstantiatedItems[index];
                if (newValue is ViewModelBase vm)
                {
                    Rebind(item, vm);
                }

                if (item is ICollectionItemOnReplaceListener rl)
                    rl.OnReplacedInCollection();
                OnCollectionChanged();
            }

            private static void Rebind(ViewBase view, ViewModelBase viewModel)
            {
                var childBindings = view.GetComponentsInChildren<IDataBindingBaseProvider>(true).SelectMany(p => p.Bindings).Where(b => b.View == view).ToArray();
                childBindings.ForEach(b => b.Unbind());
                if (view is IReusableView cv)
                {
                    cv.DiscardView();
                    cv.UpdateView(viewModel);
                }

                childBindings.ForEach(b => b.Rebind());
            }

            private void UpdateSelectedItem(IViewModel obj)
            {
                foreach (var item in InstantiatedItems)
                    if (item is ISelectableItem selectableItem)
                        selectableItem.SetSelected(item.ViewModel == obj);
            }


            private void UpdateChildVisibilities(bool visible)
            {
                foreach (var item in InstantiatedItems) item.gameObject.SetActive(visible);
            }


            protected void OnDestroy()
            {
                if (_src != null)
                {
                    _src.OnElementAdded -= AddElement;
                    _src.OnElementRemoved -= RemoveElement;
                    _src.OnElementReplaced -= ReplaceElement;
                    _src.OnElementMove -= MoveElement;
                    _src.OnCollectionReset -= ResetView;
                }
            }


            protected virtual void ResetView()
            {
//                foreach (Transform t in transform)
//                    GameObject.Destroy(t.gameObject);
                for (var i = InstantiatedItems.Count - 1; i >= 0; --i)
                    RemoveElement(i, InstantiatedItems[i]);
                if(UseStaticItems == false)
                    InstantiatedItems.Clear();
                else
                    Enumerable.Range(0, InstantiatedItems.Count).ForEach(i => _staticItemsToRebind.Add(i));
                OnCollectionChanged();
            }

            //

            // Override this method to create the gameobject that will spawn in your CollectionView

            //

            protected virtual void AddElement(int index, object vm)
            {
                ViewBase view;
                int idx;

                if (UseStaticItems)
                {

                    if (index >= InstantiatedItems.Count)
                        throw new IndexOutOfRangeException(
                            $"UseStaticItems is checked but model collection count ({index + 1}) is > static items ({InstantiatedItems.Count}). {name}:{_src.SrcCollectionName.PropertyName}");
                    idx = _reversedOrder ? InstantiatedItems.Count - 1 - index : index;
                    view = InstantiatedItems[idx];
                    view.gameObject.SetActive(true);
                }
                else
                {
                    idx = _reversedOrder ? InstantiatedItems.Count - index : index;
                    view = CreateCollectionItem(transform);
                    view.transform.SetSiblingIndex(idx);
                    InstantiatedItems.Insert(index, view);
                }

                if(UseStaticItems && _staticItemsToRebind.Contains(idx))
                    Rebind(view, (ViewModelBase)vm);
                else
                    InitItem(view, vm, index);

                if (view is ICollectionItemOnAddListener al)
                    al.OnAddedToCollection();

                OnCollectionChanged();
            }

            private void OnCollectionChanged()
            {
                _collectionChanged.Invoke();
            }

            protected virtual ViewBase CreateCollectionItem(Transform parent)
            {
                return MonoFactoryProvider.Instance.GetInstance(_listItemPrefab, transform);
            }

            protected virtual async void InitItem(ViewBase view, object viewModel, int index)
            {
                if (viewModel is ViewModelBase vm)
                    await view.InitializeAsync(vm);
                else
                    Logger.Instance.Log($"tried to initialize view of type {view.GetType().Name} with a non viewmodel instance of type {viewModel.GetType().Namespace}");
            }

            private HashSet<int> _staticItemsToRebind =new HashSet<int>();
            protected virtual void RemoveElement(int index, object oldItem)
            {
                if (index > InstantiatedItems.Count) throw new IndexOutOfRangeException();
                var idx = UseStaticItems == false || _reversedOrder == false ? index : InstantiatedItems.Count - 1 - index;

                var item = InstantiatedItems[idx];
                item.DiscardView();
                if (item is ICollectionItemOnRemoveListener rl)
                    rl.OnRemoveFromCollection();
                if (UseStaticItems)
                {
                    _staticItemsToRebind.Add(idx);
                    item.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(item.gameObject);
                    InstantiatedItems.RemoveAt(index);
                }
                OnCollectionChanged();
            }

            private void OnValidate()
            {
                _src = GetComponent<CollectionViewSource>();
            }
        }
    }
}
