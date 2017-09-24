// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Standard tool for painting tiles. This tool also provides various shortcuts
    /// allowing users to draw lines and cycle between tile variations without
    /// switching to the dedicated tools.
    /// </summary>
    /// <intro>
    /// <para>Please refer to the user guide for more information regarding the
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Paint-Tool">Paint Tool</a>.</para>
    /// </intro>
    public class PaintTool : PaintToolBase
    {
        private readonly string label = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Tool Name", "Paint"), "B"
        );


        #region Tool Information

        private CursorInfo cursor;


        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolPaint; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolPaint); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get { return this.cursor; }
        }

        #endregion


        #region Tool Interaction

        /// <summary>
        /// Gets a value indicating whether line drawing mode is currently active for paint tool.
        /// </summary>
        /// <remarks>
        /// <para>The default <see cref="PaintTool"/> implementation of this property yields a
        /// value of <c>true</c> when the shift key is held.</para>
        /// </remarks>
        protected virtual bool IsLineModeActive {
            get { return Event.current.shift; }
        }

        /// <summary>
        /// Gets a value indicating whether tool selection should be constrained for
        /// two-point operations. For instance, constrain line drawing to horizontal/vertical
        /// lines or rectangle drawing to squares.
        /// </summary>
        /// <remarks>
        /// <para>The default <see cref="PaintTool"/> implementation of this property yields a
        /// value of <c>true</c> when the control key is held.</para>
        /// </remarks>
        protected override bool IsTargetPointConstrained {
            get { return Event.current.control; }
        }

        /// <inheritdoc/>
        public override void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
            if (this.IsLineModeActive && this.IsTargetPointConstrained) {
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

            // Automatically switch between brush and line cursor.
            this.cursor = this.IsLineModeActive ? ToolCursors.Line : ToolCursors.Brush;

            // Is there any potential to cycle the active tile?
            if (ToolUtility.SelectedBrush != null && this.NozzleSize == 1 && (!e.IsLeftButtonPressed && !e.IsRightButtonPressed)) {
                var existingTile = context.TileSystem.GetTileOrNull(e.MousePointerTileIndex);
                if (existingTile != null && existingTile.brush == ToolUtility.SelectedBrush) {
                    // Find out if tile has multiple variations.
                    int orientation = OrientationUtility.DetermineTileOrientation(context.TileSystem, e.MousePointerTileIndex, ToolUtility.SelectedBrush, existingTile.PaintedRotation);
                    int variationCount = ToolUtility.SelectedBrush.CountTileVariations(orientation);
                    if (variationCount > 1) {
                        this.cursor = ToolCursors.Cycle;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void OnTool(ToolEvent e, IToolContext context)
        {
            switch (e.Type) {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    // Skip overpaint to avoid painting same cell multiple times!
                    if (e.MousePointerTileIndex == this.lastPaintIndex) {
                        return;
                    }

                    try {
                        this.OnPaint(e, context);
                    }
                    finally {
                        this.lastPaintIndex = e.MousePointerTileIndex;
                    }
                    break;
            }
        }

        /// <summary>
        /// Raised by <see cref="OnTool"/> to perform painting upon pressing left or right
        /// mouse button and then occurs again each time the mouse is dragged.
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

                var brush = (e.IsLeftButtonPressed ? ToolUtility.SelectedBrush : ToolUtility.SelectedBrushSecondary);

                if (e.Type == EventType.MouseDown) {
                    // Get existing tile instance from target cell.
                    var existingTile = tileSystem.GetTile(e.MousePointerTileIndex);
                    // Determine whether new tile will be unique.
                    bool willPaintUnique = existingTile == null || existingTile.IsGameObjectMissing || existingTile.brush == null || existingTile.brush != brush;

                    if (this.IsLineModeActive && tileSystem == this.anchorSystem) {
                        // Draw line from last anchor point when using shift key.
                        this.PaintLine(
                            system: tileSystem,
                            from: this.anchorIndex,
                            to: e.MousePointerTileIndex,
                            brush: brush
                        );
                    }
                    else if (!willPaintUnique && this.NozzleSize == 1) {
                        // Attempt to cycle tile on mouse down.
                        //
                        // Note: Always overpaint missing tile, however.
                        //
                        if (ToolUtility.Rotation != existingTile.PaintedRotation) {
                            existingTile.brush.CycleWithSimpleRotation(tileSystem, e.MousePointerTileIndex, ToolUtility.Rotation, existingTile.variationIndex);
                            // Cycle with rotation could affect the orientation of surrounding tiles.
                            // Therefore it is necessary to refresh them.
                            tileSystem.RefreshSurroundingTiles(e.MousePointerTileIndex);
                        }
                        else {
                            existingTile.brush.Cycle(tileSystem, e.MousePointerTileIndex, existingTile.variationIndex + 1);
                        }
                    }
                    else {
                        // Paint single tile.
                        this.PaintPoint(
                            system: tileSystem,
                            index: e.MousePointerTileIndex,
                            brush: brush
                        );
                    }
                }
                else {
                    this.PaintLine(
                        system: tileSystem,
                        from: this.lastPaintIndex,
                        to: e.MousePointerTileIndex,
                        brush: brush
                    );
                }
            }
            finally {
                tileSystem.EndProceduralEditing();

                this.anchorSystem = context.TileSystem;
                this.anchorIndex = e.MousePointerTileIndex;
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

            if (this.IsLineModeActive && context.TileSystem == this.anchorSystem) {
                this.DrawNozzleLine(this.anchorSystem, this.anchorIndex, e.MousePointerTileIndex);
            }

            this.DrawNozzleIndicator(context.TileSystem, e.MousePointerTileIndex, ToolUtility.BrushNozzle, this.NozzleSize);
        }

        #endregion
    }
}
