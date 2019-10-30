using System.Collections.Generic;
using System.Linq;
using MVVM.Binding.Binding;
using UnityEditor;
using UnityEngine;
using MVVM.Extension.Services;
using Utils.Unity.Editor;
using Utils.Unity.Runtime.Strings;

namespace MVVM.Binding.Editor
{
    [CustomEditor(typeof(CollectionViewSource), true)]
    public class CollectionViewSourceEditor : DataBindingBaseEditor
    {
        public int _srcIndex = 0;

        private List<string> _srcNameProp;

        protected override void CollectSerializedProperties()
        {
            base.CollectSerializedProperties();
            _srcNameProp = serializedObject.FindProperty("SrcCollections").GetPropertiesArray();
        }

        protected override void DrawChangeableElements()
        {
            base.DrawChangeableElements();

            var myClass = target as CollectionViewSource;
            var srcGenericMenu = new GenericMenu();

            for (var index = 0; index < _srcNameProp.Count; index++)
            {
                var srcCollection = _srcNameProp[index];
                var index1 = index;
                srcGenericMenu.AddItem(new GUIContent(srcCollection), index == _srcIndex, () =>
                        {
                            _srcIndex = index1;
                            UpdateSerializedProperties();
                        });
            }

            var isSelectedCollectionValid = _srcIndex >= 0 && _srcNameProp.Count > _srcIndex;

            if (!isSelectedCollectionValid)
            {
                EditorGUILayout.HelpBox("Source Collection Property is no longer valid consider fixing it! If you refactored the name of property you can reserialize it with attribute usage, click here to get needed attribute copied in you clip board.", MessageType.Error);
                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                {
                    $"[{nameof(FormerlyBindedAs)}(\"{myClass.SrcCollectionName.PropertyName}\")]".CopyToClipboard();
                }
            }

            EditorGUILayoutHelper.PopGenericMenuAsDropDown(new GUIContent("Source Collection"), new GUIContent(isSelectedCollectionValid ? _srcNameProp[_srcIndex] : (myClass.SrcCollectionName is null ? "Empty" : myClass.SrcCollectionName.PropertyName)), srcGenericMenu, contentColor: isSelectedCollectionValid ? (Color?)null : Color.red);


        }

        protected override void UpdateSerializedProperties()
        {
            base.UpdateSerializedProperties();
            var myClass = target as CollectionViewSource;
            myClass.SrcCollectionName = _srcIndex > -1 ?
                myClass.SrcCollections[_srcIndex] : myClass.SrcCollectionName;
        }

        public override void OnInspectorGUI()
        {

            if (!(target is CollectionViewSource myClass)) return;

            _srcIndex = myClass.SrcCollectionName is null
                ? -1
                : DataBindingBase.GetIndexOfBindablePropertyInfo(myClass.SrcCollectionName, ViewModelProvider.GetViewModelType(myClass.ViewModelName), myClass.SrcCollections);

            base.OnInspectorGUI();

        }

    }
}
