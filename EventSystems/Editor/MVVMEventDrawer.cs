using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MVVM.Binding.Binding;
using MVVM.Binding.Reactive;
using MVVM.Extension.Services;
using MVVM.Extension.Views.Common;
using MVVM.Binding.Utils;
using UniRx;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utils.Reflection;
using Debug = System.Diagnostics.Debug;

namespace MVVM.Binding.EventSystems.Editor
{

    [CustomPropertyDrawer(typeof(MVVMEventTrigger.TriggerEvent))]
    public class MVVMEventDrawer : PropertyDrawer
    {
        private Rect _rect;
        private SerializedProperty _property;
        private GUIContent _label;

        private ReorderableList _reorderableList;
        private SerializedProperty _listenersArray;
        private int _lastSelectedIndex;

        private int _targetIndex;

        private static GenericMenu.MenuFunction2 _menuCallBack0;
        private static GenericMenu.MenuFunction2 _menuCallBack1;

        private List<string> _targetVms = new List<string>();

        private Dictionary<string, State> _states = new Dictionary<string, State>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            _rect = position;
            _property = property;
            _label = label;
            _targetVms = ViewModelProvider.SGetViewModels();//Util.ViewModelProvider.Viewmodels;
            EditorGUI.BeginChangeCheck();
            var state = RestoreState(property);
            UpdateReorderableList();
            state._lastSelectedIndex = _lastSelectedIndex;
            if (EditorGUI.EndChangeCheck())
            {
                var cdp = _property.FindPropertyRelative("_callsDirty");
                cdp.boolValue = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RestoreState(property);
            var num = 0f;
            if (_reorderableList != null)
                num = _reorderableList.GetHeight();
            return num;
        }

        private void UpdateReorderableList()
        {
            if (_listenersArray == null || !this._listenersArray.isArray)
                return;
            if (_reorderableList == null)
                return;
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            _reorderableList.DoList(_rect);
            EditorGUI.indentLevel = indentLevel;
        }

        private State GetState(SerializedProperty prop)
        {
            string propertyPath = prop.propertyPath;
            State state;
            _states.TryGetValue(propertyPath, out state);
            if (state == null || state._reorderableList.serializedProperty.serializedObject != prop.serializedObject)
            {
                if (state == null)
                    state = new State();
                var propertyRelative = prop.FindPropertyRelative("_calls");
                state._reorderableList = new ReorderableList(prop.serializedObject, propertyRelative, false, true, true, true);
                state._reorderableList.drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(DrawEventHeader);
                state._reorderableList.drawElementCallback = new ReorderableList.ElementCallbackDelegate(DrawEventListener);
                state._reorderableList.onAddCallback = new ReorderableList.AddCallbackDelegate(AddTrigger);
                state._reorderableList.elementHeight = 61F;
                _states[propertyPath] = state;
            }
            return state;
        }

        private State RestoreState(SerializedProperty property)
        {
            var state = GetState(property);
            _listenersArray = state._reorderableList.serializedProperty;
            _reorderableList = state._reorderableList;
            return state;
        }

        private Rect[] GetRowRects(Rect rect)
        {
            Rect[] rectArray = new Rect[7];
            rect.height = 16f;
            rect.y += 2f;
            var rect5 = rect;
            rect5.width *= 0.3f;
            var rect6 = rect;
            rect6.xMin = rect5.xMax + 5f;
            rect6.width *= 0.8f;
            var rect7 = rect;
            rect7.xMin = rect6.xMax + 5f;
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect rect1 = rect;
            rect1.width *= 0.3f;
            Rect rect2 = rect1;
            rect2.y += EditorGUIUtility.singleLineHeight + 2f;
            Rect rect3 = rect;
            rect3.xMin = rect2.xMax + 5f;
            Rect rect4 = rect3;
            rect4.y += EditorGUIUtility.singleLineHeight + 2f;
            rectArray[0] = rect1;
            rectArray[1] = rect2;
            rectArray[2] = rect3;
            rectArray[3] = rect4;
            rectArray[4] = rect5;
            rectArray[5] = rect6;
            rectArray[6] = rect7;
            return rectArray;
        }

