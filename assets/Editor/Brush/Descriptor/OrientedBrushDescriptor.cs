// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes oriented brush.
    /// </summary>
    internal sealed class OrientedBrushDescriptor : BrushDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrientedBrushDescriptor"/> class.
        /// </summary>
        public OrientedBrushDescriptor()
            : base(typeof(OrientedBrush), typeof(OrientedBrushDesigner), typeof(AliasBrushDesigner))
        {
        }


        /// <inheritdoc/>
        public override bool CanHavePreviewCache(Brush brush)
        {
            var orientedBrush = brush as OrientedBrush;
            if (orientedBrush == null) {
                return false;
            }

            var orientation = orientedBrush.FindClosestOrientation(0);
            if (orientation == null || orientation.VariationCount == 0) {
                return false;
            }

            var firstVariation = orientation.GetVariation(0);

            // No preview cache is generated for atlas brushes!
            if (firstVariation is TilesetBrush) {
                return false;
            }
            var nestedAliasBrush = firstVariation as AliasBrush;
            if (nestedAliasBrush != null && nestedAliasBrush.target is TilesetBrush) {
                return false;
            }

            // No preview cache is generated for empty brushes!
            return !(firstVariation is EmptyBrush);
        }

        /// <inheritdoc/>
        protected internal override bool DrawPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            var brush = record.Brush as OrientedBrush;
            if (brush == null) {
                return false;
            }

            var orientation = brush.FindClosestOrientation(0);
            if (orientation == null || orientation.VariationCount == 0) {
                return false;
            }

            var firstVariation = orientation.GetVariation(0);

            var nestedRecord = BrushDatabase.Instance.FindRecord(firstVariation as Brush);
            if (nestedRecord == null) {
                return false;
            }

            // Use preview from nested brush.
            return RotorzEditorGUI.DrawBrushPreviewHelper(output, nestedRecord, selected);
        }
    }
}
