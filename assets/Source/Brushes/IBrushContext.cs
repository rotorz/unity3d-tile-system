// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Context of tile that is being painted using a brush.
    /// </summary>
    public interface IBrushContext
    {
        /// <summary>
        /// Gets active tile system.
        /// </summary>
        TileSystem TileSystem { get; }

        /// <summary>
        /// Gets zero-based row index of tile.
        /// </summary>
        int Row { get; }
        /// <summary>
        /// Gets zero-based column index of tile.
        /// </summary>
        int Column { get; }

        /// <summary>
        /// Gets brush that is currently being used.
        /// </summary>
        Brush Brush { get; }
    }
}
