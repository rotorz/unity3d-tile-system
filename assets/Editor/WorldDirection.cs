// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// A direction in world space.
    /// </summary>
    public enum WorldDirection
    {
        /// <summary>
        /// Upwards in world space (useful for top-down).
        /// </summary>
        Up,

        /// <summary>
        /// Downwards in world space (useful for bottom-up).
        /// </summary>
        Down,

        /// <summary>
        /// Leftwards in world space.
        /// </summary>
        Left,

        /// <summary>
        /// Rightwards in world space.
        /// </summary>
        Right,

        /// <summary>
        /// Forwards in world space (useful for platformer).
        /// </summary>
        Forward,

        /// <summary>
        /// Backwards in world space.
        /// </summary>
        Backward,
    }
}
