// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Specifies how brush list should be zoomed.
    /// </summary>
    public enum BrushListZoomMode
    {
        /// <summary>
        /// Use tile size when a tileset is active, otherwise adjust zoom to make use of
        /// the available space.
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Adjust zoom to make use of the available space.
        /// </summary>
        BestFit,

        /// <summary>
        /// Displays slider allowing user to manually adjust zoom.
        /// </summary>
        Custom,
    }
}
