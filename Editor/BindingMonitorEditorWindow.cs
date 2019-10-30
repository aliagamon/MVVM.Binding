using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MVVM.Binding.Binding;
using Utils.Unity.Editor;
using Utils.Unity.Runtime.Strings;

namespace MVVM.Binding.Editor
{
    public class BindingMonitorEditorWindow : EditorWindow
    {
        public class SelectBindingsToCollectEditorPopup : EditorWindow
        {
            public struct SelectedBindingsData
            {
                public bool CollectScenes;
                public bool CollectActiveScene;
                public bool CollectActiveBuildScenes;
                public bool CollectInactiveBuildScenes;
                public bool CollectNonBuildScenes;
                public bool CollectPrefabs;

                public void SelectAll() => SetAll(true);
                public void DeselectAll() => SetAll(false);

                private void SetAll(bool value) => CollectScenes = CollectActiveScene = CollectActiveBuildScenes =
                    CollectInactiveBuildScenes = CollectNonBuildScenes = CollectPrefabs = value;
            }

            public static void Display(Action<SelectedBindingsData> OnCloseEvent)
            {
                var window = ScriptableObject.CreateInstance<SelectBindingsToCollectEditorPopup>();
                window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 200);
                window._onCloseEvent = OnCloseEvent;
                window.ShowPopup();
            }

            private SelectedBindingsData _data = new SelectedBindingsData();
            private Action<SelectedBindingsData> _onCloseEvent;

            private void OnGUI()
            {
                _data.CollectScenes = EditorGUILayout.Toggle("Scenes", _data.CollectScenes);
                EditorGUI.indentLevel++;
                var originalGUIEnabled = GUI.enabled;
                if (!_data.CollectScenes) GUI.enabled = false;
                _data.CollectActiveScene = EditorGUILayout.Toggle("Active Scene", _data.CollectActiveScene);
                _data.CollectActiveBuildScenes =
                    EditorGUILayout.Toggle("Active build scenes", _data.CollectActiveBuildScenes);
                _data.CollectInactiveBuildScenes =
                    EditorGUILayout.Toggle("Inactive build scenes", _data.CollectInactiveBuildScenes);
                _data.CollectNonBuildScenes = EditorGUILayout.Toggle("Non-build Scenes", _data.CollectNonBuildScenes);
                GUI.enabled = originalGUIEnabled;
                EditorGUI.indentLevel--;
                _data.CollectPrefabs = EditorGUILayout.Toggle("Prefabs", _data.CollectPrefabs);
                EditorGUILayoutHelper.DrawLine();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Select All"))) _data.SelectAll();
                if (GUILayout.Button(new GUIContent("Deselect All"))) _data.DeselectAll();
                EditorGUILayout.EndHorizontal();
                EditorGUILayoutHelper.DrawLine();

                if (GUILayout.Button(new GUIContent("Done")))
                {
                    _onCloseEvent?.Invoke(_data);
                    this.Close();
                }
            }
        }

        private enum OrderType
        {
            GameObject,
            ViewModel
        }

        private enum MonitoringType
        {
            Runtime,
            Editor
        }

        [MenuItem("Unity-MVVM/Binding Monitor")]
        public static void Open()
        {
            var window = GetWindow<BindingMonitorEditorWindow>();
            window.titleContent = new GUIContent("Binding Monitor");
            window.Show();
        }

        #region Scrolls

        private Vector2 _mainScroll;
        private Vector2 _oneWayScroll;
        private Vector2 _twoWayScroll;
        private Vector2 _collecitonScroll;
        private Vector2 _visibilityScroll;

        #endregion

        private bool _isPopupOpen;

        #region Monitoring Options

        private MonitoringType _monitoringType;

        private string _filter;
        private bool _ignoreCase;
        private OrderType _orderBy;

        #endregion

        #region Fold Groups status

        private bool _oneWayFold = true;
        private bool _twoWayFold = true;
        private bool _collectionFold = true;
        private bool _visibilityFold = true;

        #endregion

        private Dictionary<string, (Type, string)> RuntimeBindings => CollectDisplayInformation(BindingMonitor.BindingComponents, _defaultToStringMap);

        private Dictionary<string, (Type, string)> _editorBindings;
        private IDisposable _bindingCollectorHandle;

