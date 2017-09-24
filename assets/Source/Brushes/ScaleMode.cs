// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Indicates the way in which a painted tile is scaled.
    /// </summary>
    public enum ScaleMode
    {
        /// <summary>
        /// Leave tile alone! Maintain scale of painted prefabs.
        /// </summary>
        DontTouch,

        /// <summary>
        /// Scale painted prefabs by cell size.
        /// </summary>
        UseCellSize,

        /// <summary>
        /// Apply custom scale to painted prefabs.
        /// </summary>
        Custom,
    }
}
