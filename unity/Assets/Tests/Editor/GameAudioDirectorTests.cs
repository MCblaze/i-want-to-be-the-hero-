using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using IWantToBeTheHero.Audio;

namespace IWantToBeTheHero.Tests
{
    public sealed class GameAudioDirectorTests
    {
        private GameObject root;
        private GameAudioDirector director;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Audio Director Test");
            director = root.AddComponent<GameAudioDirector>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void EmptyLibrary_IsSafeAndDoesNotPlay()
        {
            Assert.That(director.TryPlay(AudioCueId.LoganJump), Is.False);
            Assert.That(director.PlayMusic(AudioCueId.MusicCanopy), Is.False);
            Assert.That(director.ActiveVoiceCount, Is.Zero);
        }

        [Test]
        public void VolumeAndMuteApis_ClampAndRetainUserLevels()
        {
            director.SetMasterVolume(2f);
            director.SetSoundEffectsVolume(-1f);
            director.SetMusicVolume(0.35f);
            director.SetMuted(true);

            Assert.That(director.MasterVolume, Is.EqualTo(1f));
            Assert.That(director.SoundEffectsVolume, Is.Zero);
            Assert.That(director.MusicVolume, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(director.IsMuted, Is.True);

            director.SetMuted(false);
            Assert.That(director.IsMuted, Is.False);
            Assert.That(director.MusicVolume, Is.EqualTo(0.35f).Within(0.001f));
        }

        [Test]
        public void Awake_CreatesReusablePoolAndDedicatedMusicSources()
        {
            typeof(GameAudioDirector)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(director, null);

            Assert.That(director.PoolSize, Is.GreaterThanOrEqualTo(1));
            Assert.That(root.GetComponentsInChildren<AudioSource>(true).Length,
                Is.EqualTo(director.PoolSize + 2));
        }

        [TestCase(SurfaceAudioTag.SurfaceKind.Default, AudioCueId.LoganFootstepDefault)]
        [TestCase(SurfaceAudioTag.SurfaceKind.Wood, AudioCueId.LoganFootstepWood)]
        [TestCase(SurfaceAudioTag.SurfaceKind.Stone, AudioCueId.LoganFootstepStone)]
        public void SurfaceKinds_MapToExpectedFootstepCue(SurfaceAudioTag.SurfaceKind surface, AudioCueId expected)
        {
            Assert.That(SurfaceAudioTag.DefaultFootstepFor(surface), Is.EqualTo(expected));
        }
    }
}
