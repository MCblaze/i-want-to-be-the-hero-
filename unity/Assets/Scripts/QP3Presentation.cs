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

    public sealed class QP3BoundedMotion : MonoBehaviour
    {
        public Vector3 localAxis = Vector3.up;
        public float amplitude = 0.04f;
        public float frequency = 0.8f;
        private Vector3 startPosition;
        private float phase;

        private void Awake()
        {
            startPosition = transform.localPosition;
            phase = Mathf.Abs(GetInstanceID() % 1000) * 0.013f;
        }

        private void Update()
        {
            float offset = Mathf.Sin((Time.time + phase) * frequency * Mathf.PI * 2f) * amplitude;
            transform.localPosition = startPosition + localAxis.normalized * offset;
        }
    }

    public sealed class QP3AmbientInhabitant : MonoBehaviour
    {
        public float idleDuration = 2.2f;
        public float noticeDuration = 0.35f;
        public float reactDuration = 0.65f;
        public float settleDuration = 1.4f;
        public float cooldownDuration = 3f;
        private Vector3 startScale;
        private float stateTime;
        private float cooldown;
        private int state;

        private void Awake() => startScale = transform.localScale;

        private void Update()
        {
            if (cooldown > 0f)
            {
                cooldown -= Time.deltaTime;
                return;
            }

            stateTime += Time.deltaTime;
            float duration = state == 0 ? idleDuration : state == 1 ? noticeDuration : state == 2 ? reactDuration : settleDuration;
            if (stateTime >= duration)
            {
                stateTime = 0f;
                state++;
                if (state > 3)
                {
                    state = 0;
                    cooldown = cooldownDuration;
                }
            }

            float pulse = state == 1 ? 1.08f : state == 2 ? 0.88f : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, startScale * pulse, Time.deltaTime * 8f);
        }

        public void ResetPresentation()
        {
            state = 0;
            stateTime = 0f;
            cooldown = 0f;
            transform.localScale = startScale;
        }
    }
}
