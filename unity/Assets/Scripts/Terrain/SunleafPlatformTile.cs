using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IWantToBeTheHero
{
    /// <summary>A horizontal ledge tile that joins compatible neighbours and restores exposed end caps.</summary>
    [CreateAssetMenu(menuName = "Sunleaf/Connecting Platform Tile")]
    public sealed class SunleafPlatformTile : TileBase
    {
        public const string MossyConnectionGroup = "Sunleaf Mossy Platforms";
        public const int MixedVariation = -1;
        private const int LeftNeighbourOffset = -1;
        private const int RightNeighbourOffset = 1;
        private const uint HorizontalHash = 73856093u;
        private const uint VerticalHash = 19349663u;
        private const uint HashSalt = 83492791u;

        [Tooltip("Tiles in this family join horizontally, including different artwork variations.")]
        public string connectionGroup = MossyConnectionGroup;
        [Tooltip("-1 selects a deterministic variation per cell; otherwise selects an artwork index.")]
        public int fixedVariation = MixedVariation;
        public Sprite[] standaloneSprites = Array.Empty<Sprite>();
        public Sprite[] leftEndSprites = Array.Empty<Sprite>();
        public Sprite[] middleSprites = Array.Empty<Sprite>();
        public Sprite[] rightEndSprites = Array.Empty<Sprite>();

        /// <summary>Refreshes this cell and both horizontal neighbours after painting or erasing.</summary>
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            for (int offset = LeftNeighbourOffset; offset <= RightNeighbourOffset; offset++)
                tilemap.RefreshTile(position + new Vector3Int(offset, 0, 0));
        }

        /// <summary>Selects a complete, capped, or connecting sprite without runtime-generated objects.</summary>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            bool hasLeft = ConnectsTo(tilemap.GetTile(position + Vector3Int.left));
            bool hasRight = ConnectsTo(tilemap.GetTile(position + Vector3Int.right));
            Sprite[] choices = hasLeft
                ? (hasRight ? middleSprites : rightEndSprites)
                : (hasRight ? leftEndSprites : standaloneSprites);

            tileData.sprite = choices != null && choices.Length > 0 ? choices[GetVariation(position, choices.Length)] : null;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.gameObject = null;
            tileData.flags = TileFlags.LockAll;
            tileData.colliderType = tileData.sprite != null ? Tile.ColliderType.Sprite : Tile.ColliderType.None;
        }

        private bool ConnectsTo(TileBase neighbour)
        {
            return neighbour is SunleafPlatformTile other &&
                   string.Equals(connectionGroup, other.connectionGroup, StringComparison.Ordinal);
        }

        private int GetVariation(Vector3Int position, int count)
        {
            if (fixedVariation != MixedVariation)
                return Mathf.Clamp(fixedVariation, 0, count - 1);
            uint hash = unchecked((uint)position.x * HorizontalHash ^ (uint)position.y * VerticalHash ^ HashSalt);
            hash ^= hash >> 16;
            return (int)(hash % (uint)count);
        }
    }
}
