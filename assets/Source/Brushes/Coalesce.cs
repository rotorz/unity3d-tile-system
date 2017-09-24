// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Indicates a method of coalescing which are used by brushes that support orientation
    /// including oriented brushes and autotile brushes.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Coalesce-Mode">Coalesce Mode</a>
    /// section of user guide for further information and illustrations.</para>
    /// </intro>
    public enum Coalesce
    {
        /// <summary>
        /// Do not attempt to join adjacent tiles.
        /// </summary>
        None,

        /// <summary>
        /// Only attempt to join adjacent tiles of same type.
        /// </summary>
        Own,

        /// <summary>
        /// Do not join adjacent tiles of own type, but join with any other.
        /// </summary>
        Other,

        /// <summary>
        /// Join with adjacent tiles of own type and other type.
        /// </summary>
        Any,

        /// <summary>
        /// Join with tiles of zero or more brush groups.
        /// </summary>
        Groups,

        /// <summary>
        /// Join with adjacent tiles of same type or of zero or more brush groups.
        /// </summary>
        OwnAndGroups,
    }
}
