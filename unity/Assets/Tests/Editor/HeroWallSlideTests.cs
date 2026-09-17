using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class HeroWallSlideTests
    {
        private bool previousBackground;
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            MobileInput.Reset();
            Application.runInBackground = previousBackground;
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator PressingIntoVerticalWall_WithoutFootSupport_DoesNotSuspendHero()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var game = Object.FindAnyObjectByType<HeroGame>();
            Assert.That(game, Is.Not.Null);
            game.StartQuest();
            MobileInput.Reset();
            var body = game.Hero.GetComponent<Rigidbody2D>();
            // An isolated test location keeps authored floors/enemies out of the
            // contact. The wall top is far above the capsule: this cannot be a
            // corner landing or a diagonal support normal masquerading as ground.
            var wall = new GameObject("Wall-slide regression fixture");
            wall.transform.position = new Vector3(-20f, 1f, 0f);
            var wallCollider = wall.AddComponent<BoxCollider2D>();
            wallCollider.size = new Vector2(1f, 20f);
            body.position = new Vector2(-20.80f, 2f);
            game.Hero.transform.position = body.position;
            body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            MobileInput.Set(MobileAction.Right, true);
            bool touchedVerticalFace = false;
            var contacts = new ContactPoint2D[12];
            float startY = body.position.y;
            float until = Time.time + .8f;
            while (Time.time < until)
            {
                yield return new WaitForFixedUpdate();
                int count = body.GetContacts(contacts);
                for (int i = 0; i < count; i++)
                {
                    var contact = contacts[i];
                    if (contact.collider != wallCollider && contact.otherCollider != wallCollider) continue;
                    Assert.That(Mathf.Abs(contact.normal.y), Is.LessThan(.05f), "Fixture must provide only a vertical face, never upward support.");
                    touchedVerticalFace = true;
                }
            }
            MobileInput.Reset();
            Assert.That(touchedVerticalFace, Is.True, "The hero must genuinely press against the wall.");
            Assert.That(body.position.y, Is.LessThan(startY - 5.5f), "Horizontal motor pressure must not create a perch or substantial friction braking (unobstructed fall is about 7 units here).");
            Assert.That(body.linearVelocity.y, Is.LessThan(-12f), "Gravity must continue accelerating the unsupported hero toward its normal fall-speed cap.");
            Object.Destroy(wall);
            yield return new ExitPlayMode();
        }
    }
}

