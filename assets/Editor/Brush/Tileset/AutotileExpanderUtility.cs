// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile
{
    internal sealed class AutotileExpanderUtility
    {
        #region Mapping Data

        /// Edge subtiles that must be clamped/repeated.
        private const byte XA = 244;
        private const byte XB = 245;
        private const byte XC = 246;
        private const byte XD = 247;
        private const byte XE = 248;
        private const byte XF = 249;
        private const byte XG = 250;
        private const byte XH = 251;

        private const byte X1 = 252;
        private const byte X2 = 253;
        private const byte X3 = 254;
        private const byte X4 = 255;

        /*
        /// <summary>
        /// Collection of 16 orientations.
        /// </summary>
        /// <remarks>
        /// <para>Each orientation defines a 4x4 block of subtiles where the inner 4 tiles
        /// represent the actual tile, and the surrounding tiles make it possible to add
        /// an additional border to help reduce visual glitches.</para>
        /// </remarks>
        private static readonly byte[][] s_TIDE_STYLE = {
            new byte[] {  9,  8,  9,  8,   1,  0,  1,  0,   9,  8,  9,  8,   1,  0,  1,  0 },
            new byte[] {  9,  8,  9,  8,   3,  2,  3,  2,  11, 10, 11, 10,  17, 16, 17, 16 },
            new byte[] {  9, 12, 13, 24,   1,  4,  5, 16,   9, 12, 13, 24,   1,  4,  5, 16 },
            new byte[] { 25, 14, 15,  8,  17,  6,  7,  0,  25, 14, 15,  8,  17,  6,  7,  0 },
            new byte[] { 25, 24, 25, 24,  17, 16, 17, 16,  25, 24, 25, 24,  17, 16, 17, 16 },
            new byte[] { 25, 24, 25, 24,  19, 18, 19, 18,  27, 26, 27, 26,   1,  0,  1,  0 },
            new byte[] {  9, 12, 13, 24,   5, 20, 21, 18,  13, 28, 29, 26,  17,  6,  7,  0 },

            new byte[] { 25, 14, 15,  8,  19, 22, 23,  2,  27, 30, 31, 10,   1,  4,  5, 16 },
            new byte[] {  9,  8,  9,  8,   1, 32, 33,  2,   9, 40, 41, 10,   1,  4,  5, 16 },
            new byte[] {  9,  8,  9,  8,   3, 34, 35,  0,  11, 42, 43,  8,  17,  6,  7,  0 },
            new byte[] { 25, 24, 25, 24,  17, 36, 37, 18,  25, 44, 45, 26,  17,  6,  7,  0 },
            new byte[] { 25, 24, 25, 24,  19, 38, 39, 16,  27, 46, 47, 24,   1,  4,  5, 16 },
            new byte[] {  9, 12, 13, 24,   1, 48, 49, 18,   9, 56, 57, 26,   1,  0,  1,  0 },
            new byte[] { 25, 14, 15,  8,  19, 50, 51,  0,  27, 58, 59,  8,   1,  0,  1,  0 },

            new byte[] { 25, 14, 15,  8,  17, 52, 53,  2,  25, 60, 61, 10,  17, 16, 17, 16 },
            new byte[] {  9, 12, 13, 24,   5, 54, 55, 16,  13, 62, 63, 24,  17, 16, 17, 16 }
        };
        */

        /// <summary>
        /// Collection of 46+1 orientations.
        /// </summary>
        /// <remarks>
        /// <para>Each orientation defines a 4x4 block of subtiles where the inner 4 tiles
        /// represent the actual tile, and the surrounding tiles make it possible to add
        /// an additional border to help reduce visual glitches.</para>
        /// </remarks>
        private static readonly byte[][] s_EXTENDED = {
            new byte[] { 33, 32, 33, 32,  27, 26, 27, 26,  33, 32, 33, 32,  27, 26, 27, 26 }, //  0
            new byte[] { XG, 30, 31, 32,  XG, 24, 25, 26,  XG, 30, 31, 32,  XG, 24, 25, 26 }, // 16
            new byte[] { XA, XA, XA, XA,  15, 14, 15, 14,  21, 20, 21, 20,  27, 26, 27, 26 }, // 20
            new byte[] { 33, 34, 35, XC,  27, 28, 29, XC,  33, 34, 35, XC,  27, 28, 29, XC }, // 24
            new byte[] { 33, 32, 33, 32,  39, 38, 39, 38,  45, 44, 45, 44,  XE, XE, XE, XE }, // 28
            new byte[] { XG, 30, 35, XC,  XG, 24, 29, XC,  XG, 30, 35, XC,  XG, 24, 29, XC }, // 32
            new byte[] { XA, XA, XA, XA,  15, 14, 15, 14,  45, 44, 45, 44,  XE, XE, XE, XE }, // 33

            new byte[] { XH, XA, XA, XA,  XG, 12, 13, 14,  XG, 18, 19, 20,  XG, 24, 25, 26 }, // 34
            new byte[] { XA, XA, XA, XB,  15, 16, 17, XC,  21, 22, 23, XC,  27, 28, 29, XC }, // 36
            new byte[] { 33, 34, 35, XC,  39, 40, 41, XC,  45, 46, 47, XC,  XE, XE, XE, XD }, // 38
            new byte[] { XG, 30, 31, 32,  XG, 36, 37, 38,  XG, 42, 43, 44,  XF, XE, XE, XE }, // 40
            new byte[] { XH, XA, XA, XB,  XG, 12, 17, XC,  XG, 18, 23, XC,  XG, 24, 29, XC }, // 42
            new byte[] { XH, XA, XA, XA,  XG, 12, 13, 14,  XG, 42, 43, 44,  XF, XE, XE, XE }, // 43
            new byte[] { XG, 30, 35, XC,  XG, 36, 41, XC,  XG, 42, 47, XC,  XF, XE, XE, XD }, // 44

            new byte[] { XA, XA, XA, XB,  15, 16, 17, XC,  45, 46, 47, XC,  XE, XE, XE, XD }, // 45
            new byte[] { XH, XA, XA, XB,  XG,  0,  1, XC,  XG,  6,  7, XC,  XF, XE, XE, XD }, // 46
            new byte[] { X1, 30, 33, 32,  15,  4, 27, 26,  33, 32, 33, 32,  27, 26, 27, 26 }, //  1
            new byte[] { 33, 32, 35, X2,  27, 26,  5, 14,  33, 32, 33, 32,  27, 26, 27, 26 }, //  2
            new byte[] { X1, 30, 35, X2,  15,  4,  5, 14,  33, 32, 33, 32,  27, 26, 27, 26 }, //  3
            new byte[] { 33, 32, 33, 32,  27, 26, 27, 26,  33, 32, 11, 44,  27, 26, 29, X3 }, //  4
            new byte[] { X1, 30, 33, 32,  15,  4, 27, 26,  33, 32, 11, 44,  27, 26, 29, X3 }, //  5

            new byte[] { 33, 32, 35, X2,  27, 26,  5, 14,  33, 32, 11, 44,  27, 26, 29, X3 }, //  6
            new byte[] { X1, 30, 35, X2,  15,  4,  5, 14,  33, 32, 11, 44,  27, 26, 29, X3 }, //  7
            new byte[] { 33, 32, 33, 32,  27, 26, 27, 26,  45, 10, 33, 32,  X4, 24, 27, 26 }, //  8
            new byte[] { X1, 30, 33, 32,  15,  4, 27, 26,  45, 10, 33, 32,  X4, 24, 27, 26 }, //  9
            new byte[] { 33, 32, 35, X2,  27, 26,  5, 14,  45, 10, 33, 32,  X4, 24, 27, 26 }, // 10
            new byte[] { X1, 30, 35, X2,  15,  4,  5, 14,  45, 10, 33, 32,  X4, 24, 27, 26 }, // 11
            new byte[] { 33, 32, 33, 32,  27, 26, 27, 26,  45, 10, 11, 44,  X4, 24, 29, X3 }, // 12

            new byte[] { X1, 30, 33, 32,  15,  4, 27, 26,  45, 10, 11, 44,  X4, 24, 29, X3 }, // 13
            new byte[] { 33, 32, 35, X2,  27, 26,  5, 14,  45, 10, 11, 44,  X4, 24, 29, X3 }, // 14
            new byte[] { X1, 30, 35, X2,  15,  4,  5, 14,  45, 10, 11, 44,  X4, 24, 29, X3 }, // 15
            new byte[] { XG, 30, 35, X2,  XG, 24,  5, 14,  XG, 30, 31, 32,  XG, 24, 37, 26 }, // 17
            new byte[] { XG, 30, 19, 32,  XG, 24, 25, 26,  XG, 30, 11, 44,  XG, 24, 29, X3 }, // 18
            new byte[] { XG, 30, 35, X2,  XG, 24,  5, 14,  XG, 30, 11, 44,  XG, 24, 29, X3 }, // 19
            new byte[] { XA, XA, XA, XA,  15, 14, 15, 14,  19, 20, 11, 44,  27, 26, 29, X3 }, // 21

            new byte[] { XA, XA, XA, XA,  15, 14, 15, 14,  45, 10, 21, 22,  X4, 24, 27, 26 }, // 22
            new byte[] { XA, XA, XA, XA,  15, 14, 15, 14,  45, 10, 11, 44,  X4, 24, 29, X3 }, // 23
            new byte[] { 33, 34, 35, XC,  27, 28, 29, XC,  45, 10, 35, XC,  X4, 24, 41, XC }, // 25
            new byte[] { X1, 30, 23, XC,  15,  4, 29, XC,  33, 34, 35, XC,  27, 28, 29, XC }, // 26
            new byte[] { X1, 30, 23, XC,  15,  4, 29, XC,  45, 10, 35, XC,  X4, 24, 41, XC }, // 27
            new byte[] { X1, 30, 33, 32,  15,  4, 39, 38,  43, 44, 45, 44,  XE, XE, XE, XE }, // 29
            new byte[] { 33, 32, 35, X2,  39, 38,  5, 14,  45, 44, 45, 46,  XE, XE, XE, XE }, // 30

            new byte[] { X1, 30, 35, X2,  15,  4,  5, 14,  43, 44, 45, 46,  XE, XE, XE, XE }, // 31
            new byte[] { XH, XA, XA, XA,  XG, 12, 13, 14,  XG, 18, 11, 44,  XG, 24, 29, X3 }, // 35
            new byte[] { XA, XA, XA, XB,  15, 16, 17, XC,  45, 10, 23, XC,  X4, 24, 29, XC }, // 37
            new byte[] { X1, 30, 35, XC,  15,  4, 41, XC,  45, 46, 47, XC,  XE, XE, XE, XD }, // 39
            new byte[] { XG, 30, 35, X2,  XG, 36,  5, 14,  XG, 42, 43, 44,  XF, XE, XE, XE }, // 41

            new byte[] {  9,  8,  9,  8,   3,  2,  3,  2,   9,  8,  9,  8,   3,  2,  3,  2 }, // 47
        };

        /// <summary>
        /// Alone orientation 000-00-000 but composed from corners instead.
        /// </summary>
        private static readonly byte[] s_EXTENDED_46_CORNER =
                       { XH, XA, XA, XB,  XG, 12, 17, XC,  XG, 42, 47, XC,  XF, XE, XE, XD }; // 46

        /// <summary>
        /// Collection of 46 orientations.
        /// </summary>
        /// <remarks>
        /// <para>Each orientation defines a 4x4 block of subtiles where the inner 4 tiles
        /// represent the actual tile, and the surrounding tiles make it possible to add
        /// an additional border to help reduce visual glitches.</para>
        /// </remarks>
        private static readonly byte[][] s_BASIC = {
            new byte[] { 13, 14, 13, 14,  17, 18, 17, 18,  13, 14, 13, 14,  17, 18, 17, 18 }, //  0
            new byte[] { XG, 12, 13, 14,  XG, 16, 17, 18,  XG, 12, 13, 14,  XG, 16, 17, 18 }, // 16
            new byte[] { XA, XA, XA, XA,   9, 10,  9, 10,  13, 14, 13, 14,  17, 18, 17, 18 }, // 20
            new byte[] { 13, 14, 15, XC,  17, 18, 19, XC,  13, 14, 15, XC,  17, 18, 19, XC }, // 24
            new byte[] { 13, 14, 13, 14,  17, 18, 17, 18,  21, 22, 21, 22,  XE, XE, XE, XE }, // 28
            new byte[] { XG, 12, 15, XC,  XG, 16, 19, XC,  XG, 12, 15, XC,  XG, 16, 19, XC }, // 32
            new byte[] { XA, XA, XA, XA,   9, 10,  9, 10,  21, 22, 21, 22,  XE, XE, XE, XE }, // 33

            new byte[] { XH, XA, XA, XA,  XG,  8,  9, 10,  XG, 12, 13, 14,  XG, 16, 17, 18 }, // 34
            new byte[] { XA, XA, XA, XB,   9, 10, 11, XC,  13, 14, 15, XC,  17, 18, 19, XC }, // 36
            new byte[] { 13, 14, 15, XC,  17, 18, 19, XC,  21, 22, 23, XC,  XE, XE, XE, XD }, // 38
            new byte[] { XG, 12, 13, 14,  XG, 16, 17, 18,  XG, 20, 21, 22,  XF, XE, XE, XE }, // 40
            new byte[] { XH, XA, XA, XB,  XG,  8, 11, XC,  XG, 12, 15, XC,  XG, 16, 19, XC }, // 42
            new byte[] { XH, XA, XA, XA,  XG,  8,  9, 10,  XG, 20, 21, 22,  XF, XE, XE, XE }, // 43
            new byte[] { XG, 12, 15, XC,  XG, 16, 19, XC,  XG, 20, 23, XC,  XF, XE, XE, XD }, // 44

            new byte[] { XA, XA, XA, XB,   9, 10, 11, XC,  21, 22, 23, XC,  XE, XE, XE, XD }, // 45
            new byte[] { XH, XA, XA, XB,  XG,  0,  1, XC,  XG,  4,  5, XC,  XF, XE, XE, XD }, // 46
            new byte[] { X1, 12, 13, 14,   9,  2, 17, 18,  13, 14, 13, 14,  17, 18, 17, 18 }, //  1
            new byte[] { 13, 14, 15, X2,  17, 18,  3, 10,  13, 14, 13, 14,  17, 18, 17, 18 }, //  2
            new byte[] { X1, 12, 15, X2,   9,  2,  3, 10,  13, 14, 13, 14,  17, 18, 17, 18 }, //  3
            new byte[] { 13, 14, 13, 14,  17, 18, 17, 18,  13, 14,  7, 22,  17, 18, 19, X3 }, //  4
            new byte[] { X1, 12, 13, 14,   9,  2, 17, 18,  13, 14,  7, 22,  17, 18, 19, X3 }, //  5

            new byte[] { 13, 14, 15, X2,  17, 18,  3, 10,  13, 14,  7, 22,  17, 18, 19, X3 }, //  6
            new byte[] { X1, 12, 15, X2,   9,  2,  3, 10,  13, 14,  7, 22,  17, 18, 19, X3 }, //  7
            new byte[] { 13, 14, 13, 14,  17, 18, 17, 18,  21,  6, 13, 14,  X4, 16, 17, 18 }, //  8
            new byte[] { X1, 12, 13, 14,   9,  2, 17, 18,  21,  6, 13, 14,  X4, 16, 17, 18 }, //  9
            new byte[] { 13, 14, 15, X2,  17, 18,  3, 10,  21,  6, 13, 14,  X4, 16, 17, 18 }, // 10
            new byte[] { X1, 12, 15, X2,   9,  2,  3, 10,  21,  6, 13, 14,  X4, 16, 17, 18 }, // 11
            new byte[] { 13, 14, 13, 14,  17, 18, 17, 18,  21,  6,  7, 22,  X4, 16, 19, X3 }, // 12

            new byte[] { X1, 12, 13, 14,   9,  2, 17, 18,  21,  6,  7, 22,  X4, 16, 19, X3 }, // 13
            new byte[] { 13, 14, 15, X2,  17, 18,  3, 10,  21,  6,  7, 22,  X4, 16, 19, X3 }, // 14
            new byte[] { X1, 12, 15, X2,   9,  2,  3, 10,  21,  6,  7, 22,  X4, 16, 19, X3 }, // 15
            new byte[] { XG, 12, 15, X2,  XG, 16,  3, 10,  XG, 12, 13, 14,  XG, 16, 17, 18 }, // 17
            new byte[] { XG, 12, 13, 14,  XG, 16, 17, 18,  XG, 12,  7, 22,  XG, 16, 19, X3 }, // 18
            new byte[] { XG, 12, 15, X2,  XG, 16,  3, 10,  XG, 12,  7, 22,  XG, 16, 19, X3 }, // 19
            new byte[] { XA, XA, XA, XA,   9, 10,  9, 10,  13, 14,  7, 22,  17, 18, 19, X3 }, // 21

            new byte[] { XA, XA, XA, XA,   9, 10,  9, 10,  21,  6, 13, 14,  X4, 16, 17, 18 }, // 22
            new byte[] { XA, XA, XA, XA,   9, 10,  9, 10,  21,  6,  7, 22,  X4, 16, 19, X3 }, // 23
            new byte[] { 13, 14, 15, XC,  17, 18, 19, XC,  21,  6, 15, XC,  X4, 16, 19, XC }, // 25
            new byte[] { X1, 12, 15, XC,   9,  2, 19, XC,  13, 14, 15, XC,  17, 18, 19, XC }, // 26
            new byte[] { X1, 12, 15, XC,   9,  2, 19, XC,  21,  6, 15, XC,  X4, 16, 19, XC }, // 27
            new byte[] { X1, 12, 13, 14,   9,  2, 17, 18,  21, 22, 21, 22,  XE, XE, XE, XE }, // 29
            new byte[] { 13, 14, 15, X2,  17, 18,  3, 10,  21, 22, 21, 22,  XE, XE, XE, XE }, // 30

            new byte[] { X1, 12, 15, X2,   9,  2,  3, 10,  21, 22, 21, 22,  XE, XE, XE, XE }, // 31
            new byte[] { XH, XA, XA, XA,  XG,  8,  9, 10,  XG, 12,  7, 22,  XG, 16, 19, X3 }, // 35
            new byte[] { XA, XA, XA, XB,   9, 10, 11, XC,  21,  6, 15, XC,  X4, 16, 19, XC }, // 37
            new byte[] { X1, 12, 15, XC,   9,  2, 19, XC,  21, 22, 23, XC,  XE, XE, XE, XD }, // 39
            new byte[] { XG, 12, 15, X2,  XG, 16,  3, 10,  XG, 20, 21, 22,  XF, XE, XE, XE }, // 41
        };

        /// <summary>
        /// Alone orientation 000-00-000 but composed from corners instead.
        /// </summary>
        private static readonly byte[] s_BASIC_46_CORNER =
                       { XH, XA, XA, XB,  XG,  8, 11, XC,  XG, 20, 23, XC,  XF, XE, XE, XD }; // 46

        #endregion


        public static void ApplyTileSizeConstraints(AutotileLayout layout, Texture2D artwork, bool innerJoins, ref int tileWidth, ref int tileHeight, ref int border)
        {
            // Apply minimum and maximum constraints.
            int maxTileWidth = 256, maxTileHeight = 256;
            if (artwork != null) {
                switch (layout) {
                    case AutotileLayout.Extended:
                        maxTileWidth = artwork.width / 3;
                        maxTileHeight = artwork.height / (innerJoins ? 4 : 3);
                        break;

                    case AutotileLayout.Basic:
                        maxTileWidth = artwork.width / 2;
                        maxTileHeight = artwork.height / (innerJoins ? 3 : 2);
                        break;
                }
            }

            // Constrain maximum tile size to 256x256.
            maxTileWidth = Mathf.Min(256, maxTileWidth);
            maxTileHeight = Mathf.Min(256, maxTileHeight);

            tileWidth = Mathf.Clamp(tileWidth, 1, maxTileWidth);
            tileHeight = Mathf.Clamp(tileHeight, 1, maxTileHeight);

            // Tile size must be divisible by 2.
            if (tileWidth % 2 == 1) {
                --tileWidth;
            }
            if (tileHeight % 2 == 1) {
                --tileHeight;
            }

            border = Mathf.Clamp(border, 0, Mathf.Min(tileWidth, tileHeight) / 2);
        }

        public static void EstimateTileSize(AutotileLayout layout, Texture2D artwork, bool innerJoins, ref int tileWidth, ref int tileHeight)
        {
            switch (layout) {
                case AutotileLayout.Extended:
                    tileWidth = artwork.width / 3;
                    tileHeight = artwork.height / (innerJoins ? 4 : 3);
                    break;

                case AutotileLayout.Basic:
                    tileWidth = artwork.width / 2;
                    tileHeight = artwork.height / (innerJoins ? 3 : 2);
                    break;
            }
        }

        public static Texture2D ExpandAutotileArtwork(AutotileLayout layout, Texture2D autotileArtwork, int tileWidth, int tileHeight, bool innerJoins, int border, bool clampEdges)
        {
            AutotileExpanderUtility expander = new AutotileExpanderUtility(layout, autotileArtwork, tileWidth, tileHeight, innerJoins, border, clampEdges);
            return expander.Generate();
        }


        private AutotileLayout layout;
        private Texture2D autotileArtwork;


        /// <summary>
        /// Orientation/Autotile mappings.
        /// </summary>
        /// <remarks>
        /// <para>Each row of map describes how the autotile artwork will be expanded
        /// for a single orientation.</para>
        /// </remarks>
        private byte[][] map;
        /// <summary>
        /// Autotile mappings for the extended "ground" tile.
        /// </summary>
        private byte[] groundTile;

#pragma warning disable 414
        private int tileWidth;
        private int tileHeight;
#pragma warning restore 414
        private int border;
        private bool clampEdges;

        private int halfTileWidth;
        private int halfTileHeight;
        private int tileOuterWidth;
        private int tileOuterHeight;

        // Buffer is used to generate individual orientations so that borders can be
        // rendered properly.
        private Color[] buffer;
        private int bufferWidth;


        /// <summary>
        /// Initializes a new instance of the autotile expander utility.
        /// </summary>
        /// <param name="layout">Layout style of autotile.</param>
        /// <param name="autotileArtwork">Autotile artwork to be expanded.</param>
        /// <param name="tileWidth">Width of tile in pixels.</param>
        /// <param name="tileHeight">Height of tile in pixels.</param>
        /// <param name="innerJoins">Indicates if inner joins should be generated.</param>
        /// <param name="border">Size of additional border in pixels to generate.</param>
        /// <param name="clampEdges">Indicates if edges should be clamped instead of tiled.</param>
        public AutotileExpanderUtility(AutotileLayout layout, Texture2D autotileArtwork, int tileWidth, int tileHeight, bool innerJoins, int border, bool clampEdges)
        {
            this.layout = layout;
            this.autotileArtwork = autotileArtwork;

            switch (layout) {
                //case AutotileLayout.tIDE:
                //    _map = _TIDE_STYLE;
                //    break;

                case AutotileLayout.Extended:
                    this.PrepareExtendedMap(innerJoins);
                    break;

                case AutotileLayout.Basic:
                    this.PrepareBasicMap(innerJoins);
                    break;
            }

            if (tileWidth % 2 == 1 || tileHeight % 2 == 1) {
                throw new Exception(TileLang.ParticularText("Error", "Tile width and height must both be divisible by 2."));
            }

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;
            this.border = border;
            this.clampEdges = clampEdges;

            this.halfTileWidth = tileWidth / 2;
            this.halfTileHeight = tileHeight / 2;
            this.tileOuterWidth = tileWidth + border * 2;
            this.tileOuterHeight = tileHeight + border * 2;

            this.bufferWidth = tileWidth * 2;
            this.buffer = new Color[this.bufferWidth * tileHeight * 2];
        }

        private void OffsetMicroTileIndices(byte[][] map, byte offset)
        {
            for (int i = 0; i < map.Length; ++i) {
                byte[] row = map[i];
                map[i] = (row = (byte[])row.Clone());

                for (int j = 0; j < row.Length; ++j) {
                    if (row[j] < XA) {
                        row[j] -= offset;
                    }
                }
            }
        }

        private void PrepareExtendedMap(bool innerJoins)
        {
            int mapLength = innerJoins ? s_EXTENDED.Length : 16;
            this.map = new byte[mapLength][];

            Array.Copy(s_EXTENDED, this.map, mapLength);

            if (!innerJoins) {
                this.map[15] = s_EXTENDED_46_CORNER;
                this.OffsetMicroTileIndices(this.map, 12);
                this.groundTile = null;
            }
            else {
                this.groundTile = s_EXTENDED[47];
            }
        }

        private void PrepareBasicMap(bool innerJoins)
        {
            int mapLength = innerJoins ? s_BASIC.Length : 16;
            this.map = new byte[mapLength][];

            Array.Copy(s_BASIC, this.map, mapLength);

            if (!innerJoins) {
                this.map[15] = s_BASIC_46_CORNER;
                this.OffsetMicroTileIndices(this.map, 8);
            }

            this.groundTile = null;
        }

        public void CalculateMetrics(out int width, out int height, out int unused)
        {
            height = width = 64;

            int columns, rows;

            // Need to expand tileset texture size until tiles fit!
            while (true) {
                columns = width / this.tileOuterWidth;
                rows = height / this.tileOuterHeight;

                if (columns * rows >= this.map.Length) {
                    break;
                }

                // Double size of atlas.
                width *= 2;
                height *= 2;
            }

            // Find the actual number of rows needed.
            rows = this.map.Length / columns;

            // Calculate the number of pixels that are actually used.
            int used = columns * this.tileOuterWidth * rows * this.tileOuterHeight;
            used += this.tileOuterWidth * this.tileOuterHeight * (this.map.Length % columns);

            // Calculate the number of wasted pixels.
            unused = width * height - used;
        }

        private Texture2D CreateTextureForExpandedAutotile()
        {
            int width, height, unused;
            this.CalculateMetrics(out width, out height, out unused);

            return new Texture2D(width, height, TextureFormat.ARGB32, false);
        }

        /// <summary>
        /// Generate autotile tileset atlas from provided autotile artwork.
        /// </summary>
        /// <returns>
        /// The generated autotile tileset atlas.
        /// </returns>
        private Texture2D Generate()
        {
            try {
                EditorUtility.DisplayProgressBar(
                    TileLang.ParticularText("Action", "Generate Autotile Atlas"),
                    TileLang.Text("Preparing atlas texture..."),
                    progress: 0f
                );

                Texture2D output = this.CreateTextureForExpandedAutotile();
                this.FillTexture(output, new Color());

                int bufferInsetX = this.halfTileWidth - this.border;
                int bufferInsetY = this.halfTileHeight - this.border;
                int bufferInsetX2 = bufferInsetX + this.tileOuterWidth;
                int bufferInsetY2 = bufferInsetY + this.tileOuterHeight;

                int outX = 0, outY = 0;

                int progress = 0;
                float ratio = 1f / (float)this.map.Length;

                foreach (byte[] orientation in this.map) {
                    if (progress++ % 10 == 0) {
                        EditorUtility.DisplayProgressBar(
                            TileLang.ParticularText("Action", "Generate Autotile Atlas"),
                            TileLang.Text("Processing atlas texture..."),
                            progress: (float)progress * ratio
                        );
                    }

                    this.PopulateBuffer(orientation);

                    // Copy from buffer to output texture.
                    for (int by = bufferInsetY, oy = 0; by < bufferInsetY2; ++by, ++oy) {
                        for (int bx = bufferInsetX, ox = 0; bx < bufferInsetX2; ++bx, ++ox) {
                            output.SetPixel(outX + ox, output.height - (outY + oy) - 1, this.GetBufferPixel(bx, by));
                        }
                    }

                    outX += this.tileOuterWidth;
                    if (outX + this.tileOuterWidth > output.width) {
                        outX = 0;
                        outY += this.tileOuterHeight;
                    }
                }

                output.Apply();
                return output;
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private void FillTexture(Texture2D texture, Color rgb)
        {
            for (int y = 0; y < texture.height; ++y) {
                for (int x = 0; x < texture.width; ++x) {
                    texture.SetPixel(x, y, rgb);
                }
            }
        }

        private void SetBufferPixel(int x, int y, Color rgb)
        {
            this.buffer[y * this.bufferWidth + x] = rgb;
        }

        private Color GetBufferPixel(int x, int y)
        {
            return this.buffer[y * this.bufferWidth + x];
        }

        private void CopySubtileToBuffer(int x, int y, int subtile, bool ignoreTransparent = false)
        {
            int sourceRow, sourceColumn;

            switch (this.layout) {
                //case AutotileStyle.tIDE:
                //    sourceRow = subtile / 8;
                //    sourceColumn = subtile % 8;
                //    break;
                case AutotileLayout.Extended:
                    sourceRow = subtile / 6;
                    sourceColumn = subtile % 6;
                    break;

                case AutotileLayout.Basic:
                    sourceRow = subtile / 4;
                    sourceColumn = subtile % 4;
                    break;

                default:
                    throw new Exception(string.Format(
                        /* 0: name of the unsupported autotile format */
                        TileLang.ParticularText("Error", "Unsupported autotile format '{0}'."),
                        this.layout
                    ));
            }

            int sourceX = sourceColumn * this.halfTileWidth;
            int sourceY = sourceRow * this.halfTileHeight;

            if (ignoreTransparent) {
                // Find out if there are any non-transparent pixels in subtile.
                Color color = new Color();
                for (int iy = 0; iy < this.halfTileHeight && color.a <= 0.003f; ++iy) {
                    for (int ix = 0; ix < this.halfTileWidth && color.a <= 0.003f; ++ix) {
                        color = this.autotileArtwork.GetPixel(sourceX + ix, this.autotileArtwork.height - (sourceY + iy) - 1);
                    }
                }

                // Abort, subtile is entirely transparent.
                if (color.a <= 0.003f) {
                    return;
                }
            }

            for (int iy = 0; iy < this.halfTileHeight; ++iy) {
                for (int ix = 0; ix < this.halfTileWidth; ++ix) {
                    this.SetBufferPixel(x + ix, y + iy, this.autotileArtwork.GetPixel(sourceX + ix, this.autotileArtwork.height - (sourceY + iy) - 1));
                }
            }
        }

        /// <summary>
        /// Populate buffer with subtiles (4x4 sub-grid).
        /// </summary>
        /// <remarks>
        /// <para>The buffer contains the 2x2 primary subtiles plus additional border
        /// tiles that are needed when border is added artificially.</para>
        /// <para>The center part of the buffer is copied into the combined output
        /// autotile tileset texture. The amount of the buffer that is copied depends
        /// upon the specified border size. Greater border means that some tiles must
        /// be tiled whilst others are repeated to help avoid rendering issues.</para>
        /// </remarks>
        /// <param name="orientation">Collection of subtile zero-based indices which
        /// correspond to subtiles in the specified autotile artwork.</param>
        private void PopulateBuffer(byte[] orientation)
        {
            int i = 0, outX = 0, outY = 0;

            // Copy 4x4 subtiles described by orientation, but ignore edge cases (XX - red).
            for (int row = 0; row < 4; ++row) {
                for (int column = 0; column < 4; ++column, ++i) {
                    if (orientation[i] < XA) {
                        this.CopySubtileToBuffer(outX, outY, orientation[i]);
                    }

                    outX += this.halfTileWidth;
                }

                outX = 0;
                outY += this.halfTileHeight;
            }

            // Skip stretching of edge pixels when no border is to be generated.
            // Just to avoid wasting time, usually when generating previews!
            if (this.border == 0) {
                return;
            }

            // Stretch edge pixels.
            i = outX = outY = 0;
            for (int row = 0; row < 4; ++row) {
                for (int column = 0; column < 4; ++column, ++i) {
                    switch (orientation[i]) {
                        case XA:
                            this._xa(outX, outY);
                            break;
                        case XB:
                            this._xb(outX, outY);
                            break;
                        case XC:
                            this._xc(outX, outY);
                            break;
                        case XD:
                            this._xd(outX, outY);
                            break;
                        case XE:
                            this._xe(outX, outY);
                            break;
                        case XF:
                            this._xf(outX, outY);
                            break;
                        case XG:
                            this._xg(outX, outY);
                            break;
                        case XH:
                            this._xh(outX, outY);
                            break;
                        case X1:
                            this._x1(outX, outY);
                            break;
                        case X2:
                            this._x2(outX, outY);
                            break;
                        case X3:
                            this._x3(outX, outY);
                            break;
                        case X4:
                            this._x4(outX, outY);
                            break;
                    }

                    outX += this.halfTileWidth;
                }

                outX = 0;
                outY += this.halfTileHeight;
            }

            // Do not use "ground" tile when clamping is desired!
            // Ground tile is only available for extended autotiles.
            if (this.clampEdges || this.groundTile == null) {
                return;
            }

            // Copy edge subtiles from ground tile.
            i = outX = outY = 0;

            // Copy 4x4 subtiles described by orientation, but ignore edge cases (XX - red).
            for (int row = 0; row < 4; ++row) {
                for (int column = 0; column < 4; ++column, ++i) {
                    if (orientation[i] >= XA) {
                        this.CopySubtileToBuffer(outX, outY, this.groundTile[i], true);
                    }

                    outX += this.halfTileWidth;
                }

                outX = 0;
                outY += this.halfTileHeight;
            }
        }

        #region Pixel Stretchers

        private void _xa(int x, int y)
        {
            for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                Color rgb = this.GetBufferPixel(x + ox, y + this.halfTileHeight);
                for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xb(int x, int y)
        {
            Color rgb = this.GetBufferPixel(x - 1, y + this.halfTileHeight);
            for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xc(int x, int y)
        {
            for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                Color rgb = this.GetBufferPixel(x - 1, y + oy);
                for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xd(int x, int y)
        {
            Color rgb = this.GetBufferPixel(x - 1, y - 1);
            for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xe(int x, int y)
        {
            for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                Color rgb = this.GetBufferPixel(x + ox, y - 1);
                for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xf(int x, int y)
        {
            Color rgb = this.GetBufferPixel(x + this.halfTileWidth, y - 1);
            for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xg(int x, int y)
        {
            for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                Color rgb = this.GetBufferPixel(x + this.halfTileWidth, y + oy);
                for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _xh(int x, int y)
        {
            Color rgb = this.GetBufferPixel(x + this.halfTileWidth, y + this.halfTileHeight);
            for (int ox = 0; ox < this.halfTileWidth; ++ox) {
                for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _x1(int x, int y)
        {
            this._xa(x, y);

            int oxOffset = 0;

            for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                Color rgb = this.GetBufferPixel(x + this.halfTileWidth, y + oy);
                for (int ox = oxOffset++; ox < this.halfTileWidth; ++ox) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        private void _x2(int x, int y)
        {
            this._xa(x, y);

            int oxWidth = this.halfTileWidth;

            for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                Color rgb = this.GetBufferPixel(x - 1, y + oy);
                for (int ox = 0; ox < oxWidth; ++ox) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }

                if (--oxWidth <= 0) {
                    break;
                }
            }
        }

        private void _x3(int x, int y)
        {
            this._xe(x, y);

            int oxWidth = 1;

            for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                Color rgb = this.GetBufferPixel(x - 1, y + oy);
                for (int ox = 0; ox < oxWidth; ++ox) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }

                if (oxWidth < this.halfTileWidth) {
                    ++oxWidth;
                }
            }
        }

        private void _x4(int x, int y)
        {
            this._xe(x, y);

            int oxOffset = this.halfTileWidth;

            for (int oy = 0; oy < this.halfTileHeight; ++oy) {
                Color rgb = this.GetBufferPixel(x + this.halfTileWidth, y + oy);

                if (oxOffset > 0) {
                    --oxOffset;
                }

                for (int ox = oxOffset; ox < this.halfTileWidth; ++ox) {
                    this.SetBufferPixel(x + ox, y + oy, rgb);
                }
            }
        }

        #endregion
    }
}
