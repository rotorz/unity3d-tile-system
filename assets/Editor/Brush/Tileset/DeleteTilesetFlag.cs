// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Flags that can be specified when deleting a tileset.
    /// </summary>
    /// <seealso cref="BrushUtility.DeleteTileset"/>
    [Flags]
    public enum DeleteTilesetFlag
    {
        /// <summary>
        /// Delete tileset texture asset.
        /// </summary>
        /// <remarks>
        /// <para>Texture asset will only be deleted if it is stored in the same folder
        /// as the tileset asset.</para>
        /// </remarks>
        DeleteTexture = 0x01,

        /// <summary>
        /// Delete tileset material asset.
        /// </summary>
        /// <remarks>
        /// <para>Material asset will only be deleted if it is stored in the same folder
        /// as the tileset asset.</para>
        /// </remarks>
        DeleteMaterial = 0x02,

        /// <summary>
        /// Delete non-procedural mesh assets.
        /// </summary>
        /// <remarks>
        /// <para>Non-procedural mesh asset will only be deleted if it is stored in the
        /// same folder as the tileset asset.</para>
        /// </remarks>
        DeleteMeshAssets = 0x04,
    }
}
