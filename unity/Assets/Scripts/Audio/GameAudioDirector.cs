using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IWantToBeTheHero.Audio
{
    [DisallowMultipleComponent]
    public sealed class GameAudioDirector : MonoBehaviour
    {
        private sealed class Voice
        {
            public AudioSource source;
            public float cueVolume;
        }

        [SerializeField] private AudioCueLibrary library;
        [SerializeField, Min(1)] private int initialPoolSize = 10;
        [SerializeField, Min(1)] private int maximumPoolSize = 24;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float soundEffectsVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        [SerializeField] private bool muted;

        private readonly List<Voice> voices = new List<Voice>();
        private readonly Dictionary<AudioCueId, float> lastPlayTimes = new Dictionary<AudioCueId, float>();
        private readonly Dictionary<AudioCueId, int> lastVariations = new Dictionary<AudioCueId, int>();
        private AudioSource musicA;
        private AudioSource musicB;
        private AudioSource currentMusic;
        private AudioSource transitionIncoming;
        private Coroutine musicTransition;
        private float currentMusicCueVolume = 1f;

        public AudioCueLibrary Library { get => library; set => library = value; }
        public bool IsMuted => muted;
        public float MasterVolume => masterVolume;
        public float SoundEffectsVolume => soundEffectsVolume;
        public float MusicVolume => musicVolume;
        public int PoolSize => voices.Count;
        public int ActiveVoiceCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < voices.Count; i++)
                    if (voices[i].source.isPlaying) count++;
                return count;
            }
        }

        private void Awake()
        {
            initialPoolSize = Mathf.Max(1, initialPoolSize);
            maximumPoolSize = Mathf.Max(initialPoolSize, maximumPoolSize);
            EnsureMusicSources();
            for (int i = voices.Count; i < initialPoolSize; i++)
                CreateVoice();
            ApplyVolumes();
        }

        public bool TryPlay(AudioCueId id)
        {
            return TryPlayAt(id, transform.position);
        }

        public bool TryPlayAt(AudioCueId id, Vector3 worldPosition)
        {
            AudioCueLibrary.Cue cue;
            if (!TryResolvePlayableCue(id, AudioCueLibrary.Bus.SoundEffects, out cue))
                return false;

            float now = Time.unscaledTime;
            float lastTime;
            if (cue.cooldownSeconds > 0f && lastPlayTimes.TryGetValue(id, out lastTime) && now - lastTime < cue.cooldownSeconds)
                return false;

            Voice voice = FindVoice();
            if (voice == null)
                return false;

            AudioClip clip = SelectVariation(id, cue.variations);
            if (clip == null)
                return false;

            lastPlayTimes[id] = now;
            voice.cueVolume = Mathf.Clamp01(cue.volume);
            ConfigureSource(voice.source, cue, worldPosition);
            voice.source.clip = clip;
            voice.source.volume = EffectiveSoundEffectsVolume * voice.cueVolume;
            voice.source.Play();
            return true;
        }

        public bool PlayMusic(AudioCueId id, float crossfadeSeconds = 0.75f, bool restartIfSame = false)
        {
            AudioCueLibrary.Cue cue;
            if (!TryResolvePlayableCue(id, AudioCueLibrary.Bus.Music, out cue))
                return false;

            float now = Time.unscaledTime;
            float lastTime;
            if (cue.cooldownSeconds > 0f && lastPlayTimes.TryGetValue(id, out lastTime) && now - lastTime < cue.cooldownSeconds)
                return false;

            AudioClip clip = SelectVariation(id, cue.variations);
            if (clip == null)
                return false;
            if (!restartIfSame && currentMusic != null && currentMusic.clip == clip && currentMusic.isPlaying)
                return true;

            EnsureMusicSources();
            CancelMusicTransition();
            AudioSource incoming = currentMusic == musicA ? musicB : musicA;
            incoming.Stop();
            incoming.clip = clip;
            incoming.loop = true;
            incoming.pitch = UnityEngine.Random.Range(cue.pitchMin, cue.pitchMax);
            incoming.volume = 0f;
            incoming.Play();

            lastPlayTimes[id] = now;
            transitionIncoming = incoming;
            musicTransition = StartCoroutine(CrossfadeMusic(currentMusic, incoming, Mathf.Max(0f, crossfadeSeconds), Mathf.Clamp01(cue.volume)));
            return true;
        }

        public void StopMusic(float fadeSeconds = 0.5f)
        {
            CancelMusicTransition();
            musicTransition = StartCoroutine(CrossfadeMusic(currentMusic, null, Mathf.Max(0f, fadeSeconds), 0f));
        }

        public void SetMuted(bool value)
        {
            muted = value;
            ApplyVolumes();
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        public void SetSoundEffectsVolume(float value)
        {
            soundEffectsVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        public void StopAllSoundEffects()
        {
            for (int i = 0; i < voices.Count; i++)
            {
                voices[i].source.Stop();
                voices[i].source.clip = null;
            }
        }

        private float EffectiveSoundEffectsVolume => muted ? 0f : masterVolume * soundEffectsVolume;
        private float EffectiveMusicVolume => muted ? 0f : masterVolume * musicVolume;

        private bool TryResolvePlayableCue(AudioCueId id, AudioCueLibrary.Bus requiredBus, out AudioCueLibrary.Cue cue)
        {
            cue = null;
            return id != AudioCueId.None && library != null && library.TryGet(id, out cue) &&
                   cue.bus == requiredBus && cue.variations != null && cue.variations.Length > 0;
        }

        private Voice FindVoice()
        {
            for (int i = 0; i < voices.Count; i++)
                if (!voices[i].source.isPlaying) return voices[i];

            return voices.Count < maximumPoolSize ? CreateVoice() : null;
        }

        private Voice CreateVoice()
        {
            GameObject child = new GameObject("SFX Voice " + (voices.Count + 1));
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            Voice voice = new Voice { source = source, cueVolume = 1f };
            voices.Add(voice);
            return voice;
        }

        private static void ConfigureSource(AudioSource source, AudioCueLibrary.Cue cue, Vector3 position)
        {
            source.transform.position = position;
            source.loop = false;
            source.playOnAwake = false;
            source.pitch = UnityEngine.Random.Range(cue.pitchMin, cue.pitchMax);
            source.spatialBlend = Mathf.Clamp01(cue.spatialBlend);
            source.minDistance = Mathf.Max(0f, cue.minDistance);
            source.maxDistance = Mathf.Max(source.minDistance, cue.maxDistance);
        }

        private AudioClip SelectVariation(AudioCueId id, AudioClip[] variations)
        {
            if (variations == null || variations.Length == 0)
                return null;

            int validCount = 0;
            for (int i = 0; i < variations.Length; i++)
                if (variations[i] != null) validCount++;
            if (validCount == 0)
                return null;

            int previous;
            bool hasPrevious = lastVariations.TryGetValue(id, out previous) && previous >= 0 &&
                               previous < variations.Length && variations[previous] != null;
            int selectableCount = validCount - (hasPrevious && validCount > 1 ? 1 : 0);
            int ordinal = UnityEngine.Random.Range(0, selectableCount);
            int index = -1;
            for (int i = 0; i < variations.Length; i++)
            {
                if (variations[i] == null || (hasPrevious && validCount > 1 && i == previous))
                    continue;
                if (ordinal-- == 0)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                index = previous;
            lastVariations[id] = index;
            return variations[index];
        }

        private void EnsureMusicSources()
        {
            if (musicA == null) musicA = CreateMusicSource("Music A");
            if (musicB == null) musicB = CreateMusicSource("Music B");
        }

        private AudioSource CreateMusicSource(string sourceName)
        {
            GameObject child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            return source;
        }

        private IEnumerator CrossfadeMusic(AudioSource outgoing, AudioSource incoming, float duration, float incomingCueVolume)
        {
            float outgoingStart = outgoing != null ? outgoing.volume : 0f;
            float elapsed = 0f;
            do
            {
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                if (outgoing != null) outgoing.volume = Mathf.Lerp(outgoingStart, 0f, t);
                if (incoming != null) incoming.volume = Mathf.Lerp(0f, EffectiveMusicVolume * incomingCueVolume, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            } while (elapsed < duration);

            if (outgoing != null)
            {
                outgoing.Stop();
                outgoing.clip = null;
            }
            currentMusic = incoming;
            transitionIncoming = null;
            currentMusicCueVolume = incomingCueVolume;
            if (incoming != null) incoming.volume = EffectiveMusicVolume * incomingCueVolume;
            musicTransition = null;
        }

        private void CancelMusicTransition()
        {
            if (musicTransition != null)
            {
                StopCoroutine(musicTransition);
                musicTransition = null;
            }
            if (transitionIncoming != null && transitionIncoming != currentMusic)
            {
                transitionIncoming.Stop();
                transitionIncoming.clip = null;
            }
            transitionIncoming = null;
        }

        private void ApplyVolumes()
        {
            for (int i = 0; i < voices.Count; i++)
                voices[i].source.volume = EffectiveSoundEffectsVolume * voices[i].cueVolume;
            if (currentMusic != null)
                currentMusic.volume = EffectiveMusicVolume * currentMusicCueVolume;
        }
    }
}
