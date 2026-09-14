using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace IWantToBeTheHero.Tests
{
    [InitializeOnLoad]
    public static class LifecycleTestRunner
    {
        private static readonly TestRunnerApi Api;

        static LifecycleTestRunner()
        {
            Api = ScriptableObject.CreateInstance<TestRunnerApi>();
            Api.RegisterCallbacks(new Results());
        }

        [MenuItem("Tools/Hero/Run Lifecycle Regression Tests")]
        public static void Run()
        {
            SessionState.SetBool("HeroLifecycleActive", true);
            SessionState.SetString("HeroLifecycleResults", "Running lifecycle regression tests.");
            Api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "IWantToBeTheHero.EditorTests" }
            }));
        }

        private sealed class Results : ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool("HeroLifecycleActive", false) || result.Test.IsSuite) return;
                SessionState.SetString("HeroLifecycleResults",
                    SessionState.GetString("HeroLifecycleResults", "") + "\n" +
                    result.Name + ": " + result.TestStatus + " " + result.Message);
            }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool("HeroLifecycleActive", false)) return;
                SessionState.SetBool("HeroLifecycleActive", false);
                string report = Path.GetFullPath("Logs/hero-lifecycle-results.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(report));
                TestRunnerApi.SaveResultToFile(result, report);
                SessionState.SetString("HeroLifecycleResults",
                    SessionState.GetString("HeroLifecycleResults", "") + "\nCOMPLETE: " +
                    result.PassCount + " passed, " + result.FailCount + " failed, " +
                    result.SkipCount + " skipped. Report: " + report);
            }
        }
    }
}
