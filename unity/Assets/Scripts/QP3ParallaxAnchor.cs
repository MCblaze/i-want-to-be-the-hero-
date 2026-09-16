using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class QP3ParallaxAnchor : MonoBehaviour
    {
        public Transform cameraTarget;
        [Range(0f, 1f)] public float parallaxFactor;
        public float wrapWidth = 0f;
        private float lastCameraX;

        private void Awake()
        {
            if (cameraTarget == null && Camera.main != null) cameraTarget = Camera.main.transform;
            if (cameraTarget != null) lastCameraX = cameraTarget.position.x;
        }

        private void LateUpdate()
        {
            if (cameraTarget == null) return;
            float delta = cameraTarget.position.x - lastCameraX;
            if (Mathf.Abs(delta) > 0.001f)
            {
                transform.position += Vector3.right * (delta * parallaxFactor);
                if (wrapWidth > 0f && Mathf.Abs(transform.position.x - cameraTarget.position.x) > wrapWidth)
                    transform.position += Vector3.right * Mathf.Sign(cameraTarget.position.x - transform.position.x) * wrapWidth;
                lastCameraX = cameraTarget.position.x;
            }
        }
    }
}

