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
            Start(null);
        }

        [MenuItem("Tools/Hero/Run Movement Tests")]
        public static void RunMovement()
        {
            Start(new[] { "IWantToBeTheHero.Tests.MovementTests" });
        }

        [MenuItem("Tools/Hero/Run Movement Camera Test")]
        public static void RunCamera()
        {
            Start(new[] { "IWantToBeTheHero.Tests.MovementTests.Camera_AspectChangesKeepHeroVisibleAtBothRoomEdges" });
        }

        [MenuItem("Tools/Hero/Run Main Scene Authoring Tests")]
        public static void RunMainAuthoring()
        {
            Start(new[] {
                "IWantToBeTheHero.Tests.MainSceneAuthoringTests",
                "IWantToBeTheHero.Tests.LifecycleTests"
            });
        }

        [MenuItem("Tools/Hero/Run Main Scene Editability Test")]
        public static void RunMainEditability()
        {
            Start(new[] { "IWantToBeTheHero.Tests.MainSceneAuthoringTests" });
        }

        [MenuItem("Tools/Hero/Run Platform Regression Tests")]
        public static void RunPlatforms()
        {
            Start(new[] {
                "IWantToBeTheHero.Tests.MovementTests.MovingPlatform_AnimationUsesMovementRelativeToSupport",
                "IWantToBeTheHero.Tests.MovementTests.Platforms_OneWayLandingMovingCarryAndLowCeiling"
            });
        }

        private static void Start(string[] names)
        {
            SessionState.SetBool("HeroLifecycleActive", true);
            SessionState.SetString("HeroLifecycleResults", "Running lifecycle regression tests.");
            Api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "IWantToBeTheHero.EditorTests" },
                testNames = names
            }));
        }

        [MenuItem("Tools/Hero/Run Animation Audit")]
        public static void RunAnimationAudit()
        {
            Start(new[] { "IWantToBeTheHero.Tests.AnimationAuditTests" });
        }

        [MenuItem("Tools/Hero/Run Enemy Animation Audit")]
        public static void RunEnemyAnimationAudit()
        {
            Start(new[] { "IWantToBeTheHero.Tests.AnimationAuditTests.LiveEnemies_AllExistingClips_CaptureTransitions" });
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
