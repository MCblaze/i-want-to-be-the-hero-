using UnityEngine;

namespace IWantToBeTheHero
{
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
            if (AccessibilitySettings.ReducedEffects)
            {
                transform.localPosition = startPosition;
                return;
            }
            float offset = Mathf.Sin((Time.time + phase) * frequency * Mathf.PI * 2f) * amplitude;
            transform.localPosition = startPosition + localAxis.normalized * offset;
        }
    }
}

