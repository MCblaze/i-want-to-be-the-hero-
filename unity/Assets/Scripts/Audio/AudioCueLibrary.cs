using System;
using System.Collections.Generic;
using UnityEngine;

namespace IWantToBeTheHero.Audio
{
    [CreateAssetMenu(fileName = "AudioCueLibrary", menuName = "I Want To Be The Hero/Audio/Cue Library")]
    public sealed class AudioCueLibrary : ScriptableObject
    {
        public enum Bus
        {
            SoundEffects,
            Music
        }

        [Serializable]
        public sealed class Cue
        {
            public AudioCueId id = AudioCueId.None;
            public Bus bus = Bus.SoundEffects;
            public AudioClip[] variations = Array.Empty<AudioClip>();
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.1f, 3f)] public float pitchMin = 1f;
            [Range(0.1f, 3f)] public float pitchMax = 1f;
            [Min(0f)] public float cooldownSeconds;
            [Range(0f, 1f)] public float spatialBlend;
            [Min(0f)] public float minDistance = 1f;
            [Min(0.01f)] public float maxDistance = 25f;
        }

        [SerializeField] private Cue[] cues = Array.Empty<Cue>();
        private Dictionary<AudioCueId, Cue> lookup;

        public IReadOnlyList<Cue> Cues => cues;

        public bool TryGet(AudioCueId id, out Cue cue)
        {
            EnsureLookup();
            return lookup.TryGetValue(id, out cue) && cue != null;
        }

        private void OnEnable()
        {
            lookup = null;
        }

        private void OnValidate()
        {
            lookup = null;
            if (cues == null)
                cues = Array.Empty<Cue>();

            foreach (Cue cue in cues)
            {
                if (cue == null)
                    continue;
                cue.pitchMin = Mathf.Max(0.1f, cue.pitchMin);
                cue.pitchMax = Mathf.Max(cue.pitchMin, cue.pitchMax);
                cue.maxDistance = Mathf.Max(cue.minDistance, cue.maxDistance);
            }
        }

        private void EnsureLookup()
        {
            if (lookup != null)
                return;

            lookup = new Dictionary<AudioCueId, Cue>();
            if (cues == null)
                return;

            foreach (Cue cue in cues)
            {
                if (cue != null && cue.id != AudioCueId.None)
                    lookup[cue.id] = cue;
            }
        }
    }
}
