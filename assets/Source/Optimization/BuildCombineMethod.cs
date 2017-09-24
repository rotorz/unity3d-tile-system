// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Method of combining to perform upon build.
    /// </summary>
    /// <remarks>
    /// <para>Pre-generated procedural meshes are not merged with tiles that were painted
    /// using a static brush.</para>
    /// <para><strong>Warning,</strong> errors may occur if merged regions contain an
    /// excessive number of vertices. At present there is a hard limit of 64k vertices per
    /// <see cref="UnityEngine.Mesh"/>.</para>
    /// </remarks>
    public enum BuildCombineMethod
    {
        /// <summary>
        /// Do not combine meshes.
        /// </summary>
        None,

        /// <summary>
        /// Combine meshes on a per chunk basis.
        /// </summary>
        ByChunk,

        /// <summary>
        /// Combine meshes on a per tile system basis.
        /// </summary>
        ByTileSystem,

        /// <summary>
        /// Combine meshes on custom chunk size. This essentially allows you to work with
        /// a different chunk size when tile meshes are combined.
        /// </summary>
        CustomChunkInTiles,
    }
}