        private Dictionary<Type, Func<DataBindingBase, string>> _defaultToStringMap =
            new Dictionary<Type, Func<DataBindingBase, string>>()
            {
                {
                    typeof(OneWayDataBinding), binding =>
                    {
                        var item = (OneWayDataBinding) binding;
                        var str =
                            $"<b>{item.gameObject.name}</b> Src: <b>{item.ViewModelName}:{item.SrcPropertyName.PropertyName}({item.SrcPropertyName.PropertyType})</b> Dst: <b>{item._dstView.GetType().Name}:{item.DstPropertyName.PropertyName}</b> Bound: <b>{item.Connection?.IsBound ?? false}</b>";
                        return str;
                    }
                },
                {
                    typeof(TwoWayDataBinding), binding =>
                    {
                        var item = (TwoWayDataBinding) binding;
                        var str =
                            $"<b>{item.gameObject.name}</b> Src: <b>{item.ViewModelName}:{item.SrcPropertyName.PropertyName}({item.SrcPropertyName.PropertyType})</b> Dst: <b>{item._dstView.GetType().Name}/{item.DstPropertyName.PropertyName}</b> Event: <b>{item.DstPropertyName.PropertyName}</b> Bound: <b>{item._dstChangedEventName}</b>";
                        return str;
                    }
                },
                {
                    typeof(CollectionViewSource), binding =>
                    {
                        var item = (CollectionViewSource) binding;
                        var str =
                            $"<b>{item.gameObject.name}</b> Src: <b>{item.ViewModelName}:{item.SrcCollectionName.PropertyName}({item.SrcCollectionName.PropertyType})</b> Bound: <b>{item.IsBound}</b>";
                        return str;
                    }
                },
                {
                    typeof(VisibilityBinding), binding =>
                    {
                        var item = (VisibilityBinding) binding;
                        var str =
                            $"<b>{item.gameObject.name}</b> Src: <b>{item.ViewModelName}:{item.SrcPropertyName.PropertyName}({item.SrcPropertyName.PropertyType})</b> Dst: <b>{item._dstView.GetType().Name}:{item.DstPropertyName.PropertyName}</b> Bound: <b>{item.Connection?.IsBound ?? false}</b>";
                        return str;
                    }
                }
            };

        private void OnGUI()
        {
            var originalGUIEnabled = GUI.enabled;
            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);
            if (_isPopupOpen) GUI.enabled = false;
            EditorGUILayout.BeginHorizontal();

            var lastMonitoringType = _monitoringType;

            _monitoringType = (MonitoringType)EditorGUILayout.EnumPopup("Monitoring Type:", _monitoringType,
                GUILayout.MaxWidth(280), GUILayout.MaxHeight(20));

            if (lastMonitoringType != _monitoringType)
            {
                if (!(_editorBindings is null))
                    _editorBindings = null;
                _bindingCollectorHandle?.Dispose();
            }

            if (
                _monitoringType == MonitoringType.Editor
                && true
                && GUILayout.Button(
                    new GUIContent((_editorBindings is null ? "" : "Re") + "Collect bindings in project"),
                    GUILayout.MaxWidth(200), GUILayout.MaxHeight(20))
                && EditorUtility.DisplayDialog("Warning",
                    "The collected data will not be live and you have to recollect it if you wish!", "Ok", "Cancel"))
            {
                _isPopupOpen = true;
                SelectBindingsToCollectEditorPopup.Display(CollectBindings);
            }


            EditorGUILayout.EndHorizontal();

            if (_monitoringType == MonitoringType.Runtime && !EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("For using runtime monitoring you should play the game!", MessageType.Error);
                GUI.enabled = false;
            }

            EditorGUILayoutHelper.DrawLine();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Filter:"), GUILayout.MaxWidth(50));
            _filter = EditorGUILayout.TextField(_filter, GUILayout.MaxWidth(800));
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            _ignoreCase = EditorGUILayout.Toggle(new GUIContent("Ignore Case"), _ignoreCase, GUILayout.MaxWidth(100));
            _orderBy = (OrderType)EditorGUILayout.EnumPopup("Order By:", _orderBy);
            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUILayout.EndHorizontal();

            var bindings = _monitoringType == MonitoringType.Editor
                ? _editorBindings
                : RuntimeBindings;
            if (bindings is null || bindings.Count == 0)
            {
                var ogGUIEnabled = GUI.enabled;
                GUI.enabled = true;
                EditorGUILayout.HelpBox("There is no bindings found to monitor...", MessageType.Warning);
                GUI.enabled = ogGUIEnabled;
            }

