using System;
using UnityEngine;

namespace IWantToBeTheHero
{
    /// <summary>
    /// Positions a fixed pool of equal-width sprites on an absolute parallax grid.
    /// The grid is recalculated from the camera position every frame, so reverse
    /// travel, respawns and camera snaps cannot accumulate positional drift.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SunleafParallaxStrip : MonoBehaviour
    {
        [SerializeField] private Transform cameraTarget;
        [SerializeField, Range(0f, 1f)] private float screenDrift;
        [SerializeField] private bool alternateMirroring = true;
        [SerializeField] private SpriteRenderer[] segments = Array.Empty<SpriteRenderer>();

        private bool originCaptured;
        private float originCameraX;
        private float originLayerX;
        private float originLayerY;
        private float segmentWorldWidth;

        public Transform CameraTarget => cameraTarget;
        public float ScreenDrift => screenDrift;
        public bool AlternateMirroring => alternateMirroring;
        public SpriteRenderer[] Segments => segments;
        public float SegmentWorldWidth => segmentWorldWidth;

        private void Awake()
        {
            ResolveDefaults();
            CaptureOrigin();
        }

        private void OnEnable()
        {
            ResolveDefaults();
            CaptureOrigin();
            RefreshNow();
        }

        private void LateUpdate()
        {
            RefreshNow();
        }

        private void OnValidate()
        {
            screenDrift = Mathf.Clamp01(screenDrift);
            originCaptured = false;
        }

        public void Configure(
            Transform target,
            float drift,
            SpriteRenderer[] rendererPool,
            bool mirrorAlternateSegments = true)
        {
            cameraTarget = target;
            screenDrift = Mathf.Clamp01(drift);
            segments = rendererPool ?? Array.Empty<SpriteRenderer>();
            alternateMirroring = mirrorAlternateSegments;
            originCaptured = false;
            CaptureOrigin();
            RefreshNow();
        }

        public void RefreshNow(bool recaptureOrigin = false)
        {
            if (recaptureOrigin)
            {
                originCaptured = false;
            }

            ResolveDefaults();
            if (!originCaptured && !CaptureOrigin())
            {
                return;
            }

            if (cameraTarget == null || segments == null || segments.Length == 0 || segmentWorldWidth <= 0f)
            {
                return;
            }

            float cameraX = cameraTarget.position.x;
            float layerOriginX = CalculateLayerOriginX(originLayerX, originCameraX, cameraX, screenDrift);
            int firstWorldIndex = CalculateFirstWorldIndex(
                cameraX,
                layerOriginX,
                segmentWorldWidth,
                segments.Length);

            for (int slot = 0; slot < segments.Length; slot++)
            {
                SpriteRenderer renderer = segments[slot];
                if (renderer == null)
                {
                    continue;
                }

                int worldIndex = firstWorldIndex + slot;
                Vector3 position = renderer.transform.position;
                position.x = layerOriginX + ((worldIndex + 0.5f) * segmentWorldWidth);
                position.y = originLayerY;
                renderer.transform.position = position;
                renderer.flipX = alternateMirroring && IsMirroredWorldIndex(worldIndex);
            }
        }

        public bool ValidateConfiguration(out string reason)
        {
            ResolveDefaults();
            if (cameraTarget == null)
            {
                reason = "A camera target is required.";
                return false;
            }

            if (segments == null || segments.Length < 4)
            {
                reason = "At least four pooled segments are required.";
                return false;
            }

            float expectedWidth = 0f;
            for (int index = 0; index < segments.Length; index++)
            {
                SpriteRenderer renderer = segments[index];
                if (renderer == null || renderer.sprite == null)
                {
                    reason = $"Segment {index} is missing its renderer or sprite.";
                    return false;
                }

                if (renderer.GetComponent<Collider2D>() != null)
                {
                    reason = $"Segment {index} has a Collider2D; parallax art must be presentation-only.";
                    return false;
                }

                float width = renderer.sprite.bounds.size.x * Mathf.Abs(renderer.transform.lossyScale.x);
                if (width <= 0f)
                {
                    reason = $"Segment {index} has a non-positive world width.";
                    return false;
                }

                if (index == 0)
                {
                    expectedWidth = width;
                }
                else if (Mathf.Abs(width - expectedWidth) > 0.001f)
                {
                    reason = "All pooled segments must have the same measured world width.";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        public static int RequiredSegmentCount(float viewportWorldWidth, float segmentWidth, int reserveSegments = 2)
        {
            if (viewportWorldWidth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(viewportWorldWidth));
            }

            if (segmentWidth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentWidth));
            }

            if (reserveSegments < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reserveSegments));
            }

            return Mathf.Max(4, Mathf.CeilToInt(viewportWorldWidth / segmentWidth) + reserveSegments);
        }

        public static float CalculateLayerOriginX(
            float initialLayerX,
            float initialCameraX,
            float cameraX,
            float drift)
        {
            return initialLayerX + ((cameraX - initialCameraX) * (1f - Mathf.Clamp01(drift)));
        }

        public static int CalculateFirstWorldIndex(
            float cameraX,
            float layerOriginX,
            float segmentWidth,
            int segmentCount)
        {
            if (segmentWidth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentWidth));
            }

            if (segmentCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentCount));
            }

            float relativeCell = (cameraX - layerOriginX) / segmentWidth;
            int nearestBoundary = Mathf.FloorToInt(relativeCell + 0.5f);
            return nearestBoundary - (segmentCount / 2);
        }

        public static bool IsMirroredWorldIndex(int worldIndex)
        {
            return (worldIndex & 1) != 0;
        }

        private void ResolveDefaults()
        {
            if (cameraTarget == null && Camera.main != null)
            {
                cameraTarget = Camera.main.transform;
            }

            if ((segments == null || segments.Length == 0) && transform.childCount > 0)
            {
                segments = GetComponentsInChildren<SpriteRenderer>(true);
            }
        }

        private bool CaptureOrigin()
        {
            if (!ValidateConfiguration(out _))
            {
                originCaptured = false;
                segmentWorldWidth = 0f;
                return false;
            }

            originCameraX = cameraTarget.position.x;
            originLayerX = transform.position.x;
            originLayerY = transform.position.y;
            segmentWorldWidth = segments[0].sprite.bounds.size.x * Mathf.Abs(segments[0].transform.lossyScale.x);
            originCaptured = segmentWorldWidth > 0f;
            return originCaptured;
        }
    }
}
