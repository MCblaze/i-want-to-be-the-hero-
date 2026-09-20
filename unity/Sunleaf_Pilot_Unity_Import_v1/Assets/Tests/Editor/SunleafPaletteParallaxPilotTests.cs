using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class SunleafPaletteParallaxPilotTests
    {
        private const string PilotPath = "Assets/Scenes/Sunleaf_PaletteParallax_Pilot.unity";
        private const string PilotRootName = "Sunleaf Palette and Parallax Pilot";
        private static readonly float[] SupportedAspects = { 1f, 16f / 9f, 21f / 9f };
        private static readonly float[] RouteSamples = { 12.6f, 144f, 275.4f };

        [UnitySetUp]
        public IEnumerator OpenPilot()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(PilotPath),
                Is.Not.Null,
                "Build the isolated pilot with SunleafPaletteParallaxPilotBuilder before running this gate.");
            EditorSceneManager.OpenScene(PilotPath);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            if (EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();
            }
        }

        [UnityTest]
        public IEnumerator FourLayerPool_CoversStartMiddleAndEndAtAllSupportedAspects()
        {
            GameObject root = GameObject.Find(PilotRootName);
            Assert.That(root, Is.Not.Null);
            SunleafParallaxStrip[] strips = root.GetComponentsInChildren<SunleafParallaxStrip>(true)
                .OrderBy(strip => strip.ScreenDrift)
                .ToArray();
            Assert.That(strips, Has.Length.EqualTo(4));
            float[] expectedDrift = { 0f, 0.08f, 0.18f, 0.42f };
            for (int index = 0; index < expectedDrift.Length; index++)
            {
                Assert.That(strips[index].ScreenDrift, Is.EqualTo(expectedDrift[index]).Within(0.0001f));
            }
            Assert.That(root.GetComponentsInChildren<Collider2D>(true), Is.Empty);

            Camera camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            foreach (float aspect in SupportedAspects)
            {
                camera.aspect = aspect;
                float halfWidth = camera.orthographicSize * aspect;
                foreach (float cameraX in RouteSamples)
                {
                    Vector3 cameraPosition = camera.transform.position;
                    cameraPosition.x = cameraX;
                    camera.transform.position = cameraPosition;
                    foreach (SunleafParallaxStrip strip in strips)
                    {
                        strip.RefreshNow();
                        SpriteRenderer[] ordered = strip.Segments
                            .OrderBy(renderer => renderer.bounds.center.x)
                            .ToArray();
                        Assert.That(ordered, Has.Length.AtLeast(4));
                        Assert.That(ordered[0].bounds.min.x, Is.LessThanOrEqualTo(cameraX - halfWidth + 0.001f), strip.name);
                        Assert.That(ordered[ordered.Length - 1].bounds.max.x,
                            Is.GreaterThanOrEqualTo(cameraX + halfWidth - 0.001f), strip.name);
                        for (int index = 1; index < ordered.Length; index++)
                        {
                            Assert.That(
                                ordered[index].bounds.min.x,
                                Is.EqualTo(ordered[index - 1].bounds.max.x).Within(0.001f),
                                strip.name + " has a segment gap or overlap.");
                        }

                        Assert.That(strip.ValidateConfiguration(out string reason), Is.True, reason);
                    }
                }
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator LegacyZoneBackdropsAndAnchors_AreInactiveOnlyInPilot()
        {
            ZoneBackdrop[] backdrops = Resources.FindObjectsOfTypeAll<ZoneBackdrop>()
                .Where(item => item.gameObject.scene.IsValid())
                .ToArray();
            QP3ParallaxAnchor[] anchors = Resources.FindObjectsOfTypeAll<QP3ParallaxAnchor>()
                .Where(item => item.gameObject.scene.IsValid())
                .ToArray();

            Assert.That(backdrops, Is.Not.Empty);
            Assert.That(anchors, Is.Not.Empty);
            Assert.That(backdrops.All(item => !item.gameObject.activeInHierarchy), Is.True);
            Assert.That(anchors.All(item => !item.gameObject.activeInHierarchy), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RequiredPitsRemainOpenAndParallaxNeverEntersPhysics()
        {
            foreach (float center in new[] { 55f, 66f, 81f, 92f })
            {
                bool hasStaticSupport = Physics2D.RaycastAll(new Vector2(center, -3.41f), Vector2.down, 4.5f)
                    .Any(hit => hit.collider != null && !hit.collider.isTrigger && hit.collider.attachedRigidbody == null);
                Assert.That(hasStaticSupport, Is.False, "Pilot art or geometry bridges required pit " + center);
            }

            foreach (SunleafParallaxStrip strip in Object.FindObjectsByType<SunleafParallaxStrip>(FindObjectsSortMode.None))
            {
                Assert.That(strip.GetComponentsInChildren<Collider2D>(true), Is.Empty, strip.name);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlatformPalette_KeepsNativeSkinsContinuousAndSeamsPresentationOnly()
        {
            Transform[] all = Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)
                .ToArray();
            Transform[] floorTransforms = all
                .Where(item => item.gameObject.activeInHierarchy && item.name.StartsWith("Floor "))
                .ToArray();
            Transform[] seamTransforms = all
                .Where(item => item.gameObject.activeInHierarchy && item.name.StartsWith("Visual seam "))
                .ToArray();
            Transform[] canopySupports = all
                .Where(item => item.gameObject.activeInHierarchy && item.name == "Canopy support")
                .ToArray();

            Assert.That(floorTransforms, Has.Length.EqualTo(42));
            Assert.That(seamTransforms, Has.Length.EqualTo(33));
            Assert.That(canopySupports, Has.Length.EqualTo(3));

            var floors = new List<FloorSample>();
            foreach (Transform floor in floorTransforms)
            {
                Collider2D[] solids = floor.GetComponentsInChildren<Collider2D>(true)
                    .Where(collider => collider.enabled && !collider.isTrigger)
                    .ToArray();
                SpriteRenderer skin = floor.GetComponentsInChildren<SpriteRenderer>(true)
                    .FirstOrDefault(renderer => renderer.enabled &&
                        renderer.name.StartsWith("Premium Mossy Stone Skin") &&
                        renderer.sprite != null);
                Assert.That(solids, Is.Not.Empty, floor.name);
                Assert.That(skin, Is.Not.Null, floor.name);
                Assert.That(skin.sprite.texture.name, Is.EqualTo("StaticMossyLedge"), floor.name);
                Assert.That(skin.sprite.pixelsPerUnit, Is.EqualTo(400f).Within(0.001f), floor.name);

                Bounds bounds = solids[0].bounds;
                foreach (Collider2D collider in solids.Skip(1)) bounds.Encapsulate(collider.bounds);
                floors.Add(new FloorSample { Bounds = bounds, Skin = skin });
            }

            SpriteRenderer[] seams = seamTransforms.Select(item => item.GetComponent<SpriteRenderer>()).ToArray();
            Assert.That(seams.All(renderer => renderer != null && renderer.sprite != null), Is.True);
            Assert.That(seams.All(renderer => renderer.sprite.name == "DeliveryWalklineSeam"), Is.True);
            Assert.That(seams.All(renderer => renderer.sprite.pixelsPerUnit == 400f), Is.True);
            Assert.That(seamTransforms.SelectMany(item => item.GetComponentsInChildren<Collider2D>(true)), Is.Empty);

            int eligibleJoinCount = 0;
            foreach (FloorSample left in floors.OrderBy(sample => sample.Bounds.min.x))
            foreach (FloorSample right in floors.Where(sample => sample.Bounds.center.x > left.Bounds.center.x))
            {
                float gap = right.Bounds.min.x - left.Bounds.max.x;
                if (Mathf.Abs(gap) > 0.1f || Mathf.Abs(left.Bounds.max.y - right.Bounds.max.y) > 0.015f)
                {
                    continue;
                }

                if (left.Skin.sprite.texture != right.Skin.sprite.texture ||
                    Mathf.Abs(left.Skin.sprite.pixelsPerUnit - right.Skin.sprite.pixelsPerUnit) > 0.001f)
                {
                    continue;
                }

                eligibleJoinCount++;
                float joinX = (left.Bounds.max.x + right.Bounds.min.x) * 0.5f;
                Assert.That(
                    seams.Any(renderer => Mathf.Abs(renderer.bounds.center.x - joinX) <= 0.1f),
                    Is.True,
                    $"Missing native seam cover at x {joinX:F3}.");
            }

            Assert.That(eligibleJoinCount, Is.EqualTo(33));
            Assert.That(
                canopySupports.SelectMany(item => item.GetComponentsInChildren<SpriteRenderer>(true))
                    .Any(renderer => renderer.sprite != null && renderer.sprite.name == "TealSurfaceAligned"),
                Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoulderDecoration_IsVisibleBehindLoganAndHasNoPhysics()
        {
            GameObject boulder = GameObject.Find("Boulder Decor - No Collider");
            Assert.That(boulder, Is.Not.Null);
            SpriteRenderer renderer = boulder.GetComponent<SpriteRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.enabled && renderer.gameObject.activeInHierarchy, Is.True);
            Assert.That(renderer.sprite, Is.Not.Null);
            Assert.That(renderer.sprite.name, Is.EqualTo("Sunleaf_L01_BoulderDecor_Pilot"));
            Assert.That(renderer.sortingOrder, Is.EqualTo(3));
            Assert.That(renderer.sortingOrder, Is.LessThan(20), "Logan renders at order 20.");
            Assert.That(boulder.GetComponentsInChildren<Collider2D>(true), Is.Empty);
            Assert.That(boulder.GetComponentsInChildren<Rigidbody2D>(true), Is.Empty);
            yield return null;
        }

        private sealed class FloorSample
        {
            public Bounds Bounds;
            public SpriteRenderer Skin;
        }
    }
}
