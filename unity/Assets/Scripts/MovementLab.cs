using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class MovementLab : MonoBehaviour
    {
        public HeroTuning tuning;
        public Camera previewCamera;
        public Transform spawn;
        public GameObject heroPreview;
        private HeroGame game;
        private readonly Vector2[] stations = { new(2f, .7f), new(12f, .7f), new(37f, .7f), new(59f, .7f), new(53f, .7f) };

        private void Awake()
        {
            Application.runInBackground = true;
            if (heroPreview != null) heroPreview.SetActive(false);
            game = gameObject.AddComponent<HeroGame>();
        }

        private void Start()
        {
            // Keep the session UI active for its callbacks, but show the lab's compact HUD.
            game.UI.GetComponent<Canvas>().enabled = false;
            game.StartQuest();
            game.Hero.UnlockSpark();
            game.Hero.SetCheckpoint(spawn.position);
            game.MainCamera.GetComponent<CameraFollow>().SnapToHero();
        }

        public void Restart()
        {
            game.StartQuest();
            game.Hero.ResetTraining(spawn.position);
            foreach (var platform in FindObjectsByType<LabMovingPlatform>(FindObjectsSortMode.None)) platform.ResetMotion();
        }

        private void Update()
        {
            for (int i = 0; i < stations.Length; i++)
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) TravelTo(i);
        }

        public void TravelTo(int station)
        {
            if (station < 0 || station >= stations.Length) return;
            game.Hero.ResetTraining(stations[station]);
        }

        private void OnGUI()
        {
            if (game == null || game.Hero == null) return;
            var original = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float viewWidth = Screen.width / scale;
            float bottom = Screen.height / scale - 125f;
            GUI.Box(new Rect(16, 14, 640, 124), "");
            var heading = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
            var text = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            GUI.Label(new Rect(30, 22, 620, 32), "LOGAN  /  MOVEMENT & FEEL LAB", heading);
            string weapon = game.Hero.Weapon == HeroWeapon.Sword ? "SWORD" : "SUNSEED WAND";
            GUI.Label(new Rect(30, 57, 620, 26), $"{weapon}    HP {game.Hero.Health}/5    Air jump: {(game.Hero.AirJumpAvailable ? "ready" : "used")}    Evade: {game.Hero.EvadeCooldown:0.0}s", text);
            GUI.Label(new Rect(30, 87, 620, 30), "A/D move  |  Space jump twice  |  J/X attack  |  E swap  |  R reset", text);
            GUI.Box(new Rect(16, bottom, viewWidth - 32, 109), "");
            GUI.Label(new Rect(30, bottom + 5, viewWidth - 60, 30), "Shift/K: direction + evade = DASH; neutral on ground = BACKFLIP. Controller: A jump, X attack, B evade, Y swap.", text);
            string[] labels = { "1  Runway", "2  Gaps", "3  Platforms", "4  Combat", "5  Ceiling" };
            for (int i = 0; i < labels.Length; i++)
                if (GUI.Button(new Rect(30 + i * (viewWidth - 60) / 5f, bottom + 47, (viewWidth - 60) / 5f - 16, 37), labels[i])) TravelTo(i);
            GUI.matrix = original;
        }
    }
}
