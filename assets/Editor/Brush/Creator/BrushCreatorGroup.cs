// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Identifies a brush creator group.
    /// </summary>
    public enum BrushCreatorGroup
    {
        /// <summary>
        /// The default group.
        /// </summary>
        Default = 0,

        /// <summary>
        /// For <see cref="BrushCreator"/>'s that somehow replicate existing assets.
        /// </summary>
        Duplication,
    }
}
