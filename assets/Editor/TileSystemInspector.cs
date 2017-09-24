// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using Rotorz.Tile.Internal;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Custom inspector for tile system.
    /// </summary>
    /// <remarks>
    /// <para>This class is automatically instantiated by <see cref="TileSystemEditor"/> when
    /// inspector GUI is first drawn.</para>
    /// </remarks>
    /// <seealso cref="TileSystemEditor"/>
    internal sealed class TileSystemInspector
    {
        #region Editor Preferences

        static TileSystemInspector()
        {
            var settings = AssetSettingManagement.GetGroup("Inspector.TileSystem");

            s_setting_ToggleModifyGrid = settings.Fetch<bool>("ExpandModifyGrid", true);
            s_setting_ToggleStripping = settings.Fetch<bool>("ExpandStripping", false);
            s_setting_ToggleBuildOptions = settings.Fetch<bool>("ExpandBuildOptions", false);
            s_setting_ToggleRuntimeOptions = settings.Fetch<bool>("ExpandRuntimeOptions", false);
        }


        private static readonly Setting<bool> s_setting_ToggleModifyGrid;
        private static readonly Setting<bool> s_setting_ToggleStripping;
        private static readonly Setting<bool> s_setting_ToggleBuildOptions;
        private static readonly Setting<bool> s_setting_ToggleRuntimeOptions;

        private static bool s_ToggleBuildOptions_AdvancedUV2;

        #endregion


        /// <summary>
        /// The parent <see cref="TileSystemEditor"/> instances which controls the behaviour
        /// of this inspector instance.
        /// </summary>
        private readonly TileSystemEditor parent;

        /// <summary>
        /// Gets the serialized object.
        /// </summary>
        private SerializedObject serializedObject {
            get { return this.parent.serializedObject; }
        }
        /// <summary>
        /// Gets array of objects that are targeted by this inspector.
        /// </summary>
        private Object[] targets {
            get { return this.parent.targets; }
        }
        /// <summary>
        /// Gets the active tile system that is targeted by this inspector.
        /// </summary>
        private TileSystem target {
            get { return this.parent.target as TileSystem; }
        }


        /// <summary>
        /// Initialize new <see cref="TileSystemInspector"/> instance.
        /// </summary>
        /// <param name="parent">The parent editor.</param>
        public TileSystemInspector(TileSystemEditor parent)
        {
            this.parent = parent;

            this.InitModifyGridSection();
            this.InitStrippingSection();
            this.InitBuildOptionsSection();
            this.InitRuntimeOptionsSection();
        }


        /// <summary>
        /// Handles GUI events for inspector.
        /// </summary>
        public void OnGUI()
        {
            float initialLabelWidth = EditorGUIUtility.labelWidth;
            RotorzEditorGUI.UseExtendedLabelWidthForLocalization();

            bool formerAddNormals = this.target.addProceduralNormals;

            this.serializedObject.Update();

            GUILayout.Space(6);
            this.DrawToolbar();
            GUILayout.Space(6);

            if (!this.target.IsEditable) {
                EditorGUILayout.HelpBox(TileLang.Text("Tile system has been built and can no longer be edited."), MessageType.Info, true);
                return;
            }

            // Display message if any of the target tile systems are locked.
            foreach (TileSystem tileSystem in this.targets) {
                if (tileSystem.Locked) {
                    string message = this.targets.Length == 1
                        ? TileLang.Text("Tile system is locked. Select 'Toggle Lock' from context menu to unlock inspector.")
                        : TileLang.Text("One or more selected tile systems are locked. Unlock tile systems to unlock inspector.");
                    EditorGUILayout.HelpBox(message, MessageType.Info, true);
                    return;
                }
            }

            s_setting_ToggleModifyGrid.Value = RotorzEditorGUI.FoldoutSection(s_setting_ToggleModifyGrid,
                label: TileLang.ParticularText("Section", "Modify Grid"),
                callback: this.DrawModifyGridSection,
                paddedStyle: RotorzEditorStyles.Instance.InspectorSectionPadded
            );

            s_setting_ToggleStripping.Value = RotorzEditorGUI.FoldoutSection(s_setting_ToggleStripping,
                label: TileLang.ParticularText("Section", "Stripping"),
                callback: this.DrawStrippingSection,
                paddedStyle: RotorzEditorStyles.Instance.InspectorSectionPadded
            );

            s_setting_ToggleBuildOptions.Value = RotorzEditorGUI.FoldoutSection(s_setting_ToggleBuildOptions,
                label: TileLang.ParticularText("Section", "Build Options"),
                callback: this.DrawBuildOptionsSection,
                paddedStyle: RotorzEditorStyles.Instance.InspectorSectionPadded
            );

            s_setting_ToggleRuntimeOptions.Value = RotorzEditorGUI.FoldoutSection(s_setting_ToggleRuntimeOptions,
                label: TileLang.ParticularText("Section", "Runtime Options"),
                callback: this.DrawRuntimeOptionsSection,
                paddedStyle: RotorzEditorStyles.Instance.InspectorSectionPadded
            );

            // Ensure that changes are saved.
            if (GUI.changed) {
                EditorUtility.SetDirty(this.target);
                this.serializedObject.ApplyModifiedProperties();

                if (formerAddNormals != this.target.addProceduralNormals) {
                    this.target.UpdateProceduralTiles(true);
                }
            }

            EditorGUIUtility.labelWidth = initialLabelWidth;
        }


        #region Section: Modify Grid

        private SerializedProperty propertyCellSize;
        private SerializedProperty propertyTilesFacing;
        private SerializedProperty propertyHintForceRefresh;

        private void InitModifyGridSection()
        {
            this.propertyCellSize = this.serializedObject.FindProperty("cellSize");
            this.propertyTilesFacing = this.serializedObject.FindProperty("tilesFacing");
            this.propertyHintForceRefresh = this.serializedObject.FindProperty("hintForceRefresh");

            this.RefreshModifyGridParamsFromTileSystem();
        }

        private int inputNewRows;
        private int inputNewColumns;
        private int inputRowOffset;
        private int inputColumnOffset;
        private int inputNewChunkWidth;
        private int inputNewChunkHeight;
        private bool inputMaintainTilePositionsInWorldResize;
        private bool inputMaintainTilePositionsInWorldOffset;

        private static GUIStyle s_ModifyGridGroupStyle;

        private void DrawModifyGridSection()
        {
            if (this.targets.Length > 1) {
                EditorGUILayout.HelpBox(TileLang.Text("Cannot modify structure of multiple tile systems at the same time."), MessageType.Info);
                return;
            }

            var tileSystem = this.target as TileSystem;
            if (PrefabUtility.GetPrefabType(tileSystem) == PrefabType.Prefab) {
                EditorGUILayout.HelpBox(TileLang.Text("Prefab must be instantiated in order to modify tile system structure."), MessageType.Info);
                return;
            }

            float restoreLabelWidth = EditorGUIUtility.labelWidth;

            bool hasGridSizeChanged;
            bool hasOffsetChanged;

            if (s_ModifyGridGroupStyle == null) {
                s_ModifyGridGroupStyle = new GUIStyle();
                s_ModifyGridGroupStyle.margin.right = 55;
            }

            Rect modifyGroupRect = EditorGUILayout.BeginVertical(s_ModifyGridGroupStyle);
            {
                ExtraEditorGUI.MultiPartPrefixLabel(TileLang.ParticularText("Property", "Grid Size (in tiles)"));
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(18);
                    EditorGUIUtility.labelWidth = 65;

                    EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Rows"));
                    this.inputNewRows = Mathf.Max(1, EditorGUILayout.IntField(this.inputNewRows));
                    EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Columns"));
                    this.inputNewColumns = Mathf.Max(1, EditorGUILayout.IntField(this.inputNewColumns));

                    EditorGUIUtility.labelWidth = restoreLabelWidth;
                }
                GUILayout.EndHorizontal();

                ExtraEditorGUI.MultiPartPrefixLabel(TileLang.ParticularText("Property", "Offset Amount (in tiles)"));
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(18);
                    EditorGUIUtility.labelWidth = 65;

                    EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Rows"));
                    this.inputRowOffset = EditorGUILayout.IntField(this.inputRowOffset);
                    EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Columns"));
                    this.inputColumnOffset = EditorGUILayout.IntField(this.inputColumnOffset);

                    EditorGUIUtility.labelWidth = restoreLabelWidth;
                }
                GUILayout.EndHorizontal();

                hasGridSizeChanged = (this.inputNewRows != tileSystem.RowCount || this.inputNewColumns != tileSystem.ColumnCount);
                hasOffsetChanged = (this.inputRowOffset != 0 || this.inputColumnOffset != 0);

                if (hasGridSizeChanged) {
                    this.DrawMaintainTilePositionsInWorld(ref this.inputMaintainTilePositionsInWorldResize);
                }
                else if (hasOffsetChanged) {
                    this.DrawMaintainTilePositionsInWorld(ref this.inputMaintainTilePositionsInWorldOffset);
                }

                EditorGUIUtility.labelWidth = restoreLabelWidth;

                ExtraEditorGUI.MultiPartPrefixLabel(TileLang.ParticularText("Property", "Chunk Size (in tiles)"));
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(18);
                    EditorGUIUtility.labelWidth = 65;

                    EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Height"));
                    this.inputNewChunkHeight = Mathf.Max(1, EditorGUILayout.IntField(this.inputNewChunkHeight));
                    EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Width"));
                    this.inputNewChunkWidth = Mathf.Max(1, EditorGUILayout.IntField(this.inputNewChunkWidth));

                    EditorGUIUtility.labelWidth = restoreLabelWidth;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            Rect buttonRect = new Rect(modifyGroupRect.xMax + 5, modifyGroupRect.y + 3, 45, 35);

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.Trim,
                TileLang.ParticularText("Action", "Trim")
            )) {
                if (GUI.Button(buttonRect, content)) {
                    this.OnTrimTileSystem();
                    GUIUtility.ExitGUI();
                }
            }

            buttonRect.y = buttonRect.yMax + 3;

            EditorGUI.BeginDisabledGroup(!hasGridSizeChanged);
            {
                using (var content = ControlContent.Basic(
                    RotorzEditorStyles.Skin.CentralizeUsed,
                    TileLang.ParticularText("Action", "Centralize Tile Bounds")
                )) {
                    if (GUI.Button(buttonRect, content)) {
                        this.OnCentralizeUsedTileSystem();
                        GUIUtility.ExitGUI();
                    }
                }

                buttonRect.y = buttonRect.yMax + 3;

                using (var content = ControlContent.Basic(
                    RotorzEditorStyles.Skin.Centralize,
                    TileLang.ParticularText("Action", "Centralize")
                )) {
                    if (GUI.Button(buttonRect, content)) {
                        this.OnCentralizeTileSystem();
                        GUIUtility.ExitGUI();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();


            bool hasChunkSizeChanged = (this.inputNewChunkWidth != tileSystem.ChunkWidth || this.inputNewChunkHeight != tileSystem.ChunkHeight);

            // Display "Rebuild" button?
            if (hasGridSizeChanged || hasOffsetChanged || hasChunkSizeChanged) {
                GUILayout.Space(6);
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.HelpBox(TileLang.Text("Tile system must be reconstructed, some tiles may be force refreshed."), MessageType.Warning);

                    if (GUILayout.Button(TileLang.ParticularText("Action", "Rebuild"), GUILayout.Width(75), GUILayout.Height(40))) {
                        GUIUtility.keyboardControl = 0;
                        this.OnResizeTileSystem();
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), GUILayout.Width(75), GUILayout.Height(40))) {
                        GUIUtility.keyboardControl = 0;
                        this.RefreshModifyGridParamsFromTileSystem();
                        GUIUtility.ExitGUI();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Cell Size"),
                TileLang.Text("Span of an individual tile.")
            )) {
                Vector3 newCellSize = Vector3.Max(new Vector3(0.0001f, 0.0001f, 0.0001f), EditorGUILayout.Vector3Field(content, tileSystem.CellSize));
                if (tileSystem.CellSize != newCellSize) {
                    this.propertyCellSize.vector3Value = newCellSize;
                    this.propertyHintForceRefresh.boolValue = true;
                }
            }

            GUILayout.Space(5);

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Tiles Facing"),
                TileLang.Text("Direction that tiles will face when painted. 'Sideways' is good for platform and 2D games. 'Upwards' is good for top-down.")
            )) {
                TileFacing newTilesFacing = (TileFacing)EditorGUILayout.EnumPopup(content, tileSystem.TilesFacing);
                if (tileSystem.TilesFacing != newTilesFacing) {
                    this.propertyTilesFacing.intValue = (int)newTilesFacing;
                    this.propertyHintForceRefresh.boolValue = true;
                }
            }

            GUILayout.Space(5);

            // Display suitable warning message when force refresh is required.
            if (this.propertyHintForceRefresh.boolValue) {
                if (!RotorzEditorGUI.InfoBoxClosable(TileLang.Text("Changes may not take effect until tile system is force refreshed without preserving manual offsets, or cleared."), MessageType.Warning)) {
                    this.propertyHintForceRefresh.boolValue = false;
                    this.serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }
            }
            else {
                ExtraEditorGUI.SeparatorLight(marginTop: 0, marginBottom: 0, thickness: 1);
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                // Display extra padding to right of buttons to avoid accidental click when
                // clicking close button of warning message.
                GUILayoutOption columnWidth = GUILayout.Width((EditorGUIUtility.currentViewWidth - 30) / 3 - GUI.skin.button.margin.horizontal);

                GUILayout.BeginVertical(columnWidth);
                if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Refresh")))) {
                    TileSystemCommands.Command_Refresh(this.target);
                    GUIUtility.ExitGUI();
                }
                GUILayout.Space(2);
                if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Refresh Plops")))) {
                    TileSystemCommands.Command_RefreshPlops(this.target);
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(columnWidth);
                if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Repair")))) {
                    TileSystemCommands.Command_Repair(this.target);
                    GUIUtility.ExitGUI();
                }
                GUILayout.Space(2);
                if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Clear Plops")))) {
                    TileSystemCommands.Command_ClearPlops(this.target);
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(columnWidth);
                if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Clear")))) {
                    TileSystemCommands.Command_Clear(this.target);
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        private void DrawMaintainTilePositionsInWorld(ref bool flag)
        {
            ++EditorGUI.indentLevel;

            // "Maintain tile positions in world space"
            using (var content = ControlContent.Basic(TileLang.ParticularText("Property", "Maintain tile positions in world space"))) {
                flag = EditorGUILayout.ToggleLeft(content, flag);
            }

            --EditorGUI.indentLevel;
        }

        private void RefreshModifyGridParamsFromTileSystem()
        {
            var tileSystem = this.target;

            this.inputNewRows = tileSystem.RowCount;
            this.inputNewColumns = tileSystem.ColumnCount;
            this.inputRowOffset = this.inputColumnOffset = 0;
            this.inputMaintainTilePositionsInWorldResize = true;
            this.inputMaintainTilePositionsInWorldOffset = false;
            this.inputNewChunkWidth = tileSystem.ChunkWidth;
            this.inputNewChunkHeight = tileSystem.ChunkHeight;
        }

        private void OnResizeTileSystem()
        {
            var tileSystem = this.target as TileSystem;

            bool eraseOutOfBounds = false;

            // Display suitable warning message to user if resized tile
            // system will cause out-of-bound tiles to be erased.
            if (TileSystemUtility.WillHaveOutOfBoundTiles(tileSystem, this.inputNewRows, this.inputNewColumns, this.inputRowOffset, this.inputColumnOffset)) {
                if (!EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Action", "Rebuild Tile System"),
                    TileLang.Text("Upon modifying tile system some tiles will become out-of-bounds and will be erased.\n\nWould you like to proceed?"),
                    TileLang.ParticularText("Action", "Yes"),
                    TileLang.ParticularText("Action", "No")
                )) {
                    return;
                }

                eraseOutOfBounds = true;
            }

            Undo.RegisterFullObjectHierarchyUndo(tileSystem.gameObject, TileLang.ParticularText("Action", "Rebuild Tile System"));

            bool maintainFlag = (this.inputNewRows != tileSystem.RowCount || this.inputNewColumns != tileSystem.ColumnCount)
                ? this.inputMaintainTilePositionsInWorldResize
                : this.inputMaintainTilePositionsInWorldOffset;

            var resizer = new TileSystemResizer();
            resizer.Resize(tileSystem, this.inputNewRows, this.inputNewColumns, this.inputRowOffset, this.inputColumnOffset, this.inputNewChunkWidth, this.inputNewChunkHeight, maintainFlag, eraseOutOfBounds);

            // Refresh "Modify Grid" parameters from new state of tile system.
            this.RefreshModifyGridParamsFromTileSystem();

            SceneView.RepaintAll();
        }

        private void OnTrimTileSystem()
        {
            TileIndex min, max;

            GUIUtility.keyboardControl = 0;

            this.inputMaintainTilePositionsInWorldResize = true;

            // Bail if invalid range was encountered.
            if (!TileSystemUtility.FindTileBounds(this.target, out min, out max)) {
                this.inputNewRows = this.target.RowCount;
                this.inputNewColumns = this.target.ColumnCount;
                this.inputRowOffset = 0;
                this.inputColumnOffset = 0;
                return;
            }

            ++max.row;
            ++max.column;

            this.inputRowOffset = -min.row;
            this.inputColumnOffset = -min.column;
            this.inputNewRows = max.row - min.row;
            this.inputNewColumns = max.column - min.column;
        }

        private void OnCentralizeUsedTileSystem()
        {
            TileIndex min, max;

            GUIUtility.keyboardControl = 0;

            this.inputMaintainTilePositionsInWorldResize = true;

            // Bail if invalid range was encountered.
            if (!TileSystemUtility.FindTileBounds(this.target, out min, out max)) {
                this.inputRowOffset = 0;
                this.inputColumnOffset = 0;
                return;
            }

            ++max.row;
            ++max.column;

            int boundRows = max.row - min.row;
            int boundColumns = max.column - min.column;

            this.inputRowOffset = -(min.row - (this.inputNewRows - boundRows) / 2);
            this.inputColumnOffset = -(min.column - (this.inputNewColumns - boundColumns) / 2);
        }

        private void OnCentralizeTileSystem()
        {
            GUIUtility.keyboardControl = 0;

            this.inputMaintainTilePositionsInWorldResize = true;

            this.inputRowOffset = -(0 - (this.inputNewRows - this.target.RowCount) / 2);
            this.inputColumnOffset = -(0 - (this.inputNewColumns - this.target.ColumnCount) / 2);
        }

        #endregion


        #region Section: Stripping Options

        private SerializedProperty propertyStrippingPreset;
        private SerializedProperty propertyStrippingOptionMask;


        private void InitStrippingSection()
        {
            this.propertyStrippingPreset = this.serializedObject.FindProperty("strippingPreset");
            this.propertyStrippingOptionMask = this.serializedObject.FindProperty("strippingOptionMask");
        }

        private void DrawStrippingSection()
        {
            // "Stripping Preset"
            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Stripping Preset"),
                TileLang.Text("Custom level of stripping can be applied to tile system upon build.")
            )) {
                EditorGUILayout.PropertyField(this.propertyStrippingPreset, content);
            }

            // "Stripping Preset Toggles"
            if (!this.propertyStrippingPreset.hasMultipleDifferentValues) {
                var targetSystems = this.targets.Cast<TileSystem>().ToArray();
                int mixedMask = RotorzEditorGUI.GetMixedStrippingOptionsMask(targetSystems);
                StrippingPreset preset = targetSystems[0].StrippingPreset;
                int options = targetSystems[0].StrippingOptions & ~mixedMask;

                EditorGUI.showMixedValue = this.propertyStrippingOptionMask.hasMultipleDifferentValues;
                int diff = RotorzEditorGUI.StrippingOptions(preset, options, mixedMask);

                if (diff != 0 && preset == StrippingPreset.Custom) {
                    int addBits = diff & ~options;
                    int removeBits = diff & options;

                    Undo.RecordObjects(this.targets, TileLang.ParticularText("Action", "Modify Stripping Options"));
                    foreach (var tileSystem in targetSystems) {
                        tileSystem.StrippingOptions = (tileSystem.StrippingOptions & ~removeBits) | addBits;
                        EditorUtility.SetDirty(tileSystem);
                    }
                }
                EditorGUI.showMixedValue = false;
            }

            GUILayout.Space(5);
        }

        #endregion


        #region Section: Build Options

        private SerializedProperty propertyCombineMethod;
        private SerializedProperty propertyCombineChunkWidth;
        private SerializedProperty propertyCombineChunkHeight;
        private SerializedProperty propertyCombineIntoSubmeshes;
        private SerializedProperty propertyVertexSnapThreshold;
        private SerializedProperty propertyStaticSnapping;

        private SerializedProperty propertyGenerateSecondUVs;
        private SerializedProperty propertyGenerateSecondUVsHardAngle;
        private SerializedProperty propertyGenerateSecondUVsPackMargin;
        private SerializedProperty propertyGenerateSecondUVsAngleError;
        private SerializedProperty propertyGenerateSecondUVsAreaError;

        private SerializedProperty propertyPregenerateProcedural;

        private SerializedProperty propertyReduceColliders;


        private void InitBuildOptionsSection()
        {
            this.propertyCombineMethod = this.serializedObject.FindProperty("combineMethod");
            this.propertyCombineChunkWidth = this.serializedObject.FindProperty("combineChunkWidth");
            this.propertyCombineChunkHeight = this.serializedObject.FindProperty("combineChunkHeight");
            this.propertyCombineIntoSubmeshes = this.serializedObject.FindProperty("combineIntoSubmeshes");
            this.propertyVertexSnapThreshold = this.serializedObject.FindProperty("vertexSnapThreshold");
            this.propertyStaticSnapping = this.serializedObject.FindProperty("staticVertexSnapping");

            this.propertyGenerateSecondUVs = this.serializedObject.FindProperty("generateSecondUVs");
            this.propertyGenerateSecondUVsHardAngle = this.serializedObject.FindProperty("generateSecondUVsHardAngle");
            this.propertyGenerateSecondUVsPackMargin = this.serializedObject.FindProperty("generateSecondUVsPackMargin");
            this.propertyGenerateSecondUVsAngleError = this.serializedObject.FindProperty("generateSecondUVsAngleError");
            this.propertyGenerateSecondUVsAreaError = this.serializedObject.FindProperty("generateSecondUVsAreaError");

            this.propertyPregenerateProcedural = this.serializedObject.FindProperty("pregenerateProcedural");

            this.propertyReduceColliders = this.serializedObject.FindProperty("reduceColliders");

        }

        private void DrawBuildOptionsSection()
        {
            var tileSystem = this.target as TileSystem;

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Combine Method")
            )) {
                EditorGUILayout.PropertyField(this.propertyCombineMethod, content);

                if (!this.propertyCombineMethod.hasMultipleDifferentValues) {
                    if (tileSystem.combineMethod == BuildCombineMethod.CustomChunkInTiles) {
                        GUILayout.BeginHorizontal();
                        ++EditorGUI.indentLevel;
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

                        EditorGUIUtility.labelWidth = 0;
                        --EditorGUI.indentLevel;
                        GUILayout.EndHorizontal();
                    }

                    if (tileSystem.combineMethod != BuildCombineMethod.None) {
                        ++EditorGUI.indentLevel;
                        {
                            using (var content2 = ControlContent.WithTrailableTip(
                                TileLang.ParticularText("Property", "Combine into submeshes"),
                                TileLang.Text("Determines whether to use submeshes, or an individual mesh for each material.")
                            )) {
                                ExtraEditorGUI.ToggleLeft(this.propertyCombineIntoSubmeshes, content2);
                                ExtraEditorGUI.TrailingTip(content2);
                            }
                        }
                        --EditorGUI.indentLevel;

                        RotorzEditorGUI.InfoBox(TileLang.Text("Avoid generation of meshes with vertices in excess of 64k."), MessageType.Warning);
                    }
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
                EditorGUILayout.PropertyField(this.propertyStaticSnapping, content);
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
                        float hardAngle = this.propertyGenerateSecondUVsHardAngle.floatValue;
                        float packMargin = this.propertyGenerateSecondUVsPackMargin.floatValue * 1024f;
                        float angleError = this.propertyGenerateSecondUVsAngleError.floatValue * 100f;
                        float areaError = this.propertyGenerateSecondUVsAreaError.floatValue * 100f;

                        using (var content2 = ControlContent.WithTrailableTip(
                            TileLang.ParticularText("Property", "Hard Angle"),
                            TileLang.Text("Angle between neighbor triangles that will generate seam.")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertyGenerateSecondUVsHardAngle.hasMultipleDifferentValues;
                            hardAngle = EditorGUILayout.Slider(content2, hardAngle, 0f, 180f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertyGenerateSecondUVsHardAngle.floatValue = Mathf.Ceil(hardAngle);
                            }
                            ExtraEditorGUI.TrailingTip(content2);
                        }

                        using (var content2 = ControlContent.WithTrailableTip(
                            TileLang.ParticularText("Property", "Pack Margin"),
                            TileLang.Text("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertyGenerateSecondUVsPackMargin.hasMultipleDifferentValues;
                            packMargin = EditorGUILayout.Slider(content2, packMargin, 1f, 64f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertyGenerateSecondUVsPackMargin.floatValue = Mathf.Ceil(packMargin) / 1024f;
                            }
                            ExtraEditorGUI.TrailingTip(content2);
                        }

                        using (var content2 = ControlContent.WithTrailableTip(
                            TileLang.ParticularText("Property", "Angle Error"),
                            TileLang.Text("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measure deviation of UV triangles area from geometry triangles if they were uniformly scaled.")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertyGenerateSecondUVsAngleError.hasMultipleDifferentValues;
                            angleError = EditorGUILayout.Slider(content2, angleError, 1f, 75f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertyGenerateSecondUVsAngleError.floatValue = Mathf.Ceil(angleError) / 100f;
                            }
                            ExtraEditorGUI.TrailingTip(content2);
                        }

                        using (var content2 = ControlContent.Basic(
                            TileLang.ParticularText("Property", "Area Error")
                        )) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = this.propertyGenerateSecondUVsAreaError.hasMultipleDifferentValues;
                            areaError = EditorGUILayout.Slider(content2, areaError, 1f, 75f);
                            if (EditorGUI.EndChangeCheck()) {
                                this.propertyGenerateSecondUVsAreaError.floatValue = Mathf.Ceil(areaError) / 100f;
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

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Reduce Box Colliders"),
                TileLang.Text("Reduces count of box colliders by coalescing adjacent colliders.")
            )) {
                EditorGUILayout.PropertyField(this.propertyReduceColliders, content);
                ExtraEditorGUI.TrailingTip(content);
            }
        }

        #endregion


        #region Section: Runtime Options

        private SerializedProperty propertyApplyRuntimeStripping;
        private SerializedProperty propertyHintEraseEmptyChunks;

        private SerializedProperty propertyUpdateProceduralAtStart;
        private SerializedProperty propertyMarkProceduralDynamic;
        private SerializedProperty propertyAddProceduralNormals;

        private SerializedProperty propertySortingLayerID;
        private SerializedProperty propertySortingOrder;


        private void InitRuntimeOptionsSection()
        {
            this.propertyApplyRuntimeStripping = this.serializedObject.FindProperty("applyRuntimeStripping");
            this.propertyHintEraseEmptyChunks = this.serializedObject.FindProperty("hintEraseEmptyChunks");

            this.propertyUpdateProceduralAtStart = this.serializedObject.FindProperty("updateProceduralAtStart");
            this.propertyMarkProceduralDynamic = this.serializedObject.FindProperty("markProceduralDynamic");
            this.propertyAddProceduralNormals = this.serializedObject.FindProperty("addProceduralNormals");

            this.propertySortingLayerID = this.serializedObject.FindProperty("sortingLayerID");
            this.propertySortingOrder = this.serializedObject.FindProperty("sortingOrder");
        }

        private void DrawRuntimeOptionsSection()
        {
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

            EditorGUI.BeginChangeCheck();

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

            if (EditorGUI.EndChangeCheck()) {
                int sortingLayerID = this.propertySortingLayerID.intValue;
                int sortingOrder = this.propertySortingOrder.intValue;

                // Update existing procedurally generated tileset meshes immediately.
                foreach (var target in this.targets) {
                    ((TileSystem)target).ApplySortingPropertiesToExistingProceduralMeshes(sortingLayerID, sortingOrder);
                }
            }
        }

        #endregion


        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginDisabledGroup(PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab || this.targets.Length != 1);
            if (this.target.IsEditable && GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Build Prefab")), RotorzEditorStyles.Instance.ToolbarButtonPaddedExtra)) {
                TileSystemCommands.Command_BuildPrefab(this.target);
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.GridToggle,
                TileLang.ParticularText("Action", "Toggle Grid Display")
            )) {
                RtsPreferences.ShowGrid.Value = GUILayout.Toggle(RtsPreferences.ShowGrid, content, RotorzEditorStyles.Instance.ToolbarButtonPadded);
            }

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.ChunkToggle,
                TileLang.ParticularText("Action", "Toggle Chunk Display")
            )) {
                RtsPreferences.ShowChunks.Value = GUILayout.Toggle(RtsPreferences.ShowChunks, content, RotorzEditorStyles.Instance.ToolbarButtonPadded);
            }

            EditorGUILayout.Space();

            this.DrawHelpButton();

            GUILayout.EndHorizontal();
        }

        private void DrawHelpButton()
        {
            using (var content = ControlContent.Basic(RotorzEditorStyles.Skin.ContextHelp)) {
                Rect position = GUILayoutUtility.GetRect(content, RotorzEditorStyles.Instance.ToolbarButtonPadded);
                if (EditorInternalUtility.DropdownMenu(position, content, RotorzEditorStyles.Instance.ToolbarButtonPadded)) {
                    var helpMenu = new EditorMenu();

                    helpMenu.AddCommand(TileLang.ParticularText("Action", "Show Tips"))
                        .Checked(ControlContent.TrailingTipsVisible)
                        .Action(() => {
                            ControlContent.TrailingTipsVisible = !ControlContent.TrailingTipsVisible;
                        });

                    --position.y;
                    helpMenu.ShowAsDropdown(position);
                }
            }
        }
    }
}
