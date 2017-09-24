// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Action to take for broken and/or dirty tiles.
    /// </summary>
    public enum RepairAction
    {
        /// <summary>
        /// Count the number of broken and dirty tiles.
        /// </summary>
        JustCount,

        /// <summary>
        /// Erase broken tiles.
        /// </summary>
        Erase,

        /// <summary>
        /// Force refresh broken and dirty tiles.
        /// </summary>
        ForceRefresh,

        /// <summary>
        /// Force refresh dirty tiles.
        /// </summary>
        RefreshDirty,
    }
}
