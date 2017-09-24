// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Tile.Internal;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Rectangle tool for painting filled or outlined rectangles of tiles.
    /// </summary>
    /// <intro>
    /// <para>Please refer to the user guide for more information regarding the
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Rectangle-Tool">Rectangle Tool</a>.</para>
    /// </intro>
    public class RectangleTool : PaintToolBase
    {
        private readonly string label = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Tool Name", "Rectangle"), "U"
        );


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolRectangle; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolRectangle); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get { return ToolCursors.Rectangle; }
        }

        #endregion


        #region Tool Interaction

        /// <inheritdoc/>
        public override void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
            // Allow user to cancel painting by tapping escape key.
            if (e.Type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                this.anchorSystem = null;
                Event.current.Use();
            }
        }

        /// <inheritdoc/>
        public override void OnTool(ToolEvent e, IToolContext context)
        {
            switch (e.Type) {
                case EventType.MouseDown:
                    if (this.anchorSystem != context.TileSystem && (e.IsLeftButtonPressed || e.IsRightButtonPressed)) {
                        this.anchorIndex = e.MousePointerTileIndex;
                        this.anchorSystem = context.TileSystem;
                    }
                    else {
                        // Allow another mouse button to cancel painting.
                        this.anchorSystem = null;
                    }
                    break;

                case EventType.MouseUp:
                    if (context.TileSystem == this.anchorSystem) {
                        this.anchorSystem = null;

                        this.OnPaint(e, context);
                    }
                    break;
            }
        }

        /// <summary>
        /// Raised by <see cref="OnTool"/> to perform painting upon releasing left or right
        /// mouse button when a tile has been anchored on the active tile system.
        /// </summary>
        /// <param name="e">Tool event data.</param>
        /// <param name="context">Context that tool is being used in.</param>
        protected virtual void OnPaint(ToolEvent e, IToolContext context)
        {
            var tileSystem = context.TileSystem;

            try {
                // Avoid updating procedural meshes multiple times whilst painting tiles.
                // In most circumstances this would not happen anyhow, but for performance
                // it's better to play it safe.
                tileSystem.BeginProceduralEditing();

                var brush = (e.WasLeftButtonPressed ? ToolUtility.SelectedBrush : ToolUtility.SelectedBrushSecondary);

                TileIndex from, to;
                MathUtility.GetRectangleBoundsClamp(tileSystem, this.anchorIndex, e.MousePointerTileIndex, out from, out to, this.IsTargetPointConstrained);

                if (from != to) {
                    PaintingUtility.PaintRectangle(tileSystem, from, to, ToolUtility.FillCenter, this.GetPaintingArgs(brush));
                }
                else {
                    this.PaintPoint(tileSystem, from, brush);
                }
            }
            finally {
                tileSystem.EndProceduralEditing();
            }
        }

        #endregion


        #region Tool Options Interface

        /// <inheritdoc/>
        public override void OnToolOptionsGUI()
        {
            Rect position;

            GUILayout.BeginHorizontal();
            this.DrawStandardOptionsGUI();

            GUILayout.FlexibleSpace();

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.GetActive(RotorzEditorStyles.Skin.RectangleFill, ToolUtility.FillCenter),
                TileLang.ParticularText("Property", "Fill Center")
            )) {
                position = GUILayoutUtility.GetRect(21 + 12, 19 + 5);
                if (RotorzEditorGUI.HoverToggle(position, content, ToolUtility.FillCenter, RotorzEditorStyles.Instance.FlatButtonNoMargin)) {
                    ToolUtility.FillCenter = true;
                }
            }

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.GetActive(RotorzEditorStyles.Skin.RectangleOutline, !ToolUtility.FillCenter),
                TileLang.ParticularText("Property", "Draw Outline")
            )) {
                position = GUILayoutUtility.GetRect(21 + 12, 19 + 5);
                if (RotorzEditorGUI.HoverToggle(position, content, !ToolUtility.FillCenter, RotorzEditorStyles.Instance.FlatButtonNoMargin)) {
                    ToolUtility.FillCenter = false;
                }
            }

            RotorzEditorGUI.VerticalSeparatorLight();

            // "Paint Around Existing Tiles"
            this.DrawPaintAroundExistingTilesOption();

            GUILayout.Space(3);

            GUILayout.EndHorizontal();

            ExtraEditorGUI.SeparatorLight();
        }

        #endregion


        #region Scene View

        /// <inheritdoc/>
        protected override void DrawNozzleIndicator(TileSystem system, TileIndex index, BrushNozzle nozzle, int radius)
        {
            base.DrawNozzleIndicator(system, index, BrushNozzle.Square, 1);

            if (system == this.anchorSystem) {
                if (ToolUtility.FillCenter) {
                    ToolHandleUtility.DrawRectangle(system, this.anchorIndex, index, this.IsTargetPointConstrained);
                }
                else {
                    ToolHandleUtility.DrawRectangleBorder(system, this.anchorIndex, index, this.IsTargetPointConstrained);
                }
            }
        }

        #endregion
    }
}
