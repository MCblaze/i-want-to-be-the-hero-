using UnityEngine;

namespace IWantToBeTheHero
{
    [DefaultExecutionOrder(-100)]
    public sealed class LabMovingPlatform : MonoBehaviour
    {
        public Vector2 travel = new(3f, 0f);
        public float period = 5f;
        public Vector2 Velocity { get; private set; }
        private Rigidbody2D body;
        private Vector2 origin;
        private float started;
        private void Awake() { body = GetComponent<Rigidbody2D>(); origin = body.position; started = Time.time; }
        public void ResetMotion() { started = Time.time; Velocity = Vector2.zero; if (body != null) body.position = origin; }
        private void FixedUpdate()
        {
            float phase = (1f - Mathf.Cos((Time.time - started) * Mathf.PI * 2f / Mathf.Max(.1f, period))) * .5f;
            Vector2 next = origin + travel * phase;
            // MovePosition's physics velocity is not available to later FixedUpdate callbacks yet.
            Velocity = (next - body.position) / Time.fixedDeltaTime;
            body.MovePosition(next);
        }
    }
}
