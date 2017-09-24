// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes an autotile brush.
    /// </summary>
    internal sealed class AutotileBrushDescriptor : TilesetBrushDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutotileBrushDescriptor"/> class.
        /// </summary>
        public AutotileBrushDescriptor()
            : base(typeof(AutotileBrush), typeof(AutotileBrushDesigner), typeof(AliasBrushDesigner))
        {
        }


        /// <inheritdoc/>
        protected internal override bool DrawPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            var brush = record.Brush as AutotileBrush;
            if (brush == null) {
                return false;
            }

            var tileset = brush.Tileset;
            if (tileset == null || tileset.AtlasTexture == null) {
                return false;
            }

            // Use autotile artwork to render preview when no inner joins are
            // specified for better preview.
            if (!tileset.HasInnerJoins && tileset.rawTexture != null) {
                if (Event.current.type == EventType.Repaint) {
                    GUI.DrawTexture(output, tileset.rawTexture, UnityEngine.ScaleMode.StretchToFill, true);
                }
                return true;
            }

            if (tileset.TileWidth == 0 || tileset.TileHeight == 0 || tileset.Columns == 0) {
                return false;
            }

            if (Event.current.type == EventType.Repaint) {
                GUI.DrawTextureWithTexCoords(output, tileset.AtlasTexture, tileset.CalculateTexCoords(15), true);
            }

            return true;
        }
    }
}
