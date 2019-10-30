#define FIRST_PROPERTY_ENABLED
#define UNITY_EVENT_PROPERTY_ENABLED

#if FIRST_PROPERTY_ENABLED
#define FIRST_PROPERTY_ORIGINAL_NAME
#endif

using UnityEngine;
using UnityEngine.Events;
using MVVM.Binding.Binding;
using UnityEngine.UI;

namespace MVVM.Binding.Examples.Scripts.Components
{
    public class RefactorComponent : MonoBehaviour
    {
#if FIRST_PROPERTY_ENABLED
#if FIRST_PROPERTY_ORIGINAL_NAME
            [FormerlyBindedAs("PropertyRefactored")]
#else
        [FormerlyBindedAs("RefactorProperty")]
#endif
            public string
#if FIRST_PROPERTY_ORIGINAL_NAME
                    RefactorProperty
#else
           PropertyRefactored
#endif
            {
                    get => _firstPropertyValue;
                    set => _text.text = _firstPropertyValue = value;
            }

        private string _firstPropertyValue;
#endif

#if UNITY_EVENT_PROPERTY_ENABLED
        public UnityEvent RefactorEvent { get; set; }
#endif

        private Text _text;

        private void Start()
        {
            _text = GetComponent<Text>();
        }
    }
}
