using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class LabCheckpoint : MonoBehaviour
    {
        public Vector2 safePosition;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out HeroController hero)) return;
            hero.SetCheckpoint(safePosition);
            GetComponent<SpriteRenderer>().color = new Color(1f, .8f, .25f);
        }
    }
}
