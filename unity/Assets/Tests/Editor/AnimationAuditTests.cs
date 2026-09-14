using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    // Screenshots come from Unity cameras and the actual sprite presenter, not mockups.
    public sealed class AnimationAuditTests
    {
        private static string Output => Path.GetFullPath("Logs/animation-audit/" + SessionState.GetString("HeroAnimationAuditPhase", "current"));
        private readonly StringBuilder trace = new();
        private HeroController Hero => Object.FindAnyObjectByType<HeroController>();
        private PixelSpriteAnimator Visual => Hero.GetComponentInChildren<PixelSpriteAnimator>();
        private Rigidbody2D Body => Hero.GetComponent<Rigidbody2D>();

        [Test]
        public void EveryAuthoredFrame_BothFacings_CameraContactSheets()
        {
            Directory.CreateDirectory(Output);
            var manifest = JsonUtility.FromJson<PixelManifest>(Resources.Load<TextAsset>("Art/Animations/manifest").text);
            foreach (var definition in manifest.characters)
            {
                var scene = EditorSceneManager.NewPreviewScene();
                try
                {
                    float cw = Mathf.Max(3f, definition.unityHeight * 1.7f), ch = definition.unityHeight + 1.1f;
                    int count = definition.columns * definition.rows;
                    int rows = Mathf.CeilToInt(count / 4f);
                    var cameraObject = InScene("Audit camera", scene);
                    var camera = cameraObject.AddComponent<Camera>();
                    camera.scene = scene;
                    camera.orthographic = true;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(.035f, .065f, .08f);
                    camera.transform.position = new Vector3(cw * 3.5f, -ch * (rows - 1) / 2f + definition.unityHeight / 2f, -10f);
                    camera.orthographicSize = (rows * ch + .6f) / 2f;
                    for (int i = 0; i < count; i++)
                    {
                        var clip = definition.clips.First(c => c.frames.Contains(i));
                        int slot = Array.IndexOf(clip.frames, i);
                        for (int face = 0; face < 2; face++)
                        {
                            var owner = InScene(definition.id + " " + i + " " + face, scene);
                            owner.transform.position = new Vector3((i % 4 + face * 4) * cw, -(i / 4) * ch, 0f);
                            var animator = PixelSpriteAnimator.Attach(owner, definition.id);
                            animator.Sample(clip.name, (slot + .01f) / clip.fps);
                            animator.Face(face == 0 ? 1 : -1);
                            Assert.That(animator.Renderer.sprite.name, Is.EqualTo(definition.id + "_" + i.ToString("00")));
                            Assert.That(animator.Renderer.sprite.rect.width, Is.GreaterThan(0));
                            Label(scene, owner.transform.position + new Vector3(0, -.28f, -.2f), clip.name + " #" + i + (face == 0 ? " R" : " L"), .14f);
                            var line = InScene("Foot baseline", scene);
                            line.transform.position = owner.transform.position + new Vector3(0, 0, .1f);
                            var lr = line.AddComponent<LineRenderer>();
                            lr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
                            lr.useWorldSpace = false; lr.positionCount = 2; lr.startWidth = lr.endWidth = .013f;
                            lr.startColor = lr.endColor = new Color(.3f, .6f, .5f);
                            lr.SetPosition(0, new Vector3(-cw * .44f, 0)); lr.SetPosition(1, new Vector3(cw * .44f, 0));
                        }
                    }
                    int height = Mathf.RoundToInt(1920f * (rows * ch + .6f) / (cw * 8));
                    Render(camera, Path.Combine(Output, definition.id + "-all-frames.png"), 1920, height);
                }
                finally { EditorSceneManager.ClosePreviewScene(scene); }
            }
        }

        [UnityTest]
        public IEnumerator LiveController_AllLoganActions_CaptureAndTrace()
        {
            EditorSceneManager.OpenScene("Assets/HeroDemo/Movement/MovementAndFeel_Test.unity");
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            Directory.CreateDirectory(Output);
            trace.AppendLine("action,time,clip,frame,facing,rotation,visualMinY,colliderMinY,weapon");
            yield return Until(() => Hero != null && Hero.Grounded, "lab grounded");
            Hero.ResetTraining(new Vector2(6, .7f));
            yield return For(1.3f);
            Capture("idle-right");
            MobileInput.Set(MobileAction.Right, true); yield return For(.23f); Capture("run-right");
            MobileInput.Set(MobileAction.Right, false);
            MobileInput.Set(MobileAction.Left, true); yield return For(.3f); Capture("run-left");
            MobileInput.Reset(); yield return For(.2f); Capture("idle-left");
            MobileInput.Set(MobileAction.Jump, true); yield return Until(() => Body.linearVelocity.y > 5, "jump starts"); Capture("jump-rise");
            yield return Until(() => Mathf.Abs(Body.linearVelocity.y) < 1, "jump apex"); Capture("jump-apex");
            MobileInput.Set(MobileAction.Jump, false); MobileInput.Set(MobileAction.Jump, true);
            yield return Until(() => !Hero.AirJumpAvailable, "double jump"); Capture("double-jump");
            MobileInput.Set(MobileAction.Jump, false);
            yield return Until(() => Body.linearVelocity.y < -2, "fall"); Capture("jump-fall");
            yield return Until(() => Hero.Grounded, "land"); Capture("landing"); yield return For(.04f); Capture("landing-settled"); yield return For(.21f);
            MobileInput.Set(MobileAction.Attack, true); yield return null; MobileInput.Set(MobileAction.Attack, false);
            for (int i = 0; i < 4; i++) { yield return For(.065f); Capture("sword-" + i); }
            yield return For(.2f);
            MobileInput.Set(MobileAction.Swap, true); yield return Until(() => Hero.Weapon == HeroWeapon.SunseedWand, "swap accepted"); MobileInput.Set(MobileAction.Swap, false); Capture("wand-equipped");
            MobileInput.Set(MobileAction.Attack, true); yield return null; MobileInput.Set(MobileAction.Attack, false);
            for (int i = 0; i < 3; i++) { yield return For(.05f); Capture("wand-fire-" + i); }
            yield return For(.3f);
            Hero.ResetTraining(new Vector2(7, .7f)); yield return For(1.3f);
            MobileInput.Set(MobileAction.Dash, true); yield return Until(() => Hero.Backflipping, "backflip"); MobileInput.Set(MobileAction.Dash, false);
            Capture("backflip-start");
            for (int i = 0; i < 4; i++) { yield return For(.065f); Capture("backflip-" + i); }
            yield return For(.5f); Capture("backflip-recovery");
            MobileInput.Set(MobileAction.Right, true); MobileInput.Set(MobileAction.Dash, true);
            yield return Until(() => Hero.Dashing, "dash"); MobileInput.Set(MobileAction.Dash, false);
            Capture("dash-start"); yield return For(.045f); Capture("dash-mid"); yield return For(.045f); Capture("dash-end");
            MobileInput.Reset(); yield return For(.3f);
            Hero.Hurt(Hero.transform.position + Vector3.right); yield return Until(() => Visual.CurrentClip == "hurt", "hurt displayed"); Capture("hurt-start"); yield return For(.12f); Capture("hurt-end");
            yield return For(.3f); Hero.ResetTraining(new Vector2(6, .7f)); yield return For(1.3f); Capture("respawn");
            Hero.Game.CompleteQuest(); yield return For(.12f); Capture("victory-a"); yield return For(.26f); Capture("victory-b");
            File.WriteAllText(Path.Combine(Output, "logan-live-trace.csv"), trace.ToString());
            MobileInput.Reset();
            yield return new ExitPlayMode();
        }

        [Test]
        public void PoseRotation_KeepsHorizontalCentreAndFootClearance_BothFacings()
        {
            var owner = new GameObject("Rotation regression");
            try
            {
                var col = owner.AddComponent<BoxCollider2D>(); col.size = new Vector2(.6f, 1.4f);
                var animator = PixelSpriteAnimator.Attach(owner, "logan"); animator.Sample("apex", 0);
                var resting = animator.transform.localPosition;
                foreach (float facing in new[] { -1f, 1f })
                {
                    animator.Face(facing); animator.SetPoseRotation(0);
                    Vector3 centre = animator.Renderer.bounds.center;
                    float footPlane = animator.Renderer.bounds.min.y;
                    foreach (float angle in new[] { 45f, 90f, 180f, 270f, 315f })
                    {
                        animator.SetPoseRotation(angle * facing);
                        Assert.That(animator.Renderer.bounds.center.x, Is.EqualTo(centre.x).Within(.0001f));
                        Assert.That(animator.Renderer.bounds.min.y, Is.GreaterThanOrEqualTo(footPlane - .0001f));
                        Assert.That(owner.transform.rotation, Is.EqualTo(Quaternion.identity));
                        Assert.That(col.size, Is.EqualTo(new Vector2(.6f, 1.4f)));
                    }
                    animator.SetPoseRotation(0);
                    Assert.That(Vector3.Distance(animator.transform.localPosition, resting), Is.LessThan(.0001f));
                }
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void DestroyedCachedSprite_IsRecreatedOnNextAttach()
        {
            var first = new GameObject("Cache lifetime first");
            var second = new GameObject("Cache lifetime second");
            try
            {
                var old = PixelSpriteAnimator.Attach(first, "logan");
                Object.DestroyImmediate(old.Renderer.sprite);
                var fresh = PixelSpriteAnimator.Attach(second, "logan");
                Assert.That(fresh.Renderer.sprite != null, Is.True, "Destroyed cached Unity object must be recreated");
                Assert.That(fresh.Renderer.sprite.name, Is.EqualTo("logan_00"));
                fresh.Sample("victory", .3f);
                Assert.That(fresh.Renderer.sprite.name, Is.EqualTo("logan_23"));
            }
            finally { Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        }

        [UnityTest]
        public IEnumerator WandAndBackflip_PresentationRegressions()
        {
            EditorSceneManager.OpenScene("Assets/HeroDemo/Movement/MovementAndFeel_Test.unity");
            yield return new EnterPlayMode();
            yield return Until(() => Hero != null && Hero.Grounded, "lab ready");
            Hero.ResetTraining(new Vector2(7, .7f)); yield return For(1.3f);
            foreach (var direction in new[] { MobileAction.Left, MobileAction.Right })
            {
                MobileInput.Set(direction, true); yield return For(.14f); MobileInput.Reset(); yield return For(.2f);
                var facing = Hero.Facing;
                MobileInput.Set(MobileAction.Dash, true); yield return Until(() => Hero.Backflipping, "flip starts"); MobileInput.Reset();
                float end = Time.time + .42f;
                while (Time.time < end)
                {
                    Assert.That(Visual.Renderer.bounds.min.y, Is.GreaterThan(-.12f), "Backflip must not swing the sprite through the runway");
                    Assert.That(Hero.Facing, Is.EqualTo(facing));
                    yield return null;
                }
                Assert.That(Visual.transform.localRotation, Is.EqualTo(Quaternion.identity));
                yield return For(.4f);
            }
            MobileInput.Set(MobileAction.Swap, true); yield return Until(() => Hero.Weapon == HeroWeapon.SunseedWand, "wand equipped"); MobileInput.Reset();
            MobileInput.Set(MobileAction.Attack, true); yield return Until(() => Hero.SeedsFired > 0, "wand fired"); MobileInput.Reset(); yield return null;
            Assert.That(Visual.CurrentClip, Is.EqualTo("rise"), "Interim casting pose contains no baked sword slash");
            MobileInput.Set(MobileAction.Swap, true); yield return Until(() => Hero.Weapon == HeroWeapon.Sword, "switch during recovery"); MobileInput.Reset();
            Assert.That(Visual.CurrentClip, Is.EqualTo("rise"), "Outgoing weapon owns presentation through recovery");
            Assert.That(Hero.WandCooldown, Is.GreaterThan(0));
            yield return For(.3f);
            var lab = Object.FindAnyObjectByType<MovementLab>();
            var tuning = Object.Instantiate(lab.tuning); lab.tuning = tuning; tuning.dashDuration = .34f;
            MobileInput.Set(MobileAction.Right, true); MobileInput.Set(MobileAction.Dash, true);
            yield return Until(() => Hero.Dashing, "longer tuned dash"); MobileInput.Set(MobileAction.Dash, false);
            yield return For(.1f);
            Assert.That(Visual.Renderer.sprite.name, Is.EqualTo("logan_17"), "Dash clip scales with configured action duration");
            MobileInput.Reset();
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator LiveEnemies_AllExistingClips_CaptureTransitions()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            yield return null;
            // Create closures over live enemies only after EnterPlayMode's domain reload.
            yield return CaptureEnemyTransitions();
            yield return new ExitPlayMode();
        }

        private IEnumerator CaptureEnemyTransitions()
        {
            yield return Until(() => Hero != null, "Main spawned");
            Directory.CreateDirectory(Output);
            var enemy = Object.FindObjectsByType<Thornling>(FindObjectsSortMode.None).OrderBy(e => e.transform.position.x).First();
            var enemyVisual = enemy.GetComponentInChildren<PixelSpriteAnimator>();
            CaptureActor("thornling-idle", enemyVisual);
            Hero.Game.StartQuest(); yield return For(.15f); CaptureActor("thornling-run", enemyVisual);
            // Stand in the patrol's path so it makes real sustained contact.
            Hero.transform.position = enemy.transform.position + Vector3.right * .55f;
            Hero.GetComponent<Rigidbody2D>().position = Hero.transform.position; Physics2D.SyncTransforms();
            yield return Until(() => enemyVisual.CurrentClip == "attack", "Thornling real contact attack"); CaptureActor("thornling-attack", enemyVisual);
            enemy.TakeHit(1, Vector2.zero); yield return Until(() => enemyVisual.CurrentClip == "hurt", "Thornling hurt"); CaptureActor("thornling-hurt", enemyVisual);
            enemy.TakeHit(1, Vector2.zero); yield return For(.22f); CaptureActor("thornling-defeat", enemyVisual);
            var game = Hero.Game; var boss = game.Boss; var bossVisual = boss.GetComponentInChildren<PixelSpriteAnimator>();
            CaptureActor("guardian-idle", bossVisual);
            Hero.UnlockSpark(); Hero.transform.position = new Vector3(42f, 1f, 0); Body.position = Hero.transform.position; Physics2D.SyncTransforms();
            yield return Until(() => bossVisual.CurrentClip == "telegraph", "Guardian telegraph"); CaptureActor("guardian-telegraph", bossVisual);
            yield return Until(() => bossVisual.CurrentClip == "charge", "Guardian charge"); CaptureActor("guardian-charge", bossVisual);
            yield return Until(() => bossVisual.CurrentClip == "idle", "Guardian recovery");
            boss.TakeHit(1, Vector2.zero); yield return Until(() => bossVisual.CurrentClip == "hurt", "Guardian hurt"); CaptureActor("guardian-hurt", bossVisual);
            boss.TakeHit(100, Vector2.zero); yield return For(.3f); CaptureActor("guardian-defeat", bossVisual);
            Assert.That(game.Won, Is.True);
            yield return For(1.6f); Assert.That(bossVisual.Renderer.enabled, Is.False, "Defeat releases to ending after its original delay");
        }

        private void CaptureActor(string name, PixelSpriteAnimator animator)
        {
            var camera = Hero.Game.MainCamera; var oldPosition = camera.transform.position; var oldSize = camera.orthographicSize;
            try { camera.transform.position = animator.Renderer.bounds.center + Vector3.back * 10; camera.orthographicSize = Mathf.Max(1.7f, animator.Renderer.bounds.size.y * .8f); Render(camera, Path.Combine(Output, name + ".png"), 800, 600); }
            finally { camera.transform.position = oldPosition; camera.orthographicSize = oldSize; }
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            MobileInput.Reset();
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        private void Capture(string action)
        {
            var camera = Hero.Game.MainCamera;
            var col = Hero.GetComponent<Collider2D>();
            trace.AppendLine($"{action},{Time.time:F3},{Visual.CurrentClip},{Visual.Renderer.sprite.name},{Hero.Facing},{Visual.transform.localEulerAngles.z:F1},{Visual.Renderer.bounds.min.y:F3},{col.bounds.min.y:F3},{Hero.Weapon}");
            var oldPosition = camera.transform.position; var oldSize = camera.orthographicSize;
            try
            {
                camera.transform.position = new Vector3(Hero.transform.position.x, Hero.transform.position.y + .2f, -10);
                camera.orthographicSize = 2.4f;
                Render(camera, Path.Combine(Output, action + ".png"), 800, 600);
            }
            finally { camera.transform.position = oldPosition; camera.orthographicSize = oldSize; }
        }

        private static GameObject InScene(string name, Scene scene)
        {
            var go = new GameObject(name); SceneManager.MoveGameObjectToScene(go, scene); return go;
        }
        private static void Label(Scene scene, Vector3 position, string text, float size)
        {
            var go = InScene(text, scene); go.transform.position = position;
            var tm = go.AddComponent<TextMesh>(); tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.text = text; tm.fontSize = 30; tm.characterSize = size; tm.anchor = TextAnchor.UpperCenter;
            tm.color = new Color(.85f, .9f, .75f); tm.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        }
        private static void Render(Camera camera, string path, int width, int height)
        {
            var target = new RenderTexture(width, height, 24); var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            try { camera.targetTexture = target; camera.Render(); RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes(path, image.EncodeToPNG()); }
            finally { camera.targetTexture = previousTarget; RenderTexture.active = previousActive; Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target); }
        }
        private static IEnumerator For(float seconds) { float end = Time.time + seconds; yield return Until(() => Time.time >= end, "simulation advances"); }
        private static IEnumerator Until(Func<bool> condition, string reason)
        {
            float end = Time.realtimeSinceStartup + 8;
            while (!condition() && Time.realtimeSinceStartup < end) { EditorApplication.QueuePlayerLoopUpdate(); yield return null; }
            Assert.That(condition(), Is.True, reason);
        }
    }
}
