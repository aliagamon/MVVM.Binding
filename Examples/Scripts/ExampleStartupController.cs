using MVVM.Extension.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace MVVM.Binding.Examples
{
    public class ExampleStartupController : StartupControllerBase
    {
        public GameObject mvvmDummy;
        private void Start()
        {
            mvvmDummy.SetActive(true);
        }
    }
}
