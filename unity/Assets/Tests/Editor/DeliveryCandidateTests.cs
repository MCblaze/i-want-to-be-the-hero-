using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class DeliveryCandidateTests
    {
        private const string Candidate = "Assets/Scenes/Sunleaf_DeliveryCandidate.unity";
        private static HeroGame Game => Object.FindAnyObjectByType<HeroGame>();

        [UnitySetUp]
        public IEnumerator OpenCandidate()
        {
            EditorSceneManager.OpenScene(Candidate);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            yield return null;
            Assert.That(Game, Is.Not.Null, "Authored candidate must bootstrap without Main's scene name.");
            Game.StartQuest();
            MobileInput.Reset();
        }
        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            MobileInput.Reset();
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        private static void MoveHero(Vector2 position)
        {
            var body = Game.Hero.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.position = position;
            Game.Hero.transform.position = position;
            Physics2D.SyncTransforms();
        }

        private static IEnumerator Until(Func<bool> condition, string message, float seconds = 3f)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, message);
        }

        private static bool HasStaticSupport(float x, float y, float distance)
        {
            return Physics2D.RaycastAll(new Vector2(x, y), Vector2.down, distance)
                .Any(hit => hit.collider != null && !hit.collider.isTrigger && hit.collider.attachedRigidbody == null);
        }

        [UnityTest]
        public IEnumerator EveryCheckpoint_RealTriggerThenFall_LandsWithoutRespawnLoop()
        {
            Game.CollectHeroSpark();
            yield return null;
            var checkpoints = Game.MainLayout.checkpoints.OrderBy(entry => entry.progressOrder).ToArray();
            Assert.That(checkpoints.Length, Is.EqualTo(6));
            foreach (var checkpoint in checkpoints)
            {
                var spawn = checkpoint.respawnPosition;
                // Check both sides of the capsule rather than a point near an edge.
                Assert.That(HasStaticSupport(spawn.x - .22f, spawn.y - .5f, .9f), Is.True,
                    checkpoint.marker.name + " left foot lacks safe ground.");
                Assert.That(HasStaticSupport(spawn.x + .22f, spawn.y - .5f, .9f), Is.True,
                    checkpoint.marker.name + " right foot lacks safe ground.");
                var markerRenderer = checkpoint.marker.GetComponent<SpriteRenderer>();
                Assert.That(markerRenderer, Is.Not.Null);
                // A presentation-only sentinel lets the test observe the actual
                // OnTriggerEnter activation, instead of assuming a teleport plus
                // a fixed delay has produced a new physics contact.
                Color awaitingActivation = new Color(.17f, .29f, .43f, 1f);
                markerRenderer.color = awaitingActivation;
                var trigger = checkpoint.marker.GetComponents<Collider2D>().First(c => c.isTrigger);
                Physics2D.SyncTransforms();
                // Approach from the cleared/right side: approaching checkpoint2
                // from x92.5 would itself begin inside the pit being regressed.
                MoveHero(new Vector2(trigger.bounds.max.x + .65f, -2.75f));
                yield return new WaitForFixedUpdate();
                MobileInput.Set(MobileAction.Left, true);
                yield return Until(() => markerRenderer.color != awaitingActivation,
                    checkpoint.marker.name + " must activate from a genuine crossing of its trigger.");
                MobileInput.Reset();
                Game.Hero.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                int oldBoss = Game.Boss.GetInstanceID();
                MoveHero(new Vector2(spawn.x, -9f));
                yield return Until(() => Game.Boss.GetInstanceID() != oldBoss, "Kill plane must trigger a real respawn.");
                int resetBoss = Game.Boss.GetInstanceID();
                yield return new WaitForSeconds(1.2f);
                Assert.That(Game.Hero.transform.position.x, Is.EqualTo(spawn.x).Within(.2f), checkpoint.marker.name);
                Assert.That(Game.Hero.transform.position.y, Is.GreaterThan(-4f), "Respawn must land on the safe upper route.");
                Assert.That(Mathf.Abs(Game.Hero.GetComponent<Rigidbody2D>().linearVelocity.y), Is.LessThan(.1f), "Respawn should settle vertically on its supporting floor.");
                Assert.That(Game.Boss.GetInstanceID(), Is.EqualTo(resetBoss), "Repeated encounter reconstruction reveals a fall/respawn loop.");
                Assert.That(Game.Hero.HasSpark, Is.True);
            }
            // Returning to the first flag must not discard later progress.
            MoveHero(new Vector2(checkpoints[0].marker.transform.position.x, -2.75f));
            yield return new WaitForSeconds(.15f);
            MoveHero(new Vector2(55f, -9f));
            yield return Until(() => Game.Hero.transform.position.y > -4f, "Return from fall.");
            Assert.That(Game.Hero.transform.position.x, Is.EqualTo(checkpoints.Last().respawnPosition.x).Within(.2f));
        }

        [UnityTest]
        public IEnumerator FourMandatoryPits_AreOpenAndFallsReturnToInitialSpawn()
        {
            foreach (float center in new[] { 55f, 66f, 81f, 92f })
            {
                Assert.That(HasStaticSupport(center, -3.41f, 4.5f), Is.False,
                    "An invisible floor or uncropped source collider bridges pit " + center);
                int previousBoss = Game.Boss.GetInstanceID();
                MoveHero(new Vector2(center, -2.8f));
                yield return Until(() => Game.Boss.GetInstanceID() != previousBoss, "Standing in pit must cause a fall and recover.");
                yield return new WaitForSeconds(.3f);
                Assert.That(Game.Hero.transform.position.x, Is.EqualTo(2f).Within(.2f));
                Assert.That(Game.Hero.transform.position.y, Is.GreaterThan(-4f));
            }
        }

        [UnityTest]
        public IEnumerator CandidateBootstrapsRemoteEncounterAndCamera_RestartKeepsCandidate()
        {
            Assert.That(Object.FindObjectsByType<HeroGame>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<HeroController>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Thornling>(FindObjectsSortMode.None), Has.Length.EqualTo(14));
            Assert.That(Game.Boss.transform.position.x, Is.GreaterThan(270f), "Boss must not use Main's old x45 spawn.");
            MoveHero(new Vector2(270f, -2.8f));
            yield return null;
            Assert.That(Game.Boss.Awake, Is.False, "Spark remains required.");
            Game.CollectHeroSpark();
            yield return Until(() => Game.Boss.Awake, "Boss must activate in the remote candidate court.");
            Game.MainCamera.GetComponent<CameraFollow>().SnapToHero();
            Assert.That(Game.MainCamera.transform.position.x, Is.GreaterThan(250f));
            float viewportX = Game.MainCamera.WorldToViewportPoint(Game.Hero.transform.position).x;
            Assert.That(viewportX, Is.InRange(.05f, .95f), "Player must remain visible in the expanded court.");
            int oldGame = Game.GetInstanceID();
            Game.ResetQuest();
            yield return Until(() => Game != null && Game.GetInstanceID() != oldGame, "Candidate restart must rebuild runtime actors.");
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(Candidate));
            Assert.That(Game.Hero.HasSpark, Is.False);
            Assert.That(Game.Started, Is.False);
            Assert.That(Game.Hero.transform.position.x, Is.EqualTo(2f).Within(.2f));
            Assert.That(Object.FindObjectsByType<HeroGame>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }
    }
}



