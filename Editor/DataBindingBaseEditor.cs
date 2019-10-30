using System;
using System.Collections.Generic;
using System.Linq;
using MVVM.Binding.Binding;
using MVVM.Extension.Services;
using MVVM.Extension.Views.Common;
using UnityEditor;
using UnityEngine;
using Utils.Reflection;
using Utils.Unity.Editor;

namespace MVVM.Binding.Editor
{
    [CustomEditor(typeof(DataBindingBase), true)]
    public class DataBindingBaseEditor : UnityEditor.Editor
    {

        public int _viewModelIdx = 0;
        List<string> _viewModels = new List<string>();

        private SerializedProperty _viewmodelNameProp;

        private void OnEnable()
        {
            CollectSerializedProperties();
            (target as DataBindingBase).UpdateBindings();
        }

        protected virtual void CollectSerializedProperties()
        {
            _viewmodelNameProp = serializedObject.FindProperty("ViewModelName");
        }

        protected virtual void DrawChangeableElements()
        {
            var myClass = target as DataBindingBase;

            var preEnableState = GUI.enabled;
            var vmGenericMenu = new GenericMenu();
            if(!(myClass.PreSelectedView is null))
            {
                EditorGUILayout.HelpBox("You can not use singleton view model if a view is already selected!", MessageType.Info, true);
                GUI.enabled = false;
            }
            else
            {
                for (var i = 0; i < _viewModels.Count; i++)
                {
                    var localI = i;
                    vmGenericMenu.AddItem(new GUIContent(_viewModels[i].Replace('.','/')),
                            i == _viewModelIdx, () =>
                            {
                                _viewModelIdx = localI;
                                UpdateSerializedProperties();
                            });
                }
            }
            var isSelectedViewModelValid = _viewModelIdx >= 0 && _viewModels.Count > _viewModelIdx;

            if(!isSelectedViewModelValid)
                EditorGUILayout.HelpBox("Source ViewModel is no longer valid consider fixing it! If you refactored the name of class you can reserialize it with attribute usage, click here to get needed attribute copied in you clip board.", MessageType.Error);

            EditorGUILayoutHelper.PopGenericMenuAsDropDown(new GUIContent("Source ViewModel"), new GUIContent(isSelectedViewModelValid ? _viewModels[_viewModelIdx] : (ViewModelProvider.IsViewModelTypeNameValid(myClass.ViewModelName) ? "Empty" : myClass.ViewModelName)), vmGenericMenu, contentColor: isSelectedViewModelValid ? (Color?) null : Color.red);

            GUI.enabled = preEnableState;

        }

        protected virtual void UpdateSerializedProperties()
        {
            var myClass = target as DataBindingBase;

            myClass.ViewModelName = _viewModelIdx > -1 ?
                         _viewModels[_viewModelIdx] : myClass.ViewModelName;
            GUI.changed = true;
        }

        public override void OnInspectorGUI()
        {
            CollectSerializedProperties();

            _viewModels = ViewModelProvider.SGetViewModels();

            serializedObject.Update();
            var myClass = target as DataBindingBase;

            Undo.RecordObject(target, DateTime.Now.ToString());

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                if (!(myClass.PreSelectedView is null))
                {
                    if (!myClass.PreSelectedView.GetType().TryGetGenericTypeRecursive(DataBindingBase.PreSelectedViewFindTest, out _))
                    {
                        myClass.PreSelectedView = null;
                    }
                    else
                    {
                        myClass.ViewModelName = myClass.PreSelectedView.GetType().GetGenericTypeRecursive(DataBindingBase.PreSelectedViewFindTest).GenericTypeArguments[0]
                            .ToString();
                        myClass.UpdateBindings();
                    }
                }

            }


            // _viewModelIdx = _viewModels.ToList().IndexOf(_viewmodelNameProp.stringValue);
            _viewModelIdx = DataBindingBase.GetIndexOfViewModel(_viewmodelNameProp.stringValue, _viewModels);


            EditorGUI.BeginChangeCheck();

            DrawChangeableElements();

            if (EditorGUI.EndChangeCheck())
            {
                UpdateSerializedProperties();

                EditorUtility.SetDirty(target);

                serializedObject.ApplyModifiedProperties();

                myClass.UpdateBindings();

            }
        }
    }
}
