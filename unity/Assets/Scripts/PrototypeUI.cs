using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IWantToBeTheHero
{
    public static class MobileInput
    {
        public static bool Left;
        public static bool Right;
        public static bool JumpHeld;
        private static bool jumpPressed;
        private static bool attackPressed;
        private static bool dashPressed;
        private static bool swapPressed;

        public static void Reset()
        {
            Left = Right = JumpHeld = false;
            jumpPressed = attackPressed = dashPressed = swapPressed = false;
        }

        public static void Set(MobileAction action, bool pressed)
        {
            switch (action)
            {
                case MobileAction.Left: Left = pressed; break;
                case MobileAction.Right: Right = pressed; break;
                case MobileAction.Jump:
                    if (pressed && !JumpHeld) jumpPressed = true;
                    JumpHeld = pressed;
                    break;
                case MobileAction.Attack:
                    if (pressed) attackPressed = true;
                    break;
                case MobileAction.Dash:
                    if (pressed) dashPressed = true;
                    break;
                case MobileAction.Swap:
                    if (pressed) swapPressed = true;
                    break;
            }
        }

        public static bool ConsumeJump() { bool value = jumpPressed; jumpPressed = false; return value; }
        public static bool ConsumeAttack() { bool value = attackPressed; attackPressed = false; return value; }
        public static bool ConsumeDash() { bool value = dashPressed; dashPressed = false; return value; }
        public static bool ConsumeSwap() { bool value = swapPressed; swapPressed = false; return value; }
    }

    public enum MobileAction { Left, Right, Jump, Attack, Dash, Swap }

    public sealed class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public MobileAction Action;
        private Image image;
        private Color normal;

        private void Start()
        {
            image = GetComponent<Image>();
            normal = image.color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            MobileInput.Set(Action, true);
            image.color = new Color(0.91f, .36f, .25f, .88f);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        private void Release()
        {
            MobileInput.Set(Action, false);
            image.color = normal;
        }
    }

    public sealed class PrototypeUI : MonoBehaviour
    {
        private HeroGame game;
        private Text health;
        private Text objective;
        private Text power;
        private Text message;
        private Text bossName;
        private Image bossFill;
        private GameObject bossPanel;
        private GameObject hudPanel;
        private GameObject titlePanel;
        private GameObject victoryPanel;
        private GameObject pausePanel;
        private Button resumeButton;
        private GameObject touchRoot;
        private Text accessibilityStatus;
        private Coroutine messageRoutine;

        private static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        private static Sprite PremiumPanelSprite => Resources.Load<Sprite>("Art/UI/SunleafPanel");

        public static PrototypeUI Create(HeroGame game)
        {
            var eventSystem = new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(game.transform);

            var canvasObject = new GameObject("Prototype UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(game.transform);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = .5f;

            var ui = canvasObject.AddComponent<PrototypeUI>();
            ui.game = game;
            ui.Build(canvasObject.transform);
            return ui;
        }

        private void Build(Transform root)
        {
            BuildHud(root);
            BuildTouchControls(root);
            BuildTitle(root);
            BuildVictory(root);
            BuildPause(root);
        }

        private void Update()
        {
            if (game.Hero == null) return;
            hudPanel.SetActive(game.Started && !game.Won);
            health.text = $"LOGAN   HP {game.Hero.Health}/{game.Hero.MaxHealth}";
            objective.text = game.CurrentObjective();
            power.text = (game.Hero.Weapon == HeroWeapon.Sword ? "SWORD" : "WAND") +
                (game.Hero.HasSpark
                    ? (game.Hero.EvadeCooldown > 0f ? " · EVADE RECOVERING" : " · EVADE READY")
                    : " · E TO SWAP");

            bool bossVisible = game.Started && !game.Won && game.Boss != null && game.Boss.Awake && game.Boss.IsAlive;
            bossPanel.SetActive(bossVisible);
            if (bossVisible) bossFill.fillAmount = (float)game.Boss.Health / game.Boss.MaxHealth;
        }

        public void ShowGame()
        {
            pausePanel.SetActive(false);
            titlePanel.SetActive(false);
            victoryPanel.SetActive(false);
            touchRoot.SetActive(Application.isMobilePlatform);
            FlashMessage("SUNLEAF RUINS\nFind the Hero Spark", 2f);
        }

        public void ShowPause(bool paused)
        {
            pausePanel.SetActive(paused);
            touchRoot.SetActive(!paused && Application.isMobilePlatform);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(paused ? resumeButton.gameObject : null);
        }

        public void ShowVictory(float elapsed)
        {
            touchRoot.SetActive(false);
            StartCoroutine(RevealVictory());
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            victoryPanel.transform.Find("Card/Time").GetComponent<Text>().text =
                $"Logan found the Hero Spark and proved his courage in {minutes}:{seconds:00}.\n\nBeing a hero was never about being the strongest — it was about trying again.";
        }

        private IEnumerator RevealVictory()
        {
            // Leave the Guardian's kneel and Logan's celebration visible first.
            yield return new WaitForSeconds(1.8f);
            victoryPanel.SetActive(true);
        }

        public void FlashMessage(string text, float duration)
        {
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(MessageRoutine(text, duration));
        }

        public void ShowAccessibilityStatus()
        {
            UpdateAccessibilityStatus();
            FlashMessage($"REDUCED EFFECTS: {(AccessibilitySettings.ReducedEffects ? "ON" : "OFF")}\nAUDIO: {(AccessibilitySettings.MutedAudio ? "MUTED" : "ON")}", 2f);
        }

        private IEnumerator MessageRoutine(string text, float duration)
        {
            message.text = text;
            message.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            message.gameObject.SetActive(false);
        }

        private void BuildHud(Transform root)
        {
            var panel = Panel("HUD", root, new Color(.025f, .13f, .12f, .8f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -24f), new Vector2(440f, 112f), new Vector2(0f, 1f));
            hudPanel = panel;
            hudPanel.SetActive(false);
            health = Label("Health", panel.transform, "LOGAN   HP 5/5", 23, Color.white,
                new Vector2(.04f, .66f), new Vector2(.96f, .95f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            objective = Label("Objective", panel.transform, "Find the Hero Spark", 17, new Color(1f, .83f, .36f),
                new Vector2(.04f, .34f), new Vector2(.96f, .64f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            power = Label("Power", panel.transform, "FIND YOUR POWER", 14, new Color(.76f, .86f, .80f),
                new Vector2(.04f, .07f), new Vector2(.96f, .30f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);

            message = Label("Quest Message", root, "", 30, new Color(1f, .88f, .47f),
                new Vector2(.5f, .74f), new Vector2(.5f, .74f), Vector2.zero, new Vector2(700f, 100f), TextAnchor.MiddleCenter);
            message.gameObject.SetActive(false);

            bossPanel = Panel("Boss HUD", root, new Color(.025f, .13f, .12f, .86f),
                new Vector2(.56f, .88f), new Vector2(.98f, .97f), Vector2.zero, Vector2.zero, new Vector2(.5f, 1f));
            bossName = Label("Boss Name", bossPanel.transform, "MOSSBACK GUARDIAN", 16, Color.white,
                new Vector2(0f, .48f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            var bossBack = Panel("Boss Bar", bossPanel.transform, new Color(.18f, .24f, .2f),
                new Vector2(.04f, .12f), new Vector2(.96f, .33f), Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            var fillObject = Panel("Fill", bossBack.transform, new Color(.72f, .82f, .31f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0f, .5f));
            bossFill = fillObject.GetComponent<Image>();
            bossFill.type = Image.Type.Filled;
            bossFill.fillMethod = Image.FillMethod.Horizontal;
            bossPanel.SetActive(false);
        }

        private void BuildTouchControls(Transform root)
        {
            touchRoot = new GameObject("Touch Controls", typeof(RectTransform));
            touchRoot.transform.SetParent(root, false);
            Stretch(touchRoot.GetComponent<RectTransform>());

            TouchButton(touchRoot.transform, "Left", "◀", MobileAction.Left, new Vector2(88f, 90f), new Vector2(88f, 88f), new Color(.05f, .22f, .21f, .62f));
            TouchButton(touchRoot.transform, "Right", "▶", MobileAction.Right, new Vector2(184f, 90f), new Vector2(88f, 88f), new Color(.05f, .22f, .21f, .62f));
            TouchButton(touchRoot.transform, "Dash", "DASH", MobileAction.Dash, new Vector2(-270f, 86f), new Vector2(80f, 80f), new Color(.1f, .42f, .4f, .66f), true);
            TouchButton(touchRoot.transform, "Attack", "HIT", MobileAction.Attack, new Vector2(-173f, 100f), new Vector2(92f, 92f), new Color(.63f, .39f, .12f, .68f), true);
            TouchButton(touchRoot.transform, "Jump", "JUMP", MobileAction.Jump, new Vector2(-70f, 120f), new Vector2(112f, 112f), new Color(.78f, .27f, .18f, .72f), true);
            TouchButton(touchRoot.transform, "Swap", "SWAP", MobileAction.Swap, new Vector2(-270f, 184f), new Vector2(80f, 62f), new Color(.1f, .42f, .4f, .66f), true);
            var pause = Button("Pause", touchRoot.transform, new Vector2(.45f, .03f), new Vector2(.55f, .10f));
            pause.onClick.AddListener(() => game.SetPaused(true));
            touchRoot.SetActive(false);
        }

        private void BuildTitle(Transform root)
        {
            titlePanel = Panel("Title Panel", root, new Color(.015f, .08f, .075f, .72f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            var card = Panel("Card", titlePanel.transform, new Color(.025f, .16f, .145f, .96f),
                new Vector2(.06f, .08f), new Vector2(.64f, .92f), Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            Label("Kicker", card.transform, "LOGAN'S FIRST QUEST", 20, new Color(1f, .83f, .36f),
                new Vector2(.10f, .84f), new Vector2(.90f, .92f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            Label("Title", card.transform, "I WANT TO BE\nTHE HERO", 54, Color.white,
                new Vector2(.10f, .61f), new Vector2(.90f, .83f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            Label("Story", card.transform,
                "The Hero Spark is waiting in the Sunleaf Ruins. Find it, master its power, and prove your courage to the ancient Mossback Guardian.",
                18, new Color(1f, .93f, .75f), new Vector2(.10f, .45f), new Vector2(.90f, .60f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            var start = Button("Begin the quest", card.transform, new Vector2(.10f, .32f), new Vector2(.90f, .42f));
            start.onClick.AddListener(game.StartQuest);
            var reduced = Button("Reduced effects", card.transform, new Vector2(.10f, .23f), new Vector2(.48f, .30f));
            reduced.onClick.AddListener(() => { AccessibilitySettings.SetReducedEffects(!AccessibilitySettings.ReducedEffects); ShowAccessibilityStatus(); });
            var mute = Button("Mute audio", card.transform, new Vector2(.52f, .23f), new Vector2(.90f, .30f));
            mute.onClick.AddListener(() => { AccessibilitySettings.SetMutedAudio(!AccessibilitySettings.MutedAudio); ShowAccessibilityStatus(); });
            accessibilityStatus = Label("Accessibility Status", card.transform, "", 13, new Color(.8f, .9f, .82f),
                new Vector2(.10f, .18f), new Vector2(.90f, .22f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            UpdateAccessibilityStatus();
            Label("Controls", card.transform, "A/D · Space jump · J/X attack · E swap · Shift/K evade\nEsc pause · F1 effects · F2 audio", 14, new Color(.76f, .86f, .80f),
                new Vector2(.10f, .10f), new Vector2(.90f, .18f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
        }

        private void UpdateAccessibilityStatus()
        {
            if (accessibilityStatus == null) return;
            accessibilityStatus.text = $"EFFECTS {(AccessibilitySettings.ReducedEffects ? "LOW" : "FULL")} · AUDIO {(AccessibilitySettings.MutedAudio ? "MUTED" : "ON")}";
        }

        private void BuildPause(Transform root)
        {
            pausePanel = Panel("Pause Panel", root, new Color(.015f, .08f, .075f, .86f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            var card = Panel("Card", pausePanel.transform, new Color(.025f, .16f, .145f, .98f),
                new Vector2(.29f, .18f), new Vector2(.71f, .82f), Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            Label("Title", card.transform, "TAKE A BREATH", 32, new Color(1f, .83f, .36f),
                new Vector2(.15f, .69f), new Vector2(.85f, .84f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            Label("Help", card.transform, "E swaps sword and wand.\nAfter the Spark: jump twice, or evade while standing still to backflip.\nF1 reduced effects · F2 mute", 16, new Color(1f, .93f, .75f),
                new Vector2(.17f, .44f), new Vector2(.83f, .67f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            resumeButton = Button("Resume", card.transform, new Vector2(.18f, .29f), new Vector2(.82f, .40f));
            resumeButton.onClick.AddListener(() => game.SetPaused(false));
            var restart = Button("Restart quest", card.transform, new Vector2(.18f, .14f), new Vector2(.82f, .25f));
            restart.onClick.AddListener(game.ResetQuest);
            pausePanel.SetActive(false);
        }

        private void BuildVictory(Transform root)
        {
            victoryPanel = Panel("Victory Panel", root, new Color(.015f, .08f, .075f, .78f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            var card = Panel("Card", victoryPanel.transform, new Color(.025f, .16f, .145f, .97f),
                new Vector2(.28f, .19f), new Vector2(.72f, .81f), Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            Label("Kicker", card.transform, "QUEST COMPLETE", 18, new Color(1f, .83f, .36f),
                new Vector2(.08f, .77f), new Vector2(.92f, .94f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            Label("Title", card.transform, "HERO OF\nSUNLEAF!", 48, new Color(1f, .83f, .36f),
                new Vector2(.08f, .49f), new Vector2(.92f, .78f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            Label("Time", card.transform, "", 17, new Color(1f, .93f, .75f),
                new Vector2(.10f, .29f), new Vector2(.90f, .49f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            var again = Button("Play again", card.transform, new Vector2(.26f, .13f), new Vector2(.74f, .25f));
            again.onClick.AddListener(game.ResetQuest);
            victoryPanel.SetActive(false);
        }

        private static GameObject Panel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchored, Vector2 size, Vector2 pivot)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
            var image = obj.GetComponent<Image>();
            if (name == "Card" && PremiumPanelSprite != null)
            {
                image.sprite = PremiumPanelSprite;
                image.color = Color.white;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 4f;
            }
            else image.color = color;
            return obj;
        }

        private static Text Label(string name, Transform parent, string content, int fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchored, Vector2 size, TextAnchor alignment)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
            var text = obj.GetComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button Button(string label, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var obj = Panel(label, parent, new Color(.85f, .29f, .2f), anchorMin, anchorMax, Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            var button = obj.AddComponent<Button>();
            Label("Label", obj.transform, label.ToUpperInvariant(), 18, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            return button;
        }

        private static void TouchButton(Transform parent, string name, string label, MobileAction action, Vector2 anchored, Vector2 size, Color color, bool right = false)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TouchHoldButton));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = right ? Vector2.right : Vector2.zero;
            rect.anchorMax = right ? Vector2.right : Vector2.zero;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = color;
            obj.GetComponent<TouchHoldButton>().Action = action;
            Label("Label", obj.transform, label, 17, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
