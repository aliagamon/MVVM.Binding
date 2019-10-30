using UnityEngine;

namespace MVVM.Binding.Binding
{
    public interface IDatabindingConnection
    {
        BindTarget SrcTarget{ get; }
        BindTarget DstTarget{ get; }
        string Owner { get; }
        GameObject OwnerObject { get; }
        bool IsBound { get; }
    }
}
