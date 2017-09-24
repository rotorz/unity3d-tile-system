// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;

namespace Rotorz.Tile
{
    /// <summary>
    /// Interface for a brush which has support for coalescing rules.
    /// </summary>
    public interface ICoalescableBrush
    {
        /// <summary>
        /// Gets or sets the coalescing rule that defines how painted tiles orientate
        /// with one another.
        /// </summary>
        /// <remarks>
        /// <para>Tiles that are painted using oriented or autotile brushes may vary
        /// according to their orientation which is defined by neighbouring tiles. This
        /// property provides a small degree of control over the way in which orientations
        /// are selected as tiles are painted.</para>
        /// <para>The number of the group to coalesce against must be specified when
        /// this property is set to <see cref="Rotorz.Tile.Coalesce.Groups">Coalesce.Group</see>.
        /// See <see cref="CoalesceWithBrushGroups"/>.</para>
        /// <para>Tiles that are painted using non-oriented brushes can also influence
        /// the selection of their neighbouring tiles when using coalescing groups.</para>
        /// </remarks>
        /// <seealso cref="Brush.group"/>
        /// <seealso cref="CoalesceWithBrushGroups"/>
        Coalesce Coalesce { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether painted tiles can coalesce with
        /// tiles which have been painted with a different rotation.
        /// </summary>
        bool CoalesceWithRotated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether painted tiles can coalesce with
        /// border of tile system.
        /// </summary>
        bool CoalesceWithBorder { get; set; }

        /// <summary>
        /// Gets editable collection of brush groups that painted tiles can coalesce with.
        /// </summary>
        /// <remarks>
        /// <para>Only applicable when <see cref="Coalesce"/> is set to
        /// <see cref="Rotorz.Tile.Coalesce.Groups">Coalesce.Group</see> or
        /// <see cref="Rotorz.Tile.Coalesce.OwnAndGroups">Coalesce.OwnAndGroup</see>.</para>
        /// </remarks>
        /// <seealso cref="Brush.group"/>
        /// <seealso cref="Coalesce"/>
        ICollection<int> CoalesceWithBrushGroups { get; }
    }
}
