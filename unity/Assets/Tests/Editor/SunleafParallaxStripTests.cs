using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class SunleafParallaxStripTests
    {
        private GameObject cameraObject;
        private GameObject stripObject;
        private Texture2D texture;
        private Sprite sprite;

        [TearDown]
        public void TearDown()
        {
            if (stripObject != null) Object.DestroyImmediate(stripObject);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (sprite != null) Object.DestroyImmediate(sprite);
            if (texture != null) Object.DestroyImmediate(texture);
        }

        [Test]
        public void WidestSupportedViewport_RequiresFourSegments()
        {
            const float viewportWidth = 25.2f;
            const float segmentWidth = 24f;

            Assert.That(
                SunleafParallaxStrip.RequiredSegmentCount(viewportWidth, segmentWidth),
                Is.EqualTo(4));
        }

        [TestCase(0f, 100f, 0f)]
        [TestCase(0.08f, 92f, -8f)]
        [TestCase(0.42f, 58f, -42f)]
        [TestCase(1f, 0f, -100f)]
        public void ScreenDrift_HasDocumentedMeaning(
            float drift,
            float expectedLayerX,
            float expectedScreenOffset)
        {
            float layerX = SunleafParallaxStrip.CalculateLayerOriginX(0f, 0f, 100f, drift);

            Assert.That(layerX, Is.EqualTo(expectedLayerX).Within(0.0001f));
            Assert.That(layerX - 100f, Is.EqualTo(expectedScreenOffset).Within(0.0001f));
        }

        [Test]
        public void AlternatingMirrorParity_IsStableAcrossZero()
        {
            int[] normal = { -4, -2, 0, 2, 4 };
            int[] mirrored = { -3, -1, 1, 3 };

            Assert.That(normal.All(index => !SunleafParallaxStrip.IsMirroredWorldIndex(index)), Is.True);
            Assert.That(mirrored.All(SunleafParallaxStrip.IsMirroredWorldIndex), Is.True);
        }

        [Test]
        public void CameraSnap_RebuildsAContiguousAbsoluteGridWithoutColliders()
        {
            SunleafParallaxStrip strip = CreateStrip(0.42f);

            cameraObject.transform.position = new Vector3(275.4f, 0f, -10f);
            strip.RefreshNow();

            SpriteRenderer[] ordered = strip.Segments.OrderBy(renderer => renderer.transform.position.x).ToArray();
            for (int index = 1; index < ordered.Length; index++)
            {
                Assert.That(
                    ordered[index].transform.position.x - ordered[index - 1].transform.position.x,
                    Is.EqualTo(24f).Within(0.001f));
            }

            float leftEdge = ordered[0].bounds.min.x;
            float rightEdge = ordered[ordered.Length - 1].bounds.max.x;
            Assert.That(leftEdge, Is.LessThanOrEqualTo(275.4f - 12.6f));
            Assert.That(rightEdge, Is.GreaterThanOrEqualTo(275.4f + 12.6f));
            Assert.That(strip.GetComponentsInChildren<Collider2D>(true), Is.Empty);
            Assert.That(strip.ValidateConfiguration(out string reason), Is.True, reason);
        }

        [Test]
        public void ReverseTravel_ReturnsToTheSamePositionsAndMirrorState()
        {
            SunleafParallaxStrip strip = CreateStrip(1f);
            strip.RefreshNow();
            Vector3[] startingPositions = strip.Segments.Select(renderer => renderer.transform.position).ToArray();
            bool[] startingFlip = strip.Segments.Select(renderer => renderer.flipX).ToArray();

            cameraObject.transform.position = new Vector3(288f, 0f, -10f);
            strip.RefreshNow();
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            strip.RefreshNow();

            Assert.That(
                strip.Segments.Select(renderer => renderer.transform.position).ToArray(),
                Is.EqualTo(startingPositions));
            Assert.That(
                strip.Segments.Select(renderer => renderer.flipX).ToArray(),
                Is.EqualTo(startingFlip));
        }

        [Test]
        public void InvalidInputs_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SunleafParallaxStrip.RequiredSegmentCount(0f, 24f));
            Assert.Throws<ArgumentOutOfRangeException>(() => SunleafParallaxStrip.RequiredSegmentCount(25.2f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => SunleafParallaxStrip.CalculateFirstWorldIndex(0f, 0f, 0f, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => SunleafParallaxStrip.CalculateFirstWorldIndex(0f, 0f, 24f, 0));
        }

        private SunleafParallaxStrip CreateStrip(float drift)
        {
            cameraObject = new GameObject("Parallax test camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.aspect = 21f / 9f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            texture = new Texture2D(240, 120, TextureFormat.RGBA32, false);
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 240f, 120f),
                new Vector2(0.5f, 0.5f),
                10f,
                0,
                SpriteMeshType.FullRect);

            stripObject = new GameObject("Parallax test strip");
            SpriteRenderer[] renderers = new SpriteRenderer[4];
            for (int index = 0; index < renderers.Length; index++)
            {
                GameObject segment = new GameObject($"Segment {index}");
                segment.transform.SetParent(stripObject.transform, false);
                renderers[index] = segment.AddComponent<SpriteRenderer>();
                renderers[index].sprite = sprite;
            }

            SunleafParallaxStrip strip = stripObject.AddComponent<SunleafParallaxStrip>();
            strip.Configure(camera.transform, drift, renderers);
            Assert.That(strip.ValidateConfiguration(out string reason), Is.True, reason);
            return strip;
        }
    }
}
