using System;
using UnityEngine;
using Utils.Linq;

namespace MVVM.Binding.View
{
    public class ToggleIntCodeGroupVisibilityHandler : MonoBehaviour
    {
        [Serializable]
        public class ToggleItem
        {
            public int Id;
            public GameObject[] Objects;
        }

        [SerializeField] private ToggleItem[] _toggleItems;
        private int _activeId;

        public int ActiveId
        {
            get => _activeId;
            set => SetActiveId(value);
        }

        private void SetActiveId(int value)
        {
            _activeId = value;
            _toggleItems.ForEach(t => t.Objects.ForEach(o=> o.SetActive(_activeId == t.Id)));
        }
    }
}