        public void AddTrigger(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
            var arrayElementAtIndex = list.serializedProperty.GetArrayElementAtIndex(list.index);
            arrayElementAtIndex.FindPropertyRelative("_view").objectReferenceValue = FindView(arrayElementAtIndex.serializedObject);
        }

        #region Draw callbacks
        private void DrawEventHeader(Rect headerRect)
        {
            headerRect.height = 16f;
            var text = (!string.IsNullOrEmpty(_label.text) ? _label.text : "Event");
            GUI.Label(headerRect, text);
        }

        private void DrawEventListener(Rect rect, int index, bool isactive, bool isfocused)
        {
            var arrayElementAtIndex = _listenersArray.GetArrayElementAtIndex(index);
            ++rect.y;
            var rowRects = GetRowRects(rect);
            var position1 = rowRects[0];
            var position2 = rowRects[1];
            var rect1 = rowRects[2];
            var position3 = rowRects[3];
            var position4 = rowRects[4];
            var position5 = rowRects[5];
            var position6 = rowRects[6];
            SerializedProperty propertyRelative1 = arrayElementAtIndex.FindPropertyRelative("_callState");
            SerializedProperty propertyRelative2 = arrayElementAtIndex.FindPropertyRelative("_mode");
            SerializedProperty propertyRelative3 = arrayElementAtIndex.FindPropertyRelative("_arguments");
            SerializedProperty propertyRelative4 = arrayElementAtIndex.FindPropertyRelative("_view");
            SerializedProperty propertyRelative5 = arrayElementAtIndex.FindPropertyRelative("_methodName");
            SerializedProperty propertyRelative7 = arrayElementAtIndex.FindPropertyRelative("_vmName");

            _targetIndex = _targetVms.IndexOf(propertyRelative7.stringValue);

            var backColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            EditorGUI.PropertyField(position1, propertyRelative1, GUIContent.none);
            EditorGUI.BeginChangeCheck();
            GUI.Box(position2, GUIContent.none);
            // EditorGUI.PropertyField(position2, propertyRelative4, GUIContent.none);
            EditorGUI.LabelField(position4, "Source View");
            EditorGUI.PropertyField(position5, propertyRelative4, GUIContent.none);
            if(GUI.changed && !(propertyRelative4.objectReferenceValue is null) && propertyRelative4.objectReferenceValue.GetType().TryGetGenericTypeRecursive(DataBindingBase.PreSelectedViewFindTest, out _))
                propertyRelative4.objectReferenceValue = null;
            var reFetchStyle = new GUIStyle(GUI.skin.GetStyle("Button")) { fontSize = 17 };
            if (GUI.Button(position6, new GUIContent('\u21BB'.ToString()), reFetchStyle))
                propertyRelative4.objectReferenceValue = FindView(arrayElementAtIndex.serializedObject);

            var preGUIEnableState = GUI.enabled;
            int newTargetIndex;
            bool isVmNameDirty = false;
            if(!(propertyRelative4.objectReferenceValue is null))
            {
                var tVm = propertyRelative4.objectReferenceValue.GetType().GetGenericTypeRecursive(DataBindingBase.PreSelectedViewFindTest).GenericTypeArguments[0].ToString();
                newTargetIndex = _targetIndex = _targetVms.IndexOf(tVm);
                isVmNameDirty = true;
                GUI.enabled = false;
            }
            newTargetIndex = EditorGUI.Popup(position2, _targetIndex, _targetVms.ToArray());
            GUI.enabled = preGUIEnableState;
            isVmNameDirty = isVmNameDirty || (_targetIndex == newTargetIndex);
            if (isVmNameDirty)
                propertyRelative7.stringValue = _targetVms[newTargetIndex];
            if (EditorGUI.EndChangeCheck())
                propertyRelative5.stringValue = null;
            PersistentListenerMode persistentListenerMode = GetMode(propertyRelative2);
            if (string.IsNullOrEmpty(propertyRelative7.stringValue) || string.IsNullOrEmpty(propertyRelative5.stringValue))
                persistentListenerMode = PersistentListenerMode.Void;
            SerializedProperty propertyRelative6;
            switch (persistentListenerMode)
            {
                case PersistentListenerMode.Object:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_ObjectArgument");
                    break;
                case PersistentListenerMode.Int:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_IntArgument");
                    break;
                case PersistentListenerMode.Float:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_FloatArgument");
                    break;
                case PersistentListenerMode.String:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_StringArgument");
                    break;
                case PersistentListenerMode.Bool:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_BoolArgument");
                    break;
                default:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_IntArgument");
                    break;
            }
            string stringValue = propertyRelative3.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue;
            System.Type type = typeof(UnityEngine.Object);
            if (!string.IsNullOrEmpty(stringValue))
                type = System.Type.GetType(stringValue, false) ?? typeof(UnityEngine.Object);
            switch (persistentListenerMode)
            {
                case PersistentListenerMode.EventDefined:
                case PersistentListenerMode.Void:
                    var funcMenu = BuildPopupList(propertyRelative7.stringValue, arrayElementAtIndex);
                    using (new EditorGUI.DisabledScope(!ViewModelProvider.IsViewModelTypeNameValid(propertyRelative7.stringValue) || funcMenu.GetItemCount() <= 0))
                    {
                        EditorGUI.BeginProperty(rect1, GUIContent.none, propertyRelative5);
                        GUIContent content;
                        if (EditorGUI.showMixedValue)
                        {
                            content = GetEditorGUIMixedValueContent();
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            if (funcMenu.GetItemCount() <= 0)
                                sb.Append("No Function");
                            else
                            {
                                sb.Append(propertyRelative7.stringValue);
                                if (!string.IsNullOrEmpty(propertyRelative5.stringValue))
                                {
                                    sb.Append(".");
                                    if (propertyRelative5.stringValue.StartsWith("set_"))
                                        sb.Append(propertyRelative5.stringValue.Substring(4));
                                    else
                                        sb.Append(propertyRelative5.stringValue);
                                }
                            }
                            content = GetGUIContentTemp(sb.ToString());
                        }
                        if (GUI.Button(rect1, content, EditorStyles.popup))
                            funcMenu.DropDown(rect1);
                        EditorGUI.EndProperty();
                    }
                    GUI.backgroundColor = backColor;
                    break;
                case PersistentListenerMode.Object:
                    EditorGUI.BeginChangeCheck();
                    UnityEngine.Object @object = EditorGUI.ObjectField(position3, GUIContent.none, propertyRelative6.objectReferenceValue, type, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        propertyRelative6.objectReferenceValue = @object;
                        goto case PersistentListenerMode.EventDefined;
                    }
                    else
                        goto case PersistentListenerMode.EventDefined;
                default:
                    EditorGUI.PropertyField(position3, propertyRelative6, GUIContent.none);
                    goto case PersistentListenerMode.EventDefined;
            }
        }
        #endregion

