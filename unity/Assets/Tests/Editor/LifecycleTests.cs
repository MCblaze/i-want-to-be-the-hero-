using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class LifecycleTests
    {
        [UnitySetUp]
        public IEnumerator OpenPrototype()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            MobileInput.Reset();
            if (EditorApplication.isPlaying)
                yield return new ExitPlayMode();
        }

        private static HeroGame Game => Object.FindAnyObjectByType<HeroGame>();

        private static void MoveHeroTo(Vector2 position)
        {
            Game.Hero.GetComponent<Rigidbody2D>().position = position;
            Game.Hero.transform.position = position;
            Physics2D.SyncTransforms();
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, string message)
        {
            float timeout = Time.realtimeSinceStartup + 2f;
            while (!condition() && Time.realtimeSinceStartup < timeout)
                yield return null;
            Assert.That(condition(), Is.True, message);
            yield return null;
        }

        private static void AssertOneWorld()
        {
            Assert.That(Object.FindObjectsByType<HeroGame>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<HeroController>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Thornling>(FindObjectsSortMode.None), Has.Length.EqualTo(5));
            Assert.That(Object.FindObjectsByType<MossbackGuardian>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TwentyRestarts_RebuildExactlyOneWorldAndClearInputs()
        {
            for (int i = 0; i < 20; i++)
            {
                AssertOneWorld();
                var game = Game;
                int previousId = game.GetInstanceID();
                game.StartQuest();
                game.CollectHeroSpark();
                MobileInput.Set(MobileAction.Right, true);
                MobileInput.Set(MobileAction.Jump, true);
                MobileInput.Set(MobileAction.Attack, true);
                MobileInput.Set(MobileAction.Dash, true);
                if (i % 2 == 0)
                {
                    game.ResetQuest();
                }
                else
                {
                    game.CompleteQuest();
                    Assert.That(game.CurrentObjective(), Is.EqualTo("Sunleaf Ruins restored!"));
                    var replay = game.UI.GetComponentsInChildren<Button>(true)
                        .Single(button => button.name == "Play again");
                    replay.onClick.Invoke();
                }
                yield return WaitFor(() => Game != null && Game.GetInstanceID() != previousId,
                    "Restart must finish loading a new session.");
                AssertOneWorld();
                Assert.That(Game.GetInstanceID(), Is.Not.EqualTo(previousId), "Restart must replace the session.");
                Assert.That(Game.Started, Is.False);
                Assert.That(Game.Won, Is.False);
                Assert.That(Game.Hero.HasSpark, Is.False, "Full restart clears progression.");
                Assert.That(Game.Hero.Health, Is.EqualTo(5));
                Assert.That(MobileInput.Left || MobileInput.Right || MobileInput.JumpHeld, Is.False);
                Assert.That(MobileInput.ConsumeJump() || MobileInput.ConsumeAttack() || MobileInput.ConsumeDash(), Is.False);
            }
        }

        [UnityTest]
        public IEnumerator TenFalls_KeepCheckpointAndSpark_ResetEveryEnemy()
        {
            var game = Game;
            game.StartQuest();
            // Arrange a checkpoint collision using the real trigger, not SetCheckpoint.
            MoveHeroTo(new Vector2(24.55f, -2.75f));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            game.CollectHeroSpark();
            yield return null;

            for (int i = 0; i < 10; i++)
            {
                MoveHeroTo(new Vector2(42f, -2.75f));
                yield return WaitFor(() => game.Boss.Awake, "Boss must awaken before arranging damage.");
                Assert.That(game.Boss.Awake, Is.True);
                game.Boss.TakeHit(4, Vector2.zero);
                Assert.That(game.Boss.Health, Is.EqualTo(6));
                var defeated = Object.FindAnyObjectByType<Thornling>();
                defeated.TakeHit(10, Vector2.zero);
                int oldBossId = game.Boss.GetInstanceID();

                // Arrange a fall below the real kill plane; Update performs the respawn.
                MoveHeroTo(new Vector2(22.9f, -9f));
                yield return WaitFor(() => game.Boss.GetInstanceID() != oldBossId, "Fall must reset the encounter.");
                AssertOneWorld();
                Assert.That(game.Hero.transform.position.x, Is.EqualTo(23.95f).Within(.1f), "Checkpoint must survive.");
                Assert.That(game.Hero.Health, Is.EqualTo(5));
                Assert.That(game.Hero.HasSpark, Is.True);
                Assert.That(game.Boss.GetInstanceID(), Is.Not.EqualTo(oldBossId));
                Assert.That(game.Boss.Health, Is.EqualTo(10));
                Assert.That(game.Boss.Awake, Is.False);
                Assert.That(game.Boss.transform.position.x, Is.EqualTo(45.4f).Within(.1f));
                Assert.That(game.Boss.GetComponent<Collider2D>().enabled, Is.True);
                Assert.That(game.Boss.GetComponent<Rigidbody2D>().simulated, Is.True);
                Assert.That(Object.FindObjectsByType<Thornling>(FindObjectsSortMode.None).All(t => t.IsAlive), Is.True);
                Assert.That(GameObject.Find("Hero Spark Gate"), Is.Null, "Respawn keeps the opened gate.");
                Assert.That(game.MainCamera.transform.position.x, Is.EqualTo(26.15f).Within(.1f));
            }
        }

        [UnityTest]
        public IEnumerator LethalDamage_RespawnsAndClearsTheBossAttempt()
        {
            var game = Game;
            game.StartQuest();
            game.Hero.SetCheckpoint(new Vector2(1.2f, -2.8f));
            game.CollectHeroSpark();
            MoveHeroTo(new Vector2(42f, -2.75f));
            yield return WaitFor(() => game.Boss.Awake, "Boss must awaken before arranging damage.");
            game.Boss.TakeHit(4, Vector2.zero);
            Assert.That(game.Boss.Health, Is.EqualTo(6));
            MoveHeroTo(new Vector2(1.2f, -2.8f));

            // Respect normal damage immunity; exercise the real delayed death callback.
            for (int i = 0; i < 5; i++)
            {
                game.Hero.Hurt(game.Hero.transform.position + Vector3.right);
                game.Hero.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                Assert.That(game.Hero.Health, Is.EqualTo(4 - i), "Damage after each immunity window.");
                if (i < 4)
                {
                    float nextHitTime = Time.time + 1.1f;
                    yield return WaitFor(() => Time.time >= nextHitTime, "Wait for damage immunity to expire.");
                }
            }
            Assert.That(game.Hero.Health, Is.EqualTo(0));
            yield return WaitFor(() => game.Hero.Health == 5, "Delayed combat-death respawn must run.");
            Assert.That(game.Hero.Health, Is.EqualTo(5));
            Assert.That(game.Hero.HasSpark, Is.True);
            Assert.That(game.Hero.transform.position.x, Is.EqualTo(1.2f).Within(.1f));
            Assert.That(game.Boss.Health, Is.EqualTo(10));
            Assert.That(game.Boss.Awake, Is.False);
            AssertOneWorld();
        }
    }
}
