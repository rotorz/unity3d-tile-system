// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Tool for cycling through variations of tiles and plops which are painted using
    /// brushes which have multiple variations.
    /// </summary>
    /// <intro>
    /// <para>Please refer to the user guide for more information regarding the
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Cycle-Tool">Cycle Tool</a>.</para>
    /// </intro>
    public class CycleTool : ToolBase
    {
        private readonly string label = TileLang.ParticularText("Tool Name", "Cycle");


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolCycle; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolCycle); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get {
                return ToolUtility.ActivePlop != null
                    ? ToolCursors.PlopCycle
                    : ToolCursors.Cycle;
            }
        }

        #endregion


        #region Tool Interaction

        /// <inheritdoc/>
        public override void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
            base.OnRefreshToolEvent(e, context);

            if (Event.current.isMouse) {
                ToolUtility.ActivePlop = null;

                // "Can cycle plops"
                if (this.CanCyclePlops) {
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
                    // Note: Left button for next; right button for previous.
                    int offset = 0;
                    if (e.IsLeftButtonPressed) {
                        offset = -1;
                    }
                    else if (e.IsRightButtonPressed) {
                        offset = +1;
                    }

                    if (ToolUtility.ActivePlop != null) {
                        // Variation index to use next?
                        int nextVariation = ToolUtility.ActivePlop.VariationIndex + offset;
                        // Cycle through plop variations.
                        ToolUtility.ActivePlop = PlopUtility.CyclePlop(context.TileSystem, ToolUtility.ActivePlop, ToolUtility.ActivePlop.Brush, ToolUtility.ActivePlop.PaintedRotation, nextVariation);
                    }
                    else {
                        // Get tile at pointer
                        var tile = context.TileSystem.GetTile(e.MousePointerTileIndex);
                        if (tile == null || tile.brush == null) {
                            return;
                        }

                        // Variation index to use next?
                        int nextVariation = tile.variationIndex + offset;
                        // Cycle through tile variations.
                        tile.brush.CycleWithSimpleRotation(context.TileSystem, e.MousePointerTileIndex, tile.PaintedRotation, nextVariation);
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

            this.settingCanCyclePlops = store.Fetch<bool>("CanCyclePlops", true);
            this.settingInteractWithActiveSystemOnly = store.Fetch<bool>("InteractWithActiveSystemOnly", true);
        }


        private Setting<bool> settingCanCyclePlops;
        private Setting<bool> settingInteractWithActiveSystemOnly;


        /// <summary>
        /// Gets or sets whether tool can also cycle through plop variations.
        /// </summary>
        public bool CanCyclePlops {
            get { return this.settingCanCyclePlops.Value; }
            set { this.settingCanCyclePlops.Value = value; }
        }
        /// <summary>
        /// Gets or sets whether plops should only be cycled if they are associated with
        /// the active tile system.
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
            this.CanCyclePlops = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Can cycle plops"), this.CanCyclePlops);
            ++EditorGUI.indentLevel;
            if (this.CanCyclePlops) {
                this.InteractWithActiveSystemOnly = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Interact with active system only"), this.InteractWithActiveSystemOnly);
            }
            --EditorGUI.indentLevel;
        }

        #endregion
    }
}
