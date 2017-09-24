// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Specifies how to handle undefined orientations.
    /// </summary>
    public enum FallbackMode
    {
        /// <summary>
        /// Attempt to find next best available orientation before assuming the default
        /// orientation.
        /// </summary>
        NextBest,

        /// <summary>
        /// Use default orientation when no exact orientation is found. Diagonal
        /// connections are gracefully ignored.
        /// </summary>
        UseDefault,

        /// <summary>
        /// Use default orientation when no exact orientation is found.
        /// </summary>
        UseDefaultStrict,
    }
}
