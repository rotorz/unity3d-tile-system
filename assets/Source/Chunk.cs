// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// A chunk is a sub-grid of tiles which forms part of an overall tile system. The
    /// size of a chunk is specified when creating a tile system though can be changed via
    /// the tile system inspector.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Chunks">Chunks</a>
    /// section of user guide for further information.</para>
    /// </intro>
    /// <remarks>
    /// <para>Here are some of the benefits of using chunks:</para>
    /// <list type="bullet">
    ///    <item>Lower memory overhead (tile data) when large areas of tile system are
    ///    empty because empty chunks can be stripped.</item>
    ///    <item>Tile system builder can combine static tiles into a single mesh on a per
    ///    chunk basis. The chunk size can be fine tuned to improve rendering performance.
    ///    When needed the combiner chunk size can be specified separately (see
    ///    <see cref="Rotorz.Tile.TileSystem.combineMethod"/>).</item>
    ///    <item>Procedural meshes that are generated to represent 2D tiles are formed on
    ///    a per chunk basis.</item>
    ///    <item>Custom game-specific scripts can be written to optimize performance as
    ///    needed. This might be used, for example, to disable all tiles within a specific
    ///    chunk based on distance.</item>
    /// </list>
    /// <para>Both the chunk map and tile data can be stripped for lower memory overhead
    /// at runtime. However, in some games these data structures can be useful.</para>
    /// </remarks>
    [AddComponentMenu(""), DisallowMultipleComponent]
    public sealed class Chunk : MonoBehaviour
    {
        /// <summary>
        /// Indicates chunk must be force refreshed upon completing bulk edit.
        /// </summary>
        internal const int FLAG_DIRTY = 1 << 18;
        /// <summary>
        /// Indicates if procedural mesh should be refreshed upon completing bulk edit.
        /// </summary>
        internal const int FLAG_PROCEDURAL_DIRTY = 1 << 19;


        private bool hasInitialized;
        private TileSystem tileSystem;


        /// <summary>
        /// Gets the tile system component.
        /// </summary>
        public TileSystem TileSystem {
            get {
                if (!this.hasInitialized) {
                    this.InitializeChunk();
                }
                return this.tileSystem;
            }
        }


        private ProceduralMesh proceduralMesh;

        /// <summary>
        /// Gets the associated procedural mesh component.
        /// </summary>
        /// <remarks>
        /// <para>Will get a value of <c>null</c> when no procedural mesh component is
        /// associated with chunk.</para>
        /// </remarks>
        public ProceduralMesh ProceduralMesh {
            get {
                if (!this.hasInitialized) {
                    this.InitializeChunk();
                }
                return this.proceduralMesh;
            }
        }


        [SerializeField, HideInInspector, FormerlySerializedAs("_firstRow")]
        private int firstRow;
        [SerializeField, HideInInspector, FormerlySerializedAs("_firstColumn")]
        private int firstColumn;

        [SerializeField, HideInInspector, FormerlySerializedAs("_flags")]
        internal int flags;


        /// <summary>
        /// Gets index of first tile in chunk.
        /// </summary>
        public TileIndex First {
            get { return new TileIndex(this.firstRow, this.firstColumn); }
            internal set {
                this.firstRow = value.row;
                this.firstColumn = value.column;
            }
        }

        /// <summary>
        /// Array of tile data.
        /// </summary>
        /// <remarks>
        /// <para>Non-existent tiles are marked with <c>null</c>.</para>
        /// </remarks>
        [SerializeField, HideInInspector]
        public TileData[] tiles;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Rotorz.Tile.Chunk"/>
        /// is dirty.
        /// </summary>
        /// <remarks>
        /// <para>Chunk can be marked as dirty when it contains one or more dirty tiles.
        /// This is designed to be used in conjunction with <see cref="Rotorz.Tile.TileSystem.ScanBrokenTiles"/>:</para>
        /// <list>
        /// <item>Indicates that contained tiles should be considered for repair when
        /// <c>lazy</c> repair is undertaken by <see cref="Rotorz.Tile.TileSystem.ScanBrokenTiles"/>.</item>
        /// <item>This flag is completely ignored when <strong>not</strong> performing
        /// <c>lazy</c> repair.</item>
        /// </list>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if dirty; otherwise <c>false</c>.
        /// </value>
        public bool Dirty {
            get { return (this.flags & FLAG_DIRTY) != 0; }
            set {
                this.flags = value
                    ? (this.flags | FLAG_DIRTY)
                    : (this.flags & ~FLAG_DIRTY)
                    ;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the procedural mesh associated with
        /// this <see cref="Rotorz.Tile.Chunk"/> needs to be updated.
        /// </summary>
        /// <value>
        /// A value of <c>true</c> if procedural mesh is dirty; otherwise <c>false</c>.
        /// </value>
        /// <seealso cref="Rotorz.Tile.TileSystem.UpdateProceduralTiles"/>
        public bool ProceduralDirty {
            get { return (this.flags & FLAG_PROCEDURAL_DIRTY) != 0; }
            set {
                this.flags = value
                    ? (this.flags | FLAG_PROCEDURAL_DIRTY)
                    : (this.flags & ~FLAG_PROCEDURAL_DIRTY)
                    ;
            }
        }

        private void InitializeChunk()
        {
            this.hasInitialized = true;

            this.tileSystem = transform.parent.GetComponent<TileSystem>();
            this.proceduralMesh = this.GetComponentInChildren<ProceduralMesh>();
        }

        /// <summary>
        /// Ensures that <see cref="ProceduralMesh"/> component is presents and returns it.
        /// </summary>
        /// <returns>
        /// The <see cref="ProceduralMesh"/> component.
        /// </returns>
        internal ProceduralMesh PrepareProceduralMesh()
        {
            if (this.ProceduralMesh == null) {
                var t = this.transform;
                this.proceduralMesh = t.GetComponentInChildren<ProceduralMesh>();

                if (this.proceduralMesh == null) {
                    // Entire procedural mesh component is missing, restore it!
                    var proceduralGO = new GameObject("_procedural");
                    var proceduralTransform = proceduralGO.transform;
                    proceduralTransform.SetParent(t, false);
                    proceduralTransform.localPosition = new Vector3(
                        this.firstColumn * this.tileSystem.CellSize.x,
                        this.firstRow * -this.tileSystem.CellSize.y,
                        0f
                    );

                    this.proceduralMesh = proceduralGO.AddComponent<ProceduralMesh>();
                    this.proceduralMesh.InitialUpdateMesh();

                    InternalUtility.HideEditorWireframe(proceduralGO);
                }
                else if (this.proceduralMesh.mesh == null) {
                    // Procedural mesh component is available, but mesh is missing.
                    this.proceduralMesh.InitialUpdateMesh();

                    InternalUtility.HideEditorWireframe(this.proceduralMesh.gameObject);
                }
            }

            return this.proceduralMesh;
        }

        /// <summary>
        /// Count the number of non-empty tiles.
        /// </summary>
        /// <remarks>
        /// <para>Avoid recomputing where possible.</para>
        /// </remarks>
        /// <returns>
        /// The number non-empty tiles.
        /// </returns>
        public int CountNonEmptyTiles()
        {
            int count = 0;
            foreach (var tile in this.tiles) {
                if (tile != null && !tile.Empty) {
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// Gets a value indicating whether chunk is void of tiles.
        /// </summary>
        /// <remarks>
        /// <para>This property is more efficient than <see cref="CountNonEmptyTiles"/>,
        /// though is still a fairly slow search function.</para>
        /// </remarks>
        public bool IsEmpty {
            get {
                for (int i = 0; i < this.tiles.Length; ++i) {
                    if (this.tiles[i] != null && !this.tiles[i].Empty) {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Calculate mid point of chunk.
        /// </summary>
        /// <remarks>
        /// <para>The calculated point will be at middle of specified size of chunk even
        /// when chunk is cropped. This is illustrated below:</para>
        /// <para><img src="../art/chunk-mid-point.png" alt="Illustration of chunk mid-points."/></para>
        /// </remarks>
        /// <param name="space">Space in which to calculate position.</param>
        /// <returns>
        /// Mid point of chunk in specified space.
        /// </returns>
        public Vector3 CalculateMidPoint(Space space = Space.World)
        {
            var tileSystem = this.TileSystem;

            if (tileSystem == null) {
                Debug.LogError("Cannot calculate center point because cannot find tile system component.");
                return Vector3.zero;
            }

            Vector3 position = new Vector3(
                (this.firstColumn + tileSystem.ChunkWidth / 2) * tileSystem.CellSize.x,
                (this.firstRow + tileSystem.ChunkHeight / 2) * -tileSystem.CellSize.y
            );

            return (space == Space.World)
                ? tileSystem.transform.localToWorldMatrix.MultiplyPoint3x4(position)
                : position;
        }

        /// <summary>
        /// Find index of tile with associated game object.
        /// </summary>
        /// <param name="obj">Transform of game object that is associated with tile or
        /// nested within the game object that is associated with tile.</param>
        /// <returns>
        /// Zero-based index of tile with game object associated; or a value of
        /// <see cref="TileIndex.invalid">TileIndex.invalid</see> when not found.
        /// </returns>
        public TileIndex FindTileIndexFromGameObject(Transform obj)
        {
            var tileSystem = this.TileSystem;
            if (tileSystem == null || obj.parent == null) {
                return TileIndex.invalid;
            }

            var chunkTransform = this.transform;

            // Find game object that is immediate child of chunk object.
            while (obj != null && obj.parent != chunkTransform) {
                obj = obj.parent;
            }
            if (obj == null) {
                return TileIndex.invalid;
            }

            var go = obj.gameObject;

            // Find object within chunk.
            for (int i = 0; i < this.tiles.Length; ++i) {
                TileData tile = this.tiles[i];
                if (tile != null && tile.gameObject == go) {
                    return new TileIndex(
                        this.firstRow + i / tileSystem.ChunkWidth,
                        this.firstColumn + i % tileSystem.ChunkWidth
                    );
                }
            }

            return TileIndex.invalid;
        }

        internal TileIndex TileIndexFromIndexOfTileInChunk(int tileIndexInChunk)
        {
            TileIndex result;
            result.row = this.First.row + tileIndexInChunk / this.TileSystem.ChunkWidth;
            result.column = this.First.column + tileIndexInChunk % this.TileSystem.ChunkWidth;
            return result;
        }
    }
}