        #region InternalMock
        private static PersistentListenerMode GetMode(SerializedProperty mode)
        {
            return (PersistentListenerMode)mode.enumValueIndex;
        }

        private static GUIContent GetEditorGUIMixedValueContent()
        {
            var getMvc = typeof(EditorGUI).GetProperty("mixedValueContent", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(getMvc != null, nameof(getMvc) + " != null");
            return (GUIContent)getMvc.GetValue(null, null);
        }
        private static UnityEventBase GetDummyEvent(SerializedProperty prop)
        {
            UnityEngine.Debug.Log(prop.type);
            System.Type type = System.Type.GetType(prop.FindPropertyRelative("_typeName").stringValue, false);
            if (type == null)
                return (UnityEventBase)new UnityEvent();
            return Activator.CreateInstance(type) as UnityEventBase;
        }
        private static GUIContent GetGUIContentTemp(string t)
        {
            var funTemp = typeof(GUIContent).GetMethod("Temp", BindingFlags.NonPublic | BindingFlags.Static,
                Type.DefaultBinder, CallingConventions.Any, new Type[] { typeof(string) }, new ParameterModifier[] { });
            Debug.Assert(funTemp != null, nameof(funTemp) + " != null");
            return (GUIContent)funTemp.Invoke(null, new object[] { t });
        }

        private static GenericMenu BuildPopupList(string target, SerializedProperty listener)
        {
            SerializedProperty propertyRelative = listener.FindPropertyRelative("_methodName");

            var m = new GenericMenu();
            int num = string.IsNullOrEmpty(propertyRelative.stringValue) ? 1 : 0;
            if (_menuCallBack0 == null)
                _menuCallBack0 = new GenericMenu.MenuFunction2(ClearEventFunction);
            var fMgCache0 = _menuCallBack0;
            var local = (ValueType)new UnityEventFunction(listener, null, null, PersistentListenerMode.EventDefined, null);
            var content = new GUIContent("No Function");
            m.AddItem(content, num != 0, fMgCache0, local);
            if (string.IsNullOrEmpty(target)) return m;
            m.AddSeparator("");
            Type[] array = {typeof(BaseEventData)};
            GeneratePopUpForType(m, target, true, listener, array);
            return m;
        }
        private struct UnityEventFunction
        {
            private readonly SerializedProperty m_Listener;
            private readonly string m_Target;
            private readonly MethodInfo m_Method;
            private readonly PersistentListenerMode m_Mode;
            private readonly string m_Field;

            public UnityEventFunction(
              SerializedProperty listener,
              string target,
              MethodInfo method,
              PersistentListenerMode mode,
              string field)
            {
                this.m_Listener = listener;
                this.m_Target = target;
                this.m_Method = method;
                this.m_Mode = mode;
                this.m_Field = field;
            }

            public void Assign()
            {
                SerializedProperty propertyRelative1 = this.m_Listener.FindPropertyRelative("_vmName");
                SerializedProperty propertyRelative2 = this.m_Listener.FindPropertyRelative("_methodName");
                SerializedProperty propertyRelative3 = this.m_Listener.FindPropertyRelative("_mode");
                SerializedProperty propertyRelative4 = this.m_Listener.FindPropertyRelative("_arguments");
                propertyRelative1.stringValue = this.m_Target;
                propertyRelative2.stringValue = this.m_Field;
                propertyRelative3.enumValueIndex = (int)this.m_Mode;
                if (this.m_Mode == PersistentListenerMode.Object)
                {
                    SerializedProperty propertyRelative5 = propertyRelative4.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                    System.Reflection.ParameterInfo[] parameters = this.m_Method.GetParameters();
                    propertyRelative5.stringValue = parameters.Length != 1 || !typeof(UnityEngine.Object).IsAssignableFrom(parameters[0].ParameterType) ? typeof(UnityEngine.Object).AssemblyQualifiedName : parameters[0].ParameterType.AssemblyQualifiedName;
                }
                this.ValidateObjectParamater(propertyRelative4, this.m_Mode);
                this.m_Listener.serializedObject.ApplyModifiedProperties();
            }

            private void ValidateObjectParamater(
              SerializedProperty arguments,
              PersistentListenerMode mode)
            {
                SerializedProperty propertyRelative1 = arguments.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                SerializedProperty propertyRelative2 = arguments.FindPropertyRelative("m_ObjectArgument");
                UnityEngine.Object objectReferenceValue = propertyRelative2.objectReferenceValue;
                if (mode != PersistentListenerMode.Object)
                {
                    propertyRelative1.stringValue = typeof(UnityEngine.Object).AssemblyQualifiedName;
                    propertyRelative2.objectReferenceValue = (UnityEngine.Object)null;
                }
                else
                {
                    if (objectReferenceValue == (UnityEngine.Object)null)
                        return;
                    System.Type type = System.Type.GetType(propertyRelative1.stringValue, false);
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type) && type.IsInstanceOfType((object)objectReferenceValue))
                        return;
                    propertyRelative2.objectReferenceValue = (UnityEngine.Object)null;
                }
            }

