// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Internal
{
    /// <summary>
    /// This class can be used to adjust the number of rows and columns in a tile system,
    /// change the chunk size, or offset existing tiles.
    /// </summary>
    public sealed class TileSystemResizer
    {
        /// <summary>
        /// Total number of tasks to be completed.
        /// </summary>
        private float taskCount;
        /// <summary>
        /// Current progress.
        /// </summary>
        private float taskProgress;
        /// <summary>
        /// </summary>
        private float taskIncrement;


        /// <summary>
        /// Resize tile system.
        /// </summary>
        /// <remarks>
        /// <para>Progress bar is shown when this method is invoked in-editor, though this progress
        /// bar is not shown when working in play mode.</para>
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
        /// <param name="eraseOutOfBounds">Indicates whether out-of-bound tiles should be
        /// erased.</param>
        public void Resize(TileSystem system, int newRows, int newColumns, int rowOffset, int columnOffset, int chunkWidth, int chunkHeight, bool maintainTilePositionsInWorld, bool eraseOutOfBounds)
        {
            bool restoreEnableProgressHandler = InternalUtility.EnableProgressHandler;
            InternalUtility.EnableProgressHandler = Application.isEditor && !Application.isPlaying;
            try {
                this.taskCount = system.RowCount + newRows;
                this.taskProgress = 0f;
                this.taskIncrement = 1f / this.taskCount;

                // Erase out-of-bound tiles.
                if (eraseOutOfBounds) {
                    InternalUtility.ProgressHandler("Rebuilding Tile System", "Erasing out-of-bound tiles.", 0f);
                    this.EraseOutOfBoundTiles(system, newRows, newColumns, rowOffset, columnOffset);
                }

                InternalUtility.ProgressHandler("Rebuilding Tile System", "Extracting tiles from chunks.", 0f);

                TileData[,] map = this.GenerateTileMap(system, newRows, newColumns, rowOffset, columnOffset);

                this.ReparentTileGameObjectsIntoWorldSpace(map);
                this.RemoveChunkObjects(system);

                // Update data structure of tile system.
                Vector3 cellSize = system.CellSize;
                system.InitializeSystem(cellSize.x, cellSize.y, cellSize.z, newRows, newColumns, chunkWidth, chunkHeight);

                // Reposition tile system so that tiles are re-parented correctly.
                Transform systemTransform = system.transform;
                Vector3 previousLocalPosition = systemTransform.localPosition;
                systemTransform.position = system.WorldPositionFromTileIndex(-rowOffset, -columnOffset, false);

                system.BeginBulkEdit();

                // Reparent tile game objects.
                for (int row = 0; row < system.RowCount; ++row) {
                    this.taskProgress += this.taskIncrement;
                    InternalUtility.ProgressHandler("Rebuilding Tile System", "Creating new chunks.", this.taskProgress);

                    for (int column = 0; column < system.ColumnCount; ++column) {
                        var tile = map[row, column];
                        if (tile == null || tile.Empty) {
                            continue;
                        }

                        // Mark procedural tiles as dirty.
                        if (tile.Procedural) {
                            tile.Dirty = true;
                        }

                        // Assign tile to tile system.
                        system.SetTile(row, column, tile);

                        var chunk = system.GetChunkFromTileIndex(row, column);
                        chunk.Dirty = true;

                        // Place tile game object into chunk.
                        if (tile.gameObject != null) {
                            tile.gameObject.transform.SetParent(chunk.transform);
                        }
                    }
                }

                this.taskProgress += this.taskIncrement;
                InternalUtility.ProgressHandler("Rebuilding Tile System", "Updating tiles.", this.taskProgress);

                system.EndBulkEdit();

                if (!maintainTilePositionsInWorld) {
                    systemTransform.localPosition = previousLocalPosition;
                }
            }
            finally {
                InternalUtility.ClearProgress();
                InternalUtility.EnableProgressHandler = restoreEnableProgressHandler;
            }
        }

        /// <summary>
        /// Generate new tile map and extract existing tiles for resized/offset tiles.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="newRows">New number of rows.</param>
        /// <param name="newColumns">New number of columns.</param>
        /// <param name="rowOffset">Number of rows of tiles to offset by.</param>
        /// <param name="columnOffset">Number of columns of tiles to offset by.</param>
        private TileData[,] GenerateTileMap(TileSystem system, int newRows, int newColumns, int rowOffset, int columnOffset)
        {
            TileData[,] map = new TileData[newRows, newColumns];

            rowOffset = -rowOffset;
            columnOffset = -columnOffset;

            int offsetEndRow = rowOffset + newRows;
            int offsetEndColumn = columnOffset + newColumns;

            for (int row = 0; row < system.RowCount; ++row)
                for (int column = 0; column < system.ColumnCount; ++column) {
                    var tile = system.GetTile(row, column);
                    if (tile == null) {
                        continue;
                    }

                    // Reminder, out-of-bound tiles should already have been erased :)

                    // Extract tiles that are within bounds.
                    if (row >= rowOffset && row < offsetEndRow && column >= columnOffset && column < offsetEndColumn) {
                        map[row - rowOffset, column - columnOffset] = tile;
                    }
                }

            return map;
        }

        /// <summary>
        /// Erase all out-of-bound tiles.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="newRows">New number of rows.</param>
        /// <param name="newColumns">New number of columns.</param>
        /// <param name="rowOffset">Number of rows of tiles to offset by.</param>
        /// <param name="columnOffset">Number of columns of tiles to offset by.</param>
        private void EraseOutOfBoundTiles(TileSystem system, int newRows, int newColumns, int rowOffset, int columnOffset)
        {
            if (system.Chunks == null) {
                return;
            }

            rowOffset = -rowOffset;
            columnOffset = -columnOffset;

            int offsetEndRow = rowOffset + newRows;
            int offsetEndColumn = columnOffset + newColumns;

            system.BeginBulkEdit();

            for (int row = 0; row < system.RowCount; ++row)
                for (int column = 0; column < system.ColumnCount; ++column) {
                    var tile = system.GetTile(row, column);
                    if (tile == null) {
                        continue;
                    }

                    // Is tile out-of-bounds?
                    if (row < rowOffset || row >= offsetEndRow || column < columnOffset || column >= offsetEndColumn) {
                        system.EraseTile(row, column);
                        system.RefreshSurroundingTiles(row, column);
                    }
                }

            system.EndBulkEdit();
        }

        /// <summary>
        /// Reparent tile game objects into world space so that their transforms can be
        /// maintained when they are placed into their new chunks.
        /// </summary>
        /// <param name="map">New tile map.</param>
        private void ReparentTileGameObjectsIntoWorldSpace(TileData[,] map)
        {
            int mapRows = map.GetLength(0);
            int mapColumns = map.GetLength(1);

            for (int row = 0; row < mapRows; ++row) {
                this.taskProgress += this.taskIncrement;
                InternalUtility.ProgressHandler("Rebuilding Tile System", "Clearing existing chunks.", this.taskProgress);

                for (int column = 0; column < mapColumns; ++column) {
                    var tile = map[row, column];
                    if (tile == null || tile.Empty || tile.gameObject == null) {
                        continue;
                    }

                    tile.gameObject.transform.SetParent(null);
                }
            }
        }

        /// <summary>
        /// Remove existing chunk objects from tile system and place rogue game objects
        /// as children of tile system object.
        /// </summary>
        /// <param name="system">Tile system.</param>
        private void RemoveChunkObjects(TileSystem system)
        {
            // Clear existing chunk game objects.
            foreach (var chunk in system.Chunks) {
                if (chunk == null) {
                    continue;
                }

                // Destroy associated procedural mesh game object.
                if (chunk.ProceduralMesh != null) {
                    Object.DestroyImmediate(chunk.ProceduralMesh.gameObject);
                }

                // Move rogue game objects into game object of tile system.
                foreach (Transform child in chunk.transform) {
                    child.SetParent(system.transform);
                }

                Object.DestroyImmediate(chunk.gameObject);
            }
        }
    }
}
