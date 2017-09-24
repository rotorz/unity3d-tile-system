// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Extension methods for the <see cref="ICoalescableBrush"/> interface.
    /// </summary>
    public static class CoalescableBrushExtensions
    {
        /// <summary>
        /// Determines whether <see cref="ICoalescableBrush.CoalesceWithBrushGroups"/> is
        /// applicable for coalesable brush.
        /// </summary>
        /// <param name="coalescableBrush">Coalesable brush.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating whether <see cref="ICoalescableBrush.CoalesceWithBrushGroups"/>
        /// is being used for coalesable brush.
        /// </returns>
        public static bool IsUsingCoalesceWithBrushGroups(this ICoalescableBrush coalescableBrush)
        {
            Coalesce coalesce = coalescableBrush.Coalesce;
            return coalesce == Coalesce.Groups || coalesce == Coalesce.OwnAndGroups;
        }
    }
}
