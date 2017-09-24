// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Calculates metrics for a tileset.
    /// </summary>
    /// <remarks>
    /// <para>Metrics can be copied from objects of this class directly into tileset
    /// assets. This is useful when creating or modifying tileset assets.</para>
    /// </remarks>
    /// <example>
    /// <para>Initialize tileset and prepare metrics:</para>
    /// <code language="csharp"><![CDATA[
    /// // Calculate metrics for tileset.
    /// var metrics = new TilesetMetrics(
    ///       atlas: atlasTexture,
    ///       tileWidth: 32,
    ///       tileHeight: 32
    ///       borderSize: 4,
    ///       delta: 0f
    ///    );
    ///
    /// // Initialize tileset and assign material, atlas and metrics.
    /// tileset.Initialize(material, atlasTexture, metrics);
    /// ]]></code>
    /// </example>
    /// <seealso cref="Tileset"/>
    public sealed class TilesetMetrics : ITilesetMetrics
    {
        /// <summary>
        /// Initialize new instance of <see cref="TilesetMetrics"/>.
        /// </summary>
        public TilesetMetrics()
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="TilesetMetrics"/> class and calculate
        /// metrics for the specified tileset.
        /// </summary>
        /// <remarks>
        /// <para>Measurements are relative to original atlas size.</para>
        /// </remarks>
        /// <param name="atlas">Atlas texture for tileset.</param>
        /// <param name="tileWidth">Width of tile in pixels.</param>
        /// <param name="tileHeight">Height of tile in pixels.</param>
        /// <param name="borderSize">Border size in pixels.</param>
        /// <param name="delta">UV delta offset (fraction of pixel).</param>
        public TilesetMetrics(Texture2D atlas, int tileWidth, int tileHeight, int borderSize, float delta)
        {
            this.Calculate(atlas, tileWidth, tileHeight, borderSize, delta);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="TilesetMetrics"/> class and calculate
        /// metrics for the specified tileset.
        /// </summary>
        /// <remarks>
        /// <para>Measurements are relative to original atlas size.</para>
        /// </remarks>
        /// <param name="atlasWidth">Width of tileset atlas in pixels.</param>
        /// <param name="atlasHeight">Height of tileset atlas in pixels.</param>
        /// <param name="tileWidth">Width of tile in pixels.</param>
        /// <param name="tileHeight">Height of tile in pixels.</param>
        /// <param name="borderSize">Border size in pixels.</param>
        /// <param name="delta">UV delta offset (fraction of pixel).</param>
        public TilesetMetrics(int atlasWidth, int atlasHeight, int tileWidth, int tileHeight, int borderSize, float delta)
        {
            this.Calculate(atlasWidth, atlasHeight, tileWidth, tileHeight, borderSize, delta);
        }


        /// <summary>
        /// Clear all metrics.
        /// </summary>
        public void Clear()
        {
            this.OriginalAtlasWidth = this.OriginalAtlasHeight = 0;
            this.Rows = this.Columns = 0;

            this.TileWidth = this.TileHeight = 0;

            this.BorderSize = 0;
            this.Delta = 0f;

            this.TileIncrementX = this.TileIncrementY = 0;

            this.BorderU = this.BorderV = 0f;
            this.TileWidthUV = this.TileHeightUV = 0f;
            this.DeltaU = this.DeltaV = 0f;
            this.TileIncrementU = this.TileIncrementV = 0f;
        }

        /// <summary>
        /// Calculate metrics for specified tileset.
        /// </summary>
        /// <remarks>
        /// <para>Measurements are relative to original atlas size.</para>
        /// </remarks>
        /// <param name="atlas">Atlas texture for tileset.</param>
        /// <param name="tileWidth">Width of tile in pixels.</param>
        /// <param name="tileHeight">Height of tile in pixels.</param>
        /// <param name="borderSize">Border size in pixels.</param>
        /// <param name="delta">UV delta offset (fraction of pixel).</param>
        /// <returns>
        /// A value of <c>true</c> when valid atlas was specified; otherwise a value of <c>false</c>.
        /// </returns>
        public bool Calculate(Texture2D atlas, int tileWidth, int tileHeight, int borderSize, float delta)
        {
            int atlasWidth, atlasHeight;

            if (!EditorInternalUtility.GetImageSize(atlas, out atlasWidth, out atlasHeight)) {
                this.Clear();
                return false;
            }

            return this.Calculate(atlasWidth, atlasHeight, tileWidth, tileHeight, borderSize, delta);
        }

        /// <summary>
        /// Calculate metrics for specified tileset.
        /// </summary>
        /// <remarks>
        /// <para>Measurements are relative to original atlas size.</para>
        /// </remarks>
        /// <param name="atlasWidth">Width of tileset atlas in pixels.</param>
        /// <param name="atlasHeight">Height of tileset atlas in pixels.</param>
        /// <param name="tileWidth">Width of tile in pixels.</param>
        /// <param name="tileHeight">Height of tile in pixels.</param>
        /// <param name="borderSize">Border size in pixels.</param>
        /// <param name="delta">UV delta offset (fraction of pixel).</param>
        /// <returns>
        /// A value of <c>true</c> when valid atlas was specified; otherwise a value of <c>false</c>.
        /// </returns>
        public bool Calculate(int atlasWidth, int atlasHeight, int tileWidth, int tileHeight, int borderSize, float delta)
        {
            if (atlasWidth == 0 || atlasHeight == 0) {
                this.Clear();
                return false;
            }

            float fAtlasWidth = (float)atlasWidth;
            float fAtlasHeight = (float)atlasHeight;

            this.OriginalAtlasWidth = atlasWidth;
            this.OriginalAtlasHeight = atlasHeight;

            // Calculate increment to next tile.
            this.TileIncrementX = borderSize * 2 + tileWidth;
            this.TileIncrementY = borderSize * 2 + tileHeight;
            this.TileIncrementU = (float)this.TileIncrementX / fAtlasWidth;
            this.TileIncrementV = (float)this.TileIncrementY / fAtlasHeight;

            // Calculate size of tile in UV space.
            this.TileWidth = tileWidth;
            this.TileHeight = tileHeight;
            this.TileWidthUV = (float)tileWidth / fAtlasWidth;
            this.TileHeightUV = (float)tileHeight / fAtlasHeight;

            // Calculate size of border in UV space.
            this.BorderSize = borderSize;
            this.BorderU = (float)borderSize / fAtlasWidth;
            this.BorderV = (float)borderSize / fAtlasHeight;

            // Calculate delta in UV space.
            this.Delta = delta;
            this.DeltaU = delta / fAtlasWidth;
            this.DeltaV = delta / fAtlasHeight;

            // Calculate number of rows and columns in atlas.
            this.Rows = atlasHeight / this.TileIncrementY;
            this.Columns = atlasWidth / this.TileIncrementX;

            return true;
        }


        #region ITilesetMetrics Implementation

        /// <inheritdoc/>
        public int OriginalAtlasWidth { get; private set; }
        /// <inheritdoc/>
        public int OriginalAtlasHeight { get; private set; }

        /// <inheritdoc/>
        public int Rows { get; private set; }
        /// <inheritdoc/>
        public int Columns { get; private set; }

        /// <inheritdoc/>
        public int TileIncrementX { get; private set; }
        /// <inheritdoc/>
        public int TileIncrementY { get; private set; }
        /// <inheritdoc/>
        public float TileIncrementU { get; private set; }
        /// <inheritdoc/>
        public float TileIncrementV { get; private set; }

        /// <inheritdoc/>
        public int TileWidth { get; private set; }
        /// <inheritdoc/>
        public int TileHeight { get; private set; }
        /// <inheritdoc/>
        public float TileWidthUV { get; private set; }
        /// <inheritdoc/>
        public float TileHeightUV { get; private set; }

        /// <inheritdoc/>
        public int BorderSize { get; private set; }
        /// <inheritdoc/>
        public float BorderU { get; private set; }
        /// <inheritdoc/>
        public float BorderV { get; private set; }

        /// <inheritdoc/>
        public float Delta { get; private set; }
        /// <inheritdoc/>
        public float DeltaU { get; private set; }
        /// <inheritdoc/>
        public float DeltaV { get; private set; }

        #endregion
    }
}
