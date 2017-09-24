// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Oriented tile brush.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Oriented-Brushes">Oriented Brushes</a>
    /// section of user guide for further information.</para>
    /// </intro>
    /// <remarks>
    /// <para>Selects and paints tiles based upon their orientation and can optionally
    /// pick from multiple variations. Prefabs can be assigned to each variation which
    /// are then instantiated upon being painted. It is also possible to specify certain
    /// types of other brushes for variations.</para>
    /// </remarks>
    /// <seealso cref="AliasBrush"/>
    /// <seealso cref="TilesetBrush"/>
    /// <seealso cref="AutotileBrush"/>
    /// <seealso cref="EmptyBrush"/>
    public class OrientedBrush : Brush, IMaterialMappings, ICoalescableBrush
    {
        /// <inheritdoc/>
        public override string DesignableType {
            get { return "Oriented Brush"; }
        }


        // Mask of the default orientation
        [SerializeField, HideInInspector, FormerlySerializedAs("_defaultOrientationMask")]
        private int defaultOrientationMask = 0;

        // The default orientation
        private BrushOrientation defaultOrientation;

        /// <summary>
        /// Gets or sets bit mask that identifies the default orientation.
        /// </summary>
        /// <remarks>
        /// <para>Bit mask representation of an orientation.</para>
        /// </remarks>
        public int DefaultOrientationMask {
            get { return this.defaultOrientationMask; }
            set {
                if (value != this.defaultOrientationMask) {
                    this.defaultOrientationMask = value;
                    this.defaultOrientation = this.FindOrientation(value);
                }
            }
        }

        /// <summary>
        /// Gets the default orientation.
        /// </summary>
        public BrushOrientation DefaultOrientation {
            get { return this.defaultOrientation; }
        }


        [SerializeField, HideInInspector, FormerlySerializedAs("_fallbackMode")]
        private FallbackMode fallbackMode = FallbackMode.NextBest;


        /// <summary>
        /// Gets or sets the fallback mode.
        /// </summary>
        /// <remarks>
        /// <para>A user specified fallback mode is assumed to handle orientations that
        /// have not been explicitly defined.</para>
        /// </remarks>
        public FallbackMode FallbackMode {
            get { return this.fallbackMode; }
            set { this.fallbackMode = value; }
        }


        /// <summary>
        /// Indicates if flags of nested brushes should be overridden.
        /// </summary>
        public bool forceOverrideFlags = false;


        #region Orientations Property

        [SerializeField, FormerlySerializedAs("_orientations")]
        private BrushOrientation[] orientations = { };

        /// <summary>
        /// Gets read-only collection of orientations.
        /// </summary>
        /// <remarks>
        /// <para>Please do not attempt to modify returned collection.</para>
        /// </remarks>
        public IList<BrushOrientation> Orientations {
            get { return this.orientations; }
        }


        [NonSerialized]
        private BrushOrientation[] lookupTable;


        /// <summary>
        /// Lookup table improves performance of <see cref="this.FindOrientation"/>.
        /// </summary>
        /// <remarks>
        /// <para>This method must be called whenever changes are made to <see cref="Orientations"/>.</para>
        /// </remarks>
        private void RefreshOrientationLookupTable()
        {
            if (this.lookupTable == null) {
                this.lookupTable = new BrushOrientation[256];
            }
            else {
                for (int i = 0; i < this.lookupTable.Length; ++i) {
                    this.lookupTable[i] = null;
                }
            }

            foreach (var orientation in this.orientations) {
                this.lookupTable[orientation.Mask] = orientation;
            }
        }

        /// <summary>
        /// Adds orientation to brush optionally with rotational symmetry.
        /// </summary>
        /// <param name="mask">Bit mask describing orientation.</param>
        /// <param name="rotationalSymmetry">Indicates whether additional orientations should
        /// be added which have rotational symmetry.</param>
        /// <returns>
        /// First <see cref="BrushOrientation"/> in group with rotational symmetry.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If orientation already exists.
        /// </exception>
        /// <seealso cref="BrushOrientation.AddVariation(Object)">BrushOrientation.AddVariation(Object)</seealso>
        public BrushOrientation AddOrientation(int mask, bool rotationalSymmetry)
        {
            int[] masks = rotationalSymmetry
                ? OrientationUtility.GetMasksWithRotationalSymmetry(mask)
                : new int[] { mask }
                ;

            // Obviously, there is no rotational symmetry if there is only 1!
            if (masks.Length == 1) {
                rotationalSymmetry = false;
            }

            // First, make sure that orientations do not alreay exist.
            for (int i = 0; i < masks.Length; ++i) {
                if (this.FindOrientation(mask) != null) {
                    throw new Exception("Orientation already exists.");
                }
            }

            var newOrientations = new List<BrushOrientation>();

            // Add orientation(s).
            for (int i = 0; i < masks.Length; ++i) {
                var orientation = new BrushOrientation(masks[i]);
                orientation.type = rotationalSymmetry;
                orientation.rotation = i;

                newOrientations.Add(orientation);

                // Upgrade default orientation reference?
                // Note: This is necessary when brush is first created.
                if (masks[i] == this.defaultOrientationMask) {
                    this.defaultOrientation = newOrientations[0];
                }
            }

            // Add the new orientation(s).
            int newIndex = this.orientations.Length;
            Array.Resize(ref this.orientations, this.orientations.Length + newOrientations.Count);
            for (int i = 0; i < newOrientations.Count; ++i) {
                this.orientations[newIndex + i] = newOrientations[i];
            }

            // Sort orientations by mask.
            this.SortOrientationsByRotationalSymmetry();

            this.RefreshOrientationLookupTable();

            return newOrientations[0];
        }

        /// <summary>
        /// Adds orientation to brush.
        /// </summary>
        /// <param name="mask">Bit mask describing orientation.</param>
        /// <returns>
        /// The new <see cref="BrushOrientation"/> instance.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If orientation already exists.
        /// </exception>
        /// <seealso cref="BrushOrientation.AddVariation(Object)">BrushOrientation.AddVariation(Object)</seealso>
        public BrushOrientation AddOrientation(int mask)
        {
            return this.AddOrientation(mask, false);
        }

        /// <summary>
        /// Group orientations by rotational symmetry and then order by mask.
        /// </summary>
        private void SortOrientationsByRotationalSymmetry()
        {
            // Group orientations by rotational symmetry.
            var groups = new List<List<BrushOrientation>>();
            for (int i = 0; i < this.orientations.Length; ++i) {
                var orientation = this.orientations[i];

                // Attempt to place orientation within an existing group.
                for (int j = 0; j < groups.Count; ++j) {
                    var group = groups[j];
                    if (OrientationUtility.HasRotationalSymmetry(orientation.Mask, group[0].Mask)) {
                        group.Add(orientation);
                        orientation = null;
                        break;
                    }
                }

                // Create new group for orientation?
                if (orientation != null) {
                    var group = new List<BrushOrientation>();
                    group.Add(orientation);
                    groups.Add(group);
                }
            }

            // Sort orientations within each group by mask.
            for (int i = 0; i < groups.Count; ++i) {
                groups[i].Sort((a, b) => b.Mask - a.Mask);
            }

            // Sort groups by mask.
            groups.Sort((a, b) => a[0].Mask - b[0].Mask);

            // Flatten orientation groups into main orientations array.
            int insertIndex = 0;
            for (int i = 0; i < groups.Count; ++i) {
                var group = groups[i];
                for (int j = 0; j < group.Count; ++j) {
                    this.orientations[insertIndex++] = group[j];
                }
            }
        }

        /// <summary>
        /// Removes orientation from brush.
        /// </summary>
        /// <remarks>
        /// <para>Does nothing if orientation does not exist.</para>
        /// </remarks>
        /// <param name="mask">Bit mask of orientation that is to be removed.</param>
        public void RemoveOrientation(int mask)
        {
            var orientation = this.FindOrientation(mask);
            if (orientation == null) {
                return;
            }

            // Remove coupled orientations if orientation has rotational symmetry.
            var newArray = new List<BrushOrientation>(this.orientations);

            foreach (int groupedMask in orientation.GetGroupedOrientationMasks()) {
                orientation = this.FindOrientation(groupedMask);
                if (orientation != null) {
                    newArray.Remove(orientation);
                }
            }

            this.orientations = newArray.ToArray();

            this.RefreshOrientationLookupTable();
        }

        #endregion


        /// <inheritdoc/>
        public override bool CanOverrideTagAndLayer {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool PerformsAutomaticOrientation {
            get { return true; }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>Wireframe cursor will be assumed for oriented brushes unless the
        /// first variation of the default orientation is a brush that would prefer
        /// otherwise.</para>
        /// </remarks>
        public override bool UseWireIndicatorInEditor {
            get {
                var orientation = this.DefaultOrientation;
                if (orientation == null || orientation.VariationCount == 0) {
                    return true;
                }

                var firstVariation = orientation.GetVariation(0) as Brush;

                return firstVariation == null
                    || firstVariation.UseWireIndicatorInEditor
                    ;
            }
        }

        /// <summary>
        /// Finds the specified orientation.
        /// </summary>
        /// <param name="mask">Bit mask of tile orientation.</param>
        /// <returns>
        /// The orientation; otherwise <c>null</c> if not found.
        /// </returns>
        /// <seealso cref="DefaultOrientation"/>
        /// <seealso cref="this.FindClosestOrientation"/>
        public BrushOrientation FindOrientation(int mask)
        {
            if (this.lookupTable == null) {
                this.RefreshOrientationLookupTable();
            }

            return mask < this.lookupTable.Length
                ? this.lookupTable[mask]
                : null;
        }

        /// <summary>
        /// Finds closest match for the specified orientation mask.
        /// </summary>
        /// <param name="mask">Bit mask of tile orientation.</param>
        /// <returns>
        /// The orientation; otherwise <c>null</c> if not found.
        /// </returns>
        /// <seealso cref="DefaultOrientation"/>
        /// <seealso cref="this.FindOrientation"/>
        public BrushOrientation FindClosestOrientation(int mask)
        {
            return this.FindOrientation(this.FindClosestOrientationMask(mask));
        }

        /// <summary>
        /// Finds mask of orientation that best matches the specified orientation mask.
        /// </summary>
        /// <param name="mask">Bit mask of tile orientation.</param>
        /// <returns>
        /// Bit mask of the closest available orientation.
        /// </returns>
        public int FindClosestOrientationMask(int mask)
        {
            // Is desired orientation available?
            if (this.FindOrientation(mask) != null) {
                return mask;
            }

            // Find nearest match.
            int strongestConnections = 2;
            int weakConnections = 0;
            int strongestOrientation = this.defaultOrientationMask;

            int childOrientation, s, w;

            if (FallbackMode == Tile.FallbackMode.NextBest) {
                for (int i = 0; i < this.orientations.Length; ++i) {
                    childOrientation = this.orientations[i].Mask;

                    // If there are at least 3 strong connections...
                    s = OrientationUtility.CountStrongConnections(mask, childOrientation);
                    if (s > strongestConnections) {
                        // Strong connections overule any previous weak connection matches!
                        strongestConnections = s;
                        weakConnections = OrientationUtility.CountWeakConnections(mask, childOrientation);
                        strongestOrientation = childOrientation;
                    }
                    // If this connection is just as strong as the previous then we can determine
                    // which one is better by looking at the weak connections.
                    else if (s == strongestConnections) {
                        // Choose the connection that has the most weak connections!
                        w = OrientationUtility.CountWeakConnections(mask, childOrientation);
                        if (w > weakConnections) {
                            strongestOrientation = childOrientation;
                            weakConnections = w;
                        }
                    }
                }
            }
            else if (FallbackMode == Tile.FallbackMode.UseDefault) {
                for (int i = 0; i < this.orientations.Length; ++i) {
                    childOrientation = this.orientations[i].Mask;

                    // When using default mode there must be exactly 4 strong connections!
                    if (OrientationUtility.CountStrongConnections(mask, childOrientation) == 4) {
                        w = OrientationUtility.CountWeakConnections(mask, childOrientation);

                        // Has a strong connection been found for the first time?
                        // Otherwise, choose the connection that has the most weak connections!
                        if (strongestConnections == 2 || w > weakConnections) {
                            strongestConnections = 4;
                            weakConnections = w;
                            strongestOrientation = childOrientation;
                        }
                    }
                }
            }
            else if (FallbackMode == Tile.FallbackMode.UseDefaultStrict) {
                for (int i = 0; i < this.orientations.Length; ++i) {
                    childOrientation = this.orientations[i].Mask;

                    // When using default mode there must be exactly 4 strong connections!
                    if (OrientationUtility.CountStrongConnections(mask, childOrientation) == 4 && OrientationUtility.CountWeakConnections(mask, childOrientation) == 4) {
                        strongestOrientation = childOrientation;
                    }
                }
            }

            return strongestOrientation;
        }

        /// <summary>
        /// Synchronise variations of grouped orientations from specified orientation.
        /// </summary>
        /// <remarks>
        /// <para>This method is most relevant when working with orientations which
        /// are grouped by rotational symmetry.</para>
        /// </remarks>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// // Lookup corner orientation.
        /// int cornerMask = OrientationUtility.MaskFromName("11010000");
        /// var cornerOrientation = orientedBrush.this.FindOrientation(cornerMask);
        /// // Adjust one or more variations.
        /// cornerOrientation.variations[0] = someVariation;
        ///
        /// // Synchronise other orientations in group.
        /// orientedBrush.SyncGroupedVariations(cornerMask);
        /// ]]></code>
        /// </example>
        /// <param name="mask">Bitmask of orientation.</param>
        /// <exception cref="System.NullReferenceException">
        /// If the specified orientation was not found.
        /// </exception>
        public void SyncGroupedVariations(int mask)
        {
            var orientation = this.FindOrientation(mask);
            if (orientation == null) {
                throw new NullReferenceException("Orientation was not found.");
            }

            foreach (int groupedMask in orientation.GetGroupedOrientationMasks()) {
                // Skip the specified orientation since we are copying from there!
                if (groupedMask == mask) {
                    continue;
                }

                var groupedOrientation = this.FindOrientation(groupedMask);
                if (groupedOrientation != null) {
                    groupedOrientation.SyncVariationsFrom(orientation);
                }
            }
        }

        /// <inheritdoc/>
        public sealed override int CountTileVariations(int orientationMask)
        {
            // Try to find nearest match, assume default scenario.
            int bestOrientation = this.FindClosestOrientationMask(orientationMask);

            var orientation = this.FindOrientation(bestOrientation);
            return (orientation != null)
                ? orientation.VariationCount
                : 1;
        }

        /// <inheritdoc/>
        public sealed override int PickRandomVariationIndex(int orientationMask)
        {
            // Try to find nearest match, assume default scenario.
            int bestOrientation = this.FindClosestOrientationMask(orientationMask);

            var orientation = this.FindOrientation(bestOrientation);
            return (orientation != null && orientation.VariationCount != 0)
                ? orientation.PickRandomVariationIndex()
                : -1;
        }

        /// <inheritdoc/>
        protected internal override void PrepareTileData(IBrushContext context, TileData tile, int variationIndex)
        {
            var tileSystem = context.TileSystem;

            // Find actual orientation of target tile.
            int actualOrientation = OrientationUtility.DetermineTileOrientation(tileSystem, context.Row, context.Column, context.Brush, tile.PaintedRotation);
            // Try to find nearest match, assume default scenario.
            int bestOrientation = this.FindClosestOrientationMask(actualOrientation);

            // Select tile for painting using tile brush.
            var orientation = this.FindOrientation(bestOrientation);
            if (orientation == null) {
                Debug.LogWarning(string.Format("Brush '{0}' orientation '{1}' not defined", this.name, OrientationUtility.NameFromMask(bestOrientation)));
                return;
            }

            int orientationVariationCount = orientation.VariationCount;

            // Randomize variation?
            if (variationIndex == RANDOM_VARIATION) {
                if (!tileSystem.BulkEditMode || tileSystem.IsEndingBulkEditMode) {
                    variationIndex = orientation.PickRandomVariationIndex();
                }
            }
            // Negative offset from variations of orientation.
            else if (variationIndex < 0) {
                variationIndex = Mathf.Max(0, orientationVariationCount + variationIndex);
            }

            // Ensure variation index is within bounds of orientation!
            int wrappedVariationIndex = variationIndex;
            if (wrappedVariationIndex >= orientationVariationCount) {
                wrappedVariationIndex = 0;
            }

            if (orientationVariationCount > 0) {
                //
                // Could re-insert following to 'fix' state of randomly selected tiles.
                // Note: Randomization is lost when erasing tiles from mass filled areas.
                //
                //// Persist wrapped variation when painting tile?
                //if (!system.BulkEditMode || system.IsEndingBulkEditMode) {
                //    variationIndex = wrappedVariationIndex;
                //}

                // Fetch nested brush reference (if there is one).
                var nestedBrush = orientation.GetVariation(wrappedVariationIndex) as Brush;
                if (nestedBrush != null) {
                    // Prepare tile data using nested brush.
                    nestedBrush.PrepareTileData(context, tile, wrappedVariationIndex);
                }
            }

            tile.orientationMask = (byte)bestOrientation;
            tile.variationIndex = (byte)variationIndex;

            // Do not attempt automatic rotation where rotation has been manually specified.
            int rotation = tile.PaintedRotation + orientation.Rotation;
            if (rotation > 3) {
                rotation -= 4;
            }
            tile.Rotation = rotation;
        }

        /// <inheritdoc/>
        protected internal override bool CalculateManualOffset(IBrushContext context, TileData tile, Transform transform, out Vector3 offsetPosition, out Quaternion offsetRotation, out Vector3 offsetScale, Brush transformer)
        {
            var orientation = this.FindOrientation(tile.orientationMask);
            if (orientation != null && orientation.VariationCount != 0) {
                // Note: Do not update variation index in tile data because this may be unintended!
                int variationIndex = (int)tile.variationIndex;
                if (variationIndex >= orientation.VariationCount) {
                    variationIndex = 0;
                }

                var nestedBrush = orientation.GetVariation(variationIndex) as Brush;
                if (nestedBrush != null) {
                    // If transforming brush has not already been overridden then...
                    if (transformer == this && !this.overrideTransforms) {
                        transformer = nestedBrush;
                    }
                    return nestedBrush.CalculateManualOffset(context, tile, transform, out offsetPosition, out offsetRotation, out offsetScale, transformer);
                }

                var prefab = orientation.GetVariation(variationIndex) as GameObject;
                if (prefab != null) {
                    Matrix4x4 normalPlacement = transformer.GetTransformMatrix(context.TileSystem, context.Row, context.Column, tile.Rotation, prefab.transform);
                    MathUtility.DecomposeMatrix(ref normalPlacement, out offsetPosition, out offsetRotation, out offsetScale);

                    Vector3 localScale = transform.localScale;

                    offsetPosition = transform.localPosition - offsetPosition;
                    offsetRotation = Quaternion.Inverse(offsetRotation) * transform.localRotation;
                    offsetScale = new Vector3(
                        localScale.x / offsetScale.x,
                        localScale.y / offsetScale.y,
                        localScale.z / offsetScale.z
                    );

                    return true;
                }
            }

            return IdentityManualOffset(out offsetPosition, out offsetRotation, out offsetScale);
        }

        /// <inheritdoc/>
        protected internal override void CreateTile(IBrushContext context, TileData tile)
        {
            var orientation = this.FindOrientation(tile.orientationMask);

            if (orientation == null) {
                Debug.LogWarning(string.Format("Brush '{0}' orientation '{1}' not defined", this.name, OrientationUtility.NameFromMask(tile.orientationMask)));
                return;
            }

            if (orientation.VariationCount == 0) {
                Debug.LogWarning(string.Format("Brush '{0}' orientation '{1}' has no variations", this.name, OrientationUtility.NameFromMask(tile.orientationMask)));
                return;
            }

            // Note: Do not update variation index in tile data because this may be unintended!
            int variationIndex = (int)tile.variationIndex;
            if (variationIndex >= orientation.VariationCount) {
                variationIndex = 0;
            }

            var orientedVariation = orientation.GetVariation(variationIndex);

            // Orchestrate a "nested" brush?
            var nestedBrush = orientedVariation as Brush;
            if (nestedBrush != null) {
                // Force override flags with that of nested brush?
                //
                // Note: Naming a little backwards in this scenario, we want to preserve
                //       flags from nested brush!
                //
                if (!this.forceOverrideFlags) {
                    // Remove all user flags, solid flag and replace with flags from nested brush.
                    tile.flags = (tile.flags & ~0x8FFFF) | nestedBrush.TileFlags;
                }

                nestedBrush.CreateTile(context, tile);
                nestedBrush.PostProcessTile(context, tile);
                return;
            }

            var variationPrefab = orientedVariation as GameObject;
            if (variationPrefab == null) {
                return;
            }

            // Actually create tile!
            tile.gameObject = InstantiatePrefabForTile(variationPrefab, tile, context.TileSystem);
        }

        /// <inheritdoc/>
        protected internal override void ApplyTransforms(IBrushContext context, TileData tile, Brush transformer)
        {
            // If transforming brush has not already been overridden...
            if (transformer == this) {
                // Find nested brush reference?
                var orientation = this.FindOrientation(tile.orientationMask);
                if (orientation != null && orientation.VariationCount > 0) {
                    // Note: Do not update variation index in tile data because this may be unintended!
                    int variationIndex = (int)tile.variationIndex;
                    if (variationIndex >= orientation.VariationCount) {
                        variationIndex = 0;
                    }

                    var nestedBrush = orientation.GetVariation(variationIndex) as Brush;

                    // Apply transforms using the nested brush (as would be expected!).
                    if (nestedBrush != null) {
                        nestedBrush.ApplyTransforms(context, tile, this.overrideTransforms ? this : nestedBrush);
                        return;
                    }
                }

                base.ApplyTransforms(context, tile, this);
            }
            else {
                base.ApplyTransforms(context, tile, transformer);
            }
        }

        /// <inheritdoc/>
        public override Material GetNthMaterial(int n)
        {
            var brushOrientation = this.FindOrientation(this.defaultOrientationMask);
            if (brushOrientation == null || brushOrientation.VariationCount == 0) {
                return null;
            }

            // Get first variation.
            if (brushOrientation.GetVariation(0) == null) {
                return null;
            }

            var materials = new List<Material>();

            int variationCount = brushOrientation.VariationCount;
            for (int i = 0; i < variationCount; ++i) {
                var variation = brushOrientation.GetVariation(i);

                var variationGO = variation as GameObject;
                if (variationGO == null) {
                    continue;
                }

                // Prepare list of distinct materials.
                foreach (var renderer in variationGO.GetComponentsInChildren<Renderer>(true)) {
                    foreach (var rendererMaterial in renderer.sharedMaterials) {
                        if (!materials.Contains(rendererMaterial)) {
                            materials.Add(rendererMaterial);
                        }
                    }
                }
            }

            return n < materials.Count
                ? materials[n]
                : null;
        }


        #region Immediate Preview

        /// <inheritdoc/>
        /// <seealso cref="CustomImmediatePreview"/>
        public override void OnDrawImmediatePreview(IBrushContext context, TileData previewTile, Material previewMaterial, Brush transformer)
        {
            var orientation = this.FindClosestOrientation(previewTile.orientationMask);
            if (orientation == null || orientation.VariationCount == 0) {
                return;
            }

            // Wrap variation index if necessary.
            int variationIndex = previewTile.variationIndex;
            if (variationIndex >= orientation.VariationCount) {
                variationIndex = 0;
            }
            // Randomization is not supported here!
            if (variationIndex == RANDOM_VARIATION) {
                variationIndex = 0;
            }

            var variationGO = orientation.GetVariation(variationIndex) as GameObject;
            if (variationGO != null) {
                var variationTransform = variationGO.transform;
                Matrix4x4 matrix = ImmediatePreviewUtility.Matrix * transformer.GetTransformMatrix(context.TileSystem, context.Row, context.Column, previewTile.Rotation, variationTransform);

                // Allow variation to provide a custom immediate preview.
                var customImmediatePreview = variationGO.GetComponent<CustomImmediatePreview>();
                if (customImmediatePreview != null) {
                    // Bail if custom immediate preview was drawn.
                    if (customImmediatePreview.DrawImmediatePreview(context, previewTile, previewMaterial, matrix)) {
                        return;
                    }
                }

                ImmediatePreviewUtility.DrawNow(previewMaterial, variationTransform, matrix, context.Brush as IMaterialMappings);
                return;
            }

            var variationBrush = orientation.GetVariation(variationIndex) as Brush;
            if (variationBrush != null) {
                // If transforming brush has not already been overridden then...
                if (transformer == this && !this.overrideTransforms) {
                    transformer = variationBrush;
                }
                variationBrush.OnDrawImmediatePreview(context, previewTile, previewMaterial, transformer);
                return;
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

            // Workaround bug which was introduced by the new Undo system of Unity 4.3.
            // When Undo/Redo is performed, NonSerialized fields are not reset to their
            // default values which is non-consistent behaviour.
            if (this.lookupTable != null) {
                this.RefreshOrientationLookupTable();
            }

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

        /// <inheritdoc/>
        public override void Awake()
        {
            base.Awake();

            this.defaultOrientation = this.FindOrientation(this.DefaultOrientationMask);
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
    }
}
