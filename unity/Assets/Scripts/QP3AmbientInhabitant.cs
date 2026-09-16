using UnityEngine;

namespace IWantToBeTheHero
{
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
            if (AccessibilitySettings.ReducedEffects)
            {
                ResetPresentation();
                return;
            }
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

