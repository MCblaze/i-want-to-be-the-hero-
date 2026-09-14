using System;
using System.Collections.Generic;
using UnityEngine;

namespace IWantToBeTheHero
{
    [Serializable] public sealed class PixelClip
    {
        public string name;
        public int[] frames;
        public float fps;
        public bool loop;
    }

    [Serializable] public sealed class PixelPivot { public float x; public float y; }
    [Serializable] public sealed class PixelRegion { public int x; public int y; public int w; public int h; }

    [Serializable] public sealed class PixelCharacter
    {
        public string id;
        public string image;
        public int columns;
        public int rows;
        public int facing;
        public float pivotX;
        public float pivotY;
        public float unityHeight;
        public PixelPivot[] pivots;
        public PixelRegion[] regions;
        public PixelClip[] clips;
    }

    [Serializable] public sealed class PixelManifest { public PixelCharacter[] characters; }

    // A visual-only child. Changing a frame or pivot never scales a physics body.
    // Clip timing and source rectangles are shared with web/assets/animations/manifest.json.
    public sealed class PixelSpriteAnimator : MonoBehaviour
    {
        public SpriteRenderer Renderer { get; private set; }
        public string CurrentClip { get; private set; }
        private PixelCharacter definition;
        private PixelClip clip;
        private Sprite[] frames;
        private float clock;
        private static PixelManifest manifest;
        private static readonly Dictionary<string, Sprite[]> CachedFrames = new Dictionary<string, Sprite[]>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            manifest = null;
            CachedFrames.Clear();
        }

        public static PixelSpriteAnimator Attach(GameObject owner, string id)
        {
            var original = owner.GetComponent<SpriteRenderer>();
            var child = new GameObject("Animated Sprite");
            child.transform.SetParent(owner.transform, false);
            var physicsCollider = owner.GetComponent<Collider2D>();
            if (physicsCollider != null)
                child.transform.position = new Vector3(owner.transform.position.x, physicsCollider.bounds.min.y, owner.transform.position.z);
            var animator = child.AddComponent<PixelSpriteAnimator>();
            animator.Renderer = child.AddComponent<SpriteRenderer>();
            animator.Renderer.sortingOrder = original != null ? original.sortingOrder : 20;
            animator.Initialize(id);
            if (animator.frames != null)
            {
                if (original != null) original.enabled = false;
                animator.Sample("idle", 0f);
            }
            else if (original != null)
            {
                child.transform.localPosition = Vector3.zero;
                animator.Renderer.sprite = original.sprite;
                original.enabled = false;
                Debug.LogWarning("Animation sheet unavailable for " + id + "; displaying the original pose.");
            }
            return animator;
        }

        private void Initialize(string id)
        {
            if (manifest == null)
            {
                var data = Resources.Load<TextAsset>("Art/Animations/manifest");
                if (data == null) return;
                manifest = JsonUtility.FromJson<PixelManifest>(data.text);
            }
            foreach (var character in manifest.characters)
                if (character.id == id) { definition = character; break; }
            if (definition == null) return;
            if (CachedFrames.TryGetValue(id, out frames)) return;
            string imageName = definition.image.Substring(0, definition.image.LastIndexOf('.'));
            var texture = Resources.Load<Texture2D>("Art/Animations/" + imageName);
            if (texture == null) return;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            frames = new Sprite[definition.columns * definition.rows];
            float cellHeight = (float)texture.height / definition.rows;
            float ppu = cellHeight / definition.unityHeight;
            for (int i = 0; i < frames.Length; i++)
            {
                int column = i % definition.columns;
                int row = i / definition.columns;
                int left = Mathf.RoundToInt((float)column * texture.width / definition.columns);
                int right = Mathf.RoundToInt((float)(column + 1) * texture.width / definition.columns);
                int top = Mathf.RoundToInt((float)row * texture.height / definition.rows);
                int bottom = Mathf.RoundToInt((float)(row + 1) * texture.height / definition.rows);
                if (definition.regions != null && i < definition.regions.Length)
                {
                    var region = definition.regions[i];
                    left = region.x;
                    right = region.x + region.w;
                    top = region.y;
                    bottom = region.y + region.h;
                }
                var pivot = definition.pivots != null && i < definition.pivots.Length ? definition.pivots[i] : null;
                float px = pivot != null ? pivot.x : definition.pivotX;
                float py = pivot != null ? pivot.y : definition.pivotY;
                frames[i] = Sprite.Create(texture, new Rect(left, texture.height - bottom, right - left, bottom - top),
                    new Vector2(px, 1f - py), ppu, 0, SpriteMeshType.FullRect);
                frames[i].name = id + "_" + i.ToString("00");
            }
            CachedFrames[id] = frames;
        }

        private void Select(string name)
        {
            if (CurrentClip == name || definition == null) return;
            foreach (var candidate in definition.clips)
            {
                if (candidate.name != name) continue;
                clip = candidate;
                CurrentClip = name;
                clock = 0f;
                return;
            }
        }

        public void Tick(string name, float dt, float speed = 1f)
        {
            Select(name);
            clock += Mathf.Max(0f, dt) * Mathf.Max(0f, speed);
            ApplyFrame();
        }

        public void Sample(string name, float time)
        {
            Select(name);
            clock = Mathf.Max(0f, time);
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (clip == null || frames == null) return;
            int index = Mathf.FloorToInt(clock * clip.fps + .00001f);
            index = clip.loop ? index % clip.frames.Length : Mathf.Min(index, clip.frames.Length - 1);
            Renderer.sprite = frames[clip.frames[index]];
        }

        public void Face(float direction)
        {
            Renderer.flipX = Mathf.Sign(direction) != (definition != null ? definition.facing : 1);
        }

        public void Ghost()
        {
            var ghost = new GameObject("Hero Dash Afterimage");
            ghost.transform.position = transform.position;
            var sprite = ghost.AddComponent<SpriteRenderer>();
            sprite.sprite = Renderer.sprite;
            sprite.flipX = Renderer.flipX;
            sprite.sortingOrder = Renderer.sortingOrder - 1;
            ghost.AddComponent<PixelGhostFade>();
        }
    }

    public sealed class PixelGhostFade : MonoBehaviour
    {
        private float remaining = .16f;
        private SpriteRenderer sprite;
        private void Awake() => sprite = GetComponent<SpriteRenderer>();
        private void Update()
        {
            remaining -= Time.deltaTime;
            sprite.color = new Color(.65f, 1f, 1f, Mathf.Max(0f, remaining / .16f) * .28f);
            if (remaining <= 0f) Destroy(gameObject);
        }
    }
}
