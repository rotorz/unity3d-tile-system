// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Internal
{
    public static class StripFlag
    {
        /// <summary>
        /// Bit flag indicating that tile system component should be stripped.
        /// </summary>
        public const int STRIP_TILE_SYSTEM = 0x0001;
        /// <summary>
        /// Bit flag indicating that chunk map should be stripped.
        /// </summary>
        public const int STRIP_CHUNK_MAP = 0x0002;
        /// <summary>
        /// Bit flag indicating that tile data should be stripped.
        /// </summary>
        public const int STRIP_TILE_DATA = 0x0004;
        /// <summary>
        /// Bit flag indicating that brush references should be stripped.
        /// </summary>
        public const int STRIP_BRUSH_REFS = 0x0008;
        /// <summary>
        /// Bit flag indicating that empty game objects should be stripped.
        /// </summary>
        public const int STRIP_EMPTY_OBJECTS = 0x0010;
        /// <summary>
        /// Bit flag indicating that empty chunks should be stripped.
        /// </summary>
        public const int STRIP_EMPTY_CHUNKS = 0x0020;
        /// <summary>
        /// Bit flag indicating that chunk game objects should be stripped.
        /// </summary>
        public const int STRIP_CHUNKS = 0x0040;
        /// <summary>
        /// Bit flag indicating that empty game objects leftover after tile meshes have
        /// been combined should be stripped.
        /// </summary>
        public const int STRIP_COMBINED_EMPTY = 0x0080;
        /// <summary>
        /// Bit flag indicating that plop components should be stripped.
        /// </summary>
        public const int STRIP_PLOP_COMPONENTS = 0x0100;
    }
}
