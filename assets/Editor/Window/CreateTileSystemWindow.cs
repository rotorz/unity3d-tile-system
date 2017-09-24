// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Create tile system window.
    /// </summary>
    public sealed class CreateTileSystemWindow : RotorzWindow
    {
        #region Window Management

        /// <summary>
        /// Display create tile system window.
        /// </summary>
        /// <returns>
        /// The window.
        /// </returns>
        public static CreateTileSystemWindow ShowWindow()
        {
            return GetUtilityWindow<CreateTileSystemWindow>();
        }

        #endregion


        #region User Settings

        private static void AutoInitializeUserSettings()
        {
            if (s_HasInitializedUserSettings == true) {
                return;
            }

            var settings = AssetSettingManagement.GetGroup("CreateTileSystemWindow");

            s_SelectedPresetGuid = settings.Fetch<string>("SelectedPresetGuid", "");

            s_HasInitializedUserSettings = true;
        }

        private static bool s_HasInitializedUserSettings = false;

        private static Setting<string> s_SelectedPresetGuid;

        #endregion


        private const float FIELD_LABEL_WIDTH = 140;

        private TileSystemPreset selectedPreset;

        private TileSystemPreset currentPreset;
        private TileSystemPresetInspector currentPresetInspector;
        private string newPresetName = "";

        private Vector2 scrollPosition;
        private bool hasFocusedName;


        /// <summary>
        /// Occurs when a tile system is created using this window.
        /// </summary>
        public event TileSystemDelegate TileSystemCreated;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            AutoInitializeUserSettings();

            this.titleContent = new GUIContent(TileLang.ParticularText("Action", "Create Tile System"));
            this.InitialSize = this.minSize = new Vector2(580, 375);
            this.maxSize = new Vector2(580, Screen.currentResolution.height);

            if (this.currentPreset == null) {
                this.currentPreset = ScriptableObject.CreateInstance<TileSystemPreset>();
                this.currentPreset.hideFlags = HideFlags.DontSave;
            }

            if (this.currentPresetInspector == null) {
                this.currentPresetInspector = (TileSystemPresetInspector)UnityEditor.Editor.CreateEditor(this.currentPreset, typeof(TileSystemPresetInspector));
            }
            this.currentPresetInspector.DisableUndoOnSerializedObject = true;

            // Restore previous preset selection?
            this.SetSelectedPreset(s_SelectedPresetGuid);
        }

        /// <inheritdoc/>
        protected override void DoDestroy()
        {
            if (this.currentPresetInspector != null) {
                DestroyImmediate(this.currentPresetInspector);
                this.currentPresetInspector = null;
            }
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            EditorGUIUtility.labelWidth = FIELD_LABEL_WIDTH;
            EditorGUIUtility.wideMode = true;

            this.VerifySelectedPresetValue();

            if (ExtraEditorGUI.AcceptKeyboardReturn()) {
                this.OnButton_Create();
                return;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);

                Rect presetsPosition = EditorGUILayout.BeginVertical(GUILayout.Width(150));
                if (Event.current.type == EventType.Repaint) {
                    GUI.skin.box.Draw(new Rect(presetsPosition.x - 7, presetsPosition.y - 2, presetsPosition.width + 9, presetsPosition.height + 4), GUIContent.none, false, false, false, false);
                }
                this.OnGUI_Presets();
                EditorGUILayout.EndVertical();

                GUILayout.Space(4);

                GUILayout.BeginVertical();
                {
                    this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5);

                            GUILayout.BeginVertical();
                            this.DrawCurrentPresetEditor();
                            GUILayout.EndVertical();

                            GUILayout.Space(5);
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();

                    ExtraEditorGUI.SeparatorLight(marginTop: -1);

                    GUILayout.BeginHorizontal();
                    {
                        this.OnGUI_Buttons();
                        GUILayout.Space(3);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            if (!this.hasFocusedName) {
                this.hasFocusedName = true;
                this.currentPresetInspector.FocusNameField();
            }
        }

        private void VerifySelectedPresetValue()
        {
            if (this.selectedPreset == null && !ReferenceEquals(this.selectedPreset, null)) {
                this.selectedPreset = null;
                s_SelectedPresetGuid.Value = "";
            }
        }

        private void DrawCurrentPresetEditor()
        {
            GUILayout.Space(10);
            this.currentPresetInspector.OnInspectorGUI();
            GUILayout.Space(5);
        }

        /// <summary>
        /// Set the currently selected preset.
        /// </summary>
        /// <param name="presetGuid">Name of preset.</param>
        private void SetSelectedPreset(string presetGuid)
        {
            this.selectedPreset = null;

            // Figure out a valid preset GUID.
            if (presetGuid != "F:3D" && presetGuid != "F:2D") {
                this.selectedPreset = TileSystemPresetUtility.LoadPresetFromGUID(presetGuid);
                if (this.selectedPreset == null) {
                    presetGuid = TileSystemPresetUtility.DefaultPresetGUID;
                }
            }

            // Persist current preset selection.
            s_SelectedPresetGuid.Value = presetGuid;

            // Preserve user input for tile system name!
            string preserveSystemName = this.currentPreset.SystemName;

            // Update the new tile system configuration from the selected preset.
            switch (presetGuid) {
                case "F:3D":
                    this.currentPreset.SetDefaults3D();
                    this.currentPreset.name = TileLang.ParticularText("Preset Name", "Default: 3D");
                    this.newPresetName = "";
                    break;
                case "F:2D":
                    this.currentPreset.SetDefaults2D();
                    this.currentPreset.name = TileLang.ParticularText("Preset Name", "Default: 2D");
                    this.newPresetName = "";
                    break;
                default:
                    EditorUtility.CopySerialized(this.selectedPreset, this.currentPreset);
                    this.newPresetName = this.currentPreset.name;
                    break;
            }

            // Remove input focus from any control but most specifically "Preset Name".
            GUIUtility.keyboardControl = 0;

            if (this.currentPresetInspector.HasModifiedTileSystemName) {
                this.currentPreset.SystemName = preserveSystemName;
            }
            else {
                this.AutoAddPostfixToName();
            }
        }

        private void OnGUI_Presets()
        {
            GUILayout.Space(10);

            EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Preset"));
            EditorGUI.BeginChangeCheck();
            using (var valueContent = ControlContent.Basic(this.currentPreset.name)) {
                s_SelectedPresetGuid.Value = CustomPopupGUI.Popup(GUIContent.none, s_SelectedPresetGuid.Value, valueContent, this.PopulatePresetMenu);
            }
            if (EditorGUI.EndChangeCheck()) {
                this.SetSelectedPreset(s_SelectedPresetGuid.Value);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PrefixLabel(TileLang.ParticularText("Property", "Preset Name"));
            this.newPresetName = EditorGUILayout.TextField(this.newPresetName);

            if (GUILayout.Button(TileLang.ParticularText("Action", "Save Preset"))) {
                this.OnButton_SavePreset();
                GUIUtility.ExitGUI();
            }

            // Do not allow deletion of factory default preset.
            EditorGUI.BeginDisabledGroup(this.selectedPreset == null);
            if (GUILayout.Button(TileLang.ParticularText("Action", "Delete Preset"))) {
                this.OnButton_DeletePreset();
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            ControlContent.TrailingTipsVisible = GUILayout.Toggle(ControlContent.TrailingTipsVisible, TileLang.ParticularText("Action", "Show Tips"));

            GUILayout.Space(10);
        }

        private void PopulatePresetMenu(ICustomPopupContext<string> context)
        {
            var popup = context.Popup;

            popup.AddOption(TileLang.ParticularText("Preset Name", "Default: 3D"), context, "F:3D");
            popup.AddOption(TileLang.ParticularText("Preset Name", "Default: 2D"), context, "F:2D");

            var presets = TileSystemPresetUtility.GetPresets();
            if (presets.Length == 0) {
                return;
            }

            popup.AddSeparator();

            var presetGroups = presets
                .OrderBy(preset => preset.name)
                .GroupBy(preset => TileSystemPresetUtility.IsUserPreset(preset))
                .ToArray();

            for (int i = 0; i < presetGroups.Length; ++i) {
                if (i != 0) {
                    popup.AddSeparator();
                }

                foreach (var preset in presetGroups[i]) {
                    popup.AddOption(preset.name, context, TileSystemPresetUtility.GetPresetGUID(preset));
                }
            }
        }

        private void OnGUI_Buttons()
        {
            if (GUILayout.Button(TileLang.ParticularText("Action", "Reset"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnButton_Reset();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Create"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.OnButton_Create();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(3);

            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.Close();
                GUIUtility.ExitGUI();
            }
        }

        private void OnButton_SavePreset()
        {
            // Remove focus from input control.
            GUIUtility.keyboardControl = 0;

            this.newPresetName = this.newPresetName.Trim();

            // Name must be specified for preset!
            if (string.IsNullOrEmpty(this.newPresetName)) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "One or more inputs were invalid"),
                    TileLang.ParticularText("Error", "Name was not specified"),
                    TileLang.ParticularText("Action", "Close")
                );
            }
            else if (!TileSystemPresetUtility.IsValidPresetName(this.newPresetName)) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "Invalid name for the asset"),
                    TileLang.ParticularText("Error", "Can only use alphanumeric characters (A-Z a-z 0-9), hyphens (-), underscores (_) and spaces.\n\nName must begin with an alphanumeric character."),
                    TileLang.ParticularText("Action", "Close")
                );
            }
            else {
                if (this.selectedPreset != null && this.newPresetName == this.selectedPreset.name) {
                    TileSystemPresetUtility.OverwritePreset(this.currentPreset, this.selectedPreset);
                }
                else {
                    var newPreset = TileSystemPresetUtility.CreatePreset(this.currentPreset, this.newPresetName);
                    this.SetSelectedPreset(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newPreset)));
                }
            }
        }

        private void OnButton_DeletePreset()
        {
            if (this.selectedPreset == null) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Error"),
                    TileLang.ParticularText("Error", "Cannot delete a default preset."),
                    TileLang.ParticularText("Action", "Close")
                );
            }
            else if (EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Delete Preset"),
                string.Format(
                    /* 0: name of the preset */
                    TileLang.ParticularText("Error", "Do you want to delete the preset '{0}'?"),
                    this.currentPreset.name
                ),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                // Remove the selected preset asset.
                TileSystemPresetUtility.DeletePreset(this.selectedPreset);
                // Select the default preset.
                this.SetSelectedPreset("");
            }
        }

        private void OnButton_Reset()
        {
            this.SetSelectedPreset(s_SelectedPresetGuid.Value);
            this.AutoAddPostfixToName();
        }

        private void OnButton_Create()
        {
            this.currentPreset.SystemName = this.currentPreset.SystemName.Trim();

            // Validate inputs first.
            if (string.IsNullOrEmpty(this.currentPreset.SystemName)) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "Name was not specified"),
                    TileLang.Text("Please specify name for tile system."),
                    TileLang.ParticularText("Action", "Close")
                );
                GUIUtility.ExitGUI();
            }

            // Do not allow user to create useless system.
            if (this.currentPreset.Rows < 1 || this.currentPreset.Columns < 1) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Error"),
                    TileLang.Text("A tile system must contain at least 1 cell."),
                    TileLang.ParticularText("Action", "Close")
                );
                GUIUtility.ExitGUI();
                return;
            }

            // Create tile system using preset and select it ready for immediate usage.
            var tileSystemGO = TileSystemPresetUtility.CreateTileSystemFromPreset(this.currentPreset);
            Selection.activeObject = tileSystemGO;

            // Register undo event.
            Undo.IncrementCurrentGroup();
            Undo.RegisterCreatedObjectUndo(tileSystemGO, TileLang.ParticularText("Action", "Create Tile System"));

            if (this.TileSystemCreated != null) {
                this.TileSystemCreated(tileSystemGO.GetComponent<TileSystem>());
            }

            this.Close();
        }

        private void AutoAddPostfixToName()
        {
            var tileSystems = ToolUtility.GetAllTileSystemsInScene();

            string baseName = Regex.Replace(this.currentPreset.SystemName, "#\\d+$", "");
            string nextName = baseName;
            int increment = 1;

            while (true) {
                next:
                foreach (var tileSystem in tileSystems)
                    if (tileSystem.name == nextName) {
                        nextName = string.Format("{0} #{1}", baseName, ++increment);
                        goto next;
                    }
                break;
            }

            this.currentPreset.SystemName = nextName;
        }
    }
}
