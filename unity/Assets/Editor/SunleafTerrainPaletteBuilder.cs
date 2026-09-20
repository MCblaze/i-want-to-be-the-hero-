using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bezi;
using IWantToBeTheHero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

/// <summary>Builds the non-destructive Sunleaf ledge kit and opens Unity's standard Tile Palette.</summary>
public static class SunleafTerrainPaletteBuilder
{
    private const string SourcePath = "Assets/Art/Sunleaf/Terrain/StaticMossyLedge.png";
    private const string OutputFolder = "Assets/TilePalettes/Sunleaf Connecting Mossy";
    private const string SpriteFolder = OutputFolder + "/Sprites";
    private const string TileFolder = OutputFolder + "/Tiles";
    private const string AtlasPath = OutputFolder + "/Sunleaf Mossy Connections.png";
    private const string PaletteName = "Sunleaf - Connecting Mossy Platforms";
    private const string PalettePath = OutputFolder + "/" + PaletteName + ".prefab";
    private const string PreviewPath = OutputFolder + "/Sunleaf Mossy Examples.prefab";
    private const string TargetName = "Mossy Platforms Tilemap";
    private const string GridName = "Manual Tile Editing";
    private const string SessionName = "Main Game Session";
    private const string OpenMenu = "Tools/Sunleaf/Open Mossy Tile Palette";
    private const string ValidateMenu = "Tools/Sunleaf/Validate Mossy Tile Palette";
    private const int TileWidth = 512;
    private const int StateCount = 4;
    private const int VariationCount = 4;
    private const int WalklinePixels = 464;
    private const int CollisionDepthPixels = 160;
    private const int CapCollisionInsetPixels = 48;
    private const int CapWidth = 224;
    private const int EdgeWidth = 128;
    private const int StitchOverlap = 80;
    private const int SharedSeamSourceX = 1000;
    private const int TransparentPadding = 4;
    private const byte VisibleAlpha = 8;
    private const int SortingOrder = 6;
    private const int ExampleLength = 6;
    private const int RowSpacing = 2;
    private const int RunStartColumn = 2;
    private const float HalfCell = 0.5f;
    private const float ShapeTolerance = 0.002f;
    private const float CollisionExtrusion = 0.001f;
    private const float AlphaErrorWeight = 4f;
    private const int MaximumTextureSize = 4096;
    private static readonly int[] VariationSourceX = { 610, 280, 920, 1290 };
    private static readonly string[] VariationNames = { "Moss and Fern", "Wildflowers", "Ancient Stone", "Trailing Vines", "Natural Mix" };
    private static readonly string[] StateNames = { "Standalone", "Left End", "Middle", "Right End" };

