// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;

namespace Rotorz.Tile
{
    /// <summary>
    /// Utility functionality for tile systems.
    /// </summary>
    public static class TileSystemUtility
    {
        #region Register of Tile Systems in Scene (Editor Only!)

        //
        // This functionality is editor-only since at runtime there isn't a way to determine
        // whether a tile system instance resides within a prefab or is actively present in
        // the current scene.
        //

        private static object s_TileSystemListingDirtyLock = new object();
        private static bool s_TileSystemListingDirty = true;


        /// <exclude/>
        public static bool TileSystemListingDirty {
            get {
                lock (s_TileSystemListingDirtyLock) {
                    return s_TileSystemListingDirty;
                }
            }
            set {
                lock (s_TileSystemListingDirtyLock) {
                    s_TileSystemListingDirty = value;
                }
            }
        }

        #endregion


        /// <summary>
        /// Extract tile data from tile system.
        /// </summary>
        /// <remarks>
        /// <para>This is particularly useful for scripts which need to temporarily retain
        /// a simple two-dimensional array of tile data whilst restructuring a tile system.</para>
        /// <para>This function loops through tiles and places the data of non-empty tiles
        /// into an array which is then returned. User scripts can modify returned array
        /// as needed without effecting the actual tile system. Changes to tile data will
        /// effect tile system.</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <returns>
        /// Two-dimensional array of tile data where empty tiles are <c>null</c>.
        /// </returns>
        public static TileData[,] ExtractTiles(TileSystem system)
        {
            TileData[,] map = new TileData[system.RowCount, system.ColumnCount];
            for (int row = 0; row < system.RowCount; ++row) {
                for (int column = 0; column < system.ColumnCount; ++column) {
                    map[row, column] = system.GetTile(row, column);
                }
            }
            return map;
        }

        /// <summary>
        /// Finds minimum and maximum extents of bounds that encapsulate one or more
        /// painted tiles.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="min">Minimum tile index.</param>
        /// <param name="max">Maximum tile index.</param>
        /// <returns>
        /// A value of <c>true</c> if tile bounds were found; otherwise a value of <c>false</c>.
        /// A value of <c>false</c> is also returned for tile systems that contain no
        /// painted tiles.
        /// </returns>
        public static bool FindTileBounds(TileSystem system, out TileIndex min, out TileIndex max)
        {
            min = new TileIndex(int.MaxValue, int.MaxValue);
            max = new TileIndex(int.MinValue, int.MinValue);

            // Find first row that contains a tile.
            for (int row = 0; row < system.RowCount; ++row) {
                for (int column = 0; column < system.ColumnCount; ++column) {
                    if (system.GetTile(row, column) != null) {
                        if (row < min.row) {
                            min.row = row;
                        }
                        if (column < min.column) {
                            min.column = column;
                        }
                        if (row > max.row) {
                            max.row = row;
                        }
                        if (column > max.column) {
                            max.column = column;
                        }
                    }
                }
            }

            return (min.row != int.MaxValue && min.column != int.MaxValue && max.row != int.MinValue && max.column != int.MaxValue);
        }

        /// <summary>
        /// Determines whether tiles will become out-of-bounds upon resizing tile system,
        /// or offsetting tiles within tile system.
        /// </summary>
        /// <remarks>
        /// <para>This can be used to present a confirmation message to warn user that
        /// tiles will be erased upon altering tile system using <see cref="Resize">Resize</see>.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source demonstrates how to present a warning message prior
        /// to resizing a tile system. This example is for an editor script, though similar
        /// could be achieved at runtime.</para>
        /// <code language="csharp"><![CDATA[
        /// if (TileSystemUtility.WillHaveOutOfBoundTiles(system, 10, 10, 0, 0)) {
        ///     // Display confirmation message with user and abort if cancelled.
        ///     if (!EditorUtility.DisplayDialog(
        ///         "Resize",
        ///         "Tiles will be clipped if you proceed.",
        ///         "Proceed",
        ///         "Abort"
        ///     )) return;
        /// }
        ///
        /// TileSystemUtility.Resize(system, 10, 10, 0, 0, 10, 10, true);
        /// ]]></code>
        /// </example>
        /// <param name="system">Tile system.</param>
        /// <param name="newRows">New number of rows.</param>
        /// <param name="newColumns">New number of columns.</param>
        /// <param name="rowOffset">Number of rows of tiles to offset by.</param>
        /// <param name="columnOffset">Number of columns of tiles to offset by.</param>
        /// <returns>
        /// A value of <c>true</c> if tiles will become out-of-bounds; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="Resize"/>
        public static bool WillHaveOutOfBoundTiles(TileSystem system, int newRows, int newColumns, int rowOffset, int columnOffset)
        {
            if (system.Chunks == null) {
                return false;
            }

            rowOffset = -rowOffset;
            columnOffset = -columnOffset;

            int offsetEndRow = rowOffset + newRows;
            int offsetEndColumn = columnOffset + newColumns;

            for (int row = 0; row < system.RowCount; ++row) {
                for (int column = 0; column < system.ColumnCount; ++column) {
                    TileData tile = system.GetTile(row, column);
                    if (tile == null) {
                        continue;
                    }

                    // Is tile out-of-bounds?
                    if (row < rowOffset || row >= offsetEndRow || column < columnOffset || column >= offsetEndColumn) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adjust the number of rows, columns and/or chunk size of a tile system.
        /// </summary>
        /// <remarks>
        /// <para>This is achieved by reconstructing the data structure of the specified
        /// tile system whilst maintaining game objects that are attached to tiles where
        /// possible. Procedural chunk meshes are automatically updated to conform to the
        /// updated data structure.</para>
        /// <para>This function may take a while to complete and should not be used unless
        /// absolutely necessary.</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <param name="newRows">New number of rows.</param>
        /// <param name="newColumns">New number of columns.</param>
        /// <param name="rowOffset">Number of rows of tiles to offset by.</param>
        /// <param name="columnOffset">Number of columns of tiles to offset by.</param>
        /// <param name="chunkWidth">New chunk width.</param>
        /// <param name="chunkHeight">New chunk height.</param>
        /// <param name="maintainTilePositionsInWorld">Indicates if tile positions should
        /// be maintained in world space.</param>
        /// <seealso cref="WillHaveOutOfBoundTiles"/>
        public static void Resize(TileSystem system, int newRows, int newColumns, int rowOffset, int columnOffset, int chunkWidth, int chunkHeight, bool maintainTilePositionsInWorld)
        {
            TileSystemResizer resizer = new TileSystemResizer();
            resizer.Resize(system, newRows, newColumns, rowOffset, columnOffset, chunkWidth, chunkHeight, maintainTilePositionsInWorld, true);
        }
    }
}
