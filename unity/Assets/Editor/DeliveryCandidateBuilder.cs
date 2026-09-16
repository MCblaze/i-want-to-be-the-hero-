using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWantToBeTheHero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class DeliveryCandidateBuilder
{
    public const string CandidatePath = "Assets/Scenes/Sunleaf_DeliveryCandidate.unity";
    private const string SourcePath = "Assets/Scenes/Main.unity";

    public static void Build() => Build(false);

    public static void Build(bool explicitlyReplaceCandidate)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save or discard open scene changes before constructing the candidate.");
        bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(CandidatePath) != null;
        if (exists && !explicitlyReplaceCandidate)
            throw new InvalidOperationException("Candidate exists. Build(true) is required to explicitly replace it.");
        if (exists)
        {
            string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Sunleaf_DeliveryCandidate_Backup.unity");
            if (!AssetDatabase.CopyAsset(CandidatePath, backup)) throw new IOException("Candidate backup failed.");
            // Only this explicitly confirmed candidate is replaced; the prior version remains an asset.
            File.Copy(SourcePath, CandidatePath, true);
            AssetDatabase.ImportAsset(CandidatePath, ImportAssetOptions.ForceUpdate);
        }
        else if (!AssetDatabase.CopyAsset(SourcePath, CandidatePath))
            throw new IOException("Could not copy Main into the candidate.");

        var scene = EditorSceneManager.OpenScene(CandidatePath, OpenSceneMode.Single);
        var all = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        var layout = all.Select(t => t.GetComponent<MainSceneLayout>()).FirstOrDefault(x => x != null);
        if (layout == null) throw new InvalidOperationException("Source has no authored layout.");
        var grounds = all.Where(t => t.name.StartsWith("Ground ")).Select(t => t.gameObject).ToArray();
        var ledges = all.Where(t => t.name.StartsWith("Ledge ")).Select(t => t.gameObject).ToArray();
        if (grounds.Length == 0 || ledges.Length == 0 || layout.checkpoint == null)
            throw new InvalidOperationException("Required source geometry/checkpoint templates are missing.");
        var lift = all.Select(t => t.GetComponent<LabMovingPlatform>()).FirstOrDefault(x => x != null);
        var groundTemplate = grounds[0];
        var ledgeTemplate = ledges[0];
        float groundWidth = SolidBounds(groundTemplate).size.x;
        if (groundWidth < 1f) throw new InvalidOperationException("Invalid ground module width.");

        var route = Child("Delivery candidate - twelve authored rooms", layout.transform);
        string cropFolder = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/DeliveryCandidateCrops");
        AssetDatabase.CreateFolder("Assets/Scenes", Path.GetFileName(cropFolder));
        string[] names = { "Trailhead", "First courage", "Broken approach", "Ruin crossing", "Canopy lesson", "Canopy choice", "Shrine approach", "Spark shrine", "Weapon gallery", "Mastery crossing", "Court threshold", "Guardian court" };
        var rooms = names.Select((name, index) => Child($"R{index + 1:00} - {name} [{index * 24}..{(index + 1) * 24}]", route)).ToArray();
        // Four required short jumps; two canopy sections drop onto visible lower
        // recovery shelves with a single-jump climbout. Crop edge modules rather
        // than stretching artwork or allowing invisible collider bridges.
        Vector2[] floorIntervals = { new(0f,54f), new(56f,65f), new(67f,80f), new(82f,91f),
            new(93f,100f), new(118f,124f), new(142f,288f) };
        foreach (var interval in floorIntervals)
            Floor(groundTemplate, rooms, interval.x, interval.y, -3.4f, groundWidth, cropFolder);
        Floor(groundTemplate, rooms, 100f, 118f, -5f, groundWidth, cropFolder);
        Floor(groundTemplate, rooms, 124f, 142f, -5f, groundWidth, cropFolder);
        float[] ledgeX = { 11f, 20f, 43f, 56f, 62f, 64f, 83f, 91f, 100f, 114f, 122f, 137f, 155f, 162f, 164f, 173f, 183f, 185f, 207f, 210f, 220f, 227f, 248f, 255f };
        foreach (float x in ledgeX)
        {
            float top = x == 64f || x == 164f || x == 210f ? -.2f : x == 183f || x == 185f || x == 227f ? -.3f : -1.8f;
            var ledge = Place(ledgeTemplate, rooms[Mathf.Min(11, (int)(x / 24f))], $"Ledge {x:F0}", x, top);
            var b = SolidBounds(ledge);
            if (b.min.y < -1.99f) ledge.transform.position += Vector3.up * (-1.99f - b.min.y);
        }
        if (lift != null)
        {
            foreach (float x in new[] { 104f, 127f, 223f })
            {
                var clone = Place(lift.gameObject, rooms[(int)(x / 24f)], "Canopy support", x, -1.6f);
                var movement = clone.GetComponent<LabMovingPlatform>();
                movement.travel = new Vector2(3f, 0f);
                movement.period = 4.5f;
            }
        }
        foreach (var original in grounds.Concat(ledges)) original.SetActive(false);
        if (lift != null) lift.gameObject.SetActive(false);
        foreach (var hazard in layout.hazards ?? Array.Empty<GameObject>()) if (hazard != null) hazard.SetActive(false);
        // No copied spikes: each new pit and lower shelf is visible and has a
        // deliberate recovery/respawn consequence, without disguised damage.
        layout.hazards = Array.Empty<GameObject>();

        float[] checkpointX = { 46f, 94f, 142f, 189f, 238f, 260f };
        // Spawn on the cleared side of each obstacle, never over the 91..93 pit
        // or back down on the Canopy recovery shelf.
        float[] checkpointRespawnX = { 44.5f, 94f, 143.5f, 187.5f, 237f, 260f };
        var checkpoints = new List<MainSceneLayout.CheckpointSpawn>();
        foreach (float x in checkpointX)
        {
            var marker = Object.Instantiate(layout.checkpoint, rooms[Mathf.Min(11, (int)(x / 24f))]);
            marker.name = "Checkpoint " + (checkpoints.Count + 1);
            marker.transform.position = new Vector3(x, -3.05f, 0f);
            marker.SetActive(true);
            checkpoints.Add(new MainSceneLayout.CheckpointSpawn { marker = marker, respawnPosition = new Vector2(checkpointRespawnX[checkpoints.Count], -2.8f), progressOrder = checkpoints.Count });
        }
        layout.checkpoint.SetActive(false);
        layout.checkpoints = checkpoints.ToArray();
        layout.heroSpawn.position = new Vector3(2f, -2.8f, 0f);
        if (layout.heroSpark != null) layout.heroSpark.transform.position = new Vector3(176f, -2.3f, 0f);
        if (layout.sparkGate != null) layout.sparkGate.transform.position = new Vector3(236f, -1.2f, 0f);
        if (layout.optionalBackflipCache != null) layout.optionalBackflipCache.transform.position = new Vector3(228f, 1.2f, 0f);
        var targets = layout.encounterTargets ?? Array.Empty<GameObject>();
        for (int i = 0; i < targets.Length; i++)
            if (targets[i] != null) targets[i].transform.position = new Vector3(i == 0 ? 199f : 210f, i == 0 ? -2.45f : .25f, 0f);

        layout.horizontalBounds = new Vector2(0f, 288f);
        layout.cameraCenterY = 0f;
        layout.killPlaneY = -8f;
        layout.useAuthoredEnemySpawns = true;
        float[] enemyX = { 33f, 40f, 60f, 85f, 89f, 113f, 135f, 153f, 161f, 201f, 214f, 232f, 249f, 257f };
        layout.enemySpawns = enemyX.Select(x => new MainSceneLayout.EnemySpawn {
            encounter = names[Mathf.Min(11, (int)(x / 24f))], position = new Vector2(x, -2.9f), patrolMinX = x - 1.5f, patrolMaxX = x + 1.5f }).ToArray();
        layout.bossSpawnPosition = new Vector2(278f, -2.05f);
        layout.bossArenaBounds = new Vector2(268f, 284f);
        layout.bossWakeX = 268f;
        if (layout.mainCamera != null) layout.mainCamera.transform.position = new Vector3(9.5f, 0f, -10f);
        var backdrops = all.Select(t => t.GetComponent<ZoneBackdrop>()).Where(x => x != null).OrderBy(x => x.GetComponent<SpriteRenderer>().sortingOrder).ToArray();
        for (int i = 0; i < backdrops.Length; i++) backdrops[i].Configure(24f + 48f * i, 24f, 5f, 1f);
        // Original decorative markers stay preserved in the candidate but hidden:
        // they describe the compact Main route and must not misdirect the new one.
        foreach (var t in all)
            if (t.name.StartsWith("Camera Transition") || t.name == "Decoration - zone readability") t.gameObject.SetActive(false);
        EditorUtility.SetDirty(layout);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, CandidatePath)) throw new IOException("Candidate save failed.");
        Selection.activeGameObject = route.gameObject;
        AssetDatabase.SaveAssets();
        Debug.Log("Saved editable challenge candidate: four 2-unit floor gaps and two lower canopy recovery shelves. Main unchanged. Reach, art and duration qualification remain unverified.");
    }

    private static Transform Child(string name, Transform parent)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        return obj.transform;
    }

    private static Bounds SolidBounds(GameObject obj)
    {
        var colliders = obj.GetComponentsInChildren<Collider2D>().Where(c => c.enabled && !c.isTrigger).ToArray();
        if (colliders.Length == 0) throw new InvalidOperationException("Template lacks an enabled solid collider: " + obj.name);
        Physics2D.SyncTransforms();
        Bounds bounds = colliders[0].bounds;
        foreach (var c in colliders.Skip(1)) bounds.Encapsulate(c.bounds);
        return bounds;
    }

    private static GameObject Place(GameObject template, Transform parent, string name, float centerX, float topY)
    {
        var clone = Object.Instantiate(template, parent);
        clone.name = name;
        clone.SetActive(true);
        Bounds bounds = SolidBounds(clone);
        clone.transform.position += new Vector3(centerX - bounds.center.x, topY - bounds.max.y, 0f);
        return clone;
    }

    private static void Floor(GameObject template, Transform[] rooms, float start, float end, float top, float nativeWidth, string cropFolder)
    {
        for (float left = start; left < end - .01f; left += nativeWidth - .03f)
        {
            float right = Mathf.Min(end, left + nativeWidth);
            var clone = Place(template, rooms[Mathf.Min(11, (int)(left / 24f))], $"Floor {left:F1} to {right:F1}", left + nativeWidth * .5f, top);
            if (right < left + nativeWidth - .001f) CropRight(clone, right, cropFolder);
        }
    }

    private static void CropRight(GameObject module, float right, string cropFolder)
    {
        Physics2D.SyncTransforms();
        foreach (var collider in module.GetComponentsInChildren<BoxCollider2D>())
        {
            Bounds bounds = collider.bounds;
            if (bounds.min.x >= right) { collider.enabled = false; continue; }
            if (bounds.max.x <= right) continue;
            float width = right - bounds.min.x;
            Vector3 center = new Vector3(bounds.min.x + width * .5f, bounds.center.y, bounds.center.z);
            Vector3 localCenter = collider.transform.InverseTransformPoint(center);
            collider.offset = new Vector2(localCenter.x, collider.offset.y);
            collider.size = new Vector2(width / Mathf.Abs(collider.transform.lossyScale.x), collider.size.y);
        }
        foreach (var renderer in module.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!renderer.enabled || renderer.sprite == null) continue;
            Bounds bounds = renderer.bounds;
            if (bounds.min.x >= right) { renderer.enabled = false; continue; }
            if (bounds.max.x <= right) continue;
            if (renderer.flipX || Mathf.Abs(renderer.transform.eulerAngles.z) > .01f)
                throw new InvalidOperationException("Crop template must use unrotated, unflipped artwork.");
            Sprite source = renderer.sprite;
            Rect rect = source.rect;
            float ratio = Mathf.Clamp01((right - bounds.min.x) / bounds.size.x);
            float width = Mathf.Max(1f, Mathf.Floor(rect.width * ratio));
            float sourcePivotX = source.pivot.x;
            var cropped = Sprite.Create(source.texture, new Rect(rect.x, rect.y, width, rect.height),
                new Vector2(.5f, source.pivot.y / rect.height), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            cropped.name = source.name + " candidate edge";
            AssetDatabase.CreateAsset(cropped, AssetDatabase.GenerateUniqueAssetPath(cropFolder + "/Edge.asset"));
            // Keep the source left edge planted while changing width/pivot.
            float localShift = (width * .5f - sourcePivotX) / source.pixelsPerUnit;
            var visualObject = new GameObject(renderer.name + " cropped visual");
            visualObject.transform.SetParent(renderer.transform.parent, false);
            visualObject.transform.localPosition = renderer.transform.localPosition;
            visualObject.transform.localRotation = renderer.transform.localRotation;
            visualObject.transform.localScale = renderer.transform.localScale;
            visualObject.transform.position += renderer.transform.TransformVector(new Vector3(localShift, 0f, 0f));
            var visual = visualObject.AddComponent<SpriteRenderer>();
            EditorUtility.CopySerialized(renderer, visual);
            visual.sprite = cropped;
            renderer.enabled = false;
        }
    }
}
