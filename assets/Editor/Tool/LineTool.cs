// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Line tool for painting lines of tiles.
    /// </summary>
    /// <intro>
    /// <para>Please refer to the user guide for more information regarding the
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Line-Tool">Line Tool</a>.</para>
    /// </intro>
    public class LineTool : PaintToolBase
    {
        private readonly string label = TileLang.ParticularText("Tool Name", "Line");


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolLine; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolLine); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get { return ToolCursors.Line; }
        }

        #endregion


        #region Tool Interaction

        /// <inheritdoc/>
        public override void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
            if (this.IsTargetPointConstrained) {
                TileIndex targetIndex = e.MousePointerTileIndex;

                // Determine whether to constrain horizontally or vertically.
                int lineRowCount = Mathf.Abs(targetIndex.row - anchorIndex.row);
                int lineColumnCount = Mathf.Abs(targetIndex.column - anchorIndex.column);
                if (lineRowCount < lineColumnCount) {
                    targetIndex.row = anchorIndex.row;
                }
                else {
                    targetIndex.column = anchorIndex.column;
                }

                e.MousePointerTileIndex = targetIndex;
            }

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

            // Lines are only painted upon releasing mouse button.
            // See: LineTool.OnTool implementation.

            try {
                // Avoid updating procedural meshes multiple times whilst painting tiles.
                // In most circumstances this would not happen anyhow, but for performance
                // it's better to play it safe.
                tileSystem.BeginProceduralEditing();

                var brush = (e.WasLeftButtonPressed ? ToolUtility.SelectedBrush : ToolUtility.SelectedBrushSecondary);

                // Draw line from last anchor point.
                this.PaintLine(
                    system: tileSystem,
                    from: this.anchorIndex,
                    to: e.MousePointerTileIndex,
                    brush: brush
                );
            }
            finally {
                tileSystem.EndProceduralEditing();
            }
        }

        #endregion


        #region Scene View

        /// <inheritdoc/>
        public override void OnSceneGUI(ToolEvent e, IToolContext context)
        {
            if (!IsEditorNearestControl) {
                return;
            }

            if (context.TileSystem == this.anchorSystem) {
                this.DrawNozzleLine(this.anchorSystem, this.anchorIndex, e.MousePointerTileIndex);
            }

            this.DrawNozzleIndicator(context.TileSystem, e.MousePointerTileIndex, ToolUtility.BrushNozzle, this.NozzleSize);
        }

        #endregion
    }
}
