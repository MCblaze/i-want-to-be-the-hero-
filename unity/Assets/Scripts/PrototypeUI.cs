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

        public static void Reset()
        {
            Left = Right = JumpHeld = false;
            jumpPressed = attackPressed = dashPressed = false;
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
            }
        }

        public static bool ConsumeJump() { bool value = jumpPressed; jumpPressed = false; return value; }
        public static bool ConsumeAttack() { bool value = attackPressed; attackPressed = false; return value; }
        public static bool ConsumeDash() { bool value = dashPressed; dashPressed = false; return value; }
    }

    public enum MobileAction { Left, Right, Jump, Attack, Dash }

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
        private GameObject titlePanel;
        private GameObject victoryPanel;
        private GameObject touchRoot;
        private Coroutine messageRoutine;

        private static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
        }

        private void Update()
        {
            if (game.Hero == null) return;
            health.text = $"LOGAN   HP {game.Hero.Health}/{game.Hero.MaxHealth}";
            objective.text = game.CurrentObjective();
            power.text = game.Hero.HasSpark ? "HERO DASH READY" : "FIND YOUR POWER";

            bool bossVisible = game.Boss != null && game.Boss.Awake && game.Boss.IsAlive;
            bossPanel.SetActive(bossVisible);
            if (bossVisible) bossFill.fillAmount = (float)game.Boss.Health / game.Boss.MaxHealth;
        }

        public void ShowGame()
        {
            titlePanel.SetActive(false);
            victoryPanel.SetActive(false);
            touchRoot.SetActive(Application.isMobilePlatform);
            FlashMessage("SUNLEAF RUINS\nFind the Hero Spark", 2f);
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
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -24f), new Vector2(420f, 82f), new Vector2(0f, 1f));
            health = Label("Health", panel.transform, "LOGAN   HP 5/5", 23, Color.white,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -12f), new Vector2(-36f, 28f), TextAnchor.MiddleLeft);
            objective = Label("Objective", panel.transform, "Find the Hero Spark", 17, new Color(1f, .83f, .36f),
                new Vector2(0f, 0f), new Vector2(1f, .55f), new Vector2(18f, 4f), new Vector2(-36f, -2f), TextAnchor.MiddleLeft);
            power = Label("Power", panel.transform, "FIND YOUR POWER", 12, new Color(.62f, .75f, .68f),
                new Vector2(.5f, 0f), new Vector2(1f, .55f), new Vector2(4f, 4f), new Vector2(-18f, -2f), TextAnchor.MiddleRight);

            message = Label("Quest Message", root, "", 30, new Color(1f, .88f, .47f),
                new Vector2(.5f, .74f), new Vector2(.5f, .74f), Vector2.zero, new Vector2(700f, 100f), TextAnchor.MiddleCenter);
            message.gameObject.SetActive(false);

            bossPanel = Panel("Boss HUD", root, new Color(.025f, .13f, .12f, .86f),
                new Vector2(.5f, 1f), new Vector2(.5f, 1f), new Vector2(0f, -28f), new Vector2(520f, 54f), new Vector2(.5f, 1f));
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
            touchRoot.SetActive(false);
        }

        private void BuildTitle(Transform root)
        {
            titlePanel = Panel("Title Panel", root, new Color(.015f, .08f, .075f, .72f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            var card = Panel("Card", titlePanel.transform, new Color(.025f, .16f, .145f, .96f),
                new Vector2(.05f, .12f), new Vector2(.52f, .88f), Vector2.zero, Vector2.zero, new Vector2(.5f, .5f));
            Label("Kicker", card.transform, "LOGAN'S FIRST QUEST", 20, new Color(1f, .83f, .36f),
                new Vector2(.07f, .8f), new Vector2(.93f, .94f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            Label("Title", card.transform, "I WANT TO BE\nTHE HERO", 58, Color.white,
                new Vector2(.07f, .47f), new Vector2(.93f, .82f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            Label("Story", card.transform,
                "The Hero Spark is waiting in the Sunleaf Ruins. Find it, master its power, and prove your courage to the ancient Mossback Guardian.",
                18, new Color(1f, .93f, .75f), new Vector2(.07f, .22f), new Vector2(.93f, .48f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
            var start = Button("Begin the quest", card.transform, new Vector2(.07f, .06f), new Vector2(.62f, .19f));
            start.onClick.AddListener(game.StartQuest);
            Label("Controls", card.transform, "A/D · Space · J/X · Shift/K · Controller supported", 12, new Color(.64f, .76f, .7f),
                new Vector2(.07f, 0f), new Vector2(.93f, .06f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
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
                new Vector2(.08f, .21f), new Vector2(.92f, .49f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            var again = Button("Play again", card.transform, new Vector2(.26f, .06f), new Vector2(.74f, .18f));
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
            obj.GetComponent<Image>().color = color;
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
