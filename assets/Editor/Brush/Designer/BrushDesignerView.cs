// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Collections;
using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Base class for all brush designer views.
    /// </summary>
    /// <remarks>
    /// <para>It is likely that custom brush kinds will require their own specialized
    /// designer views. Custom brush views can be associated with custom brush kinds using
    /// <see cref="BrushUtility.RegisterDescriptor"/>.</para>
    /// <para>Each kind of brush can have two registered designers; one for editing brush
    /// instances, and another for editing aliases of its kind. Custom alias designers
    /// must extend <see cref="AliasBrushDesigner"/>.</para>
    /// </remarks>
    /// <seealso cref="AliasBrushDesigner"/>
    /// <seealso cref="BrushUtility.RegisterDescriptor"/>
    public abstract class BrushDesignerView : DesignerView
    {
        #region User Settings

        static BrushDesignerView()
        {
            var settings = AssetSettingManagement.GetGroup("Designer.Brush");

            s_ShowCustomPreviewSetting = settings.Fetch<bool>("ShowExtendedCustomPreview", false);
            s_ShowExtendedFlagsSetting = settings.Fetch<bool>("ShowExtendedFlags", false);
            s_ShowExtendedOrientationSetting = settings.Fetch<bool>("ShowExtendedOrientation", true);
        }


        private static readonly Setting<bool> s_ShowCustomPreviewSetting;
        private static readonly Setting<bool> s_ShowExtendedFlagsSetting;
        private static readonly Setting<bool> s_ShowExtendedOrientationSetting;


        /// <summary>
        /// Gets or sets a value indicating whether custom preview section should be shown
        /// under extended properties.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is persisted as an editor preference.</para>
        /// </remarks>
        public static bool ShowExtendedCustomPreview {
            get { return s_ShowCustomPreviewSetting; }
            set { s_ShowCustomPreviewSetting.Value = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether flags should be shown under extended
        /// properties.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is persisted as an editor preference.</para>
        /// </remarks>
        public static bool ShowExtendedFlags {
            get { return s_ShowExtendedFlagsSetting; }
            set { s_ShowExtendedFlagsSetting.Value = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether extended orientation properties should
        /// be shown.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is persisted as an editor preference.</para>
        /// </remarks>
        public static bool ShowExtendedOrientation {
            get { return s_ShowExtendedOrientationSetting; }
            set { s_ShowExtendedOrientationSetting.Value = value; }
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets brush that is being edited.
        /// </summary>
        public Brush Brush { get; internal set; }

        #endregion


        #region History States

        /// <inheritdoc/>
        public override void UpdateHistoryState(HistoryManager.State state)
        {
            base.UpdateHistoryState(state);

            HistoryState historyState = state as HistoryState;
            if (historyState == null) {
                return;
            }

            historyState.SetExpandedSectionState("ShowSectionMaterialMapper", ShowSectionMaterialMapper);

            historyState.SetExpandedSectionState("ShowExtendedCustomPreview", ShowExtendedCustomPreview);
            historyState.SetExpandedSectionState("ShowExtendedFlags", ShowExtendedFlags);
            historyState.SetExpandedSectionState("ShowExtendedOrientation", ShowExtendedOrientation);
        }

        /// <inheritdoc/>
        public override void RestoreHistoryState(HistoryManager.State state)
        {
            base.RestoreHistoryState(state);

            HistoryState historyState = state as HistoryState;
            if (historyState == null) {
                return;
            }

            ShowSectionMaterialMapper = historyState.GetExpandedSectionState("ShowSectionMaterialMapper");

            ShowExtendedCustomPreview = historyState.GetExpandedSectionState("ShowExtendedCustomPreview");
            ShowExtendedFlags = historyState.GetExpandedSectionState("ShowExtendedFlags");
            ShowExtendedOrientation = historyState.GetExpandedSectionState("ShowExtendedOrientation");
        }

        #endregion


        #region Messages and Events

        /// <summary>
        /// Label for brush name field.
        /// </summary>
        private string labelBrushName;

        /// <summary>
        /// Name of brush as specified by brush name field.
        /// </summary>
        /// <remarks>
        /// <para>Value of field will be persisted once the "Rename" button is clicked.</para>
        /// </remarks>
        protected string inputBrushName;


        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.labelBrushName = BrushUtility.GetDescriptor(Brush.GetType()).DisplayName;
            this.inputBrushName = this.Brush.name;
        }

        /// <inheritdoc/>
        protected internal override bool IsValid {
            get { return this.Brush != null; }
        }


        /// <summary>
        /// Occurs when header GUI is rendered and for GUI event handling.
        /// </summary>
        /// <remarks>
        /// <para>This means that your <see cref="OnFixedHeaderGUI"/> implementation might
        /// be called several times per frame (one call per event).</para>
        /// <para>The default implementation allows users to:</para>
        /// <list type="bullet">
        ///     <item>Rename brush</item>
        ///     <item>Mark brush as "static"</item>
        ///     <item>Mark brush as "smooth"</item>
        ///     <item>Hide brush</item>
        ///     <item>Set layer and tag for painted tiles</item>
        ///     <item>Categorize brush</item>
        /// </list>
        /// </remarks>
        public override void OnFixedHeaderGUI()
        {
            EditorGUIUtility.labelWidth = 80;

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            {
                this.DrawMenuArea();

                Rect previewPosition = GUILayoutUtility.GetRect(0, 0, GUILayout.Width(5 + 52));
                var brushRecord = BrushDatabase.Instance.FindRecord(this.Brush);
                RotorzEditorGUI.DrawBrushPreviewWithoutFallbackLabel(new Rect(previewPosition.x + 4, previewPosition.y, 52, 52), brushRecord, false);

                GUILayout.BeginVertical();
                {
                    // Properties common to all brush types
                    GUILayout.BeginHorizontal();
                    {
                        this.DrawBrushNameField();

                        GUILayout.Space(10);

                        this.BeginChangeCheck();

                        EmptyBrush emptyBrush = this.Brush as EmptyBrush;
                        Rect togglePosition;
                        bool guiStatic;


                        using (var content = ControlContent.Basic(
                            TileLang.ParticularText(/* Enables static optimization */ "Property", "Static"),
                            TileLang.Text("Static tiles can be combined when optimizing tile systems.")
                        )) {
                            togglePosition = GUILayoutUtility.GetRect(content, GUI.skin.toggle, RotorzEditorStyles.ContractWidth);
                            togglePosition.y += 2;

                            guiStatic = EditorGUI.ToggleLeft(togglePosition, content, this.Brush.Static);
                            if (guiStatic != this.Brush.Static) {
                                this.Brush.Static = guiStatic;
                            }
                        }
                        GUILayout.Space(10);


                        using (var content = ControlContent.Basic(
                            TileLang.ParticularText(/* Enables smooth visual joins */ "Property", "Smooth"),
                            TileLang.Text("Recalculates normals of touching vertices for smoother joins.")
                        )) {
                            // Don't bother showing "Smooth" field for empty or tileset brushes.
                            if (emptyBrush == null && !(this.Brush is TilesetBrush)) {
                                togglePosition = GUILayoutUtility.GetRect(content, GUI.skin.toggle, RotorzEditorStyles.ContractWidth);
                                togglePosition.y += 2;

                                // Disable "Smooth" field if "Static" field is not selected.
                                EditorGUI.BeginDisabledGroup(!guiStatic);
                                this.Brush.Smooth = EditorGUI.ToggleLeft(togglePosition, content, this.Brush.Smooth);
                                GUILayout.Space(8);
                                EditorGUI.EndDisabledGroup();
                            }
                        }


                        EditorGUI.BeginChangeCheck();
                        this.Brush.visibility = (BrushVisibility)EditorGUILayout.EnumPopup(this.Brush.visibility, RotorzEditorStyles.Instance.BrushVisibilityControl, GUILayout.Width(90));
                        if (EditorGUI.EndChangeCheck()) {
                            ++BrushDatabase.s_TimeLastUpdated;
                            ToolUtility.RepaintBrushPalette();
                        }


                        this.DrawHelpButton();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(8);

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 125;

                        this.DrawTagAndLayerFields();

                        using (var content = ControlContent.Basic(
                            TileLang.ParticularText("Property", "Category")
                        )) {
                            GUILayout.Label(content, RotorzEditorStyles.ContractWidth);
                            EditorGUI.BeginChangeCheck();
                            this.Brush.CategoryId = RotorzEditorGUI.BrushCategoryField(this.Brush.CategoryId);
                            if (EditorGUI.EndChangeCheck()) {
                                ++BrushDatabase.s_TimeLastUpdated;
                                ToolUtility.RepaintBrushPalette();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    // Compensate for difference in height caused by tick boxes?
                    if (!this.Brush.CanOverrideTagAndLayer) {
                        GUILayout.Space(1);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            this.EndChangeCheck();

            ExtraEditorGUI.SeparatorLight(marginTop: 6, marginBottom: 0, thickness: 3);
        }

        private void DrawTagAndLayerFields()
        {
            EditorGUI.BeginDisabledGroup(this.Brush is EmptyBrush);

            this.DrawTagField();
            this.DrawLayerField();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawTagField()
        {
            var aliasBrush = this.Brush as AliasBrush;

            if (!this.Brush.CanOverrideTagAndLayer) {
                GUILayout.Label(TileLang.ParticularText("Property", "Tag"), RotorzEditorStyles.ContractWidth);
            }
            else {
                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Tag"),
                    TileLang.Text("Tick to override tag of painted tiles.")
                )) {
                    Rect togglePosition = GUILayoutUtility.GetRect(content, GUI.skin.toggle, RotorzEditorStyles.ContractWidth);
                    this.Brush.overrideTag = EditorGUI.ToggleLeft(togglePosition, content, this.Brush.overrideTag);
                }
            }

            // Only enable tag GUI if tag override is enabled.
            EditorGUI.BeginDisabledGroup(this.Brush.CanOverrideTagAndLayer && !this.Brush.overrideTag);

            // Show ghost of target tag when not overridden.
            if (aliasBrush != null && !aliasBrush.overrideTag && aliasBrush.target != null) {
                EditorGUILayout.TagField(aliasBrush.target.tag);
            }
            else {
                this.Brush.tag = EditorGUILayout.TagField(this.Brush.tag);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawLayerField()
        {
            var aliasBrush = this.Brush as AliasBrush;

            if (!this.Brush.CanOverrideTagAndLayer) {
                GUILayout.Label(TileLang.ParticularText("Property", "Layer"), RotorzEditorStyles.ContractWidth);
            }
            else {
                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Layer"),
                    TileLang.Text("Tick to override layer of painted tiles.")
                )) {
                    Rect togglePosition = GUILayoutUtility.GetRect(content, GUI.skin.toggle, RotorzEditorStyles.ContractWidth);
                    this.Brush.overrideLayer = EditorGUI.ToggleLeft(togglePosition, content, this.Brush.overrideLayer);
                }
            }

            // Only enable layer GUI if layer override is enabled.
            EditorGUI.BeginDisabledGroup(this.Brush.CanOverrideTagAndLayer && !this.Brush.overrideLayer);

            // Show ghost of target layer when not overridden.
            if (aliasBrush != null && !aliasBrush.overrideLayer && aliasBrush.target != null) {
                EditorGUILayout.LayerField(aliasBrush.target.layer);
            }
            else {
                this.Brush.layer = EditorGUILayout.LayerField(this.Brush.layer);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawBrushNameField()
        {
            GUILayout.Label(this.labelBrushName, RotorzEditorStyles.Instance.LabelMiddleLeft);

            this.inputBrushName = EditorGUILayout.TextField(this.inputBrushName, RotorzEditorStyles.Instance.TextFieldRoundEdge);

            var currentBrushName = this.Brush.name;
            if (this.inputBrushName != currentBrushName) {
                string filteredName = this.inputBrushName;

                // Limit to 70 characters.
                if (filteredName.Length > 70) {
                    filteredName = filteredName.Substring(0, 70);
                }

                // Restrict to alphanumeric characters.
                filteredName = Regex.Replace(filteredName, "[^- A-Za-z0-9_+!~#()]+", "");

                // Display reset button for any changes made to brush name.
                using (var content = ControlContent.Basic(
                    "",
                    TileLang.ParticularText("Action", "Restore Current Name")
                )) {
                    if (GUILayout.Button(content, RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButton)) {
                        GUIUtility.keyboardControl = 0;
                        this.inputBrushName = currentBrushName;
                        GUIUtility.ExitGUI();
                    }
                }

                // Only display rename button when changes can actually be applied.
                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Action", "Rename")
                )) {
                    if (!string.IsNullOrEmpty(filteredName) && filteredName != currentBrushName) {
                        if (GUILayout.Button(content, RotorzEditorStyles.Instance.ButtonPaddedExtra)) {
                            currentBrushName = this.inputBrushName = filteredName;
                            this.OnRename(this.inputBrushName);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
            }
            else {
                GUILayout.Label(GUIContent.none, RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButtonEmpty);
            }
        }

        /// <inheritdoc/>
        protected internal override void BeginExtendedProperties()
        {
            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Group #"),
                TileLang.Text("Logical group that brush belongs to. This is used when specifying more advanced coalescing rules.")
            )) {
                this.Brush.group = EditorGUILayout.IntField(content, this.Brush.group, GUI.skin.textField);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Force Legacy Sideways"),
                TileLang.Text("Forces legacy behavior for tile systems with sideways facing tiles.")
            )) {
                // The property does not apply to alias or empty brushes.
                if (!(this.Brush is AliasBrush || this.Brush is EmptyBrush)) {
                    ExtraEditorGUI.SeparatorLight();

                    this.Brush.forceLegacySideways = EditorGUILayout.ToggleLeft(content, this.Brush.forceLegacySideways);
                    ExtraEditorGUI.TrailingTip(content);
                }
            }
        }

        /// <summary>
        /// Occurs when rendering and handling GUI events of extended properties.
        /// </summary>
        /// <remarks>
        /// <para>This means that your <see cref="OnExtendedPropertiesGUI"/> implementation
        /// might be called several times per frame (one call per event).</para>
        /// </remarks>
        /// <example>
        /// <para>Example usage for custom extended properties GUI:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomBrushDesigner : BrushDesigner
        /// {
        ///     public override void OnExtendedPropertiesGUI()
        ///     {
        ///         base.OnExtendedPropertiesGUI();
        ///
        ///         ShowExtendedOrientation = RotorzEditorGUI.TitleFoldout(ShowExtendedOrientation, "Automatic Orientation");
        ///         if (ShowExtendedOrientation) {
        ///             // These coalescing fields.
        ///             this.ExtendedProperties_Coalescing();
        ///             // Nice to end foldout section with light splitter.
        ///             GUILayout.Space(5);
        ///         }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <seealso cref="OnExtendedGUI_ScaleMode"/>
        /// <seealso cref="OnExtendedGUI_Coalescing"/>
        public override void OnExtendedPropertiesGUI()
        {
            this.OnExtendedGUI_ScaleMode();

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Disable immediate preview"),
                TileLang.Text("In-editor preview of tile can be disabled on a per brush basis.")
            )) {
                this.Brush.disableImmediatePreview = EditorGUILayout.ToggleLeft(content, this.Brush.disableImmediatePreview);
                ExtraEditorGUI.TrailingTip(content);
            }
        }

        /// <inheritdoc/>
        protected internal override void EndExtendedProperties()
        {
            ShowExtendedCustomPreview = RotorzEditorGUI.FoldoutSection(ShowExtendedCustomPreview,
                label: TileLang.Text("Custom Preview"),
                callback: this.OnExtendedGUI_CustomPreview
            );

            ShowExtendedFlags = RotorzEditorGUI.FoldoutSection(ShowExtendedFlags,
                label: TileLang.Text("Flags"),
                callback: this.OnExtendedGUI_Flags
            );
        }

        #endregion


        #region Menu Commands and Buttons

        /// <summary>
        /// Draw menu area in upper-left corner of designer window.
        /// </summary>
        /// <remarks>
        /// <para>Menu area contains primary and the optional secondary menu buttons.</para>
        /// </remarks>
        /// <seealso cref="AddItemsToMenu"/>
        /// <seealso cref="DrawSecondaryMenuButton"/>
        private void DrawMenuArea()
        {
            GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(93), GUILayout.Height(48));

            this.DrawMenuButton(new Rect(2, 29, 44, 26), TileLang.Text("Brush Menu"));
            this.DrawSecondaryMenuButton(new Rect(47, 29, 44, 26));

            ExtraEditorGUI.SeparatorLight(new Rect(0, 26, 91, 1));
        }

        /// <summary>
        /// Draw secondary menu button.
        /// </summary>
        /// <example>
        /// <para>Here is how one might typically implement this method:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void DrawSecondaryMenuButton(Rect position)
        /// {
        ///     EditorGUI.BeginDisabledGroup(this.xyz == null);
        ///
        ///     if (RotorzEditorGUI.HoverButton(this.Window, position, this.ButtonIconContent)) {
        ///         this.Foo();
        ///     }
        ///
        ///     EditorGUI.EndDisabledGroup();
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="position">Position of button in window.</param>
        public virtual void DrawSecondaryMenuButton(Rect position)
        {
        }

        /// <inheritdoc/>
        public override void AddItemsToMenu(EditorMenu menu)
        {
            base.AddItemsToMenu(menu);

            menu.AddCommand(TileLang.ParticularText("Action", "Reveal Asset"))
                .Action(() => {
                    EditorGUIUtility.PingObject(this.Brush);
                });

            menu.AddSeparator();

            menu.AddCommand(TileLang.ParticularText("Action", "Refresh Preview"))
                .Action(() => {
                    BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
                    Window.Repaint();
                });

            menu.AddSeparator();

            menu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Create Duplicate")))
                .Action(() => {
                    CreateBrushWindow window = CreateBrushWindow.ShowWindow<DuplicateBrushCreator>();
                    window.SharedProperties["targetBrush"] = this.Brush;
                });

            menu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Create Alias")))
                .Action(() => {
                    CreateBrushWindow window = CreateBrushWindow.ShowWindow<AliasBrushCreator>();
                    window.SharedProperties["targetBrush"] = this.Brush;
                });

            menu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Delete Brush")))
                .Action(() => {
                    TileSystemCommands.DeleteBrush(this.Brush, true);
                });
        }

        /// <summary>
        /// Occurs when "Rename" button is clicked.
        /// </summary>
        /// <param name="newName">New name for brush.</param>
        protected virtual void OnRename(string newName)
        {
            try {
                this.inputBrushName = BrushDatabase.Instance.RenameBrush(this.Brush, newName);

                // Defocus name input field.
                GUIUtility.keyboardControl = 0;

                ToolUtility.RepaintBrushPalette();

                // Ensure that brush is properly selected.
                DesignerWindow designerWindow = Window as DesignerWindow;
                // Only update selection in brush list if designer window is not locked!
                if (this.Brush == ToolUtility.SelectedBrush && !Window.IsLocked) {
                    ToolUtility.RevealBrush(this.Brush);
                }
            }
            catch (ArgumentException ex) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "Was unable to rename brush"),
                    ex.Message,
                    TileLang.ParticularText("Action", "OK")
                );
            }
        }

        #endregion


        #region Help Button

        private void DrawHelpButton()
        {
            GUILayout.Space(38);

            Rect position = new Rect(Window.position.width - 37, 2, 34, 26);

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.ContextHelp,
                TileLang.ParticularText("Action", "Help")
            )) {
                if (EditorInternalUtility.DropdownMenu(position, content, RotorzEditorStyles.Instance.FlatButton)) {
                    var helpMenu = new EditorMenu();

                    helpMenu.AddCommand(TileLang.ParticularText("Action", "Show Tips"))
                        .Checked(ControlContent.TrailingTipsVisible)
                        .Action(() => {
                            ControlContent.TrailingTipsVisible = !ControlContent.TrailingTipsVisible;
                        });

                    position.y -= 2;
                    helpMenu.ShowAsDropdown(position);
                }
            }
        }

        #endregion


        #region Material Mapper

        /// <summary>
        /// Indicates whether "Material Mapper:" section is shown in designer.
        /// </summary>
        public static bool ShowSectionMaterialMapper = true;


        private BrushDesignerMaterialMapper brushDesignerMaterialMapper;


        /// <summary>
        /// Render and handle GUI events for material mapping.
        /// </summary>
        /// <remarks>
        /// <para>Unlike with extended properties, rendered controls are encapsulated
        /// within a foldout section.</para>
        /// <para>Interface is only displayed for brushes that implement <see cref="IMaterialMappings"/>.</para>
        /// </remarks>
        /// <seealso cref="DesignerView.OnGUI"/>
        protected void Section_MaterialMapper()
        {
            var materialMappings = this.Brush as IMaterialMappings;
            if (materialMappings == null) {
                return;
            }

            if (this.brushDesignerMaterialMapper == null) {
                this.brushDesignerMaterialMapper = new BrushDesignerMaterialMapper(this);
            }

            this.BeginFixedSection();

            // Draw a button to add the first material mapping when there are no material
            // mappings; otherwise, draw the material mapper section.
            if (materialMappings.MaterialMappingFrom.Length == 0) {
                if (GUILayout.Button(TileLang.ParticularText("Action", "Add Material Mapping"), ExtraEditorStyles.Instance.BigButton)) {
                    ShowSectionMaterialMapper = true;
                    this.brushDesignerMaterialMapper.OnAddMaterialMapping();
                }
            }
            else {
                ShowSectionMaterialMapper = RotorzEditorGUI.FoldoutSection(ShowSectionMaterialMapper,
                    label: TileLang.Text("Material Mapper"),
                    callback: () => this.brushDesignerMaterialMapper.OnGUI()
                );
            }

            GUILayoutUtility.GetRect(0, 3);

            this.EndFixedSection();
        }

        #endregion


        #region Extended Properties Library

        /// <summary>
        /// Extended property "Scale Mode" for use within extended properties GUI.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>Renders various controls plus handles GUI events.</item>
        ///     <item>Suitable for use with alias brush designers (see <see cref="AliasBrushDesigner"/>).</item>
        ///     <item>Includes tips when <see cref="RtsPreferences.ShowTips"/> is set to <c>true</c>.</item>
        ///     <item>Foldout container must be added manually if desired.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <para>Example usage for entirely custom extended properties GUI:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomBrushDesigner : BrushDesigner
        /// {
        ///     public override void OnExtendedPropertiesGUI()
        ///     {
        ///         // Other custom GUI...
        ///
        ///         this.ExtendedProperty_ScaleMode();
        ///         ExtraEditorGUI.SeparatorLight();
        ///
        ///         // Other custom GUI...
        ///     }
        ///
        /// }
        /// ]]></code>
        /// <para>In most scenarios the following is equivalent because "Scale Mode"
        /// is included by default.</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomBrushDesigner : BrushDesigner
        /// {
        ///     public override void OnExtendedPropertiesGUI()
        ///     {
        ///         // Other custom GUI...
        ///
        ///         base.OnExtendedPropertiesGUI();
        ///
        ///         // Other custom GUI...
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <seealso cref="OnExtendedPropertiesGUI"/>
        protected void OnExtendedGUI_ScaleMode()
        {
            var sourceBrush = this.Brush;
            bool overrideTransforms = false;

            EditorGUI.BeginChangeCheck();

            var aliasBrush = this.Brush as AliasBrush;
            var tilesetBrush = this.Brush as TilesetBrush;

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Override Transforms"),
                TileLang.Text("Overrides transform of target brush.")
            )) {
                if (aliasBrush != null) {
                    overrideTransforms = EditorGUILayout.ToggleLeft(content, aliasBrush.overrideTransforms);
                    ++EditorGUI.indentLevel;

                    if (!aliasBrush.overrideTransforms && aliasBrush.target != null) {
                        sourceBrush = aliasBrush.target;
                    }
                }
            }

            // Properties that are shown from source brush are not editable when values
            // are being shown from elsewhere (i.e. the target of an alias brush).
            EditorGUI.BeginDisabledGroup(sourceBrush != this.Brush);

            bool applyPrefabTransform;
            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Apply Prefab Transform"),
                TileLang.Text("Tick to use prefab transform to offset position, rotation and scale of painted tiles.")
            )) {
                applyPrefabTransform = EditorGUILayout.ToggleLeft(content, sourceBrush.applyPrefabTransform);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Apply Simple Rotation"),
                TileLang.Text("Applies simple tile rotation to tile object.")
            )) {
                if (tilesetBrush != null) {
                    tilesetBrush.applySimpleRotationToAttachment = EditorGUILayout.ToggleLeft(content, tilesetBrush.applySimpleRotationToAttachment);
                    ExtraEditorGUI.TrailingTip(content);
                }
            }

            GUILayout.Space(3);

            ScaleMode inputScaleMode;
            Vector3 inputScaleVector;

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Scale Mode"),
                TileLang.Text("Specifies the way in which painted tiles should be scaled.")
            )) {
                inputScaleMode = (ScaleMode)EditorGUILayout.EnumPopup(content, sourceBrush.scaleMode);

                inputScaleVector = sourceBrush.transformScale;
                if (sourceBrush.scaleMode == ScaleMode.Custom) {
                    // Cancel out label above vector field.
                    inputScaleVector = EditorGUILayout.Vector3Field("", inputScaleVector);
                    GUILayout.Space(-17);
                }

                ExtraEditorGUI.TrailingTip(content);
            }

            EditorGUI.EndDisabledGroup();

            // Apply changes to brush?
            if (EditorGUI.EndChangeCheck()) {
                // Do not update other properties if "Override Transforms" has been changed.
                if (aliasBrush != null && overrideTransforms != aliasBrush.overrideTransforms) {
                    aliasBrush.overrideTransforms = overrideTransforms;
                }
                else {
                    // Update properties that have changed.
                    if (this.Brush.scaleMode != inputScaleMode) {
                        this.Brush.scaleMode = inputScaleMode;

                        // Default to one when one or more components are zero.
                        if (inputScaleMode == ScaleMode.Custom && (this.Brush.transformScale.x == 0 || this.Brush.transformScale.y == 0 || this.Brush.transformScale.z == 0)) {
                            inputScaleVector = Vector3.one;
                        }
                    }

                    this.Brush.applyPrefabTransform = applyPrefabTransform;

                    if (sourceBrush.scaleMode == ScaleMode.Custom) {
                        if (this.Brush.transformScale != inputScaleVector) {
                            this.Brush.transformScale = inputScaleVector;
                        }
                    }
                }

                this.SetDirty();
            }

            if (aliasBrush != null) {
                --EditorGUI.indentLevel;
            }
        }

        /// <summary>
        /// Extended property "Coalescing" for use within extended properties GUI.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>Renders various controls plus handles GUI events.</item>
        ///     <item>Suitable for use with alias brush designers (see <see cref="AliasBrushDesigner"/>).</item>
        ///     <item>Includes tips when <see cref="RtsPreferences.ShowTips"/> is set to <c>true</c>.</item>
        ///     <item>Foldout container must be added manually if desired.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <para>Example usage for custom extended properties GUI:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomBrushDesigner : BrushDesigner
        /// {
        ///     public override void OnExtendedPropertiesGUI()
        ///     {
        ///         base.OnExtendedPropertiesGUI();
        ///
        ///         ShowExtendedOrientation = RotorzEditorGUI.TitleFoldout(ShowExtendedOrientation, "Automatic Orientation");
        ///         if (ShowExtendedOrientation) {
        ///             // These coalescing fields
        ///             this.ExtendedProperties_Coalescing();
        ///             // Nice to end foldout section with light splitter
        ///             ExtraEditorGUI.SeparatorLight();
        ///         }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <seealso cref="OnExtendedPropertiesGUI"/>
        protected void OnExtendedGUI_Coalescing()
        {
            var coalescableBrush = this.Brush as ICoalescableBrush;
            if (coalescableBrush == null) {
                return;
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Coalesce Mode"),
                TileLang.Text("Specifies how adjacent tiles should coalesce (or \"join\") with one another.")
            )) {
                coalescableBrush.Coalesce = (Coalesce)EditorGUILayout.EnumPopup(content, coalescableBrush.Coalesce);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "With Group #"),
                TileLang.Text("List of brush group numbers that painted tiles should coalesce with.")
            )) {
                if (coalescableBrush.Coalesce == Coalesce.Groups || coalescableBrush.Coalesce == Coalesce.OwnAndGroups) {
                    ++EditorGUI.indentLevel;

                    this.DrawCoalesceGroupList(content, coalescableBrush.CoalesceWithBrushGroups);
                    ExtraEditorGUI.TrailingTip(content);

                    --EditorGUI.indentLevel;
                }
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Coalesce with Rotated"),
                TileLang.Text("Indicates if painted tiles should coalesce with those of a different rotation.")
            )) {
                coalescableBrush.CoalesceWithRotated = EditorGUILayout.ToggleLeft(content, coalescableBrush.CoalesceWithRotated);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Coalesce with Border"),
                TileLang.Text("Indicates if painted tiles should coalesce with tile system boundaries.")
            )) {
                coalescableBrush.CoalesceWithBorder = EditorGUILayout.ToggleLeft(content, coalescableBrush.CoalesceWithBorder);
                ExtraEditorGUI.TrailingTip(content);
            }
        }


        #region Coalesce Group List Control

        private string inputNewCoalesceWithGroupNumber = "";


        private void DrawCoalesceGroupList(GUIContent label, ICollection<int> coalesceWithGroups)
        {
            int entryHeight = 19;

            Rect position = GUILayoutUtility.GetRect(0, entryHeight * (coalesceWithGroups.Count + 1));
            position = EditorGUI.PrefixLabel(position, label);

            int restoreIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect itemPosition = position;
            itemPosition.y -= 1;
            itemPosition.height = entryHeight - 1;

            int remainingCount = coalesceWithGroups.Count;
            foreach (int withGroup in coalesceWithGroups) {
                this.DrawCoalesceGroupListItem(itemPosition, coalesceWithGroups, withGroup);

                itemPosition.y += itemPosition.height;

                if (--remainingCount != 0) {
                    ExtraEditorGUI.SeparatorLight(new Rect(itemPosition.x, itemPosition.y, itemPosition.width, 1));
                }
            }

            // Draw control for adding new group.
            var footerButtonStyle = ReorderableListStyles.Instance.FooterButton2;

            // Add new coalesce group upon pressing return key.
            if (Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl != 0 && GUI.GetNameOfFocusedControl() == "NewCoalesceGroup") {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) {
                    this.AttemptToAddCoalesceWithGroup(coalesceWithGroups);
                }
            }

            GUI.SetNextControlName("NewCoalesceGroup");
            itemPosition.width -= 30;
            itemPosition.height = footerButtonStyle.fixedHeight;
            this.inputNewCoalesceWithGroupNumber = EditorGUI.TextField(itemPosition, this.inputNewCoalesceWithGroupNumber);

            Rect addButtonPosition = itemPosition;
            addButtonPosition.x = itemPosition.xMax;
            addButtonPosition.width = 30;

            var addIcon = ReorderableListStyles.Skin.Icon_Add_Normal;
            var addIconActive = ReorderableListStyles.Skin.Icon_Add_Active;

            if (ExtraEditorGUI.IconButton(addButtonPosition, addIcon, addIconActive, footerButtonStyle)) {
                this.AttemptToAddCoalesceWithGroup(coalesceWithGroups);
            }

            EditorGUI.indentLevel = restoreIndentLevel;
        }

        private void AttemptToAddCoalesceWithGroup(ICollection<int> coalesceWithGroups)
        {
            int otherGroup;
            if (int.TryParse(this.inputNewCoalesceWithGroupNumber, out otherGroup)) {
                this.inputNewCoalesceWithGroupNumber = "";
                coalesceWithGroups.Add(otherGroup);
                this.SetDirty();
                GUIUtility.keyboardControl = 0;
                GUIUtility.ExitGUI();
            }
        }

        private void DrawCoalesceGroupListItem(Rect position, ICollection<int> coalesceWithGroups, int withGroup)
        {
            var itemButtonStyle = ReorderableListStyles.Instance.ItemButton;

            Rect labelPosition = new Rect(position.x, position.y + 1, position.width - 30, position.height - 1);
            GUI.Label(labelPosition, withGroup.ToString());

            var removeIcon = ReorderableListStyles.Skin.Icon_Remove_Normal;
            var removeIconActive = ReorderableListStyles.Skin.Icon_Remove_Active;

            Rect removeButtonPosition = new Rect(labelPosition.xMax, position.y, 30, position.height);
            if (ExtraEditorGUI.IconButton(removeButtonPosition, removeIcon, removeIconActive, itemButtonStyle)) {
                coalesceWithGroups.Remove(withGroup);
                this.SetDirty();
                GUIUtility.ExitGUI();
            }
        }

        #endregion


        private void OnExtendedGUI_CustomPreview()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            this.Brush.customPreviewImage = EditorGUILayout.ObjectField(this.Brush.customPreviewImage, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100)) as Texture2D;
            if (EditorGUI.EndChangeCheck()) {
                this.SetDirty();
                ToolUtility.RepaintBrushPalette();
            }

            GUILayout.Space(5);
            GUILayout.BeginVertical();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Clear"))) {
                this.Brush.customPreviewImage = null;
                this.SetDirty();
                ToolUtility.RepaintBrushPalette();

                GUIUtility.ExitGUI();
            }

            GUILayout.Space(15);
            GUILayout.Label(TileLang.Text("Power of two recommended\n\ni.e. 128x128"), EditorStyles.wordWrappedMiniLabel);

            GUILayout.EndVertical();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Use at design time"),
                TileLang.Text("Tick to use custom preview within Unity editor.")
            )) {
                EditorGUI.BeginChangeCheck();
                this.Brush.customPreviewDesignTime = EditorGUILayout.ToggleLeft(content, this.Brush.customPreviewDesignTime);
                if (EditorGUI.EndChangeCheck()) {
                    ToolUtility.RepaintBrushPalette();
                }
                ExtraEditorGUI.TrailingTip(content);
            }
        }

        private string[] userFlagLabels;

        private void UpdateFlagLabels()
        {
            this.userFlagLabels = this.Brush.UserFlagLabels;

            string[] defaultFlagLabels = ProjectSettings.Instance.FlagLabels;
            string label;

            string defaultFlagLabel = TileLang.ParticularText("Flaggable", "Flag");
            string userFlagFormat = TileLang.ParticularText("Format|UserFlagLabel",
                /* i.e. '9: My Ninth Flag'
                   0: flag number
                   1: flag label */
                "{0:00}: {1}"
            );

            for (int i = 0; i < this.userFlagLabels.Length; ++i) {
                // Assume default flag label.
                if (!string.IsNullOrEmpty(this.userFlagLabels[i])) {
                    label = this.userFlagLabels[i];
                }
                else if (!string.IsNullOrEmpty(defaultFlagLabels[i])) {
                    label = defaultFlagLabels[i];
                }
                else {
                    label = defaultFlagLabel;
                }

                this.userFlagLabels[i] = string.Format(userFlagFormat, i + 1, label);
            }
        }

        private void OnExtendedGUI_Flags()
        {
            var orientedBrush = this.Brush as OrientedBrush;
            var aliasBrush = this.Brush as AliasBrush;

            if (ControlContent.TrailingTipsVisible) {
                ExtraEditorGUI.TrailingTip(TileLang.Text("Flags can be used in custom scripts. Use of flags is entirely user defined!"));
            }

            if (orientedBrush != null) {
                using (var content = ControlContent.WithTrailableTip(
                    TileLang.ParticularText("Property", "Force Override Flags"),
                    TileLang.Text("Overrides flags of nested brushes.")
                )) {
                    bool newOverrideFlags = EditorGUILayout.ToggleLeft(content, orientedBrush.forceOverrideFlags);
                    if (newOverrideFlags != orientedBrush.forceOverrideFlags) {
                        orientedBrush.forceOverrideFlags = newOverrideFlags;
                        this.SetDirty();
                        GUIUtility.ExitGUI();
                    }
                    ExtraEditorGUI.TrailingTip(content);
                }

                ExtraEditorGUI.SeparatorLight();

                // Note: Do not disable user input!
            }
            else if (aliasBrush != null) {
                using (var content = ControlContent.WithTrailableTip(
                    TileLang.ParticularText("Property", "Override Flags"),
                    TileLang.Text("Overrides flags of the target brush.")
                )) {
                    bool newOverrideFlags = EditorGUILayout.ToggleLeft(content, aliasBrush.overrideFlags);
                    if (newOverrideFlags != aliasBrush.overrideFlags) {
                        aliasBrush.overrideFlags = newOverrideFlags;

                        // Copy flags from target brush where possible.
                        if (newOverrideFlags && aliasBrush.target != null) {
                            aliasBrush.TileFlags = aliasBrush.target.TileFlags;
                        }

                        this.SetDirty();
                        GUIUtility.ExitGUI();
                    }
                    ExtraEditorGUI.TrailingTip(content);
                }

                ExtraEditorGUI.SeparatorLight();
            }

            // Not overriding flags of target brush?
            EditorGUI.BeginDisabledGroup(aliasBrush != null && !aliasBrush.overrideFlags);

            // "Solid" flag is not shown for autotile brushes because it is shown
            // elsewhere in same interface.
            if (!(this.Brush is AutotileBrush)) {
                GUILayout.BeginHorizontal();

                using (var content = ControlContent.WithTrailableTip(
                    TileLang.ParticularText("Flaggable", "Solid Flag"),
                    TileLang.Text("Solid flag can be used to assist with user defined collision detection or pathfinding.")
                )) {
                    EditorGUI.BeginChangeCheck();
                    this.Brush.SolidFlag = EditorGUILayout.ToggleLeft(content, this.Brush.SolidFlag);
                    if (EditorGUI.EndChangeCheck()) {
                        this.SetDirty();
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.EndHorizontal();

                    ExtraEditorGUI.TrailingTip(content);
                }

                ExtraEditorGUI.SeparatorLight();
            }

            GUILayout.BeginHorizontal();

            if (Event.current.type == EventType.Layout || this.userFlagLabels == null) {
                this.UpdateFlagLabels();
            }

            // Calculate metrics for flag toggle control.
            var flagToggleStyle = EditorStyles.toggle;
            float flagToggleHeight = flagToggleStyle.CalcHeight(GUIContent.none, 0);

            Rect flagTogglePosition = EditorGUILayout.GetControlRect(false, (flagToggleHeight + 4) * 8);
            flagTogglePosition.width = flagTogglePosition.width / 2 - flagToggleStyle.margin.left;
            flagTogglePosition.height = flagToggleHeight;

            float resetY = flagTogglePosition.y;

            // Custom user flags 1 to 16.
            for (int flagNumber = 1; flagNumber <= 16; ++flagNumber) {
                if (flagNumber == 9) {
                    flagTogglePosition.x = flagTogglePosition.xMax + 2;
                    flagTogglePosition.y = resetY;
                }

                bool userFlagState = this.Brush.GetUserFlag(flagNumber);
                EditorGUI.BeginChangeCheck();
                EditorGUI.ToggleLeft(flagTogglePosition, this.userFlagLabels[flagNumber - 1], userFlagState);
                if (EditorGUI.EndChangeCheck()) {
                    this.Brush.SetUserFlag(flagNumber, !userFlagState);
                    this.SetDirty();
                }

                flagTogglePosition.y = flagTogglePosition.yMax + 4;
            }

            GUILayout.EndHorizontal();

            Rect toolbarPosition = GUILayoutUtility.GetRect(0f, 20f);
            toolbarPosition.width -= 29 + 2;

            string[] quickSelectButtons = {
                TileLang.ParticularText("Action|Select", "All"),
                TileLang.ParticularText("Action|Select", "None"),
                TileLang.ParticularText("Action|Select", "Invert"),
            };

            switch (GUI.Toolbar(toolbarPosition, -1, quickSelectButtons)) {
                case 0:
                    this.Brush.TileFlags |= 0xFFFF;
                    this.SetDirty();
                    GUIUtility.ExitGUI();
                    break;
                case 1:
                    this.Brush.TileFlags &= ~0xFFFF;
                    this.SetDirty();
                    GUIUtility.ExitGUI();
                    break;
                case 2:
                    this.Brush.TileFlags = (this.Brush.TileFlags & ~0xFFFF) | (~this.Brush.TileFlags & 0xFFFF);
                    this.SetDirty();
                    GUIUtility.ExitGUI();
                    break;
            }

            toolbarPosition.x = toolbarPosition.xMax + 2;
            toolbarPosition.width = 29;

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.EditLabel,
                TileLang.ParticularText("Action", "Edit Flag Labels")
            )) {
                if (RotorzEditorGUI.HoverButton(toolbarPosition, content)) {
                    EditFlagLabelsWindow.ShowWindow(this.Brush);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        #endregion


        #region Methods

        /// <summary>
        /// Set brush as dirty so that Unity can save changes.
        /// </summary>
        public override void SetDirty()
        {
            if (this.Brush != null) {
                EditorUtility.SetDirty(this.Brush);
            }
        }

        #endregion
    }
}
