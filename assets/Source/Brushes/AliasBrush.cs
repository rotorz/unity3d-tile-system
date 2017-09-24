// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// An alias brush targets an existing brush allowing the user to override certain
    /// properties and remap materials.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Alias-Brushes">Alias Brushes</a>
    /// section of user guide for further information.</para>
    /// </intro>
    /// <remarks>
    /// <para>The purpose of an alias brush is to avoid the burden of redefining complex
    /// oriented brushes where only minor differences occur. Alias brushes can target a
    /// range of brush types though they cannot target other alias brushes.</para>
    /// </remarks>
    /// <seealso cref="OrientedBrush"/>
    /// <seealso cref="TilesetBrush"/>
    /// <seealso cref="AutotileBrush"/>
    /// <seealso cref="EmptyBrush"/>
    public class AliasBrush : Brush, IMaterialMappings, ICoalescableBrush
    {
        /// <inheritdoc/>
        public override string DesignableType {
            get { return "Alias Brush"; }
        }

        /// <summary>
        /// The target brush to create an alias of.
        /// </summary>
        /// <remarks>
        /// <para>Alias brushes can target a variety of brush types including those listed
        /// below, however they must not target other alias brushes.</para>
        /// <list type="bullet">
        ///    <item><see cref="OrientedBrush"/></item>
        ///    <item><see cref="AutotileBrush"/></item>
        ///    <item><see cref="TilesetBrush"/></item>
        ///    <item><see cref="EmptyBrush"/></item>
        /// </list>
        /// </remarks>
        public Brush target;
        /// <summary>
        /// Indicates if this brush should override the flags of its target.
        /// </summary>
        public bool overrideFlags = false;


        /// <inheritdoc/>
        public override int TileFlags {
            get {
                if (this.overrideFlags) {
                    return this.userFlags;
                }
                else {
                    return this.target != null ? this.target.TileFlags : 0;
                }
            }
            set {
                if (this.overrideFlags) {
                    this.userFlags = value;
                }
            }
        }

        /// <inheritdoc/>
        public override bool CanOverrideTagAndLayer {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool PerformsAutomaticOrientation {
            get {
                return this.target != null
                    ? this.target.PerformsAutomaticOrientation
                    : false;
            }
        }

        /// <inheritdoc/>
        public override bool UseWireIndicatorInEditor {
            get {
                return this.target != null
                    ? this.target.UseWireIndicatorInEditor
                    : false;
            }
        }

        /// <inheritdoc/>
        public sealed override int CountTileVariations(int orientationMask)
        {
            return this.target.CountTileVariations(orientationMask);
        }

        /// <inheritdoc/>
        public sealed override int PickRandomVariationIndex(int orientationMask)
        {
            return this.target.PickRandomVariationIndex(orientationMask);
        }

        /// <inheritdoc/>
        protected internal override void PrepareTileData(IBrushContext context, TileData tile, int variationIndex)
        {
            this.target.PrepareTileData(context, tile, variationIndex);
        }

        /// <inheritdoc/>
        protected internal override bool CalculateManualOffset(IBrushContext context, TileData tile, Transform transform, out Vector3 offsetPosition, out Quaternion offsetRotation, out Vector3 offsetScale, Brush transformer)
        {
            // If transforming brush has not already been overridden...
            if (transformer == this && !this.overrideTransforms) {
                transformer = this.target;
            }
            return this.target.CalculateManualOffset(context, tile, transform, out offsetPosition, out offsetRotation, out offsetScale, transformer);
        }

        /// <inheritdoc/>
        protected internal override void CreateTile(IBrushContext context, TileData tile)
        {
            this.target.CreateTile(context, tile);
        }

        /// <inheritdoc/>
        protected internal override void ApplyTransforms(IBrushContext context, TileData tile, Brush transformer)
        {
            // If transforming brush has not already been overridden...
            if (transformer == this && !this.overrideTransforms) {
                transformer = this.target;
            }
            this.target.ApplyTransforms(context, tile, transformer);
        }

        /// <inheritdoc/>
        protected internal override void PostProcessTile(IBrushContext context, TileData tile)
        {
            this.target.PostProcessTile(context, tile);
            base.PostProcessTile(context, tile);
        }

        /// <inheritdoc/>
        public override Material GetNthMaterial(int n)
        {
            if (this.target != null) {
                return this.target.GetNthMaterial(n);
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Revert alias brush to match its target as closely as possible.
        /// </summary>
        public void RevertToTarget()
        {
            if (this.target == null) {
                return;
            }

            this.overrideLayer = false;
            this.overrideTag = false;
            this.overrideTransforms = false;
            this.overrideFlags = false;

            this.Static = this.target.Static;
            this.Smooth = this.target.Smooth;

            this.scaleMode = this.target.scaleMode;
            this.transformScale = this.target.transformScale;
            this.applyPrefabTransform = this.target.applyPrefabTransform;

            this.group = this.target.group;

            this.RevertCoalescingToTarget();

            this.customPreviewDesignTime = this.target.customPreviewDesignTime;
            this.customPreviewImage = this.target.customPreviewImage;
        }


        #region Immediate Preview

        /// <inheritdoc/>
        public override void OnDrawImmediatePreview(IBrushContext context, TileData previewTile, Material previewMaterial, Brush transformer)
        {
            if (this.target != null) {
                // If transforming brush has not already been overridden then...
                if (transformer == this && !this.overrideTransforms) {
                    transformer = this.target;
                }
                this.target.OnDrawImmediatePreview(context, previewTile, previewMaterial, transformer);
            }
        }

        #endregion


        #region IMaterialMappings Implementation

        [SerializeField, FormerlySerializedAs("_materialMappingFrom")]
        private Material[] materialMappingFrom;
        [SerializeField, FormerlySerializedAs("_materialMappingTo")]
        private Material[] materialMappingTo;


        /// <inheritdoc/>
        public Material[] MaterialMappingFrom {
            get { return this.materialMappingFrom; }
            set { this.materialMappingFrom = value; }
        }

        /// <inheritdoc/>
        public Material[] MaterialMappingTo {
            get { return this.materialMappingTo; }
            set { this.materialMappingTo = value; }
        }

        #endregion


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

        private void RevertCoalescingToTarget()
        {
            // Reset coalescing properties to their default values.
            this.Coalesce = Tile.Coalesce.Own;
            this.CoalesceWithRotated = false;
            this.CoalesceWithBrushGroups.Clear();

            // Revert coalescing properties from target if target supports this.
            var coalescableTarget = this.target as ICoalescableBrush;
            if (coalescableTarget != null) {
                this.Coalesce = coalescableTarget.Coalesce;

                foreach (int otherGroup in coalescableTarget.CoalesceWithBrushGroups) {
                    this.CoalesceWithBrushGroups.Add(otherGroup);
                }

                this.CoalesceWithRotated = coalescableTarget.CoalesceWithRotated;
                this.CoalesceWithBorder = coalescableTarget.CoalesceWithBorder;
            }
        }

        #endregion
    }
}
