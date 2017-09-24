// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Collections;
using Rotorz.Games.Localization;
using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Graphical user interface for editor preferences.
    /// </summary>
    internal sealed class EditorPreferencesWindow : RotorzWindow
    {
        #region Window Management

        public static void ShowWindow()
        {
            GetUtilityWindow<EditorPreferencesWindow>();
        }

        #endregion


        private GUIContent[] tabs;
        private Vector2[] scrolling;

        private int selectedTabIndex = 0;

        private GenericListAdaptor<ToolBase> toolsAdaptor;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(string.Format(
                /* 0: name of product */
                TileLang.Text("Preferences - {0}"),
                ProductInfo.Name
            ));
            this.InitialSize = this.minSize = new Vector2(500, 342);
            this.maxSize = new Vector2(500, 10000);

            this.wantsMouseMove = true;

            this.tabs = new GUIContent[] {
                ControlContent.Basic(TileLang.ParticularText("Preferences|TabLabel", "Tools")),
                ControlContent.Basic(TileLang.ParticularText("Preferences|TabLabel", "Painting")),
                ControlContent.Basic(TileLang.ParticularText("Preferences|TabLabel", "Grid")),
                ControlContent.Basic(TileLang.ParticularText("Preferences|TabLabel", "Misc"))
            };

            this.scrolling = new Vector2[this.tabs.Length];

            this.toolsAdaptor = new GenericListAdaptor<ToolBase>(ToolManager.Instance.toolsInUserOrder, this.DrawAvailableToolEntry, 22);

            this.GatherLocaleOptions();
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            {
                this.selectedTabIndex = RotorzEditorGUI.VerticalTabSelector(this.selectedTabIndex, this.tabs, GUILayout.Width(108), GUILayout.Height(Screen.height - 45));
                this.scrolling[this.selectedTabIndex] = GUILayout.BeginScrollView(this.scrolling[this.selectedTabIndex]);
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginVertical();
                    this.DrawSelectedTab();
                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            this.DrawButtonStrip();

            GUILayout.EndVertical();

            RotorzEditorGUI.DrawHoverTip(this);

            // Refresh all scene views?
            if (GUI.changed) {
                SceneView.RepaintAll();
            }
        }

        private void DrawSelectedTab()
        {
            RotorzEditorGUI.UseExtendedLabelWidthForLocalization();

            GUILayout.Space(10);

            RotorzEditorGUI.Title(this.tabs[this.selectedTabIndex]);

            GUILayout.Space(10);

            switch (this.selectedTabIndex) {
                case 0:
                    this.DrawToolsTab();
                    break;
                case 1:
                    this.DrawPaintingTab();
                    break;
                case 2:
                    this.DrawGridTab();
                    break;
                case 3:
                    this.DrawMiscTab();
                    break;
            }
        }

        private void DrawButtonStrip()
        {
            ExtraEditorGUI.Separator(marginTop: 0, thickness: 1);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Use Defaults"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnUseDefaults();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Close"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.Close();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        private void DrawToolsTab()
        {
            EditorGUI.BeginChangeCheck();
            ReorderableListGUI.Title(TileLang.Text("Tool Palette:"));
            ReorderableListGUI.ListField(this.toolsAdaptor, ReorderableListFlags.HideAddButton | ReorderableListFlags.HideRemoveButtons);
            if (EditorGUI.EndChangeCheck()) {
                ToolManagementSettings.SaveToolOrdering();
                ToolUtility.RepaintToolPalette();
            }

            ExtraEditorGUI.TrailingTip(TileLang.Text("Specify which tools appear in the tools palette."));
            GUILayout.Space(5);

            RtsPreferences.AutoShowToolPalette.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Automatically show tool palette upon activating tool"), RtsPreferences.AutoShowToolPalette);

            GUILayout.Space(5);
        }

        private ToolBase DrawAvailableToolEntry(Rect position, ToolBase tool)
        {
            position.width -= 25;

            Rect eyePosition = position;
            eyePosition.x = position.xMax;
            eyePosition.width = 25;

            string tipText = tool.Visible
                ? TileLang.ParticularText("Action", "Click to hide tool")
                : TileLang.ParticularText("Action", "Click to show tool");
            int eyeControlID = RotorzEditorGUI.GetHoverControlID(eyePosition, tipText);

            switch (Event.current.GetTypeForControl(eyeControlID)) {
                case EventType.Repaint:
                    Color restoreColor = GUI.color;
                    if (!tool.Visible) {
                        GUI.color = new Color(0f, 0f, 0f, 0.55f);
                    }

                    if (tool.IconNormal != null) {
                        Rect iconPosition = position;
                        iconPosition.width = 22;
                        iconPosition.height = 22;

                        GUI.DrawTexture(iconPosition, tool.IconNormal);
                    }

                    position.x += 25;
                    position.width -= 25;
                    position.height -= 2;
                    RotorzEditorStyles.Instance.LabelMiddleLeft.Draw(position, tool.Label, false, false, false, false);

                    if (!tool.Visible) {
                        GUI.color = restoreColor;
                    }

                    eyePosition.x += 3;
                    eyePosition.y = eyePosition.y + (eyePosition.height - 18) / 2;
                    eyePosition.width = 21;
                    eyePosition.height = 18;
                    GUI.DrawTexture(eyePosition, tool.Visible ? RotorzEditorStyles.Skin.EyeOpen : RotorzEditorStyles.Skin.EyeShut);
                    break;

                case EventType.MouseDown:
                    if (Event.current.button == 0 && eyePosition.Contains(Event.current.mousePosition)) {
                        tool.Visible = !tool.Visible;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;
            }

            return tool;
        }

        private void DrawPaintingTab()
        {
            RtsPreferences.EraseEmptyChunksPreference.Value = (EraseEmptyChunksPreference)EditorGUILayout.EnumPopup(TileLang.Text("Erase empty chunks"), RtsPreferences.EraseEmptyChunksPreference);

            ExtraEditorGUI.SeparatorLight();

            RtsPreferences.ToolPreferredNozzleIndicator.Value = (NozzleIndicator)EditorGUILayout.EnumPopup(TileLang.Text("Preferred nozzle indicator"), RtsPreferences.ToolPreferredNozzleIndicator);
            ++EditorGUI.indentLevel;
            {
                RtsPreferences.ToolWireframeColor.Value = EditorGUILayout.ColorField(TileLang.ParticularText("RenderMode", "Wireframe"), RtsPreferences.ToolWireframeColor);
                RtsPreferences.ToolShadedColor.Value = EditorGUILayout.ColorField(TileLang.ParticularText("RenderMode", "Shaded"), RtsPreferences.ToolShadedColor);
            }
            --EditorGUI.indentLevel;

            ExtraEditorGUI.SeparatorLight();

            RtsPreferences.ToolImmediatePreviews.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Display immediate previews"), RtsPreferences.ToolImmediatePreviews);
            EditorGUI.BeginDisabledGroup(!RtsPreferences.ToolImmediatePreviews);
            ++EditorGUI.indentLevel;
            {
                RtsPreferences.ToolImmediatePreviewsTintColor.Value = EditorGUILayout.ColorField(TileLang.Text("Preview Tint"), RtsPreferences.ToolImmediatePreviewsTintColor);

                RtsPreferences.ToolImmediatePreviewsSeeThrough.Value = EditorGUILayout.Toggle(TileLang.Text("See-through previews"), RtsPreferences.ToolImmediatePreviewsSeeThrough);
                ExtraEditorGUI.TrailingTip(TileLang.Text("Hold control when painting to temporarily see-through."));
            }
            --EditorGUI.indentLevel;
            EditorGUI.EndDisabledGroup();
        }

        private void DrawGridTab()
        {
            RtsPreferences.BackgroundGridColor.Value = EditorGUILayout.ColorField(TileLang.ParticularText("Grid", "Background"), RtsPreferences.BackgroundGridColor);
            RtsPreferences.MinorGridColor.Value = EditorGUILayout.ColorField(TileLang.ParticularText("Grid", "Minor Grid"), RtsPreferences.MinorGridColor);
            RtsPreferences.MajorGridColor.Value = EditorGUILayout.ColorField(TileLang.ParticularText("Grid", "Minor Grid"), RtsPreferences.MajorGridColor);
            RtsPreferences.ChunkGridColor.Value = EditorGUILayout.ColorField(TileLang.ParticularText("Grid", "Chunk Boundary"), RtsPreferences.ChunkGridColor);

            EditorGUILayout.Space();

            RtsPreferences.ShowActiveTileSystem.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Highlight active tile system"), RtsPreferences.ShowActiveTileSystem);
            RtsPreferences.ShowGrid.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Display grid lines"), RtsPreferences.ShowGrid);
            RtsPreferences.ShowChunks.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Display chunk boundaries"), RtsPreferences.ShowChunks);

            EditorGUILayout.Space();

            if (HookAutoHideSceneViewGrid.IsFeatureAvailable) {
                HookAutoHideSceneViewGrid.HookEnabled.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Hide scene view grid upon activating tool"), HookAutoHideSceneViewGrid.HookEnabled);
            }
        }

        private void DrawMiscTab()
        {
            this.DrawPreferredLanguageField();

            ExtraEditorGUI.Separator(marginBottom: 10);

            ControlContent.TrailingTipsVisible = EditorGUILayout.ToggleLeft(TileLang.Text("Show detailed tips when available."), ControlContent.TrailingTipsVisible);
            RtsPreferences.DisableCustomCursors.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Disable custom cursors."), RtsPreferences.DisableCustomCursors);
            RtsPreferences.AlwaysCenterUtilityWindows.Value = EditorGUILayout.ToggleLeft(TileLang.Text("Always center utility windows."), RtsPreferences.AlwaysCenterUtilityWindows);
        }

        private void DrawPreferredLanguageField()
        {
            EditorGUILayout.BeginHorizontal();

            // "Preferred Language (BETA)"
            using (var content = ControlContent.Basic(TileLang.Text("Preferred Language (BETA)"))) {
                EditorGUI.BeginChangeCheck();
                this.preferredCultureIndex = EditorGUILayout.Popup(content, this.preferredCultureIndex, this.availableCultureLabels);
                if (EditorGUI.EndChangeCheck()) {
                    var selectedCulture = this.availableCultures[this.preferredCultureIndex];
                    PackageLanguageManager.SetPreferredCulture(selectedCulture);
                }
            }

            GUILayout.Space(32);

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.RefreshIcon,
                TileLang.ParticularText("Action", "Refresh")
            )) {
                Rect refreshButtonPosition = GUILayoutUtility.GetLastRect();
                refreshButtonPosition.x = refreshButtonPosition.xMax - 31 + 3;
                refreshButtonPosition.y = refreshButtonPosition.y - 1;
                refreshButtonPosition.width = 31;
                refreshButtonPosition.height = 21;
                if (RotorzEditorGUI.HoverButton(refreshButtonPosition, content)) {
                    PackageLanguageManager.ReloadAll();
                }
            }

            EditorGUILayout.EndHorizontal();

            string translators = TileLang.Text("__Translators__").Trim();
            if (!string.IsNullOrEmpty(translators) && translators != "-" && translators != "__Translators__") {
                string message = string.Format(
                    /* list of special people names */
                    TileLang.Text("Special Thanks: {0}"),
                    translators
                );
                EditorGUILayout.HelpBox(message, MessageType.None);
            }
        }

        private void OnUseDefaults()
        {
            if (EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Use Default Preferences"),
                TileLang.Text("Would you like to use the default preferences?"),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                RtsPreferences.ResetToDefaultValues();

                // Refresh editor windows.
                ToolUtility.RepaintToolPalette();
                ToolUtility.RepaintBrushPalette();
                // Refresh all scene views.
                SceneView.RepaintAll();
            }
        }


        #region Language Selection

        [NonSerialized]
        private CultureInfo[] availableCultures;
        [NonSerialized]
        private GUIContent[] availableCultureLabels;
        [NonSerialized]
        private int preferredCultureIndex;

        private void GatherLocaleOptions()
        {
            string preferredCultureName = PackageLanguageManager.PreferredCulture.Name;

            this.availableCultures = PackageLanguageManager.DiscoverAvailableCultures();
            this.availableCultureLabels = (from culture in this.availableCultures select new GUIContent(culture.NativeName)).ToArray();
            this.preferredCultureIndex = Array.FindIndex(this.availableCultures, culture => culture.Name == preferredCultureName);
        }

        #endregion
    }
}
