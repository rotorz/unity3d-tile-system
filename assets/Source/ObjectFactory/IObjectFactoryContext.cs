// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Interface to describe the context in which an object is being instantiated or
    /// destroyed.
    /// </summary>
    public interface IObjectFactoryContext
    {
        /// <summary>
        /// Gets the associated tile system.
        /// </summary>
        TileSystem TileSystem { get; }

        /// <summary>
        /// Gets data for tile that object is being created for.
        /// </summary>
        TileData Tile { get; }

        /// <summary>
        /// Gets brush that is being used to create object.
        /// </summary>
        Brush Brush { get; }
    }
}
