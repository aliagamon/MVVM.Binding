using UnityEditor;
using UnityEngine;
using MVVM.Binding.Binding;
using Utils.Unity.Editor;
using Utils.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System;
using Utils.Collections.Generic;
using MVVM.Binding.Binding.DataBindingTestCases;

namespace MVVM.Binding.Editor
{
    public class BindingConnectionTestWindow : EditorWindow
    {

        [MenuItem("Unity-MVVM/Binding Connection Test")]
        public static void Open()
        {
            var bctWindow = GetWindow<BindingConnectionTestWindow>();
            bctWindow.titleContent = new GUIContent("Binding Connections Test");
            bctWindow.Show();
        }

        private Vector2 _mainScroll;
        private bool _testAbortOnMissingTestCase = true;
        private bool _testScenes;
        private bool _testActiveScene;
        private bool _testActiveBuildScenes;
        private bool _testInactiveBuildScenes;
        private bool _testNonBuildScenes;
        private bool _testPrefabs;

        private string _preTestScenePath;

        private void OnGUI()
        {
            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);
            EditorGUILayout.LabelField(new GUIContent("Select things you want to test:"));
            EditorGUILayoutHelper.DrawLine();
            _testScenes = EditorGUILayout.Toggle(new GUIContent("Scenes"), _testScenes);
            EditorGUI.indentLevel++;
            if (!_testScenes)
                GUI.enabled = false;
            _testActiveScene = EditorGUILayout.Toggle(new GUIContent("Active scene"), _testActiveScene);
            _testActiveBuildScenes = EditorGUILayout.Toggle(new GUIContent("Active build scenes"), _testActiveBuildScenes);
            _testInactiveBuildScenes = EditorGUILayout.Toggle(new GUIContent("Inactive build scenes"), _testInactiveBuildScenes);
            _testNonBuildScenes = EditorGUILayout.Toggle(new GUIContent("Non-build scenes"), _testNonBuildScenes);
            EditorGUI.indentLevel--;
            GUI.enabled = true;
            _testPrefabs = EditorGUILayout.Toggle(new GUIContent("Prefabs"), _testPrefabs);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Select All")))
                SetAllTestValues(true);
            if (GUILayout.Button(new GUIContent("Deselect All")))
                SetAllTestValues(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayoutHelper.DrawLine();

            EditorGUILayout.HelpBox("If there is no test case class for a specific binder we can't test it, should test throw error on missing test cases?", MessageType.Warning);
            _testAbortOnMissingTestCase = EditorGUILayout.Toggle(new GUIContent("Throw (Recommended)"), _testAbortOnMissingTestCase);

            EditorGUILayoutHelper.DrawLine();

            if (GUILayout.Button(new GUIContent("Test")))
            {
                try
                {
                    BindingConnectionTest.DoTest(_testAbortOnMissingTestCase, _testScenes, _testActiveScene, _testActiveBuildScenes, _testInactiveBuildScenes, _testNonBuildScenes, _testPrefabs);

                EditorUtility.DisplayDialog("Success", "Test done without any errors", "Ok", "Cancel");
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error", e.Message, "Ok", "Cancel");
                }

            }


            EditorGUILayout.EndScrollView();

        }


        private void SetAllTestValues(bool value) =>
            _testScenes = _testActiveScene = _testActiveBuildScenes = _testInactiveBuildScenes = _testNonBuildScenes = _testPrefabs = value;




    }
}
