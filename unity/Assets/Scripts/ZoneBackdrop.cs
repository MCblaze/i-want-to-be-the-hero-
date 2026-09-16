using UnityEngine;

namespace IWantToBeTheHero
{
    [ExecuteAlways, DefaultExecutionOrder(100), DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class ZoneBackdrop : MonoBehaviour
    {
        [SerializeField] private float centerX;
        [SerializeField, Min(.1f)] private float fullVisibilityHalfWidth = 4f;
        [SerializeField, Min(.1f)] private float fadeWidth = 2f;
        [SerializeField, Range(0f, 1f)] private float maximumAlpha = 1f;
        private SpriteRenderer spriteRenderer;
        private ZoneBackdrop[] siblings;

        private void OnEnable()
        {
            siblings = transform.parent == null ? new[] { this } : transform.parent.GetComponentsInChildren<ZoneBackdrop>();
        }

        public void Configure(float center, float halfWidth, float edgeFade, float alpha)
        {
            centerX = center;
            fullVisibilityHalfWidth = Mathf.Max(.1f, halfWidth);
            fadeWidth = Mathf.Max(.1f, edgeFade);
            maximumAlpha = Mathf.Clamp01(alpha);
            LateUpdate();
        }

        private void LateUpdate()
        {
            var camera = Camera.main;
            if (camera == null || !camera.orthographic) return;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite == null) return;

            // Each painted scene covers the view; crossfades never expose card edges.
            var size = spriteRenderer.sprite.bounds.size;
            float height = camera.orthographicSize * 2f;
            float scale = Mathf.Max(height / size.y, height * camera.aspect / size.x) * 1.08f;
            var parentScale = transform.parent == null ? Vector3.one : transform.parent.lossyScale;
            transform.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, 1f);
            float drift = Mathf.Clamp((camera.transform.position.x - centerX) * .025f, -.25f, .25f);
            transform.position = new Vector3(camera.transform.position.x - drift, camera.transform.position.y, 5f);

            float previous = float.NegativeInfinity;
            if (siblings == null) OnEnable();
            foreach (var other in siblings)
                if (other != null && other != this && other.centerX < centerX) previous = Mathf.Max(previous, other.centerX);
            float alpha = 1f;
            if (!float.IsNegativeInfinity(previous))
            {
                float midpoint = (previous + centerX) * .5f;
                alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(midpoint - fadeWidth, midpoint + fadeWidth, camera.transform.position.x));
            }
            // World coordinates are not rendering layers: expanded routes can
            // exceed x=180 and would otherwise cover gameplay sprites. Preserve
            // the original left-to-right compositing with a bounded rank instead.
            int rank = 0;
            foreach (var other in siblings)
                if (other != null && other != this &&
                    (other.centerX < centerX || (Mathf.Approximately(other.centerX, centerX) &&
                    other.transform.GetSiblingIndex() < transform.GetSiblingIndex()))) rank++;
            spriteRenderer.sortingOrder = -180 + Mathf.Min(rank, 79);
            spriteRenderer.color = new Color(.77f, .85f, .87f, alpha * maximumAlpha);
        }
    }
}
