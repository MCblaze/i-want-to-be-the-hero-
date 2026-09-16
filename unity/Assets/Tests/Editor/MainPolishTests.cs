using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class MainPolishTests
    {
        [UnityTest]
        public IEnumerator Main_PauseFreezesInputsAndRestartRestoresTime()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            var game = Object.FindAnyObjectByType<HeroGame>();
            game.StartQuest();
            yield return new WaitForSeconds(.2f);
            game.SetPaused(true);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(AudioListener.pause, Is.True);
            Assert.That(game.UI.transform.Find("Pause Panel").gameObject.activeSelf, Is.True);
            var position = game.Hero.transform.position;
            int seeds = game.Hero.SeedsFired;
            MobileInput.Set(MobileAction.Right, true);
            MobileInput.Set(MobileAction.Swap, true);
            MobileInput.Set(MobileAction.Attack, true);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(game.Hero.transform.position, Is.EqualTo(position));
            Assert.That(game.Hero.Weapon, Is.EqualTo(HeroWeapon.Sword));
            Assert.That(game.Hero.SeedsFired, Is.EqualTo(seeds));
            game.UI.transform.Find("Pause Panel/Card/Resume").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.That(game.Paused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(AudioListener.pause, Is.False);
            Assert.That(MobileInput.Right, Is.False);
            Assert.That(MobileInput.ConsumeSwap() || MobileInput.ConsumeAttack(), Is.False);
            game.SetPaused(true);
            int oldId = game.GetInstanceID();
            bool reduced = AccessibilitySettings.ReducedEffects;
            AccessibilitySettings.SetReducedEffects(true);
            game.ResetQuest();
            float deadline = Time.realtimeSinceStartup + 3f;
            while(Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                var current = Object.FindAnyObjectByType<HeroGame>();
                if(current != null && current.GetInstanceID() != oldId) break;
            }
            var restarted = Object.FindAnyObjectByType<HeroGame>();
            Assert.That(restarted.GetInstanceID(), Is.Not.EqualTo(oldId));
            Assert.That(restarted.Paused || restarted.Started, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(AudioListener.pause, Is.False);
            yield return new WaitForSeconds(.1f);
            foreach(var particles in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                Assert.That(particles.isPlaying, Is.False, "Reduced effects survives quest restart");
            AccessibilitySettings.SetReducedEffects(reduced);
        }

        [UnityTest]
        public IEnumerator Main_AbilitiesAndBackdropCoverage()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            Assert.That(Object.FindAnyObjectByType<MainSceneLayout>().tuning, Is.Not.Null);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            var game = Object.FindAnyObjectByType<HeroGame>();
            game.StartQuest();
            game.Hero.UnlockSpark();
            yield return new WaitForSeconds(.3f);
            Assert.That(game.Hero.AirJumpAvailable, Is.True, "Main enables its air jump");
            MobileInput.Set(MobileAction.Swap, true);
            yield return null;
            yield return null;
            Assert.That(game.Hero.Weapon, Is.EqualTo(HeroWeapon.SunseedWand));
            MobileInput.Set(MobileAction.Attack, true);
            float deadline = Time.realtimeSinceStartup + 2f;
            while(game.Hero.SeedsFired == 0 && Time.realtimeSinceStartup < deadline)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }
            Assert.That(game.Hero.SeedsFired, Is.EqualTo(1));
            MobileInput.Reset();
            MobileInput.Set(MobileAction.Dash, true);
            deadline = Time.realtimeSinceStartup + 2f;
            while(!game.Hero.Backflipping && Time.realtimeSinceStartup < deadline)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }
            Assert.That(game.Hero.Backflipping, Is.True, "Neutral evade works in Main");
            MobileInput.Reset();
            var camera = game.MainCamera;
            var heroPosition = game.Hero.transform.position;
            foreach(float aspect in new[] {1f, 16f/9f, 21f/9f})
            foreach(float x in new[] {1f, 50f})
            {
                camera.aspect = aspect;
                game.Hero.transform.position = new Vector3(x, heroPosition.y, 0f);
                camera.GetComponent<CameraFollow>().SnapToHero();
                float viewportX = camera.WorldToViewportPoint(game.Hero.transform.position).x;
                Assert.That(viewportX, Is.InRange(0f, 1f), "Hero visible at route edge, aspect="+aspect);
            }
            game.Hero.transform.position = heroPosition;
            camera.GetComponent<CameraFollow>().enabled = false;
            foreach (float aspect in new[] {1f, 16f/9f, 21f/9f})
            foreach (float x in new[] {4f, 8.25f, 12.5f, 22f, 30.5f, 37.5f, 46f})
            {
                camera.aspect = aspect;
                camera.transform.position = new Vector3(x, 0f, -10f);
                bool covered = false;
                deadline = Time.realtimeSinceStartup + 2f;
                while(!covered && Time.realtimeSinceStartup < deadline)
                {
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
                foreach (var card in Object.FindObjectsByType<ZoneBackdrop>(FindObjectsSortMode.None))
                {
                    var renderer = card.GetComponent<SpriteRenderer>();
                    if(renderer.color.a < .999f) continue;
                    float halfHeight = camera.orthographicSize;
                    var b = renderer.bounds;
                    covered |= b.min.x <= x-halfHeight*aspect && b.max.x >= x+halfHeight*aspect &&
                               b.min.y <= -halfHeight && b.max.y >= halfHeight;
                }
                }
                Assert.That(covered, Is.True, "Opaque background covers aspect="+aspect+" x="+x+" camera="+camera.transform.position);
            }
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            MobileInput.Reset();
            if(EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
    }
}
