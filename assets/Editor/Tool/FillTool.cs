// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{

    /// <summary>
    /// Flood fill tool.
    /// </summary>
    /// <intro>
    /// <para>Please refer to the user guide for more information regarding the
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Fill-Tool">Fill Tool</a>.</para>
    /// </intro>
    /// <remarks>
    /// <para>The number of painted tiles is constrained to an upper limit to avoid
    /// crashing Unity editor when lots of tiles are to be painted. This limit can
    /// be customized inside the "Advanced" options section of main tool palette.</para>
    /// </remarks>
    public class FillTool : PaintToolBase
    {
        private readonly string label = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Tool Name", "Fill"), "G"
        );


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolFill; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolFill); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get { return ToolCursors.Fill; }
        }

        #endregion


        #region Tool Interaction

        /// <inheritdoc/>
        public override void OnTool(ToolEvent e, IToolContext context)
        {
            switch (e.Type) {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    var brush = e.IsLeftButtonPressed ? ToolUtility.SelectedBrush : ToolUtility.SelectedBrushSecondary;

                    int restoreMaximumFillCount = PaintingUtility.MaximumFillCount;
                    PaintingUtility.MaximumFillCount = this.MaximumFillCount;
                    try {
                        PaintingUtility.FloodFill(context.TileSystem, e.MousePointerTileIndex, this.GetPaintingArgs(brush));
                    }
                    finally {
                        PaintingUtility.MaximumFillCount = restoreMaximumFillCount;
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

            this.settingMaximumFillCount = store.Fetch<int>("MaximumFillCount", 300,
                filter: value => Mathf.Clamp(value, 1, 10000)
            );
        }


        private Setting<int> settingMaximumFillCount;


        /// <summary>
        /// Gets or sets maximum number of tiles which can be filled at once.
        /// </summary>
        public int MaximumFillCount {
            get { return this.settingMaximumFillCount.Value; }
            set { this.settingMaximumFillCount.Value = value; }
        }

        #endregion


        #region Tool Options Interface

        /// <inheritdoc/>
        public override void OnToolOptionsGUI()
        {
            this.TemporarilyDisableVariationShifting = ToolUtility.RandomizeVariations;

            GUILayout.BeginHorizontal();
            this.DrawStandardOptionsGUI();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ExtraEditorGUI.SeparatorLight();
        }

        /// <inheritdoc/>
        public override void OnAdvancedToolOptionsGUI()
        {
            this.MaximumFillCount = EditorGUILayout.IntField(TileLang.ParticularText("Property", "Fill Limit"), this.MaximumFillCount);
            ExtraEditorGUI.TrailingTip(TileLang.Text("Filling too many tiles may cause Unity to crash. Avoid setting this preference too high."));
        }

        #endregion


        /// <inheritdoc/>
        protected override bool SupportsPaintAroundExistingTiles {
            get { return false; }
        }
    }
}
