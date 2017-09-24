// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    [CustomEditor(typeof(TileSystemPreset))]
    [CanEditMultipleObjects]
    internal sealed class TileSystemPresetInspector : UnityEditor.Editor
    {
        #region User Settings

        private static void AutoInitializeUserSettings()
        {
            if (s_HasInitializedUserSettings == true) {
                return;
            }

            var settings = AssetSettingManagement.GetGroup("TileSystemPresetInspector");

            s_SectionStripping = settings.Fetch<bool>("ExpandStripping", false);
            s_SectionBuildOptions = settings.Fetch<bool>("ExpandBuildOptions", false);
            s_SectionRuntimeOptions = settings.Fetch<bool>("ExpandRuntimeOptions", false);

            s_HasInitializedUserSettings = true;
        }

        private static bool s_HasInitializedUserSettings = false;

        private static Setting<bool> s_SectionStripping;
        private static Setting<bool> s_SectionBuildOptions;
        private static Setting<bool> s_SectionRuntimeOptions;

        private static bool s_ToggleBuildOptions_AdvancedUV2;

        #endregion


        #region Serialized Properties

        private SerializedProperty propertySystemName;

        // Grid
        private SerializedProperty propertyTileWidth;
        private SerializedProperty propertyTileHeight;
        private SerializedProperty propertyTileDepth;

        private SerializedProperty propertyRows;
        private SerializedProperty propertyColumns;

        private SerializedProperty propertyChunkWidth;
        private SerializedProperty propertyChunkHeight;

        private SerializedProperty propertyAutoAdjustDirection;
        private SerializedProperty propertyTilesFacing;
        private SerializedProperty propertyDirection;

        // Stripping
        private SerializedProperty propertyStrippingPreset;
        private SerializedProperty propertyStrippingOptions;

        // Build Options
        private SerializedProperty propertyCombineMethod;
        private SerializedProperty propertyCombineChunkWidth;
        private SerializedProperty propertyCombineChunkHeight;
        private SerializedProperty propertyCombineIntoSubmeshes;

        private SerializedProperty propertyStaticVertexSnapping;
        private SerializedProperty propertyVertexSnapThreshold;

        private SerializedProperty propertyGenerateSecondUVs;
        private SerializedProperty propertySecondUVsHardAngle;
        private SerializedProperty propertySecondUVsPackMargin;
        private SerializedProperty propertySecondUVsAngleError;
        private SerializedProperty propertySecondUVsAreaError;

        private SerializedProperty propertyPregenerateProcedural;

        private SerializedProperty propertyReduceColliders;

        // Runtime
        private SerializedProperty propertyHintEraseEmptyChunks;
        private SerializedProperty propertyApplyRuntimeStripping;

        private SerializedProperty propertyUpdateProceduralAtStart;
        private SerializedProperty propertyMarkProceduralDynamic;
        private SerializedProperty propertyAddProceduralNormals;

        private SerializedProperty propertySortingLayerID;
        private SerializedProperty propertySortingOrder;

        private void InitializeSerializedProperties()
        {
            this.propertySystemName = this.serializedObject.FindProperty("systemName");

            // Grid
            this.propertyTileWidth = this.serializedObject.FindProperty("tileWidth");
            this.propertyTileHeight = this.serializedObject.FindProperty("tileHeight");
            this.propertyTileDepth = this.serializedObject.FindProperty("tileDepth");

            this.propertyRows = this.serializedObject.FindProperty("rows");
            this.propertyColumns = this.serializedObject.FindProperty("columns");

            this.propertyChunkWidth = this.serializedObject.FindProperty("chunkWidth");
            this.propertyChunkHeight = this.serializedObject.FindProperty("chunkHeight");

            this.propertyAutoAdjustDirection = this.serializedObject.FindProperty("autoAdjustDirection");
            this.propertyTilesFacing = this.serializedObject.FindProperty("tilesFacing");
            this.propertyDirection = this.serializedObject.FindProperty("direction");

            // Stripping
            this.propertyStrippingPreset = this.serializedObject.FindProperty("strippingPreset");
            this.propertyStrippingOptions = this.serializedObject.FindProperty("strippingOptions");

            // Build Options
            this.propertyCombineMethod = this.serializedObject.FindProperty("combineMethod");
            this.propertyCombineChunkWidth = this.serializedObject.FindProperty("combineChunkWidth");
            this.propertyCombineChunkHeight = this.serializedObject.FindProperty("combineChunkHeight");
            this.propertyCombineIntoSubmeshes = this.serializedObject.FindProperty("combineIntoSubmeshes");

            this.propertyStaticVertexSnapping = this.serializedObject.FindProperty("staticVertexSnapping");
            this.propertyVertexSnapThreshold = this.serializedObject.FindProperty("vertexSnapThreshold");

            this.propertyGenerateSecondUVs = this.serializedObject.FindProperty("generateSecondUVs");
            this.propertySecondUVsHardAngle = this.serializedObject.FindProperty("secondUVsHardAngle");
            this.propertySecondUVsPackMargin = this.serializedObject.FindProperty("secondUVsPackMargin");
            this.propertySecondUVsAngleError = this.serializedObject.FindProperty("secondUVsAngleError");
            this.propertySecondUVsAreaError = this.serializedObject.FindProperty("secondUVsAreaError");

            this.propertyPregenerateProcedural = this.serializedObject.FindProperty("pregenerateProcedural");

            this.propertyReduceColliders = this.serializedObject.FindProperty("reduceColliders");

            // Runtime
            this.propertyHintEraseEmptyChunks = this.serializedObject.FindProperty("hintEraseEmptyChunks");
            this.propertyApplyRuntimeStripping = this.serializedObject.FindProperty("applyRuntimeStripping");

            this.propertyUpdateProceduralAtStart = this.serializedObject.FindProperty("updateProceduralAtStart");
            this.propertyMarkProceduralDynamic = this.serializedObject.FindProperty("markProceduralDynamic");
            this.propertyAddProceduralNormals = this.serializedObject.FindProperty("addProceduralNormals");

            this.propertySortingLayerID = this.serializedObject.FindProperty("sortingLayerID");
            this.propertySortingOrder = this.serializedObject.FindProperty("sortingOrder");
        }

        #endregion


        /// <summary>
        /// Gets or sets a value indicating if the tile system name has been modified by
        /// the user.
        /// </summary>
        public bool HasModifiedTileSystemName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether undo should be disabled on the
        /// underlying serialized object if possible.
        /// </summary>
        public bool DisableUndoOnSerializedObject { get; set; }


        private void OnEnable()
        {
            AutoInitializeUserSettings();
            this.InitializeSerializedProperties();
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        protected override void OnHeaderGUI()
        {
            Rect position = GUILayoutUtility.GetRect(0, 46);
            if (Event.current.type == EventType.Repaint) {
                GUI.skin.box.Draw(position, GUIContent.none, false, false, false, false);
                GUI.DrawTexture(new Rect(7, 7, 32, 32), RotorzEditorStyles.Skin.Icon_PresetTileSystem);

                string headerText = targets.Length == 1
                    ? string.Format(
                        /* 0: name of tile system preset */
                        TileLang.Text("{0} (Tile System Preset)"),
                        target.name
                    )
                    : string.Format(
                        /* 0: quantity of selected tile system presets */
                        TileLang.Text("{0} Tile System Presets"),
                        targets.Length
                    );

                EditorStyles.largeLabel.Draw(new Rect(48, 7, position.width - 48, position.height), headerText, false, false, false, false);
            }

            Rect menuPosition = new Rect(position.width - 25, 7, 22, 16);
            if (GUI.Button(menuPosition, RotorzEditorStyles.Skin.SmallGearButton, GUIStyle.none)) {
                this.ShowContextMenu(menuPosition);
                GUIUtility.ExitGUI();
            }

            EditorGUI.BeginDisabledGroup(targets.Length != 1);
            {
                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Action", "Create Tile System")
                )) {
                    Vector2 createButtonSize = EditorStyles.miniButton.CalcSize(content);
                    Rect createButtonPosition = new Rect(position.width - createButtonSize.x - 5, 24, createButtonSize.x, createButtonSize.y);
                    if (GUI.Button(createButtonPosition, content, EditorStyles.miniButton)) {
                        var tileSystemGO = TileSystemPresetUtility.CreateTileSystemFromPreset((TileSystemPreset)target);
                        Selection.activeObject = tileSystemGO;
                        Undo.RegisterCreatedObjectUndo(tileSystemGO, content.LabelContent.text);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
        }

        private void ShowContextMenu(Rect menuPosition)
        {
            var menu = new EditorMenu();

            string labelResetToDefault3D = string.Format(
                /* 0: name of previous state */
                TileLang.ParticularText("Action", "Reset to '{0}'"),
                TileLang.ParticularText("Preset Name", "Default: 3D")
            );
            menu.AddCommand(labelResetToDefault3D)
                .Action(() => {
                    Undo.RecordObjects(targets, labelResetToDefault3D);
                    foreach (var target in targets) {
                        ((TileSystemPreset)target).SetDefaults3D();
                    }
                });

            string labelResetToDefault2D = string.Format(
                /* 0: name of previous state */
                TileLang.ParticularText("Action", "Reset to '{0}'"),
                TileLang.ParticularText("Preset Name", "Default: 2D")
            );
            menu.AddCommand(labelResetToDefault2D)
                .Action(() => {
                    Undo.RecordObjects(targets, labelResetToDefault2D);
                    foreach (var target in targets) {
                        ((TileSystemPreset)target).SetDefaults2D();
                    }
                });

            menu.ShowAsDropdown(menuPosition);
        }

        public override void OnInspectorGUI()
        {
            float initialLabelWidth = EditorGUIUtility.labelWidth;
            RotorzEditorGUI.UseExtendedLabelWidthForLocalization();

            this.serializedObject.Update();

            this.DrawTileSystemNameField();
            ExtraEditorGUI.SeparatorLight(marginTop: 7);

            this.OnSection_TileSystem();

            s_SectionStripping.Value = RotorzEditorGUI.FoldoutSection(s_SectionStripping,
                label: TileLang.ParticularText("Section", "Stripping"),
                callback: this.OnSection_Stripping
            );

            s_SectionBuildOptions.Value = RotorzEditorGUI.FoldoutSection(s_SectionBuildOptions,
                label: TileLang.ParticularText("Section", "Build Options"),
                callback: this.OnSection_BuildOptions
            );

            s_SectionRuntimeOptions.Value = RotorzEditorGUI.FoldoutSection(s_SectionRuntimeOptions,
                label: TileLang.ParticularText("Section", "Runtime Options"),
                callback: this.OnSection_RuntimeOptions
            );

            if (this.DisableUndoOnSerializedObject) {
                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            else {
                this.serializedObject.ApplyModifiedProperties();
            }

            EditorGUIUtility.labelWidth = initialLabelWidth;
        }

        private void DrawTileSystemNameField()
        {
            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile System Name"));
            GUI.SetNextControlName("NameField");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.propertySystemName, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) {
                this.HasModifiedTileSystemName = true;
            }
        }

        private void OnSection_TileSystem()
        {
            float initialLabelWidth = EditorGUIUtility.labelWidth;

            BeginMultiPartField(TileLang.ParticularText("Property", "Grid Size (in tiles)"));
            {
                EditorGUIUtility.labelWidth = 65;

                EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Rows"));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.propertyRows, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) {
                    this.propertyRows.intValue = Mathf.Max(1, this.propertyRows.intValue);
                }

                EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Columns"));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.propertyColumns, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) {
                    this.propertyColumns.intValue = Mathf.Max(1, this.propertyColumns.intValue);
                }

                EditorGUIUtility.labelWidth = initialLabelWidth;
            }
            EndMultiPartField();

            BeginMultiPartField(TileLang.ParticularText("Property", "Chunk Size (in tiles)"));
            {
                EditorGUIUtility.labelWidth = 65;

                EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Height"));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.propertyChunkHeight, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) {
                    this.propertyChunkHeight.intValue = Mathf.Max(1, this.propertyChunkHeight.intValue);
                }

                EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Width"));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.propertyChunkWidth, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) {
                    this.propertyChunkWidth.intValue = Mathf.Max(1, this.propertyChunkWidth.intValue);
                }

                EditorGUIUtility.labelWidth = initialLabelWidth;
            }
            EndMultiPartField();

            if (!this.propertyChunkHeight.hasMultipleDifferentValues && !this.propertyChunkWidth.hasMultipleDifferentValues) {
                if (this.propertyChunkHeight.intValue * this.propertyChunkWidth.intValue > 10000) {
                    RotorzEditorGUI.InfoBox(TileLang.Text("Do not exceed an area of 100x100 tiles per chunk when using procedural tilesets."), MessageType.Warning);
                }
            }

            if (ControlContent.TrailingTipsVisible) {
                ExtraEditorGUI.TrailingTip(TileLang.Text("Number of tiles that contribute to a chunk."));
            }

            ExtraEditorGUI.SeparatorLight();

            BeginMultiPartField(TileLang.ParticularText("Property", "Cell Size"));
            {
                EditorGUI.showMixedValue = this.propertyTileWidth.hasMultipleDifferentValues || this.propertyTileHeight.hasMultipleDifferentValues || this.propertyTileDepth.hasMultipleDifferentValues;

                Vector3 cellSize = new Vector3(this.propertyTileWidth.floatValue, this.propertyTileHeight.floatValue, this.propertyTileDepth.floatValue);
                EditorGUI.BeginChangeCheck();
                cellSize = EditorGUILayout.Vector3Field(GUIContent.none, cellSize);
                if (EditorGUI.EndChangeCheck()) {
                    this.propertyTileWidth.floatValue = Mathf.Max(0.0001f, cellSize.x);
                    this.propertyTileHeight.floatValue = Mathf.Max(0.0001f, cellSize.y);
                    this.propertyTileDepth.floatValue = Mathf.Max(0.0001f, cellSize.z);
                }

                EditorGUI.showMixedValue = false;
            }
            EndMultiPartField();
            if (ControlContent.TrailingTipsVisible) {
                ExtraEditorGUI.TrailingTip(TileLang.Text("Span of an individual tile."));
            }

            GUILayout.Space(10);

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Tiles Facing"),
                TileLang.Text("Direction that tiles will face when painted. 'Sideways' is good for platform and 2D games. 'Upwards' is good for top-down.")
            )) {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(this.propertyTilesFacing, content);
                ExtraEditorGUI.TrailingTip(content);

                if (EditorGUI.EndChangeCheck() && this.propertyAutoAdjustDirection.boolValue) {
                    switch ((TileFacing)this.propertyTilesFacing.intValue) {
                        case TileFacing.Sideways:
                            this.propertyDirection.intValue = (int)WorldDirection.Forward;
                            break;
                        case TileFacing.Upwards:
                            this.propertyDirection.intValue = (int)WorldDirection.Up;
                            break;
                    }
                }
            }

            GUILayout.Space(3);

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Initial Direction"),
                TileLang.Text("Initial direction of tile system upon creation. If in doubt assume default and rotate afterwards.")
            )) {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.propertyDirection, content);
                ExtraEditorGUI.TrailingTip(content);

                if (EditorGUI.EndChangeCheck()) {
                    this.propertyAutoAdjustDirection.boolValue = false;
                }
            }

            GUILayout.Space(5);
        }

        private void OnSection_Stripping()
        {
            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Stripping Preset"),
                TileLang.Text("Custom level of stripping can be applied to tile system upon build.")
            )) {
                EditorGUILayout.PropertyField(this.propertyStrippingPreset, content);
            }

            // "Stripping Preset Toggles"
            if (!this.propertyStrippingPreset.hasMultipleDifferentValues) {
                var targetPresets = this.targets.Cast<TileSystemPreset>().ToArray();
                int mixedMask = RotorzEditorGUI.GetMixedStrippingOptionsMask(targetPresets);
                StrippingPreset preset = targetPresets[0].StrippingPreset;
                int options = targetPresets[0].StrippingOptions & ~mixedMask;

                EditorGUI.showMixedValue = this.propertyStrippingOptions.hasMultipleDifferentValues;
                int diff = RotorzEditorGUI.StrippingOptions(preset, options, mixedMask);

                if (diff != 0 && preset == StrippingPreset.Custom) {
                    int addBits = diff & ~options;
                    int removeBits = diff & options;

                    Undo.RecordObjects(this.targets, TileLang.ParticularText("Action", "Modify Stripping Options"));
                    foreach (var targetPreset in targetPresets) {
                        targetPreset.StrippingOptions = (targetPreset.StrippingOptions & ~removeBits) | addBits;
                        EditorUtility.SetDirty(targetPreset);
                    }
                }
                EditorGUI.showMixedValue = false;
            }

            GUILayout.Space(5);
        }

        private void OnSection_BuildOptions()
        {
            float initialLabelWidth = EditorGUIUtility.labelWidth;

            GUILayout.Space(3);

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Combine Method")
            )) {
                EditorGUILayout.PropertyField(this.propertyCombineMethod, content);
            }

            if (!this.propertyCombineMethod.hasMultipleDifferentValues) {
                if ((BuildCombineMethod)this.propertyCombineMethod.intValue == BuildCombineMethod.CustomChunkInTiles) {
                    GUILayout.BeginHorizontal();
                    ++EditorGUI.indentLevel;
                    {
                        EditorGUIUtility.labelWidth = 65;

                        EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Height"));
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(this.propertyCombineChunkHeight, GUIContent.none);
                        if (EditorGUI.EndChangeCheck()) {
                            this.propertyCombineChunkHeight.intValue = Mathf.Max(1, this.propertyCombineChunkHeight.intValue);
                        }

                        EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Width"));
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(this.propertyCombineChunkWidth, GUIContent.none);
                        if (EditorGUI.EndChangeCheck()) {
                            this.propertyCombineChunkWidth.intValue = Mathf.Max(1, this.propertyCombineChunkWidth.intValue);
                        }

                        EditorGUIUtility.labelWidth = initialLabelWidth;
                    }
                    --EditorGUI.indentLevel;
                    GUILayout.EndHorizontal();
                }

                if ((BuildCombineMethod)this.propertyCombineMethod.intValue != BuildCombineMethod.None) {
                    ++EditorGUI.indentLevel;
                    {
                        this.propertyCombineIntoSubmeshes.boolValue = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Combine into submeshes"), this.propertyCombineIntoSubmeshes.boolValue);
                        if (ControlContent.TrailingTipsVisible) {
                            ExtraEditorGUI.TrailingTip(TileLang.Text("Determines whether to use submeshes, or an individual mesh for each material."));
                        }
                    }
                    --EditorGUI.indentLevel;

                    RotorzEditorGUI.InfoBox(TileLang.Text("Avoid generation of meshes with vertices in excess of 64k."), MessageType.Warning);
                }
            }

            EditorGUILayout.Space();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Vertex Snap Threshold"),
                TileLang.Text("Increase threshold to snap vertices that are more widely spread.")
            )) {
                EditorGUILayout.PropertyField(this.propertyVertexSnapThreshold, content);
                if (!this.propertyVertexSnapThreshold.hasMultipleDifferentValues) {
                    if (this.propertyVertexSnapThreshold.floatValue == 0f) {
                        EditorGUILayout.HelpBox(TileLang.Text("No snapping occurs when threshold is 0."), MessageType.Warning, true);
                    }
                }
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Static Snapping"),
                TileLang.Text("Applies vertex snapping to static tiles to avoid tiny gaps due to numerical inaccuracies. Vertex snapping is always applied to 'smooth' tiles.")
            )) {
                EditorGUILayout.PropertyField(this.propertyStaticVertexSnapping, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Generate Lightmap UVs")
            )) {
                EditorGUILayout.PropertyField(this.propertyGenerateSecondUVs, content);
                if (this.propertyGenerateSecondUVs.boolValue) {
                    ++EditorGUI.indentLevel;

                    s_ToggleBuildOptions_AdvancedUV2 = EditorGUILayout.Foldout(s_ToggleBuildOptions_AdvancedUV2, TileLang.ParticularText("Section", "Advanced"));
                    if (s_ToggleBuildOptions_AdvancedUV2) {
                        float hardAngle = this.propertySecondUVsHardAngle.floatValue;
                        float packMargin = this.propertySecondUVsPackMargin.floatValue * 1024f;
                        float angleError = this.propertySecondUVsAngleError.floatValue * 100f;
                        float areaError = this.propertySecondUVsAreaError.floatValue * 100f;

                        using (var content2 = ControlContent.WithTrailableTip(
                            TileLang.ParticularText("Property", "Hard Angle"),
                            TileLang.Text("Angle between neighbor triangles that will generate seam.")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertySecondUVsHardAngle.hasMultipleDifferentValues;
                            hardAngle = EditorGUILayout.Slider(content, hardAngle, 0f, 180f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertySecondUVsHardAngle.floatValue = Mathf.Ceil(hardAngle);
                            }
                            ExtraEditorGUI.TrailingTip(content2);
                        }

                        using (var content2 = ControlContent.WithTrailableTip(
                            TileLang.ParticularText("Property", "Pack Margin"),
                            TileLang.Text("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertySecondUVsPackMargin.hasMultipleDifferentValues;
                            packMargin = EditorGUILayout.Slider(content, packMargin, 1f, 64f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertySecondUVsPackMargin.floatValue = Mathf.Ceil(packMargin) / 1024f;
                            }
                            ExtraEditorGUI.TrailingTip(content2);
                        }

                        using (var content2 = ControlContent.WithTrailableTip(
                            TileLang.ParticularText("Property", "Angle Error"),
                            TileLang.Text("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measure deviation of UV triangles area from geometry triangles if they were uniformly scaled.")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertySecondUVsAngleError.hasMultipleDifferentValues;
                            angleError = EditorGUILayout.Slider(content, angleError, 1f, 75f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertySecondUVsAngleError.floatValue = Mathf.Ceil(angleError) / 100f;
                            }
                            ExtraEditorGUI.TrailingTip(content2);
                        }

                        using (var content2 = ControlContent.Basic(
                            TileLang.ParticularText("Property", "Area Error")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertySecondUVsAreaError.hasMultipleDifferentValues;
                            areaError = EditorGUILayout.Slider(content2, areaError, 1f, 75f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertySecondUVsAreaError.floatValue = Mathf.Ceil(areaError) / 100f;
                            }
                        }

                        EditorGUI.showMixedValue = false;
                    }

                    --EditorGUI.indentLevel;
                }
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Pre-generate Procedural"),
                TileLang.Text("Increases size of scene but allows brushes to be stripped from builds.")
            )) {
                EditorGUILayout.PropertyField(this.propertyPregenerateProcedural, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            RotorzEditorGUI.InfoBox(TileLang.Text("Stripping capabilities are reduced when procedural tiles are present but are not pre-generated since they are otherwise generated at runtime."), MessageType.Info);
            GUILayout.Space(5);

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Reduce Box Colliders"),
                TileLang.Text("Reduces count of box colliders by coalescing adjacent colliders.")
            )) {
                EditorGUILayout.PropertyField(this.propertyReduceColliders, content);
            }
        }

        private void OnSection_RuntimeOptions()
        {
            GUILayout.Space(3);

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Erase Empty Chunks"),
                TileLang.Text("Hints that empty chunks should be erased when they become empty at runtime.")
            )) {
                EditorGUILayout.PropertyField(this.propertyHintEraseEmptyChunks, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Apply Basic Stripping"),
                TileLang.Text("Applies a basic degree of stripping at runtime upon awakening.")
            )) {
                EditorGUILayout.PropertyField(this.propertyApplyRuntimeStripping, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Update Procedural at Start"),
                TileLang.Text("Automatically updates procedural meshes at runtime upon awakening.")
            )) {
                EditorGUILayout.PropertyField(this.propertyUpdateProceduralAtStart, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Mark Procedural Dynamic"),
                TileLang.Text("Helps to improve performance when procedural tiles are updated frequently at runtime. Unset if only updated at start of level.")
            )) {
                EditorGUILayout.PropertyField(this.propertyMarkProceduralDynamic, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Add Procedural Normals"),
                TileLang.Text("Adds normals to procedural meshes.")
            )) {
                EditorGUILayout.PropertyField(this.propertyAddProceduralNormals, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Procedural Sorting Layer"),
                TileLang.Text("Sorting layer for procedural tileset meshes.")
            )) {
                RotorzEditorGUI.SortingLayerField(this.propertySortingLayerID, content);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Procedural Order in Layer"),
                TileLang.Text("Order in sorting layer.")
            )) {
                EditorGUILayout.PropertyField(this.propertySortingOrder, content);
                ExtraEditorGUI.TrailingTip(content);
            }
        }

        private static void BeginMultiPartField(string prefixLabel)
        {
            ExtraEditorGUI.MultiPartPrefixLabel(prefixLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(18);
        }

        private static void EndMultiPartField()
        {
            EditorGUILayout.EndHorizontal();
        }

        public void FocusNameField()
        {
            EditorGUI.FocusTextInControl("NameField");
        }
    }
}
