// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes alias brush.
    /// </summary>
    internal sealed class AliasBrushDescriptor : BrushDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasBrushDescriptor"/> class.
        /// </summary>
        public AliasBrushDescriptor()
            : base(typeof(AliasBrush), typeof(AliasBrushDesignerAdaptor), null)
        {
        }


        /// <inheritdoc/>
        public override bool CanHavePreviewCache(Brush brush)
        {
            var aliasBrush = brush as AliasBrush;
            if (aliasBrush == null || aliasBrush.target == null) {
                return false;
            }

            return !(aliasBrush.target is TilesetBrush || aliasBrush.target is EmptyBrush);
        }

        /// <inheritdoc/>
        protected internal override bool DrawPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            var brush = record.Brush as AliasBrush;
            if (brush == null) {
                return false;
            }
            var targetRecord = BrushDatabase.Instance.FindRecord(brush.target);
            if (targetRecord == null) {
                return false;
            }

            return RotorzEditorGUI.DrawBrushPreviewHelper(output, targetRecord, selected);
        }
    }
}
