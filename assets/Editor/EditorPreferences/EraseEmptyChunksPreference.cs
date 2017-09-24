// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    internal enum EraseEmptyChunksPreference
    {
        /// <summary>
        /// Empty chunks should be erased when painting (default).
        /// </summary>
        Yes = 0,

        /// <summary>
        /// Empty chunks should not be erased when painting.
        /// </summary>
        No = 1,

        /// <summary>
        /// Use per tile system preference.
        /// </summary>
        PerTileSystem = 2,
    }
}