            else
            {
                var orderedBindings = bindings.OrderBy(pair => _orderBy == OrderType.GameObject ? pair.Value.Item2 : pair.Key);
                var oneWays = orderedBindings
                    .Where(pair => pair.Value.Item1 == typeof(OneWayDataBinding))
                    .ToDictionary(x => x.Key, x => x.Value.Item2);

                var twoWays = orderedBindings
                    .Where(pair => pair.Value.Item1 == typeof(TwoWayDataBinding))
                    .ToDictionary(x => x.Key, x => x.Value.Item2);

                var collections = orderedBindings
                    .Where(pair => pair.Value.Item1 == typeof(CollectionViewSource))
                    .ToDictionary(x => x.Key, x => x.Value.Item2);

                var visibility = orderedBindings
                    .Where(pair => pair.Value.Item1 == typeof(VisibilityBinding))
                    .ToDictionary(x => x.Key, x => x.Value.Item2);

                var connections = BindingMonitor.Connections;

                var style = new GUIStyle(GUI.skin.label) { richText = true };
                _oneWayFold = EditorGUILayout.Foldout(_oneWayFold, new GUIContent($"One Way Bindings: {oneWays.Count}"),
                    true);

                if (_oneWayFold)
                {
                    _oneWayScroll = EditorGUILayout.BeginScrollView(_oneWayScroll, GUILayout.MaxHeight(666));
                    EditorGUI.indentLevel++;
                    foreach (var binding in oneWays)
                    {
                        DisplayItem(binding, connections, style);
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndScrollView();
                }

                _twoWayFold = EditorGUILayout.Foldout(_twoWayFold, new GUIContent($"Two Way Bindings: {twoWays.Count}"));

                if (_twoWayFold)
                {
                    _twoWayScroll = EditorGUILayout.BeginScrollView(_twoWayScroll, GUILayout.MaxHeight(666));
                    EditorGUI.indentLevel++;
                    foreach (var binding in twoWays)
                        DisplayItem(binding, connections, style);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndScrollView();
                }

                _collectionFold = EditorGUILayout.Foldout(_collectionFold, new GUIContent($"Collection Bindings: {collections.Count}"));

                if (_collectionFold)
                {
                    _collecitonScroll = EditorGUILayout.BeginScrollView(_collecitonScroll, GUILayout.MaxHeight(666));
                    EditorGUI.indentLevel++;
                    foreach (var binding in collections)
                        DisplayItem(binding, connections, style);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndScrollView();
                }

                _visibilityFold = EditorGUILayout.Foldout(_visibilityFold, new GUIContent($"Visibility Bindings: {visibility.Count}"));

                if (_visibilityFold)
                {
                    _visibilityScroll = EditorGUILayout.BeginScrollView(_visibilityScroll, GUILayout.MaxHeight(666));
                    EditorGUI.indentLevel++;
                    foreach (var binding in visibility)
                        DisplayItem(binding, connections, style);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndScrollView();
                }

                if (EditorApplication.isPlaying)
                {
                    EditorGUILayoutHelper.DrawLine();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent($"Active Connections: {connections.Count}"));

                    if (GUILayout.Button("Reset binding monitor", GUILayout.MaxWidth(200)))
                        BindingMonitor.Reset();

                    EditorGUILayout.EndHorizontal();


                }
            }


            EditorGUILayout.EndScrollView();

            GUI.enabled = originalGUIEnabled;
        }

        private void CollectBindings(SelectBindingsToCollectEditorPopup.SelectedBindingsData data)
        {
            _isPopupOpen = false;
            Debug.Log("Collecting bindings...");
            using (BindingCollector.CollectBindings(data.CollectScenes, data.CollectActiveScene,
                data.CollectActiveBuildScenes, data.CollectInactiveBuildScenes, data.CollectNonBuildScenes,
                data.CollectPrefabs, out var bindings))
            {
                _editorBindings = CollectDisplayInformation(bindings, _defaultToStringMap);
            }
        }

        private Dictionary<string, (Type, string)> CollectDisplayInformation(
            Dictionary<DataBindingBase, string> bindings, Dictionary<Type, Func<DataBindingBase, string>> toString)
        {
            var dic = new Dictionary<string, (Type, string)>();
            foreach (var binding in bindings)
            {
                dic[toString[binding.Key.GetType()].Invoke(binding.Key)] = (binding.Key.GetType(), binding.Value);
            }

            return dic;
        }

        private void DisplayItem(KeyValuePair<string, string> item, List<IDatabindingConnection> connections, GUIStyle style)
        {
            var display = string.IsNullOrEmpty(_filter) || (_ignoreCase
                              ? item.Key.ToLower().Contains(_filter.ToLower())
                              : item.Key.Contains(_filter));
            if (!display) return;
            var originalContentColor = GUI.contentColor;
            if (EditorApplication.isPlaying)
            {
                var connection = connections.FirstOrDefault(x =>
                        {
                            var xPath = BindingMonitor.GetPathStringOfBinding(x.OwnerObject);
                            return item.Value.StartsWith(xPath) && !item.Value.Replace(xPath, "").Contains("/");
                        });
                if (connection != default)
                    GUI.contentColor = connection.IsBound ? Color.green : Color.yellow;
            }
            EditorGUILayout.LabelField(item.Key, style);
            GUI.contentColor = originalContentColor;
            if (EditorGUILayoutHelper.LastControlRectClicked(0))
            {
                if (EditorUtility.DisplayDialog("Info", $"binding location: {item.Value}", "Copy To Clipboard", "Close"))
                    item.Value.CopyToClipboard();
            }
        }
    }
}
