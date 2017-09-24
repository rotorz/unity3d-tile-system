// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile
{
    /// <summary>
    /// Tile orientation utility functions.
    /// </summary>
    public static class OrientationUtility
    {
        /// <summary>
        /// Count the number of strong connections between two orientations.
        /// </summary>
        /// <remarks>
        /// <para>The number of strong connections between two orientations is useful
        /// when searching for the orientation that best matches the actual orientation
        /// of a tile.</para>
        /// <para>This function is commutative since ordering of operands is not important.</para>
        /// </remarks>
        /// <param name="a">Bitmask of first orientation.</param>
        /// <param name="b">Bitmask of second orientation.</param>
        /// <returns>
        /// The number of strong connections between specified orientations. A result of 4
        /// indicates the maximum number of strong connections.
        /// </returns>
        public static int CountStrongConnections(int a, int b)
        {
            int count = 0;

            if ((a & (1 << 1)) == (b & (1 << 1))) {
                ++count;
            }
            if ((a & (1 << 3)) == (b & (1 << 3))) {
                ++count;
            }
            if ((a & (1 << 4)) == (b & (1 << 4))) {
                ++count;
            }
            if ((a & (1 << 6)) == (b & (1 << 6))) {
                ++count;
            }

            return count;
        }

        /// <summary>
        /// Count the number of weak connections between two orientations.
        /// </summary>
        /// <remarks>
        /// <para>The number of weak connections between two orientations is useful
        /// when searching for the orientation that best matches the actual orientation
        /// of a tile.</para>
        /// <para>This function is commutative since ordering of operands is not important.</para>
        /// </remarks>
        /// <param name="a">Bitmask of first orientation.</param>
        /// <param name="b">Bitmask of second orientation.</param>
        /// <returns>
        /// The number of weak connections between specified orientations. A result of 4
        /// indicates the maximum number of weak connections.
        /// </returns>
        public static int CountWeakConnections(int a, int b)
        {
            int count = 0;

            if ((a & (1 << 0)) == (b & (1 << 0))) {
                ++count;
            }
            if ((a & (1 << 2)) == (b & (1 << 2))) {
                ++count;
            }
            if ((a & (1 << 5)) == (b & (1 << 5))) {
                ++count;
            }
            if ((a & (1 << 7)) == (b & (1 << 7))) {
                ++count;
            }

            return count;
        }

        /// <summary>
        /// Character buffer for efficiently generating orientation name strings.
        /// </summary>
        private static char[] s_Buffer = new char[8];

        /// <summary>
        /// Gets name of orientation from bitmask.
        /// </summary>
        /// <remarks>
        /// <para>Name of orientation is represented using ones and zeros where one
        /// represents the presence of neighbouring tiles and zero represents the absence
        /// of neighbouring tiles.</para>
        /// <list type="bullet">
        ///    <item>Leftmost digit represents tile to north-west of context tile.</item>
        ///    <item>Rightmost digit represents tile to south-east of context tile.</item>
        ///    <item>Context tile is not included in name because it is always present.</item>
        /// </list>
        /// <para>The following image illustrates this:</para>
        /// <para><img src="../art/orientation-name.png" alt="Ordering of digits in orientation name."/></para>
        /// </remarks>
        /// <param name="mask">Bitmask of orientation.</param>
        /// <returns>
        /// Name of orientation.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="mask"/> is not a valid orientation.
        /// </exception>
        public static string NameFromMask(int mask)
        {
            if (mask < 0 || mask > 0xFF) {
                throw new ArgumentOutOfRangeException("Invalid mask was specified.");
            }

            for (int i = 0; i < 8; ++i) {
                s_Buffer[ i ] = (mask & (1 << i)) != 0 ? '1' : '0';
            }

            return new string(s_Buffer);
        }

        /// <summary>
        /// Gets bitmask representation of orientation from orientation name.
        /// </summary>
        /// <remarks>
        /// <para>An orientation is represented using a bitmask where the first bit
        /// represents the upper-left neighbouring tile and the eighth bit represents
        /// the lower-right neighbouring tile.</para>
        /// <para>The following image illustrates this:</para>
        /// <para><img src="../art/orientation-mask.png" alt="Format of mask representation of orientation."/></para>
        /// </remarks>
        /// <param name="name">Name of orientation.</param>
        /// <returns>
        /// Bitmask representation of orientation.
        /// </returns>
        /// <exception cref="System.NullReferenceException">
        /// If <paramref name="name"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If input orientation name containers fewer than 8 characters.
        /// </exception>
        public static int MaskFromName(string name)
        {
            int mask = 0;
            for (int i = 0; i < 8; ++i) {
                if (name[i] == '1') {
                    mask |= 1 << i;
                }
            }
            return mask;
        }

        /// <summary>
        /// Determines orientation of the specified tile based upon the tiles which surround it.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        /// <param name="brush">Brush to consider orientation of.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// Bitmask representing orientation of tile.
        /// </returns>
        public static int DetermineTileOrientation(TileSystem system, int row, int column, Brush brush, int rotation = 0)
        {
            // Is orientation obvious?
            var coalescableBrush = brush as ICoalescableBrush;
            if (coalescableBrush == null || coalescableBrush.Coalesce == Coalesce.None) {
                return 0;//"00000000";
            }

            int orientation = 0, i = 0;

            // Take local copy of property values.
            Coalesce coalesce = coalescableBrush.Coalesce;
            bool coalesceWithRotated = coalescableBrush.CoalesceWithRotated;

            // Determine orientation of specified tile by analysing surrounding tiles.
            for (int ir = row - 1; ir <= row + 1; ++ir) {
                // Skip non-existent row!
                if (ir < 0 || ir >= system.RowCount) {
                    //orientation += "000";
                    i += 3;
                    continue;
                }

                for (int ic = column - 1; ic <= column + 1; ++ic) {
                    // Skip non-existent column!
                    if (ic < 0 || ic >= system.ColumnCount) {
                        //orientation += "0";
                        ++i;
                        continue;
                    }

                    // Skip targetted tile!
                    if (ir == row && ic == column) {
                        continue;
                    }

                    // Fetch tile from system.
                    TileData tile = system.GetTile(ir, ic);
                    if (tile != null && tile.brush != null && (rotation == tile.PaintedRotation || coalesceWithRotated)) {
                        switch (coalesce) {
                            case Coalesce.Own:
                                if (tile.brush == brush) {
                                    orientation |= 1 << i;
                                }
                                break;

                            case Coalesce.Other:
                                if (tile.brush != brush) {
                                    orientation |= 1 << i;
                                }
                                break;

                            case Coalesce.Any:
                                orientation |= 1 << i;
                                break;

                            case Coalesce.Groups:
                                if (coalescableBrush.CoalesceWithBrushGroups.Contains(tile.brush.group)) {
                                    orientation |= 1 << i;
                                }
                                break;

                            case Coalesce.OwnAndGroups:
                                if (tile.brush == brush || coalescableBrush.CoalesceWithBrushGroups.Contains(tile.brush.group)) {
                                    orientation |= 1 << i;
                                }
                                break;
                        }
                    }

                    //orientation += "0";
                    ++i;
                }
            }

            if (coalescableBrush.CoalesceWithBorder) {
                orientation = CoalesceOrientationWithBorder(system, row, column, orientation);
            }

            // Counteract base rotation transform to orientation?
            return OrientationUtility.RotateAntiClockwise(orientation, rotation);
        }

        /// <inheritdoc cref="DetermineTileOrientation(TileSystem, int, int, Brush, int)"/>
        /// <param name="index">Index of tile.</param>
        public static int DetermineTileOrientation(TileSystem system, TileIndex index, Brush brush, int rotation = 0)
        {
            return DetermineTileOrientation(system, index.row, index.column, brush, rotation);
        }

        private static int CoalesceOrientationWithBorder(TileSystem system, int row, int column, int orientation)
        {
            int endRow = system.RowCount - 1;
            int endColumn = system.ColumnCount - 1;

            // Don't bother proceeding if not adjacent to border of tile system.
            if (row != 0 & column != 0 & row != endRow & column != endColumn) {
                return orientation;
            }

            if (row == 0) {
                orientation |= 1 << 1;
            }
            if (column == 0) {
                orientation |= 1 << 3;
            }
            if (row == endRow) {
                orientation |= 1 << 6;
            }
            if (column == endColumn) {
                orientation |= 1 << 4;
            }

            if (row == 0 && column == 0) {
                orientation |= 1 << 0;
            }
            if (row == 0 && column == endColumn) {
                orientation |= 1 << 2;
            }
            if (row == endRow && column == 0) {
                orientation |= 1 << 5;
            }
            if (row == endRow && column == endColumn) {
                orientation |= 1 << 7;
            }

            bool mask_00010000 = (orientation & (1 << 3)) != 0;
            bool mask_00001000 = (orientation & (1 << 4)) != 0;
            bool mask_01000000 = (orientation & (1 << 1)) != 0;
            bool mask_00000010 = (orientation & (1 << 6)) != 0;

            if (row == 0) {
                if (mask_00010000) {
                    orientation |= 1 << 0;
                }
                if (mask_00001000) {
                    orientation |= 1 << 2;
                }
            }
            if (column == 0) {
                if (mask_01000000) {
                    orientation |= 1 << 0;
                }
                if (mask_00000010) {
                    orientation |= 1 << 5;
                }
            }
            if (row == endRow) {
                if (mask_00010000) {
                    orientation |= 1 << 5;
                }
                if (mask_00001000) {
                    orientation |= 1 << 7;
                }
            }
            if (column == endColumn) {
                if (mask_01000000) {
                    orientation |= 1 << 2;
                }
                if (mask_00000010) {
                    orientation |= 1 << 7;
                }
            }

            return orientation;
        }


        #region Rotational Symmetry

        /// <summary>
        /// Rotate orientation clockwise by 90 degrees.
        /// </summary>
        /// <param name="orientation">Bitmask of input orientation.</param>
        /// <returns>
        /// Bitmask of rotated orientation.
        /// </returns>
        private static int DoRotateClockwise(int orientation)
        {
            int rotatedOrientation = 0;

            if ((orientation & (1 << 0)) != 0) {
                rotatedOrientation |= 1 << 2;
            }
            if ((orientation & (1 << 1)) != 0) {
                rotatedOrientation |= 1 << 4;
            }
            if ((orientation & (1 << 2)) != 0) {
                rotatedOrientation |= 1 << 7;
            }

            if ((orientation & (1 << 3)) != 0) {
                rotatedOrientation |= 1 << 1;
            }
            if ((orientation & (1 << 4)) != 0) {
                rotatedOrientation |= 1 << 6;
            }

            if ((orientation & (1 << 5)) != 0) {
                rotatedOrientation |= 1 << 0;
            }
            if ((orientation & (1 << 6)) != 0) {
                rotatedOrientation |= 1 << 3;
            }
            if ((orientation & (1 << 7)) != 0) {
                rotatedOrientation |= 1 << 5;
            }

            return rotatedOrientation;
        }

        /// <summary>
        /// Rotate orientation anti-clockwise by 90 degrees.
        /// </summary>
        /// <param name="orientation">Bitmask of input orientation.</param>
        /// <returns>
        /// Bitmask of rotated orientation.
        /// </returns>
        private static int DoRotateAntiClockwise(int orientation)
        {
            int rotatedOrientation = 0;

            if ((orientation & (1 << 0)) != 0) {
                rotatedOrientation |= 1 << 5;
            }
            if ((orientation & (1 << 1)) != 0) {
                rotatedOrientation |= 1 << 3;
            }
            if ((orientation & (1 << 2)) != 0) {
                rotatedOrientation |= 1 << 0;
            }

            if ((orientation & (1 << 3)) != 0) {
                rotatedOrientation |= 1 << 6;
            }
            if ((orientation & (1 << 4)) != 0) {
                rotatedOrientation |= 1 << 1;
            }

            if ((orientation & (1 << 5)) != 0) {
                rotatedOrientation |= 1 << 7;
            }
            if ((orientation & (1 << 6)) != 0) {
                rotatedOrientation |= 1 << 4;
            }
            if ((orientation & (1 << 7)) != 0) {
                rotatedOrientation |= 1 << 2;
            }

            return rotatedOrientation;
        }

        private static readonly int[][] s_RotationMap = {
            new int[] { 1 << 2, 1 << 4, 1 << 7, 1 << 1, 1 << 6, 1 << 0, 1 << 3, 1 << 5 },
            new int[] { 1 << 7, 1 << 6, 1 << 5, 1 << 4, 1 << 3, 1 << 2, 1 << 1, 1 << 0 },
            new int[] { 1 << 5, 1 << 3, 1 << 0, 1 << 6, 1 << 1, 1 << 7, 1 << 4, 1 << 2 }
        };

        private static int RemapOrientationMask(int[] map, int orientation, int rotation)
        {
            int rotatedOrientation = 0;
            int mask = 1;

            for (int i = 0; i < map.Length; ++i) {
                if ((orientation & mask) != 0) {
                    rotatedOrientation |= map[i];
                }
                mask <<= 1;
            }

            return rotatedOrientation;
        }

        /// <summary>
        /// Rotate orientation clockwise in increments of 90.
        /// </summary>
        /// <remarks>
        /// <para>No rotation is applied if rotation index is out of bounds.</para>
        /// </remarks>
        /// <param name="orientation">Bitmask of orientation.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// Bitmask of rotated orientation.
        /// </returns>
        /// <seealso cref="RotateAntiClockwise(int, int)"/>
        public static int RotateClockwise(int orientation, int rotation = 1)
        {
            switch (rotation) {
                case 1:
                    return DoRotateClockwise(orientation);

                case 2:
                case 3:
                    return RemapOrientationMask(s_RotationMap[--rotation], orientation, rotation);

                default:
                    return orientation;
            }
        }

        /// <summary>
        /// Rotate orientation anti-clockwise in increments of 90.
        /// </summary>
        /// <remarks>
        /// <para>No rotation is applied if rotation index is out of bounds.</para>
        /// </remarks>
        /// <param name="orientation">Bitmask of orientation.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// Bitmask of rotated orientation.
        /// </returns>
        /// <seealso cref="RotateClockwise(int, int)"/>
        public static int RotateAntiClockwise(int orientation, int rotation = 1)
        {
            switch (rotation) {
                case 1:
                    return DoRotateAntiClockwise(orientation);

                case 2:
                case 3:
                    return RemapOrientationMask(s_RotationMap[2 - --rotation], orientation, rotation);

                default:
                    return orientation;
            }
        }

        /// <summary>
        /// Determines whether two orientations share rotational symmetry.
        /// </summary>
        /// <param name="a">Bitmask of first orientation.</param>
        /// <param name="b">Bitmask of second orientation.</param>
        /// <returns>
        /// A value of <c>true</c> if input orientations share rotational symmetry;
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool HasRotationalSymmetry(int a, int b)
        {
            if (a == b) {
                return true;
            }

            b = DoRotateClockwise(b);
            if (a == b) {
                return true;
            }

            b = DoRotateClockwise(b);
            return a == b || a == DoRotateClockwise(b);
        }

        /// <summary>
        /// Determines mask of first orientation in group with rotational symmetry.
        /// </summary>
        /// <param name="mask">Bitmask of orientation.</param>
        /// <returns>
        /// Bitmask of first orientation when sorted by rotational symmetry.
        /// </returns>
        public static int FirstMaskWithRotationalSymmetry(int mask)
        {
            int firstMask = mask;

            int nextMask = mask;
            for (int i = 0; i < 3; ++i) {
                nextMask = DoRotateClockwise(nextMask);
                if (nextMask > firstMask) {
                    firstMask = nextMask;
                }
            }

            return firstMask;
        }

        // Temporary variable used by `GetMasksWithRotationalSymmetry`.
        private static int[] s_Temp = new int[4];

        /// <summary>
        /// Get array of orientation masks which share rotational symmetry.
        /// </summary>
        /// <param name="mask">Bitmask of orientation.</param>
        /// <returns>
        /// An array of 1 or more orientation bitmasks.
        /// </returns>
        public static int[] GetMasksWithRotationalSymmetry(int mask)
        {
            mask = FirstMaskWithRotationalSymmetry(mask);
            s_Temp[0] = mask;

            int count = 1;

            // Rotate orientation by 90 degrees 3 times to find unique orientations
            // which share rotational symmetry.
            for (int i = 1; i < 4; ++i) {
                mask = DoRotateClockwise(mask);

                // Is rotated mask unique?
                bool unique = true;
                for (int j = 0; j < count; ++j)
                    if (s_Temp[j] == mask) {
                        unique = false;
                        break;
                    }

                if (unique) {
                    s_Temp[count++] = mask;
                }
            }

            int[] result = new int[count];
            for (int i = 0; i < count; ++i) {
                result[i] = s_Temp[i];
            }

            return result;
        }

        #endregion
    }
}
