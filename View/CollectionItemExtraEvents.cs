using MVVM.Core.ViewModels;
using MVVM.Extension.Views.Common;

namespace MVVM.Binding.View
{
    public interface ICollectionItemOnAddListener
    {
        void OnAddedToCollection();
    }

    public interface ICollectionItemOnRemoveListener
    {
        void OnRemoveFromCollection();
    }

    public interface ICollectionItemOnReplaceListener
    {
        void OnReplacedInCollection();
    }

    public interface ICollectionItemOnMoveListener
    {
        void OnMoveInCollection();
    }
}
