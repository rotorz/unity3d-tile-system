// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes tileset brush.
    /// </summary>
    internal class TilesetBrushDescriptor : BrushDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TilesetBrushDescriptor"/> class.
        /// </summary>
        public TilesetBrushDescriptor(Type brushType, Type brushDesignerType, Type brushAliasDesignerType)
            : base(brushType, brushDesignerType, brushAliasDesignerType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesetBrushDescriptor"/> class.
        /// </summary>
        public TilesetBrushDescriptor()
            : base(typeof(TilesetBrush), typeof(TilesetBrushDesigner), typeof(AliasBrushDesigner))
        {
        }


        /// <inheritdoc/>
        protected internal override bool DrawPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            var brush = record.Brush as TilesetBrush;
            if (brush == null) {
                return false;
            }

            var tileset = brush.Tileset;
            if (tileset == null || tileset.AtlasTexture == null) {
                return false;
            }

            if (tileset.TileWidth == 0 || tileset.TileHeight == 0 || tileset.Columns == 0) {
                return false;
            }

            if (Event.current.type == EventType.Repaint) {
                GUI.DrawTextureWithTexCoords(output, tileset.AtlasTexture, tileset.CalculateTexCoords(brush.tileIndex), true);
            }

            return true;
        }

        /// <inheritdoc/>
        public override Brush DuplicateBrush(string name, BrushAssetRecord record)
        {
            // Assume default implementation?
            if (record.MainAsset == record.Brush) {
                return base.DuplicateBrush(name, record);
            }

            // Is main asset a tileset?
            if (record.MainAsset is Tileset) {
                var duplicateBrush = Object.Instantiate(record.Brush) as Brush;
                duplicateBrush.name = name;

                AssetDatabase.AddObjectToAsset(duplicateBrush, record.MainAsset);

                return duplicateBrush;
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool DeleteBrush(BrushAssetRecord record)
        {
            // Assume default implementation?
            if (record.MainAsset == record.Brush) {
                return base.DeleteBrush(record);
            }

            // Is main asset a tileset?
            if (record.MainAsset is Tileset) {
                // Just destroy brush object.
                Object.DestroyImmediate(record.Brush, true);
                BrushDatabase.Instance.ClearMissingRecords();
                return true;
            }

            return false;
        }
    }
}
