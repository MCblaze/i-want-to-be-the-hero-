using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class TrainingTarget : HeroTarget
    {
        public int DamageTaken { get; private set; }
        public int HitCount { get; private set; }
        public override bool IsAlive => true;
        public void ResetTarget() { DamageTaken = HitCount = 0; }
        public override void TakeHit(int damage, Vector2 force)
        {
            DamageTaken += damage;
            HitCount++;
        }
        private void Update()
        {
            var label = GetComponentInChildren<TextMesh>();
            if (label != null) label.text = $"TARGET\n{HitCount} hits / {DamageTaken} damage";
        }
    }
}
