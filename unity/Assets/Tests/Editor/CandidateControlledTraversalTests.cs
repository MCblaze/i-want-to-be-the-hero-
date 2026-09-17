using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IWantToBeTheHero.Tests
{
    // Diagnostic input policy, not a human-duration or complete playability test.
    // This deliberately cannot teleport, repair health, grant abilities or force victory.
    public sealed class CandidateControlledTraversalTests
    {
        private const string ScenePath = "Assets/Scenes/Sunleaf_DeliveryCandidate.unity";
        private string evidencePath;
        private bool previousBackground;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            MobileInput.Reset();
            Application.runInBackground = previousBackground;
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator TraverseFromSpawn_UsingOnlyPlayerInputs_ReportFirstBlocker()
        {
            EditorSceneManager.OpenScene(ScenePath);
            yield return new EnterPlayMode();
            previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            MobileInput.Reset();
            var game = Object.FindAnyObjectByType<HeroGame>();
            Assert.That(game, Is.Not.Null);
            game.UI.GetComponentsInChildren<Button>(true).Single(b => b.name == "Begin the quest").onClick.Invoke();
            var hero = game.Hero;
            var body = hero.GetComponent<Rigidbody2D>();
            var capsule = hero.GetComponent<CapsuleCollider2D>();
            evidencePath = Path.GetFullPath("Logs/CandidateTraversal-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(evidencePath);
            var log = new StringBuilder("realSeconds,gameSeconds,x,y,health,spark,weapon,bossHealth,deaths,decision\n");
            float began = Time.realtimeSinceStartup;
            float nextLog = 0f, nextAttack = 0f, nextJump = 0f, jumpRelease = 0f;
            float furthest = hero.transform.position.x;
            float recoveryFurthest = furthest;
            float lastProgress = began;
            Vector2 previousPosition = hero.transform.position;
            int deaths = 0;
            string result = "180-second diagnostic time limit";
            while (Time.realtimeSinceStartup - began < 180f && !game.Won)
            {
                float now = Time.realtimeSinceStartup;
                Vector2 position = hero.transform.position;
                if (Vector2.Distance(previousPosition, position) > 5f)
                {
                    deaths++;
                    // Respawn starts a new recovery segment. Keep the historical
                    // route maximum for reporting, but do not demand that replay
                    // immediately surpass it to count as forward progress.
                    recoveryFurthest = position.x;
                    lastProgress = now;
                    log.AppendLine($"# Respawn observed at {position}; furthest={furthest:F2}");
                    if (deaths >= 3) { result = "Three actual deaths: policy stopped for review"; break; }
                }
                previousPosition = position;
                furthest = Mathf.Max(furthest, position.x);
                if (position.x > recoveryFurthest + .2f)
                {
                    recoveryFurthest = position.x;
                    lastProgress = now;
                }
                bool grounded = HitsSolid(new Vector2(capsule.bounds.center.x, capsule.bounds.min.y + .04f), Vector2.down, .13f, hero);
                bool edgeAhead = grounded && !HitsSolid(new Vector2(position.x + .9f, capsule.bounds.min.y + .1f), Vector2.down, .6f, hero);
                bool wallAhead = HitsSolid(new Vector2(capsule.bounds.center.x, capsule.bounds.center.y), Vector2.right, .85f, hero);
                float move = 1f;
                string decision = "advance";
                HeroTarget threat = Object.FindObjectsByType<Thornling>(FindObjectsSortMode.None)
                    .Where(t => t.IsAlive && Mathf.Abs(t.transform.position.y - position.y) < 1.3f &&
                        t.transform.position.x > position.x - 1.5f && t.transform.position.x < position.x + 6.5f)
                    .OrderBy(t => Mathf.Abs(t.transform.position.x - position.x)).FirstOrDefault();
                if (game.Boss != null && game.Boss.Awake && game.Boss.IsAlive) threat = game.Boss;
                if (threat != null)
                {
                    float dx = threat.transform.position.x - position.x;
                    move = Mathf.Abs(dx) > 6.5f ? Mathf.Sign(dx) : 0f;
                    if (Mathf.Sign(dx) != hero.Facing) move = Mathf.Sign(dx);
                    decision = "combat spacing";
                    if (hero.Weapon != HeroWeapon.SunseedWand) MobileInput.Set(MobileAction.Swap, true);
                    if (Mathf.Abs(dx) < 2f && threat is MossbackGuardian)
                    {
                        move = position.x < 269f ? 1f : position.x > 283f ? -1f : -Mathf.Sign(dx);
                        decision = "boss evade";
                        if (grounded && now >= nextJump) { PressJump(now, ref nextJump, ref jumpRelease); }
                    }
                    if (Mathf.Sign(dx) == hero.Facing && now >= nextAttack)
                    {
                        MobileInput.Set(MobileAction.Attack, true);
                        nextAttack = now + .48f;
                    }
                }
                if (grounded && now >= nextJump && (edgeAhead || (wallAhead && threat == null)))
                {
                    PressJump(now, ref nextJump, ref jumpRelease);
                    decision = edgeAhead ? "jump visible gap" : "jump climbout";
                }
                if (now >= jumpRelease) MobileInput.Set(MobileAction.Jump, false);
                MobileInput.Set(MobileAction.Right, move > .1f);
                MobileInput.Set(MobileAction.Left, move < -.1f);
                if (now >= nextLog)
                {
                    log.AppendLine(FormattableString.Invariant($"{now - began:F2},{game.Elapsed:F2},{position.x:F3},{position.y:F3},{hero.Health},{hero.HasSpark},{hero.Weapon},{(game.Boss == null ? -1 : game.Boss.Health)},{deaths},{decision}"));
                    File.WriteAllText(Path.Combine(evidencePath, "trace.csv"), log.ToString());
                    nextLog = now + .5f;
                }
                if (now - lastProgress > 12f && threat == null)
                {
                    result = "No forward progress for 12 seconds outside combat";
                    break;
                }
                yield return null;
            }
            MobileInput.Reset();
            if (game.Won) result = "Victory reached through normal gameplay inputs";
            File.WriteAllText(Path.Combine(evidencePath, "trace.csv"), log.ToString());
            File.WriteAllText(Path.Combine(evidencePath, "result.txt"),
                $"Result: {result}\nFurthest X: {furthest:F2}\nReal elapsed: {Time.realtimeSinceStartup - began:F2}\nGame elapsed: {game.Elapsed:F2}\nDeaths: {deaths}\nSpark: {hero.HasSpark}\nPosition: {hero.transform.position}\nAutomated diagnostic policy, not human timing or full qualification.\n");
            ScreenCapture.CaptureScreenshot(Path.Combine(evidencePath, "final-frame.png"));
            yield return null;
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(game.Won, Is.True, result + "; furthest x=" + furthest.ToString("F2") + "; evidence=" + evidencePath);
            yield return new ExitPlayMode();
        }

        private static void PressJump(float now, ref float nextJump, ref float release)
        {
            MobileInput.Set(MobileAction.Jump, false);
            MobileInput.Set(MobileAction.Jump, true);
            nextJump = now + .5f;
            release = now + .65f;
        }

        private static bool HitsSolid(Vector2 origin, Vector2 direction, float distance, HeroController hero)
        {
            return Physics2D.RaycastAll(origin, direction, distance).Any(hit =>
                hit.collider != null && !hit.collider.isTrigger && hit.collider.GetComponentInParent<HeroController>() != hero &&
                hit.collider.GetComponentInParent<HeroTarget>() == null);
        }
    }
}
