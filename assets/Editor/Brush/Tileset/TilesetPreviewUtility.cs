// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal sealed class TilesetPreviewUtility
    {
        private readonly IRepaintableUI repaintableUI;

        private int previewColumnCount = 1;
        private int previewRowCount;

        private float previewAreaHeight;


        public TilesetPreviewUtility(IRepaintableUI repaintableUI)
        {
            this.repaintableUI = repaintableUI;
        }


        public bool DrawTilePreviews(Texture2D tileset, ITilesetMetrics metrics, int tileCount = 0)
        {
            if (tileset == null || metrics == null || metrics.TileWidth == 0 || metrics.TileHeight == 0) {
                return false;
            }

            int tilesetCount = metrics.Rows * metrics.Columns;
            bool tilesetCountCapped = false;

            // Limit number of tiles that are displayed.
            // Note: Do not exceed 2500 because that starts to become slow!
            if (tileCount > 0) {
                tilesetCount = Mathf.Min(tileCount, tilesetCount);
            }
            if (tilesetCount > 2500) {
                tilesetCount = 2500;
                tilesetCountCapped = true;
            }

            int previewSpacing = 5;
            int previewOffsetX = metrics.TileWidth + previewSpacing + 2;
            int previewOffsetY = metrics.TileHeight + previewSpacing + 2;

            Rect r = EditorGUILayout.BeginVertical();
            if (r.width > 0) {
                this.previewColumnCount = (int)r.width / previewOffsetX;
                this.previewRowCount = Mathf.CeilToInt((float)tilesetCount / (float)this.previewColumnCount);
            }

            if (this.previewColumnCount == 0) {
                GUILayout.Space(1);
                EditorGUILayout.EndVertical();
                return false;
            }

            // Repaint window if preview area has changed.
            float previewAreaHeight = this.previewRowCount * previewOffsetY;
            if (previewAreaHeight != this.previewAreaHeight) {
                this.previewAreaHeight = previewAreaHeight;
                this.repaintableUI.Repaint();
            }
            GUILayout.Space(this.previewAreaHeight);

            // Get rectangle for outputting previews.
            Rect output = new Rect(r.x, r.y, metrics.TileWidth + 2, metrics.TileHeight + 2);

            float texX = metrics.BorderU;

            Rect texCoords = new Rect(
                texX,
                1f - (metrics.BorderV + metrics.TileHeightUV),
                metrics.TileWidthUV,
                metrics.TileHeightUV
            );

            GUIStyle boxStyle = GUI.skin.box;

            for (int i = 0; i < tilesetCount; ++i) {
                if (i != 0) {
                    if (i % this.previewColumnCount == 0) {
                        output.x = r.x;
                        output.y += previewOffsetY;
                    }

                    if (i % metrics.Columns == 0) {
                        texCoords.x = texX;
                        texCoords.y -= metrics.TileIncrementV;
                    }
                }

                if (Event.current.type == EventType.Repaint) {
                    boxStyle.Draw(output, false, false, false, false);

                    GUI.DrawTextureWithTexCoords(
                        new Rect(output.x + 1, output.y + 1, output.width - 2, output.height - 2),
                        tileset,
                        texCoords
                    );
                }

                output.x += previewOffsetX;
                texCoords.x += metrics.TileIncrementU;
            }

            // Display warning message?
            if (tilesetCountCapped) {
                EditorGUILayout.HelpBox(TileLang.Text("Not all tile previews have been displayed to avoid poor performance."), MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            return true;
        }

        public bool DrawTilePreviews(AutotileLayout layout, Texture2D tileset, bool innerJoins, ITilesetMetrics metrics)
        {
            int tileCount = 0;

            switch (layout) {
                //case AutotileStyle.tIDE:
                //    tileCount = 16;
                //    break;
                case AutotileLayout.Basic:
                    tileCount = innerJoins ? 47 : 16;
                    break;
                case AutotileLayout.Extended:
                    tileCount = innerJoins ? 48 : 16;
                    break;
            }

            if (tileCount == 0) {
                return false;
            }

            return this.DrawTilePreviews(tileset, metrics, tileCount);
        }
    }
}
