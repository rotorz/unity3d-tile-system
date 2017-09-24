// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;

namespace Rotorz.Tile
{
    /// <summary>
    /// A selection of arguments which can be used to influence the way in which tiles are
    /// painted when using the <see cref="PaintingUtility"/> class.
    /// </summary>
    /// <seealso cref="PaintingUtility"/>
    /// <seealso cref="PaintingArgs.GetDefaults(Brush)"/>
    public struct PaintingArgs
    {
        #region Default Arguments

        /// <summary>
        /// Default painting arguments.
        /// </summary>
        private static readonly PaintingArgs s_Defaults = new PaintingArgs() {
            fillRatePercentage = 100
        };


        /// <summary>
        /// Get default painting arguments.
        /// </summary>
        /// <returns>
        /// <see cref="PaintingArgs"/> is a structure and so is always copied by value.
        /// </returns>
        public static PaintingArgs GetDefaults()
        {
            return s_Defaults;
        }

        /// <summary>
        /// Get default painting arguments.
        /// </summary>
        /// <param name="brush">Initially selected brush.</param>
        /// <returns>
        /// <see cref="PaintingArgs"/> is a structure and so is always copied by value.
        /// </returns>
        public static PaintingArgs GetDefaults(Brush brush)
        {
            var defaults = s_Defaults;
            defaults.brush = brush;
            return defaults;
        }

        #endregion


        /// <summary>
        /// Brush to use when painting tiles. Specify a value of <c>null</c> to erase
        /// tiles instead of painting them.
        /// </summary>
        public Brush brush;

        /// <summary>
        /// Zero-based index of desired tile variation or a value of <see cref="Brush.RANDOM_VARIATION"/>
        /// to randomize tile variations when painting.
        /// </summary>
        public int variation;

        /// <summary>
        /// Count of variations to shift by.
        /// </summary>
        public int variationShiftCount;

        /// <summary>
        /// Rotation of painted tiles.
        /// </summary>
        /// <value>
        /// <para>Zero-based index of simple rotation (0 to 3 inclusive):</para>
        /// <list type="bullet">
        /// <item>0 = 0°</item>
        /// <item>1 = 90°</item>
        /// <item>2 = 180°</item>
        /// <item>3 = 270°</item>
        /// </list>
        /// </value>
        public int rotation;

        /// <summary>
        /// A value ranging between 0 and 100 (inclusive) which indicates the rate
        /// in which candidate tiles are filled. Specify a value lower than 100 to
        /// spray tiles onto a tile system.
        /// </summary>
        public int fillRatePercentage;

        /// <summary>
        /// Indicates whether rotation should be randomized when painting tiles.
        /// </summary>
        public bool randomizeRotation;

        /// <summary>
        /// Indicates whether new tiles should be painted around existing tiles
        /// rather than painting over them. This of course does not apply when
        /// erasing tiles.
        /// </summary>
        public bool paintAroundExistingTiles;

        /// <summary>
        /// Resolve variation index by applying shift.
        /// </summary>
        /// <param name="orientationMask">Bitmask that identifies orientation of target tile.</param>
        /// <returns>
        /// Zero-based index of resolved variation.
        /// </returns>
        public int ResolveVariation(int orientationMask)
        {
            if (this.brush == null) {
                return 0;
            }

            int variationIndex = this.variation;

            // Apply randomization up-front rather than relying upon brush to do this.
            if (variationIndex == Brush.RANDOM_VARIATION) {
                variationIndex = this.brush.PickRandomVariationIndex(orientationMask);
            }
            else {
                // Apply shift to variation?
                if (this.variationShiftCount != 0) {
                    int variationCount = this.brush.CountTileVariations(orientationMask);
                    variationIndex = MathUtility.Mod(variationIndex + this.variationShiftCount, variationCount);
                }
            }

            return variationIndex;
        }
    }
}
