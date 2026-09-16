using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using IWantToBeTheHero.Audio;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    public sealed class AudioFinishTests
    {
        private readonly string[] keys = { "Hero.Audio.Master", "Hero.Audio.SoundEffects", "Hero.Audio.Music" };
        private bool[] existed;
        private float[] values;
        private bool oldRunInBackground;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            values = keys.Select(k => PlayerPrefs.GetFloat(k)).ToArray();
            oldRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (existed != null)
                for (int i = 0; i < keys.Length; i++)
                    if (existed[i]) PlayerPrefs.SetFloat(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            Application.runInBackground = oldRunInBackground;
            if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator MixSettings_AreReloadedByANewDirector()
        {
            var first = new GameObject("Mix persistence first").AddComponent<GameAudioDirector>();
            first.SetMasterVolume(.37f);
            first.SetSoundEffectsVolume(.21f);
            first.SetMusicVolume(.63f);
            first.SaveVolumeSettings();
            Object.Destroy(first.gameObject);
            yield return null;
            var second = new GameObject("Mix persistence second").AddComponent<GameAudioDirector>();
            Assert.That(second.MasterVolume, Is.EqualTo(.37f).Within(.001f));
            Assert.That(second.SoundEffectsVolume, Is.EqualTo(.21f).Within(.001f));
            Assert.That(second.MusicVolume, Is.EqualTo(.63f).Within(.001f));
            Object.Destroy(second.gameObject);
        }

        [UnityTest]
        public IEnumerator VictoryCrossfade_ObeysMuteMixAndDoesNotLoop()
        {
            var game = Object.FindAnyObjectByType<HeroGame>();
            var audio = game.Audio;
            audio.SetMasterVolume(.5f);
            audio.SetMusicVolume(.4f);
            audio.SetMuted(false);
            Assert.That(audio.PlayMusic(AudioCueId.MusicCanopy, 0f), Is.True);
            yield return null;
            yield return null;
            game.CompleteQuest();
            yield return new WaitForSecondsRealtime(.05f);
            audio.SetMuted(true);
            var sources = audio.GetComponentsInChildren<AudioSource>();
            Assert.That(sources.All(s => s.volume == 0f), Is.True, "Both sides of the fade must mute immediately.");
            yield return new WaitForSecondsRealtime(.45f);
            audio.SetMuted(false);
            Assert.That(audio.Library.TryGet(AudioCueId.MusicVictory, out var cue), Is.True);
            var victory = sources.Single(s => s.clip != null && cue.variations.Contains(s.clip));
            Assert.That(victory.loop, Is.False);
            Assert.That(victory.volume, Is.EqualTo(.5f * .4f * cue.volume).Within(.002f));
            Assert.That(sources.Count(s => s.clip != null && s != victory && s.loop), Is.Zero,
                "The outgoing level music should release its clip after the fade.");
        }

        [UnityTest]
        public IEnumerator Footsteps_IgnorePlatformTransportAndEvade()
        {
            var game = Object.FindAnyObjectByType<HeroGame>();
            game.StartQuest();
            var hero = game.Hero;
            hero.enabled = false;
            var body = hero.GetComponent<Rigidbody2D>();
            body.simulated = false;
            var floor = new GameObject("Audio test moving support").AddComponent<Rigidbody2D>();
            floor.bodyType = RigidbodyType2D.Kinematic;
            floor.simulated = false;
            floor.linearVelocity = new Vector2(3f, 0f);
            body.linearVelocity = new Vector2(3f, 0f);
            Set(hero, "support", floor);
            Set(hero, "lastGrounded", Time.time);
            Set(hero, "footstepDistance", .6f);
            Invoke(hero, "UpdateFootsteps");
            Assert.That(Get<float>(hero, "footstepDistance"), Is.Zero,
                "Transport velocity alone must not produce a walking cadence.");
            body.linearVelocity = new Vector2(5f, 0f);
            Invoke(hero, "UpdateFootsteps");
            Assert.That(Get<float>(hero, "footstepDistance"), Is.GreaterThan(0f),
                "Walking relative to the same platform must advance the cadence.");
            Set(hero, "dashRemaining", .2f);
            Invoke(hero, "UpdateFootsteps");
            Assert.That(Get<float>(hero, "footstepDistance"), Is.Zero);
            Object.Destroy(floor.gameObject);
            yield return null;
        }

        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
        private static T Get<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static void Invoke(object target, string name) => target.GetType().GetMethod(name, Private).Invoke(target, null);
    }
}
