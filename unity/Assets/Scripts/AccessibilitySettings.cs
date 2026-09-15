using UnityEngine;

namespace IWantToBeTheHero
{
    public static class AccessibilitySettings
    {
        public static bool ReducedEffects { get; private set; }
        public static bool MutedAudio { get; private set; }

        public static void SetReducedEffects(bool enabled)
        {
            ReducedEffects = enabled;
            foreach (var particles in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (enabled) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                else particles.Play(true);
            }
        }

        public static void SetMutedAudio(bool enabled)
        {
            MutedAudio = enabled;
            AudioListener.volume = enabled ? 0f : 1f;
        }

        public static void Reset()
        {
            SetReducedEffects(false);
            SetMutedAudio(false);
        }
    }
}
