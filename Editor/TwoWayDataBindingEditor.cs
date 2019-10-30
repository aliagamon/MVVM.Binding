using MVVM.Binding.Binding;
using UnityEditor;
using UnityEngine;
using Utils.Unity.Editor;

namespace MVVM.Binding.Editor
{
    [CustomEditor(typeof(TwoWayDataBinding), true)]
    public class TwoWayDataBindingEditor : OneWayDataBindingEditor
    {
        private void OnEnable()
        {
            CollectSerializedProperties();
        }

        protected override void CollectSerializedProperties()
        {
            base.CollectSerializedProperties();
            _eventNameProp = serializedObject.FindProperty("_dstChangedEventName");
        }

        int _eventIdx = 0;
        SerializedProperty _eventNameProp;

        protected override void DrawChangeableElements()
        {
            base.DrawChangeableElements();
            var myClass = target as TwoWayDataBinding;

            var evntGenericMenu = new GenericMenu();

            for(var index = 0; index < myClass.DstChangedEvents.Count; index++)
            {
                var dstEvent = myClass.DstChangedEvents[index];
                var index1 = index;
                evntGenericMenu.AddItem(new GUIContent(dstEvent), index == _eventIdx, () =>
                        {
                            _eventIdx = index1;
                            UpdateSerializedProperties();
                        });
            }

            var isSelectedDstEventValid = _eventIdx >= 0 && myClass.DstChangedEvents.Count > _eventIdx;

            if(!isSelectedDstEventValid)
                EditorGUILayout.HelpBox("Destination Changed Event is no longer valid consider fixing it!", MessageType.Error);

            EditorGUILayoutHelper.PopGenericMenuAsDropDown(new GUIContent("Destination Changed Event"), new GUIContent(isSelectedDstEventValid ? myClass.DstChangedEvents[_eventIdx] : "Empty"), evntGenericMenu, contentColor: isSelectedDstEventValid ? (Color?) null : Color.red);

        }

        protected override void UpdateSerializedProperties()
        {
            base.UpdateSerializedProperties();

            var myClass = target as TwoWayDataBinding;

            myClass._dstChangedEventName = _eventIdx > -1 ?
                myClass.DstChangedEvents[_eventIdx] : myClass._dstChangedEventName;
        }

        public override void OnInspectorGUI()
        {

            var myClass = target as TwoWayDataBinding;

            _eventIdx = myClass.DstChangedEvents.IndexOf(_eventNameProp.stringValue);

            base.OnInspectorGUI();
        }
    }
}
