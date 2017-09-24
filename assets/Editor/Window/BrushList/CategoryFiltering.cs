// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Type of category filtering.
    /// </summary>
    public enum CategoryFiltering
    {
        /// <summary>
        /// Brushes should not be filtered by category.
        /// </summary>
        None = 0,

        /// <summary>
        /// Display all brushes which reside within category selection.
        /// </summary>
        Selection,

        /// <summary>
        /// Display all brushes which reside within custom category selection.
        /// </summary>
        CustomSelection,
    }
}
