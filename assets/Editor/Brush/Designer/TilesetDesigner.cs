// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Tile.Editor.Internal;
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal class TilesetDesigner : DesignerView
    {
        #region Properties

        /// <inheritdoc/>
        public override bool HasExtendedProperties {
            get { return false; }
        }

        /// <summary>
        /// Gets the tileset that is being edited.
        /// </summary>
        public Tileset Tileset { get; internal set; }

        #endregion


        #region Messages and Events

        private TilesetAssetRecord tilesetRecord;
        private string inputTilesetName;

        private ITilesetDesignerTab[] tabs;


        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.inputTilesetName = this.Tileset.name;

            this.tabs = new ITilesetDesignerTab[] {
                new BrushCreatorTab(this),
                null,
                new TilesetInfoTab(this),
            };

            if (this.Tileset is AutotileTileset) {
                this.tabs[1] = new ModifyAutotileTab(this);
            }
            else {
                this.tabs[1] = new ModifyTilesetTab(this);
            }

            this.tabs[0].OnEnable();
            this.tabs[1].OnEnable();
            this.tabs[2].OnEnable();
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            base.OnDisable();

            foreach (var tab in this.tabs) {
                if (tab != null) {
                    tab.OnDisable();
                }
            }
        }

        /// <inheritdoc/>
        protected internal override bool IsValid {
            get { return this.Tileset != null; }
        }

        /// <inheritdoc/>
        protected internal override void BeginView()
        {
            var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(this.Tileset);
            if (tilesetRecord != this.tilesetRecord) {
                this.tilesetRecord = tilesetRecord;

                foreach (var tab in this.tabs) {
                    tab.OnNewTilesetRecord(this.tilesetRecord);
                }
            }

            base.BeginView();
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
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(90);

                EditorGUIUtility.labelWidth = 80;
                this.DrawTilesetNameField();

                GUILayout.Label(
                    TileLang.PluralText(
                        /* 0: quantity of brushes */
                        "Contains 1 brush",
                        "Contains {0} brushes",
                        this.tilesetRecord.BrushRecords.Count
                    ),
                    RotorzEditorStyles.Instance.LabelMiddleLeft
                );

                Rect menuPosition = GUILayoutUtility.GetRect(GUIContent.none, RotorzEditorStyles.Instance.LabelMiddleLeft, GUILayout.Width(45));
                this.DrawMenuButton(new Rect(menuPosition.x, 2, 44, 26), TileLang.Text("Tileset Menu"));

                this.DrawHelpButton();
            }
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 125;

            ExtraEditorGUI.SeparatorLight(marginTop: 7, marginBottom: 0, thickness: 3);

            this.DrawTabs();
        }

        internal static int s_SelectedTab = 0;

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();

            for (int i = 0; i < this.tabs.Length; ++i) {
                using (var tabLabelContent = ControlContent.Basic(this.tabs[i].Label)) {
                    Rect position = GUILayoutUtility.GetRect(tabLabelContent, RotorzEditorStyles.Instance.Tab);
                    int controlID = GUIUtility.GetControlID(FocusType.Passive, position);

                    switch (Event.current.GetTypeForControl(controlID)) {
                        case EventType.MouseDown:
                            if (position.Contains(Event.current.mousePosition)) {
                                s_SelectedTab = i;
                                Event.current.Use();
                            }
                            break;

                        case EventType.Repaint:
                            RotorzEditorStyles.Instance.Tab.Draw(position, tabLabelContent, false, false, s_SelectedTab == i, false);
                            break;
                    }
                }
            }

            GUILayout.Box(GUIContent.none, RotorzEditorStyles.Instance.TabBackground);
            GUILayout.EndHorizontal();
        }

        private void DrawTilesetNameField()
        {
            GUILayout.Label(TileLang.ParticularText("Property", "Tileset"), RotorzEditorStyles.Instance.LabelMiddleLeft);

            this.inputTilesetName = EditorGUILayout.TextField(this.inputTilesetName, RotorzEditorStyles.Instance.TextFieldRoundEdge);

            var currentTilesetName = Tileset.name;
            if (this.inputTilesetName != currentTilesetName) {
                string filteredName = this.inputTilesetName;

                // Limit to 70 characters.
                if (filteredName.Length > 70) {
                    filteredName = filteredName.Substring(0, 70);
                }

                // Restrict to alphanumeric characters.
                filteredName = Regex.Replace(filteredName, "[^- A-Za-z0-9_+!~#()]+", "");

                using (var content = ControlContent.Basic(
                    "",
                    TileLang.ParticularText("Action", "Restore Current Name")
                )) {
                    if (GUILayout.Button(content, RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButton)) {
                        GUIUtility.keyboardControl = 0;
                        this.inputTilesetName = currentTilesetName;
                        GUIUtility.ExitGUI();
                    }
                }

                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Action", "Rename")
                )) {
                    if (!string.IsNullOrEmpty(filteredName) && filteredName != currentTilesetName) {
                        if (GUILayout.Button(content, RotorzEditorStyles.Instance.ButtonPaddedExtra)) {
                            currentTilesetName = this.inputTilesetName = filteredName;
                            this.OnRename(this.inputTilesetName);
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
        public override void AddItemsToMenu(EditorMenu menu)
        {
            base.AddItemsToMenu(menu);

            menu.AddCommand(TileLang.ParticularText("Action", "Reveal Material"))
                .Action(() => {
                    EditorInternalUtility.FocusInspectorWindow();
                    EditorGUIUtility.PingObject(Tileset.AtlasMaterial);
                    Selection.activeObject = Tileset.AtlasMaterial;
                });

            menu.AddCommand(TileLang.ParticularText("Action", "Reveal Texture"))
                .Action(() => {
                    if (Tileset.AtlasMaterial.mainTexture != null) {
                        EditorInternalUtility.FocusInspectorWindow();
                        EditorGUIUtility.PingObject(Tileset.AtlasMaterial.mainTexture);
                        Selection.activeObject = Tileset.AtlasMaterial.mainTexture;
                    }
                });

            // Only display "Cleanup Meshes" command when meshes are actually present!
            if (Tileset.tileMeshes != null && Tileset.tileMeshes.Length != 0) {
                menu.AddSeparator();

                menu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Cleanup Meshes")))
                    .Action(() => {
                        CleanupTilesetMeshesWindow.ShowWindow(Tileset);
                    });
            }

            menu.AddSeparator();

            menu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Delete Tileset")))
                .Action(() => {
                    DeleteTilesetWindow.ShowWindow(Tileset);
                });
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            this.tabs[s_SelectedTab].OnGUI();
        }

        internal override void Draw()
        {
            ITilesetDesignerTab tab = this.tabs[s_SelectedTab];

            this.viewScrollPosition = tab.ScrollPosition;

            GUILayout.BeginVertical();
            tab.OnFixedHeaderGUI();
            GUILayout.Space(-3);
            base.Draw();
            GUILayout.EndVertical();

            tab.OnSideGUI();
            tab.ScrollPosition = this.viewScrollPosition;
        }

        #endregion


        #region Help Button

        private void DrawHelpButton()
        {
            GUILayout.Space(33);

            Rect position = new Rect(Window.position.width - 37, 2, 34, 26);

            using (var helpMenuContent = ControlContent.Basic(
                RotorzEditorStyles.Skin.ContextHelp,
                TileLang.ParticularText("Action", "Help")
            )) {
                if (EditorInternalUtility.DropdownMenu(position, helpMenuContent, RotorzEditorStyles.Instance.FlatButton)) {
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

        #endregion


        #region Actions

        private void OnRename(string newName)
        {
            try {
                this.inputTilesetName = BrushDatabase.Instance.RenameTileset(this.Tileset, this.inputTilesetName);

                // Defocus name input field.
                GUIUtility.keyboardControl = 0;

                ToolUtility.RepaintBrushPalette();
            }
            catch (ArgumentException ex) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", " Was unable to rename tileset"),
                    ex.Message,
                    TileLang.ParticularText("Action", "OK")
                );
            }
        }

        #endregion


        #region Methods

        /// <inheritdoc/>
        public override void SetDirty()
        {
            // We are interested in whether Unity has destroyed tileset...
            if (this.Tileset == null) {
                return;
            }

            EditorUtility.SetDirty(this.Tileset);
        }

        #endregion
    }
}