            public void Clear()
            {
                this.m_Listener.FindPropertyRelative("_methodName").stringValue = (string)null;
                this.m_Listener.FindPropertyRelative("_mode").enumValueIndex = 1;
                this.m_Listener.serializedObject.ApplyModifiedProperties();
            }
        }
        private struct ValidMethodMap
        {
            public string target;
            public MethodInfo methodInfo;
            public string fieldName;
            public PersistentListenerMode mode;
        }
        private static void ClearEventFunction(object source)
        {
            ((UnityEventFunction)source).Clear();
        }

        private static void GeneratePopUpForType(
  GenericMenu menu,
  string target,
  bool useFullTargetName,
  SerializedProperty listener,
  System.Type[] delegateArgumentsTypes)
        {
            List<ValidMethodMap> methods = new List<ValidMethodMap>();
            string targetName = !useFullTargetName ? target.GetType().Name : target;
            bool flag = false;
            if (delegateArgumentsTypes.Length != 0)
            {
                GetMethodsForTargetAndMode(target, delegateArgumentsTypes, methods, PersistentListenerMode.EventDefined);
                if (methods.Count > 0)
                {
                    menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " + string.Join(", ", ((IEnumerable<System.Type>)delegateArgumentsTypes).Select<System.Type, string>((Func<System.Type, string>)(GetTypeName)).ToArray<string>())));
                    AddMethodsToMenu(menu, listener, methods, targetName);
                    flag = true;
                }
            }
            methods.Clear();
            GetMethodsForTargetAndMode(target, new System.Type[1]
            {
        typeof (float)
            }, methods, PersistentListenerMode.Float);
            GetMethodsForTargetAndMode(target, new System.Type[1]
            {
        typeof (int)
            }, methods, PersistentListenerMode.Int);
            GetMethodsForTargetAndMode(target, new System.Type[1]
            {
        typeof (string)
            }, methods, PersistentListenerMode.String);
            GetMethodsForTargetAndMode(target, new System.Type[1]
            {
        typeof (bool)
            }, methods, PersistentListenerMode.Bool);
            GetMethodsForTargetAndMode(target, new System.Type[1]
            {
        typeof (UnityEngine.Object)
            }, methods, PersistentListenerMode.Object);
            GetMethodsForTargetAndMode(target, new System.Type[0], methods, PersistentListenerMode.Void);
            if (methods.Count <= 0)
                return;
            if (flag)
                menu.AddItem(new GUIContent(targetName + "/ "), false, (GenericMenu.MenuFunction)null);
            if (delegateArgumentsTypes.Length != 0)
                menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
            AddMethodsToMenu(menu, listener, methods, targetName);
        }

