using MVVM.Binding.Model;
using MVVM.Core.ViewModels;
using MVVM.Extension.Views.Common;

namespace MVVM.Binding.Binding
{
    public interface ISelectableItem
    {
        void SetSelected(bool v);
        bool IsSelected { get; }
    }
}
