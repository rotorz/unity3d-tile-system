// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Spray tool gradually fills area of brush nozzle with tiles and can optionally
    /// randomize tile rotation.
    /// </summary>
    public class SprayTool : PaintTool
    {
        private readonly string label = TileLang.ParticularText("Tool Name", "Spray");


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolSpray; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolSpray); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get { return ToolCursors.Spray; }
        }

        #endregion


        #region Tool Options

        /// <inheritdoc/>
        protected override void PrepareOptions(ISettingStore store)
        {
            base.PrepareOptions(store);

            this.settingFillRatePercentage = store.Fetch<int>("FillRatePercentage", 50,
                filter: value => Mathf.Clamp(value, 0, 100)
            );
            this.settingRandomizeRotation = store.Fetch<bool>("RandomizeRotation", false);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>The spray tool has a default nozzle size of 5 since smaller nozzle sizes
        /// are far less useful.</para>
        /// </remarks>
        public override int DefaultNozzleSize {
            get { return 5; }
        }


        private Setting<int> settingFillRatePercentage;
        private Setting<bool> settingRandomizeRotation;

        /// <summary>
        /// Gets or sets fill rate of spray tool; a value within the range 0 to 100.
        /// </summary>
        public int FillRatePercentage {
            get { return this.settingFillRatePercentage.Value; }
            set { this.settingFillRatePercentage.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether rotation of tiles should be randomized whilst painting tiles.
        /// </summary>
        public bool RandomizeRotation {
            get { return this.settingRandomizeRotation.Value; }
            set { this.settingRandomizeRotation.Value = value; }
        }

        #endregion


        #region Tool Options Interface

        /// <inheritdoc/>
        public override void OnToolOptionsGUI()
        {
            this.TemporarilyDisableVariationShifting = ToolUtility.RandomizeVariations;

            base.OnToolOptionsGUI();

            this.FillRatePercentage = EditorGUILayout.IntSlider(TileLang.ParticularText("Property", "Fill Rate (%)"), this.FillRatePercentage, 0, 100);

            GUILayout.Space(3);
            this.RandomizeRotation = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Randomize Rotation"), this.RandomizeRotation);
        }

        #endregion

        /// <inheritdoc/>
        protected override PaintingArgs GetPaintingArgs(Brush brush)
        {
            var args = base.GetPaintingArgs(brush);
            args.fillRatePercentage = this.FillRatePercentage;
            args.randomizeRotation = this.RandomizeRotation;
            return args;
        }
    }
}