        private static void GetMethodsForTargetAndMode(
            string target,
            System.Type[] delegateArgumentsTypes,
            List<ValidMethodMap> methods,
            PersistentListenerMode mode)
        {
            foreach (ValidMethodMap method in CalculateMethodMap(target, delegateArgumentsTypes, mode == PersistentListenerMode.Object))
            {
                var validMethodMap = method;
                validMethodMap.mode = mode;
                methods.Add(validMethodMap);
            }
        }
        private static IEnumerable<ValidMethodMap> CalculateMethodMap(
  string target,
  System.Type[] t,
  bool allowSubclasses)
        {
            var validMethodMapList = new List<ValidMethodMap>();
            if (!ViewModelProvider.IsViewModelTypeNameValid(target) || t == null) return validMethodMapList;
            var type = ViewModelProvider.GetViewModelType(target);
            var commands = type.GetProperties().Where(x => x.PropertyType.GetInterfaces().Any(i =>
                i.IsGenericType &&
                new []
                {
                    typeof(IReactiveCommand<>), typeof(IAsyncReactiveCommand<>)
                }.Any(tp=> i.GetGenericTypeDefinition() == tp)))
                /* .Select(c => c.FieldType.GetMethod("Execute")) */.ToArray();
            foreach (var com in commands)
            {
                MethodInfo c;
                if (com.PropertyType == typeof(RxCommand) || com.PropertyType == typeof(RxAsyncCommand))
                {
                    c = com.PropertyType.GetMethod("Execute", new Type[0]);
                }
                else if (com.PropertyType.IsGenericType &&
                         new []
                         {
                             typeof(RxCommand<>), typeof(RxAsyncCommand<>)
                         }.Any(tp=>com.PropertyType.GetGenericTypeDefinition() == tp))
                {
                    c = com.PropertyType.GetMethod("Execute");
                }
                else
                    continue;
                var parameters = c.GetParameters();
                if (parameters.Length == t.Length && c.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length <= 0)
                {
                    var flag = true;
                    for (int i = 0; i < t.Length; ++i)
                    {
                        if (!parameters[i].ParameterType.IsAssignableFrom(t[i]))
                            flag = false;
                        if (allowSubclasses && t[i].IsAssignableFrom(parameters[i].ParameterType))
                            flag = true;
                    }
                    if (flag)
                        validMethodMapList.Add(
                                new ValidMethodMap()
                                {
                                    target = target,
                                    methodInfo = c,
                                    fieldName = com.Name
                                });
                }
            }
            return validMethodMapList;
        }

