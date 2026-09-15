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
