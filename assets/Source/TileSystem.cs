// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Delegate for tile system.
    /// </summary>
    public delegate void TileSystemDelegate(TileSystem tileSystem);

    /// <summary>
    /// Represents a method that is used to find tile when using <see cref="TileSystem.TileTrace">TileSystem.TileTraceCustom</see>.
    /// </summary>
    /// <remarks>
    /// <para>Method is invoked for each tile that is checked when performing trace.</para>
    /// </remarks>
    /// <param name="tile">Current tile.</param>
    /// <returns>
    /// A value of <c>true</c> if tile was hit; otherwise <c>false</c> to proceed to the
    /// next tile in sequence.
    /// </returns>
    public delegate bool TileTraceDelegate(TileData tile);


    /// <summary>
    /// Main tile system component which allows you to interact with a tile system using
    /// runtime or editor scripts.
    /// </summary>
    /// <intro>
    /// <para>For further information regarding the usage of tile systems refer to
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Tile-Systems">Tile Systems</a>
    /// section of the user guide.</para>
    /// </intro>
    [AddComponentMenu(""), DisallowMultipleComponent]
    public sealed class TileSystem : MonoBehaviour, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Initialize new <see cref="TileSystem"/> instance.
        /// </summary>
        public TileSystem()
        {
            // Only register tile systems in scene for Unity editor.
            TileSystemUtility.TileSystemListingDirty = true;
        }


        #region ISerializationCallbackReceiver Members

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Pre-calculated values should now be recalculated!
            this.InvalidatePrecalculatedValues = true;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Pre-calculated values should now be recalculated!
            this.InvalidatePrecalculatedValues = true;
        }

        #endregion


        /// <summary>
        /// Version of tile system extension associated with component instance.
        /// </summary>
        /// <remarks>
        /// <para>Version number is a string with the format <c>#.#.#.#</c> and can end
        /// with <c>BETA</c> or <c>ALPHA</c> when applicable. This version string may
        /// assist with future versions of this extension.</para>
        /// <para>The version string of new tile systems is initialized using <see cref="Rotorz.Tile.ProductInfo.Version"/>.</para>
        /// </remarks>
        [SerializeField]
        internal string version;

        /// <summary>
        /// Indicates whether tile system is editable.
        /// </summary>
        [SerializeField, FormerlySerializedAs("_editable")]
        internal bool isEditable = false;
        /// <summary>
        /// Indicates whether tile system has been built.
        /// </summary>
        [SerializeField, FormerlySerializedAs("_built")]
        public bool isBuilt = false;
        /// <summary>
        /// Indicates whether tile system is locked and should not be edited.
        /// </summary>
        [SerializeField, FormerlySerializedAs("_locked")]
        private bool isLocked = false;


        /// <summary>
        /// Gets a value indicating whether tile system has been built.
        /// </summary>
        public bool IsBuilt {
            get { return this.isBuilt; }
        }

        /// <summary>
        /// Gets or sets whether tile system is locked and shouldn't be edited.
        /// </summary>
        /// <remarks>
        /// <para>Whilst this property is respected where possible, it cannot be guaranteed
        /// that changes will not be made to a locked tile system. This property is respected
        /// in-editor for paint tools to prevent users from painting or erasing tiles.</para>
        /// <para>In-editor this property can be toggled via the tile system context menu
        /// within the "RTS: Scene" window or alternative via the context menu of the
        /// "Tile System" component in the inspector window.</para>
        /// </remarks>
        public bool Locked {
            get { return this.isLocked; }
            set { this.isLocked = value; }
        }


        /// <summary>
        /// The size of a tile.
        /// </summary>
        /// <exclude/>
        [SerializeField, FormerlySerializedAs("tileSize"), FormerlySerializedAs("_tileSize"), FormerlySerializedAs("_cellSize")]
        private Vector3 cellSize = Vector3.one;

        /// <summary>
        /// Gets or sets size of cell within system.
        /// </summary>
        public Vector3 CellSize {
            get { return this.cellSize; }
            set {
                this.cellSize = value;
                this.InvalidatePrecalculatedValues = true;
            }
        }


        #region Tiles Facing and Rotation

        [SerializeField, HideInInspector, FormerlySerializedAs("_tilesFacing")]
        private TileFacing tilesFacing = TileFacing.Sideways;


        /// <summary>
        /// Gets or sets the direction in which painted tiles will face.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///    <item><see cref="TileFacing.Sideways"/> - Good for side scrollers.</item>
        ///    <item><see cref="TileFacing.Upwards"/> - Good for top-down.</item>
        /// </list>
        /// </remarks>
        public TileFacing TilesFacing {
            get { return this.tilesFacing; }
            set { this.tilesFacing = value; }
        }

        #endregion


        /// <summary>
        /// The total number of rows in tile system.
        /// </summary>
        /// <remarks>
        /// <para>The value of this field must not be manually changed.</para>
        /// </remarks>
        /// <exclude/>
        [SerializeField, FormerlySerializedAs("rows"), FormerlySerializedAs("_rows")]
        private int rowCount = 100;
        /// <summary>
        /// The total number of columns in tile system.
        /// </summary>
        /// <remarks>
        /// <para>The value of this field must not be manually changed.</para>
        /// </remarks>
        /// <exclude/>
        [SerializeField, FormerlySerializedAs("columns"), FormerlySerializedAs("_columns")]
        private int columnCount = 100;


        /// <summary>
        /// Gets count of rows in tile system.
        /// </summary>
        public int RowCount {
            get { return this.rowCount; }
        }
        /// <summary>
        /// Gets count of columns in tile system.
        /// </summary>
        public int ColumnCount {
            get { return this.columnCount; }
        }


        /// <summary>
        /// Gets a value indicating whether this <see cref="TileSystem"/> is editable.
        /// </summary>
        /// <remarks>
        /// <para>A tile system becomes uneditable once essential functionality has been
        /// stripped.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if is editable; otherwise <c>false</c>.
        /// </value>
        public bool IsEditable {
            get { return this.isEditable; }
        }

        /// <summary>
        /// Gets mathematical plane representation of tile system.
        /// </summary>
        public Plane Plane {
            get {
                var t = this.transform;
                return new Plane(t.forward, t.position);
            }
        }


        #region Stripping Options

        [SerializeField, FormerlySerializedAs("_strippingPreset")]
        private StrippingPreset strippingPreset = StrippingPreset.StripRuntime;

        /// <summary>
        /// Bitmask of stripping options to apply when runtime stripping is applied, or
        /// when tile system is built.
        /// </summary>
        [SerializeField, FormerlySerializedAs("_strippingOptions")]
        private int strippingOptionMask = 0;


        /// <summary>
        /// Indicates if reduced level of stripping should be applied at runtime.
        /// </summary>
        /// <remarks>
        /// <para>This property is automatically disabled for built tile systems.</para>
        /// </remarks>
        public bool applyRuntimeStripping = false;

        /// <summary>
        /// Indicates whether procedural meshes should be updated when tile system becomes
        /// active.
        /// </summary>
        /// <remarks>
        /// <para>When set to <c>false</c>, procedural meshes can be updated manually
        /// either altogether using <see cref="UpdateProceduralTiles"/> or on a per
        /// chunk basis using <see cref="ProceduralMesh.InitialUpdateMesh"/> which can be
        /// accessed via <see cref="Chunk"/>.</para>
        /// </remarks>
        public bool updateProceduralAtStart = true;


        /// <summary>
        /// Gets or sets stripping preset.
        /// </summary>
        /// <remarks>
        /// <para>By default runtime stripping is specified.</para>
        /// </remarks>
        public StrippingPreset StrippingPreset {
            get { return this.strippingPreset; }
            set {
                if (value == StrippingPreset.Custom) {
                    this.strippingOptionMask = this.StrippingOptions;
                }
                this.strippingPreset = value;
            }
        }

        private void WarnCustomStrippingRequired()
        {
            Debug.LogError("Cannot change stripping property without first setting `strippingPreset` to `StrippingPreset.Custom`.", this);
        }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Rotorz.Tile.TileSystem"/>
        /// component should be stripped at runtime or when performing build.
        /// </summary>
        /// <remarks>
        /// <para>The system component provides access to tile data (when available) but
        /// is also able to transform a world coordinate into a tile index.</para>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripSystemComponent {
            get { return (this.StrippingOptions & StripFlag.STRIP_TILE_SYSTEM) != 0; }
            set {
                // Note: Changes to this functionality must also be reflected in
                //       `StrippingUtility.PreFilterStrippingOptions`.
                //
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_TILE_SYSTEM | StripFlag.STRIP_CHUNK_MAP | StripFlag.STRIP_TILE_DATA | StripFlag.STRIP_BRUSH_REFS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_TILE_SYSTEM);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether chunk map should be stripped at
        /// runtime or when performing build.
        /// </summary>
        /// <remarks>
        /// <para>The chunk map makes it possible to locate the chunk game object for the
        /// specified tile index.</para>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripChunkMap {
            get { return (this.StrippingOptions & StripFlag.STRIP_CHUNK_MAP) != 0; }
            set {
                // Note: Changes to this functionality must also be reflected in
                //       `StrippingUtility.PreFilterStrippingOptions`.
                //
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_CHUNK_MAP | StripFlag.STRIP_TILE_DATA | StripFlag.STRIP_BRUSH_REFS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_CHUNK_MAP);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tile data should be stripped at
        /// runtime or when performing build.
        /// </summary>
        /// <remarks>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripTileData {
            get { return (this.StrippingOptions & StripFlag.STRIP_TILE_DATA) != 0; }
            set {
                // Note: Changes to this functionality must also be reflected in
                //       `StrippingUtility.PreFilterStrippingOptions`.
                //
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_TILE_DATA | StripFlag.STRIP_BRUSH_REFS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_TILE_DATA);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether brush references should be stripped at
        /// runtime or when performing build.
        /// </summary>
        /// <remarks>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripBrushReferences {
            get { return (this.StrippingOptions & StripFlag.STRIP_BRUSH_REFS) != 0; }
            set {
                // Note: Changes to this functionality must also be reflected in
                //       `StrippingUtility.PreFilterStrippingOptions`.
                //
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_BRUSH_REFS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_BRUSH_REFS);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether empty game objects should be stripped
        /// when performing build.
        /// </summary>
        /// <remarks>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripEmptyObjects {
            get { return (this.StrippingOptions & StripFlag.STRIP_EMPTY_OBJECTS) != 0; }
            set {
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_EMPTY_OBJECTS | StripFlag.STRIP_COMBINED_EMPTY)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_EMPTY_OBJECTS);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether empty game objects leftover after mesh
        /// combine should be stripped.
        /// </summary>
        /// <remarks>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripCombinedEmptyObjects {
            get { return (this.StrippingOptions & StripFlag.STRIP_COMBINED_EMPTY) != 0; }
            set {
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_COMBINED_EMPTY)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_COMBINED_EMPTY);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether chunk game objects should be stripped
        /// when performing build.
        /// </summary>
        /// <remarks>
        /// <para>Nested game objects will be reparented to tile system.</para>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripChunks {
            get { return (this.StrippingOptions & StripFlag.STRIP_CHUNKS) != 0; }
            set {
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_CHUNKS | StripFlag.STRIP_EMPTY_CHUNKS | StripFlag.STRIP_CHUNK_MAP | StripFlag.STRIP_TILE_DATA | StripFlag.STRIP_BRUSH_REFS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_CHUNKS);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether empty chunk game objects should be
        /// stripped when performing build.
        /// </summary>
        /// <remarks>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripEmptyChunks {
            get { return (this.StrippingOptions & StripFlag.STRIP_EMPTY_CHUNKS) != 0; }
            set {
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_EMPTY_CHUNKS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_EMPTY_CHUNKS);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether plop instance and group components
        /// should be stripped from plopped tiles.
        /// </summary>
        /// <remarks>
        /// <para><see cref="StrippingPreset"/> must be set to <see cref="Rotorz.Tile.StrippingPreset.Custom"/>
        /// before changing property.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> to strip; otherwise <c>false</c>.
        /// </value>
        public bool StripPlopComponents {
            get { return (this.StrippingOptions & StripFlag.STRIP_PLOP_COMPONENTS) != 0; }
            set {
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = value
                        ? (this.strippingOptionMask | StripFlag.STRIP_PLOP_COMPONENTS)
                        : (this.strippingOptionMask & ~StripFlag.STRIP_PLOP_COMPONENTS);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        /// <summary>
        /// Gets or sets stripping options.
        /// </summary>
        /// <remarks>
        /// <para>This value should only be modified by an editor script because it will
        /// have either very little (or absolutely no) effect at runtime.</para>
        /// </remarks>
        public int StrippingOptions {
            get {
                return (this.strippingPreset == StrippingPreset.Custom)
                    ? this.strippingOptionMask
                    : StripFlagUtility.GetPresetOptions(this.strippingPreset);
            }
            set {
                if (this.strippingPreset == StrippingPreset.Custom) {
                    this.strippingOptionMask = StripFlagUtility.PreFilterStrippingOptions(value);
                }
                else {
                    this.WarnCustomStrippingRequired();
                }
            }
        }

        #endregion


        #region Build Options

        /// <summary>
        /// Method of combining to perform upon build.
        /// </summary>
        /// <remarks>
        /// <para><strong>Warning,</strong> errors may occur if merged regions contain an
        /// excessive number of vertices. At present there is a hard limit of 64k vertices
        /// per <see cref="UnityEngine.Mesh"/>.</para>
        /// </remarks>
        public BuildCombineMethod combineMethod = BuildCombineMethod.ByChunk;
        /// <summary>
        /// The width of a tile chunk to combine (in tiles).
        /// </summary>
        /// <remarks>
        /// <para>Only applicable when <see cref="combineMethod"/> is set to <see cref="BuildCombineMethod.CustomChunkInTiles"/>.</para>
        /// </remarks>
        public int combineChunkWidth = 30;
        /// <summary>
        /// The height of a tile chunk to combine (in tiles).
        /// </summary>
        /// <remarks>
        /// <para>Only applicable when <see cref="combineMethod"/> is set to <see cref="BuildCombineMethod.CustomChunkInTiles"/>.</para>
        /// </remarks>
        public int combineChunkHeight = 30;
        /// <summary>
        /// Indicates whether tile meshes should be combined into submeshes when they
        /// contain multiple materials. Disable to create individual mesh objects on a
        /// per-material basis.
        /// </summary>
        /// <remarks>
        /// <para>Often disabling this option will add a little mileage before the 64k
        /// vertex limit is encountered.</para>
        /// </remarks>
        public bool combineIntoSubmeshes = true;

        /// <summary>
        /// Indicates if vertex snapping should be applied during build process.
        /// </summary>
        /// <remarks>
        /// <para>Vertex snapping is always applied for tiles that are painted using
        /// "smooth" brushes. However snapping can be enabled/disabled for tiles that are
        /// painted using non smooth "static" brushes.</para>
        /// </remarks>
        public bool staticVertexSnapping = false;
        /// <summary>
        /// Vertices are automatically snapped and smoothed within given threshold.
        /// Default is usually recommended but can be disabled with zero.
        /// </summary>
        public float vertexSnapThreshold = 0.001f;

        [SerializeField]
        private bool generateSecondUVs = false;

        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_hardAngle")]
        private float generateSecondUVsHardAngle = 88;
        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_packMargin")]
        private float generateSecondUVsPackMargin = 0.00390625f;
        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_angleError")]
        private float generateSecondUVsAngleError = 0.08f;
        [SerializeField, HideInInspector, FormerlySerializedAs("_2ndUV_areaError")]
        private float generateSecondUVsAreaError = 0.15f;

        /// <summary>
        /// Indicates if second set of UVs should be automatically generated for static
        /// tiles during build.
        /// </summary>
        public bool GenerateSecondUVs {
            get { return this.generateSecondUVs; }
            set { this.generateSecondUVs = value; }
        }

        public float SecondUVsHardAngle {
            get { return this.generateSecondUVsHardAngle; }
            set { this.generateSecondUVsHardAngle = value; }
        }
        public float SecondUVsPackMargin {
            get { return this.generateSecondUVsPackMargin; }
            set { this.generateSecondUVsPackMargin = value; }
        }
        public float SecondUVsAngleError {
            get { return this.generateSecondUVsAngleError; }
            set { this.generateSecondUVsAngleError = value; }
        }
        public float SecondUVsAreaError {
            get { return this.generateSecondUVsAreaError; }
            set { this.generateSecondUVsAreaError = value; }
        }

        /// <summary>
        /// Indicates if tiles that are procedurally generated should be pre-generated
        /// when building tile system.
        /// </summary>
        /// <remarks>
        /// <para>Whilst not required, this remove runtime dependencies which means that
        /// stripping can be applied if needed.</para>
        /// </remarks>
        public bool pregenerateProcedural = false;

        [SerializeField, FormerlySerializedAs("_reduceColliders")]
        private ReduceColliderOptions reduceColliders;

        /// <summary>
        /// Often box colliders of tiles painted using static brushes can be reduced.
        /// This field indicates whether such colliders should be reduced when building
        /// tile system.
        /// </summary>
        /// <seealso cref="ReduceColliderOptions.Active">ReduceColliderOptions.Active</seealso>
        public ReduceColliderOptions ReduceColliders {
            get {
                if (this.reduceColliders == null) {
                    this.reduceColliders = new ReduceColliderOptions();
                }
                return this.reduceColliders;
            }
        }

        #endregion

        #region Messages and Events

        private void Awake()
        {
            // Cannot perform any sort of stripping or updating when chunks have not been
            // initialized! Just bail :-)
            if (this.chunks == null) {
                return;
            }

            // Update procedural tile meshes at start if tile system has not yet been
            // built or if procedural tiles have not been pre-generated.
            if (this.updateProceduralAtStart && (!this.IsBuilt || !this.pregenerateProcedural)) {
                foreach (var chunk in this.chunks) {
                    if (chunk != null && chunk.ProceduralMesh != null) {
                        chunk.ProceduralMesh.InitialUpdateMesh();
                    }
                }
            }

            if (this.applyRuntimeStripping) {
                StrippingUtility.ApplyRuntimeStripping(this);
            }
        }

        #endregion

        #region Editor Data

        /// <summary>
        /// Indicates if force refresh is recommended for tile system.
        /// </summary>
        /// <remarks>
        /// <para>Flag is automatically cleared upon next force refresh.</para>
        /// </remarks>
#pragma warning disable 414
        [SerializeField, FormerlySerializedAs("_hintForceRefresh")]
        private bool hintForceRefresh;
#pragma warning restore 414

        [SerializeField, FormerlySerializedAs("_lastBuildPrefabPath")]
        private string lastBuildPrefabPath;
        [SerializeField, FormerlySerializedAs("_lastBuildDataPath")]
        private string lastBuildDataPath;

        public string LastBuildPrefabPath {
            get { return this.lastBuildPrefabPath; }
            set { this.lastBuildPrefabPath = value; }
        }
        public string LastBuildDataPath {
            get { return this.lastBuildDataPath; }
            set { this.lastBuildDataPath = value; }
        }

        /// <summary>
        /// Order of tile system in "RTS: Scene" editor window.
        /// </summary>
        /// <remarks>
        /// <para>This field is primarily for internal editor use, however can be modified
        /// to sort tile systems in a custom order.</para>
        /// </remarks>
        [SerializeField]
        public int sceneOrder;

        #endregion

        #region Tiles and Chunks

        [SerializeField, FormerlySerializedAs("_chunkColumns")]
        internal int chunkColumns;
        [SerializeField, FormerlySerializedAs("_chunkRows")]
        internal int chunkRows;
        [SerializeField, FormerlySerializedAs("_chunkWidth")]
        internal int chunkWidth;
        [SerializeField, FormerlySerializedAs("_chunkHeight")]
        internal int chunkHeight;
        [SerializeField, FormerlySerializedAs("_chunks")]
        internal Chunk[] chunks;

        /// <summary>
        /// Hints if empty chunks should be automatically removed.
        /// </summary>
        /// <remarks>
        /// <para>Redundant meshes leftover from procedural brushes will also be removed
        /// when chunk is automatically removed. When actively adding and removing tiles
        /// it is generally better to set this field to <c>false</c>.</para>
        /// <para>Note: The value of this field may not always be enforced.</para>
        /// </remarks>
        public bool hintEraseEmptyChunks = true;

        /// <summary>
        /// Gets the number of columns of chunks.
        /// </summary>
        public int ChunkColumns {
            get { return this.chunkColumns; }
        }
        /// <summary>
        /// Gets the number of rows of chunks.
        /// </summary>
        public int ChunkRows {
            get { return this.chunkRows; }
        }
        /// <summary>
        /// Gets the width of a chunk (number of tiles).
        /// </summary>
        public int ChunkWidth {
            get { return this.chunkWidth; }
        }
        /// <summary>
        /// Gets the height of a chunk (number of tiles).
        /// </summary>
        public int ChunkHeight {
            get { return this.chunkHeight; }
        }
        /// <summary>
        /// Gets list of available chunk components.
        /// </summary>
        /// <remarks>
        /// <para>Missing chunks are marked as <c>null</c>.</para>
        /// </remarks>
        public Chunk[] Chunks {
            get { return this.chunks; }
        }

        /// <summary>
        /// Gets chunk component at row and column.
        /// </summary>
        /// <param name="row">Zero-based index of chunk row.</param>
        /// <param name="column">Zero-based index of chunk column.</param>
        /// <returns>
        /// The chunk.
        /// </returns>
        public Chunk GetChunk(int row, int column)
        {
            int chunkIndex = row * this.chunkColumns + column;
            return chunkIndex < 0 || chunkIndex >= this.chunks.Length
                ? null
                : this.chunks[chunkIndex];
        }

        /// <summary>
        /// Calculate chunk index from tile index.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// The zero-based chunk index.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public int ChunkIndexFromTileIndex(int row, int column)
        {
            if ((uint)row >= this.RowCount) {
                throw new ArgumentOutOfRangeException("row");
            }
            if ((uint)column >= this.ColumnCount) {
                throw new ArgumentOutOfRangeException("column");
            }

            // Implementation inlined elsewhere:
            //  - GetTile
            //  - GetChunkFromTileIndex

            //int chunkRow = row / _chunkHeight;
            //int chunkColumn = column / _chunkWidth;
            //int chunkIndex = chunkRow * _chunkColumns + chunkColumn;

            return (row / this.chunkHeight) * this.chunkColumns + (column / this.chunkWidth);
        }

        /// <summary>
        /// Calculate chunk index from tile index.
        /// </summary>
        /// <param name="index">Index of tile.</param>
        /// <returns>
        /// The zero-based chunk index.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public int ChunkIndexFromTileIndex(TileIndex index)
        {
            return this.ChunkIndexFromTileIndex(index.row, index.column);
        }

        /// <summary>
        /// Gets chunk component from a tile index.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// The chunk component when available; otherwise a value of <c>null</c>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public Chunk GetChunkFromTileIndex(int row, int column)
        {
            if ((uint)row >= this.RowCount) {
                throw new ArgumentOutOfRangeException("row");
            }
            if ((uint)column >= this.ColumnCount) {
                throw new ArgumentOutOfRangeException("column");
            }

            //int chunkIndex = ChunkIndexFromTileIndex(row, column);
            int chunkIndex = (row / this.chunkHeight) * this.chunkColumns + (column / this.chunkWidth);        // (INLINED)
            return this.chunks[chunkIndex];
        }

        /// <summary>
        /// Gets chunk component from a tile index.
        /// </summary>
        /// <param name="index">Zero based index of tile.</param>
        /// <returns>
        /// The chunk component when available; otherwise a value of <c>null</c>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public Chunk GetChunkFromTileIndex(TileIndex index)
        {
            return this.GetChunkFromTileIndex(index.row, index.column);
        }

        /// <summary>
        /// Gets index of tile at row and column in relevant chunk.
        /// </summary>
        /// <param name="row">Zero-based index of tile row (in tile system).</param>
        /// <param name="column">Zero-based index of tile column (in tile system).</param>
        /// <returns>
        /// Zero-based index of tile in relevant chunk.
        /// </returns>
        public int IndexOfTileInChunk(int row, int column)
        {
            // Implementation inlined elsewhere:
            //  - GetTile

            return (row % this.chunkHeight) * this.chunkWidth + (column % this.chunkWidth);
        }

        /// <summary>
        /// Gets index of tile at row and column in relevant chunk.
        /// </summary>
        /// <param name="index">Zero-based index of tile (in tile system).</param>
        /// <returns>
        /// Zero-based index of tile in relevant chunk.
        /// </returns>
        public int IndexOfTileInChunk(TileIndex index)
        {
            return (index.row % this.chunkHeight) * this.chunkWidth + (index.column % this.chunkWidth);
        }

        /// <summary>
        /// Calculate position of tile in coordinates local to tile system.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <param name="center">Specify <c>true</c> for position at center of tile;
        /// otherwise <c>false</c> for position at top-left of tile.</param>
        /// <returns>
        /// Position of tile local to tile system.
        /// </returns>
        public Vector3 LocalPositionFromTileIndex(int row, int column, bool center = true)
        {
            Vector3 position = new Vector3(column * this.CellSize.x, row * -this.CellSize.y, 0f);
            if (center) {
                position.x += this.CellSize.x * 0.5f;
                position.y -= this.CellSize.y * 0.5f;
            }
            return position;
        }

        /// <summary>
        /// Calculate position of tile in coordinates local to tile system.
        /// </summary>
        /// <param name="index">Zero-based index of tile.</param>
        /// <param name="center">Specify <c>true</c> for position at center of tile;
        /// otherwise <c>false</c> for position at top-left of tile.</param>
        /// <returns>
        /// Position of tile local to tile system.
        /// </returns>
        public Vector3 LocalPositionFromTileIndex(TileIndex index, bool center = true)
        {
            Vector3 position = new Vector3(index.column * this.CellSize.x, index.row * -this.CellSize.y, 0f);
            if (center) {
                position.x += this.CellSize.x * 0.5f;
                position.y -= this.CellSize.y * 0.5f;
            }
            return position;
        }

        /// <summary>
        /// Calculate position of tile in world coordinates.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <param name="center">Specify <c>true</c> for position at center of tile;
        /// otherwise <c>false</c> for position at top-left of tile.</param>
        /// <returns>
        /// Position of tile in world coordinates.
        /// </returns>
        public Vector3 WorldPositionFromTileIndex(int row, int column, bool center = true)
        {
            Vector3 tilePosition = this.LocalPositionFromTileIndex(row, column, center);
            return this.transform.localToWorldMatrix.MultiplyPoint3x4(tilePosition);
        }

        /// <summary>
        /// Calculate position of tile in world coordinates.
        /// </summary>
        /// <param name="index">Zero-based index of tile.</param>
        /// <param name="center">Specify <c>true</c> for position at center of tile;
        /// otherwise <c>false</c> for position at top-left of tile.</param>
        /// <returns>
        /// Position of tile in world coordinates.
        /// </returns>
        public Vector3 WorldPositionFromTileIndex(TileIndex index, bool center = true)
        {
            Vector3 tilePosition = this.LocalPositionFromTileIndex(index, center);
            return this.transform.localToWorldMatrix.MultiplyPoint3x4(tilePosition);
        }

        /// <summary>
        /// Determines whether the specified tile index is within bounds of the tile system.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// A value of <c>true</c> if tile is within bounds; otherwise a value of <c>false</c>.
        /// </returns>
        public bool InBounds(int row, int column)
        {
            // Optimized in favour of passing thus we use & instead of &&.
            return (uint)row < this.RowCount & (uint)column < this.ColumnCount;
        }

        /// <summary>
        /// Determines whether the specified tile index is within bounds of the tile system.
        /// </summary>
        /// <param name="index">Index of tile.</param>
        /// <returns>
        /// A value of <c>true</c> if tile is within bounds; otherwise a value of <c>false</c>.
        /// </returns>
        public bool InBounds(TileIndex index)
        {
            // Optimized in favour of passing thus we use & instead of &&.
            return (uint)index.row < this.RowCount & (uint)index.column < this.ColumnCount;
        }

        /// <summary>
        /// Gets data of tile at row and column.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// Tile data or a value of <c>null</c> if no tile is present.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        /// <seealso cref="GetTileOrNull(int, int)"/>
        /// <seealso cref="InBounds(int, int)"/>
        public TileData GetTile(int row, int column)
        {
            if ((uint)column >= this.ColumnCount) {
                throw new ArgumentOutOfRangeException("column");
            }
            if ((uint)row >= this.RowCount) {
                throw new ArgumentOutOfRangeException("row");
            }

            // int chunkIndex = ChunkIndexFromTileIndex(row, column);                              (INLINED)
            int chunkIndex = (row / this.chunkHeight) * this.chunkColumns + (column / this.chunkWidth);

            var chunk = this.chunks[chunkIndex];
            if (chunk == null) {
                return null;
            }

            //int index = IndexOfTileInChunk(row, column);                                         (INLINED)
            int index = (row % this.chunkHeight) * this.chunkWidth + (column % this.chunkWidth);

            var tile = chunk.tiles[index];
            return (tile == null || tile.Empty)
                ? null
                : tile;
        }

        /// <summary>
        /// Gets data of tile at row and column.
        /// </summary>
        /// <param name="index">Index of tile.</param>
        /// <returns>
        /// Tile data or a value of <c>null</c> if no tile is present.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        /// <seealso cref="GetTileOrNull(TileIndex)"/>
        /// <seealso cref="InBounds(TileIndex)"/>
        public TileData GetTile(TileIndex index)
        {
            return this.GetTile(index.row, index.column);
        }

        /// <summary>
        /// Gets data of tile at row and column but doesn't throw any exceptions.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// Tile data or a value of <c>null</c> if no tile is present or if specified index
        /// is outside the bounds of the tile system.
        /// </returns>
        /// <seealso cref="GetTile(int, int)"/>
        /// <seealso cref="InBounds(int, int)"/>
        public TileData GetTileOrNull(int row, int column)
        {
            if ((uint)column >= this.ColumnCount | (uint)row >= this.RowCount) {
                return null;
            }

            // int chunkIndex = ChunkIndexFromTileIndex(row, column);                              (INLINED)
            int chunkIndex = (row / this.chunkHeight) * this.chunkColumns + (column / this.chunkWidth);
            if ((uint)chunkIndex >= this.chunks.Length) {
                return null;
            }

            var chunk = this.chunks[chunkIndex];
            if (chunk == null) {
                return null;
            }

            //int index = IndexOfTileInChunk(row, column);                                         (INLINED)
            int index = (row % this.chunkHeight) * this.chunkWidth + (column % this.chunkWidth);

            var tile = chunk.tiles[index];
            return (tile == null || tile.Empty)
                ? null
                : tile;
        }

        /// <inheritdoc cref="GetTileOrNull(int, int)"/>
        /// <param name="index">Index of tile.</param>
        /// <seealso cref="GetTile(TileIndex)"/>
        /// <seealso cref="InBounds(TileIndex)"/>
        public TileData GetTileOrNull(TileIndex index)
        {
            return this.GetTileOrNull(index.row, index.column);
        }

        /// <summary>
        /// Prepare chunk for specific tile index.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// The chunk component.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        private Chunk PrepareChunkForTile(int row, int column)
        {
            int chunkIndex = this.ChunkIndexFromTileIndex(row, column);

            var chunk = this.chunks[chunkIndex];
            if (chunk != null) {
                return chunk;
            }

            // Okay, let's prepare chunk for first time!

            int chunkRow = chunkIndex / this.chunkColumns;
            int chunkColumn = chunkIndex % this.chunkColumns;

            // Create game object for chunk and attach to tile system.
            var chunkGO = new GameObject("chunk_" + chunkRow + "_" + chunkColumn);
            chunkGO.transform.SetParent(this.transform, false);

            // Add chunk component.
            chunk = chunkGO.AddComponent<Chunk>();
            this.chunks[chunkIndex] = chunk;
            chunk.tiles = new TileData[this.chunkWidth * this.chunkHeight];

            chunk.First = new TileIndex(row - row % this.chunkHeight, column - column % this.chunkWidth);

            PaintingUtility.RaiseChunkCreatedEvent(this, chunk, new TileIndex(row, column));

            return chunk;
        }

        /// <summary>
        /// Set contents of tile at specified row and column. Associated game object is
        /// erased if tile already exists.
        /// </summary>
        /// <intro>
        /// <para>Existing <see cref="TileData"/> instance is replaced by the one specified.
        /// Where possible please consider using <see cref="SetTileFrom(int, int, TileData)"/> instead.</para>
        /// </intro>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <param name="tile">The tile data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public void SetTile(int row, int column, TileData tile)
        {
            // Prepare chunk to place tile within.
            var chunk = this.PrepareChunkForTile(row, column);

            int chunkTileIndex = this.IndexOfTileInChunk(row, column);

            // Erase existing tile.
            if (chunk.tiles[chunkTileIndex] != null) {
                this.EraseTileHelper(chunk, chunkTileIndex, false);
            }

            // Store tile data.
            chunk.tiles[chunkTileIndex] = tile;
        }

        /// <summary>
        /// Set contents of tile at specified row and column. Associated game object is
        /// erased if tile already exists.
        /// </summary>
        /// <inheritdoc cref="SetTile(int, int, TileData)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public void SetTile(TileIndex index, TileData tile)
        {
            this.SetTile(index.row, index.column, tile);
        }

        /// <summary>
        /// Set contents of tile at specified row and column from existing tile data.
        /// Associated game object is erased if tile already exists.
        /// </summary>
        /// <remarks>
        /// <para>Existing <see cref="TileData"/> instance is recycled when available,
        /// otherwise a new instance is created.</para>
        /// </remarks>
        /// <example>
        /// <para>Where possible the specified <see cref="TileData"/> should be
        /// reused to reduce memory allocations:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class TileUpdaterTest
        /// {
        ///     // Temporary variable used when updating tile data.
        ///     private static TileData s_Temp = new TileData();
        ///
        ///
        ///     public void ReplaceTileData(TileSystem system, int row, int column, Brush brush)
        ///     {
        ///         // Recycle temporary tile data object
        ///         s_Temp.Clear();
        ///         s_Temp.Empty = false;
        ///         s_Temp.brush = brush;
        ///
        ///         // Set contents of tile from temporary tile data.
        ///         system.SetTileFrom(row, column, s_Temp);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <param name="other">The tile data.</param>
        /// <returns>
        /// Returns the <see cref="TileData"/> instance that was updated.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public TileData SetTileFrom(int row, int column, TileData other)
        {
            // Prepare chunk to place tile within.
            var chunk = this.PrepareChunkForTile(row, column);
            int chunkTileIndex = this.IndexOfTileInChunk(row, column);
            var tile = chunk.tiles[chunkTileIndex];

            // Erase existing tile.
            if (tile != null) {
                this.EraseTileHelper(chunk, chunkTileIndex, false);
            }
            else {
                chunk.tiles[chunkTileIndex] = (tile = new TileData());
            }

            tile.SetFrom(other);
            return tile;
        }

        /// <summary>
        /// Set contents of tile at specified row and column from existing tile data.
        /// Associated game object is erased if tile already exists.
        /// </summary>
        /// <inheritdoc cref="SetTileFrom(int, int, TileData)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public void SetTileFrom(TileIndex index, TileData other)
        {
            this.SetTileFrom(index.row, index.column, other);
        }

        /// <summary>
        /// Counts the number of non-empty tiles.
        /// </summary>
        /// <remarks>
        /// <para>Avoid recomputing this excessively.</para>
        /// </remarks>
        /// <returns>
        /// The number non-empty tiles.
        /// </returns>
        public int CountNonEmptyTiles()
        {
            int count = 0;
            foreach (var chunk in this.chunks) {
                if (chunk != null) {
                    count += chunk.CountNonEmptyTiles();
                }
            }
            return count;
        }

        #endregion


        #region Tile Transforms

        static TileSystem()
        {
            PrecalculateSimpleRotationTransforms();
        }


        private static readonly Quaternion[] s_precalc_SimpleRotationQuaternions = new Quaternion[4 * 2];
        private static readonly Matrix4x4[] s_precalc_SimpleRotationMatrices = new Matrix4x4[4 * 2];

        private static void PrecalculateSimpleRotationTransforms()
        {
            for (int rotation = 0; rotation < 4; ++rotation) {
                // TilesFacing == TileFacing.Sideways
                s_precalc_SimpleRotationQuaternions[rotation] = Quaternion.Euler(0, 0, rotation * -90f);
                s_precalc_SimpleRotationMatrices[rotation] = Matrix4x4.TRS(Vector3.zero, s_precalc_SimpleRotationQuaternions[rotation], Vector3.one);

                // TilesFacing == TileFacing.Upwards
                s_precalc_SimpleRotationQuaternions[4 + rotation] = MathUtility.AngleAxis_90_Left * Quaternion.Euler(0, rotation * 90f, 0);
                s_precalc_SimpleRotationMatrices[4 + rotation] = Matrix4x4.TRS(Vector3.zero, s_precalc_SimpleRotationQuaternions[4 + rotation], Vector3.one);
            }
        }

        /// <summary>
        /// Gets pre-calculated quaternion for simple rotation.
        /// </summary>
        /// <param name="facing">Way in which tiles are facing from tile system.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// Resulting simple rotation transform.
        /// </returns>
        private static Quaternion GetSimpleRotationQuaternion(TileFacing facing, int rotation)
        {
            int offset = (facing == TileFacing.Upwards ? 4 : 0);
            return s_precalc_SimpleRotationQuaternions[offset + rotation];
        }

        /// <summary>
        /// Gets pre-calculated matrix for simple rotation.
        /// </summary>
        /// <param name="matrix">Output resulting simple rotation transform.</param>
        /// <param name="facing">Way in which tiles are facing from tile system.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        private static void GetSimpleRotationMatrix(out Matrix4x4 matrix, TileFacing facing, int rotation)
        {
            int offset = (facing == TileFacing.Upwards ? 4 : 0);
            matrix = s_precalc_SimpleRotationMatrices[offset + rotation];
        }

        /// <summary>
        /// Indicates whether pre-calculated values should be invalidated so that they
        /// are recalculated when they are next required.
        /// </summary>
        private bool InvalidatePrecalculatedValues = true;

        /// <summary>
        /// Always access via <see cref="PrecalculatedCellSizeScaleVectors"/> instead.
        /// </summary>
        private readonly Vector3[] __precalc__CellSizeScaleVectors = new Vector3[4];

        /// <summary>
        /// Update pre-calculated matrix and quaternion values to reduce the amount of
        /// matrix and quaternion operations when painting tiles.
        /// </summary>
        /// <remarks>
        /// <para>Force pre-calculated values to be recalculated by assigning a value
        /// of <c>true</c> to the field <see cref="InvalidatePrecalculatedValues"/>.</para>
        /// </remarks>
        private void UpdatedPrecalculatedValues()
        {
            for (int rotation = 0; rotation < 4; ++rotation) {
                // Matrix to scale tiles by "Cell Size".
                Matrix4x4 scaleMatrix;
                GetSimpleRotationMatrix(out scaleMatrix, this.TilesFacing, rotation);
                MathUtility.MultiplyScaleByMatrix(ref scaleMatrix, this.CellSize);
                this.__precalc__CellSizeScaleVectors[rotation] = MathUtility.ExtractScaleFromMatrix(ref scaleMatrix);
            }

            this.InvalidatePrecalculatedValues = false;
        }

        /// <summary>
        /// Gets array of pre-calculated vectors for applying "Cell Size" as scale whilst taking
        /// simple rotations into consideration for non-uniform cell sizes.
        /// </summary>
        private Vector3[] PrecalculatedCellSizeScaleVectors {
            get {
                if (this.InvalidatePrecalculatedValues) {
                    this.UpdatedPrecalculatedValues();
                }
                return this.__precalc__CellSizeScaleVectors;
            }
        }

        /// <summary>
        /// Calculate rotational offset to tile and orientate to face away from tile system
        /// according to a specified value of <see cref="TileFacing"/>.
        /// </summary>
        /// <param name="facing">Direction in which tile will face.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// Quaternion which describes the rotation.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If an invalid rotation is specified.
        /// </exception>
        public Quaternion CalculateSimpleRotation(TileFacing facing, int rotation = 0)
        {
            return GetSimpleRotationQuaternion(facing, rotation);
        }

        /// <summary>
        /// Calculate rotational offset to tile and orientate to face away from tile system
        /// according to the value of <see cref="TileSystem.TilesFacing"/>.
        /// </summary>
        /// <inheritdoc cref="CalculateSimpleRotation(TileFacing, int)"/>
        /// <seealso cref="TileSystem.TilesFacing"/>
        public Quaternion CalculateSimpleRotation(int rotation = 0)
        {
            return GetSimpleRotationQuaternion(this.TilesFacing, rotation);
        }

        /// <summary>
        /// Calculate matrix which can be used to rotate a tile to face away from the system
        /// according to a specified value of <see cref="TileFacing"/>.
        /// </summary>
        /// <param name="matrix">The resulting transform matrix.</param>
        /// <param name="row">Zero-based index of row within tile system.</param>
        /// <param name="column">Zero-based index of column within tile system.</param>
        /// <param name="facing">Direction in which tile will face.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <param name="center">A value of <c>true</c> indicates that transform matrix should
        /// arrive at center of cell whereas a value of <c>false</c> indicates that transform
        /// matrix should arrive at upper-left corner of cell.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If an invalid rotation is specified.
        /// </exception>
        /// <seealso cref="CalculateTileMatrix(out Matrix4x4, int)"/>
        /// <seealso cref="CalculateSimpleRotation(int)"/>
        public void CalculateTileMatrix(out Matrix4x4 matrix, int row, int column, TileFacing facing, int rotation = 0, bool center = true)
        {
            Vector3 tileOffset = this.LocalPositionFromTileIndex(row, column, center);

            GetSimpleRotationMatrix(out matrix, facing, rotation);
            matrix.m03 = tileOffset.x;
            matrix.m13 = tileOffset.y;
            matrix.m23 = tileOffset.z;
        }

        /// <summary>
        /// Calculate matrix which can be used to rotate a tile to face away from the system
        /// according to a specified value of <see cref="TileFacing"/>.
        /// </summary>
        /// <inheritdoc cref="CalculateTileMatrix(out Matrix4x4, int, int, TileFacing, int, bool)"/>
        public void CalculateTileMatrix(out Matrix4x4 matrix, TileFacing facing, int rotation = 0)
        {
            this.CalculateTileMatrix(out matrix, 0, 0, facing, rotation, false);
        }

        /// <summary>
        /// Calculate matrix which can be used to position a tile within its cell and rotate
        /// it to face away from the system according to the value of <see cref="TileSystem.TilesFacing"/>.
        /// </summary>
        /// <inheritdoc cref="CalculateTileMatrix(out Matrix4x4, int, int, TileFacing, int, bool)"/>
        public void CalculateTileMatrix(out Matrix4x4 matrix, int row, int column, int rotation = 0, bool center = true)
        {
            this.CalculateTileMatrix(out matrix, row, column, this.TilesFacing, rotation, center);
        }

        /// <summary>
        /// Calculate matrix which can be used to rotate a tile to face away from the system
        /// according to the value of <see cref="TileSystem.TilesFacing"/>.
        /// </summary>
        /// <inheritdoc cref="CalculateTileMatrix(out Matrix4x4, int, int, TileFacing, int, bool)"/>
        public void CalculateTileMatrix(out Matrix4x4 matrix, int rotation = 0)
        {
            this.CalculateTileMatrix(out matrix, 0, 0, this.TilesFacing, rotation, false);
        }

        /// <summary>
        /// Calculate vector which can be used to scale an object by <see cref="CellSize"/>
        /// whilst taking simple rotations into consideration.
        /// </summary>
        /// <remarks>
        /// <para>Simple rotation is factored in when scaling an object by cell size
        /// so that tile fits non-uniformly sized cells</para>
        /// </remarks>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// Resulting scale vector.
        /// </returns>
        public Vector3 CalculateCellSizeScale(int rotation = 0)
        {
            return this.PrecalculatedCellSizeScaleVectors[rotation];
        }

        #endregion


        #region Bulk Editing

        private int bulkEditLevel;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Rotorz.Tile.TileSystem"/> is
        /// in bulk edit mode.
        /// </summary>
        /// <value>
        /// A value of <c>true</c> if bulk edit mode; otherwise <c>false</c>.
        /// </value>
        public bool BulkEditMode {
            get { return this.bulkEditLevel > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether changes made during bulk edit mode are being
        /// finalized. The value of this property is occassionally useful in the context
        /// of painting and refreshing tiles.
        /// </summary>
        public bool IsEndingBulkEditMode { get; internal set; }

        /// <summary>
        /// Begins bulk painting and/or erasing of tiles.
        /// </summary>
        /// <remarks>
        /// <para>When painting large numbers of oriented tiles it is possible that tiles
        /// will be repainted multiple times as their orientation changes. Painting game
        /// objects can be a slow process and thus bulk painting performance can be
        /// improved by deferring tile creation until all changes have been made.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source demonstrates how to utilise bulk edit mode when
        /// painting an oriented tile and refreshing its surrounding tiles. This can
        /// improve performance because painting multiple oriented tiles may cause
        /// surrounding tiles to be updated
        /// multiple times:</para>
        /// <code language="csharp"><![CDATA[
        /// system.BeginBulkEdit();
        ///     brush.Paint(system, 5, 5);
        ///     system.RefreshSurroundingTiles(5, 5);
        ///     brush.Paint(system, 5, 6);
        ///     system.RefreshSurroundingTiles(5, 6);
        /// system.EndBulkEdit();
        /// ]]></code>
        ///
        /// <para>For convenience <see cref="BeginBulkEdit"/> and <see cref="EndBulkEdit"/>
        /// may be nested within prior bulk edit modes. Changes will not take effect until
        /// last call of <see cref="EndBulkEdit"/>:</para>
        /// <code language="csharp"><![CDATA[
        /// private void PaintTwoTiles(TileSystem system, Brush brush, int row, int column)
        /// {
        ///     system.BeginBulkEdit();
        ///         brush.Paint(system, row, column);
        ///         system.RefreshSurroundingTiles(row, column);
        ///         brush.Paint(system, row, column + 1);
        ///         system.RefreshSurroundingTiles(row, column + 1);
        ///     system.EndBulkEdit();
        /// }
        ///
        /// private void PaintExampleTiles(TileSystem system, Brush brush)
        /// {
        ///     system.BeginBulkEdit();
        ///         PaintTwoTiles(system, brush, 5, 5);
        ///         PaintTwoTiles(system, brush, 6, 5);
        ///     system.EndBulkEdit();
        /// }
        /// ]]></code>
        /// </example>
        /// <seealso cref="EndBulkEdit"/>
        public void BeginBulkEdit()
        {
            if (this.bulkEditLevel == 0) {
                this.BeginProceduralEditing();
            }

            ++this.bulkEditLevel;
        }

        /// <summary>
        /// Ends bulk painting and/or erasing of tiles.
        /// </summary>
        /// <remarks>
        /// <para>Note: Some changes will not be committed until last call of <see cref="EndBulkEdit"/>
        /// when calls are nested.</para>
        /// </remarks>
        /// <returns>
        /// The number of tiles that were committed by this call to <see cref="EndBulkEdit"/>.
        /// This function will always return <c>0</c> for nested calls.
        /// </returns>
        /// <seealso cref="BeginBulkEdit"/>
        public int EndBulkEdit()
        {
            if (--this.bulkEditLevel < 0) {
                Debug.LogError("`TileSystem.BeginBulkEdit` has not yet been called. Each call to `TileSystem.EndBulkEdit` must be paired with a call to `RotorzTileSystem.BeginBulkEdit`.", this);

                // Correct error.
                this.bulkEditLevel = 0;
                return 0;
            }

            // Commit changes?
            if (this.bulkEditLevel == 0) {
                try {
                    this.IsEndingBulkEditMode = true;

                    int count = this.ScanBrokenTiles(RepairAction.RefreshDirty, true);
                    this.EndProceduralEditing();
                    return count;
                }
                finally {
                    this.IsEndingBulkEditMode = false;
                }
            }
            else {
                return 0;
            }
        }

        #endregion


        #region Tile Tracing

        /// <summary>
        /// Determines whether specific tile is flagged as being solid.
        /// </summary>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <returns>
        /// A value of <c>true</c> if tile is flagged solid; otherwise <c>false</c>.
        /// </returns>
        public bool IsSolid(int row, int column)
        {
            var tile = this.GetTileOrNull(row, column);
            return tile != null && (tile.flags & TileData.FLAG_SOLID) != 0;
        }

        /// <summary>
        /// Determines whether specific tile is flagged as being solid.
        /// </summary>
        /// <param name="index">Index of tile.</param>
        /// <returns>
        /// A value of <c>true</c> if tile is flagged solid; otherwise <c>false</c>.
        /// </returns>
        public bool IsSolid(TileIndex index)
        {
            return this.IsSolid(index.row, index.column);
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that
        /// is found using custom delegate.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// </remarks>
        /// <example>
        /// <para>Perform custom tile trace to find first tile that has some required
        /// flags and does not have any disallowed flags.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// static class TileTraceExtensions
        /// {
        ///     // Extension method for extended flag tile trace.
        ///
        ///     public static bool TileTraceFlagEx(this TileSystem system,
        ///         int requiredFlags,
        ///         int excludeFlags,
        ///         TileTraceDirection direction,
        ///         int fromRow,
        ///         int fromColumn,
        ///         out TileTraceHit hit
        ///     ) {
        ///         return system.TileTraceCustom(direction, fromRow, fromColumn, out hit,
        ///             delegate(TileData tile) {
        ///                 if (tile == null) {
        ///                     return false;
        ///                 }
        ///
        ///                 int flags = tile.UserFlags;
        ///
        ///                 // Make sure that all required flags are set.
        ///                 if ((flags & requiredFlags) != requiredFlags) {
        ///                     return false;
        ///                 }
        ///
        ///                 // Make sure that no disallowed flags are set!
        ///                 if ((flags & excludeFlags) != 0) {
        ///                     return false;
        ///                 }
        ///
        ///                 return true;
        ///             }
        ///         );
        ///     }
        ///
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="direction">Direction to search.</param>
        /// <param name="row">Zero-based index of origin row.</param>
        /// <param name="column">Zero-based index of origin column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <param name="fn">Custom search function.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTrace(TileTraceDirection direction, int row, int column, out TileTraceHit hit, TileTraceDelegate fn)
        {
            TileData tile;
            int max;

            switch (direction) {
                case TileTraceDirection.West:
                    if (row >= 0 && row < this.RowCount) {
                        while (--column >= 0) {
                            tile = this.GetTileOrNull(row, column);
                            if (fn(tile)) {
                                hit = new TileTraceHit(row, column, tile);
                                return true;
                            }
                        }
                    }
                    hit = TileTraceHit.none;
                    return false;

                default:
                case TileTraceDirection.East:
                    if (row >= 0 && row < this.RowCount) {
                        max = this.ColumnCount;
                        while (++column < max) {
                            tile = this.GetTileOrNull(row, column);
                            if (fn(tile)) {
                                hit = new TileTraceHit(row, column, tile);
                                return true;
                            }
                        }
                    }
                    hit = TileTraceHit.none;
                    return false;

                case TileTraceDirection.North:
                    if (column >= 0 && column < this.ColumnCount) {
                        while (--row >= 0) {
                            tile = this.GetTileOrNull(row, column);
                            if (fn(tile)) {
                                hit = new TileTraceHit(row, column, tile);
                                return true;
                            }
                        }
                    }
                    hit = TileTraceHit.none;
                    return false;

                case TileTraceDirection.South:
                    if (column >= 0 && column < this.ColumnCount) {
                        max = this.RowCount;
                        while (++row < max) {
                            tile = this.GetTileOrNull(row, column);
                            if (fn(tile)) {
                                hit = new TileTraceHit(row, column, tile);
                                return true;
                            }
                        }
                    }
                    hit = TileTraceHit.none;
                    return false;
            }
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that is
        /// found using custom delegate.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// </remarks>
        /// <example>
        /// <para>Perform custom tile trace to find first tile that has some required
        /// flags and does not have any disallowed flags.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// static class TileTraceExtensions
        /// {
        ///     // Extension method for extended flag tile trace.
        ///
        ///     public static bool TileTraceFlagEx(this TileSystem system,
        ///         int requiredFlags,
        ///         int excludeFlags,
        ///         int fromRow,
        ///         int fromColumn,
        ///         int toRow,
        ///         int toColumn,
        ///         out TileTraceHit hit
        ///     ) {
        ///         return system.TileTraceCustom(fromRow, fromColumn, toRow, toColumn, out hit,
        ///             delegate(TileData tile) {
        ///                 if (tile == null) {
        ///                     return false;
        ///                 }
        ///
        ///                 int flags = tile.UserFlags;
        ///
        ///                 // Make sure that all required flags are set.
        ///                 if ((flags & requiredFlags) != requiredFlags) {
        ///                     return false;
        ///                 }
        ///
        ///                 // Make sure that no disallowed flags are set!
        ///                 if ((flags & excludeFlags) != 0) {
        ///                     return false;
        ///                 }
        ///
        ///                 return true;
        ///             }
        ///         );
        ///     }
        ///
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="originRow">Zero-based index of origin row.</param>
        /// <param name="originColumn">Zero-based index of origin column.</param>
        /// <param name="destRow">Zero-based index of destination row.</param>
        /// <param name="destColumn">Zero-based index of destination column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <param name="fn">Custom search function.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTrace(int originRow, int originColumn, int destRow, int destColumn, out TileTraceHit hit, TileTraceDelegate fn)
        {
            TileIndex origin, dest;
            origin.row = originRow;
            origin.column = originColumn;
            dest.row = destRow;
            dest.column = destColumn;

            bool reverse = PaintingUtility.NormalizeLineEndPoints(ref origin, ref dest);
            var line = PaintingUtility.s_TempLineIndices;

            PaintingUtility.GetLineIndices(line, origin, dest);
            if (reverse) {
                line.Reverse();
            }

            // Skip first point in line.
            int end = line.Count;
            for (int i = 1; i < end; ++i) {
                var idx = line[i];

                var tile = this.GetTileOrNull(idx.row, idx.column);
                if (fn(tile)) {
                    hit = new TileTraceHit(idx.row, idx.column, tile);
                    return true;
                }
            }

            hit = TileTraceHit.none;
            return false;
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that is
        /// flagged as solid.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para><img src="../art/solid-flag-trace.jpg" alt="Illustration of tracing solid tiles"/></para>
        /// </remarks>
        /// <example>
        /// <para>Use tile trace to place a glowing object at the next possible position
        /// of player character:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class PlayerBehaviour : MonoBehaviour
        /// {
        ///     // The associated tile system.
        ///     public TileSystem tileSystem;
        ///
        ///     // Row and column that character is nearest.
        ///     public int row;
        ///     public int column;
        ///
        ///     // 4-point direction that player is facing.
        ///     public TileTraceDirection direction;
        ///
        ///     // A glowing orb that indicates where player will next move.
        ///     public GlowingOrb glowingOrb;
        ///
        ///
        ///     // This function gets called when the player has completed their
        ///     // movement or when their direction is changed.
        ///     private void UpdateGlowingOrbPosition()
        ///     {
        ///         TileTraceHit hit;
        ///         if (this.tileSystem.TileTraceSolid(this.direction, this.row, this.column, out hit)) {
        ///             // Place glowing orb one square in front of solid tile.
        ///             switch (this.direction) {
        ///                 case TileTraceDirection.North:
        ///                     this.glowingOrb.SetPosition(hit.row + 1, hit.column);
        ///                     break;
        ///                 case TileTraceDirection.East:
        ///                     this.glowingOrb.SetPosition(hit.row, hit.column - 1);
        ///                     break;
        ///                 case TileTraceDirection.South:
        ///                     this.glowingOrb.SetPosition(hit.row - 1, hit.column);
        ///                     break;
        ///                 case TileTraceDirection.West:
        ///                     this.glowingOrb.SetPosition(hit.row, hit.column + 1);
        ///                     break;
        ///             }
        ///         }
        ///     }
        ///
        ///     // Remainder of player behaviour implementation...
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="direction">Direction to search.</param>
        /// <param name="row">Zero-based index of origin row.</param>
        /// <param name="column">Zero-based index of origin column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceSolid(TileTraceDirection direction, int row, int column, out TileTraceHit hit)
        {
            return this.TileTrace(direction, row, column, out hit,
                (tile) => tile != null && (tile.flags & TileData.FLAG_SOLID) != 0
            );
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that
        /// is flagged as solid.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para><img src="../art/solid-flag-trace.jpg" alt="Illustration of tracing solid tiles"/></para>
        /// </remarks>
        /// <example>
        /// <para>Use tile trace to place a glowing object at the next possible position
        /// of player character:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class PlayerBehaviour : MonoBehaviour
        /// {
        ///     // The associated tile system.
        ///     public TileSystem tileSystem;
        ///     // Row and column that character is nearest.
        ///     public TileIndex tileIndex;
        ///
        ///     // 4-point direction that player is facing.
        ///     public TileTraceDirection direction;
        ///
        ///     // A glowing orb that indicates where player will next move.
        ///     public GlowingOrb glowingOrb;
        ///
        ///
        ///     // This function gets called when the player has completed their
        ///     // movement or when their direction is changed.
        ///     private void UpdateGlowingOrbPosition()
        ///     {
        ///         TileTraceHit hit;
        ///         if (this.tileSystem.TileTraceSolid(this.direction, this.tileIndex, out hit)) {
        ///             // Place glowing orb one square in front of solid tile.
        ///             switch (this.direction) {
        ///                 case TileTraceDirection.North:
        ///                     this.glowingOrb.SetPosition(hit.row + 1, hit.column);
        ///                     break;
        ///                 case TileTraceDirection.East:
        ///                     this.glowingOrb.SetPosition(hit.row, hit.column - 1);
        ///                     break;
        ///                 case TileTraceDirection.South:
        ///                     this.glowingOrb.SetPosition(hit.row - 1, hit.column);
        ///                     break;
        ///                 case TileTraceDirection.West:
        ///                     this.glowingOrb.SetPosition(hit.row, hit.column + 1);
        ///                     break;
        ///             }
        ///         }
        ///     }
        ///
        ///     // Remainder of player behaviour implementation...
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="direction">Direction to search.</param>
        /// <param name="origin">Zero-based index of origin.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceSolid(TileTraceDirection direction, TileIndex origin, out TileTraceHit hit)
        {
            return this.TileTraceSolid(direction, origin.row, origin.column, out hit);
        }

        /// <summary>
        /// Trace through tiles from origin to destination to find first tile that
        /// is flagged as solid.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para><img src="../art/solid-flag-trace2.jpg" alt="Illustration of tracing solid tiles"/></para>
        /// </remarks>
        /// <example>
        /// <para>Use tile trace to test line of sight:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class EnemyBehaviour : MonoBehaviour
        /// {
        ///     // The associated tile system.
        ///     public TileSystem tileSystem;
        ///
        ///     // Row and column that enemy character is nearest.
        ///     public int row;
        ///     public int column;
        ///
        ///
        ///     // Fire towards player if player is within line of sight.
        ///     private void FireAtPlayer(PlayerBehaviour player)
        ///     {
        ///         TileTraceHit hit;
        ///         if (!this.tileSystem.TileTraceSolid(this.row, this.column, player.row, player.column, out hit)) {
        ///             // Trace did not hit a solid tile, fire at will!
        ///             this.FireRoundTowards(player.tileIndex);
        ///         }
        ///     }
        ///
        ///     // Remainder of enemy behaviour implementation...
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="originRow">Zero-based index of origin row.</param>
        /// <param name="originColumn">Zero-based index of origin column.</param>
        /// <param name="destRow">Zero-based index of destination row.</param>
        /// <param name="destColumn">Zero-based index of destination column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceSolid(int originRow, int originColumn, int destRow, int destColumn, out TileTraceHit hit)
        {
            return this.TileTrace(originRow, originColumn, destRow, destColumn, out hit,
                (tile) => tile != null && (tile.flags & TileData.FLAG_SOLID) != 0
            );
        }

        /// <summary>
        /// Trace through tiles from origin to destination to find first tile that
        /// is flagged as solid.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para><img src="../art/solid-flag-trace2.jpg" alt="Illustration of tracing solid tiles"/></para>
        /// </remarks>
        /// <example>
        /// <para>Use tile trace to test line of sight:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class EnemyBehaviour : MonoBehaviour
        /// {
        ///     // The associated tile system.
        ///     public TileSystem tileSystem;
        ///
        ///     // Player that the enemy is tracking..
        ///     public PlayerGridMovement player;
        ///
        ///
        ///     // Fire towards player if player is within line of sight.
        ///     private void FireAtPlayer(PlayerBehaviour player)
        ///     {
        ///         TileTraceHit hit;
        ///         if (!this.tileSystem.TileTraceSolid(tileIndex, player.tileIndex, out hit)) {
        ///             // Trace did not hit a solid tile, fire at will!
        ///             this.FireRoundTowards(this.player.tileIndex);
        ///         }
        ///     }
        ///
        ///     // Remainder of enemy behaviour implementation...
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="origin">Zero-based index of origin.</param>
        /// <param name="dest">Zero-based index of destination.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceSolid(TileIndex origin, TileIndex dest, out TileTraceHit hit)
        {
            return this.TileTraceSolid(origin.row, origin.column, dest.row, dest.column, out hit);
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that has
        /// all required flags.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// </remarks>
        /// <param name="requiredFlags">Bit mask of up to 16 user flags.</param>
        /// <param name="direction">Direction to search.</param>
        /// <param name="row">Zero-based index of origin row.</param>
        /// <param name="column">Zero-based index of origin column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceFlags(int requiredFlags, TileTraceDirection direction, int row, int column, out TileTraceHit hit)
        {
            return this.TileTrace(direction, row, column, out hit,
                (tile) => tile != null && (tile.flags & requiredFlags) == requiredFlags
            );
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that has
        /// all required flags.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// </remarks>
        /// <param name="requiredFlags">Bit mask of up to 16 user flags.</param>
        /// <param name="direction">Direction to search.</param>
        /// <param name="origin">Zero-based index of origin.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceFlags(int requiredFlags, TileTraceDirection direction, TileIndex origin, out TileTraceHit hit)
        {
            return this.TileTraceFlags(requiredFlags, direction, origin.row, origin.column, out hit);
        }

        /// <summary>
        /// Trace through tiles from origin to destination to find first tile that has
        /// all required flags.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// </remarks>
        /// <param name="requiredFlags">Bit mask of up to 16 user flags.</param>
        /// <param name="originRow">Zero-based index of origin row.</param>
        /// <param name="originColumn">Zero-based index of origin column.</param>
        /// <param name="destRow">Zero-based index of destination row.</param>
        /// <param name="destColumn">Zero-based index of destination column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceFlags(int requiredFlags, int originRow, int originColumn, int destRow, int destColumn, out TileTraceHit hit)
        {
            return this.TileTrace(originRow, originColumn, destRow, destColumn, out hit,
                (tile) => tile != null && (tile.flags & requiredFlags) == requiredFlags
            );
        }

        /// <summary>
        /// Trace through tiles from origin to destination to find first tile that has
        /// all required flags.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// </remarks>
        /// <param name="requiredFlags">Bit mask of up to 16 user flags.</param>
        /// <param name="origin">Zero-based index of origin.</param>
        /// <param name="dest">Zero-based index of destination.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceFlags(int requiredFlags, TileIndex origin, TileIndex dest, out TileTraceHit hit)
        {
            return this.TileTraceFlags(requiredFlags, origin.row, origin.column, dest.row, dest.column, out hit);
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that was
        /// painted using a specific brush.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para>This method will always return <c>false</c> if brush references have been
        /// stripped from tile data structure.</para>
        /// </remarks>
        /// <param name="brush">Brush to search for.</param>
        /// <param name="direction">Direction to search.</param>
        /// <param name="row">Zero-based index of origin row.</param>
        /// <param name="column">Zero-based index of origin column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTrace(Brush brush, TileTraceDirection direction, int row, int column, out TileTraceHit hit)
        {
            return this.TileTrace(direction, row, column, out hit,
                (tile) => (tile != null && tile.brush == brush) || (tile == null && brush == null)
            );
        }

        /// <summary>
        /// Trace through tiles horizontally or vertically to find first tile that was
        /// painted using a specific brush.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para>This method will always return <c>false</c> if brush references have been
        /// stripped from tile data structure.</para>
        /// </remarks>
        /// <param name="brush">Brush to search for.</param>
        /// <param name="direction">Direction to search.</param>
        /// <param name="origin">Zero-based index of origin.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTrace(Brush brush, TileTraceDirection direction, TileIndex origin, out TileTraceHit hit)
        {
            return this.TileTrace(brush, direction, origin.row, origin.column, out hit);
        }

        /// <summary>
        /// Trace through tiles from origin to destination to find first tile that was
        /// painted using a specific brush.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para>This method will always return <c>false</c> if brush references have been
        /// stripped from tile data structure.</para>
        /// </remarks>
        /// <param name="brush">Brush to search for.</param>
        /// <param name="originRow">Zero-based index of origin row.</param>
        /// <param name="originColumn">Zero-based index of origin column.</param>
        /// <param name="destRow">Zero-based index of destination row.</param>
        /// <param name="destColumn">Zero-based index of destination column.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceBrush(Brush brush, int originRow, int originColumn, int destRow, int destColumn, out TileTraceHit hit)
        {
            return this.TileTrace(originRow, originColumn, destRow, destColumn, out hit,
                (tile) => (tile != null && tile.brush == brush) || (tile == null && brush == null)
            );
        }

        /// <summary>
        /// Trace through tiles from origin to destination to find first tile that was
        /// painted using a specific brush.
        /// </summary>
        /// <remarks>
        /// <para>Tile tracing functions are generally useful when performing line of
        /// sight type tests. These functions can be applied in wide range of different
        /// ways.</para>
        /// <para>This method will always return <c>false</c> if brush references have been
        /// stripped from tile data structure.</para>
        /// </remarks>
        /// <param name="brush">Brush to search for.</param>
        /// <param name="origin">Zero-based index of origin.</param>
        /// <param name="dest">Zero-based index of destination.</param>
        /// <param name="hit">Details about hit. Check return value before using hit data.</param>
        /// <returns>
        /// A value of <c>true</c> if a tile is flagged as solid was hit; otherwise <c>false</c>.
        /// </returns>
        public bool TileTraceBrush(Brush brush, TileIndex origin, TileIndex dest, out TileTraceHit hit)
        {
            return this.TileTraceBrush(brush, origin.row, origin.column, dest.row, dest.column, out hit);
        }

        #endregion


        #region Methods

        internal void InitializeSystem(float cellWidth, float cellHeight, float cellDepth, int rows, int columns, int chunkWidth, int chunkHeight)
        {
            this.version = ProductInfo.Version;

            this.CellSize = new Vector3(cellWidth, cellHeight, cellDepth);

            this.rowCount = rows;
            this.columnCount = columns;

            this.isEditable = true;

            this.chunkWidth = chunkWidth;
            this.chunkHeight = chunkHeight;
            this.chunkColumns = columns / chunkWidth + (columns % chunkWidth > 0 ? 1 : 0);
            this.chunkRows = rows / chunkHeight + (rows % chunkHeight > 0 ? 1 : 0);

            this.chunks = new Chunk[this.chunkColumns * this.chunkRows];
        }

        /// <summary>
        /// Creates the tile system with specified number of rows and columns.
        /// </summary>
        /// <remarks>
        /// <para>All tiles will be erased if invoked on an existing tile system.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source code demonstrates how to create a tile system at
        /// runtime and then paint a row of tiles using a specified brush.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class DynamicallyCreateTileSystem : MonoBehaviour
        /// {
        ///     // We will use this brush to paint a line of tiles.
        ///     public Brush testBrush;
        ///
        ///
        ///     private void Start()
        ///     {
        ///         // Create game object for tile system.
        ///         var tileSystemGO = new GameObject("Dynamic Tiles");
        ///         // Attach tile system component.
        ///         var tileSystem = tileSystemGO.AddComponent<TileSystem>();
        ///
        ///         // Initialize tile system with 10 rows, 15 columns.
        ///         tileSystem.CreateSystem(1f, 1f, 1f, 10, 15, 30, 30);
        ///
        ///         // Paint a horizontal line of tiles?
        ///         if (this.testBrush != null) {
        ///             for (int i = 0; i < 15; ++i) {
        ///                 this.testBrush.Paint(tileSystem, 5, i);
        ///             }
        ///         }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="cellWidth">Width of each tile cell.</param>
        /// <param name="cellHeight">Height of each tile cell.</param>
        /// <param name="cellDepth">Depth of each tile cell.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="chunkWidth">Number of columns within a chunk.</param>
        /// <param name="chunkHeight">Number of rows within a chunk.</param>
        public void CreateSystem(float cellWidth, float cellHeight, float cellDepth, int rows, int columns, int chunkWidth = 30, int chunkHeight = 30)
        {
            // Erase any existing tiles.
            this.EraseAllTiles();
            // Initialize tile system data.
            this.InitializeSystem(cellWidth, cellHeight, cellDepth, rows, columns, chunkWidth, chunkHeight);
        }

        private bool EraseTileHelper(Chunk chunk, int indexOfTileInChunk, bool eraseEmptyChunks)
        {
            var tile = chunk.tiles[indexOfTileInChunk];
            if (tile == null) {
                return false;
            }

            // If tile is not dirty then it existed before bulk edit mode was initiated.
            if (!tile.Dirty && !tile.Empty) {
                PaintingUtility.RaiseWillEraseTileEvent(this, chunk.TileIndexFromIndexOfTileInChunk(indexOfTileInChunk), tile);
            }

            // Destroy game object?
            if (tile.gameObject != null) {
                Brush.DestroyTileGameObject(tile, this);
            }

            chunk.ProceduralDirty |= tile.Procedural;

            // Remove tile data from chunk?
            //
            // Note: Flag as dirty during bulk edit mode so that surrounding
            //       tiles can be updated properly.
            //
            if (this.BulkEditMode) {
                tile.Dirty = true;
                tile.brush = null;
                chunk.Dirty = true;
            }
            else {
                tile.Clear();

                //!TODO: Can the following be optimised for bulk edits?

                // Has chunk become useless?
                if (eraseEmptyChunks && InternalUtility.ShouldEraseEmptyChunks(this) && chunk.IsEmpty) {
                    var chunkTransform = chunk.transform;
                    int chunkChildCount = chunkTransform.childCount;

                    // If chunk is empty (or just contains redundant procedural mesh)...
                    if (chunkChildCount == 0 || (chunk.ProceduralMesh != null && chunkChildCount == 1)) {
                        // Only destroy chunk if it has no components (other than `Transform` and `Chunk`).
                        if (chunk.GetComponents<Component>().Length == 2) {
                            int chunkIndex = this.ChunkIndexFromTileIndex(chunk.First);
                            this.chunks[chunkIndex] = null;

                            InternalUtility.Destroy(chunk.gameObject);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Erases all tiles.
        /// </summary>
        public void EraseAllTiles()
        {
            this.hintForceRefresh = false;

            // Tile system doesn't contain any tiles to force refresh!
            if (this.chunks == null) {
                return;
            }

            this.BeginBulkEdit();

            for (int i = 0; i < this.chunks.Length; ++i) {
                var chunk = this.chunks[i];
                if (chunk == null) {
                    continue;
                }

                // Erase each tile in chunk.
                int length = chunk.tiles.Length;
                for (int j = 0; j < length; ++j) {
                    this.EraseTileHelper(chunk, j, false);
                }

                // Should empty chunk be removed?
                if (InternalUtility.ShouldEraseEmptyChunks(this)) {
                    this.chunks[i] = null;

                    InternalUtility.Destroy(chunk.gameObject);
                }
            }

            this.EndBulkEdit();
        }

        /// <summary>
        /// Erase a specific tile.
        /// </summary>
        /// <remarks>
        /// <para>Remember to call <see cref="RefreshSurroundingTiles"/> to refresh the
        /// surrounding tiles afterwards when applicable.</para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// // Erase tile from system.
        /// tileSystem.EraseTile(row, column);
        /// // Update orientations of surrounding tiles.
        /// tileSystem.RefreshSurroundingTiles(row, column);
        /// ]]></code>
        /// </example>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <returns>
        /// A value of <c>true</c> when tile was erased; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public bool EraseTile(int row, int column)
        {
            var chunk = this.GetChunkFromTileIndex(row, column);
            if (chunk == null) {
                return false;
            }

            var tile = this.GetTile(row, column);
            if (tile == null) {
                return false;
            }

            bool procedural = tile.Procedural;

            if (this.EraseTileHelper(chunk, this.IndexOfTileInChunk(row, column), true)) {
                // Was this a procedural tile?
                if (procedural) {
                    this.UpdateProceduralTiles();
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Erase a specific tile.
        /// </summary>
        /// <inheritdoc cref="EraseTile(int, int)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public bool EraseTile(TileIndex index)
        {
            return this.EraseTile(index.row, index.column);
        }

        /// <summary>
        /// Refresh a specific tile using its original brush.
        /// </summary>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="flags">A bitwise combination of <see cref="RefreshFlags"/> values.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index refers to non-existent tile outside bounds of tile system.
        /// </exception>
        public void RefreshTile(int row, int column, RefreshFlags flags = RefreshFlags.None)
        {
            var tile = this.GetTile(row, column);
            if (tile == null | tile.brush == null) {
                return;
            }

            tile.brush.Refresh(this, row, column, flags);
        }

        /// <summary>
        /// Refresh a specific tile using its original brush.
        /// </summary>
        /// <inheritdoc cref="RefreshTile(int, int, RefreshFlags)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public void RefreshTile(TileIndex index, RefreshFlags flags = RefreshFlags.None)
        {
            this.RefreshTile(index.row, index.column, flags);
        }

        /// <summary>
        /// Refresh all tiles in system.
        /// </summary>
        /// <remarks>
        /// <para>Progress bar is shown when this method is invoked in-editor, though this progress
        /// bar is not shown when working in play mode.</para>
        /// </remarks>
        /// <param name="flags">A bitwise combination of <see cref="RefreshFlags"/> values.</param>
        public void RefreshAllTiles(RefreshFlags flags = RefreshFlags.None)
        {
            // Tile system is not satisfied that force refresh has occurred?
            if ((flags & RefreshFlags.Force) == RefreshFlags.Force) {
                this.hintForceRefresh = false;
            }

            if (this.chunks == null) {
                return;
            }

            TileIndex index;
            int i;

            bool restoreEnableProgressHandler = InternalUtility.EnableProgressHandler;
            InternalUtility.EnableProgressHandler = Application.isEditor && !Application.isPlaying;
            try {
                // Overall tile index used for editor progress bar.
                int tileIndex = 0;
                float inverseTileCount = 1f / (float)this.RowCount;

                // Indicates if procedural tiles should be considered for updating.
                bool updateProcedural = (flags & (RefreshFlags.Force | RefreshFlags.UpdateProcedural)) != 0;
                // Indicates if procedural tiles for current chunk need to be updated.
                bool proceduralDirty;

                this.BeginProceduralEditing();

                foreach (var chunk in this.chunks) {
                    if (chunk == null || chunk.tiles == null) {
                        continue;
                    }

                    index = chunk.First;
                    i = 0;
                    proceduralDirty = false;

                    foreach (var tile in chunk.tiles) {
                        if (tile != null) {
                            // Note: Tile might be procedural now, but might not be afterwards!
                            proceduralDirty |= tile.Procedural;

                            if (tile.brush != null && !tile.Empty) {
                                tile.brush.Refresh(this, index.row, index.column, flags);
                            }

                            proceduralDirty |= tile.Procedural;
                        }

                        // Update progress bar in editor.
                        if (++tileIndex % this.ColumnCount == 0) {
                            InternalUtility.ProgressHandler("Refresh All Tiles", "Refreshing tile system...", (float)tileIndex * inverseTileCount);
                        }

                        ++index.column;
                        if (++i >= this.chunkWidth) {
                            i = 0;
                            index.column = chunk.First.column;
                            ++index.row;
                        }
                    }

                    // Flag procedural mesh of chunk as dirty if procedural tiles were detected?
                    if (proceduralDirty && updateProcedural) {
                        chunk.ProceduralDirty = true;
                    }
                }

                InternalUtility.ProgressHandler("Refresh All Tiles", "Updating procedural mesh...", 1f);

                this.EndProceduralEditing();
            }
            finally {
                InternalUtility.ClearProgress();
                InternalUtility.EnableProgressHandler = restoreEnableProgressHandler;
            }
        }

        /// <summary>
        /// Refresh tiles that surround a specific tile.
        /// </summary>
        /// <remarks>
        /// <para>Technical Note: Tiles that are flagged as "dirty" will not be refreshed
        /// because they will need to be replaced when bulk edit mode is ended.</para>
        /// </remarks>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="flags">A bitwise combination of <see cref="RefreshFlags"/> values.</param>
        public void RefreshSurroundingTiles(int row, int column, RefreshFlags flags)
        {
            TileData tile;

            this.BeginProceduralEditing();

            int rowCount = this.RowCount;
            int columnCount = this.ColumnCount;

            for (int rowIndex = row - 1; rowIndex <= row + 1; ++rowIndex) {
                // Skip non-existent row!
                if ((uint)rowIndex >= rowCount) {
                    continue;
                }

                for (int columnIndex = column - 1; columnIndex <= column + 1; ++columnIndex) {
                    // Skip non-existent column and skip targetted tile!
                    if ((uint)columnIndex >= columnCount || (rowIndex == row && columnIndex == column)) {
                        continue;
                    }

                    // Fetch tile from system.
                    tile = this.GetTileOrNull(rowIndex, columnIndex);

                    // Do not attempt to refresh tiles that are already dirty because they
                    // will need to be replaced anyway.
                    if (tile != null && tile.brush != null && !tile.Dirty) {
                        tile.brush.Refresh(this, rowIndex, columnIndex, flags);
                    }
                }
            }

            this.EndProceduralEditing();
        }

        /// <summary>
        /// Refresh tiles that surround a specific tile.
        /// </summary>
        /// <inheritdoc cref="RefreshSurroundingTiles(int, int, RefreshFlags)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public void RefreshSurroundingTiles(TileIndex index, RefreshFlags flags)
        {
            this.RefreshSurroundingTiles(index.row, index.column, flags);
        }

        /// <summary>
        /// Refresh tiles that surround a specific tile.
        /// </summary>
        /// <inheritdoc cref="RefreshSurroundingTiles(int, int, RefreshFlags)"/>
        public void RefreshSurroundingTiles(int row, int column)
        {
            this.RefreshSurroundingTiles(row, column, RefreshFlags.None);
        }

        /// <summary>
        /// Refresh tiles that surround a specific tile.
        /// </summary>
        /// <inheritdoc cref="RefreshSurroundingTiles(int, int, RefreshFlags)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public void RefreshSurroundingTiles(TileIndex index)
        {
            this.RefreshSurroundingTiles(index.row, index.column, RefreshFlags.None);
        }

        /// <summary>
        /// Log of tiles which were erased during repair.
        /// </summary>
        /// <seealso cref="ScanBrokenTiles"/>
        private static List<TileIndex> s_EraseRepairLog = new List<TileIndex>();

        /// <summary>
        /// Count and/or repair broken tiles.
        /// </summary>
        /// <remarks>
        /// <para><strong>Warning: Cannot be called under bulk edit mode.</strong></para>
        /// <para>A painted tile will often be accompanied by a game object that represents
        /// its visual state and behaviour. These game objects can of course be manually
        /// deleted. When the game object that is associated with a tile is deleted, the
        /// tile becomes broken because the game object is missing.</para>
        /// <para>This method counts the number of tiles that are missing the game object
        /// that was painted.</para>
        /// <para>User scripts should erase unwanted tiles via <see cref="EraseTile"/>
        /// or <see cref="EraseAllTiles"/> to ensure that both tile data and game object
        /// are removed.</para>
        /// <para>Broken tiles that are missing their game object can often be automatically
        /// restored to their original state. This can be achieved by force refreshing using
        /// the original brush.</para>
        /// </remarks>
        /// <param name="action">Repair action to undertake for broken and/or dirty tiles.</param>
        /// <param name="lazy">A value of <c>true</c> indicates that a lazier repair action
        /// should be undertaken. Only tiles from chunks that are flagged as dirty will be
        /// scanned for repair.
        /// <para>If in doubt simply assume the default value of <c>false</c>.</para>
        /// </param>
        /// <returns>
        /// The number of detected broken tiles.
        /// </returns>
        public int ScanBrokenTiles(RepairAction action, bool lazy = false)
        {
            int count = 0;

            // Note: This function can be called within `this.BeginProceduralEditing` and `EndProceduralEditing`
            if (this.BulkEditMode) {
                Debug.LogError("Cannot call `TileSystem.ScanBrokenTiles` whilst in bulk edit mode.", this);
                return 0;
            }

            if (this.chunks == null) {
                return 0;
            }

            TileIndex index;
            int i;

            // Clear log of erased tiles so that refreshing of surrounding tiles can
            // be deferred until after all tiles have been erased. This prevents
            // reinsertion of tiles which should have been erased.
            s_EraseRepairLog.Clear();

            this.BeginProceduralEditing();

            foreach (var chunk in this.chunks) {
                if (chunk == null || chunk.tiles == null) {
                    continue;
                }

                // If using lazy action bypass if chunk is not dirty.
                if (lazy && !chunk.Dirty) {
                    continue;
                }

                index = chunk.First;
                i = 0;

                foreach (var tile in chunk.tiles) {
                    // Only consider valid tile.
                    if (tile != null && !tile.Empty) {
                        switch (action) {
                            case RepairAction.JustCount:
                                if (tile.IsGameObjectMissing || tile.Dirty) {
                                    ++count;
                                }
                                break;

                            case RepairAction.Erase:
                                if (tile.IsGameObjectMissing || tile.Dirty) {
                                    this.EraseTileHelper(chunk, this.IndexOfTileInChunk(index), true);
                                    s_EraseRepairLog.Add(index);
                                    ++count;
                                }
                                break;

                            case RepairAction.ForceRefresh:
                                if (!tile.IsGameObjectMissing && !tile.Dirty) {
                                    break;
                                }

                                // Simply erase tile when brush is missing.
                                if (tile.brush == null) {
                                    this.EraseTileHelper(chunk, this.IndexOfTileInChunk(index), true);
                                    s_EraseRepairLog.Add(index);
                                }
                                else {
                                    if (tile.Dirty) {
                                        this.RefreshSurroundingTiles(index.row, index.column, RefreshFlags.PreservePaintedFlags | RefreshFlags.PreserveTransform);
                                    }
                                    this.RefreshTile(index.row, index.column, RefreshFlags.Force);
                                }

                                ++count;
                                break;

                            case RepairAction.RefreshDirty:
                                if (!tile.Dirty)
                                    break;

                                // Simply erase tile when brush is missing.
                                if (tile.brush == null) {
                                    this.EraseTileHelper(chunk, this.IndexOfTileInChunk(index), true);
                                    s_EraseRepairLog.Add(index);
                                }
                                else {
                                    this.RefreshTile(index.row, index.column, RefreshFlags.Force);
                                    this.RefreshSurroundingTiles(index.row, index.column, RefreshFlags.PreservePaintedFlags | RefreshFlags.PreserveTransform);
                                }

                                ++count;
                                break;
                        }
                    }

                    // Proceed to next tile.
                    ++index.column;
                    if (++i >= this.chunkWidth) {
                        i = 0;
                        index.column = chunk.First.column;
                        ++index.row;
                    }
                }

                // Chunk is no longer dirty.
                chunk.Dirty = false;
            }

            // Refresh tiles which surround tiles which have been erased?
            if (s_EraseRepairLog.Count > 0) {
                for (int logIndex = 0; logIndex < s_EraseRepairLog.Count; ++logIndex) {
                    this.RefreshSurroundingTiles(s_EraseRepairLog[logIndex], RefreshFlags.PreservePaintedFlags | RefreshFlags.PreserveTransform);
                }
                s_EraseRepairLog.Clear();
            }

            this.EndProceduralEditing();

            return count;
        }

        /// <summary>
        /// Replaces all tiles that were painted using a specific brush.
        /// </summary>
        /// <param name="source">Search for tiles that were painted using <c>source</c> brush.</param>
        /// <param name="replacement">Repaint matching tiles using <c>replacement</c> brush.
        /// Specify <c>null</c> to erase matching tiles.</param>
        /// <returns>
        /// The number of tiles that were replaced.
        /// </returns>
        public int ReplaceByBrush(Brush source, Brush replacement)
        {
            // Do not proceed with replacement if source and replacement are the same!
            if (source == null || source == replacement) {
                return 0;
            }

            int count = 0;

            this.BeginBulkEdit();

            for (int row = 0; row < this.RowCount; ++row) {
                for (int column = 0; column < this.ColumnCount; ++column) {
                    var tile = this.GetTile(row, column);
                    if (tile == null || tile.brush != source) {
                        continue;
                    }

                    if (replacement != null) {
                        replacement.Paint(this, row, column, tile.variationIndex);
                    }
                    else {
                        this.EraseTile(row, column);
                    }

                    ++count;
                }
            }

            this.EndBulkEdit();

            return count;
        }

        /// <summary>
        /// Index of closest tile from ray.
        /// </summary>
        /// <remarks>
        /// <para>The calculated index is clamped by the boundaries of the tile system.
        /// This means that when the ray does not intersect with the tile system area, the
        /// index of a tile around the edge of the tile system will be returned.</para>
        /// </remarks>
        /// <param name="ray">Ray in world space.</param>
        /// <returns>
        /// The zero-based tile index.
        /// </returns>
        public TileIndex ClosestTileIndexFromRay(Ray ray)
        {
            // Calculate point where mouse ray intersect tile system plane.
            float distanceToPlane = 0f;
            if (Plane.Raycast(ray, out distanceToPlane)) {
                // Calculate world position of cursor in local space of tile system.
                Vector3 worldPoint = ray.GetPoint(distanceToPlane);
                Vector3 localPoint = this.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPoint);

                // Calculate position within grid.
                return new TileIndex(
                    Mathf.Clamp((int)(-localPoint.y / this.CellSize.y), 0, this.RowCount - 1),
                    Mathf.Clamp((int)(localPoint.x / this.CellSize.x), 0, this.ColumnCount - 1)
                );
            }

            return TileIndex.zero;
        }

        /// <summary>
        /// Index of closest tile from ray.
        /// </summary>
        /// <remarks>
        /// <para>The calculated index is clamped by the boundaries of the tile system.
        /// This means that when the ray does not intersect with the tile system area, the
        /// index of a tile around the edge of the tile system will be returned.</para>
        /// </remarks>
        /// <param name="worldPoint">Point in world space.</param>
        /// <returns>
        /// The tile index.
        /// </returns>
        public TileIndex ClosestTileIndexFromWorld(Vector3 worldPoint)
        {
            Ray ray = new Ray(worldPoint, Vector3.Scale(this.Plane.normal, MathUtility.MinusOneVector));
            ray.origin = ray.GetPoint(-100f);
            return this.ClosestTileIndexFromRay(ray);
        }

        #endregion


        #region Sorting Layers

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
            set {
                if (value != this.sortingLayerID) {
                    this.sortingLayerID = value;
                    this.ApplySortingPropertiesToExistingProceduralMeshes(this.SortingLayerID, this.SortingOrder);
                }
            }
        }

        /// <summary>
        /// Gets or sets order in sorting layers which is used to control render order of
        /// procedurally generated meshes.
        /// </summary>
        public int SortingOrder {
            get { return this.sortingOrder; }
            set {
                if (value != this.sortingOrder) {
                    this.sortingOrder = value;
                    this.ApplySortingPropertiesToExistingProceduralMeshes(this.SortingLayerID, this.SortingOrder);
                }
            }
        }

        public void ApplySortingPropertiesToExistingProceduralMeshes(int sortingLayerID, int sortingOrder)
        {
            foreach (var chunk in this.Chunks) {
                if (chunk == null || chunk.ProceduralMesh == null) {
                    continue;
                }

                var renderer = chunk.ProceduralMesh.GetComponent<Renderer>();
                renderer.sortingLayerID = sortingLayerID;
                renderer.sortingOrder = sortingOrder;
            }
        }

        #endregion


        #region Procedural Mesh

        /// <summary>
        /// Indicates if normals should be added to procedurally generated meshes.
        /// </summary>
        /// <remarks>
        /// <para>Only add normals when they are required by the shaders that you would
        /// like to use.</para>
        /// </remarks>
        public bool addProceduralNormals;

        [SerializeField, FormerlySerializedAs("_markProceduralDynamic")]
        private bool markProceduralDynamic = true;


        /// <summary>
        /// Gets or sets a value indicating whether procedurally generated meshes are
        /// likely to be updated throughout play where tiles are painted and/or erased
        /// frequently to improve performance (this may use more memory).
        /// </summary>
        /// <remarks>
        /// <para>It is useful to mark procedural meshes as dynamic when painting or
        /// erasing tiles as part of game-play or a custom in-game level designer.</para>
        /// <para>Avoid setting if procedural meshes are only updated at start of
        /// level when loading or generating map.</para>
        /// </remarks>
        public bool MarkProceduralDynamic {
            get { return this.markProceduralDynamic; }
            set { this.markProceduralDynamic = value; }
        }


        private int updateProceduralDepth;

        //!TODO: Benefits are generally internal, perhaps consider exposing in the future...
        /// <exclude/>
        public void BeginProceduralEditing()
        {
            ++this.updateProceduralDepth;
        }

        //!TODO: Benefits are generally internal, perhaps consider exposing in the future...
        /// <exclude/>
        public void EndProceduralEditing()
        {
            if (--this.updateProceduralDepth <= 0) {
                this.updateProceduralDepth = 0;
                this.UpdateProceduralTiles();
            }
        }

        /// <summary>
        /// Update procedural meshes from tile data.
        /// </summary>
        /// <param name="force">A value of <c>true</c> indicates that all procedural meshes
        /// should be updated regardless of whether changes are detected. A value of <c>false</c>
        /// will only update procedural chunks that have been flagged as dirty.</param>
        public void UpdateProceduralTiles(bool force = false)
        {
            if (this.updateProceduralDepth != 0) {
                if (force) {
                    // Flag all procedural meshes for update because that will now be expected!
                    for (int i = 0; i < this.chunks.Length; ++i) {
                        var chunk = this.chunks[i];
                        if (chunk != null && chunk.ProceduralMesh != null) {
                            chunk.ProceduralDirty = true;
                        }
                    }
                }
                return;
            }

            for (int i = 0; i < this.chunks.Length; ++i) {
                var chunk = this.chunks[i];
                if (chunk == null || !(force || chunk.ProceduralDirty)) {
                    continue;
                }

                var proceduralMesh = chunk.ProceduralMesh;
                if (proceduralMesh != null) {
                    proceduralMesh.UpdateMesh();
                }
                else {
                    chunk.ProceduralDirty = false;
                }
            }
        }

        #endregion
    }
}
