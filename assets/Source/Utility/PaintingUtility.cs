// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rotorz.Tile
{
    /// <summary>
    /// Utility functions for painting tiles, lines of tiles, circles and squares.
    /// The editor tools included with this extension use this class to paint tiles
    /// though this class can also be used by custom scripts at runtime.
    /// </summary>
    public static class PaintingUtility
    {
        #region Painting Events

        /// <summary>
        /// Occurs when a tile is painted using a brush.
        /// </summary>
        /// <remarks>
        /// <para>This event can be consumed by both editor and runtime scripts to post
        /// process tiles as they are painted. This event occurs for each tile that is
        /// painted, cycled or refreshed.</para>
        /// <para>Do not forget to remove event handlers when they are no longer required
        /// to avoid memory related issues.</para>
        /// </remarks>
        /// <example>
        /// <para>The following example demonstrates how the game objects of painted tiles
        /// can be rotated towards a particular point in world space.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     private void Awake()
        ///     {
        ///         PaintingUtility.TilePainted += this.OnTilePainted;
        ///     }
        ///
        ///     private void OnDestroy()
        ///     {
        ///         PaintingUtility.TilePainted -= this.OnTilePainted;
        ///     }
        ///
        ///     private void OnTilePainted(TilePaintedEventArgs args)
        ///     {
        ///         // Tile might not have a game object attached!
        ///         if (args.GameObject != null) {
        ///             args.GameObject.transform.LookAt(Vector3.zero);
        ///         }
        ///     }
        /// }
        /// ]]></code>
        ///
        /// <para>This event can also be utilised by custom editor scripts:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// [InitializeOnLoad]
        /// static class MyCustomEditor
        /// {
        ///     static MyCustomEditor()
        ///     {
        ///         PaintingUtility.TilePainted += OnTilePainted;
        ///     }
        ///
        ///
        ///     private static void OnTilePainted(TilePaintedEventArgs args)
        ///     {
        ///         // Tile might not have a game object attached!
        ///         if (args.GameObject != null) {
        ///             args.GameObject.transform.LookAt(Vector3.zero);
        ///         }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public static event TilePaintedEventHandler TilePainted;

        internal static void RaiseTilePaintedEvent(IBrushContext context, TileData tile)
        {
            if (TilePainted != null) {
                TileIndex index;
                index.row = context.Row;
                index.column = context.Column;
                TilePainted(new TilePaintedEventArgs(context.TileSystem, index, tile));
            }
        }

        /// <summary>
        /// Occurs when a tile will be erased. This event <strong>should not</strong> be
        /// used to paint new tiles.
        /// </summary>
        /// <remarks>
        /// <para>This event can be consumed by both editor and runtime scripts to take
        /// action immediately before a tile is erased.</para>
        /// <para>Do not forget to remove event handlers when they are no longer required
        /// to avoid memory related issues.</para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     private void Awake()
        ///     {
        ///         PaintingUtility.WillEraseTile += this.OnWillEraseTile;
        ///     }
        ///
        ///     private void OnDestroy()
        ///     {
        ///         PaintingUtility.WillEraseTile -= this.OnWillEraseTile;
        ///     }
        ///
        ///     private void OnWillEraseTile(WillEraseTileEventArgs args)
        ///     {
        ///         Debug.Log("Will erase tile at index: " + args.TileIndex);
        ///     }
        /// }
        /// ]]></code>
        ///
        /// <para>This event can also be utilised by custom editor scripts:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// [InitializeOnLoad]
        /// static class MyCustomEditor
        /// {
        ///     static MyCustomEditor()
        ///     {
        ///         PaintingUtility.WillEraseTile += OnWillEraseTile;
        ///     }
        ///
        ///
        ///     private static void OnWillEraseTile(WillEraseTileEventArgs args)
        ///     {
        ///         Debug.Log("Will erase tile at index: " + args.TileIndex);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public static event WillEraseTileEventHandler WillEraseTile;

        internal static void RaiseWillEraseTileEvent(TileSystem system, TileIndex index, TileData tile)
        {
            if (WillEraseTile != null) {
                WillEraseTile(new WillEraseTileEventArgs(system, index, tile));
            }
        }

        /// <summary>
        /// Occurs when new chunk is created.
        /// </summary>
        /// <remarks>
        /// <para>This event can be consumed by both editor and runtime scripts to
        /// manipulate new chunks as they are created. This can be used to automatically
        /// add new components on creation.</para>
        /// <para>Do not forget to remove event handlers when they are no longer required
        /// to avoid memory related issues.</para>
        /// </remarks>
        /// <example>
        /// <para>The following example demonstrates how to add a new component to chunks
        /// as they are created.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     private void Awake()
        ///     {
        ///         PaintingUtility.ChunkCreated += this.OnChunkCreated;
        ///     }
        ///
        ///     private void OnDestroy()
        ///     {
        ///         PaintingUtility.ChunkCreated -= this.OnChunkCreated;
        ///     }
        ///
        ///     private void OnChunkCreated(ChunkCreatedEventArgs args)
        ///     {
        ///         args.GameObject.AddComponent<CustomBehaviour>();
        ///     }
        /// }
        /// ]]></code>
        ///
        /// <para>This event can also be utilised by custom editor scripts:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// [InitializeOnLoad]
        /// static class MyCustomEditor
        /// {
        ///     static MyCustomEditor()
        ///     {
        ///         PaintingUtility.ChunkCreated += OnChunkCreated;
        ///     }
        ///
        ///
        ///     private static void OnChunkCreated(ChunkCreatedEventArgs args)
        ///     {
        ///         args.GameObject.AddComponent<CustomBehaviour>();
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public static event ChunkCreatedEventHandler ChunkCreated;

        internal static void RaiseChunkCreatedEvent(TileSystem system, Chunk chunk, TileIndex index)
        {
            if (ChunkCreated != null) {
                ChunkCreated(new ChunkCreatedEventArgs(system, chunk, index));
            }
        }

        #endregion


        // Internal buffers for tile indices without needing lots of allocations.
        internal static List<TileIndex> s_TempLineIndices = new List<TileIndex>();
        private static List<TileIndex> s_TempNozzleIndices = new List<TileIndex>();
        private static List<TileIndex> s_PaintIndices = new List<TileIndex>();
        private static HashSet<TileIndex> s_ExploredIndices = new HashSet<TileIndex>(TileIndex.EqualityComparer);
        private static List<TileIndex> s_TempSingleTile = new List<TileIndex>(new TileIndex[] { TileIndex.zero });


        #region Point Circle Indices

        /// <summary>
        /// Get tile indices for filled circle of the specified radius.
        /// </summary>
        /// <param name="indices">List of which to output indices to; any existing
        /// indices will be removed from list.</param>
        /// <param name="index">Index of tile at center of circle.</param>
        /// <param name="radius">Radius of circle where 1 is the smallest value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="indices"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="PaintCircle"/>
        public static void GetCircleIndices(IList<TileIndex> indices, TileIndex index, int radius)
        {
            if (indices == null) {
                throw new ArgumentNullException("indices");
            }

            indices.Clear();

            radius = Mathf.Max(0, radius - 1);

            int f = 1 - radius;
            int ddF_x = 1;
            int ddF_y = -2 * radius;
            int x = 0;
            int y = radius;

            GetCircleLineIndices(indices, index.row + radius, index.column, index.row - radius, index.column);
            GetCircleLineIndices(indices, index.row, index.column + radius, index.row, index.column - radius);

            while (x < y) {
                // ddF_x == 2 * x + 1;
                // ddF_y == -2 * y;
                // f == x*x + y*y - radius*radius + 2*x - y + 1;
                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                GetCircleLineIndices(indices, index.row + y, index.column + x, index.row + y, index.column - x);
                GetCircleLineIndices(indices, index.row - y, index.column + x, index.row - y, index.column - x);
                GetCircleLineIndices(indices, index.row + x, index.column + y, index.row + x, index.column - y);
                GetCircleLineIndices(indices, index.row - x, index.column + y, index.row - x, index.column - y);
            }
        }

        private static void GetCircleLineIndices(IList<TileIndex> indices, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            int dx = Mathf.Abs(toColumn - fromColumn);
            int dy = Mathf.Abs(toRow - fromRow);

            int sx = (fromColumn < toColumn)
                ? 1
                : -1;
            int sy = (fromRow < toRow)
                ? 1
                : -1;

            int err = dx - dy;

            // It's a little more efficient to avoid using the TileIndex constructor within
            // the main loop, so let's just use TileIndex from the outset!
            TileIndex from;
            from.row = fromRow;
            from.column = fromColumn;

            while (true) {
                indices.Add(from);

                if (from.column == toColumn && from.row == toRow) {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy) {
                    err -= dy;
                    from.column += sx;
                }
                if (e2 < dx) {
                    err += dx;
                    from.row += sy;
                }
            }
        }

        /// <summary>
        /// Get tile indices for outline of circle of the specified radius.
        /// </summary>
        /// <param name="indices">List of which to output indices to; any existing
        /// indices will be removed from list.</param>
        /// <param name="index">Index of tile at center of circle.</param>
        /// <param name="radius">Radius of circle where 1 is the smallest value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="indices"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="PaintCircleOutline"/>
        public static void GetCircleOutlineIndices(IList<TileIndex> indices, TileIndex index, int radius)
        {
            if (indices == null) {
                throw new ArgumentNullException("indices");
            }

            indices.Clear();

            radius = Mathf.Max(0, radius - 1);

            int f = 1 - radius;
            int ddF_x = 1;
            int ddF_y = -2 * radius;
            int x = 0;
            int y = radius;

            indices.Add(new TileIndex(index.row + radius, index.column));
            indices.Add(new TileIndex(index.row - radius, index.column));
            indices.Add(new TileIndex(index.row, index.column + radius));
            indices.Add(new TileIndex(index.row, index.column - radius));

            while (x < y) {
                // ddF_x == 2 * x + 1;
                // ddF_y == -2 * y;
                // f == x*x + y*y - radius*radius + 2*x - y + 1;
                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                indices.Add(new TileIndex(index.row + y, index.column + x));
                indices.Add(new TileIndex(index.row + y, index.column - x));
                indices.Add(new TileIndex(index.row - y, index.column + x));
                indices.Add(new TileIndex(index.row - y, index.column - x));
                indices.Add(new TileIndex(index.row + x, index.column + y));
                indices.Add(new TileIndex(index.row + x, index.column - y));
                indices.Add(new TileIndex(index.row - x, index.column + y));
                indices.Add(new TileIndex(index.row - x, index.column - y));
            }
        }

        #endregion


        #region Point Square Indices

        /// <summary>
        /// Get tile indices for filled square of the specified size.
        /// </summary>
        /// <param name="indices">List of which to output indices to; any existing
        /// indices will be removed from list.</param>
        /// <param name="index">Index of tile at center of square.</param>
        /// <param name="size">Size of square where 1 is the smallest value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="indices"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="PaintSquare"/>
        public static void GetSquareIndices(IList<TileIndex> indices, TileIndex index, int size)
        {
            if (indices == null) {
                throw new ArgumentNullException("indices");
            }

            indices.Clear();

            size = Mathf.Max(1, size);

            int radius = (size - 1) / 2;
            index.row -= radius;
            index.column -= radius;

            TileIndex idx;

            for (int ir = 0; ir < size; ++ir)
                for (int ic = 0; ic < size; ++ic) {
                    idx.row = index.row + ir;
                    idx.column = index.column + ic;
                    indices.Add(idx);
                }
        }

        /// <summary>
        /// Get tile indices for outline of circle of the specified size.
        /// </summary>
        /// <param name="indices">List of which to output indices to; any existing
        /// indices will be removed from list.</param>
        /// <param name="index">Index of tile at center of square.</param>
        /// <param name="size">Size of square where 1 is the smallest value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="indices"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="PaintSquareOutline"/>
        public static void GetSquareOutlineIndices(IList<TileIndex> indices, TileIndex index, int size)
        {
            if (indices == null) {
                throw new ArgumentNullException("indices");
            }

            indices.Clear();

            size = Mathf.Max(1, size);

            int radius = (size - 1) / 2;
            index.row -= radius;
            index.column -= radius;

            TileIndex idx;

            // Top and bottom edges.
            for (int ic = 0; ic < size; ++ic) {
                idx.row = index.row;
                idx.column = index.column + ic;
                indices.Add(idx);

                if (size > 1) {
                    idx.row = index.row + size - 1;
                    indices.Add(idx);
                }
            }

            // Left and right edges.
            for (int ir = 1; ir < size - 1; ++ir) {
                idx.row = index.row + ir;
                idx.column = index.column;
                indices.Add(idx);

                if (size > 1) {
                    idx.column = index.column + size - 1;
                    indices.Add(idx);
                }
            }
        }

        #endregion


        #region Line and Stroke Indices

        /// <summary>
        /// Normalize line endings so that 'from' refers to the upper-left most tile and 'to'
        /// refers to the lower-right most tile.
        /// </summary>
        /// <example>
        /// <para>It is useful to normalize line endings when drawing lines to ensure that
        /// the exact same line is painted in reverse:</para>
        /// <code language="csharp"><![CDATA[
        /// var from = new TileIndex(10, 3);
        /// var to = new TileIndex(0, 0);
        /// PaintingUtility.NormalizeLineEndPoints(ref from, ref to);
        /// PaintingUtility.PaintLine(system, from, to, args);
        /// ]]></code>
        /// </example>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <returns>
        /// A value of <c>true</c> if line indices were normalized; otherwise a value of
        /// <c>false</c> indicates that no changes were made.
        /// </returns>
        public static bool NormalizeLineEndPoints(ref TileIndex from, ref TileIndex to)
        {
            if (from.column > to.column) {
                // Swap anchor and target for consistency.
                int temp = from.row;
                from.row = to.row;
                to.row = temp;

                temp = from.column;
                from.column = to.column;
                to.column = temp;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get tile indices for line.
        /// </summary>
        /// <param name="indices">List of which to output indices to; any existing
        /// indices will be removed from list.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="indices"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="NormalizeLineEndPoints"/>
        public static void GetLineIndices(IList<TileIndex> indices, TileIndex from, TileIndex to)
        {
            if (indices == null) {
                throw new ArgumentNullException("indices");
            }

            indices.Clear();

            int dx = Mathf.Abs(to.column - from.column);
            int dy = Mathf.Abs(to.row - from.row);

            int sx = (from.column < to.column)
                ? 1
                : -1;
            int sy = (from.row < to.row)
                ? 1
                : -1;

            int err = dx - dy;

            while (true) {
                indices.Add(from);

                if (from.column == to.column && from.row == to.row) {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy) {
                    err -= dy;
                    from.column += sx;
                }
                if (e2 < dx) {
                    err += dx;
                    from.row += sy;
                }
            }
        }

        /// <summary>
        /// Get tile indices for line stroked with the specified nozzle.
        /// </summary>
        /// <example>
        /// <para>Manually painting an extended line of circles:</para>
        /// <code language="csharp"><![CDATA[
        /// // Get indices of tiles to paint.
        /// var lineIndices = new List<TileIndex>();
        /// var nozzleIndices = new List<TileIndex>();
        /// PaintingUtility.GetCircleIndices(nozzleIndices, TileIndex.zero, 3);
        /// PaintingUtility.GetStrokeLineIndices(lineIndices, from, to, nozzleIndices);
        ///
        /// // Paint the tiles!
        /// var paintingArgs = PaintingArgs.GetDefaults(yourBrush);
        /// PaintingUtility.Paint(system, lineIndices, paintingArgs);
        /// ]]></code>
        /// </example>
        /// <param name="indices">List of which to output indices to; any existing
        /// indices will be removed from list.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <param name="nozzleIndices">Nozzle indices should be set around origin (0, 0).</param>
        /// <seealso cref="NormalizeLineEndPoints"/>
        public static void GetStrokeLineIndices(IList<TileIndex> indices, TileIndex from, TileIndex to, IList<TileIndex> nozzleIndices)
        {
            indices.Clear();

            s_TempLineIndices.Clear();
            GetLineIndices(s_TempLineIndices, from, to);

            int niEnd = nozzleIndices.Count;
            TileIndex idx;

            s_ExploredIndices.Clear();

            foreach (var lineIndex in s_TempLineIndices) {
                for (int ni = 0; ni < niEnd; ++ni) {
                    idx = nozzleIndices[ni];
                    idx.row += lineIndex.row;
                    idx.column += lineIndex.column;

                    // It is significantly faster to use a HashSet to ensure that the output
                    // collection contains distinct indices.
                    if (!s_ExploredIndices.Contains(idx)) {
                        indices.Add(idx);
                        s_ExploredIndices.Add(idx);
                    }
                }
            }

            s_ExploredIndices.Clear();
        }

        #endregion


        #region Filtering and Fill Rate

        /// <summary>
        /// Get filtered list of paintable tiles excluding non-paintable and out-of-range tiles
        /// from input list of indices.
        /// </summary>
        /// <param name="system">Tile system for bounds.</param>
        /// <param name="indices">List of tile indices.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <returns>
        /// Filtered list of tiles.
        /// </returns>
        private static IList<TileIndex> FilterTiles(TileSystem system, IList<TileIndex> indices, PaintingArgs args)
        {
            // s_TempNozzleIndices is not being used right now, so may as well use this!
            s_TempNozzleIndices.Clear();

            int count = indices.Count;
            if (count > 0) {
                for (int i = 0; i < count; ++i) {
                    var idx = indices[i];

                    // Is tile within bounds of tile system?
                    if (!system.InBounds(idx)) {
                        continue;
                    }

                    // Get current state of tile.
                    var tile = system.GetTile(idx.row, idx.column);

                    if (args.brush != null) {
                        // Consider tile empty if it is utterly useless!
                        bool isTileEmpty = (tile == null || tile.brush == null || tile.IsGameObjectMissing);

                        // Should we paint around existing tiles?
                        if (!isTileEmpty && args.paintAroundExistingTiles) {
                            continue;
                        }
                    }
                    else {
                        // Skip if the tile is already empty!
                        if (tile == null) {
                            continue;
                        }
                    }

                    // So we are interested in painting this index!
                    s_TempNozzleIndices.Add(idx);
                }

                // Do we need to reduce the number of tiles for the fill rate?
                if (args.fillRatePercentage < 100) {
                    FilterIndicesForFillRate(s_TempNozzleIndices, args.fillRatePercentage);
                }
            }

            return s_TempNozzleIndices;
        }

        private static float EaseCubicIn(float currentTime, float startValue, float totalChange, float duration)
        {
            return startValue + totalChange * ((currentTime /= duration) * currentTime * currentTime);
        }

        /// <summary>
        /// Apply filter to remove excess indices when a lower fill rate has been specified.
        /// </summary>
        /// <param name="indices">List of tile indices.</param>
        /// <param name="fillRatePercentage">A value ranging between 0 and 100 (inclusive).</param>
        private static void FilterIndicesForFillRate(List<TileIndex> indices, int fillRatePercentage)
        {
            // Convert fill rate into a 0-1 value (rather than 0-100) and apply Cubic In Easing
            float fillRate = Mathf.Max(0.0025f, (Mathf.Clamp(fillRatePercentage, 0, 100) / 100f));
            fillRate = EaseCubicIn(fillRate, 0f, 1f, 1f);
            int totalCount = indices.Count;
            int fillCount = Mathf.CeilToInt(totalCount * fillRate);

            // Pick random selection of tiles!
            if (indices.Count > fillCount) {
                for (int i = 0; i < fillCount; ++i) {
                    int randomIndex = Random.Range(i, totalCount);
                    TileIndex temp = indices[i];
                    indices[i] = indices[randomIndex];
                    indices[randomIndex] = temp;
                }

                // Remove excess!
                indices.RemoveRange(fillCount, totalCount - fillCount);
            }
        }

        #endregion


        #region Painting

        /// <summary>
        /// Paint filled circle of tiles of the specified radius.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile at center of circle.</param>
        /// <param name="radius">Radius of circle where 1 is the smallest value.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetCircleIndices"/>
        public static void PaintCircle(TileSystem system, TileIndex index, int radius, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            GetCircleIndices(s_PaintIndices, index, radius);
            Paint(system, s_PaintIndices, args);
        }

        /// <summary>
        /// Paint outline of circle of tiles of the specified radius.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile at center of circle.</param>
        /// <param name="radius">Radius of circle where 1 is the smallest value.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetCircleIndices"/>
        public static void PaintCircleOutline(TileSystem system, TileIndex index, int radius, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            GetCircleOutlineIndices(s_PaintIndices, index, radius);
            Paint(system, s_PaintIndices, args);
        }

        /// <summary>
        /// Paint filled square of tiles of the specified size.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile at center of square.</param>
        /// <param name="size">Size of square where 1 is the smallest value.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetSquareIndices"/>
        public static void PaintSquare(TileSystem system, TileIndex index, int size, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            GetSquareIndices(s_PaintIndices, index, size);
            Paint(system, s_PaintIndices, args);
        }

        /// <summary>
        /// Paint outline of square of tiles of the specified size.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile at center of square.</param>
        /// <param name="size">Size of square where 1 is the smallest value.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetSquareOutlineIndices"/>
        public static void PaintSquareOutline(TileSystem system, TileIndex index, int size, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            GetSquareOutlineIndices(s_PaintIndices, index, size);
            Paint(system, s_PaintIndices, args);
        }

        /// <summary>
        /// Paint simple line of tiles.
        /// </summary>
        /// <remarks>
        /// <para>Line is automatically normalized to ensure end points are commutative.
        /// This means that the same tiles are painted even when end points are reversed.</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetLineIndices"/>
        public static void PaintLine(TileSystem system, TileIndex from, TileIndex to, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            if (from == to) {
                Paint(system, from, args);
            }
            else {
                NormalizeLineEndPoints(ref from, ref to);
                GetLineIndices(s_PaintIndices, from, to);
                Paint(system, s_PaintIndices, args);
            }
        }

        /// <summary>
        /// Stroke line with custom nozzle shape.
        /// </summary>
        /// <remarks>
        /// <para>Line is automatically normalized to ensure end points are commutative.
        /// This means that the same tiles are painted even when end points are reversed.</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <param name="nozzleIndices">Nozzle indices should be set around origin (0, 0).</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetStrokeLineIndices"/>
        public static void StrokeLine(TileSystem system, TileIndex from, TileIndex to, IList<TileIndex> nozzleIndices, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            switch (nozzleIndices.Count) {
                case 0:
                    return;

                case 1:
                    PaintLine(system, from, to, args);
                    break;

                default:
                    NormalizeLineEndPoints(ref from, ref to);
                    GetStrokeLineIndices(s_PaintIndices, from, to, nozzleIndices);
                    Paint(system, s_PaintIndices, args);
                    break;
            }
        }

        /// <summary>
        /// Stroke line with a circle nozzle shape.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <param name="radius">Radius of circle where 1 is the smallest value.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetCircleIndices"/>
        /// <seealso cref="StrokeLine"/>
        public static void StrokeLineWithCircle(TileSystem system, TileIndex from, TileIndex to, int radius, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            GetCircleIndices(s_TempNozzleIndices, TileIndex.zero, radius);
            StrokeLine(system, from, to, s_TempNozzleIndices, args);
        }

        /// <summary>
        /// Stroke line with a square nozzle shape.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <param name="size">Size of square where 1 is the smallest value.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="GetSquareIndices"/>
        /// <seealso cref="StrokeLine"/>
        public static void StrokeLineWithSquare(TileSystem system, TileIndex from, TileIndex to, int size, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            GetSquareIndices(s_TempNozzleIndices, TileIndex.zero, size);
            StrokeLine(system, from, to, s_TempNozzleIndices, args);
        }

        /// <summary>
        /// Paint single tile at the specified index.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        public static void Paint(TileSystem system, TileIndex index, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            s_TempSingleTile[0] = index;
            Paint(system, s_TempSingleTile, args);
        }

        /// <summary>
        /// Paint rectangle of tiles and optionally fill inner area.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of first corner of rectangle.</param>
        /// <param name="to">Index of second corner of rectangle.</param>
        /// <param name="fill">Indicates whether inner area should be filled. Paints outline
        /// of rectangle when <c>false</c> is specified.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        public static void PaintRectangle(TileSystem system, TileIndex from, TileIndex to, bool fill, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            MathUtility.GetRectangleBoundsClamp(system, from, to, out from, out to);

            s_PaintIndices.Clear();

            TileIndex idx;

            for (idx.row = from.row; idx.row <= to.row; ++idx.row)
                for (idx.column = from.column; idx.column <= to.column; ++idx.column) {
                    // Fill center area of rectangle?
                    if (!fill && (idx.column != from.column && idx.column != to.column && idx.row != from.row && idx.row != to.row)) {
                        continue;
                    }

                    s_PaintIndices.Add(idx);
                }

            Paint(system, s_PaintIndices, args);
        }

        /// <summary>
        /// Paint multiple tiles.
        /// </summary>
        /// <remarks>
        /// <para>Bulk editing mode is assumed when more than one tile is painted or erased.</para>
        /// </remarks>
        /// <example>
        /// <para>Remember that bulk editing mode can be nested allowing even more paint/erase
        /// operations to be optimized outside the scope of this function:</para>
        /// <code language="csharp"><![CDATA[
        /// system.BeginBulkEdit();
        ///     PaintingUtility.Paint(system, indicesListA, paintingArgs);
        ///     PaintingUtility.Paint(system, indicesListB, paintingArgs);
        /// system.EndBulkEdit();
        /// ]]></code>
        /// </example>
        /// <param name="system">Tile system.</param>
        /// <param name="indices">List of tile indices.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <list type="bullet">
        /// <item>If <paramref name="system"/> is <c>null</c>.</item>
        /// <item>If <paramref name="indices"/> is <c>null</c>.</item>
        /// </list>
        /// </exception>
        public static void Paint(TileSystem system, IList<TileIndex> indices, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }
            if (indices == null) {
                throw new ArgumentNullException("indices");
            }

            // Reduce the effectiveness of painting when the fill rate is reduced.
            indices = FilterTiles(system, indices, args);

            int count = indices.Count;
            if (count == 0) {
                return;
            }

            int rotation = args.rotation;

            try {
                system.BeginBulkEdit();

                for (int i = 0; i < count; ++i) {
                    var idx = indices[i];

                    if (args.brush != null) {
                        // Painting tile using brush!
                        if (args.randomizeRotation) {
                            rotation = Random.Range(0, 4);
                        }

                        args.brush.PaintWithSimpleRotation(system, idx.row, idx.column, rotation, 0);
                    }
                    else {
                        // Erasing the tile!
                        system.EraseTile(idx.row, idx.column);
                    }
                }

                // Apply variation shift to painted tiles?
                if (args.brush != null) {
                    ApplyVariationIndices(system, indices, args);
                }
            }
            finally {
                system.EndBulkEdit();
            }
        }

        private static void ApplyVariationIndices(TileSystem system, IList<TileIndex> indices, PaintingArgs args)
        {
            // This method is only called when tiles are being painted.

            int count = indices.Count;
            for (int i = 0; i < count; ++i) {
                var tile = system.GetTile(indices[i]);
                if (tile == null || tile.brush == null) {
                    continue;
                }

                // Since we are in bulk editing mode (and mulitple tiles may have been painted)
                // the final resolved orientation may differ from that found inside tile data.
                int actualOrientationMask = OrientationUtility.DetermineTileOrientation(system, indices[i], tile.brush, tile.PaintedRotation);

                byte resolvedVariationIndex = (byte)args.ResolveVariation(actualOrientationMask);

                //
                // When tiles are painted using `PaintWithSimpleRotation` the method
                // avoids repainting tiles that do not appear to have been changed to
                // improve performance. However, the orientation of a tile changes as
                // new ones are painted and thus the resolved variation may not match
                // after additional tiles are painted.
                //
                // To resolve this issue it is important to verify whether the tile
                // has become dirty AFTER all batched tiles have been painted.
                //
                // This works since tiles are painted using bulk edit mode which means
                // that the physical representation has yet to be generated.
                //
                if (!tile.Dirty && resolvedVariationIndex != tile.variationIndex) {
                    var chunk = system.GetChunkFromTileIndex(indices[i]);
                    tile.Dirty = true;
                    chunk.Dirty = true;
                }

                // Adjust variation as needed.
                tile.variationIndex = resolvedVariationIndex;
            }
        }

        #endregion


        #region Flood Fill

        private static int s_MaximumFillCount = 300;

        /// <summary>
        /// Gets or sets the maximum number of tiles which can be painted when using <see cref="FloodFill"/>.
        /// This is a fail safe which can help to avoid crashing your game (or the Unity editor)
        /// when filling a large area. This does <strong>not</strong> apply when erasing tiles.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///    <item>The default value of this property is 300.</item>
        ///    <item>Specify value of 0 to disable maximum fill count.</item>
        /// </list>
        /// </remarks>
        public static int MaximumFillCount {
            get { return s_MaximumFillCount; }
            set { s_MaximumFillCount = Mathf.Max(0, value); }
        }


        private static void FloodFillHelper(TileSystem system, TileIndex index, PaintingArgs args, IList<TileIndex> filledIndices)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }
            if (filledIndices == null) {
                throw new ArgumentNullException("filledIndices");
            }
            if (filledIndices.Count != 0) {
                throw new ArgumentException("List is not empty.", "filledIndices");
            }

            Brush targetBrush = null;
            int targetRotation = 0;

            // Learn about target tile at specified index.
            var tile = system.GetTileOrNull(index);
            if (tile != null) {
                if (tile.brush == null) {
                    // Skip tile because it does not contain a brush.
                    Debug.LogWarning("Cannot fill non-empty tile because it doesn't contain a valid brush.");
                    return;
                }

                targetBrush = tile.brush;
                targetRotation = tile.PaintedRotation;
            }

            // Skip fill when no changes will be made.
            if (targetBrush == args.brush && (tile == null || tile.PaintedRotation == args.rotation)) {
                return;
            }
            // We cannot perform fill since tile is already present!
            if (args.brush != null && targetBrush != null && args.paintAroundExistingTiles) {
                return;
            }

            // Let's perform the flood fill!
            system.BeginBulkEdit();
            try {
                var queue = new Queue<TileIndex>();
                queue.Enqueue(index);

                int paintCount = 0;
                while (queue.Count > 0 && paintCount <= MaximumFillCount) {
                    index = queue.Dequeue();

                    // Only proceed if tile is to be painted.
                    tile = system.GetTileOrNull(index.row, index.column);
                    if (tile == null) {
                        if (targetBrush != null) {
                            continue;
                        }
                    }
                    else if (tile.brush != targetBrush || tile.PaintedRotation != targetRotation) {
                        continue;
                    }

                    // Paint replacement tile.
                    if (args.brush != null) {
                        if (args.randomizeRotation) {
                            args.rotation = Random.Range(0, 4);
                        }

                        filledIndices.Add(index);

                        args.brush.PaintWithSimpleRotation(system, index.row, index.column, args.rotation, 0);
                        if (MaximumFillCount != 0) {
                            ++paintCount;
                        }
                    }
                    else {
                        system.EraseTile(index.row, index.column);
                    }

                    // Proceed with filling!
                    if (index.row > 0) {
                        queue.Enqueue(new TileIndex(index.row - 1, index.column));
                    }
                    if (index.row + 1 < system.RowCount) {
                        queue.Enqueue(new TileIndex(index.row + 1, index.column));
                    }
                    if (index.column > 0) {
                        queue.Enqueue(new TileIndex(index.row, index.column - 1));
                    }
                    if (index.column + 1 < system.ColumnCount) {
                        queue.Enqueue(new TileIndex(index.row, index.column + 1));
                    }
                }

                ApplyVariationIndices(system, filledIndices, args);
            }
            finally {
                system.EndBulkEdit();
            }
        }

        /// <summary>
        /// Flood fill area of tile system.
        /// </summary>
        /// <remarks>
        /// <para>By default a maximum of 300 tiles will be considered when performing the
        /// flood fill. This is a safety mechanism but the upper threshold can be customized
        /// by adjusting the value of <see cref="MaximumFillCount"/>. This threshold is not
        /// applied when erasing tiles.</para>
        /// <para>Logs warning message to console when attempting to fill a non-empty tile
        /// which does not reference a brush. This scenario can occur when attempting to
        /// fill tiles on a tile system where brush references have been stripped.</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Target index at which to begin filling from.</param>
        /// <param name="args">Arguments to customize painting of tiles.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        /// <seealso cref="MaximumFillCount"/>
        public static void FloodFill(TileSystem system, TileIndex index, PaintingArgs args)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            try {
                s_PaintIndices.Clear();
                FloodFillHelper(system, index, args, s_PaintIndices);
            }
            finally {
                s_PaintIndices.Clear();
            }
        }

        /// <inheritdoc cref="FloodFill(TileSystem, TileIndex, PaintingArgs)"/>
        /// <example>
        /// <para>Fill area with tiles and retrieve list of painted indices:</para>
        /// <code language="csharp"><![CDATA[
        /// var filledIndices = new List<TileIndex>();
        /// PaintingUtility.FloodFill(system, index, args, filledIndices);
        /// Debug.Log(string.Format("Filled {0} tiles.", filledIndices.Count));
        /// ]]></code>
        /// </example>
        /// <param name="filledIndices">Tile indices are added to the specified list as
        /// they are filled or erased by this method; any existing indices will be
        /// removed from the collection.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <list type="bullet">
        /// <item>If <paramref name="system"/> is <c>null</c>.</item>
        /// <item>If <paramref name="filledIndices"/> is <c>null</c>.</item>
        /// </list>
        /// </exception>
        public static void FloodFill(TileSystem system, TileIndex index, PaintingArgs args, IList<TileIndex> filledIndices)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }
            if (filledIndices == null) {
                throw new ArgumentNullException("filledIndices");
            }

            filledIndices.Clear();
            FloodFillHelper(system, index, args, filledIndices);
        }

        #endregion
    }
}
