// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// A tileset allows tileset brushes to paint procedural or non-procedural tiles using
    /// an atlas texture that contains multiple tiles.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Tilesets">Tilesets</a>
    /// section of user guide for further information about tilesets.</para>
    /// </intro>
    /// <remarks>
    /// <para>Tile atlases can include additional border area around each tile reduce the
    /// effects of bleeding that is caused by texture filtering. Alternatively you can
    /// choose to inset UVs by a small fraction of a pixel (delta). Whilst specifying a
    /// delta is not as effective as adding borders around tiles, a delta does not require
    /// manual changes to be made to the atlas artwork.</para>
    /// </remarks>
    /// <seealso cref="TilesetBrush"/>
    public class Tileset : ScriptableObject, IDesignableObject, ITilesetMetrics
    {
        #region Metrics

        [SerializeField, HideInInspector, FormerlySerializedAs("_originalAtlasWidth")]
        internal int originalAtlasWidth;
        [SerializeField, HideInInspector, FormerlySerializedAs("_originalAtlasHeight")]
        internal int originalAtlasHeight;

        [SerializeField, HideInInspector, FormerlySerializedAs("_rows")]
        internal int rowCount;
        [SerializeField, HideInInspector, FormerlySerializedAs("_columns")]
        internal int columnCount;

        [SerializeField, HideInInspector, FormerlySerializedAs("_tileIncrementX")]
        internal int tileIncrementX;
        [SerializeField, HideInInspector, FormerlySerializedAs("_tileIncrementY")]
        internal int tileIncrementY;
        [SerializeField, HideInInspector, FormerlySerializedAs("_tileIncrementU")]
        internal float tileIncrementU;
        [SerializeField, HideInInspector, FormerlySerializedAs("_tileIncrementV")]
        internal float tileIncrementV;

        [SerializeField, HideInInspector, FormerlySerializedAs("_tileWidth")]
        internal int tileWidth;
        [SerializeField, HideInInspector, FormerlySerializedAs("_tileHeight")]
        internal int tileHeight;
        [SerializeField, HideInInspector, FormerlySerializedAs("_tileWidthUV")]
        internal float tileWidthUV;
        [SerializeField, HideInInspector, FormerlySerializedAs("_tileHeightUV")]
        internal float tileHeightUV;

        [SerializeField, HideInInspector, FormerlySerializedAs("_borderSize")]
        internal int borderSize;
        [SerializeField, HideInInspector, FormerlySerializedAs("_borderU")]
        internal float borderU;
        [SerializeField, HideInInspector, FormerlySerializedAs("_borderV")]
        internal float borderV;

        [SerializeField, HideInInspector, FormerlySerializedAs("_delta")]
        internal float delta;
        [SerializeField, HideInInspector, FormerlySerializedAs("_deltaU")]
        internal float deltaU;
        [SerializeField, HideInInspector, FormerlySerializedAs("_deltaV")]
        internal float deltaV;


        /// <summary>
        /// Set metrics of tileset.
        /// </summary>
        /// <param name="metrics">Object that contains metrics for tileset.</param>
        /// <exception cref="System.InvalidOperationException">
        /// If attempting to set metrics of tileset from itself.
        /// </exception>
        public void SetMetricsFrom(ITilesetMetrics metrics)
        {
            if (ReferenceEquals(metrics, this)) {
                throw new InvalidOperationException("Unable to set metrics of tileset from itself!");
            }

            this.originalAtlasWidth = metrics.OriginalAtlasWidth;
            this.originalAtlasHeight = metrics.OriginalAtlasHeight;

            this.rowCount = metrics.Rows;
            this.columnCount = metrics.Columns;

            this.tileIncrementX = metrics.TileIncrementX;
            this.tileIncrementY = metrics.TileIncrementY;
            this.tileIncrementU = metrics.TileIncrementU;
            this.tileIncrementV = metrics.TileIncrementV;

            this.tileWidth = metrics.TileWidth;
            this.tileHeight = metrics.TileHeight;
            this.tileWidthUV = metrics.TileWidthUV;
            this.tileHeightUV = metrics.TileHeightUV;

            this.borderSize = metrics.BorderSize;
            this.borderU = metrics.BorderU;
            this.borderV = metrics.BorderV;

            this.delta = metrics.Delta;
            this.deltaU = metrics.DeltaU;
            this.deltaV = metrics.DeltaV;
        }

        #endregion


        #region Properties

        /// <inheritdoc/>
        public virtual string DesignableType {
            get { return "Tileset"; }
        }


        /// <summary>
        /// Indicates if tiles should be generated procedurally or whether to create a
        /// game object for each tile.
        /// </summary>
        public bool procedural;

        [SerializeField, HideInInspector, FormerlySerializedAs("_atlasMaterial")]
        private Material atlasMaterial;
        [SerializeField, HideInInspector, FormerlySerializedAs("_atlasTexture")]
        private Texture2D atlasTexture;


        /// <summary>
        /// Gets or sets atlas material.
        /// </summary>
        public Material AtlasMaterial {
            get { return this.atlasMaterial; }
            set { this.atlasMaterial = value; }
        }
        /// <summary>
        /// Gets or sets atlas texture.
        /// </summary>
        public Texture2D AtlasTexture {
            get { return this.atlasTexture; }
            set { this.atlasTexture = value; }
        }

        /// <inheritdoc/>
        string IDesignableObject.DisplayName {
            get { return this.name; }
        }

        /// <inheritdoc/>
        string IHistoryObject.HistoryName {
            get { return (this as IDesignableObject).DisplayName + " : " + this.DesignableType; }
        }

        /// <inheritdoc/>
        bool IHistoryObject.Exists {
            get { return this != null; }
        }

        #endregion


        #region Non-Procedural Mesh Management

        [SerializeField, HideInInspector, FormerlySerializedAs("_tileMeshAsset")]
        public TilesetMeshAsset tileMeshAsset;

        [SerializeField, HideInInspector, FormerlySerializedAs("_tileMeshes")]
        public Mesh[] tileMeshes = new Mesh[0];


        /// <summary>
        /// Prepares mesh for specific non-procedural tile if it has not already been prepared.
        /// Non-procedural meshes are typically prepared and stored within tileset asset.
        /// </summary>
        /// <remarks>
        /// <para>Tile meshes are used to present non-procedural tiles from tilesets and
        /// are typically pre-generated at design time. A tile mesh is a plane comprising of
        /// 4 vertices and 2 triangles with UV coordinates calculated for the specific tile.</para>
        /// <para>Custom scripts can use this method to generate meshes for procedural tiles
        /// at runtime if desired. Though if you are creating some sort of preview mechanism,
        /// you may prefer to reuse a single preview mesh using <see cref="CreateStandaloneTileMesh"/>
        /// instead of generating one mesh for each tile.</para>
        /// </remarks>
        /// <param name="tileIndex">Zero-based index of tile in atlas.</param>
        /// <returns>
        /// The tile <see cref="UnityEngine.Mesh"/>.
        /// </returns>
        /// <seealso cref="RefreshTileMesh"/>
        public Mesh PrepareTileMesh(int tileIndex)
        {
            if (this.tileMeshes == null || tileIndex >= this.tileMeshes.Length || this.tileMeshes[tileIndex] == null) {
                this.RefreshTileMesh(tileIndex);
            }
            return this.tileMeshes[tileIndex];
        }

        /// <summary>
        /// Refresh mesh of non-procedural tile if it already exists, otherwise create it.
        /// </summary>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <returns>
        /// The tile <see cref="UnityEngine.Mesh"/>.
        /// </returns>
        /// <seealso cref="PrepareTileMesh"/>
        public Mesh RefreshTileMesh(int tileIndex)
        {
            if (this.tileMeshes == null) {
                this.tileMeshes = new Mesh[tileIndex + 1];
            }
            else if (tileIndex >= this.tileMeshes.Length) {
                Array.Resize(ref this.tileMeshes, tileIndex + 1);
            }

            var mesh = this.tileMeshes[tileIndex];
            if (mesh == null) {
                mesh = this.CreateStandaloneTileMesh(tileIndex);
            }
            else {
                this.UpdateStandaloneTileMesh(tileIndex, mesh);
            }

            this.tileMeshes[tileIndex] = mesh;
            return mesh;
        }

        /// <summary>
        /// Gets non-procedural mesh for tile.
        /// </summary>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <returns>
        /// The <see cref="UnityEngine.Mesh"/> when available; otherwise <c>null</c> if
        /// tile mesh has not been prepared.
        /// </returns>
        public Mesh GetTileMesh(int tileIndex)
        {
            return (this.tileMeshes != null && tileIndex < this.tileMeshes.Length)
                ? this.tileMeshes[tileIndex]
                : null;
        }

        private static readonly Vector3[] s_TilePlaneVertices = new Vector3[] {
            new Vector3(+0.5f, +0.5f, 0f),
            new Vector3(+0.5f, -0.5f, 0f),
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(-0.5f, +0.5f, 0f),
        };

        private static readonly int[] s_TilePlaneTriangles = new int[] {
            2, 1, 0,
            0, 3, 2
        };

        /// <summary>
        /// Create new two-triangle plane mesh to represent tile. Mesh is not maintained
        /// within tileset and should be manually destroyed when nolonger needed.
        /// </summary>
        /// <remarks>
        /// <para>If you are looking to pre-generate non-procedural meshes then you should
        /// be using <see cref="PrepareTileMesh"/> instead.</para>
        /// </remarks>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <returns>
        /// Sparkling new mesh with 4 vertices and 2 triangles fitting one unit of space.
        /// </returns>
        /// <seealso cref="UpdateStandaloneTileMesh"/>
        /// <seealso cref="UpdateStandaloneTileMeshUVs"/>
        public Mesh CreateStandaloneTileMesh(int tileIndex)
        {
            var mesh = new Mesh();
            mesh.name = tileIndex.ToString();

            this.UpdateStandaloneTileMesh(tileIndex, mesh);

            return mesh;
        }

        private static Vector2[] s_UvBuffer = new Vector2[4];

        /// <summary>
        /// Update vertices, uvs and triangles of existing standalone tile mesh.
        /// </summary>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <param name="mesh">Mesh which is to be updated.</param>
        /// <seealso cref="CreateStandaloneTileMesh"/>
        /// <seealso cref="UpdateStandaloneTileMeshUVs"/>
        public void UpdateStandaloneTileMesh(int tileIndex, Mesh mesh)
        {
            mesh.Clear();

            mesh.vertices = s_TilePlaneVertices;
            this.UpdateStandaloneTileMeshUVs(tileIndex, mesh);
            mesh.triangles = s_TilePlaneTriangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// Just update UV coordinates of existing standalone tile mesh.
        /// </summary>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <param name="mesh">Mesh which is to be updated must already contain 4 vertices and 2 triangles.</param>
        /// <seealso cref="CreateStandaloneTileMesh"/>
        /// <seealso cref="UpdateStandaloneTileMesh"/>
        public void UpdateStandaloneTileMeshUVs(int tileIndex, Mesh mesh)
        {
            // Calculate index of row and column of brush in tileset.
            float column = (float)(tileIndex % this.Columns);
            float row = (float)(tileIndex / this.Columns);

            float x = this.borderU + this.tileIncrementU * column;
            float y = this.borderV + this.tileIncrementV * row;

            // Calculate mapping coordinates.
            s_UvBuffer[0] = new Vector2(x + this.deltaU, 1f - (y + this.deltaV));
            s_UvBuffer[1] = new Vector2(x + this.deltaU, 1f - (y + this.tileHeightUV - this.deltaV));
            s_UvBuffer[2] = new Vector2(x + this.tileWidthUV - this.deltaU, 1f - (y + this.tileHeightUV - this.deltaV));
            s_UvBuffer[3] = new Vector2(x + this.tileWidthUV - this.deltaU, 1f - (y + this.deltaV));
            mesh.uv = s_UvBuffer;
        }

        /// <summary>
        /// Calculate texture coordinates for use with <b>GUI.DrawTextureWithTexCoords</b>.
        /// </summary>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <returns>
        /// Texture coordinates.
        /// </returns>
        public Rect CalculateTexCoords(int tileIndex)
        {
            // Calculate index of row and column of tile in atlas.
            float column = (float)(tileIndex % this.columnCount);
            float row = (float)(tileIndex / this.columnCount);

            return new Rect(
                this.borderU + column * this.tileIncrementU,
                1f - this.borderV - this.tileHeightUV - row * this.tileIncrementV,
                this.tileWidthUV,
                this.tileHeightUV
            );
        }

        #endregion


        #region Methods

        /// <summary>
        /// Initialize tileset for first time.
        /// </summary>
        /// <param name="material">Atlas material.</param>
        /// <param name="atlas">Atlas texture.</param>
        /// <param name="metrics">Object that contains metrics for tileset.</param>
        /// <exception cref="System.NotSupportedException">
        /// If tileset has already been initialized.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="atlas"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// If attempting to set metrics of tileset from itself.
        /// </exception>
        public void Initialize(Material material, Texture2D atlas, ITilesetMetrics metrics)
        {
            if (this.tileWidth != 0) {
                throw new NotSupportedException("Tileset has already been initialized.");
            }
            if (material == null) {
                throw new ArgumentNullException("material");
            }

            this.atlasMaterial = material;
            this.atlasTexture = atlas;

            this.SetMetricsFrom(metrics);
        }

        #endregion


        #region ITilesetMetrics Implementation

        /// <inheritdoc/>
        public int OriginalAtlasWidth {
            get { return this.originalAtlasWidth; }
        }
        /// <inheritdoc/>
        public int OriginalAtlasHeight {
            get { return this.originalAtlasHeight; }
        }

        /// <inheritdoc/>
        public int Rows {
            get { return this.rowCount; }
        }
        /// <inheritdoc/>
        public int Columns {
            get { return this.columnCount; }
        }

        /// <inheritdoc/>
        public int TileIncrementX {
            get { return this.tileIncrementX; }
        }
        /// <inheritdoc/>
        public int TileIncrementY {
            get { return this.tileIncrementY; }
        }
        /// <inheritdoc/>
        public float TileIncrementU {
            get { return this.tileIncrementU; }
        }
        /// <inheritdoc/>
        public float TileIncrementV {
            get { return this.tileIncrementV; }
        }

        /// <inheritdoc/>
        public int TileWidth {
            get { return this.tileWidth; }
        }
        /// <inheritdoc/>
        public int TileHeight {
            get { return this.tileHeight; }
        }
        /// <inheritdoc/>
        public float TileWidthUV {
            get { return this.tileWidthUV; }
        }
        /// <inheritdoc/>
        public float TileHeightUV {
            get { return this.tileHeightUV; }
        }

        /// <inheritdoc/>
        public int BorderSize {
            get { return this.borderSize; }
        }
        /// <inheritdoc/>
        public float BorderU {
            get { return this.borderU; }
        }
        /// <inheritdoc/>
        public float BorderV {
            get { return this.borderV; }
        }

        /// <inheritdoc/>
        public float Delta {
            get { return this.delta; }
        }
        /// <inheritdoc/>
        public float DeltaU {
            get { return this.deltaU; }
        }
        /// <inheritdoc/>
        public float DeltaV {
            get { return this.deltaV; }
        }

        #endregion
    }
}
