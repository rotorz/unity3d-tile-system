// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Direction that tiles will face upon being painted.
    /// </summary>
    public enum TileFacing
    {
        /// <summary>
        /// Tiles face sideways which is good for side-scrolling games like platformers.
        /// </summary>
        /// <remarks>
        /// <para>Since Rotorz Tile System version 2.2.2 the default behavior for tile
        /// systems with sideways facing tiles has been changed. Though brushes which
        /// were created using the legacy behavior will continue to work like previously
        /// since <see cref="Brush.forceLegacySideways"/> is automatically set when
        /// updating this extension.</para>
        /// </remarks>
        Sideways = 0,

        /// <summary>
        /// Tiles face upwards which is better for top-down games.
        /// </summary>
        Upwards = 1,
    }
}
