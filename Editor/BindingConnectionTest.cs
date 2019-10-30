using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Utils.Collections.Generic;
using Utils.Reflection;
using MVVM.Binding.Binding;
using MVVM.Binding.Binding.DataBindingTestCases;

namespace MVVM.Binding.Editor
{
    public static class BindingConnectionTest
    {
        public static void DoTest(bool testAbortOnMissingTestCase, bool testScenes, bool testActiveScene, bool testActiveBuildScenes, bool testInactiveBuildScenes, bool testNonBuildScenes, bool testPrefabs)
        {
            if (!CheckAllTestCasesExists(out var errors, out var testCaseMap) && testAbortOnMissingTestCase)
            {
                foreach (var error in errors)
                    Debug.LogError(error);
                throw new Exception("One or more binding classes don't have a test case, check the logs for more information.");
            }
            if (testCaseMap == null || testCaseMap.Count == 0)
            {
                throw new Exception("There is no test case aborting binding test.");
            }
            using (BindingCollector.CollectBindings(testScenes, testActiveScene, testActiveBuildScenes, testInactiveBuildScenes, testNonBuildScenes, testPrefabs, out var bindings))
            {
                var errorsCount = 0;
                foreach (var binding in bindings)
                {
                    if (testCaseMap.TryGetValue(binding.Key.GetType().Name, out var testCaseType))
                        if (!TestDataBinding(binding.Key, testCaseType, out var testErrors))
                        {
                            errorsCount++;
                            Debug.LogError($"Binding error at:{binding.Value}");
                            while (testErrors.Count > 0)
                                Debug.LogError(testErrors.Pop());
                        }
                }


                if (errorsCount > 0)
                    throw new Exception($"There is {errorsCount} errors in connection test, check your console for more informations!");

            }
        }


        private static bool CheckAllTestCasesExists(out List<string> errors, out Dictionary<string, Type> testCaseMap)
        {
            errors = new List<string>();
            testCaseMap = new Dictionary<string, Type>();
            var derivedTypes = typeof(DataBindingBase).GetDerivedTypes(AppDomain.CurrentDomain.GetAssemblies()).Where(t => !t.IsAbstract);

            bool searchTypeFromName(string typename, out Type type)
            {
                type = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                        from t in asm.GetTypes()
                        where t.Name == typename
                        select t).FirstOrDefault();
                return type != default;
            }

            bool rValue = true;

            foreach (var type in derivedTypes)
            {
                var testCaseClassName = $"{type.Name}TestCase";
                var testCaseBaseType = typeof(DataBindingTestCaseBase);
                if (searchTypeFromName(testCaseClassName, out var t) && testCaseBaseType.IsAssignableFrom(t))
                {
                    testCaseMap[type.Name] = t;
                    continue;
                }
                errors.Add($"Missing test case class for {type.Name}, You should create a test case class called {testCaseClassName}, derived from {testCaseBaseType.ToString()}!");
                rValue = false;
            }
            return rValue && testCaseMap.Count > 0;
        }

        private static bool TestDataBinding(DataBindingBase binding, Type testCaseType, out Stack<string> errors)
        {
            var ict = binding as IConnectionTestEventReciver;
            ict?.PreConnectionTest();

            var testCase = Activator.CreateInstance(testCaseType, new[] { binding }) as IDataBindingTestCase;
            var rValue = testCase.ExecuteTest() == true;
            errors = testCase.Errors;
            ict?.PostConnectionTest();
            return rValue;
        }
    }
}
