// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Metrics of a tileset.
    /// </summary>
    /// <seealso cref="Tileset"/>
    public interface ITilesetMetrics
    {
        /// <summary>
        /// Gets width of original texture asset.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property may differ from <b>UnityEngine.Texture2D.width</b>
        /// because it identifies the width of the texture before being processed.</para>
        /// </remarks>
        int OriginalAtlasWidth { get; }

        /// <summary>
        /// Gets height of original texture asset.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property may differ from <b>UnityEngine.Texture2D.height</b>
        /// because it identifies the height of the texture before being processed.</para>
        /// </remarks>
        int OriginalAtlasHeight { get; }

        /// <summary>
        /// Gets number of rows of tiles in atlas texture.
        /// </summary>
        int Rows { get; }

        /// <summary>
        /// Gets number of columns of tiles in atlas texture.
        /// </summary>
        int Columns { get; }


        /// <summary>
        /// Gets offset in pixels to next tile on X-axis of atlas texture.
        /// </summary>
        int TileIncrementX { get; }

        /// <summary>
        /// Gets offset in pixels to next tile on Y-axis of atlas texture.
        /// </summary>
        int TileIncrementY { get; }

        /// <summary>
        /// Gets offset to next tile on U-axis of UV coordinates.
        /// </summary>
        float TileIncrementU { get; }

        /// <summary>
        /// Gets offset to next tile on V-axis of UV coordinates.
        /// </summary>
        float TileIncrementV { get; }


        /// <summary>
        /// Gets width of tile in pixels.
        /// </summary>
        int TileWidth { get; }

        /// <summary>
        /// Gets height of tile in pixels.
        /// </summary>
        int TileHeight { get; }

        /// <summary>
        /// Gets width of tile in UV space.
        /// </summary>
        float TileWidthUV { get; }

        /// <summary>
        /// Gets height of tile in UV space.
        /// </summary>
        float TileHeightUV { get; }


        /// <summary>
        /// Gets size of tile border in pixels.
        /// </summary>
        int BorderSize { get; }

        /// <summary>
        /// Gets size of tile border on U-axis in UV space.
        /// </summary>
        float BorderU { get; }

        /// <summary>
        /// Gets size of tile border on V-axis in UV space.
        /// </summary>
        float BorderV { get; }


        /// <summary>
        /// Gets delta value.
        /// </summary>
        /// <remarks>
        /// <para>Delta identifies inset of UV coordinates which is specified as the
        /// percentage of a pixel <c>0f</c> through to <c>1f</c>.</para>
        /// </remarks>
        float Delta { get; }

        /// <summary>
        /// Gets delta for U-axis.
        /// </summary>
        float DeltaU { get; }

        /// <summary>
        /// Gets delta for V-axis.
        /// </summary>
        float DeltaV { get; }
    }
}
