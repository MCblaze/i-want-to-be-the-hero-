using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class SunseedProjectile : MonoBehaviour
    {
        private HeroController owner;
        private float direction, speed, remaining;

        public void Initialize(HeroController hero, float facing, float velocity, float range)
        {
            owner = hero;
            direction = facing;
            speed = velocity;
            remaining = range;
            var render = gameObject.AddComponent<SpriteRenderer>();
            render.sprite = PrototypeArt.Star();
            render.color = new Color(1f, .85f, .3f);
            render.sortingOrder = 24;
            transform.localScale = Vector3.one * .22f;
        }

        private void FixedUpdate()
        {
            if (owner == null || owner.Game == null || !owner.Game.Started || owner.Game.Won)
            { Destroy(gameObject); return; }
            float distance = Mathf.Min(speed * Time.fixedDeltaTime, remaining);
            Vector2 travel = Vector2.right * direction;
            // Sweep from the actual release position so thin walls cannot be skipped.
            foreach (var hit in Physics2D.CircleCastAll(transform.position, .1f, travel, distance))
            {
                if (hit.collider.isTrigger || hit.collider.GetComponent<HeroController>() == owner) continue;
                var target = hit.collider.GetComponent<HeroTarget>();
                if (target != null && target.IsAlive) target.TakeHit(1, travel * 2f);
                Destroy(gameObject);
                return;
            }
            transform.position += (Vector3)(travel * distance);
            remaining -= distance;
            if (remaining <= 0f) Destroy(gameObject);
        }
    }
}
