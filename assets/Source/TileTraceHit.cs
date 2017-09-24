// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Provides details about tile trace hit result.
    /// </summary>
    public struct TileTraceHit
    {
        /// <summary>
        /// Indicates that no tile was hit during tile trace.
        /// </summary>
        public static readonly TileTraceHit none = new TileTraceHit(-1, -1, null);


        /// <summary>
        /// Zero-based index of row that was hit.
        /// </summary>
        /// <value>
        /// A value of <c>-1</c> if no tile was hit.
        /// </value>
        public int row;

        /// <summary>
        /// Zero-based index of column that was hit.
        /// </summary>
        /// <remarks>
        /// A value of <c>-1</c> if no tile was hit.
        /// </remarks>
        public int column;

        /// <summary>
        /// Data for tile that was hit.
        /// </summary>
        /// <value>
        /// A value of <c>null</c> if no tile was hit.
        /// </value>
        public TileData tile;


        /// <summary>
        /// Initialize tile trace hit.
        /// </summary>
        /// <param name="row">Zero-based index of row.</param>
        /// <param name="column">Zero-based index of column.</param>
        /// <param name="tile">Data for tile that was hit.</param>
        public TileTraceHit(int row, int column, TileData tile)
        {
            this.row = row;
            this.column = column;
            this.tile = tile;
        }
    }
}
