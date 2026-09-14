using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class MovementTests
    {
        private const string ScenePath = "Assets/HeroDemo/Movement/MovementAndFeel_Test.unity";
        private HeroController Hero => Object.FindAnyObjectByType<HeroController>();
        private Rigidbody2D Body => Hero.GetComponent<Rigidbody2D>();
        private MovementLab Lab => Object.FindAnyObjectByType<MovementLab>();

        [UnitySetUp]
        public IEnumerator OpenLab()
        {
            EditorApplication.isPaused = false;
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            yield return Until(() => Hero != null && Hero.Game.Started && Hero.Grounded, "Lab starts grounded");
        }

        [UnityTearDown]
        public IEnumerator CloseLab()
        {
            MobileInput.Reset();
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        private static IEnumerator Until(Func<bool> condition, string reason, float timeout = 5f)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!condition() && Time.realtimeSinceStartup < end)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }
            Assert.That(condition(), Is.True, reason);
        }

        private static IEnumerator For(float seconds)
        {
            float end = Time.time + seconds;
            yield return Until(() => Time.time >= end, "Simulation must advance", seconds + 5f);
        }

        private void Place(Vector2 position)
        {
            Hero.ResetTraining(position);
            Body.position = position;
            Hero.transform.position = position;
            Physics2D.SyncTransforms();
        }

        private static void Press(MobileAction action) => MobileInput.Set(action, true);
        private static void Release(MobileAction action) => MobileInput.Set(action, false);

        [UnityTest]
        public IEnumerator JumpMeasurements_OneAirJumpAndShortHop()
        {
            float[] rise = new float[3];
            float[] flight = new float[3];
            for (int mode = 0; mode < 3; mode++)
            {
                Place(new Vector2(5f, .7f));
                yield return Until(() => Hero.Grounded, "Reset lands on runway");
                float startY = Body.position.y;
                float startTime = Time.time;
                Press(MobileAction.Jump);
                yield return Until(() => Body.linearVelocity.y > 5f, "First jump starts");
                if (mode == 1) Release(MobileAction.Jump);
                float maxY = Body.position.y;
                bool second = false;
                while (!Hero.Grounded && Time.time - startTime < 3f)
                {
                    maxY = Mathf.Max(maxY, Body.position.y);
                    if (mode == 2 && !second && Body.linearVelocity.y < 1f)
                    {
                        Release(MobileAction.Jump);
                        Press(MobileAction.Jump);
                        yield return Until(() => !Hero.AirJumpAvailable, "Second jump spends its budget");
                        Assert.That(Body.linearVelocity.y, Is.GreaterThan(5f));
                        second = true;
                        yield return For(.08f);
                        float previous = Body.linearVelocity.y;
                        Release(MobileAction.Jump);
                        Press(MobileAction.Jump);
                        yield return For(.08f);
                        Assert.That(Body.linearVelocity.y, Is.LessThan(previous), "Third press cannot add height");
                    }
                    yield return null;
                }
                Assert.That(Hero.Grounded, Is.True, "Jump returns to runway");
                Assert.That(Hero.AirJumpAvailable, Is.True, "Landing restores one air jump");
                rise[mode] = maxY - startY;
                flight[mode] = Time.time - startTime;
                Release(MobileAction.Jump);
            }
            Assert.That(rise[0], Is.InRange(2f, 2.6f));
            Assert.That(rise[1], Is.LessThan(rise[0] * .65f));
            Assert.That(rise[2], Is.GreaterThan(rise[0] + .8f));
            Place(new Vector2(3f, .7f));
            yield return Until(() => Hero.Grounded, "Ready for speed test");
            Press(MobileAction.Right);
            yield return For(.3f);
            float startX = Body.position.x;
            float runStart = Time.time;
            yield return For(.5f);
            float speed = (Body.position.x - startX) / (Time.time - runStart);
            Assert.That(speed, Is.EqualTo(5.2f).Within(.35f));
            Release(MobileAction.Right);
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/movement-measurements.csv", "case,rise_units,flight_seconds\n" +
                $"full,{rise[0]:F3},{flight[0]:F3}\nshort,{rise[1]:F3},{flight[1]:F3}\ndouble,{rise[2]:F3},{flight[2]:F3}\nrun_speed,{speed:F3},\n");
        }

        [UnityTest]
        public IEnumerator Evade_DirectionAndSharedCooldownPreserveAirBudget()
        {
            Place(new Vector2(7f, .7f));
            yield return Until(() => Hero.Grounded, "Ready for backflip");
            float startX = Body.position.x;
            float facing = Hero.Facing;
            Press(MobileAction.Dash);
            yield return Until(() => Hero.Backflipping, "Neutral evade begins backflip");
            Release(MobileAction.Dash);
            yield return For(.12f);
            Assert.That((Body.position.x - startX) * facing, Is.LessThan(-.3f));
            Assert.That(Hero.Facing, Is.EqualTo(facing));
            Assert.That(Hero.transform.rotation, Is.EqualTo(Quaternion.identity), "Physics root never rotates");
            Press(MobileAction.Right);
            Press(MobileAction.Dash);
            yield return For(.08f);
            Assert.That(Hero.Dashing, Is.False, "Backflip and dash share cooldown");
            Release(MobileAction.Dash);
            Release(MobileAction.Right);
            yield return Until(() => Hero.EvadeCooldown == 0f && Hero.Grounded, "Evade recovers");
            Press(MobileAction.Jump);
            yield return Until(() => Body.linearVelocity.y > 5f, "Jump launches");
            yield return For(.2f);
            Release(MobileAction.Jump);
            Press(MobileAction.Jump);
            yield return Until(() => !Hero.AirJumpAvailable, "Spend air jump");
            Press(MobileAction.Right);
            Press(MobileAction.Dash);
            yield return Until(() => Hero.Dashing, "Air evade dashes");
            yield return For(.06f);
            Assert.That(Body.linearVelocity.x, Is.GreaterThan(11f));
            Assert.That(Hero.AirJumpAvailable, Is.False, "Dash cannot refill air jump");
        }

        [UnityTest]
        public IEnumerator Backflip_DamageWindowHasVulnerableRecovery()
        {
            Place(new Vector2(7f, .7f));
            yield return For(1.3f);
            Press(MobileAction.Dash);
            yield return Until(() => Hero.Backflipping, "Backflip begins");
            Release(MobileAction.Dash);
            yield return For(.1f);
            Hero.Hurt(Hero.transform.position + Vector3.right);
            Assert.That(Hero.Health, Is.EqualTo(5), "Middle of backflip evades damage");
            yield return For(.16f);
            Hero.Hurt(Hero.transform.position + Vector3.right);
            Assert.That(Hero.Health, Is.EqualTo(4), "Recovery is vulnerable");
        }

        [UnityTest]
        public IEnumerator Weapons_SwapPreservesCooldownAndProjectileHits()
        {
            Place(new Vector2(58f, .7f));
            yield return Until(() => Hero.Grounded, "Combat setup");
            var target = Object.FindAnyObjectByType<TrainingTarget>();
            Press(MobileAction.Swap);
            yield return Until(() => Hero.Weapon == HeroWeapon.SunseedWand, "Select wand");
            Press(MobileAction.Attack);
            yield return Until(() => Hero.SeedsFired == 1, "Fire one seed");
            Press(MobileAction.Swap);
            yield return Until(() => Hero.Weapon == HeroWeapon.Sword, "Swap to sword");
            Press(MobileAction.Swap);
            yield return Until(() => Hero.Weapon == HeroWeapon.SunseedWand, "Swap back");
            Press(MobileAction.Attack);
            yield return For(.12f);
            Assert.That(Hero.SeedsFired, Is.EqualTo(1), "Swapping cannot reset wand cooldown");
            yield return Until(() => target.HitCount == 1, "In-flight seed survives swap and hits target");
            Assert.That(target.DamageTaken, Is.EqualTo(1));

            Place(new Vector2(65f, .7f));
            target.transform.position = new Vector2(69f, .8f);
            Physics2D.SyncTransforms();
            yield return Until(() => Hero.Grounded, "Wall test ready");
            Press(MobileAction.Swap);
            yield return Until(() => Hero.Weapon == HeroWeapon.SunseedWand, "Select wand again");
            int previous = Hero.SeedsFired;
            Press(MobileAction.Attack);
            yield return Until(() => Hero.SeedsFired > previous, "Fire toward wall");
            yield return Until(() => Object.FindObjectsByType<SunseedProjectile>(FindObjectsSortMode.None).Length == 0, "Wall stops projectile");
            Assert.That(target.HitCount, Is.EqualTo(0), "Thin wall shields target");
        }

        [UnityTest]
        public IEnumerator Sword_OneHitPerSwingAndSwapCannotConvertAttack()
        {
            Place(new Vector2(61f, .7f));
            yield return Until(() => Hero.Grounded, "Melee setup");
            var target = Object.FindAnyObjectByType<TrainingTarget>();
            Press(MobileAction.Attack);
            yield return Until(() => target.HitCount == 1, "Sword makes contact");
            Press(MobileAction.Swap);
            yield return For(.22f);
            Assert.That(target.HitCount, Is.EqualTo(1), "A swing can hit only once");
            Assert.That(Hero.SeedsFired, Is.EqualTo(0), "Swap cannot turn swing into shot");
            Assert.That(target.DamageTaken, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Platforms_OneWayLandingMovingCarryAndLowCeiling()
        {
            Place(new Vector2(38f, .7f));
            yield return Until(() => Hero.Grounded, "Under one-way platform");
            Press(MobileAction.Jump);
            yield return Until(() => Body.position.y > 2.7f, "Jump passes through platform from below");
            yield return Until(() => Hero.Grounded, "Land on top of one-way platform");
            Assert.That(Body.position.y, Is.GreaterThan(2.6f));
            Release(MobileAction.Jump);

            var platform = Object.FindAnyObjectByType<LabMovingPlatform>().GetComponent<Rigidbody2D>();
            Place(platform.position + Vector2.up * .85f);
            yield return Until(() => Hero.Grounded, "Land on moving platform");
            float offset = Body.position.x - platform.position.x;
            yield return For(1.1f);
            Assert.That(Hero.Grounded, Is.True, "Stay grounded on moving platform");
            Assert.That(Body.position.x - platform.position.x, Is.EqualTo(offset).Within(.25f), "Platform carries idle hero");

            Place(new Vector2(54f, .7f));
            yield return Until(() => Hero.Grounded, "Under low ceiling");
            Press(MobileAction.Jump);
            yield return Until(() => Body.linearVelocity.y > 5f, "Ceiling jump starts");
            yield return For(.25f);
            Assert.That(Body.position.y, Is.LessThan(1.95f), "Head collision stops rising");
            Assert.That(Hero.AirJumpAvailable, Is.True, "Ceiling contact is not a landing");
        }

        [UnityTest]
        public IEnumerator Traversal_GapsCoyoteBufferAndCheckpoint()
        {
            float[] edges = { 13f, 21f, 29.75f };
            float[] gaps = { 2f, 2.75f, 3.5f };
            for (int i = 0; i < gaps.Length; i++)
            {
                Place(new Vector2(edges[i] - 2f, .7f));
                yield return Until(() => Hero.Grounded, "Gap approach ready");
                Press(MobileAction.Right);
                yield return Until(() => Body.position.x > edges[i] - .5f, "Run to takeoff marker");
                Press(MobileAction.Jump);
                yield return Until(() => Body.linearVelocity.y > 8f, "Ground jump across gap");
                yield return Until(() => Body.position.x > edges[i] + gaps[i] + .5f, "Cross gap " + gaps[i]);
                Release(MobileAction.Right);
                yield return Until(() => Hero.Grounded, "Land beyond gap");
                Assert.That(Body.position.y, Is.GreaterThan(.5f), "Main route landing, not recovery shelf");
                Assert.That(Hero.AirJumpAvailable, Is.True, "Single jump traverses teaching gaps");
                Release(MobileAction.Jump);
            }

            Place(new Vector2(11f, .7f));
            yield return Until(() => Hero.Grounded, "Coyote approach ready");
            Press(MobileAction.Right);
            yield return Until(() => Body.position.x > 13.32f, "Leave the actual ledge");
            Press(MobileAction.Jump);
            yield return Until(() => Body.linearVelocity.y > 5f, "Late ledge jump accepted");
            Assert.That(Body.linearVelocity.y, Is.GreaterThan(8.7f), "Coyote uses first-jump impulse");
            Assert.That(Hero.AirJumpAvailable, Is.True, "Coyote does not spend air jump");

            Place(new Vector2(5f, .7f));
            yield return Until(() => Hero.Grounded, "Buffer setup grounded");
            Press(MobileAction.Jump);
            yield return Until(() => Body.linearVelocity.y > 5f, "Buffer setup jump");
            yield return For(.15f);
            Release(MobileAction.Jump);
            Press(MobileAction.Jump);
            yield return Until(() => !Hero.AirJumpAvailable, "Spend air jump before buffering landing");
            yield return Until(() => Body.position.y < 1.25f && Body.linearVelocity.y < -1f, "Approach landing");
            Release(MobileAction.Jump);
            Press(MobileAction.Jump);
            yield return Until(() => Body.linearVelocity.y > 5f, "Buffered press launches after landing");
            Assert.That(Hero.AirJumpAvailable, Is.True, "Landing buffer preserves new air jump");

            Place(new Vector2(34f, .7f));
            var flag = Object.FindAnyObjectByType<LabCheckpoint>().GetComponent<SpriteRenderer>();
            Color before = flag.color;
            Press(MobileAction.Right);
            yield return Until(() => flag.color != before, "Real lab checkpoint trigger activates");
            Release(MobileAction.Right);
            Body.position = new Vector2(35f, -9f);
            Hero.transform.position = Body.position;
            Physics2D.SyncTransforms();
            yield return Until(() => Body.position.y > 0f, "Fall restores checkpoint");
            Assert.That(Body.position.x, Is.EqualTo(34.5f).Within(.15f));
            Assert.That(Hero.HasSpark, Is.True);
        }

        [UnityTest]
        public IEnumerator Camera_AspectChangesKeepHeroVisibleAtBothRoomEdges()
        {
            Camera camera = Hero.Game.MainCamera;
            foreach (float aspect in new[] { 1f, 16f / 10f, 16f / 9f })
            {
                camera.aspect = aspect;
                foreach (float x in new[] { 1.2f, 68.8f })
                {
                    Place(new Vector2(x, .7f));
                    camera.GetComponent<CameraFollow>().SnapToHero();
                    Vector3 viewport = camera.WorldToViewportPoint(Body.position);
                    Assert.That(viewport.x, Is.InRange(.05f, .95f), "Room edge visible at aspect " + aspect);
                    Assert.That(viewport.y, Is.InRange(.1f, .9f));
                    yield return null;
                }
            }
            camera.ResetAspect();
        }

        [UnityTest]
        public IEnumerator LabReset_ClearsProjectilesInputsAndPreservesSingleSession()
        {
            int gameId = Hero.Game.GetInstanceID();
            for (int i = 0; i < 3; i++)
            {
                Press(MobileAction.Swap);
                yield return Until(() => Hero.Weapon == HeroWeapon.SunseedWand, "Select wand for reset");
                Press(MobileAction.Attack);
                yield return Until(() => Object.FindObjectsByType<SunseedProjectile>(FindObjectsSortMode.None).Length > 0, "Seed exists before reset");
                Press(MobileAction.Right);
                Press(MobileAction.Jump);
                Lab.Restart();
                yield return Until(() => Object.FindObjectsByType<SunseedProjectile>(FindObjectsSortMode.None).Length == 0,
                    "Reset completes deferred projectile destruction");
                Assert.That(Object.FindObjectsByType<SunseedProjectile>(FindObjectsSortMode.None), Is.Empty);
                Assert.That(Object.FindObjectsByType<HeroController>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Hero.Game.GetInstanceID(), Is.EqualTo(gameId));
                Assert.That(Hero.HasSpark && Hero.AirJumpAvailable, Is.True);
                Assert.That(Hero.Weapon, Is.EqualTo(HeroWeapon.Sword));
                Assert.That(MobileInput.Right || MobileInput.JumpHeld || MobileInput.ConsumeJump() || MobileInput.ConsumeSwap(), Is.False);
            }
        }
    }
}
