// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// The built-in keys of properties that can be shared across <see cref="BrushCreator"/>
    /// multiple instances.
    /// </summary>
    public static class BrushCreatorSharedPropertyKeys
    {
        /// <summary>
        /// The name of the brush that is being created.
        /// </summary>
        /// <remarks>
        /// <para>Value type should be a <see langword="string"/>.</para>
        /// </remarks>
        public const string BrushName = "[BrushName]";

        /// <summary>
        /// The brush that is being targetted for duplication or aliasing.
        /// </summary>
        /// <remarks>
        /// <para>Value type should be a <see cref="Brush"/>.</para>
        /// </remarks>
        public const string TargetBrush = "[TargetBrush]";

        /// <summary>
        /// The name of the tileset that is being created.
        /// </summary>
        /// <remarks>
        /// <para>Value type should be a <see langword="string"/>.</para>
        /// </remarks>
        public const string TilesetName = "[TilesetName]";
    }
}
