// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// A preset that describes a tile system which can be used to quickly create tile
    /// systems with common configurations.
    /// </summary>
    /// <seealso cref="TileSystem"/>
    /// <seealso cref="TileSystemPresetUtility"/>
    public sealed class TileSystemPreset : ScriptableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileSystemPreset"/> class.
        /// </summary>
        public TileSystemPreset()
        {
            this.ReduceColliders = new ReduceColliderOptions();
            this.SetDefaults3D();
        }


        [SerializeField, FormerlySerializedAs("_systemName")]
        private string systemName;


        /// <summary>
        /// Gets or sets the default name for the new tile system.
        /// </summary>
        public string SystemName {
            get { return this.systemName; }
            set { this.systemName = value; }
        }


        #region Grid

        [SerializeField, FormerlySerializedAs("_tileWidth")]
        private float tileWidth;
        [SerializeField, FormerlySerializedAs("_tileHeight")]
        private float tileHeight;
        [SerializeField, FormerlySerializedAs("_tileDepth")]
        private float tileDepth;

        /// <summary>
        /// Gets or sets the width of a tile.
        /// </summary>
        public float TileWidth {
            get { return this.tileWidth; }
            set { this.tileWidth = value; }
        }

        /// <summary>
        /// Gets or sets the height of a tile.
        /// </summary>
        public float TileHeight {
            get { return this.tileHeight; }
            set { this.tileHeight = value; }
        }

        /// <summary>
        /// Gets or sets the depth of a tile.
        /// </summary>
        public float TileDepth {
            get { return this.tileDepth; }
            set { this.tileDepth = value; }
        }

        [SerializeField, FormerlySerializedAs("_rows")]
        private int rows;
        [SerializeField, FormerlySerializedAs("_columns")]
        private int columns;

        /// <summary>
        /// Gets or sets the number of rows in tile system.
        /// </summary>
        public int Rows {
            get { return this.rows; }
            set { this.rows = value; }
        }

        /// <summary>
        /// Gets or sets the number of columns in tile system.
        /// </summary>
        public int Columns {
            get { return this.columns; }
            set { this.columns = value; }
        }

        [SerializeField, FormerlySerializedAs("_chunkWidth")]
        private int chunkWidth;
        [SerializeField, FormerlySerializedAs("_chunkHeight")]
        private int chunkHeight;

        /// <summary>
        /// Gets or sets the width of a chunk (in tiles).
        /// </summary>
        /// <remarks>
        /// Splitting a tile system into chunks can be useful for:
        /// <list type="bullet">
        ///     <item>Specialised game play logic</item>
        ///     <item>Performance optimizations</item>
        /// </list>
        /// <para>Avoid splitting tile system into too many chunks.</para>
        /// </remarks>
        /// <seealso cref="ChunkHeight"/>
        public int ChunkWidth {
            get { return this.chunkWidth; }
            set { this.chunkWidth = value; }
        }

        /// <summary>
        /// Gets or sets the height of a chunk (in tiles).
        /// </summary>
        /// <remarks>
        /// Splitting a tile system into chunks can be useful for:
        /// <list type="bullet">
        ///     <item>Specialised game play logic</item>
        ///     <item>Performance optimizations</item>
        /// </list>
        /// <para>Avoid splitting tile system into too many chunks.</para>
        /// </remarks>
        /// <seealso cref="ChunkWidth"/>
        public int ChunkHeight {
            get { return this.chunkHeight; }
            set { this.chunkHeight = value; }
        }

        [SerializeField, FormerlySerializedAs("_autoAdjustDirection")]
        private bool autoAdjustDirection;
        [SerializeField, FormerlySerializedAs("_tilesFacing")]
        private TileFacing tilesFacing;
        [SerializeField, FormerlySerializedAs("_direction")]
        private WorldDirection direction;

        /// <summary>
        /// Gets or sets the direction that painted tiles will face.
        /// </summary>
        public TileFacing TilesFacing {
            get { return this.tilesFacing; }
            set { this.tilesFacing = value; }
        }

        /// <summary>
        /// Gets or sets the initial direction of the tile system.
        /// </summary>
        public WorldDirection Direction {
            get { return this.direction; }
            set { this.direction = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Direction"/> property
        /// should be automatically adjusted whenever the <see cref="TilesFacing"/>
        /// property is modified.
        /// </summary>
        internal bool AutoAdjustDirection {
            get { return this.autoAdjustDirection; }
            set { this.autoAdjustDirection = value; }
        }

        #endregion


        #region Stripping

        [SerializeField, FormerlySerializedAs("_strippingPreset")]
        private StrippingPreset strippingPreset;
        [SerializeField, FormerlySerializedAs("_strippingOptions")]
        private int strippingOptions;

        /// <summary>
        /// Gets or sets the stripping preset.
        /// </summary>
        /// <remarks>
        /// <para>Defaults to <see cref="Rotorz.Tile.StrippingPreset"/>.</para>
        /// </remarks>
        public StrippingPreset StrippingPreset {
            get { return this.strippingPreset; }
            set { this.strippingPreset = value; }
        }

        /// <summary>
        /// Gets or sets options for custom stripping.
        /// </summary>
        public int StrippingOptions {
            get { return this.strippingOptions; }
            set { this.strippingOptions = value; }
        }

        #endregion


        #region Build Options

        [SerializeField, FormerlySerializedAs("_combineMethod")]
        private BuildCombineMethod combineMethod;
        [SerializeField, FormerlySerializedAs("_combineChunkWidth")]
        private int combineChunkWidth;
        [SerializeField, FormerlySerializedAs("_combineChunkHeight")]
        private int combineChunkHeight;
        [SerializeField, FormerlySerializedAs("_combineIntoSubmeshes")]
        private bool combineIntoSubmeshes;

        /// <summary>
        /// Gets or sets the method of combining to perform upon build.
        /// </summary>
        public BuildCombineMethod CombineMethod {
            get { return this.combineMethod; }
            set { this.combineMethod = value; }
        }

        /// <summary>
        /// Gets or sets the width of a tile chunk to combine (in tiles).
        /// </summary>
        public int CombineChunkWidth {
            get { return this.combineChunkWidth; }
            set { this.combineChunkWidth = value; }
        }

        /// <summary>
        /// Gets or sets the height of a tile chunk to combine (in tiles).
        /// </summary>
        public int CombineChunkHeight {
            get { return this.combineChunkHeight; }
            set { this.combineChunkHeight = value; }
        }

        /// <summary>
        /// Gets or sets whether tile meshes should be combined into submeshes when
        /// they contain multiple materials. Disable to create individual mesh objects
        /// on a per-material basis.
        /// </summary>
        public bool CombineIntoSubmeshes {
            get { return this.combineIntoSubmeshes; }
            set { this.combineIntoSubmeshes = value; }
        }

        [SerializeField, FormerlySerializedAs("_staticVertexSnapping")]
        private bool staticVertexSnapping;
        [SerializeField, FormerlySerializedAs("_vertexSnapThreshold")]
        private float vertexSnapThreshold;

        /// <summary>
        /// Gets or sets whether vertex snapping should be applied during build process.
        /// </summary>
        /// <remarks>
        /// Vertex snapping is always applied for tiles that are painted using
        /// "smooth" brushes. However snapping can be enabled/disabled for tiles
        /// that are painted using non smooth "static" brushes.
        /// </remarks>
        public bool StaticVertexSnapping {
            get { return this.staticVertexSnapping; }
            set { this.staticVertexSnapping = value; }
        }

        /// <summary>
        /// Gets or sets vertex snap threshold for vertex snapping and smoothing.
        /// </summary>
        public float VertexSnapThreshold {
            get { return this.vertexSnapThreshold; }
            set { this.vertexSnapThreshold = value; }
        }

        [SerializeField, FormerlySerializedAs("_generateSecondUVs")]
        private bool generateSecondUVs;

        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_hardAngle")]
        internal float secondUVsHardAngle;
        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_packMargin")]
        internal float secondUVsPackMargin;
        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_angleError")]
        internal float secondUVsAngleError;
        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_areaError")]
        internal float secondUVsAreaError;

        /// <summary>
        /// Gets or sets whether second set of UVs should be generated.
        /// </summary>
        public bool GenerateSecondUVs {
            get { return this.generateSecondUVs; }
            set { this.generateSecondUVs = value; }
        }

        /// <summary>
        /// Gets or sets advanced paramaters used when generating secondary UVs.
        /// </summary>
        public UnwrapParam GenerateSecondUVsParams {
            get {
                UnwrapParam param = new UnwrapParam();
                param.hardAngle = this.secondUVsHardAngle;
                param.packMargin = this.secondUVsPackMargin;
                param.angleError = this.secondUVsAngleError;
                param.areaError = this.secondUVsAreaError;
                return param;
            }
            set {
                this.secondUVsHardAngle = value.hardAngle;
                this.secondUVsPackMargin = value.packMargin;
                this.secondUVsAngleError = value.angleError;
                this.secondUVsAreaError = value.areaError;
            }
        }

        [SerializeField, FormerlySerializedAs("_pregenerateProcedural")]
        private bool pregenerateProcedural;

        /// <summary>
        /// Gets or sets whether tiles that are procedurally generated should be
        /// pre-generated when building tile system.
        /// </summary>
        public bool PregenerateProcedural {
            get { return this.pregenerateProcedural; }
            set { this.pregenerateProcedural = value; }
        }

        [SerializeField, FormerlySerializedAs("_reduceColliders")]
        private ReduceColliderOptions reduceColliders;

        /// <summary>
        /// Gets options which are considered when reducing colliders.
        /// </summary>
        public ReduceColliderOptions ReduceColliders {
            get {
                if (this.reduceColliders == null)
                    this.reduceColliders = new ReduceColliderOptions();
                return this.reduceColliders;
            }
            private set {
                this.reduceColliders = value;
            }
        }

        #endregion


        #region Runtime Options

        [SerializeField, FormerlySerializedAs("_hintEraseEmptyChunks")]
        private bool hintEraseEmptyChunks;
        [SerializeField, FormerlySerializedAs("_applyRuntimeStripping")]
        private bool applyRuntimeStripping;

        /// <summary>
        /// Gets or sets a hint of whether empty chunks should be erased at runtime.
        /// </summary>
        public bool HintEraseEmptyChunks {
            get { return this.hintEraseEmptyChunks; }
            set { this.hintEraseEmptyChunks = value; }
        }

        /// <summary>
        /// Gets or sets whether to apply basic level of stripping at runtime.
        /// </summary>
        public bool ApplyRuntimeStripping {
            get { return this.applyRuntimeStripping; }
            set { this.applyRuntimeStripping = value; }
        }

        [SerializeField, FormerlySerializedAs("_updateProceduralAtStart")]
        private bool updateProceduralAtStart;
        [SerializeField, FormerlySerializedAs("_markProceduralDynamic")]
        private bool markProceduralDynamic;
        [SerializeField, FormerlySerializedAs("_addProceduralNormals")]
        private bool addProceduralNormals;

        /// <summary>
        /// Gets or sets whether procedural meshes should be updated when tile system
        /// becomes active.
        /// </summary>
        public bool UpdateProceduralAtStart {
            get { return this.updateProceduralAtStart; }
            set { this.updateProceduralAtStart = value; }
        }

        /// <summary>
        /// Gets or sets whether procedural meshes should be marked as dynamic.
        /// </summary>
        public bool MarkProceduralDynamic {
            get { return this.markProceduralDynamic; }
            set { this.markProceduralDynamic = value; }
        }

        /// <summary>
        /// Gets or sets whether normals should be added to procedural meshes.
        /// </summary>
        public bool AddProceduralNormals {
            get { return this.addProceduralNormals; }
            set { this.addProceduralNormals = value; }
        }

        [SerializeField, FormerlySerializedAs("_sortingLayerID")]
        private int sortingLayerID;
        [SerializeField, FormerlySerializedAs("_sortingOrder")]
        private int sortingOrder;

        /// <summary>
        /// Gets or sets identifier of sorting layer which is used to control render
        /// order of procedurally generated meshes.
        /// </summary>
        public int SortingLayerID {
            get { return this.sortingLayerID; }
            set { this.sortingLayerID = value; }
        }

        /// <summary>
        /// Gets or sets order in sorting layers which is used to control render order of
        /// procedurally generated meshes.
        /// </summary>
        public int SortingOrder {
            get { return this.sortingOrder; }
            set { this.sortingOrder = value; }
        }

        #endregion


        /// <summary>
        /// Set to the default values for a tile system that has 3D tiles.
        /// </summary>
        public void SetDefaults3D()
        {
            this.SystemName = "Untitled Tile System";

            this.TileWidth = 1f;
            this.TileHeight = 1f;
            this.TileDepth = 1f;

            this.Rows = 100;
            this.Columns = 100;

            this.ChunkWidth = 30;
            this.ChunkHeight = 30;

            this.AutoAdjustDirection = true;
            this.TilesFacing = TileFacing.Sideways;
            this.Direction = WorldDirection.Forward;

            // Stripping
            this.StrippingPreset = StrippingPreset.StripRuntime;
            this.StrippingOptions = 0;

            // Build Options
            this.CombineMethod = BuildCombineMethod.ByChunk;
            this.CombineChunkWidth = 30;
            this.CombineChunkHeight = 30;
            this.CombineIntoSubmeshes = true;

            this.StaticVertexSnapping = true;
            this.VertexSnapThreshold = 0.001f;

            this.GenerateSecondUVs = false;
            this.secondUVsHardAngle = 88;
            this.secondUVsPackMargin = 0.00390625f;
            this.secondUVsAngleError = 0.08f;
            this.secondUVsAreaError = 0.15f;

            this.PregenerateProcedural = false;

            this.ReduceColliders.SetDefaults();
            this.ReduceColliders.SolidTileColliderType = ColliderType.BoxCollider3D;

            // Runtime Options
            this.HintEraseEmptyChunks = false;
            this.ApplyRuntimeStripping = false;

            this.UpdateProceduralAtStart = true;
            this.MarkProceduralDynamic = true;
            this.AddProceduralNormals = false;
        }

        /// <summary>
        /// Set to the default values for a tile system that has 2D tiles.
        /// </summary>
        public void SetDefaults2D()
        {
            this.SetDefaults3D();

            this.ChunkWidth = 100;
            this.ChunkHeight = 100;

            this.ReduceColliders.SolidTileColliderType = ColliderType.BoxCollider2D;
        }
    }
}
