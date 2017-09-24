// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Arguments for <see cref="TilePaintedEventHandler"/> event handlers.
    /// </summary>
    public struct TilePaintedEventArgs
    {
        internal TilePaintedEventArgs(TileSystem system, TileIndex index, TileData tile)
        {
            this.TileSystem = system;
            this.TileIndex = index;
            this.TileData = tile;
        }


        /// <summary>
        /// The tile system that was painted on.
        /// </summary>
        public readonly TileSystem TileSystem;
        /// <summary>
        /// Zero-based index of tile that was painted.
        /// </summary>
        public readonly TileIndex TileIndex;
        /// <summary>
        /// Data for tile that was painted.
        /// </summary>
        public readonly TileData TileData;


        /// <summary>
        /// Gets brush that was used to paint tile.
        /// </summary>
        public Brush Brush {
            get { return this.TileData.brush; }
        }
        /// <summary>
        /// Gets game object that is associated with tile (if any).
        /// </summary>
        public GameObject GameObject {
            get { return this.TileData.gameObject; }
        }
    }


    /// <summary>
    /// Represents the method that will handle the <see cref="PaintingUtility.TilePainted"/> event.
    /// </summary>
    /// <param name="args">Event arguments.</param>
    public delegate void TilePaintedEventHandler(TilePaintedEventArgs args);


    /// <summary>
    /// Arguments for <see cref="WillEraseTileEventHandler"/> event handlers.
    /// </summary>
    public struct WillEraseTileEventArgs
    {
        internal WillEraseTileEventArgs(TileSystem system, TileIndex index, TileData tile)
        {
            this.TileSystem = system;
            this.TileIndex = index;
            this.TileData = tile;
        }


        /// <summary>
        /// The tile system that is being manipulated.
        /// </summary>
        public readonly TileSystem TileSystem;
        /// <summary>
        /// Zero-based index of tile that will be erased.
        /// </summary>
        public readonly TileIndex TileIndex;
        /// <summary>
        /// Data for tile that will be erased.
        /// </summary>
        public readonly TileData TileData;


        /// <summary>
        /// Gets brush that was used to paint tile.
        /// </summary>
        public Brush Brush {
            get { return this.TileData.brush; }
        }
        /// <summary>
        /// Gets game object that is associated with tile (if any).
        /// </summary>
        public GameObject GameObject {
            get { return this.TileData.gameObject; }
        }
    }


    /// <summary>
    /// Represents the method that will handle the <see cref="PaintingUtility.WillEraseTile"/> event.
    /// </summary>
    /// <param name="args">Event arguments.</param>
    public delegate void WillEraseTileEventHandler(WillEraseTileEventArgs args);


    /// <summary>
    /// Arguments for <see cref="ChunkCreatedEventHandler"/> event handlers.
    /// </summary>
    public struct ChunkCreatedEventArgs
    {
        internal ChunkCreatedEventArgs(TileSystem system, Chunk chunk, TileIndex index)
        {
            this.TileSystem = system;
            this.Chunk = chunk;
            this.SeedTileIndex = index;
        }


        /// <summary>
        /// The tile system where chunk has been created.
        /// </summary>
        public readonly TileSystem TileSystem;
        /// <summary>
        /// The chunk that was created.
        /// </summary>
        public readonly Chunk Chunk;
        /// <summary>
        /// Zero-based index of tile that was painted leading to creation of chunk.
        /// </summary>
        public readonly TileIndex SeedTileIndex;


        /// <summary>
        /// Gets game object of chunk.
        /// </summary>
        public GameObject GameObject {
            get { return this.Chunk.gameObject; }
        }
    }


    /// <summary>
    /// Represents the method that will handle the <see cref="PaintingUtility.ChunkCreated"/> event.
    /// </summary>
    /// <param name="args">Event arguments.</param>
    public delegate void ChunkCreatedEventHandler(ChunkCreatedEventArgs args);
}
