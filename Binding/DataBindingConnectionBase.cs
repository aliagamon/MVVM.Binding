using System;
using UnityEngine;

namespace MVVM.Binding.Binding
{
    public abstract class DataBindingConnectionBase : IDatabindingConnection, IDisposable
    {
        public BindTarget SrcTarget { get; private set; }
        public BindTarget DstTarget { get; private set; }
        public string Owner => OwnerObject?.name;
        public GameObject OwnerObject { get; private set; }
        public bool IsBound { get; protected set; }
        public abstract void Dispose();

        protected DataBindingConnectionBase(GameObject owner, BindTarget src, BindTarget dst)
        {
            OwnerObject = owner;
            SrcTarget = src;
            DstTarget = dst;
        }
    }
}
