// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Base class for painting tools which provides easy access to painting arguments
    /// from tool options along with default gizmo drawing functionality.
    /// </summary>
    /// <seealso cref="ToolBase"/>
    public abstract class PaintToolBase : ToolBase
    {
        #region Tool Options

        /// <inheritdoc/>
        protected override void PrepareOptions(ISettingStore store)
        {
            base.PrepareOptions(store);

            this.settingNozzleSizeOption = store.Fetch<int>("NozzleSize", this.DefaultNozzleSize,
                filter: value => Mathf.Clamp(value, 1, 19)
            );
            this.settingNozzleSizeOption.ValueChanged += (args) => ToolUtility.RepaintToolPalette();
        }


        private Setting<int> settingNozzleSizeOption;


        /// <summary>
        /// Gets default size of nozzle.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation of this property simply returns a value of
        /// 1 though paint tools can provide a custom implementation to return a more
        /// suitable value.</para>
        /// </remarks>
        public virtual int DefaultNozzleSize {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets size of nozzle.
        /// </summary>
        /// <remarks>
        /// <para>Changes made to this property will also be reflected by <see cref="NozzleRadius"/>.
        /// Nozzle radius is calculated as <c>(NozzleSize + 1) / 2</c>.</para>
        /// </remarks>
        /// <seealso cref="NozzleRadius"/>
        public int NozzleSize {
            get { return this.settingNozzleSizeOption; }
            set { this.settingNozzleSizeOption.Value = value; }
        }

        /// <summary>
        /// Gets or sets radius of nozzle.
        /// </summary>
        /// <remarks>
        /// <para>Changes made to this property will also be reflected by <see cref="NozzleSize"/>.
        /// Nozzle size is calculated as <c>(NozzleRadius * 2) - 1</c>.</para>
        /// </remarks>
        /// <seealso cref="NozzleSize"/>
        public int NozzleRadius {
            get { return (this.NozzleSize + 1) / 2; }
            set { this.NozzleSize = value * 2 - 1; }
        }

        #endregion


        #region Tool Interaction

        /// <summary>
        /// Index of last tile which was painted.
        /// </summary>
        /// <remarks>
        /// <para>Set to <c>TileIndex.invalid</c> for none.</para>
        /// <para>Reverts to <c>TileIndex.invalid</c> when mouse button is released.</para>
        /// </remarks>
        protected TileIndex lastPaintIndex = TileIndex.invalid;

        /// <summary>
        /// Tile system which contains anchored tile.
        /// </summary>
        protected TileSystem anchorSystem;
        /// <summary>
        /// Anchored index of previously painted tile.
        /// </summary>
        protected TileIndex anchorIndex = TileIndex.invalid;

        /// <summary>
        /// Gets or sets whether variations can be shifted.
        /// </summary>
        protected bool EnableVariationShifting { get; set; }
        /// <summary>
        /// Gets or sets whether variation shifting interface should be temporarily
        /// disabled to avoid confusion in cases where shifting is unavailable.
        /// </summary>
        protected bool TemporarilyDisableVariationShifting { get; set; }


        private int variationShiftCount;

        /// <summary>
        /// Gets or sets count of variations to shift through.
        /// </summary>
        private int VariationShiftCount {
            get { return this.variationShiftCount; }
            set {
                if (value != this.variationShiftCount) {
                    this.variationShiftCount = value;
                    SceneView.RepaintAll();
                }
            }
        }


        /// <inheritdoc/>
        public override Vector3 PreFilterLocalPoint(Vector3 localPoint)
        {
            if (ToolUtility.BrushNozzle == BrushNozzle.Square && (this.NozzleSize & 0x01) == 0) {
                Vector3 cellSize = ToolUtility.ActiveTileSystem.CellSize;
                localPoint.x -= cellSize.x / 2f;
                localPoint.y += cellSize.y / 2f;
            }
            return localPoint;
        }

        /// <inheritdoc/>
        public override void OnEnable()
        {
            ToolUtility.ShowBrushPalette(false);

            this.RandomizeVariationShift();

            this.EnableVariationShifting = true;
        }

        /// <inheritdoc/>
        public override void OnToolInactive(ToolEvent e, IToolContext context)
        {
            // Clear index of previously painted tile.
            this.lastPaintIndex = TileIndex.invalid;
        }

        #endregion


        #region Tool Options Interface

        private static float s_WheelRadius;


        /// <inheritdoc/>
        public override void OnCheckKeyboardShortcuts()
        {
            bool usingNozzleRadius = (ToolUtility.BrushNozzle == BrushNozzle.Round);
            int nozzleSizeIncrement = usingNozzleRadius ? 2 : 1;

            switch (Event.current.type) {
                case EventType.ScrollWheel:
                    s_WheelRadius -= Event.current.delta.y;
                    if (Mathf.Abs(s_WheelRadius) >= 0.25f) {
                        this.OnMouseWheel(s_WheelRadius >= 0f ? +1f : -1f);
                        s_WheelRadius = 0f;
                    }
                    break;

                case EventType.KeyDown:
                    switch (Event.current.keyCode) {
                        // Increase/decrease brush radius:
                        case KeyCode.LeftBracket:
                            this.NozzleSize -= nozzleSizeIncrement;
                            ToolUtility.RepaintToolPalette();
                            SceneView.RepaintAll();
                            Event.current.Use();
                            break;
                        case KeyCode.RightBracket:
                            this.NozzleSize += nozzleSizeIncrement;
                            ToolUtility.RepaintToolPalette();
                            SceneView.RepaintAll();
                            Event.current.Use();
                            break;

                        // Increase/decrease variation offset.
                        case KeyCode.Alpha0:
                            if (this.EnableVariationShifting && !this.TemporarilyDisableVariationShifting) {
                                this.VariationShiftCount = 0;
                                Event.current.Use();
                            }
                            break;
                        case KeyCode.Minus:
                        case KeyCode.KeypadMinus:
                            if (this.EnableVariationShifting && !this.TemporarilyDisableVariationShifting) {
                                --this.VariationShiftCount;
                                Event.current.Use();
                            }
                            break;
                        case KeyCode.Plus:
                        case KeyCode.Equals:
                        case KeyCode.KeypadPlus:
                            if (this.EnableVariationShifting && !this.TemporarilyDisableVariationShifting) {
                                ++this.VariationShiftCount;
                                Event.current.Use();
                            }
                            break;
                    }
                    break;
            }
        }

        private void OnMouseWheel(float direction)
        {
            // Adjust nozzle size with Ctrl + Wheel.
            if (IsCommandKeyPressed) {
                bool usingNozzleRadius = (ToolUtility.BrushNozzle == BrushNozzle.Round);
                int nozzleSizeIncrement = usingNozzleRadius ? 2 : 1;
                if (direction > 0f) {
                    this.NozzleSize += nozzleSizeIncrement;
                }
                else {
                    this.NozzleSize -= nozzleSizeIncrement;
                }

                Event.current.Use();
            }
            // Adjust variation shift with Alt + Wheel.
            else if (Event.current.alt) {
                if (direction > 0f) {
                    ++this.VariationShiftCount;
                }
                else {
                    --this.VariationShiftCount;
                }

                Event.current.Use();
            }
        }

        /// <inheritdoc/>
        public override void OnToolOptionsGUI()
        {
            GUILayout.BeginHorizontal();
            {
                this.DrawStandardOptionsGUI();

                GUILayout.FlexibleSpace();

                GUILayout.Space(5);

                EditorGUI.BeginChangeCheck();

                using (var content = ControlContent.Basic(
                    RotorzEditorStyles.Skin.GetActive(RotorzEditorStyles.Skin.BrushRound, ToolUtility.BrushNozzle == BrushNozzle.Round),
                    TileLang.Text("Round Nozzle")
                )) {
                    Rect togglePosition = GUILayoutUtility.GetRect(28, 24);
                    if (RotorzEditorGUI.HoverToggle(togglePosition, content, ToolUtility.BrushNozzle == BrushNozzle.Round, RotorzEditorStyles.Instance.SmallFlatButton)) {
                        ToolUtility.BrushNozzle = BrushNozzle.Round;
                    }
                }

                using (var content = ControlContent.Basic(
                    RotorzEditorStyles.Skin.GetActive(RotorzEditorStyles.Skin.BrushSquare, ToolUtility.BrushNozzle == BrushNozzle.Square),
                    TileLang.Text("Square Nozzle")
                )) {
                    Rect togglePosition = GUILayoutUtility.GetRect(28, 24);
                    if (RotorzEditorGUI.HoverToggle(togglePosition, content, ToolUtility.BrushNozzle == BrushNozzle.Square, RotorzEditorStyles.Instance.SmallFlatButton)) {
                        ToolUtility.BrushNozzle = BrushNozzle.Square;
                    }
                }

                // Repaint scene views if nozzle index has changed.
                if (EditorGUI.EndChangeCheck()) {
                    SceneView.RepaintAll();
                }

                if (this.SupportsPaintAroundExistingTiles) {
                    RotorzEditorGUI.VerticalSeparatorLight();

                    this.DrawPaintAroundExistingTilesOption();
                }

                GUILayout.Space(3);
            }
            GUILayout.EndHorizontal();

            ExtraEditorGUI.SeparatorLight();

            bool usingNozzleRadius = (ToolUtility.BrushNozzle == BrushNozzle.Round);

            // Repaint scene views if brush radius has changed.
            EditorGUI.BeginChangeCheck();
            if (usingNozzleRadius) {
                this.NozzleRadius = EditorGUILayout.IntSlider(TileLang.ParticularText("Property", "Nozzle Radius"), this.NozzleRadius, 1, 10);
            }
            else {
                this.NozzleSize = EditorGUILayout.IntSlider(TileLang.ParticularText("Property", "Nozzle Size"), this.NozzleSize, 1, 19);
            }
            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Draw "Paint Around Existing Tiles" toggle.
        /// </summary>
        /// <remarks>
        /// <para>This method should be called within horizontally flowing layout.</para>
        /// </remarks>
        protected void DrawPaintAroundExistingTilesOption()
        {
            if (!this.SupportsPaintAroundExistingTiles) {
                throw new InvalidOperationException("This tool does not support the 'Paint Around Existing Tiles' option. Consider overriding `SupportsPaintAroundExistingTiles`.");
            }

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.GetActive(RotorzEditorStyles.Skin.RectanglePaintAround, ToolUtility.PaintAroundExistingTiles),
                TileLang.ParticularText("Property", "Paint Around Existing Tiles")
            )) {
                Rect position = GUILayoutUtility.GetRect(33, 24);
                ToolUtility.PaintAroundExistingTiles = RotorzEditorGUI.HoverToggle(position, content, ToolUtility.PaintAroundExistingTiles, RotorzEditorStyles.Instance.FlatButtonNoMargin);
            }
        }

        /// <summary>
        /// Draw standard options GUI using layout engine. This method should be called
        /// within a horizontally flowing layout.
        /// </summary>
        /// <example>
        /// <code language="csharp"><![CDATA[
        /// GUILayout.BeginHorizontal();
        /// {
        ///     DrawStandardOptionsGUI();
        /// }
        /// GUILayout.EndHorizontal();
        /// ]]></code>
        /// </example>
        protected void DrawStandardOptionsGUI()
        {
            Rect position = GUILayoutUtility.GetRect(77, 0);
            position.y -= 3;

            // Draw "Rotation" selector interface.
            EditorGUI.BeginChangeCheck();
            ToolUtility.Rotation = RotorzEditorGUI.RotationSelector(new Rect(position.x + 3, position.y, 39, 36), ToolUtility.Rotation);
            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.GetActive(RotorzEditorStyles.Skin.Randomize, ToolUtility.RandomizeVariations),
                TileLang.ParticularText("Property", "Randomize Variations")
            )) {
                Rect togglePosition = position;
                togglePosition.x += 3 + 39 + 5;
                togglePosition.y += 1;
                togglePosition.width = 32;
                togglePosition.height = 30;

                ToolUtility.RandomizeVariations = RotorzEditorGUI.HoverToggle(togglePosition, content, ToolUtility.RandomizeVariations);
            }

            if (this.EnableVariationShifting) {
                EditorGUI.BeginDisabledGroup(this.TemporarilyDisableVariationShifting);
                this.DrawVariationShifter();
                EditorGUI.EndDisabledGroup();
            }

            RotorzEditorGUI.VerticalSeparatorLight();
        }


        private readonly string _shiftToNextVariationTooltip = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Action", "Shift to Next Variation"),
            "+"
        );
        private readonly string _shiftToPrevVariationTooltip = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Action", "Shift to Previous Variation"),
            "-"
        );


        private void DrawVariationShifter()
        {
            Rect position = GUILayoutUtility.GetRect(17, 0);
            position.x += 2;
            position.height = 27;

            Rect plusButtonPosition = new Rect(position.x, position.y - 4, 17, 17);
            using (var content = ControlContent.Basic("", this._shiftToNextVariationTooltip)) {
                if (RotorzEditorGUI.HoverButton(plusButtonPosition, content)) {
                    ++this.VariationShiftCount;
                }
            }

            Rect minusButtonPosition = new Rect(position.x, position.yMax - 14, 17, 17);
            using (var content = ControlContent.Basic("", this._shiftToPrevVariationTooltip)) {
                if (RotorzEditorGUI.HoverButton(minusButtonPosition, content)) {
                    --this.VariationShiftCount;
                }
            }

            if (Event.current.type == EventType.Repaint) {
                Color restoreColor = GUI.color;
                if (!GUI.enabled) {
                    GUI.color = new Color(1f, 1f, 1f, 0.4f);
                }

                plusButtonPosition.x += 6;
                plusButtonPosition.y += 5;
                plusButtonPosition.width = 5;
                plusButtonPosition.height = 7;
                GUI.DrawTextureWithTexCoords(plusButtonPosition, RotorzEditorStyles.Skin.VariationOffsetSelector, new Rect(0f, 0.5f, 1f, 0.5f));

                minusButtonPosition.x += 6;
                minusButtonPosition.y += 5;
                minusButtonPosition.width = 5;
                minusButtonPosition.height = 7;
                GUI.DrawTextureWithTexCoords(minusButtonPosition, RotorzEditorStyles.Skin.VariationOffsetSelector, new Rect(0f, 0f, 1f, 0.5f));

                GUI.color = restoreColor;
            }
        }

        #endregion


        #region Scene View

        /// <inheritdoc/>
        public override void OnDrawGizmos(TileSystem system)
        {
            var brush = (PreviousToolEvent != null && PreviousToolEvent.IsRightButtonPressed)
                ? ToolUtility.SelectedBrushSecondary
                : ToolUtility.SelectedBrush;
            if (brush == null) {
                return;
            }

            // Do not draw immediate preview when larger radius is selected.
            if (this.NozzleSize > 1) {
                return;
            }

            // Only draw immediate preview when not disabled on per-brush basis.
            if (!brush.disableImmediatePreview) {
                // Preview is forced when see-through material is used.
                bool force = ImmediatePreviewUtility.IsSeeThroughPreviewMaterial;

                // Only display immediate preview if it differs from actual tile.
                var existingTile = ToolUtility.ActiveTile;
                if (existingTile == null || existingTile.brush != brush || existingTile.PaintedRotation != ToolUtility.Rotation || force) {
                    var context = Brush.GetSharedContext(brush, system, ToolUtility.ActiveTileIndex);
                    var previewTile = ImmediatePreviewUtility.GetPreviewTileData(context, brush, ToolUtility.Rotation);

                    var args = this.GetPaintingArgs(brush);
                    if (args.variation == Brush.RANDOM_VARIATION) {
                        args.variation = this.PreRandomizeVariation(brush, previewTile.orientationMask);
                    }
                    previewTile.variationIndex = (byte)args.ResolveVariation(previewTile.orientationMask);

                    ImmediatePreviewUtility.Matrix = system.transform.localToWorldMatrix;
                    brush.OnDrawImmediatePreview(context, previewTile, ImmediatePreviewUtility.PreviewMaterial, brush);
                }
            }
        }

        /// <summary>
        /// Draw line between two tile indices taking nozzle area into consideration.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of first tile.</param>
        /// <param name="to">Index of second tile.</param>
        protected virtual void DrawNozzleLine(TileSystem system, TileIndex from, TileIndex to)
        {
            Vector3 fromPoint = this.PreFilterLocalPoint(system.LocalPositionFromTileIndex(from));
            Vector3 toPoint = this.PreFilterLocalPoint(system.LocalPositionFromTileIndex(to));

            Vector3 cellSize = system.CellSize;
            if (ToolUtility.BrushNozzle == BrushNozzle.Square && (this.NozzleSize & 0x01) == 0) {
                fromPoint.x += cellSize.x;
                fromPoint.y -= cellSize.y;
                toPoint.x += cellSize.x;
                toPoint.y -= cellSize.y;
            }

            ToolHandleUtility.DrawLineHandles(fromPoint, toPoint, Color.white);
        }

        #endregion


        /// <summary>
        /// Gets a value indicating whether this tool supports the "Paint Around Existing Tiles" feature.
        /// </summary>
        protected virtual bool SupportsPaintAroundExistingTiles {
            get { return true; }
        }


        /// <summary>
        /// Gets painting arguments for tool. This method can be overridden to take further
        /// control over the way in which tiles are painted.
        /// </summary>
        /// <example>
        /// <para>Override fill rate of painting arguments for a custom spray tool:</para>
        /// <code language="csharp"><![CDATA[
        /// protected override PaintingArgs GetPaintingArgs(Brush brush)
        /// {
        ///     var args = base.GetPaintingArgs(brush);
        ///     args.fillRatePercentage = 50; // 50%
        ///     return args;
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="brush">Brush to paint with or specify <c>null</c> to erase existing tiles.</param>
        /// <returns>
        /// Painting arguments.
        /// </returns>
        protected virtual PaintingArgs GetPaintingArgs(Brush brush)
        {
            var args = PaintingArgs.GetDefaults(brush);
            if (this.SupportsPaintAroundExistingTiles) {
                args.paintAroundExistingTiles = ToolUtility.PaintAroundExistingTiles;
            }
            args.rotation = ToolUtility.Rotation;
            args.variation = ToolUtility.RandomizeVariations ? Brush.RANDOM_VARIATION : 0;
            if (this.EnableVariationShifting && !this.TemporarilyDisableVariationShifting) {
                args.variationShiftCount = this.VariationShiftCount;
            }
            return args;
        }

        /// <summary>
        /// Paint line of tiles using tool configuration.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="from">Index of tile at start of line.</param>
        /// <param name="to">Index of tile at end of line.</param>
        /// <param name="brush">Brush to paint with or specify <c>null</c> to erase existing tiles.</param>
        protected void PaintLine(TileSystem system, TileIndex from, TileIndex to, Brush brush)
        {
            var args = this.GetPaintingArgs(brush);

            if (args.variation == Brush.RANDOM_VARIATION) {
                // Use pre-randomized variation when painting individual tiles?
                if (brush != null && from == to && this.NozzleSize == 1) {
                    int orientationMask = OrientationUtility.DetermineTileOrientation(system, from, brush, args.rotation);
                    args.variation = this.PreRandomizeVariation(brush, orientationMask);
                    this.RandomizeVariationShift();
                }
            }

            switch (ToolUtility.BrushNozzle) {
                default:
                case BrushNozzle.Round:
                    PaintingUtility.StrokeLineWithCircle(
                        system: system,
                        from: from,
                        to: to,
                        radius: this.NozzleRadius,
                        args: args
                    );
                    break;

                case BrushNozzle.Square:
                    PaintingUtility.StrokeLineWithSquare(
                        system: system,
                        from: from,
                        to: to,
                        size: this.NozzleSize,
                        args: args
                    );
                    break;
            }
        }

        /// <summary>
        /// Paint single tile using tool configuration.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile to paint.</param>
        /// <param name="brush">Brush to paint with or specify <c>null</c> to erase existing tiles.</param>
        protected void PaintPoint(TileSystem system, TileIndex index, Brush brush)
        {
            this.PaintLine(system, index, index, brush);
        }


        private Dictionary<int, int> nextRandomVariationCache = new Dictionary<int, int>();

        /// <summary>
        /// Pre-randomize tile variation for specific orientation.
        /// </summary>
        /// <param name="brush">Brush which is being used to paint tile.</param>
        /// <param name="orientationMask">Bitmask identifying orientation of target tile.</param>
        /// <returns>
        /// Zero-based index of tile variation.
        /// </returns>
        /// <seealso cref="RandomizeVariationShift()"/>
        protected int PreRandomizeVariation(Brush brush, int orientationMask)
        {
            int variationIndex = 0;

            if (brush != null && brush.PerformsAutomaticOrientation) {
                // Pre-randomized variations are cached for each candidate orientation
                // so that immediate previews are consistent as mouse pointer is moved
                // to different locations in active tile system.
                if (!this.nextRandomVariationCache.TryGetValue(0, out variationIndex)) {
                    variationIndex = brush.PickRandomVariationIndex(orientationMask);
                    this.nextRandomVariationCache[0] = variationIndex;
                }
            }
            else {
                variationIndex = Brush.RANDOM_VARIATION;
            }

            return variationIndex;
        }

        /// <summary>
        /// Randomize shift of variation for next tile.
        /// </summary>
        /// <seealso cref="PreRandomizeVariation(Brush, int)"/>
        protected void RandomizeVariationShift()
        {
            this.nextRandomVariationCache.Clear();
            this.variationShiftCount = 0;
        }
    }
}
