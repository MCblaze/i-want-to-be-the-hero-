using UnityEngine;

namespace IWantToBeTheHero
{
    [CreateAssetMenu(menuName = "Hero/Movement Test Tuning")]
    public sealed class HeroTuning : ScriptableObject
    {
        [Header("Movement")]
        [Min(.1f)] public float runSpeed = 5.2f;
        [Min(.1f)] public float jumpSpeed = 10.6f;
        [Min(.1f)] public float secondJumpSpeed = 8.5f;
        [Min(0f)] public float coyoteTime = .12f;
        [Min(0f)] public float jumpBuffer = .14f;
        [Header("Evade")]
        [Min(.01f)] public float evadeCooldown = .7f;
        [Min(.01f)] public float dashDuration = .17f;
        [Min(.1f)] public float dashSpeed = 12.5f;
        [Min(.01f)] public float flipDuration = .4f;
        [Min(.1f)] public float flipSpeed = 4.5f;
        [Min(.1f)] public float flipJumpSpeed = 4f;
        [Header("Sunseed wand")]
        [Min(.01f)] public float wandCooldown = .45f;
        [Min(.1f)] public float seedSpeed = 12f;
        [Min(.1f)] public float seedRange = 10f;
        [Range(1, 8)] public int maxSeeds = 4;
    }

    public enum HeroWeapon { Sword, SunseedWand }
}
