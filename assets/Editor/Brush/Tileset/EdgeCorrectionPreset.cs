// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Preset to correct bleeding at edges of atlas tiles.
    /// </summary>
    internal enum EdgeCorrectionPreset
    {
        /// <summary>
        /// Do not attempt edge correction.
        /// </summary>
        /// <remarks>
        /// <para>Visual artifacts may present themselves between tiles as
        /// lines that flicker with movement.</para>
        /// <para>This is less of an issue for alpha blended tiles (usually
        /// interactive objects) where the artwork includes a transparent border.</para>
        /// <para>This preset is the equivalent of no tile border and no delta
        /// offset.</para>
        /// </remarks>
        DoNothing,

        /// <summary>
        /// Inset UVs by half a pixel to improve issue.
        /// </summary>
        /// <remarks>
        /// <para>Pixels at edges of tiles will appear to be half the size of
        /// inner pixels when viewed closely. This side-effect is usually less
        /// noticable at a distance and often not noticable when tiles are
        /// rendered at actual pixel size (1 pixel of tile = 1 pixel of output).</para>
        /// <para>This preset is the equivalent of no tile border and 0.5 delta
        /// offset.</para>
        /// </remarks>
        InsetUVs,

        /// <summary>
        /// Specify custom border and/or delta correction.
        /// </summary>
        /// <remarks>
        /// <para>Adding a border around each tile by repeating tile edges
        /// as intended will usually provide a better outcome. Depending upon
        /// the artwork it is sometimes beneficial to duplicate pixels from
        /// the expected adjacent tile.</para>
        /// <para>Custom border and delta offset must be specified.</para>
        /// </remarks>
        Custom,
    }
}
