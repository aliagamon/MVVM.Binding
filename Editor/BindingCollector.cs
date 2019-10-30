using System.Linq;
using System.Collections.Generic;
using MVVM.Binding.Binding;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Utils.Collections.Generic;
using Utils.Unity.Runtime;
using System;

namespace MVVM.Binding.Editor
{
    public static class BindingCollector
    {

        public class CollectorHandle : IDisposable
        {
            private HashSet<Scene> _scenesToDispose = new HashSet<Scene>();

            public void AddSceneToDispose(Scene scene) => _scenesToDispose.Add(scene);

            public void Dispose()
            {
                foreach (var scene in _scenesToDispose)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static IDisposable CollectBindings(bool collectScenes, bool collectActiveScene, bool collectActiveBuildScenes, bool collectInactiveBuildScenes, bool collectNonBuildScenes, bool collectPrefabs, out Dictionary<DataBindingBase, string> bindings)
        {
            bindings = new Dictionary<DataBindingBase, string>();
            var preTestScenePath = SceneManager.GetActiveScene().path;

            var handle = new CollectorHandle();

            if (collectScenes)
            {
                if (collectActiveScene)
                    bindings.UnionWith(CollectDataBindingsInScene(preTestScenePath, preTestScenePath, ref handle));
                if (collectActiveBuildScenes)
                    foreach (var scene in EditorBuildSettings.scenes)
                        if (collectInactiveBuildScenes || scene.enabled)
                            bindings.UnionWith(CollectDataBindingsInScene(scene.path, preTestScenePath, ref handle));
                if (collectNonBuildScenes)
                    foreach (var scene in CollectAllScenes())
                        bindings.UnionWith(CollectDataBindingsInScene(scene, preTestScenePath, ref handle));
            }
            if (collectPrefabs)
                foreach (var prefab in CollectAllPrefabs())
                    bindings.UnionWith(CollectDataBindingsInPrefab(prefab));


            return handle;
        }


        private static string[] CollectAllScenes()
        {
            var sceneGUIDs = AssetDatabase.FindAssets("t:Scene");
            return sceneGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        }

        private static string[] CollectAllPrefabs()
        {
            var prefabGUIDs = AssetDatabase.FindAssets("t:prefab");
            return prefabGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        }


        private static Dictionary<DataBindingBase, string> CollectDataBindingsInScene(string path, in string preTestScenePath, ref CollectorHandle handle)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var roots = scene.GetRootGameObjects();
            var bindings =
                (from item in roots
                 select item.GetComponentsInChildren<DataBindingBase>(true)
                    into componnents
                 where !(componnents is null)
                 from com in componnents
                 select com).ToDictionary(com => com,
                    com => $"{path}{com.gameObject.GetPathInScene()}/{com.GetType().Name}");
            if (preTestScenePath != path)
                handle.AddSceneToDispose(scene);
            return bindings;
        }

        private static Dictionary<DataBindingBase, string> CollectDataBindingsInPrefab(string path)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var components = go.GetComponentsInChildren<DataBindingBase>(true);
            return components.ToDictionary(com => com,
                com => $"{path}{com.gameObject.GetPathInScene()}/{com.GetType().Name}");
        }
    }
}
