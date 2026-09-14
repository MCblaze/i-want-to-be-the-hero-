using System;
using System.Collections.Generic;
using UnityEngine;

namespace IWantToBeTheHero
{
    public static class PrototypeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildPrototype()
        {
            if (UnityEngine.Object.FindAnyObjectByType<HeroGame>() != null)
                return;

            var root = new GameObject("I Want to Be the Hero - Runtime Level");
            root.AddComponent<HeroGame>();
        }
    }

    public sealed class HeroGame : MonoBehaviour
    {
        public HeroController Hero { get; private set; }
        public MossbackGuardian Boss { get; private set; }
        public PrototypeUI UI { get; private set; }
        public Camera MainCamera { get; private set; }
        public bool Started { get; private set; }
        public bool Won { get; private set; }
        public float Elapsed { get; private set; }

        private GameObject sparkGate;
        private readonly List<HeroTarget> targets = new();

        private static readonly Color Stone = new(0.13f, 0.22f, 0.18f);
        private static readonly Color Grass = new(0.49f, 0.66f, 0.25f);
        private static readonly Color PlatformTop = new(0.68f, 0.78f, 0.31f);

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Physics2D.gravity = new Vector2(0f, -24f);
            BuildCameraAndBackdrop();
            BuildWorld();
            BuildActors();
            UI = PrototypeUI.Create(this);
        }

        private void Update()
        {
            if (Started && !Won)
                Elapsed += Time.deltaTime;

            if (!Started && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
                StartQuest();

            if (Started && !Won && Input.GetKeyDown(KeyCode.R))
                ResetQuest();
        }

        public void StartQuest()
        {
            Started = true;
            Won = false;
            Elapsed = 0f;
            UI.ShowGame();
        }

        public void ResetQuest()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void RegisterTarget(HeroTarget target)
        {
            if (!targets.Contains(target)) targets.Add(target);
        }

        public void UnregisterTarget(HeroTarget target) => targets.Remove(target);

        public void HeroAttack(Bounds hitBounds, int damage, Vector2 force, HashSet<HeroTarget> hitTargets = null)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                HeroTarget target = targets[i];
                if (target == null)
                {
                    targets.RemoveAt(i);
                    continue;
                }

                if (target.IsAlive && hitBounds.Intersects(target.HitBounds) && (hitTargets == null || !hitTargets.Contains(target)))
                {
                    if (hitTargets != null) hitTargets.Add(target);
                    target.TakeHit(damage, force);
                }
            }
        }

        public void CollectHeroSpark()
        {
            Hero.UnlockSpark();
            if (sparkGate != null) Destroy(sparkGate);
            UI.FlashMessage("HERO SPARK FOUND!\nDash unlocked · Shift / K / B button", 2.6f);
        }

        public string CurrentObjective()
        {
            if (!Hero.HasSpark) return "Find the Hero Spark";
            if (!Boss.Awake) return "Dash onward to the ancient gate";
            return "Defeat the Mossback Guardian";
        }

        public void CompleteQuest()
        {
            if (Won) return;
            Won = true;
            UI.ShowVictory(Elapsed);
        }

        private void BuildCameraAndBackdrop()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            MainCamera = cameraObject.AddComponent<Camera>();
            MainCamera.orthographic = true;
            MainCamera.orthographicSize = 5.4f;
            MainCamera.backgroundColor = new Color(0.18f, 0.42f, 0.43f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(9.5f, 0f, -10f);

            var background = new GameObject("Sunleaf Ruins Backdrop");
            var renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeArt.Load("Art/sunleaf-ruins", 11.5f);
            renderer.sortingOrder = -100;
            background.transform.position = new Vector3(9.5f, 0f, 5f);
            var follow = background.AddComponent<BackdropFollow>();
            follow.Target = MainCamera.transform;

            var followCamera = cameraObject.AddComponent<CameraFollow>();
            followCamera.Game = this;
        }

        private void BuildWorld()
        {
            CreatePlatform(3.8f, -4.1f, 7.6f, 1.4f);
            CreatePlatform(11.6f, -4.1f, 6.2f, 1.4f);
            CreatePlatform(18.95f, -4.1f, 6.9f, 1.4f);
            CreatePlatform(27.65f, -4.1f, 8.5f, 1.4f);
            CreatePlatform(36.3f, -4.1f, 7f, 1.4f);
            CreatePlatform(45.8f, -4.1f, 10.4f, 1.4f);

            CreatePlatform(5.55f, -2.45f, 1.7f, .28f);
            CreatePlatform(10.25f, -2.45f, 1.9f, .28f);
            CreatePlatform(12.7f, -1.25f, 1.6f, .28f);
            CreatePlatform(17.35f, -2.45f, 1.7f, .28f);
            CreatePlatform(19.9f, -1.55f, 1.8f, .28f);
            CreatePlatform(25.85f, -2.35f, 2.3f, .28f);
            CreatePlatform(28.7f, -1.2f, 1.8f, .28f);
            CreatePlatform(31f, -2.35f, 1.6f, .28f);
            CreatePlatform(34.8f, -2.3f, 2f, .28f);
            CreatePlatform(37.45f, -1.1f, 1.7f, .28f);

            CreateSpikes(7.05f, -3.35f, .55f);
            CreateSpikes(14.15f, -3.35f, .7f);
            CreateSpikes(21.95f, -3.35f, .65f);
            CreateSpikes(31.55f, -3.35f, .55f);
            CreateSpikes(39.45f, -3.35f, .5f);

            CreateCheckpoint(new Vector2(24.55f, -3.05f));
            CreateHeroSpark(new Vector2(31f, -1.65f));

            sparkGate = CreateBlock("Hero Spark Gate", new Vector2(40.25f, -1.2f), new Vector2(.3f, 4.6f), new Color(0.37f, 0.55f, 0.39f), true);
            sparkGate.GetComponent<SpriteRenderer>().color = new Color(0.45f, 0.72f, 0.45f, .78f);
        }

        private void BuildActors()
        {
            var heroObject = new GameObject("Logan");
            heroObject.transform.position = new Vector3(1.2f, -2.8f, 0f);
            var heroRenderer = heroObject.AddComponent<SpriteRenderer>();
            heroRenderer.sprite = PrototypeArt.Load("Art/logan", 1.62f);
            heroRenderer.sortingOrder = 20;
            var heroBody = heroObject.AddComponent<Rigidbody2D>();
            heroBody.gravityScale = 1f;
            heroBody.freezeRotation = true;
            heroBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            heroBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var heroCollider = heroObject.AddComponent<CapsuleCollider2D>();
            heroCollider.size = new Vector2(.52f, 1.15f);
            heroCollider.offset = new Vector2(0f, -.03f);
            Hero = heroObject.AddComponent<HeroController>();
            Hero.Game = this;

            CreateThornling(5.8f, 4.4f, 7f);
            CreateThornling(10.6f, 8.8f, 13.8f);
            CreateThornling(17.4f, 15.7f, 21.8f);
            CreateThornling(26.2f, 23.8f, 30.8f);
            CreateThornling(34.8f, 33f, 38.9f);

            var bossObject = new GameObject("Mossback Guardian");
            bossObject.transform.position = new Vector3(45.4f, -2.05f, 0f);
            var bossRenderer = bossObject.AddComponent<SpriteRenderer>();
            bossRenderer.sprite = PrototypeArt.Load("Art/mossback-guardian", 3.15f);
            bossRenderer.sortingOrder = 15;
            var bossBody = bossObject.AddComponent<Rigidbody2D>();
            bossBody.gravityScale = 1f;
            bossBody.freezeRotation = true;
            bossBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            var bossCollider = bossObject.AddComponent<BoxCollider2D>();
            bossCollider.size = new Vector2(1.9f, 2.25f);
            bossCollider.offset = new Vector2(0f, -.18f);
            Boss = bossObject.AddComponent<MossbackGuardian>();
            Boss.Game = this;
            RegisterTarget(Boss);
        }

        private void CreateThornling(float x, float minX, float maxX)
        {
            var enemyObject = new GameObject("Thornling");
            enemyObject.transform.position = new Vector3(x, -2.9f, 0f);
            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeArt.Load("Art/thornling", 1.15f);
            renderer.sortingOrder = 12;
            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = enemyObject.AddComponent<CircleCollider2D>();
            collider.radius = .42f;
            collider.offset = new Vector2(0f, -.1f);
            var thornling = enemyObject.AddComponent<Thornling>();
            thornling.Game = this;
            thornling.MinX = minX;
            thornling.MaxX = maxX;
            RegisterTarget(thornling);
        }

        private void CreatePlatform(float x, float y, float width, float height)
        {
            CreateBlock("Mossy Platform", new Vector2(x, y), new Vector2(width, height), Stone, true);
            CreateBlock("Grass Edge", new Vector2(x, y + height * .5f - .045f), new Vector2(width, .09f), height > .5f ? Grass : PlatformTop, false);
        }

        private void CreateSpikes(float x, float y, float width)
        {
            var spikes = CreateBlock("Soft Stone Spikes", new Vector2(x, y), new Vector2(width, .28f), new Color(0.22f, 0.19f, 0.18f), false);
            var trigger = spikes.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            spikes.AddComponent<Hazard>().Game = this;
        }

        private void CreateCheckpoint(Vector2 position)
        {
            var checkpoint = CreateBlock("Checkpoint Flag", position, new Vector2(.18f, 1.2f), new Color(0.38f, 0.24f, 0.15f), false);
            var trigger = checkpoint.AddComponent<BoxCollider2D>();
            trigger.size = new Vector2(1f, 1.7f);
            trigger.isTrigger = true;
            var component = checkpoint.AddComponent<Checkpoint>();
            component.Game = this;
            component.RespawnPosition = position + new Vector2(-.6f, .25f);
        }

        private void CreateHeroSpark(Vector2 position)
        {
            var objectRoot = new GameObject("Hero Spark");
            objectRoot.transform.position = position;
            var renderer = objectRoot.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeArt.Star();
            renderer.color = new Color(1f, .82f, .3f);
            renderer.sortingOrder = 18;
            var trigger = objectRoot.AddComponent<CircleCollider2D>();
            trigger.radius = .55f;
            trigger.isTrigger = true;
            var spark = objectRoot.AddComponent<HeroSpark>();
            spark.Game = this;
        }

        private static GameObject CreateBlock(string name, Vector2 position, Vector2 size, Color color, bool collider)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            block.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = PrototypeArt.WhiteSquare();
            renderer.color = color;
            renderer.sortingOrder = 5;
            if (collider) block.AddComponent<BoxCollider2D>();
            return block;
        }
    }

    public static class PrototypeArt
    {
        private static Sprite white;
        private static Sprite star;
        private static readonly Dictionary<string, Sprite> Loaded = new();

        public static Sprite WhiteSquare()
        {
            if (white != null) return white;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "Runtime White Pixel";
            texture.filterMode = FilterMode.Point;
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            white = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
            return white;
        }

        public static Sprite Star()
        {
            if (star != null) return star;
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Runtime Hero Star";
            texture.filterMode = FilterMode.Point;
            var clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - 7.5f);
                float dy = Mathf.Abs(y - 7.5f);
                bool inside = dx + dy * .52f < 4.2f || dy + dx * .52f < 4.2f;
                texture.SetPixel(x, y, inside ? Color.white : clear);
            }
            texture.Apply();
            star = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 16f);
            return star;
        }

        public static Sprite Load(string resourcePath, float desiredHeight)
        {
            if (Loaded.TryGetValue(resourcePath, out Sprite cached)) return cached;
            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return WhiteSquare();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            float pixelsPerUnit = texture.height / desiredHeight;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), pixelsPerUnit);
            Loaded[resourcePath] = sprite;
            return sprite;
        }
    }

    public abstract class HeroTarget : MonoBehaviour
    {
        public HeroGame Game { get; set; }
        public abstract bool IsAlive { get; }
        public Bounds HitBounds => GetComponent<Collider2D>().bounds;
        public abstract void TakeHit(int damage, Vector2 force);

        protected virtual void OnDestroy()
        {
            if (Game != null) Game.UnregisterTarget(this);
        }
    }

    public sealed class HeroController : MonoBehaviour
    {
        public HeroGame Game { get; set; }
        public bool HasSpark { get; private set; }
        public int Health { get; private set; } = 5;
        public int MaxHealth => 5;

        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private PixelSpriteAnimator visual;
        private float facing = 1f;
        private float lastGrounded = -10f;
        private float lastJumpPressed = -10f;
        private float attackCooldown;
        private float invulnerable;
        private float dashCooldown;
        private float dashRemaining;
        private float attackRemaining;
        private float hurtRemaining;
        private float landingRemaining;
        private float ghostCooldown;
        private const float AttackDuration = .32f;
        private readonly HashSet<HeroTarget> attackHits = new HashSet<HeroTarget>();
        private Vector2 respawn = new(1.2f, -2.8f);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = PixelSpriteAnimator.Attach(gameObject, "logan");
            sprite = visual.Renderer;
        }

        private void Update()
        {
            if (Game == null || !Game.Started || Game.Won) return;

            attackCooldown -= Time.deltaTime;
            invulnerable -= Time.deltaTime;
            dashCooldown -= Time.deltaTime;
            attackRemaining = Mathf.Max(0f, attackRemaining - Time.deltaTime);
            hurtRemaining = Mathf.Max(0f, hurtRemaining - Time.deltaTime);
            landingRemaining = Mathf.Max(0f, landingRemaining - Time.deltaTime);
            ghostCooldown -= Time.deltaTime;

            if (JumpPressed()) lastJumpPressed = Time.time;

            if (AttackPressed() && attackCooldown <= 0f && dashRemaining <= 0f && hurtRemaining <= 0f)
            {
                attackCooldown = AttackDuration;
                attackRemaining = AttackDuration;
                attackHits.Clear();
            }

            if (DashPressed() && HasSpark && dashCooldown <= 0f && hurtRemaining <= 0f)
            {
                dashCooldown = .65f;
                dashRemaining = .17f;
                attackRemaining = 0f;
                ghostCooldown = 0f;
            }

            // Wind-up is frame 1; only frames 2 and the beginning of 3 deal damage.
            float attackAge = AttackDuration - attackRemaining;
            if (attackRemaining > 0f && attackAge >= .08f && attackAge < .20f)
            {
                var center = transform.position + Vector3.right * facing * .72f;
                Game.HeroAttack(new Bounds(center, new Vector3(1.15f, 1.05f, 1f)),
                    HasSpark ? 2 : 1, new Vector2(facing * 4.5f, 2.2f), attackHits);
            }

            if (!JumpHeld() && body.linearVelocity.y > 4f)
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * .55f);

            if (transform.position.y < -8f) Respawn();
        }

        private void FixedUpdate()
        {
            if (Game == null || !Game.Started || Game.Won)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            if (dashRemaining > 0f)
            {
                dashRemaining -= Time.fixedDeltaTime;
                body.linearVelocity = new Vector2(facing * 12.5f, 0f);
                return;
            }

            float horizontal = HorizontalInput();
            if (Mathf.Abs(horizontal) > .05f && attackRemaining <= 0f && hurtRemaining <= 0f)
            {
                facing = Mathf.Sign(horizontal);
            }

            float targetSpeed = horizontal * 5.2f;
            float acceleration = IsGrounded ? 42f : 27f;
            body.linearVelocity = new Vector2(
                Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime),
                Mathf.Max(body.linearVelocity.y, -15f));

            if (Time.time - lastJumpPressed <= .14f && Time.time - lastGrounded <= .12f)
            {
                lastJumpPressed = -10f;
                lastGrounded = -10f;
                body.linearVelocity = new Vector2(body.linearVelocity.x, 10.6f);
            }
        }

        private bool IsGrounded => Time.time - lastGrounded < .09f && body.linearVelocity.y < .5f;

        private void LateUpdate()
        {
            if (visual == null || Game == null) return;
            visual.Face(facing);
            if (Game.Won) { visual.Tick("victory", Time.deltaTime); sprite.color = Color.white; return; }
            if (!Game.Started) { visual.Tick("idle", Time.deltaTime); return; }
            if (hurtRemaining > 0f) visual.Sample("hurt", .2f - hurtRemaining);
            else if (dashRemaining > 0f) visual.Sample("dash", .17f - dashRemaining);
            else if (attackRemaining > 0f) visual.Sample("attack", AttackDuration - attackRemaining);
            else if (!IsGrounded)
                visual.Tick(Mathf.Abs(body.linearVelocity.y) < 1f ? "apex" : body.linearVelocity.y > 0f ? "rise" : "fall", Time.deltaTime);
            else if (landingRemaining > 0f) visual.Tick("land", Time.deltaTime);
            else visual.Tick(Mathf.Abs(body.linearVelocity.x) > .2f ? "run" : "idle", Time.deltaTime,
                Mathf.Abs(body.linearVelocity.x) > .2f ? Mathf.Max(.5f, Mathf.Abs(body.linearVelocity.x) / 5.2f) : 1f);
            sprite.color = new Color(1f, 1f, 1f, invulnerable > 0f && Mathf.FloorToInt(invulnerable * 18f) % 2 == 0 ? .4f : 1f);
            if (dashRemaining > 0f && ghostCooldown <= 0f)
            {
                visual.Ghost();
                ghostCooldown = .04f;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > .55f)
                {
                    if (Time.time - lastGrounded > .12f) landingRemaining = .1f;
                    lastGrounded = Time.time;
                    return;
                }
            }
        }

        public void UnlockSpark()
        {
            HasSpark = true;
            Health = MaxHealth;
        }

        public void SetCheckpoint(Vector2 position)
        {
            respawn = position;
            Health = MaxHealth;
        }

        public void Hurt(Vector2 source)
        {
            if (Game == null || !Game.Started || invulnerable > 0f || dashRemaining > 0f || Game.Won) return;
            Health--;
            invulnerable = 1.05f;
            hurtRemaining = .2f;
            attackRemaining = 0f;
            float push = transform.position.x < source.x ? -6f : 6f;
            body.linearVelocity = new Vector2(push, 6f);
            if (Health <= 0) Invoke(nameof(Respawn), .3f);
        }

        private void Respawn()
        {
            CancelInvoke(nameof(Respawn));
            transform.position = respawn;
            body.linearVelocity = Vector2.zero;
            Health = MaxHealth;
            invulnerable = 1.2f;
            attackRemaining = dashRemaining = hurtRemaining = landingRemaining = 0f;
            lastGrounded = lastJumpPressed = -10f;
            attackHits.Clear();
            visual.Sample("idle", 0f);
        }

        private static float HorizontalInput()
        {
            float keyboard = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || MobileInput.Left) keyboard -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || MobileInput.Right) keyboard += 1f;
            float gamepad = 0f;
            try { gamepad = Input.GetAxisRaw("Horizontal"); } catch { /* Project still works with digital input. */ }
            return Mathf.Clamp(keyboard + gamepad, -1f, 1f);
        }

        private static bool JumpPressed() =>
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.JoystickButton0) || MobileInput.ConsumeJump();

        private static bool JumpHeld() =>
            Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.JoystickButton0) || MobileInput.JumpHeld;

        private static bool AttackPressed() =>
            Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.X) ||
            Input.GetKeyDown(KeyCode.JoystickButton2) || MobileInput.ConsumeAttack();

        private static bool DashPressed() =>
            Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.K) ||
            Input.GetKeyDown(KeyCode.JoystickButton1) || MobileInput.ConsumeDash();
    }

    public sealed class Thornling : HeroTarget
    {
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public override bool IsAlive => health > 0;

        private int health = 2;
        private float direction = 1f;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private PixelSpriteAnimator visual;
        private float hurtRemaining;
        private float contactRemaining;
        private float deathTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = PixelSpriteAnimator.Attach(gameObject, "thornling");
            sprite = visual.Renderer;
        }

        private void LateUpdate()
        {
            hurtRemaining = Mathf.Max(0f, hurtRemaining - Time.deltaTime);
            contactRemaining = Mathf.Max(0f, contactRemaining - Time.deltaTime);
            visual.Face(direction);
            if (!IsAlive)
            {
                deathTime += Time.deltaTime;
                visual.Sample("defeat", deathTime);
                sprite.color = new Color(1f, 1f, 1f, Mathf.Clamp01((.75f - deathTime) / .25f));
            }
            else if (hurtRemaining > 0f) visual.Sample("hurt", .2f - hurtRemaining);
            else if (contactRemaining > 0f) visual.Sample("attack", 1f / 3f - contactRemaining);
            else visual.Tick(Game != null && Game.Started && !Game.Won ? "run" : "idle", Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!IsAlive || Game == null || !Game.Started || Game.Won)
            {
                if (body != null) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if (transform.position.x <= MinX) direction = 1f;
            if (transform.position.x >= MaxX) direction = -1f;
            body.linearVelocity = new Vector2(direction * 1.25f, body.linearVelocity.y);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out HeroController hero))
            {
                if (contactRemaining <= 0f) contactRemaining = 1f / 3f;
                hero.Hurt(transform.position);
            }
        }

        public override void TakeHit(int damage, Vector2 force)
        {
            if (!IsAlive) return;
            health -= damage;
            hurtRemaining = .2f;
            body.AddForce(force, ForceMode2D.Impulse);
            if (health <= 0)
            {
                GetComponent<Collider2D>().enabled = false;
                body.simulated = false;
                visual.Sample("defeat", 0f);
                Destroy(gameObject, .75f);
            }
        }
    }

    public sealed class MossbackGuardian : HeroTarget
    {
        public override bool IsAlive => health > 0;
        public bool Awake { get; private set; }
        public int Health => health;
        public int MaxHealth => 10;

        private int health = 10;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private PixelSpriteAnimator visual;
        private float hurtRemaining;
        private float deathTime;
        private float timer;
        private BossState state = BossState.Sleeping;
        private float facing = -1f;

        private enum BossState { Sleeping, Telegraph, Charging, Resting }

        private void Start()
        {
            body = GetComponent<Rigidbody2D>();
            visual = PixelSpriteAnimator.Attach(gameObject, "mossback-guardian");
            sprite = visual.Renderer;
        }

        private void LateUpdate()
        {
            if (visual == null) return;
            hurtRemaining = Mathf.Max(0f, hurtRemaining - Time.deltaTime);
            visual.Face(facing);
            if (!IsAlive)
            {
                deathTime += Time.deltaTime;
                visual.Sample("defeat", deathTime);
                sprite.color = new Color(1f, 1f, 1f, Mathf.Clamp01((1.8f - deathTime) / .6f));
            }
            else if (hurtRemaining > 0f) visual.Sample("hurt", .2f - hurtRemaining);
            else visual.Tick(state == BossState.Telegraph ? "telegraph" : state == BossState.Charging ? "charge" : "idle",
                Time.deltaTime, health <= 5 && state == BossState.Charging ? 1.25f : 1f);
        }

        private void Update()
        {
            if (!IsAlive || Game == null || !Game.Started || Game.Won) return;
            if (!Awake && Game.Hero.HasSpark && Game.Hero.transform.position.x > 41.1f)
            {
                Awake = true;
                state = BossState.Telegraph;
                timer = 1.05f;
                Game.UI.FlashMessage("MOSSBACK GUARDIAN\nShow me your courage!", 2f);
            }
            if (!Awake) return;

            // Commit to the advertised direction for the entire charge.
            if (state != BossState.Charging)
                facing = Game.Hero.transform.position.x < transform.position.x ? -1f : 1f;
            timer -= Time.deltaTime;

            if (state == BossState.Telegraph)
            {
                sprite.color = Mathf.FloorToInt(timer * 10f) % 2 == 0 ? new Color(1f, .65f, .35f) : Color.white;
                if (timer <= 0f)
                {
                    sprite.color = Color.white;
                    state = BossState.Charging;
                    timer = health <= 5 ? .82f : .68f;
                }
            }
            else if (state == BossState.Resting && timer <= 0f)
            {
                state = BossState.Telegraph;
                timer = health <= 5 ? .62f : .92f;
            }
        }

        private void FixedUpdate()
        {
            if (!Awake || !IsAlive || Game.Won) return;
            if (state == BossState.Charging)
            {
                body.linearVelocity = new Vector2(facing * (health <= 5 ? 8.2f : 6.8f), body.linearVelocity.y);
                if (transform.position.x < 41.1f || transform.position.x > 49.8f || timer <= 0f)
                {
                    transform.position = new Vector3(Mathf.Clamp(transform.position.x, 41.15f, 49.75f), transform.position.y, 0f);
                    body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                    state = BossState.Resting;
                    timer = .72f;
                }
            }
            else
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (Awake && collision.gameObject.TryGetComponent(out HeroController hero))
                hero.Hurt(transform.position);
        }

        public override void TakeHit(int damage, Vector2 force)
        {
            if (!Awake || !IsAlive || state == BossState.Charging) return;
            health -= damage;
            hurtRemaining = .2f;
            body.AddForce(force * .35f, ForceMode2D.Impulse);
            if (health <= 0)
            {
                health = 0;
                GetComponent<Collider2D>().enabled = false;
                body.simulated = false;
                visual.Sample("defeat", 0f);
                Game.CompleteQuest();
                StartCoroutine(VictoryAnimation());
            }
        }

        private System.Collections.IEnumerator VictoryAnimation()
        {
            yield return new WaitForSeconds(1.8f);
            sprite.enabled = false;
        }
    }

    public sealed class HeroSpark : MonoBehaviour
    {
        public HeroGame Game { get; set; }
        private Vector3 origin;

        private void Start() => origin = transform.position;

        private void Update()
        {
            transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * 3f) * .12f);
            transform.Rotate(0f, 0f, 55f * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out HeroController _)) return;
            Game.CollectHeroSpark();
            Destroy(gameObject);
        }
    }

    public sealed class Hazard : MonoBehaviour
    {
        public HeroGame Game { get; set; }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out HeroController hero)) hero.Hurt(transform.position);
        }
    }

    public sealed class Checkpoint : MonoBehaviour
    {
        public HeroGame Game { get; set; }
        public Vector2 RespawnPosition { get; set; }
        private bool activated;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.TryGetComponent(out HeroController hero)) return;
            activated = true;
            hero.SetCheckpoint(RespawnPosition);
            GetComponent<SpriteRenderer>().color = new Color(1f, .77f, .25f);
            Game.UI.FlashMessage("CHECKPOINT!", 1.25f);
        }
    }

    public sealed class CameraFollow : MonoBehaviour
    {
        public HeroGame Game { get; set; }
        private Vector3 velocity;

        private void LateUpdate()
        {
            if (Game == null || Game.Hero == null) return;
            float desiredX = Mathf.Clamp(Game.Hero.transform.position.x + 2.2f, 9.5f, 41.4f);
            Vector3 target = new(desiredX, 0f, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, .2f);
        }
    }

    public sealed class BackdropFollow : MonoBehaviour
    {
        public Transform Target { get; set; }
        private void LateUpdate()
        {
            if (Target != null) transform.position = new Vector3(Target.position.x, 0f, 5f);
        }
    }
}
