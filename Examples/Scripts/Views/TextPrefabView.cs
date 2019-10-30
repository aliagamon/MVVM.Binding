using System.ComponentModel.Composition;
using MVVM.Binding.View;
using MVVM.Extension.Views.Common;
using UnityEngine.UI;

namespace MVVM.Binding.Examples
{
    [Export]
    public class TextPrefabView : ViewBase<TestViewModel.TestDataViewModel>, ICollectionItemOnAddListener
    {
        public Text text;
        public Image image;

        private void Awake()
        {
            text = GetComponentInChildren<Text>();
            image = GetComponent<Image>();
        }

        public void OnAddedToCollection()
        {
            text.text = ViewModel.Message;
            image.color = ViewModel.Color;
        }
    }
}