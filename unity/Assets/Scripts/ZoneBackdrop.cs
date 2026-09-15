using UnityEngine;

namespace IWantToBeTheHero
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ZoneBackdrop : MonoBehaviour
    {
        [SerializeField] private float centerX;
        [SerializeField, Min(0.1f)] private float fullVisibilityHalfWidth = 4f;
        [SerializeField, Min(0.1f)] private float fadeWidth = 2f;
        [SerializeField, Range(0f, 1f)] private float maximumAlpha = 1f;

        private SpriteRenderer spriteRenderer;
        private Camera trackedCamera;

        public void Configure(float center, float halfWidth, float edgeFade, float alpha)
        {
            centerX = center;
            fullVisibilityHalfWidth = Mathf.Max(0.1f, halfWidth);
            fadeWidth = Mathf.Max(0.1f, edgeFade);
            maximumAlpha = Mathf.Clamp01(alpha);
            ApplyAlpha(maximumAlpha);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            trackedCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (trackedCamera == null) trackedCamera = Camera.main;
            if (trackedCamera == null) return;

            float distance = Mathf.Abs(trackedCamera.transform.position.x - centerX);
            float alpha = 1f - Mathf.InverseLerp(fullVisibilityHalfWidth, fullVisibilityHalfWidth + fadeWidth, distance);
            ApplyAlpha(alpha * maximumAlpha);
        }

        private void ApplyAlpha(float alpha)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
