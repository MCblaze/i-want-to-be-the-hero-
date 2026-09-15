using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace IWantToBeTheHero.Tests
{
    public sealed class AccessibilityTests
    {
        [UnitySetUp]
        public IEnumerator OpenPrototype()
        {
            AccessibilitySettings.Reset();
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            yield return new EnterPlayMode();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            AccessibilitySettings.Reset();
            if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator TitleOffersPersistentReadableControls()
        {
            var labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.That(labels.Any(t => t.text.Contains("F1 effects") && t.text.Contains("F2 audio")), Is.True);
            Assert.That(labels.Any(t => t.text.Contains("EFFECTS FULL") && t.text.Contains("AUDIO ON")), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReducedEffectsStopsOptionalMotionAndParticles()
        {
            AccessibilitySettings.SetReducedEffects(true);
            yield return null;
            Assert.That(AccessibilitySettings.ReducedEffects, Is.True);
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).All(p => !p.isPlaying), Is.True);
        }

        [UnityTest]
        public IEnumerator MutedAudioKeepsVisualStatusAvailable()
        {
            AccessibilitySettings.SetMutedAudio(true);
            Object.FindAnyObjectByType<PrototypeUI>().ShowAccessibilityStatus();
            yield return null;
            Assert.That(AudioListener.volume, Is.Zero);
            Assert.That(Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(t => t.text.Contains("AUDIO: MUTED")), Is.True);
        }
    }
}
