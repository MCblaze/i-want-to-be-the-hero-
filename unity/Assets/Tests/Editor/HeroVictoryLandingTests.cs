using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class HeroVictoryLandingTests
    {
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            MobileInput.Reset();
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator AirborneVictory_CancelsUpwardMotionAndLandsWithoutPlayerMovement()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            var game = Object.FindAnyObjectByType<HeroGame>();
            game.StartQuest();
            var body = game.Hero.GetComponent<Rigidbody2D>();
            var capsule = game.Hero.GetComponent<CapsuleCollider2D>();
            body.position = new Vector2(48f, 1f);
            game.Hero.transform.position = body.position;
            body.linearVelocity = new Vector2(5f, 8f);
            Physics2D.SyncTransforms();
            var floor = Physics2D.RaycastAll(body.position, Vector2.down, 8f)
                .First(hit => !hit.collider.isTrigger && hit.collider.attachedRigidbody == null);
            // Arrange only the completion event; physics must perform the landing.
            game.CompleteQuest();
            MobileInput.Set(MobileAction.Right, true);
            MobileInput.Set(MobileAction.Jump, true);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.y, Is.LessThanOrEqualTo(.01f), "Victory stops further upward travel.");
            for (int step = 0; step < 80; step++) yield return new WaitForFixedUpdate();
            Assert.That(game.Won, Is.True);
            Assert.That(body.position.x, Is.EqualTo(48f).Within(.05f), "Held movement must not move the celebrating hero.");
            Assert.That(capsule.bounds.min.y, Is.EqualTo(floor.point.y).Within(.08f), "Victory must settle on the actual supporting surface instead of hovering.");
            Assert.That(Mathf.Abs(body.linearVelocity.y), Is.LessThan(.1f));
            MobileInput.Reset();
            yield return new ExitPlayMode();
        }
    }
}
