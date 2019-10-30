using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVM.Binding.Binding;
using MVVM.Extension.Services;
using Rotorz.Games.Collections;
using UnityEditor;
using UnityEngine;
using Utils.Unity.Editor;
using Utils.Unity.Runtime.Strings;
using Utils.Reflection;

namespace MVVM.Binding.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(OneWayDataBinding), true)]
    public class OneWayDataBindingEditor : DataBindingBaseEditor
    {
        public int _srcIndex = 0;
        public int _dstIndex = 0;

        SerializedProperty _srcProps;
        SerializedProperty _dstProps;

        List<(string, bool)> _srcPropNames;
        List<string> _dstPropNames;

        private SerializedProperty _convertersProp;

        protected override void CollectSerializedProperties()
        {
            base.CollectSerializedProperties();

            _srcProps = serializedObject.FindProperty("SrcProps");
            _dstProps = serializedObject.FindProperty("DstProps");

            _srcPropNames = _srcProps.GetSrcPropertiesArray();
            _dstPropNames = _dstProps.GetPropertiesArray();

            _convertersProp = serializedObject.FindProperty("Converters");

        }

        protected override void DrawChangeableElements()
        {
            ReorderableListGUI.Title("Converters");
            ReorderableListGUI.ListField(_convertersProp,
                () => EditorGUILayout.LabelField("There is no converter in use.", EditorStyles.miniLabel));

            base.DrawChangeableElements();

            var myClass = target as OneWayDataBinding;
            var srcGenericMenu = new GenericMenu();
            for (var index = 0; index < _srcPropNames.Count; index++)
            {
                var (item1, item2) = _srcPropNames[index];
                var index1 = index;
                srcGenericMenu.AddItem(new GUIContent((item2 ? "Static/" : "Reactive/") + item1),
                    index == _srcIndex, () =>
                    {
                        _srcIndex = index1;
                        UpdateSerializedProperties();
                    });
            }
            var isSelectedSrcPropertyValid = _srcIndex >= 0 && _srcPropNames.Count > _srcIndex;

            if (!isSelectedSrcPropertyValid)
            {
                EditorGUILayout.HelpBox("Source Property is no longer valid consider fixing it! If you refactored the name of property you can reserialize it with attribute usage, click here to get needed attribute copied in you clip board.", MessageType.Error);
                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                {
                    $"[{nameof(FormerlyBindedAs)}(\"{myClass.SrcPropertyName.PropertyName}\")]".CopyToClipboard();
                }
            }

            EditorGUILayoutHelper.PopGenericMenuAsDropDown(new GUIContent("Source Property"), new GUIContent(isSelectedSrcPropertyValid ? _srcPropNames[_srcIndex].Item1 : (myClass.SrcPropertyName is null ? "Empty" : myClass.SrcPropertyName.PropertyName)), srcGenericMenu, contentColor: isSelectedSrcPropertyValid ? (Color?)null : Color.red);

            var dstGenericMenu = new GenericMenu();

            for (var index = 0; index < _dstPropNames.Count; index++)
            {
                var dstProp = _dstPropNames[index];
                var index1 = index;
                dstGenericMenu.AddItem(new GUIContent(dstProp), index == _dstIndex, () =>
                        {
                            _dstIndex = index1;
                            UpdateSerializedProperties();
                        });
            }

            var isSelectedDstPropertyValid = _dstIndex >= 0 && _dstPropNames.Count > _dstIndex;

            if (!isSelectedDstPropertyValid)
            {
                EditorGUILayout.HelpBox("Destination Property is no longer valid consider fixing it! If you refactored the name of property you can reserialize it with attribute usage, click here to get needed attribute copied in you clip board.", MessageType.Error);
                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                {
                    $"[{nameof(FormerlyBindedAs)}(\"{myClass.DstPropertyName.PropertyName}\")]".CopyToClipboard();
                }
            }

            EditorGUILayoutHelper.PopGenericMenuAsDropDown(new GUIContent("Destination Property"), new GUIContent(isSelectedDstPropertyValid ? _dstPropNames[_dstIndex] : (myClass.DstPropertyName is null ? "Empty" : myClass.DstPropertyName.PropertyName)), dstGenericMenu, contentColor: isSelectedDstPropertyValid ? (Color?)null : Color.red);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_propertySet"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_propertyChanged"));
        }

        protected override void UpdateSerializedProperties()
        {
            base.UpdateSerializedProperties();
            if (!(target is OneWayDataBinding myClass)) return;

            myClass.SrcPropertyName = _srcIndex > -1 ?
                myClass.SrcProps[_srcIndex] : myClass.SrcPropertyName;


            myClass.DstPropertyName = _dstIndex > -1 ?
                myClass.DstProps[_dstIndex] : myClass.DstPropertyName;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!(target is OneWayDataBinding myClass)) return;


            _srcIndex = myClass.SrcPropertyName is null || !ViewModelProvider.IsViewModelTypeNameValid(myClass.ViewModelName)
                ? -1
                : DataBindingBase.GetIndexOfBindablePropertyInfo(myClass.SrcPropertyName, ViewModelProvider.GetViewModelType(myClass.ViewModelName), myClass.SrcProps);

            _dstIndex = myClass.DstPropertyName is null || myClass._dstView is null
                ? -1
                : DataBindingBase.GetIndexOfBindablePropertyInfo(myClass.DstPropertyName, myClass._dstView.GetType(), myClass.DstProps);
        }
    }

    public static class SerializedPropertyExt
    {

        public static List<string> GetStringArray(this SerializedProperty prop)
        {
            List<string> list = new List<string>(prop.arraySize);

            for (int i = 0; i < prop.arraySize; i++)
            {
                list.Add(prop.GetArrayElementAtIndex(i).stringValue);
            }

            return list;
        }

        public static List<string> GetPropertiesArray(this SerializedProperty prop)
        {
            List<string> list = new List<string>(prop.arraySize);

            for (int i = 0; i < prop.arraySize; i++)
            {
                var arrayElementAtIndex = prop.GetArrayElementAtIndex(i);
                list.Add(
                    $"{arrayElementAtIndex.FindPropertyRelative("PropertyName").stringValue}({arrayElementAtIndex.FindPropertyRelative("PropertyType").stringValue})");
            }

            return list;
        }

        public static List<(string, bool)> GetSrcPropertiesArray(this SerializedProperty prop)
        {
            var list = new List<(string, bool)>();
            for (int i = 0; i < prop.arraySize; ++i)
            {
                var arrayElementAtIndex = prop.GetArrayElementAtIndex(i);
                list.Add((
                    $"{arrayElementAtIndex.FindPropertyRelative("PropertyName").stringValue}({arrayElementAtIndex.FindPropertyRelative("PropertyType").stringValue})",
                    arrayElementAtIndex.FindPropertyRelative("IsStatic").boolValue));
            }

            return list;
        }

    }
}
