using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DeliveryTerrainSeams
{
    private const string CandidatePath = "Assets/Scenes/Sunleaf_DeliveryCandidate.unity";
    private const string RootName = "Delivery terrain - visual seam covers";
    private const string CropPath = "Assets/Art/Sunleaf/Terrain/DeliveryWalklineSeam.asset";
    private sealed class Floor
    {
        public Transform transform;
        public Bounds bounds;
        public SpriteRenderer skin;
    }

    [MenuItem("Tools/Hero/Repair Candidate Terrain Visual Seams")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play Mode before repairing visual seams.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != CandidatePath)
            throw new InvalidOperationException("Open Sunleaf_DeliveryCandidate first. This operation never modifies Main.");
        Physics2D.SyncTransforms();
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        var floors = new List<Floor>();
        foreach (var t in all.Where(t => t.gameObject.activeInHierarchy && t.name.StartsWith("Floor ")))
        {
            var solids = t.GetComponentsInChildren<Collider2D>().Where(c => c.enabled && !c.isTrigger).ToArray();
            var skin = t.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(r => r.enabled &&
                r.name.StartsWith("Premium Mossy Stone Skin") && r.sprite != null);
            if (solids.Length == 0 || skin == null) continue;
            Bounds b = solids[0].bounds;
            foreach (var c in solids.Skip(1)) b.Encapsulate(c.bounds);
            floors.Add(new Floor { transform = t, bounds = b, skin = skin });
        }
        if (floors.Count < 2) throw new InvalidOperationException("Candidate floor skins/colliders were not found.");
        var reference = floors.Select(f => f.skin).FirstOrDefault(r =>
            Mathf.Abs(r.sprite.rect.height - 490f) < .1f && r.sprite.texture.width >= 1130);
        if (reference == null) throw new InvalidOperationException("Expected 490-pixel-high source terrain crop is missing.");
        // Use the original texture and central opaque strip. No PNG pixels, imports,
        // collision geometry, platform positions or existing sprites are modified.
        float pivotY = reference.sprite.pivot.y / reference.sprite.rect.height;
        Sprite crop = AssetDatabase.LoadAssetAtPath<Sprite>(CropPath);
        if (crop == null)
        {
            crop = Sprite.Create(reference.sprite.texture, new Rect(950f, 80f, 180f, 490f),
                new Vector2(.5f, pivotY), reference.sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            crop.name = "Delivery Walkline Seam - native center strip";
            AssetDatabase.CreateAsset(crop, CropPath);
        }
        else if (crop.texture != reference.sprite.texture || crop.rect != new Rect(950f, 80f, 180f, 490f) ||
            Mathf.Abs(crop.pixelsPerUnit - reference.sprite.pixelsPerUnit) > .001f ||
            Mathf.Abs(crop.pivot.y / crop.rect.height - pivotY) > .001f)
            throw new InvalidOperationException("Existing seam crop differs from the expected source. Inspect it before replacing it.");

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Repair candidate visual floor seams");
        foreach (var old in all.Where(t => t.name == RootName).ToArray()) Undo.DestroyObjectImmediate(old.gameObject);
        var root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        var route = all.FirstOrDefault(t => t.name.StartsWith("Delivery candidate -"));
        if (route != null) root.transform.SetParent(route, true);
        Undo.RegisterCreatedObjectUndo(root, "Create seam visual group");
        // Identity root preserves the exact world scale of the native crop.
        int count = 0;
        foreach (var left in floors.OrderBy(f => f.bounds.min.x))
        foreach (var right in floors.Where(f => f.bounds.center.x > left.bounds.center.x))
        {
            float gap = right.bounds.min.x - left.bounds.max.x;
            if (Mathf.Abs(gap) > .1f || Mathf.Abs(left.bounds.max.y - right.bounds.max.y) > .015f) continue;
            var source = left.skin;
            if (source.sprite.texture != crop.texture ||
                Mathf.Abs(source.sprite.pixelsPerUnit - crop.pixelsPerUnit) > .001f) continue;
            var go = new GameObject($"Visual seam {++count:00} - {left.transform.name}");
            Undo.RegisterCreatedObjectUndo(go, "Create native seam cover");
            go.transform.SetParent(root.transform, false);
            // Sliced renderers can have a size unrelated to transform scale.
            // Match their measured world pixel scale, not their localScale.
            Vector3 sourceWorldSize = source.bounds.size;
            Vector3 sourceNativeSize = source.sprite.bounds.size;
            if (sourceNativeSize.x <= 0f || sourceNativeSize.y <= 0f)
                throw new InvalidOperationException("Source skin has empty native bounds.");
            Vector3 worldScale = new Vector3(sourceWorldSize.x / sourceNativeSize.x,
                sourceWorldSize.y / sourceNativeSize.y, 1f);
            float cropPivotY = crop.pivot.y / crop.rect.height;
            go.transform.position = new Vector3((left.bounds.max.x + right.bounds.min.x) * .5f,
                source.bounds.min.y + cropPivotY * sourceWorldSize.y, source.transform.position.z);
            go.transform.rotation = source.transform.rotation;
            Vector3 parentScale = root.transform.lossyScale;
            go.transform.localScale = new Vector3(worldScale.x / parentScale.x,
                worldScale.y / parentScale.y, worldScale.z / parentScale.z);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = crop;
            renderer.color = source.color;
            renderer.sharedMaterial = source.sharedMaterial;
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = source.sortingOrder + 1;
            renderer.flipX = source.flipX;
            renderer.flipY = source.flipY;
            renderer.maskInteraction = source.maskInteraction;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.spriteSortPoint = source.spriteSortPoint;
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save candidate seam visuals.");
        Debug.Log($"Candidate visual seams repaired: {count}. Native center crops only; no colliders or Main assets changed.");
    }
}

