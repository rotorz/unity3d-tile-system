// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile
{
    /// <summary>
    /// Bit mask that defines how tiles should be refreshed.
    /// </summary>
    [Flags]
    public enum RefreshFlags
    {
        /// <summary>
        /// Does not represent any flag at all.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Tiles should each be force refreshed.
        /// </summary>
        /// <remarks>
        /// <para>Tiles are re-painted so that changes that have been made to the
        /// associated brush are reflected. Painted flags and tile transform tweaks can
        /// usually be preserved by specifying other refresh flags.</para>
        /// </remarks>
        /// <seealso cref="RefreshFlags.PreservePaintedFlags"/>
        /// <seealso cref="RefreshFlags.PreserveTransform"/>
        Force = 0x01,

        /// <summary>
        /// Procedural tiles are to be refreshed from tile data.
        /// </summary>
        /// <remarks>
        /// <para>Procedural meshes are updated to reflect changes that have been made to
        /// brushes.</para>
        /// </remarks>
        UpdateProcedural = 0x02,

        /// <summary>
        /// User flags of painted tiles should be preserved.
        /// </summary>
        /// <remarks>
        /// <para>It is possible for custom editor scripts to adjust the state of custom
        /// user flags on a per tile basis for a variety of reasons. Custom tools might
        /// even utilise custom user flags. When this is true it may be beneficial to
        /// preserve the flags of painted tiles.</para>
        /// <para>Changes that are made to the initial state of custom user flags (using
        /// the brush designer) will not be reflected unless the flags of painted tiles
        /// are also refreshed.</para>
        /// </remarks>
        PreservePaintedFlags = 0x04,

        /// <summary>
        /// Manual offsets of position, rotation and scale should be preserved.
        /// </summary>
        /// <remarks>
        /// <para>Users are able to manually tweak the position, rotation and scale of
        /// tiles that have been painted. This can usually be preserved using this flag.</para>
        /// </remarks>
        PreserveTransform = 0x08,
    }
}
