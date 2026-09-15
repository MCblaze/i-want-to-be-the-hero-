using UnityEngine;

namespace IWantToBeTheHero.Audio
{
    public sealed class SurfaceAudioTag : MonoBehaviour
    {
        public enum SurfaceKind
        {
            Default,
            Wood,
            Stone
        }

        [SerializeField] private SurfaceKind surface = SurfaceKind.Default;
        [SerializeField] private AudioCueId footstepOverride = AudioCueId.None;
        [SerializeField] private AudioCueId landingOverride = AudioCueId.None;

        public SurfaceKind Surface => surface;
        public AudioCueId FootstepCue => footstepOverride != AudioCueId.None
            ? footstepOverride
            : DefaultFootstepFor(surface);
        public AudioCueId LandingCue => landingOverride != AudioCueId.None
            ? landingOverride
            : AudioCueId.LoganLand;

        public static AudioCueId DefaultFootstepFor(SurfaceKind kind)
        {
            switch (kind)
            {
                case SurfaceKind.Wood: return AudioCueId.LoganFootstepWood;
                case SurfaceKind.Stone: return AudioCueId.LoganFootstepStone;
                default: return AudioCueId.LoganFootstepDefault;
            }
        }
    }
}
