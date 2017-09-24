// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile
{
    /// <summary>
    /// Utility class provides functionality for optimizing 2D and 3D box colliders within
    /// tiles for the specified tile system. Behaviour can be customized on a per tile system
    /// basis by altering <see cref="TileSystem.ReduceColliders">TileSystem.ReduceColliders</see>.
    /// </summary>
    public sealed class ReduceColliderUtility
    {
        /// <summary>
        /// Attempt to reduce number of colliders inside input tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        public static void Optimize(TileSystem system)
        {
            var combiner = new ReduceColliderUtility(system);
            combiner.Execute();
        }


        #region Collider Information

        /// <summary>
        /// Collider information for a specific tile.
        /// </summary>
        private sealed class ColliderInfo
        {
            #region Pooling

            private static Stack<ColliderInfo> s_InfoPool = new Stack<ColliderInfo>();


            /// <summary>
            /// Spawn <see cref="ColliderInfo"/> instance from pool.
            /// </summary>
            /// <remarks>
            /// <para>Value of <see cref="Bounds"/> is not reset from previous usage; so it
            /// is important to manually update this to the desired value.</para>
            /// </remarks>
            /// <param name="tile">Data for associated tile.</param>
            /// <param name="row">Zero-based index of tile row within system.</param>
            /// <param name="column">Zero-based index of tile column within system/</param>
            /// <param name="type">Type of collider.</param>
            /// <returns>
            /// The <see cref="ColliderInfo"/> instance.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">
            /// If <paramref name="tile"/> is <c>null</c>.
            /// </exception>
            public static ColliderInfo Spawn(TileData tile, int row, int column, ColliderType type)
            {
                if (tile == null) {
                    throw new ArgumentNullException("tile");
                }

                var info = (s_InfoPool.Count != 0 ? s_InfoPool.Pop() : new ColliderInfo());

                info.Tile = tile;
                info.Row = row;
                info.Column = column;

                info.Type = type;
                //info.bounds = default(BoxBounds); // No point in wasting CPU
                info.IsTrigger = false;
                info.Material = null;
                info.Collider = null;

                return info;
            }

            /// <summary>
            /// Return unwanted <see cref="ColliderInfo"/> instance to pool for recyling.
            /// </summary>
            /// <param name="info">Unwanted collider information instance.</param>
            public static void Despawn(ColliderInfo info)
            {
                if (info != null) {
                    s_InfoPool.Push(info);
                }
            }

            #endregion


            /// <summary>
            /// Associated tile data.
            /// </summary>
            public TileData Tile;
            /// <summary>
            /// Zero-based index of tile row within system.
            /// </summary>
            public int Row;
            /// <summary>
            /// Zero-based index of tile column within system.
            /// </summary>
            public int Column;

            /// <summary>
            /// Type of collider.
            /// </summary>
            public ColliderType Type;
            /// <summary>
            /// Bounds of collider in local space of tile system.
            /// </summary>
            public BoxBounds Bounds;
            /// <summary>
            /// Indicates if this is a trigger collider.
            /// </summary>
            public bool IsTrigger;
            /// <summary>
            /// The associated <see cref="PhysicMaterial"/> or <see cref="PhysicsMaterial2D"/>
            /// instance (if any).
            /// </summary>
            public Object Material;
            /// <summary>
            /// The collider component or a value of <c>null</c> for hypothetical colliders.
            /// </summary>
            public Component Collider;
        }


        /// <summary>
        /// Transform minimum and maximum bounds from one space to another. For example,
        /// this is useful to find the minimum and maximum bounds of a tile collider
        /// within local space of tile system.
        /// </summary>
        /// <param name="bounds">Bounds that are to be transformed.</param>
        /// <param name="mat">Space-to-space transformation matrix.</param>
        /// <returns>
        /// The transformed bounds.
        /// </returns>
        private static BoxBounds SpaceToSpace(BoxBounds bounds, Matrix4x4 mat)
        {
            return new BoxBounds(
                mat.MultiplyPoint3x4(bounds.Min),
                mat.MultiplyPoint3x4(bounds.Max)
            );
        }

        /// <summary>
        /// Get collider information from specific tile within tile system. Assigns a value
        /// of <c>null</c> to <paramref name="info"/> if tile does not contain a collider.
        /// </summary>
        /// <param name="info">Reference to variable that will contain collider information
        /// once this method has returned. Any existing <see cref="ColliderInfo"/> instance
        /// will be despawned.</param>
        /// <param name="row">Zero-based index of tile row.</param>
        /// <param name="column">Zero-based index of tile column.</param>
        private void GetColliderInfo(ref ColliderInfo info, int row, int column)
        {
            // Automatically return prior collider information to pool for reuse later.
            if (info != null) {
                ColliderInfo.Despawn(info);
                info = null;
            }

            // Only consider collider information for non-empty tiles which were painted
            // using a brush which has been marked "Static".
            var tile = this.system.GetTile(row, column);
            if (tile == null || tile.brush == null || !tile.brush.Static) {
                return;
            }

            // Gather information from collider component?
            if (tile.gameObject != null) {
                var boxCollider3D = tile.gameObject.GetComponentInChildren<BoxCollider>();
                if (boxCollider3D != null) {
                    info = ColliderInfo.Spawn(tile, row, column, ColliderType.BoxCollider3D);
                    this.GatherInfo_BoxCollider3D(info, boxCollider3D);
                    return;
                }
                else {
                    var boxCollider2D = tile.gameObject.GetComponentInChildren<BoxCollider2D>();
                    if (boxCollider2D != null) {
                        info = ColliderInfo.Spawn(tile, row, column, ColliderType.BoxCollider2D);
                        this.GatherInfo_BoxCollider2D(info, boxCollider2D);
                        return;
                    }
                }
            }

            // Assume collider due to state of 'Solid' flag?
            if (this.system.ReduceColliders.IncludeSolidTiles && tile.SolidFlag) {
                info = ColliderInfo.Spawn(tile, row, column, this.system.ReduceColliders.SolidTileColliderType);
                this.GatherInfo_SolidFlag(info);
                return;
            }
        }

        /// <summary>
        /// Gather collider information from 3D <see cref="BoxCollider"/> component.
        /// </summary>
        /// <param name="info">Collider information instance.</param>
        /// <param name="collider">Component which resides somewhere within tile.</param>
        private void GatherInfo_BoxCollider3D(ColliderInfo info, BoxCollider collider)
        {
            var tileToSystemMatrix = this.worldToSystem * collider.transform.localToWorldMatrix;

            info.IsTrigger = collider.isTrigger;
            info.Material = collider.sharedMaterial;
            info.Collider = collider;
            info.Bounds = SpaceToSpace(BoxBounds.FromBounds(collider.center, collider.size), tileToSystemMatrix);
        }

        /// <summary>
        /// Gather collider information from <see cref="BoxCollider2D"/> component.
        /// </summary>
        /// <param name="info">Collider information instance.</param>
        /// <param name="collider">Component which resides somewhere within tile.</param>
        private void GatherInfo_BoxCollider2D(ColliderInfo info, BoxCollider2D collider)
        {
            var tileToSystemMatrix = this.worldToSystem * collider.transform.localToWorldMatrix;

            // Bounds of 2D colliders need to be consistent.
            Vector3 boundsSize = collider.size;
            boundsSize.z = 1f;

            Vector3 center = collider.offset;

            info.IsTrigger = collider.isTrigger;
            info.Material = collider.sharedMaterial;
            info.Collider = collider;
            info.Bounds = SpaceToSpace(BoxBounds.FromBounds(center, boundsSize), tileToSystemMatrix);
        }

        /// <summary>
        /// Gather hypothetical collider information from solid flag.
        /// </summary>
        /// <param name="info">Collider information instance.</param>
        private void GatherInfo_SolidFlag(ColliderInfo info)
        {
            info.IsTrigger = false;

            // Bounds of 2D colliders need to be consistent.
            Vector3 boundsSize = this.system.CellSize;
            if (info.Type == ColliderType.BoxCollider2D) {
                boundsSize.z = 1f;
            }

            info.Bounds = BoxBounds.FromBounds(this.system.LocalPositionFromTileIndex(info.Row, info.Column), boundsSize);
        }

        /// <inheritdoc cref="this.GetColliderInfo(ref ColliderInfo, int, int)"/>
        /// <param name="index">Index of tile.</param>
        private void GetColliderInfo(ref ColliderInfo info, TileIndex index)
        {
            this.GetColliderInfo(ref info, index.row, index.column);
        }

        #endregion


        // The tile system which is being optimized.
        private TileSystem system;

        // Local cache of tile system matrices to improve performance a little.
        private Matrix4x4 worldToSystem;
        private Matrix4x4 systemToWorld;

        // Combiner options extracted from tile system 'ReduceColliders' flags.
        private bool separateByTag;
        private bool separateByLayer;


        // Tiles are marked once they have been reduced to avoid needlessly reconsidering
        // each tile multiple times. Once a tile has been reduced no further optimizations
        // are applied.
        private bool[,] markedTiles;


        /// <summary>
        /// Initialize new <see cref="ReduceColliderUtility"/> instance.
        /// </summary>
        /// <param name="system">Tile system which is to be combined.</param>
        private ReduceColliderUtility(TileSystem system)
        {
            this.system = system;

            this.worldToSystem = system.transform.worldToLocalMatrix;
            this.systemToWorld = system.transform.localToWorldMatrix;

            this.separateByTag = (system.ReduceColliders.KeepSeparate & KeepSeparateColliderFlag.ByTag) != 0;
            this.separateByLayer = (system.ReduceColliders.KeepSeparate & KeepSeparateColliderFlag.ByLayer) != 0;
        }


        /// <summary>
        /// Execute collider reduction on associated tile system.
        /// </summary>
        private void Execute()
        {
            // Temporarily adjust default error threshold of `BoxBounds` to threshold
            // specified for collider reduction in tile system.
            float restoreErrorThreshold = BoxBounds.ErrorThreshold;
            BoxBounds.ErrorThreshold = this.system.ReduceColliders.SnapThreshold;

            try {
                this.markedTiles = new bool[this.system.RowCount, this.system.ColumnCount];

                // Consider each non-marked tile as anchor and then scan rightwards and
                // then downwards for tiles which can be reduced with anchor.

                TileIndex anchor = new TileIndex();
                for (anchor.row = 0; anchor.row < this.system.RowCount; ++anchor.row) {
                    for (anchor.column = 0; anchor.column < this.system.ColumnCount; ++anchor.column) {
                        if (!this.markedTiles[anchor.row, anchor.column] && this.system.GetTile(anchor) != null) {
                            anchor.column = this.Scan(anchor);
                        }
                    }
                }
            }
            finally {
                BoxBounds.ErrorThreshold = restoreErrorThreshold;
            }
        }

        /// <summary>
        /// Determine whether input colliders could be reduced assuming that they
        /// are geometrically compatible.
        /// </summary>
        /// <remarks>
        /// <para>Colliders cannot be reduced under the following circumstances:</para>
        /// <list type="bullet">
        /// <item>Either <paramref name="a"/> or <paramref name="b"/> was a value of <c>null</c>
        /// indicating that at least one of the parameters represents a tile which does not
        /// contain a collider that can be reduced.</item>
        /// <item><paramref name="a"/> and <paramref name="b"/> represent different types of
        /// collider; for instance, a <see cref="BoxCollider"/> and a <see cref="BoxCollider2D"/>
        /// are not compatible.</item>
        /// <item>One collider represents a trigger whilst the other doesn't. These cannot be
        /// combined since it would affect game behaviour.</item>
        /// <item>Input colliders have a different physics material which cannot be combined
        /// since it would affect game behaviour.</item>
        /// <item><see cref="TileSystem.ReduceColliders">TileSystem.ReduceColliders</see> can
        /// prevent colliders from being reduced if they have a different tag or layer.</item>
        /// </list>
        /// </remarks>
        /// <param name="a">Information for first collider.</param>
        /// <param name="b">Information for second collider.</param>
        /// <returns>
        /// A value of <c>true</c> if input colliders can be reduced; otherwise, a
        /// value of <c>false</c>.
        /// </returns>
        private bool CanReduce(ColliderInfo a, ColliderInfo b)
        {
            if (a == null || b == null || a.Type != b.Type || a.IsTrigger != b.IsTrigger || a.Material != b.Material) {
                return false;
            }

            var goA = a.Tile.gameObject;
            var goB = b.Tile.gameObject;

            // Hypothetical colliders can be reduced without having to determine whether
            // tags or layers are to be kept separate.
            if (goA == null || goB == null) {
                return true;
            }

            return (!this.separateByTag || goA.tag == goB.tag)
                && (!this.separateByLayer || goA.layer == goB.layer);
        }


        private List<Component> candidateColliders = new List<Component>();
        private List<Component> reducedColliderComponents = new List<Component>();
        private int reducedColliderCount;


        private void ResetReducedColliderTracking()
        {
            this.reducedColliderComponents.Clear();
            this.reducedColliderCount = 0;
        }

        private void TrackReducedCollider(int row, int column, Component collider)
        {
            if (collider != null) {
                this.reducedColliderComponents.Add(collider);
            }

            this.markedTiles[row, column] = true;
            ++this.reducedColliderCount;
        }

        /// <summary>
        /// Scans for and applies reductions if specified anchor tile contains a collider.
        /// </summary>
        /// <remarks>
        /// <para>This method searches from left-to-right from specified anchor tile to
        /// find furthest tile horizontally that it can be reduced with. Once the initial
        /// horizontal sequence is known this method proceeds to scan vertically for other
        /// sequences of colliders that can further reduce the initial horizontal sequence.</para>
        /// </remarks>
        /// <param name="anchor">Index of anchor tile.</param>
        /// <returns></returns>
        private int Scan(TileIndex anchor)
        {
            ColliderInfo anchorInfo = null;
            this.GetColliderInfo(ref anchorInfo, anchor);
            if (anchorInfo == null) {
                return anchor.column;
            }

            ColliderInfo info = null, firstInfo = null, lastInfo = null;

            // Begin tracking list of collider components which will be combined.
            this.ResetReducedColliderTracking();
            this.TrackReducedCollider(anchorInfo.Row, anchorInfo.Column, anchorInfo.Collider);

            try {
                TileIndex target = anchor;
                BoxBounds reducedBounds = anchorInfo.Bounds;

                // Scan rightwards for colliders which can be combined with anchor.
                for (int column = anchor.column + 1; column < this.system.ColumnCount; ++column) {
                    if (this.markedTiles[anchor.row, column]) {
                        break;
                    }

                    this.GetColliderInfo(ref info, anchor.row, column);
                    if (!this.CanReduce(info, anchorInfo)) {
                        break;
                    }

                    // Can candidate collider be combined with anchor?
                    if (!reducedBounds.Encapsulate(info.Bounds)) {
                        break;
                    }

                    target.column = column;

                    this.TrackReducedCollider(info.Row, info.Column, info.Collider);
                }

                this.GetColliderInfo(ref info, target);
                var prevFirstBounds = anchorInfo.Bounds;
                var prevLastBounds = info.Bounds;

                // Extend downwards if possible, though avoid crossing T-junctions since splitting
                // a T-junction will often cause more colliders to occur in result.
                for (int row = anchor.row + 1; row < this.system.RowCount; ++row) {
                    this.GetColliderInfo(ref firstInfo, row, anchor.column);
                    this.GetColliderInfo(ref lastInfo, row, target.column);
                    if (firstInfo == null || lastInfo == null || !this.CanReduce(firstInfo, anchorInfo) || this.CheckForTJunction(firstInfo, lastInfo)) {
                        goto FinishedExtendingDownards;
                    }

                    // Does first and last tile from previous row extend downwards?
                    if (!prevFirstBounds.Encapsulate(firstInfo.Bounds) || !prevLastBounds.Encapsulate(lastInfo.Bounds)) {
                        goto FinishedExtendingDownards;
                    }

                    // No need to track collider from `lastInfo` since it will be included
                    // within the following loop.
                    this.candidateColliders.Clear();
                    this.candidateColliders.Add(firstInfo.Collider);

                    // Reduce bounds for strip.
                    BoxBounds stripBounds = firstInfo.Bounds;
                    for (int column = anchor.column + 1; column <= target.column; ++column) {
                        this.GetColliderInfo(ref info, row, column);
                        if (!this.CanReduce(info, anchorInfo) || !stripBounds.Encapsulate(info.Bounds)) {
                            goto FinishedExtendingDownards;
                        }
                        this.candidateColliders.Add(info.Collider);
                    }

                    // The following should not fail, but let's just be safe!
                    if (!reducedBounds.Encapsulate(stripBounds)) {
                        Debug.LogWarning("Unable to encapsulate collider on reduce.");
                        goto FinishedExtendingDownards;
                    }

                    target.row = row;

                    // Accept and track candidate colliders!
                    for (int i = 0; i < this.candidateColliders.Count; ++i) {
                        this.TrackReducedCollider(row, anchor.column + i, this.candidateColliders[i]);
                    }

                    prevFirstBounds = firstInfo.Bounds;
                    prevLastBounds = lastInfo.Bounds;
                }

                FinishedExtendingDownards:
                ;

                bool hasManyColliders = (this.reducedColliderCount > 1);
                bool hasOneHypotheticalCollider = (this.reducedColliderCount == 1 && this.reducedColliderComponents.Count == 0);
                if (hasManyColliders || hasOneHypotheticalCollider) {
                    this.Reduce(anchorInfo, reducedBounds);
                }

                return target.column;
            }
            finally {
                ColliderInfo.Despawn(anchorInfo);
                ColliderInfo.Despawn(info);
                ColliderInfo.Despawn(firstInfo);
                ColliderInfo.Despawn(lastInfo);
            }
        }

        /// <summary>
        /// Create new "Combined Collider" game object which covers bounds of tiles that
        /// have been reduced. Reduced tile game objects are stripped if they nolonger
        /// contain any useful components.
        /// </summary>
        /// <param name="info">Information about reduced collider.</param>
        /// <param name="reducedBounds">Bounding area of all reduced colliders.</param>
        private void Reduce(ColliderInfo info, BoxBounds reducedBounds)
        {
            var reducedGO = new GameObject("Combined Collider");

            // Can only copy layer and tag from collider information when the collider
            // information actually contains a game object!
            if (info.Tile.gameObject != null) {
                if (this.separateByLayer) {
                    reducedGO.layer = info.Tile.gameObject.layer;
                }
                if (this.separateByTag) {
                    reducedGO.tag = info.Tile.gameObject.tag;
                }
            }

            var reducedTransform = reducedGO.transform;
            reducedTransform.SetParent(this.system.transform, false);

            reducedBounds = SpaceToSpace(reducedBounds, reducedTransform.worldToLocalMatrix * this.systemToWorld);

            switch (info.Type) {
                case ColliderType.BoxCollider2D: {
                        var collider = reducedGO.AddComponent<BoxCollider2D>();
                        collider.offset = reducedBounds.center;
                        collider.size = reducedBounds.size;
                        collider.isTrigger = info.IsTrigger;
                        collider.sharedMaterial = info.Material as PhysicsMaterial2D;

                        this.DestroyTrackedColliders();
                    }
                    break;

                case ColliderType.BoxCollider3D: {
                        var collider = reducedGO.AddComponent<BoxCollider>();
                        collider.center = reducedBounds.center;
                        collider.size = reducedBounds.size;
                        collider.isTrigger = info.IsTrigger;
                        collider.sharedMaterial = info.Material as PhysicMaterial;

                        this.DestroyTrackedColliders();
                    }
                    break;

                default:
                    Debug.LogWarning(string.Format("Collider reduction not implemented for '{0}'.", info.Type));
                    break;
            }
        }

        /// <summary>
        /// Utility method to destroy an <see cref="UnityEngine.Object"/> instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static void DestroyObject(Object obj)
        {
            // Allow Unity to optimize object destruction at runtime, but in edit mode
            // we must destroy the object immediately (as suggested in Unity docs).
            if (Application.isPlaying) {
                Object.Destroy(obj);
            }
            else {
                Object.DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Destroy collider components that have been tracked and strip associated game
        /// object if nolonger contains any useful components.
        /// </summary>
        private void DestroyTrackedColliders()
        {
            foreach (var collider in this.reducedColliderComponents) {
                var transform = collider.transform;
                DestroyObject(collider);
                StrippingUtility.StripEmptyGameObject(transform);
            }
            this.reducedColliderComponents.Clear();
        }

        /// <summary>
        /// Check for 'T' junction of combinable tile colliders.
        /// </summary>
        /// <param name="firstInfo">Collider information for first tile in horizontal sequence.</param>
        /// <param name="lastInfo">Collider information for last tile in horizontal sequence.</param>
        /// <returns>
        /// A value of <c>true</c> if T-junction was detected; otherwise, a value of <c>false</c>.
        /// </returns>
        private bool CheckForTJunction(ColliderInfo firstInfo, ColliderInfo lastInfo)
        {
            if (firstInfo.Column == 0 || lastInfo.Column + 1 == this.system.ColumnCount) {
                return false;
            }

            if (this.markedTiles[firstInfo.Row, firstInfo.Column - 1] || this.markedTiles[lastInfo.Row, lastInfo.Column + 1]) {
                return false;
            }

            ColliderInfo leftInfo = null, rightInfo = null;
            try {
                this.GetColliderInfo(ref leftInfo, firstInfo.Row, firstInfo.Column - 1);
                if (!this.CanReduce(leftInfo, firstInfo)) {
                    return false;
                }

                this.GetColliderInfo(ref rightInfo, lastInfo.Row, lastInfo.Column + 1);
                if (!this.CanReduce(rightInfo, firstInfo)) {
                    return false;
                }

                return leftInfo.Bounds.Encapsulate(firstInfo.Bounds)
                    && lastInfo.Bounds.Encapsulate(rightInfo.Bounds)
                    ;
            }
            finally {
                ColliderInfo.Despawn(leftInfo);
                ColliderInfo.Despawn(rightInfo);
            }
        }
    }
}
