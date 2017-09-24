// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Tool for picking the brush that was used to paint an existing tile.
    /// </summary>
    /// <intro>
    /// <para>Please refer to the user guide for more information regarding the
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Picker-Tool">Picker Tool</a>.</para>
    /// </intro>
    public class PickerTool : ToolBase
    {
        private readonly string label = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Tool Name", "Picker"), "I"
        );


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolPicker; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolPicker); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get { return ToolCursors.Picker; }
        }

        /// <inheritdoc/>
        public override void OnEnable()
        {
            ToolUtility.ShowBrushPalette(false);
        }

        #endregion


        #region Tool Interaction

        /// <inheritdoc/>
        public override void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
            base.OnRefreshToolEvent(e, context);

            if (Event.current.isMouse) {
                ToolUtility.ActivePlop = null;

                // "Can pick plops"
                if (this.CanPickPlops) {
                    var go = HandleUtility.PickGameObject(Event.current.mousePosition, true);
                    if (go != null) {
                        ToolUtility.ActivePlop = go.GetComponentInParent<PlopInstance>();
                        if (ToolUtility.ActivePlop != null) {
                            // "Interact with active system only"
                            if (this.InteractWithActiveSystemOnly && ToolUtility.ActivePlop.Owner != context.TileSystem) {
                                ToolUtility.ActivePlop = null;
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void OnTool(ToolEvent e, IToolContext context)
        {
            switch (e.Type) {
                case EventType.MouseDown:
                    ToolBase fallbackRestoreTool;

                    Brush pickedBrush = null;

                    if (ToolUtility.ActivePlop != null && ToolUtility.ActivePlop.Brush != null) {
                        fallbackRestoreTool = ToolManager.Instance.Find<PlopTool>();

                        // Get plop at pointer.
                        pickedBrush = ToolUtility.ActivePlop.Brush;
                        // Pick rotation from tile also!
                        ToolUtility.Rotation = ToolUtility.ActivePlop.PaintedRotation;
                    }
                    else {
                        fallbackRestoreTool = ToolManager.DefaultPaintTool;

                        // Get tile at pointer.
                        var tile = context.TileSystem.GetTile(e.MousePointerTileIndex);
                        if (tile != null) {
                            pickedBrush = tile.brush;

                            // Pick rotation from tile also!
                            ToolUtility.Rotation = tile.PaintedRotation;
                        }
                    }

                    // Select brush in tool window and force auto scroll.
                    if (e.IsLeftButtonPressed) {
                        ToolUtility.SelectedBrush = pickedBrush;
                        ToolUtility.RevealBrush(pickedBrush);
                    }
                    else {
                        ToolUtility.SelectedBrushSecondary = pickedBrush;
                    }

                    ToolUtility.RepaintBrushPalette();

                    // Switch to previous tool or the "Paint" tool.
                    var toolManager = ToolManager.Instance;
                    if (toolManager.PreviousTool != null && toolManager.PreviousTool != this) {
                        toolManager.CurrentTool = toolManager.PreviousTool;
                    }
                    else {
                        toolManager.CurrentTool = fallbackRestoreTool;
                    }

                    break;
            }
        }

        #endregion


        #region Tool Options

        /// <inheritdoc/>
        protected override void PrepareOptions(ISettingStore store)
        {
            base.PrepareOptions(store);

            this.settingCanPickPlops = store.Fetch<bool>("CanPickPlops", true);
            this.settingInteractWithActiveSystemOnly = store.Fetch<bool>("InteractWithActiveSystemOnly", true);
        }


        private Setting<bool> settingCanPickPlops;
        private Setting<bool> settingInteractWithActiveSystemOnly;


        /// <summary>
        /// Gets or sets whether tool can pick brush from plops.
        /// </summary>
        public bool CanPickPlops {
            get { return this.settingCanPickPlops.Value; }
            set { this.settingCanPickPlops.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether brush should only be picked from plops that are
        /// associated with the active tile system.
        /// </summary>
        public bool InteractWithActiveSystemOnly {
            get { return this.settingInteractWithActiveSystemOnly.Value; }
            set { this.settingInteractWithActiveSystemOnly.Value = value; }
        }

        #endregion


        #region Tool Options Interface

        /// <inheritdoc/>
        public override void OnAdvancedToolOptionsGUI()
        {
            // Repaint scene views when options are changed so that handles updated.
            EditorGUI.BeginChangeCheck();

            this.CanPickPlops = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Can pick plops"), this.CanPickPlops);
            ++EditorGUI.indentLevel;
            if (this.CanPickPlops) {
                this.InteractWithActiveSystemOnly = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Interact with active system only"), this.InteractWithActiveSystemOnly);
            }
            --EditorGUI.indentLevel;

            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }
        }

        #endregion


        #region Scene View

        /// <inheritdoc/>
        protected override NozzleIndicator GetNozzleIndicator(TileSystem system, TileIndex index, BrushNozzle nozzle)
        {
            NozzleIndicator mode = RtsPreferences.ToolPreferredNozzleIndicator;

            if (mode == NozzleIndicator.Automatic) {
                mode = NozzleIndicator.Flat;

                // Determine based upon active tile.
                var tile = system.GetTile(index);
                if (tile != null && tile.brush != null && tile.brush.UseWireIndicatorInEditor) {
                    mode = NozzleIndicator.Wireframe;
                }
            }

            return mode;
        }

        #endregion
    }
}
