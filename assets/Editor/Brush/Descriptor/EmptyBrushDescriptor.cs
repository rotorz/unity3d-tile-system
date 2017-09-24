// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes empty brush.
    /// </summary>
    internal sealed class EmptyBrushDescriptor : BrushDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyBrushDescriptor"/> class.
        /// </summary>
        public EmptyBrushDescriptor()
            : base(typeof(EmptyBrush), typeof(EmptyBrushDesigner), typeof(AliasBrushDesigner))
        {
        }


        /// <inheritdoc/>
        protected internal override bool DrawPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            GUI.DrawTexture(output, RotorzEditorStyles.Skin.EmptyPreview);
            return true;
        }
    }
}
