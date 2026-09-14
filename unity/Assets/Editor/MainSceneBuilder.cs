using System.IO;
using System.Linq;
using IWantToBeTheHero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/Main.unity";
    public const string Folder = "Assets/HeroDemo/Main";
    private const string VisualsPath = Folder + "/MainSceneVisuals.asset";
    private static Sprite pixel;

    [MenuItem("Tools/Hero/Open Editable Main Scene")]
    public static void Open()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before opening Main.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
        FrameLevel();
    }

    [MenuItem("Tools/Hero/Build Editable Main Scene")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before building Main.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        if (scene.rootCount != 0)
        {
            var roots = scene.GetRootGameObjects();
            if (roots.Length != 1 || roots[0].name != "Main Game Session")
                throw new System.InvalidOperationException("Main contains authored work. This builder will not overwrite it.");
            if (!EditorUtility.DisplayDialog(
                    "Rebuild Editable Main Scene?",
                    "This replaces the current Main Game Session, including platform edits. Use Cancel to keep your work.",
                    "Rebuild",
                    "Cancel"))
                return;
            Object.DestroyImmediate(roots[0]);
        }

        EnsureFolder("Assets/HeroDemo");
        EnsureFolder(Folder);
        pixel = EnsureVisuals();

        var session = new GameObject("Main Game Session");
        var layout = session.AddComponent<MainSceneLayout>();

        var environment = Child("Environment - edit these objects", session.transform);
        var backdrop = Child("Sunleaf Ruins Backdrop", environment.transform);
        var backdropRenderer = backdrop.AddComponent<SpriteRenderer>();
        backdropRenderer.sprite = LoadSprite("Sunleaf Ruins Preview");
        backdropRenderer.sortingOrder = -100;
        backdrop.transform.position = new Vector3(9.5f, 0f, 5f);
        layout.backdrop = backdrop;

        var geometry = Child("Platforms - duplicate and edit", session.transform);
        Platform("Ground 01", 3.8f, -4.1f, 7.6f, 1.4f, geometry.transform);
        Platform("Ground 02", 11.6f, -4.1f, 6.2f, 1.4f, geometry.transform);
        Platform("Ground 03", 18.95f, -4.1f, 6.9f, 1.4f, geometry.transform);
        Platform("Ground 04", 27.65f, -4.1f, 8.5f, 1.4f, geometry.transform);
        Platform("Ground 05", 36.3f, -4.1f, 7f, 1.4f, geometry.transform);
        Platform("Ground 06 - Boss Arena", 45.8f, -4.1f, 10.4f, 1.4f, geometry.transform);
        Platform("Ledge 01", 5.55f, -2.45f, 1.7f, .28f, geometry.transform);
        Platform("Ledge 02", 10.25f, -2.45f, 1.9f, .28f, geometry.transform);
        Platform("Ledge 03", 12.7f, -1.25f, 1.6f, .28f, geometry.transform);
        Platform("Ledge 04", 17.35f, -2.45f, 1.7f, .28f, geometry.transform);
        Platform("Ledge 05", 19.9f, -1.55f, 1.8f, .28f, geometry.transform);
        Platform("Ledge 06", 25.85f, -2.35f, 2.3f, .28f, geometry.transform);
        Platform("Ledge 07 - Spark", 28.7f, -1.2f, 1.8f, .28f, geometry.transform);
        Platform("Ledge 08", 31f, -2.35f, 1.6f, .28f, geometry.transform);
        Platform("Ledge 09", 34.8f, -2.3f, 2f, .28f, geometry.transform);
        Platform("Ledge 10", 37.45f, -1.1f, 1.7f, .28f, geometry.transform);

        var hazards = Child("Hazards - move or duplicate", session.transform);
        layout.hazards = new[] {
            Spikes("Spikes 01", 7.05f, -3.35f, .55f, hazards.transform),
            Spikes("Spikes 02", 14.15f, -3.35f, .7f, hazards.transform),
            Spikes("Spikes 03", 21.95f, -3.35f, .65f, hazards.transform),
            Spikes("Spikes 04", 31.55f, -3.35f, .55f, hazards.transform),
            Spikes("Spikes 05", 39.45f, -3.35f, .5f, hazards.transform)
        };

        var progression = Child("Progression - checkpoint, Spark and gate", session.transform);
        layout.checkpoint = Block("Checkpoint Flag", new Vector2(24.55f, -3.05f), new Vector2(.18f, 1.2f),
            new Color(.38f, .24f, .15f), progression.transform, false);
        var checkpointTrigger = layout.checkpoint.AddComponent<BoxCollider2D>();
        checkpointTrigger.size = new Vector2(1f, 1.7f);
        checkpointTrigger.isTrigger = true;
        layout.checkpointRespawnPosition = new Vector2(23.95f, -2.8f);

        layout.heroSpark = Child("Hero Spark", progression.transform);
        layout.heroSpark.transform.position = new Vector2(31f, -1.65f);
        var sparkRenderer = layout.heroSpark.AddComponent<SpriteRenderer>();
        sparkRenderer.sprite = LoadSprite("Hero Spark Preview");
        sparkRenderer.color = new Color(1f, .82f, .3f);
        sparkRenderer.sortingOrder = 18;
        var sparkTrigger = layout.heroSpark.AddComponent<CircleCollider2D>();
        sparkTrigger.radius = .55f;
        sparkTrigger.isTrigger = true;

        layout.sparkGate = Block("Hero Spark Gate", new Vector2(40.25f, -1.2f), new Vector2(.3f, 4.6f),
            new Color(.45f, .72f, .45f, .78f), progression.transform, true);

        var markers = Child("Markers - move these transforms", session.transform);
        layout.heroSpawn = Child("Logan Spawn", markers.transform).transform;
        layout.heroSpawn.position = new Vector2(1.2f, -2.8f);
        layout.heroPreview = Block("Logan Spawn Preview (hidden in Play Mode)", layout.heroSpawn.position,
            new Vector2(.52f, 1.15f), new Color(1f, .82f, .25f, .7f), layout.heroSpawn, false);
        layout.heroPreview.transform.localPosition = Vector3.zero;

        var cameraObject = Child("Main Camera", session.transform);
        cameraObject.tag = "MainCamera";
        layout.mainCamera = cameraObject.AddComponent<Camera>();
        layout.mainCamera.orthographic = true;
        layout.mainCamera.orthographicSize = 5.4f;
        layout.mainCamera.clearFlags = CameraClearFlags.SolidColor;
        layout.mainCamera.backgroundColor = new Color(.18f, .42f, .43f);
        cameraObject.transform.position = new Vector3(9.5f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();

        EditorUtility.SetDirty(layout);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = geometry;
        FrameLevel();
        Debug.Log("Editable Main scene saved. Platforms and progression objects now persist outside Play Mode.");
    }

    private static Sprite EnsureVisuals()
    {
        if (File.Exists(VisualsPath)) return LoadSprite("Main Pixel");
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "Main Pixel Texture", filterMode = FilterMode.Point };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, VisualsPath);
        var pixelSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1f);
        pixelSprite.name = "Main Pixel";
        AssetDatabase.AddObjectToAsset(pixelSprite, texture);

        var starTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false) { name = "Hero Spark Texture", filterMode = FilterMode.Point };
        for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
        {
            float dx = Mathf.Abs(x - 7.5f), dy = Mathf.Abs(y - 7.5f);
            bool inside = dx + dy * .52f < 4.2f || dy + dx * .52f < 4.2f;
            starTexture.SetPixel(x, y, inside ? Color.white : Color.clear);
        }
        starTexture.Apply();
        AssetDatabase.AddObjectToAsset(starTexture, texture);
        var star = Sprite.Create(starTexture, new Rect(0, 0, 16, 16), Vector2.one * .5f, 16f);
        star.name = "Hero Spark Preview";
        AssetDatabase.AddObjectToAsset(star, texture);

        var backdropTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/sunleaf-ruins.png");
        var backdrop = Sprite.Create(backdropTexture, new Rect(0, 0, backdropTexture.width, backdropTexture.height),
            Vector2.one * .5f, backdropTexture.height / 11.5f);
        backdrop.name = "Sunleaf Ruins Preview";
        AssetDatabase.AddObjectToAsset(backdrop, texture);
        AssetDatabase.SaveAssets();
        return pixelSprite;
    }

    private static Sprite LoadSprite(string name) =>
        AssetDatabase.LoadAllAssetsAtPath(VisualsPath).OfType<Sprite>().First(sprite => sprite.name == name);

    private static GameObject Child(string name, Transform parent)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent);
        return child;
    }

    private static void Platform(string name, float x, float y, float width, float height, Transform parent)
    {
        var platform = Child(name + " - edit or duplicate", parent);
        platform.transform.position = new Vector2(x, y);
        Block("Stone Body", Vector2.zero, new Vector2(width, height), new Color(.13f, .22f, .18f), platform.transform, true).transform.localPosition = Vector3.zero;
        var edgeColor = height > .5f ? new Color(.49f, .66f, .25f) : new Color(.68f, .78f, .31f);
        var edge = Block("Grass Edge", Vector2.zero, new Vector2(width, .09f), edgeColor, platform.transform, false);
        edge.transform.localPosition = new Vector3(0f, height * .5f - .045f, 0f);
    }

    private static GameObject Spikes(string name, float x, float y, float width, Transform parent)
    {
        var spikes = Block(name, new Vector2(x, y), new Vector2(width, .28f), new Color(.22f, .19f, .18f), parent, false);
        var trigger = spikes.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        return spikes;
    }

    private static GameObject Block(string name, Vector2 position, Vector2 size, Color color, Transform parent, bool collision)
    {
        var block = Child(name, parent);
        block.transform.position = position;
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        var renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = pixel;
        renderer.color = color;
        renderer.sortingOrder = 5;
        if (collision) block.AddComponent<BoxCollider2D>();
        return block;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }

    private static void FrameLevel()
    {
        if (SceneView.lastActiveSceneView == null) return;
        SceneView.lastActiveSceneView.in2DMode = true;
        SceneView.lastActiveSceneView.Frame(new Bounds(new Vector3(25f, -1f), new Vector3(56f, 14f, 1f)), false);
    }
}
