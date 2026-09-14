using System.IO;
using System.Linq;
using IWantToBeTheHero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MovementLabBuilder
{
    public const string Folder = "Assets/HeroDemo/Movement";
    public const string ScenePath = Folder + "/MovementAndFeel_Test.unity";
    private static Sprite pixel;

    [MenuItem("Tools/Hero/Open Movement and Feel Test")]
    public static void Open()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before opening the authoring scene.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
        Build();
    }

    public static void Build()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
            throw new System.InvalidOperationException("Save the current scene and exit Play Mode before building the room.");
        if (File.Exists(ScenePath)) throw new System.InvalidOperationException("The room already exists; open it instead of overwriting it.");
        EnsureFolder("Assets/HeroDemo");
        EnsureFolder(Folder);
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "Greybox Pixel", filterMode = FilterMode.Point };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, Folder + "/GreyboxPixel.asset");
        pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1f);
        pixel.name = "Greybox Pixel Sprite";
        AssetDatabase.AddObjectToAsset(pixel, texture);
        var tuning = ScriptableObject.CreateInstance<HeroTuning>();
        AssetDatabase.CreateAsset(tuning, Folder + "/MovementTuning.asset");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var room = new GameObject("Movement Room - Authored Geometry");
        Color ground = new(.15f, .34f, .29f);
        Ground("Runway", new Vector2(6.5f, -.5f), new Vector2(13f, 1f), room.transform, ground);
        Ground("Gap 1 landing", new Vector2(18f, -.5f), new Vector2(6f, 1f), room.transform, ground);
        Ground("Gap 2 landing", new Vector2(26.75f, -.5f), new Vector2(6f, 1f), room.transform, ground);
        Ground("Platforms and combat", new Vector2(51.625f, -.5f), new Vector2(36.75f, 1f), room.transform, ground);
        Ground("Recovery shelf", new Vector2(23f, -3.4f), new Vector2(23f, .6f), room.transform, new Color(.12f, .23f, .25f));
        Box("Left boundary", new Vector2(-.3f, 2f), new Vector2(.6f, 5f), room.transform, ground, true);
        Box("Right boundary", new Vector2(70.3f, 2f), new Vector2(.6f, 5f), room.transform, ground, true);
        Label("Runway title", new Vector2(5f, 3.8f), "1  RUNWAY\nJump / double jump / evade", room.transform);
        Label("Gap title", new Vector2(24f, 4.8f), "2  MEASURED GAPS\n2.0 / 2.75 / 3.5 units", room.transform);
        Label("Recovery label", new Vector2(23f, -2.4f), "Recovery shelf - jump back up or press 2", room.transform);

        var ledge = Box("One-way ledge - jump through from below", new Vector2(38f, 2f), new Vector2(3f, .2f), room.transform, new Color(1f, .72f, .3f), true);
        ledge.GetComponent<BoxCollider2D>().usedByEffector = true;
        var effector = ledge.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.useSideFriction = false;
        Label("Platform title", new Vector2(42f, 4.8f), "3  PLATFORMS\nGold = one-way; teal = solid", room.transform);
        var moving = Box("Moving platform", new Vector2(43f, 1.2f), new Vector2(2.4f, .3f), room.transform, new Color(.45f, .8f, .8f), true);
        var rigidbody = moving.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        moving.AddComponent<LabMovingPlatform>();
        Box("Low ceiling", new Vector2(54f, 2.6f), new Vector2(5f, .35f), room.transform, ground, true);
        Box("Wall probe", new Vector2(50.5f, 1.5f), new Vector2(.4f, 3f), room.transform, ground, true);
        Label("Ceiling title", new Vector2(54f, 4f), "5  LOW CEILING", room.transform);
        var target = Box("Training target", new Vector2(62f, .8f), new Vector2(.8f, 1.6f), room.transform, new Color(.9f, .45f, .25f), true);
        target.AddComponent<TrainingTarget>();
        Label("Target readout", new Vector2(62f, 2.8f), "TARGET\n0 hits / 0 damage", target.transform);
        Label("Combat title", new Vector2(62f, 4.8f), "4  COMBAT\nE / Y: sword and wand", room.transform);
        Box("Projectile blocking wall", new Vector2(67f, 1f), new Vector2(.35f, 2f), room.transform, ground, true);
        var flag = Box("Checkpoint", new Vector2(35f, .7f), new Vector2(.2f, 1.4f), room.transform, new Color(.9f, .8f, .4f), true);
        flag.GetComponent<BoxCollider2D>().isTrigger = true;
        var checkpoint = flag.AddComponent<LabCheckpoint>();
        checkpoint.safePosition = new Vector2(34.5f, .7f);

        var spawn = new GameObject("Logan Spawn");
        spawn.transform.SetParent(room.transform);
        spawn.transform.position = new Vector2(2f, .7f);
        var preview = Box("Logan position - Edit Mode preview", spawn.transform.position, new Vector2(.52f, 1.15f), room.transform, new Color(1f, .85f, .3f), false);
        PrefabUtility.SaveAsPrefabAssetAndConnect(room, Folder + "/MovementRoom.prefab", InteractionMode.AutomatedAction);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.orthographicSize = 5.4f;
        camera.backgroundColor = new Color(.045f, .095f, .12f);
        camera.transform.position = new Vector3(8.5f, 1f, -10f);
        cameraObject.AddComponent<AudioListener>();
        var session = new GameObject("Movement Lab Session").AddComponent<MovementLab>();
        session.previewCamera = camera;
        session.spawn = spawn.transform;
        session.heroPreview = preview;
        session.tuning = tuning;
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = room;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.in2DMode = true;
            SceneView.lastActiveSceneView.Frame(new Bounds(new Vector3(35f, 1f), new Vector3(76f, 13f, 1f)), false);
        }
        Debug.Log("Movement room saved: " + ScenePath);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }

    private static GameObject Box(string name, Vector2 position, Vector2 size, Transform parent, Color color, bool collision)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = pixel;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        renderer.color = color;
        if (collision) obj.AddComponent<BoxCollider2D>().size = size;
        return obj;
    }

    private static void Ground(string name, Vector2 position, Vector2 size, Transform parent, Color color)
    {
        Box(name, position, size, parent, color, true);
        Box(name + " landing edge", position + Vector2.up * (size.y * .5f), new Vector2(size.x, .08f), parent, new Color(.7f, .91f, .5f), false);
    }

    private static void Label(string name, Vector2 position, string text, Transform parent)
    {
        var label = new GameObject(name);
        label.transform.SetParent(parent);
        label.transform.position = new Vector3(position.x, position.y, -.2f);
        var mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mesh.fontSize = 48;
        mesh.characterSize = .07f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = new Color(.9f, .94f, .86f);
        label.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
    }
}