        private static void AddMethodsToMenu(
            GenericMenu menu,
            SerializedProperty listener,
            List<ValidMethodMap> methods,
            string targetName)
        {
            foreach (var method in methods.OrderBy<ValidMethodMap, string>(e => e.methodInfo.Name))
                AddFunctionsForScript(menu, listener, method, targetName);
        }

        private static void AddFunctionsForScript(
  GenericMenu menu,
  SerializedProperty listener,
  ValidMethodMap method,
  string targetName)
        {
            PersistentListenerMode mode1 = method.mode;
            var objectReferenceValue = listener.FindPropertyRelative("_vmName").stringValue;
            string stringValue = listener.FindPropertyRelative("_methodName").stringValue;
            PersistentListenerMode mode2 = GetMode(listener.FindPropertyRelative("_mode"));
            SerializedProperty propertyRelative = listener.FindPropertyRelative("_arguments").FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
            StringBuilder stringBuilder = new StringBuilder();
            int length = method.methodInfo.GetParameters().Length;
            for (int index = 0; index < length; ++index)
            {
                System.Reflection.ParameterInfo parameter = method.methodInfo.GetParameters()[index];
                stringBuilder.Append(string.Format("{0}", (object)GetTypeName(parameter.ParameterType)));
                if (index < length - 1)
                    stringBuilder.Append(", ");
            }
            bool flag = objectReferenceValue == method.target && stringValue == method.methodInfo.Name && mode1 == mode2;
            if (flag && mode1 == PersistentListenerMode.Object && method.methodInfo.GetParameters().Length == 1)
                flag &= method.methodInfo.GetParameters()[0].ParameterType.AssemblyQualifiedName == propertyRelative.stringValue;
            string formattedMethodName = GetFormattedMethodName(targetName, method.fieldName, stringBuilder.ToString(), mode1 == PersistentListenerMode.EventDefined);
            GenericMenu genericMenu = menu;
            GUIContent content = new GUIContent(formattedMethodName);
            int num = flag ? 1 : 0;
            if (_menuCallBack1 == null)
            {
                _menuCallBack1 = new GenericMenu.MenuFunction2(SetEventFunction);
            }

            GenericMenu.MenuFunction2 fMgCache1 = _menuCallBack1;
            var local = (ValueType)new UnityEventFunction(listener, method.target, method.methodInfo, mode1, method.fieldName);
            genericMenu.AddItem(content, num != 0, fMgCache1, (object)local);
        }


        private static string GetTypeName(System.Type t)
        {
            if (t == typeof(int))
                return "int";
            if (t == typeof(float))
                return "float";
            if (t == typeof(string))
                return "string";
            if (t == typeof(bool))
                return "bool";
            return t.Name;
        }

        private static string GetFormattedMethodName(
            string targetName,
            string methodName,
            string args,
            bool dynamic)
        {
            if (dynamic)
            {
                if (methodName.StartsWith("set_"))
                    return string.Format("{0}/{1}", (object)targetName, (object)methodName.Substring(4));
                return string.Format("{0}/{1}", (object)targetName, (object)methodName);
            }
            if (methodName.StartsWith("set_"))
                return string.Format("{0}/{2} {1}", (object)targetName, (object)methodName.Substring(4), (object)args);
            return string.Format("{0}/{1} ({2})", (object)targetName, (object)methodName, (object)args);
        }

        private static void SetEventFunction(object source)
        {
            ((UnityEventFunction)source).Assign();
        }

        #endregion

        private static ViewBase FindView(SerializedObject so)
        {
            if (ViewFinder.FindRelativeView(((Component)so.targetObject).gameObject, ViewFinder.FindRelativeViewUpward, out ViewBase v))
                return v;
            return default;
        }

        private class State
        {
            public ReorderableList _reorderableList;
            public int _lastSelectedIndex;
        }
    }
}
