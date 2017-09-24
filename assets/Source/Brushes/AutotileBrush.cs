// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Autotile brushes make it easier for artists to design 2D tiles that connect with
    /// one another. This type of brush is particularly useful for creating things like
    /// walls, paths, platforms, etc.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Autotile-Brushes">Autotile Brushes</a>
    /// section of user guide for further information.</para>
    /// </intro>
    /// <remarks>
    /// <para>Basic and extended autotile layouts are supported for which some developers
    /// and artists will be familiar with. In addition to making it easier for artists to
    /// produce the artwork for their games, this also means that existing autotile artwork
    /// can be used. A template for each supported autotile layout is included which some
    /// artists may find useful when producing their artwork.</para>
    /// <para><img src="../art/autotile-templates.jpg"/></para>
    /// <para>When designing the artwork for an autotile it is possible to use a variety
    /// of tile sizes provided that they are even numbers. For example, each tile could be
    /// 32x32, 48x48, etc.</para>
    /// <para>Upon creating an autotile brush using the brush designer interface, an
    /// autotile atlas is automatically generated from the provided autotile artwork. The
    /// input autotile artwork is best placed within a directory named "Editor" so that it
    /// can be accessed by the editor when needed, whilst not included within final game
    /// builds.</para>
    /// <para>Tiles that are painted using autotile brushes are presented using a
    /// procedurally generated mesh. If desired the procedurally generated mesh can be
    /// pre-generated when building a tile system if desired (see <see cref="TileSystem.pregenerateProcedural"/>).</para>
    /// </remarks>
    /// <seealso cref="AutotileTileset"/>
    /// <seealso cref="OrientedBrush"/>
    /// <seealso cref="AliasBrush"/>
    /// <seealso cref="TilesetBrush"/>
    /// <seealso cref="EmptyBrush"/>
    public sealed class AutotileBrush : TilesetBrush, ICoalescableBrush
    {
        private AutotileTileset autotileTileset;


        /// <inheritdoc/>
        public override string DesignableType {
            get { return "Autotile Brush"; }
        }

        /// <summary>
        /// Gets tileset that brush belongs to.
        /// </summary>
        public new AutotileTileset Tileset {
            get {
                if (this.autotileTileset == null) {
                    this.autotileTileset = base.Tileset as AutotileTileset;
                }
                return this.autotileTileset;
            }
        }


        #region Messages and Events

        /// <inheritdoc/>
        protected override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            // Promote value from legacy field.
            if (this.coalesceWithBrushGroups == null && this.IsUsingCoalesceWithBrushGroups()) {
                this.coalesceWithBrushGroups = new int[] { this.coalesceBrushGroup };
            }
        }

        /// <inheritdoc/>
        protected override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();

            // Convert coalesce groups from set to array so that Unity can serialize it.
            if (this.IsUsingCoalesceWithBrushGroups()) {
                if (this.coalesceWithBrushGroupSet != null) {
                    if (this.coalesceWithBrushGroups == null || this.coalesceWithBrushGroups.Length != this.coalesceWithBrushGroupSet.Count) {
                        this.coalesceWithBrushGroups = new int[this.coalesceWithBrushGroupSet.Count];
                    }
                    this.coalesceWithBrushGroupSet.CopyTo(this.coalesceWithBrushGroups);
                }
            }
            else {
                this.coalesceWithBrushGroups = null;
            }
        }

        #endregion


        #region ICoalescableBrush Members

        [SerializeField, FormerlySerializedAs("_coalesce")]
        private Coalesce coalesce = Coalesce.Own;
        [SerializeField, FormerlySerializedAs("_coalesceWithRotated")]
        private bool coalesceWithRotated;
        [SerializeField, FormerlySerializedAs("_coalesceWithBorder")]
        private bool coalesceWithBorder;
        [SerializeField, FormerlySerializedAs("_coalesceBrushGroup")]
        private int coalesceBrushGroup = 0;
        [SerializeField, FormerlySerializedAs("_coalesceWithBrushGroups")]
        private int[] coalesceWithBrushGroups;

        private HashSet<int> coalesceWithBrushGroupSet;


        /// <inheritdoc/>
        public Coalesce Coalesce {
            get { return this.coalesce; }
            set { this.coalesce = value; }
        }

        /// <inheritdoc/>
        public bool CoalesceWithRotated {
            get { return this.coalesceWithRotated; }
            set { this.coalesceWithRotated = value; }
        }

        /// <inheritdoc/>
        public bool CoalesceWithBorder {
            get { return this.coalesceWithBorder; }
            set { this.coalesceWithBorder = value; }
        }

        /// <inheritdoc/>
        public ICollection<int> CoalesceWithBrushGroups {
            get {
                if (this.coalesceWithBrushGroupSet == null) {
                    this.coalesceWithBrushGroupSet = new HashSet<int>(this.coalesceWithBrushGroups ?? EmptyArray<int>.Instance);
                }
                return this.coalesceWithBrushGroupSet;
            }
        }

        #endregion


        [SerializeField, FormerlySerializedAs("_innerSolidFlag")]
        private bool innerSolidFlag;

        /// <summary>
        /// Indicates whether box colliders should be added to inner painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>An individual box collider is added to each painted tile which can cause
        /// the editor to perform considerably slower, not to mention slower performance at
        /// runtime. Please consider carefully whether colliders are absolutely necessary
        /// for inner tiles and if possible avoid.</para>
        /// <para>Custom tile based collision detection logic is often more efficient and
        /// can be implemented using tile flags (and of course the "Solid" flag where
        /// applicable).</para>
        /// </remarks>
        public bool addInnerCollider;


        /// <summary>
        /// Gets or sets a value indicating whether inner painted tiles should be
        /// flagged as solid.
        /// </summary>
        /// <remarks>
        /// <para>When set, the solid flag of inner tiles will be set as tiles are painted.
        /// An inner tile is one that has 4 adjacent surrounding tiles.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if solid; otherwise <c>false</c>.
        /// </value>
        /// <seealso cref="TileData.SolidFlag">TileData.SolidFlag</seealso>
        /// <seealso cref="TileSystem.TileTraceSolid">TileSystem.TileTraceSolid</seealso>
        public bool InnerSolidFlag {
            get { return this.innerSolidFlag; }
            set { this.innerSolidFlag = value; }
        }

        /// <summary>
        /// Gets the style of autotile layout.
        /// </summary>
        public AutotileLayout Layout {
            get { return this.Tileset.AutotileLayout; }
        }

        /// <inheritdoc/>
        public override bool PerformsAutomaticOrientation {
            get { return true; }
        }


        /// <inheritdoc/>
        protected internal override void PrepareTileData(IBrushContext context, TileData tile, int variationIndex)
        {
            // Find actual orientation of target tile.
            int actualOrientation = OrientationUtility.DetermineTileOrientation(context.TileSystem, context.Row, context.Column, context.Brush, tile.PaintedRotation);
            // Find nearest match, assume default scenario.
            tile.orientationMask = (byte)this.Tileset.FindClosestOrientation(actualOrientation);

            tile.Procedural = true;
            tile.tileset = this.Tileset;
            tile.tilesetIndex = this.Tileset.IndexFromOrientation(tile.orientationMask);

            // Is this an inner tile?
            if ((tile.orientationMask & 0x5A) == 0x5A) {
                tile.SolidFlag = this.InnerSolidFlag;
            }
        }

        /// <inheritdoc/>
        protected internal override void CreateTile(IBrushContext context, TileData tile)
        {
            bool addCollider = ((tile.orientationMask & 0x5A) == 0x5A)
                ? this.addInnerCollider
                : this.addCollider;

            // Tile is procedural for autotile brushes regardless.
            this.CreateProceduralTile(context, tile, addCollider);
        }
    }
}
