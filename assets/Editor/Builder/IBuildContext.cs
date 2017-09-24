// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Interface that describes context when building a tile system.
    /// </summary>
    public interface IBuildContext
    {
        /// <summary>
        /// Gets the tile system that is being built.
        /// </summary>
        /// <remarks>
        /// <para>This may become <c>null</c> if tile system component has been stripped.</para>
        /// </remarks>
        TileSystem TileSystem { get; }

        /// <summary>
        /// Gets game object of tile system that is being built.
        /// </summary>
        GameObject TileSystemGameObject { get; }

        /// <summary>
        /// Gets the build combine method.
        /// </summary>
        BuildCombineMethod Method { get; }

        /// <summary>
        /// Gets the width of a combined chunk.
        /// </summary>
        int CombineChunkWidth { get; }
        /// <summary>
        /// Gets the height of a combined chunk.
        /// </summary>
        int CombineChunkHeight { get; }
    }
}
