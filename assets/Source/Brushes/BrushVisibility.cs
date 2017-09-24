// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// The visibility of a brush.
    /// </summary>
    /// <seealso cref="Brush.visibility"/>
    public enum BrushVisibility
    {
        /// <summary>
        /// Brush is hidden.
        /// <para>Brushes that are not intended for direct use when designing levels can
        /// be hidden from brush lists by marking them as hidden.</para>
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// Brush is shown.
        /// <para>Brush should be shown in brush lists unless otherwise filtered.</para>
        /// </summary>
        Shown,

        /// <summary>
        /// Brush is shown and marked as a favorite.
        /// <para>Brush list controls provide the option to show or hide tileset brushes.
        /// Sometimes it is useful to hide all tileset brushes with the exception of those
        /// that have been favorited.</para>
        /// <para>Brush should be shown in brush lists unless otherwise filtered.</para>
        /// </summary>
        Favorite,
    }
}