    [MenuItem(OpenMenu)]
    private static void OpenFromMenu()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null) BuildPalette();
        Debug.Log(OpenPalette());
    }

    [MenuItem(ValidateMenu)]
    private static void ValidateFromMenu() => Debug.Log(ValidatePalette());

    /// <summary>Reports source-art bounds and walkline coverage without changing the source importer.</summary>
    [BeziAction("Inspect the source bounds and alpha coverage used to build the Sunleaf connecting mossy palette.", IsReadOnly = true)]
    public static string InspectSource()
    {
        Texture2D source = LoadPixels(SourcePath);
        try
        {
            Color32[] pixels = source.GetPixels32();
            RectInt bounds = FindVisibleBounds(pixels, source.width, source.height);
            int[] rows = { 200, 300, 400, WalklinePixels, 480, 520, 580, 640 };
            var lines = new List<string> { $"Source {source.width}x{source.height}; visible bounds {bounds}; walkline {WalklinePixels}." };
            foreach (int row in rows.Where(row => row < source.height))
            {
                int[] columns = Enumerable.Range(0, source.width).Where(x => pixels[x + row * source.width].a > VisibleAlpha).ToArray();
                lines.Add($"Row {row}: {(columns.Length == 0 ? "empty" : $"{columns.First()}..{columns.Last()}, {columns.Length} visible pixels")}");
            }
            return string.Join("\n", lines);
        }
        finally { Object.DestroyImmediate(source); }
    }

    /// <summary>Creates the atlas, persistent sprites, five connecting brushes and populated palette once.</summary>
    [BeziAction("Build the Sunleaf automatic connecting mossy palette and isolated examples. Does not alter existing platforms or paint into the level.")]
    public static string BuildPalette()
    {
        RequireEditMode();
        EnsureFolder(SpriteFolder);
        EnsureFolder(TileFolder);
        Texture2D source = LoadPixels(SourcePath);
        try
        {
            if (source.width <= VariationSourceX.Max() + TileWidth || source.height <= WalklinePixels)
                throw new InvalidOperationException("The source terrain artwork has unexpected dimensions.");
            if (!File.Exists(AtlasPath)) BuildAtlas(source);
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
            if (atlas == null || atlas.width != TileWidth * StateCount || atlas.height != source.height * VariationCount)
                throw new InvalidOperationException("Existing generated atlas dimensions differ. No existing artwork has been overwritten.");

            var sprites = new Sprite[VariationCount, StateCount];
            for (int variation = 0; variation < VariationCount; variation++)
            for (int state = 0; state < StateCount; state++)
                sprites[variation, state] = CreateSprite(atlas, source.height, variation, state);

            var tiles = new SunleafPlatformTile[VariationNames.Length];
            for (int variation = 0; variation < tiles.Length; variation++)
            {
                string path = GetTilePath(variation);
                tiles[variation] = AssetDatabase.LoadAssetAtPath<SunleafPlatformTile>(path);
                if (tiles[variation] != null) continue;
                var tile = ScriptableObject.CreateInstance<SunleafPlatformTile>();
                tile.name = $"{variation + 1:00} - {VariationNames[variation]} - Auto Connect";
                tile.fixedVariation = variation < VariationCount ? variation : SunleafPlatformTile.MixedVariation;
                tile.standaloneSprites = GetStateSprites(sprites, 0);
                tile.leftEndSprites = GetStateSprites(sprites, 1);
                tile.middleSprites = GetStateSprites(sprites, 2);
                tile.rightEndSprites = GetStateSprites(sprites, 3);
                AssetDatabase.CreateAsset(tile, path);
                tiles[variation] = tile;
            }
            AssetDatabase.SaveAssets();
            CreatePaletteAndExamples(tiles);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return $"Created/verified {VariationCount * StateCount} capped/connecting sprites and {tiles.Length} automatic brushes. Palette: {PalettePath}. Existing level platforms were not modified.";
        }
        finally { Object.DestroyImmediate(source); }
    }

    /// <summary>Opens the populated palette and selects a dedicated, collision-ready painting layer.</summary>
    [BeziAction("Open the Sunleaf mossy Tile Palette and select its dedicated tilemap. Creates only the new painting layer if absent; never paints example tiles into the scene.")]
    public static string OpenPalette()
    {
        RequireEditMode();
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        if (palette == null) throw new InvalidOperationException("Build the mossy palette before opening it.");
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            throw new InvalidOperationException("Exit Prefab Mode and open your level before using the painting shortcut.");
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
            throw new InvalidOperationException("Open a saved level scene before selecting a painting layer.");
        GameObject session = scene.GetRootGameObjects().FirstOrDefault(root => root.name == SessionName);
        if (session == null) throw new InvalidOperationException("The active scene has no Main Game Session. Open a Sunleaf level first.");
        Transform gridTransform = session.transform.Find(GridName);
        if (gridTransform == null)
        {
            var gridObject = new GameObject(GridName);
            Undo.RegisterCreatedObjectUndo(gridObject, "Create manual tile grid");
            Undo.SetTransformParent(gridObject.transform, session.transform, "Parent manual tile grid");
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.transform.localRotation = Quaternion.identity;
            gridObject.transform.localScale = Vector3.one;
            Undo.AddComponent<Grid>(gridObject).cellSize = Vector3.one;
            gridTransform = gridObject.transform;
        }
        Grid grid = gridTransform.GetComponent<Grid>();
        if (grid == null || grid.cellSize != Vector3.one || grid.cellGap != Vector3.zero ||
            grid.cellLayout != GridLayout.CellLayout.Rectangle || grid.cellSwizzle != GridLayout.CellSwizzle.XYZ ||
            grid.transform.lossyScale != Vector3.one || grid.transform.rotation != Quaternion.identity)
            throw new InvalidOperationException("The mossy kit requires an unscaled, unrotated 1x1 XY grid. Existing grid settings were not changed.");
        Transform targetTransform = gridTransform.Find(TargetName);
        if (targetTransform == null)
        {
            var target = new GameObject(TargetName);
            Undo.RegisterCreatedObjectUndo(target, "Create mossy painting layer");
            Undo.SetTransformParent(target.transform, gridTransform, "Parent mossy painting layer");
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
            Tilemap tilemap = Undo.AddComponent<Tilemap>(target);
            tilemap.tileAnchor = new Vector3(HalfCell, HalfCell, 0f);
            Undo.AddComponent<TilemapRenderer>(target).sortingOrder = SortingOrder;
            Undo.AddComponent<Rigidbody2D>(target).bodyType = RigidbodyType2D.Static;
            CompositeCollider2D composite = Undo.AddComponent<CompositeCollider2D>(target);
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            TilemapCollider2D collider = Undo.AddComponent<TilemapCollider2D>(target);
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            collider.extrusionFactor = CollisionExtrusion;
            targetTransform = target.transform;
            EditorSceneManager.MarkSceneDirty(scene);
        }
        if (targetTransform.GetComponent<Tilemap>() == null)
            throw new InvalidOperationException("The existing mossy target has no Tilemap. It was not replaced.");
        EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
        GridPaintingState.palette = palette;
        Selection.activeGameObject = targetTransform.gameObject;
        GridPaintingState.scenePaintTarget = targetTransform.gameObject;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.in2DMode = true;
        return $"Opened {PaletteName}. Paint target: {SessionName}/{GridName}/{TargetName}. Rows top to bottom: {string.Join(", ", VariationNames)}. Pick a single tile from the left column to paint; right-hand runs are examples. Horizontal connections only. Save the scene after painting.";
    }

    /// <summary>Checks saved sprites, joins, erase refresh, mixed variations, palette registration and collision.</summary>
    [BeziAction("Validate the Sunleaf connecting palette with isolated paint/erase tests, sprite boundary checks and flat composite collision checks.", IsReadOnly = true)]
    public static string ValidatePalette()
    {
        RequireEditMode();
        int assertions = 0;
        Action<bool, string> check = (condition, message) =>
        {
            assertions++;
            if (!condition) throw new InvalidOperationException("Mossy palette validation: " + message);
        };
        SunleafPlatformTile[] tiles = Enumerable.Range(0, VariationNames.Length)
            .Select(index => AssetDatabase.LoadAssetAtPath<SunleafPlatformTile>(GetTilePath(index))).ToArray();
        check(tiles.All(tile => tile != null), "All five brush assets must exist.");
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        check(palette != null, "The palette prefab must exist.");
        check(AssetDatabase.LoadAllAssetsAtPath(PalettePath).OfType<GridPalette>().Any(), "The prefab must include GridPalette metadata.");
        check(GridPaintingState.palettes.Contains(palette), "Unity must discover the palette.");
        check(palette.GetComponentInChildren<Tilemap>().GetUsedTilesCount() == tiles.Length, "The palette must contain all five brushes.");
        foreach (SunleafPlatformTile tile in tiles)
        {
            foreach (Sprite[] sprites in new[] { tile.standaloneSprites, tile.leftEndSprites, tile.middleSprites, tile.rightEndSprites })
            {
                check(sprites.Length == VariationCount && sprites.All(sprite => sprite != null), "Every brush requires four complete sprite sets.");
                foreach (Sprite sprite in sprites)
                {
                    check(Mathf.Abs(sprite.bounds.size.x - 1f) < ShapeTolerance, "Sprites must be exactly one grid cell wide.");
                    var shape = new List<Vector2>();
                    check(sprite.GetPhysicsShapeCount() == 1, "Each sprite must have one authored collision outline.");
                    sprite.GetPhysicsShape(0, shape);
                    check(shape.Count == 4, "Hanging leaves must not become collision vertices.");
                    check(Mathf.Abs(shape.Max(point => point.y) - HalfCell) < ShapeTolerance, "Collision top must align with the cell's top edge.");
                }
            }
        }
        ValidateAtlasBoundaries(check);
        Scene previewScene = EditorSceneManager.NewPreviewScene();
        Tile unrelated = ScriptableObject.CreateInstance<Tile>();
        try
        {
            GameObject root = new GameObject("Mossy palette verification", typeof(Grid));
            SceneManager.MoveGameObjectToScene(root, previewScene);
            var target = new GameObject("Verification Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
            target.transform.SetParent(root.transform, false);
            Tilemap map = target.GetComponent<Tilemap>();
            map.tileAnchor = new Vector3(HalfCell, HalfCell, 0f);
            map.SetTile(Vector3Int.zero, tiles[0]);
            check(map.GetSprite(Vector3Int.zero) == tiles[0].standaloneSprites[0], "An isolated tile needs both caps.");
            map.SetTile(Vector3Int.right, tiles[1]);
            check(map.GetSprite(Vector3Int.zero) == tiles[0].leftEndSprites[0], "Painting to the right must update the old tile to a left cap.");
            check(map.GetSprite(Vector3Int.right) == tiles[1].rightEndSprites[1], "Different styles must join.");
            map.SetTile(Vector3Int.left, tiles[2]);
            check(map.GetSprite(Vector3Int.zero) == tiles[0].middleSprites[0], "Two neighbours require an uncapped middle.");
            map.SetTile(Vector3Int.right, null);
            check(map.GetSprite(Vector3Int.zero) == tiles[0].rightEndSprites[0], "Erasing the right neighbour must restore the right cap immediately.");
            map.SetTile(Vector3Int.left, null);
            check(map.GetSprite(Vector3Int.zero) == tiles[0].standaloneSprites[0], "Erasing both neighbours must restore the standalone piece.");
            map.SetTile(Vector3Int.right, unrelated);
            map.SetTile(Vector3Int.up, tiles[1]);
            check(map.GetSprite(Vector3Int.zero) == tiles[0].standaloneSprites[0], "Unrelated tiles and vertical neighbours must not join.");
            map.ClearAllTiles();
            var mixedPosition = new Vector3Int(-11, -3, 0);
            map.SetTile(mixedPosition, tiles[VariationCount]);
            Sprite mixedSprite = map.GetSprite(mixedPosition);
            map.RefreshAllTiles();
            check(map.GetSprite(mixedPosition) == mixedSprite && tiles[VariationCount].standaloneSprites.Contains(mixedSprite), "Mixed variation must remain stable at negative coordinates.");
            var selectedSprites = new HashSet<Sprite>();
            for (int index = 0; index < ExampleLength * VariationCount; index++)
            {
                Vector3Int position = new Vector3Int(index * RowSpacing, 0, 0);
                map.SetTile(position, tiles[VariationCount]);
                selectedSprites.Add(map.GetSprite(position));
            }
            check(selectedSprites.Count > 1, "The mixed brush must actually provide visual variety.");
            map.ClearAllTiles();
            for (int column = 0; column < ExampleLength; column++)
                map.SetTile(new Vector3Int(column, 0, 0), tiles[column % tiles.Length]);
            target.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            CompositeCollider2D composite = target.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            composite.generationType = CompositeCollider2D.GenerationType.Manual;
            TilemapCollider2D collider = target.AddComponent<TilemapCollider2D>();
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            collider.extrusionFactor = CollisionExtrusion;
            collider.ProcessTilemapChanges();
            composite.GenerateGeometry();
            check(composite.pathCount == 1, "Adjacent platform cells must form one solid collider path.");
            var path = new Vector2[composite.GetPathPointCount(0)];
            composite.GetPath(0, path);
            check(Mathf.Abs(path.Max(point => point.y) - 1f) < ShapeTolerance + CollisionExtrusion, "Composite walkline must stay at y=1.");
            check(path.Max(point => point.x) - path.Min(point => point.x) > ExampleLength - HalfCell, "Composite must span the connected row.");
        }
        finally
        {
            Object.DestroyImmediate(unrelated);
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
        return $"PASS: {assertions} palette checks. Paint/erase recapping, cross-style connections, stable natural variation, shared art boundaries, Unity palette discovery, and merged flat walkline collision verified. No level tiles changed.";
    }

    private static void BuildAtlas(Texture2D source)
    {
        Color32[] pixels = source.GetPixels32();
        RectInt bounds = FindVisibleBounds(pixels, source.width, source.height);
        int atlasWidth = TileWidth * StateCount;
        int atlasHeight = source.height * VariationCount;
        var atlasPixels = new Color32[atlasWidth * atlasHeight];
        int leftSource = Mathf.Max(0, bounds.xMin - TransparentPadding);
        int rightSource = Mathf.Min(source.width - CapWidth, bounds.xMax + TransparentPadding - CapWidth);
        for (int variation = 0; variation < VariationCount; variation++)
        {
            Color32[] middle = ExtractStrip(pixels, source.width, source.height, VariationSourceX[variation], TileWidth);
            Color32[] leftJoin = ExtractStrip(pixels, source.width, source.height, SharedSeamSourceX, EdgeWidth);
            Color32[] rightJoin = ExtractStrip(pixels, source.width, source.height, SharedSeamSourceX - EdgeWidth, EdgeWidth);
            StitchPatch(middle, leftJoin, source.height, 0, EdgeWidth, true);
            StitchPatch(middle, rightJoin, source.height, TileWidth - EdgeWidth, EdgeWidth, false);
            Color32[] leftCap = ExtractStrip(pixels, source.width, source.height, leftSource, CapWidth);
            Color32[] rightCap = ExtractStrip(pixels, source.width, source.height, rightSource, CapWidth);
            for (int state = 0; state < StateCount; state++)
            {
                Color32[] tile = (Color32[])middle.Clone();
                if (state == 0 || state == 1) StitchPatch(tile, leftCap, source.height, 0, CapWidth, true);
                if (state == 0 || state == 3) StitchPatch(tile, rightCap, source.height, TileWidth - CapWidth, CapWidth, false);
                for (int row = 0; row < source.height; row++)
                    Array.Copy(tile, row * TileWidth, atlasPixels,
                        state * TileWidth + (variation * source.height + row) * atlasWidth, TileWidth);
            }
        }
        var atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);
        try
        {
            atlas.SetPixels32(atlasPixels);
            atlas.Apply(false, false);
            File.WriteAllBytes(AtlasPath, atlas.EncodeToPNG());
        }
        finally { Object.DestroyImmediate(atlas); }
        AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        importer.textureType = TextureImporterType.Default;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.alphaIsTransparency = true;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = MaximumTextureSize;
        importer.isReadable = false;
        importer.SaveAndReimport();
    }

    // Minimum-error quilting selects source pixels instead of blurring or cross-fading pixel art.
    private static void StitchPatch(Color32[] target, Color32[] patch, int height, int startColumn, int patchWidth, bool patchOnLeft)
    {
        int overlapStart = patchOnLeft ? patchWidth - StitchOverlap : 0;
        var costs = new float[height * StitchOverlap];
        var parents = new int[height * StitchOverlap];
        for (int row = 0; row < height; row++)
        for (int column = 0; column < StitchOverlap; column++)
        {
            int patchColumn = overlapStart + column;
            float difference = PixelDifference(target[startColumn + patchColumn + row * TileWidth], patch[patchColumn + row * patchWidth]);
            int bestParent = column;
            float previousCost = 0f;
            if (row > 0)
            {
                previousCost = float.PositiveInfinity;
                for (int candidate = Mathf.Max(0, column - 1); candidate <= Mathf.Min(StitchOverlap - 1, column + 1); candidate++)
                {
                    float candidateCost = costs[(row - 1) * StitchOverlap + candidate];
                    if (candidateCost >= previousCost) continue;
                    previousCost = candidateCost;
                    bestParent = candidate;
                }
            }
            costs[row * StitchOverlap + column] = difference + previousCost;
            parents[row * StitchOverlap + column] = bestParent;
        }
        int seamColumn = 0;
        for (int column = 1; column < StitchOverlap; column++)
            if (costs[(height - 1) * StitchOverlap + column] < costs[(height - 1) * StitchOverlap + seamColumn]) seamColumn = column;
        for (int row = height - 1; row >= 0; row--)
        {
            int cut = overlapStart + seamColumn;
            for (int column = 0; column < patchWidth; column++)
                if (patchOnLeft ? column <= cut : column >= cut)
                    target[startColumn + column + row * TileWidth] = patch[column + row * patchWidth];
            seamColumn = parents[row * StitchOverlap + seamColumn];
        }
    }

    private static float PixelDifference(Color32 first, Color32 second)
    {
        float firstAlpha = first.a / (float)byte.MaxValue;
        float secondAlpha = second.a / (float)byte.MaxValue;
        float red = first.r * firstAlpha - second.r * secondAlpha;
        float green = first.g * firstAlpha - second.g * secondAlpha;
        float blue = first.b * firstAlpha - second.b * secondAlpha;
        float alpha = first.a - second.a;
        return red * red + green * green + blue * blue + alpha * alpha * AlphaErrorWeight;
    }

    private static Sprite CreateSprite(Texture2D atlas, int height, int variation, int state)
    {
        string spriteName = $"{VariationNames[variation]} - {StateNames[state]}";
        string path = $"{SpriteFolder}/{spriteName}.asset";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        float pivotY = (WalklinePixels - TileWidth * HalfCell) / height;
        if (sprite == null)
        {
            sprite = Sprite.Create(atlas, new Rect(state * TileWidth, variation * height, TileWidth, height),
                new Vector2(HalfCell, pivotY), TileWidth, 0, SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.name = spriteName;
            AssetDatabase.CreateAsset(sprite, path);
        }
        float left = state == 0 || state == 1 ? CapCollisionInsetPixels : 0;
        float right = state == 0 || state == 3 ? TileWidth - CapCollisionInsetPixels : TileWidth;
        Vector2[] outlinePixels =
        {
            new Vector2(left, WalklinePixels - CollisionDepthPixels),
            new Vector2(left, WalklinePixels),
            new Vector2(right, WalklinePixels),
            new Vector2(right, WalklinePixels - CollisionDepthPixels)
        };
        Undo.RecordObject(sprite, "Set mossy sprite collision outline");
        // Persistent Sprite assets need serialized unit-space outlines rather than
        // runtime OverridePhysicsShape calls, so collision survives asset reimport.
        var serializedSprite = new SerializedObject(sprite);
        SerializedProperty shapes = serializedSprite.FindProperty("m_PhysicsShape");
        if (shapes == null || !shapes.isArray)
            throw new InvalidOperationException("This Unity version does not expose Sprite physics outlines for saving.");
        shapes.arraySize = 1;
        SerializedProperty outline = shapes.GetArrayElementAtIndex(0);
        outline.arraySize = outlinePixels.Length;
        for (int index = 0; index < outlinePixels.Length; index++)
            outline.GetArrayElementAtIndex(index).vector2Value = (outlinePixels[index] - sprite.pivot) / sprite.pixelsPerUnit;
        serializedSprite.ApplyModifiedProperties();
        EditorUtility.SetDirty(sprite);
        return sprite;
    }

    private static void CreatePaletteAndExamples(SunleafPlatformTile[] tiles)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null)
        {
            GridPaletteUtility.CreateNewPalette(OutputFolder, PaletteName, GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Manual, Vector3.one, GridLayout.CellSwizzle.XYZ);
            GameObject contents = PrefabUtility.LoadPrefabContents(PalettePath);
            try
            {
                Tilemap map = contents.GetComponentInChildren<Tilemap>();
                map.name = "Pick left column - auto-connecting ledges";
                FillExamples(map, tiles);
                PrefabUtility.SaveAsPrefabAsset(contents, PalettePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PreviewPath) != null) return;
        Scene previewScene = EditorSceneManager.NewPreviewScene();
        try
        {
            var root = new GameObject("Sunleaf Mossy Examples - standalone and connected rows", typeof(Grid));
            SceneManager.MoveGameObjectToScene(root, previewScene);
            var child = new GameObject("Fern - Flowers - Stone - Vines - Natural Mix", typeof(Tilemap), typeof(TilemapRenderer));
            child.transform.SetParent(root.transform, false);
            FillExamples(child.GetComponent<Tilemap>(), tiles);
            PrefabUtility.SaveAsPrefabAsset(root, PreviewPath);
        }
        finally { EditorSceneManager.ClosePreviewScene(previewScene); }
    }

    private static void FillExamples(Tilemap map, SunleafPlatformTile[] tiles)
    {
        map.tileAnchor = new Vector3(HalfCell, HalfCell, 0f);
        for (int row = 0; row < tiles.Length; row++)
        {
            map.SetTile(new Vector3Int(0, -row * RowSpacing, 0), tiles[row]);
            for (int column = 0; column < ExampleLength; column++)
                map.SetTile(new Vector3Int(column + RunStartColumn, -row * RowSpacing, 0), tiles[row]);
        }
        map.RefreshAllTiles();
        map.CompressBounds();
    }

    private static void ValidateAtlasBoundaries(Action<bool, string> check)
    {
        Texture2D atlas = LoadPixels(AtlasPath);
        try
        {
            Color32[] pixels = atlas.GetPixels32();
            int height = atlas.height / VariationCount;
            for (int variation = 0; variation < VariationCount; variation++)
            {
                bool leftMatches = true;
                bool rightMatches = true;
                bool exposedEndsTransparent = true;
                for (int row = 0; row < height; row++)
                {
                    int rowOffset = (variation * height + row) * atlas.width;
                    Color32 left = pixels[rowOffset + TileWidth * 2];
                    Color32 right = pixels[rowOffset + TileWidth * 3 - 1];
                    leftMatches &= left.Equals(pixels[row * atlas.width + TileWidth * 2]);
                    leftMatches &= left.Equals(pixels[rowOffset + TileWidth * 3]);
                    rightMatches &= right.Equals(pixels[row * atlas.width + TileWidth * 3 - 1]);
                    rightMatches &= right.Equals(pixels[rowOffset + TileWidth * 2 - 1]);
                    exposedEndsTransparent &= pixels[rowOffset].a <= VisibleAlpha && pixels[rowOffset + TileWidth - 1].a <= VisibleAlpha;
                }
                check(leftMatches && rightMatches, "All styles must share matching connecting boundary strips.");
                check(exposedEndsTransparent, "Standalone sprites must preserve transparent padding outside both finished caps.");
            }
        }
        finally { Object.DestroyImmediate(atlas); }
    }

    private static Sprite[] GetStateSprites(Sprite[,] sprites, int state)
    {
        return Enumerable.Range(0, VariationCount).Select(variation => sprites[variation, state]).ToArray();
    }

    private static Color32[] ExtractStrip(Color32[] pixels, int width, int height, int startColumn, int stripWidth)
    {
        var strip = new Color32[stripWidth * height];
        for (int row = 0; row < height; row++) Array.Copy(pixels, startColumn + row * width, strip, row * stripWidth, stripWidth);
        return strip;
    }

    private static RectInt FindVisibleBounds(Color32[] pixels, int width, int height)
    {
        int minX = width;
        int maxX = -1;
        int minY = height;
        int maxY = -1;
        for (int row = 0; row < height; row++)
        for (int column = 0; column < width; column++)
        {
            if (pixels[column + row * width].a <= VisibleAlpha) continue;
            minX = Mathf.Min(minX, column);
            maxX = Mathf.Max(maxX, column);
            minY = Mathf.Min(minY, row);
            maxY = Mathf.Max(maxY, row);
        }
        if (maxX < minX) throw new InvalidOperationException("Source artwork is entirely transparent.");
        return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static Texture2D LoadPixels(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Required palette artwork is missing.", path);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (ImageConversion.LoadImage(texture, File.ReadAllBytes(path), false)) return texture;
        Object.DestroyImmediate(texture);
        throw new InvalidOperationException("Could not decode " + path);
    }

    private static string GetTilePath(int variation) => $"{TileFolder}/{variation + 1:00} - {VariationNames[variation]} - Auto Connect.asset";

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(parent)) throw new InvalidOperationException("Invalid generated asset folder.");
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    private static void RequireEditMode()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Wait for compilation and exit Play Mode before editing the mossy palette.");
    }
}
