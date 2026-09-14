using System.Linq;
using IWantToBeTheHero;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IWantToBeTheHero.Tests
{
    public sealed class MainSceneAuthoringTests
    {
        [Test]
        public void MainScene_ContainsEditableLevelAndSavedCamera()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            var layout = Object.FindAnyObjectByType<MainSceneLayout>();
            Assert.That(scene.rootCount, Is.GreaterThan(0));
            Assert.That(layout, Is.Not.Null);
            Assert.That(layout.mainCamera, Is.Not.Null);
            Assert.That(layout.backdrop, Is.Not.Null);
            Assert.That(layout.heroSpawn, Is.Not.Null);
            Assert.That(layout.sparkGate, Is.Not.Null);
            Assert.That(layout.GetComponentsInChildren<BoxCollider2D>(true).Count(c => !c.isTrigger), Is.EqualTo(17));
            Assert.That(layout.hazards.Count(h => h != null), Is.EqualTo(5));
            Assert.That(layout.checkpoint, Is.Not.Null);
            Assert.That(layout.heroSpark, Is.Not.Null);
            Assert.That(layout.gameObject.scene.path, Is.EqualTo("Assets/Scenes/Main.unity"));
        }
    }
}
