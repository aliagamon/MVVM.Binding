using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Unity.Runtime;

namespace MVVM.Binding.Binding
{

    public class BindingMonitor
    {
        public static IEnumerable<IDatabindingConnection> ActiveConnections
        {
            get
            {
                return Connections?.Where(c => c.IsBound);
            }
        }
        public static IEnumerable<IDatabindingConnection> InactiveConnections
        {
            get
            {
                return Connections?.Where(c => !c.IsBound);
            }
        }

        public static List<IDatabindingConnection> Connections = new List<IDatabindingConnection>();
#if UNITY_EDITOR
        public static Dictionary<DataBindingBase, string> BindingComponents = new Dictionary<DataBindingBase, string>();
#endif

        public static void RegisterConnection(IDatabindingConnection c)
        {
            Connections.Add(c);
        }

        public static void UnRegisterConnection(IDatabindingConnection c)
        {
            Connections.Remove(c);
        }

#if UNITY_EDITOR
        public static string GetPathStringOfBinding(GameObject go, DataBindingBase binding = null)
        {
            return $"{go.scene.path}{go.GetPathInScene()}/{binding?.GetType().Name}";
        }

        public static void RegisterBinding(DataBindingBase binding)
        {
            BindingComponents[binding] = GetPathStringOfBinding(binding.gameObject, binding);
        }

        public static void UnRegisterBinding(DataBindingBase binding)
        {
            if (BindingComponents.ContainsKey(binding))
                BindingComponents.Remove(binding);
        }
#endif

        public static void Reset()
        {
            Connections.Clear();
        }
    }
}
