// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Autotile tilesets are created automatically and are paired with an atlas texture
    /// that is an expanded form of the input autotile artwork.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Autotile-Tilesets">Autotile Tilesets</a>
    /// section of user guide for further information about autotile tilesets.</para>
    /// </intro>
    /// <remarks>
    /// <para>Autotile tilesets can contain both <see cref="AutotileBrush"/>'s as well
    /// as regular <see cref="TilesetBrush"/>'s.</para>
    /// <para>Additional border space can be automatically added to autotile atlases
    /// when the autotile artwork is expanded. Whilst this does not entirely eliminate
    /// bleeding at the edges of tiles, it is a significant improvement when tiles are
    /// not viewed to closely.</para>
    /// </remarks>
    /// <seealso cref="AutotileBrush"/>
    /// <seealso cref="TilesetBrush"/>
    public sealed class AutotileTileset : Tileset
    {
        #region Properties

        /// <inheritdoc/>
        public override string DesignableType {
            get { return "Autotile Tileset"; }
        }

        [SerializeField, HideInInspector, FormerlySerializedAs("_autotileStyle")]
        private AutotileLayout autotileLayout;
        [SerializeField, HideInInspector, FormerlySerializedAs("_hasInnerJoins")]
        private bool hasInnerJoins = true;
        [SerializeField, HideInInspector, FormerlySerializedAs("_forceClampEdges")]
        private bool forceClampEdges;

        /// <summary>
        /// Raw texture that was used to generate autotile atlas.
        /// </summary>
        public Texture2D rawTexture;

        /// <summary>
        /// Gets the style of autotile layout.
        /// </summary>
        /// <remarks>
        /// <para>Once created the layout style of an autotile cannot be changed.</para>
        /// </remarks>
        public AutotileLayout AutotileLayout {
            get { return this.autotileLayout; }
        }

        /// <summary>
        /// Gets a value indicating whether autotile tileset contains inner joins.
        /// </summary>
        /// <remarks>
        /// <para>Once created inner joins cannot be added or removed.</para>
        /// </remarks>
        public bool HasInnerJoins {
            get { return this.hasInnerJoins; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether edges should be clamped
        /// when generating atlas texture.
        /// </summary>
        public bool ForceClampEdges {
            get { return this.forceClampEdges; }
            set { this.forceClampEdges = value; }
        }

        /// <summary>
        /// Gets a value indicating whether secondary brush is supported.
        /// </summary>
        public bool IsSecondaryBrushSupported {
            get { return this.autotileLayout == AutotileLayout.Extended; }
        }

        #endregion


        #region Brush Creation

        /// <summary>
        /// Create instance of primary autotile brush.
        /// </summary>
        /// <param name="name">Name for brush.</param>
        /// <returns>
        /// The new autotile brush.
        /// </returns>
        public AutotileBrush CreatePrimaryBrush(string name)
        {
            AutotileBrush brush = ScriptableObject.CreateInstance<AutotileBrush>();
            brush.Initialize(this);
            brush.name = name;
            brush.visibility = BrushVisibility.Favorite;

            return brush;
        }

        /// <summary>
        /// Create instance of secondary autotile brush.
        /// </summary>
        /// <remarks>
        /// <para>Always check to see if secondary brush is supported using <see cref="IsSecondaryBrushSupported"/>
        /// before calling this method.</para>
        /// </remarks>
        /// <param name="name">Name for brush.</param>
        /// <returns>
        /// The new tileset brush.
        /// </returns>
        public TilesetBrush CreateSecondaryBrush(string name)
        {
            TilesetBrush brush = ScriptableObject.CreateInstance<TilesetBrush>();
            brush.Initialize(this);
            brush.name = name;
            brush.tileIndex = /*(_style == AutotileStyle.tIDE) ? 4 :*/ 47;
            brush.visibility = BrushVisibility.Favorite;

            return brush;
        }

        #endregion


        #region Autotile Mapping

        /*private static readonly int[] s_OrientationMap4 = {
            0xFF, // 11111111
            0x1F, // 11111000
            0x6B, // 11010110
            0xD6, // 01101011
            0x00, // 00000000
            0xF8, // 00011111
            0xDB, // 11011011
            0x7E, // 01111110
            0x7F, // 11111110
            0xDF, // 11111011
            0xD0, // 00001011
            0x68, // 00010110
            0xFB, // 11011111
            0xFE, // 01111111
            0x16, // 01101000
            0x0B, // 11010000
        };*/

        private static readonly int[] s_OrientationMap8 = {
            0xFF, // 11111111
            0xD6, // 01101011
            0xF8, // 00011111
            0x6B, // 11010110
            0x1F, // 11111000
            0x42, // 01000010
            0x18, // 00011000
            0xD0, // 00001011
            0x68, // 00010110
            0x0B, // 11010000
            0x16, // 01101000
            0x40, // 00000010
            0x10, // 00001000
            0x02, // 01000000
            0x08, // 00010000
            0x00, // 00000000
            0xFE, // 01111111
            0xFB, // 11011111
            0xFA, // 01011111
            0x7F, // 11111110
            0x7E, // 01111110
            0x7B, // 11011110
            0x7A, // 01011110
            0xDF, // 11111011
            0xDE, // 01111011
            0xDB, // 11011011
            0xDA, // 01011011
            0x5F, // 11111010
            0x5E, // 01111010
            0x5B, // 11011010
            0x5A, // 01011010
            0xD2, // 01001011
            0x56, // 01101010
            0x52, // 01001010
            0x78, // 00011110
            0xD8, // 00011011
            0x58, // 00011010
            0x4B, // 11010010
            0x6A, // 01010110
            0x4A, // 01010010
            0x1E, // 01111000
            0x1B, // 11011000
            0x1A, // 01011000
            0x50, // 00001010
            0x48, // 00010010
            0x0A, // 01010000
            0x12, // 01001000
        };

        /// <summary>
        /// Gets tile index for specified orientation.
        /// </summary>
        /// <param name="orientation">Bit mask of tile orientation.</param>
        /// <returns>
        /// Zero-based index of tile in tileset; or <c>-1</c> if undefined.
        /// </returns>
        public int IndexFromOrientation(int orientation)
        {
            //int[] map = (_style == AutotileStyle.tIDE) ? s_OrientationMap4 : s_OrientationMap8;
            return Array.IndexOf(s_OrientationMap8, orientation);
        }

        /// <summary>
        /// Gets orientation for specified tile index.
        /// </summary>
        /// <param name="index">Zero-based index of tile in tileset.</param>
        /// <returns>
        /// Bit-mask representation of orientation; or <c>-1</c> if undefined.
        /// </returns>
        public int OrientationFromIndex(int index)
        {
            //int[] map = (_style == AutotileStyle.tIDE) ? s_OrientationMap4 : s_OrientationMap8;
            return (index >= 0 && index < s_OrientationMap8.Length)
                ? s_OrientationMap8[index]
                : -1;
        }

        /// <summary>
        /// Finds tile index that is closest to specified orientation.
        /// </summary>
        /// <param name="orientation">Bit mask of tile orientation.</param>
        /// <returns>
        /// Zero-based index of tile in tileset.
        /// </returns>
        public int FindClosestIndexFromOrientation(int orientation)
        {
            //int[] map = (_style == AutotileStyle.tIDE) ? s_OrientationMap4 : s_OrientationMap8;

            int index = Array.IndexOf(s_OrientationMap8, orientation);
            if (index != -1)
                return index;

            int strongestConnections = 2;
            int weakConnections = 0;

            //int strongestOrientation = 0x00; // 00000000
            index = /*(_style == AutotileStyle.tIDE) ? 0 :*/ 15;

            for (int i = 0; i < s_OrientationMap8.Length; ++i) {
                int childOrientation = s_OrientationMap8[i];

                // If there are at least 3 strong connections...
                int s = OrientationUtility.CountStrongConnections(orientation, childOrientation);
                if (s > strongestConnections) {
                    // Strong connections overule any previous weak connection matches!
                    strongestConnections = s;
                    weakConnections = OrientationUtility.CountWeakConnections(orientation, childOrientation);
                    //strongestOrientation = childOrientation;
                    index = i;
                }
                else if (s == strongestConnections) {
                    // Choose the connection that has the most weak connections!
                    int w = OrientationUtility.CountWeakConnections(orientation, childOrientation);
                    if (w > weakConnections) {
                        //strongestOrientation = childOrientation;
                        index = i;
                        weakConnections = w;
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Finds the closest match for the specified orientation.
        /// </summary>
        /// <param name="orientation">Bit mask of tile orientation.</param>
        /// <returns>
        /// Bit mask of the closest available orientation.
        /// </returns>
        public int FindClosestOrientation(int orientation)
        {
            return s_OrientationMap8[
                this.FindClosestIndexFromOrientation(orientation)
            ];
        }

        #endregion


        /// <summary>
        /// Initialize tileset for first time.
        /// </summary>
        /// <param name="material">Atlas material.</param>
        /// <param name="atlas">Atlas texture.</param>
        /// <param name="metrics">Object that contains metrics for tileset.</param>
        /// <exception cref="System.NotSupportedException">
        /// If tileset has already been initialized.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="atlas"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// If attempting to set metrics of tileset from itself.
        /// </exception>
        public void Initialize(AutotileLayout layout, bool hasInnerJoins, Material material, Texture2D atlas, ITilesetMetrics metrics)
        {
            base.Initialize(material, atlas, metrics);

            this.autotileLayout = layout;
            this.hasInnerJoins = hasInnerJoins;
        }
    }
}
