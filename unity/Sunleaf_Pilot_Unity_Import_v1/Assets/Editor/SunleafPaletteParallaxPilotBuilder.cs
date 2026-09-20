using System;
using System.IO;
using System.Linq;
using IWantToBeTheHero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class SunleafPaletteParallaxPilotBuilder
{
    public const string CandidatePath = "Assets/Scenes/Sunleaf_DeliveryCandidate.unity";
    public const string PilotPath = "Assets/Scenes/Sunleaf_PaletteParallax_Pilot.unity";
    public const string PilotRootName = "Sunleaf Palette and Parallax Pilot";

    private const string AssetRoot = "Assets/Art/Sunleaf/PaletteAndParallax/Pilot/";
    private const string BoulderFileName = "Sunleaf_L01_BoulderDecor_Pilot.png";
    private const float WidestAspect = 21f / 9f;

    private readonly struct LayerDefinition
    {
        public LayerDefinition(string name, string fileName, float drift, int order, float opacity)
        {
            Name = name;
            FileName = fileName;
            Drift = drift;
            Order = order;
            Opacity = opacity;
        }

        public string Name { get; }
        public string FileName { get; }
        public float Drift { get; }
        public int Order { get; }
        public float Opacity { get; }
    }

    private static readonly LayerDefinition[] Layers =
    {
        new("L00 Sky", "Sunleaf_L01_L00_Sky_Pilot.png", 0f, -260, 1f),
        new("L10 Far Ruins - PROVISIONAL", "Sunleaf_L01_L10_FarRuins_Pilot.png", 0.08f, -240, 0.58f),
        new("L20 Distant Forest", "Sunleaf_L01_L20_DistantForest_Pilot.png", 0.18f, -220, 0.82f),
        new("L30 Near Foliage", "Sunleaf_L01_L30_NearFoliage_Pilot.png", 0.42f, -200, 0.72f)
    };

    [MenuItem("Sunleaf/Palette and Parallax/Build Isolated Pilot")]
    public static void BuildFromMenu()
    {
        Build(false);
    }

    [MenuItem("Sunleaf/Palette and Parallax/Rebuild Isolated Pilot")]
    public static void RebuildFromMenu()
    {
        if (!EditorUtility.DisplayDialog(
                "Rebuild Sunleaf pilot?",
                "The current pilot will be backed up before a fresh copy is built. Main and the delivery candidate are not modified.",
                "Back up and rebuild",
                "Cancel"))
        {
            return;
        }

        Build(true);
    }

    public static void Build(bool explicitlyReplacePilot)
    {
        RequireSafeEditorState();
        RequireAsset<SceneAsset>(CandidatePath);

        bool pilotExists = AssetDatabase.LoadAssetAtPath<SceneAsset>(PilotPath) != null;
        if (pilotExists && !explicitlyReplacePilot)
        {
            throw new InvalidOperationException(
                "The isolated pilot already exists. Call Build(true) or use the explicit rebuild menu item.");
        }

        if (pilotExists)
        {
            string backupPath = AssetDatabase.GenerateUniqueAssetPath(
                "Assets/Scenes/Sunleaf_PaletteParallax_Pilot_Backup.unity");
            if (!AssetDatabase.CopyAsset(PilotPath, backupPath))
            {
                throw new IOException("Could not back up the existing pilot scene.");
            }

            if (!AssetDatabase.DeleteAsset(PilotPath))
            {
                throw new IOException("Could not remove the backed-up pilot scene before rebuilding.");
            }
        }

        if (!AssetDatabase.CopyAsset(CandidatePath, PilotPath))
        {
            throw new IOException("Could not copy the delivery candidate into the isolated pilot scene.");
        }

        AssetDatabase.ImportAsset(PilotPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Scene scene = EditorSceneManager.OpenScene(PilotPath, OpenSceneMode.Single);
        Transform[] all = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .ToArray();

        MainSceneLayout layout = all.Select(item => item.GetComponent<MainSceneLayout>())
            .FirstOrDefault(item => item != null);
        if (layout == null || layout.mainCamera == null)
        {
            throw new InvalidOperationException("The candidate is missing its MainSceneLayout camera reference.");
        }

        Camera camera = layout.mainCamera.GetComponent<Camera>();
        if (camera == null || !camera.orthographic)
        {
            throw new InvalidOperationException("The pilot requires the candidate's orthographic camera.");
        }

        foreach (ZoneBackdrop backdrop in all.Select(item => item.GetComponent<ZoneBackdrop>()).Where(item => item != null))
        {
            backdrop.gameObject.SetActive(false);
        }

        foreach (QP3ParallaxAnchor anchor in all.Select(item => item.GetComponent<QP3ParallaxAnchor>()).Where(item => item != null))
        {
            anchor.gameObject.SetActive(false);
        }

        GameObject root = new(PilotRootName);
        root.transform.position = new Vector3(camera.transform.position.x, layout.cameraCenterY, 0f);

        float viewportWidth = camera.orthographicSize * 2f * WidestAspect;
        foreach (LayerDefinition definition in Layers)
        {
            Sprite sprite = RequireAsset<Sprite>(AssetRoot + definition.FileName);
            float segmentWidth = sprite.bounds.size.x;
            float segmentHeight = sprite.bounds.size.y;
            if (Mathf.Abs(segmentWidth - 24f) > 0.01f || Mathf.Abs(segmentHeight - 12f) > 0.01f)
            {
                throw new InvalidOperationException(
                    $"{definition.FileName} imported at {segmentWidth:F3} by {segmentHeight:F3} world units; expected 24 by 12.");
            }

            int segmentCount = SunleafParallaxStrip.RequiredSegmentCount(viewportWidth, segmentWidth);
            GameObject layer = new(definition.Name);
            layer.transform.SetParent(root.transform, false);
            SpriteRenderer[] renderers = new SpriteRenderer[segmentCount];
            for (int index = 0; index < segmentCount; index++)
            {
                GameObject segment = new($"Segment {index:00}");
                segment.transform.SetParent(layer.transform, false);
                SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = definition.Order;
                renderer.color = new Color(1f, 1f, 1f, definition.Opacity);
                renderers[index] = renderer;
            }

            SunleafParallaxStrip strip = layer.AddComponent<SunleafParallaxStrip>();
            strip.Configure(camera.transform, definition.Drift, renderers, true);
            if (!strip.ValidateConfiguration(out string reason))
            {
                throw new InvalidOperationException($"{definition.Name} is invalid: {reason}");
            }

            EditorUtility.SetDirty(strip);
            foreach (SpriteRenderer renderer in renderers)
            {
                EditorUtility.SetDirty(renderer);
            }
        }

        Sprite boulderSprite = RequireAsset<Sprite>(AssetRoot + BoulderFileName);
        if (Mathf.Abs(boulderSprite.bounds.size.x - 3f) > 0.01f ||
            Mathf.Abs(boulderSprite.bounds.size.y - 2f) > 0.01f)
        {
            throw new InvalidOperationException(
                $"{BoulderFileName} imported at {boulderSprite.bounds.size.x:F3} by " +
                $"{boulderSprite.bounds.size.y:F3} world units; expected 3 by 2.");
        }

        GameObject boulder = new("Boulder Decor - No Collider");
        boulder.transform.SetParent(root.transform, false);
        boulder.transform.position = new Vector3(8f, -2.85f, 0f);
        SpriteRenderer boulderRenderer = boulder.AddComponent<SpriteRenderer>();
        boulderRenderer.sprite = boulderSprite;
        boulderRenderer.sortingOrder = 3;
        boulderRenderer.color = new Color(1f, 1f, 1f, 0.92f);
        EditorUtility.SetDirty(boulderRenderer);

        if (root.GetComponentsInChildren<Collider2D>(true).Length != 0)
        {
            throw new InvalidOperationException("The parallax pilot unexpectedly contains Collider2D components.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, PilotPath))
        {
            throw new IOException("Could not save the isolated parallax pilot scene.");
        }

        Selection.activeGameObject = root;
        AssetDatabase.SaveAssets();
        Debug.Log(
            "Built the isolated Sunleaf palette/parallax pilot. Main and Sunleaf_DeliveryCandidate remain unchanged. " +
            "Run SunleafParallaxStripTests and SunleafPaletteParallaxPilotTests before approving any rollout.");
    }

    private static void RequireSafeEditorState()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException("Exit Play Mode before building the isolated pilot.");
        }

        for (int index = 0; index < SceneManager.sceneCount; index++)
        {
            if (SceneManager.GetSceneAt(index).isDirty)
            {
                throw new InvalidOperationException(
                    "Save or discard open scene changes before building the isolated pilot.");
            }
        }
    }

    private static T RequireAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"Required asset is missing or has not imported: {path}");
        }

        return asset;
    }
}
