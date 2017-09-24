// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// The type of list view.
    /// </summary>
    [Flags]
    public enum BrushListView
    {
        /// <summary>
        /// Indicates that all brushes should be shown.
        /// </summary>
        Brushes = 0x01,

        /// <summary>
        /// Indicates that tilesets should be shown.
        /// </summary>
        Tileset = 0x02,

        /// <summary>
        /// Indicates that all master brushes should be shown.
        /// </summary>
        Master = 0x04,
    }
}
