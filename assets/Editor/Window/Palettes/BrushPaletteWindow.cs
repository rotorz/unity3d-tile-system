// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Brush palette allows user to select the brushes that they would like to use when
    /// interacting with tile systems.
    /// </summary>
    internal sealed class BrushPaletteWindow : RotorzWindow
    {
        [NonSerialized]
        private BrushListControl brushList;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.wantsMouseMove = true;

            this.titleContent = new GUIContent(TileLang.Text("Brushes"));
            this.minSize = new Vector2(255, 200);

            // Set up brush list.
            this.brushList = new BrushListControl(this);
            this.brushList.Model = ToolUtility.SharedBrushListModel;
            this.brushList.CanShowHidden = true;
            this.brushList.EnableDragAndDrop = true;
            this.brushList.DragThreshold = 16;
            this.brushList.EmptyLabel = TileLang.ParticularText("Action", "(Erase)");
            this.brushList.VisibleViews = BrushListView.Brushes | BrushListView.Tileset;
            this.brushList.ShowTilesetContextMenu = true;
            this.brushList.BrushMouseDown += this._brushList_BrushMouseDown;
            this.brushList.BrushClicked += this._brushList_BrushClicked;
            this.brushList.BrushContextMenu += this._brushList_BrushContextMenu;

            this.brushList.Model.ViewChanged += this.Model_ViewChanged;
            this.brushList.Model.SelectedBrushChanged += this.Model_SelectedBrushChanged;
            this.brushList.Model.SelectedTilesetChanged += this.Model_SelectedTilesetChanged;
        }

        /// <inheritdoc/>
        protected override void DoDestroy()
        {
            // Remove event handlers to avoid memory leak.
            this.brushList.Model.ViewChanged -= this.Model_ViewChanged;
            this.brushList.Model.SelectedBrushChanged -= this.Model_SelectedBrushChanged;
            this.brushList.Model.SelectedTilesetChanged -= this.Model_SelectedTilesetChanged;

            this.brushList.BrushMouseDown -= this._brushList_BrushMouseDown;
            this.brushList.BrushClicked -= this._brushList_BrushClicked;
            this.brushList.BrushContextMenu -= this._brushList_BrushContextMenu;
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            this.DrawBrushesGUI();

            RotorzEditorGUI.DrawHoverTip(this);
        }

        private void Model_ViewChanged(BrushListView previous, BrushListView current)
        {
            this.Repaint();
        }

        private void Model_SelectedBrushChanged(Brush previous, Brush current)
        {
            this.Repaint();
        }

        private void Model_SelectedTilesetChanged(Tileset previous, Tileset current)
        {
            this.Repaint();
        }

        private void _brushList_BrushMouseDown(Brush brush)
        {
            // Double-click to ensure that designer window is shown.
            if (Event.current.clickCount == 2 && brush != null) {
                ToolUtility.ShowBrushInDesigner(brush);

                GUIUtility.hotControl = 0;
                GUIUtility.ExitGUI();
                return;
            }

            // If right mouse button was depressed, without holding control key, select
            // as primary brush ready for context menu.
            if (Event.current.button == 1 && !Event.current.control) {
                ToolUtility.SelectedBrush = brush;
            }
        }

        private void _brushList_BrushClicked(Brush brush)
        {
            // Only proceed if either the left mouse button or right mouse button was used.
            if (Event.current.button != 0 && Event.current.button != 1) {
                return;
            }

            Brush previousSelectedBrush = null;

            // Left click to select brush, or holding control to select secondary brush.
            if (Event.current.button == 0) {
                previousSelectedBrush = ToolUtility.SelectedBrush;
                if (brush != previousSelectedBrush) {
                    ToolUtility.SelectedBrush = brush;
                    this.brushList.ScrollToBrush(brush);
                }
                else {
                    // Brush is already selected, open in designer?
                    var designerWindow = RotorzWindow.GetInstance<DesignerWindow>();
                    if (designerWindow != null && !designerWindow.IsLocked) {
                        designerWindow.SelectedObject = brush;
                    }
                }

                Event.current.Use();
            }
            else if (Event.current.control) {
                previousSelectedBrush = ToolUtility.SelectedBrushSecondary;
                ToolUtility.SelectedBrushSecondary = brush;

                Event.current.Use();
            }

            // If control was held, toggle paint tool.
            if (Event.current.control) {
                if (ToolManager.Instance.CurrentTool == null) {
                    ToolManager.Instance.CurrentTool = ToolManager.DefaultPaintTool;
                }
                else if (Event.current.button == 0 && previousSelectedBrush == brush) {
                    // Deselect paint tool if control was held whilst clicking selected brush.
                    ToolManager.Instance.CurrentTool = null;
                }
            }

            // If painting tool is active then repaint active scene view so that gizmos,
            // handles and other annotations are properly updated.
            if (ToolManager.Instance.CurrentTool != null && SceneView.lastActiveSceneView != null) {
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        private void _brushList_BrushContextMenu(Brush brush)
        {
            // Do not attempt to display context menu for "(Erase)" item.
            if (brush == null) {
                return;
            }

            var brushRecord = BrushDatabase.Instance.FindRecord(brush);

            var brushContextMenu = new EditorMenu();

            brushContextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Show in Designer")))
                .Enabled(!brushRecord.IsMaster) // Cannot edit a master brush :)
                .Action(() => {
                    ToolUtility.ShowBrushInDesigner(brush);
                });

            var selectedTilesetBrush = brush as TilesetBrush;
            if (selectedTilesetBrush != null) {
                brushContextMenu.AddCommand(TileLang.ParticularText("Action", "Goto Tileset"))
                    .Action(() => {
                        this.brushList.Model.View = BrushListView.Tileset;
                        this.brushList.Model.SelectedTileset = selectedTilesetBrush.Tileset;

                        var designerWindow = RotorzWindow.GetInstance<DesignerWindow>();
                        if (designerWindow != null && !designerWindow.IsLocked) {
                            designerWindow.SelectedObject = selectedTilesetBrush.Tileset;
                        }

                        this.Repaint();
                    });
            }

            brushContextMenu.AddCommand(TileLang.ParticularText("Action", "Reveal Asset"))
                .Action(() => {
                    EditorGUIUtility.PingObject(brush);
                });

            brushContextMenu.AddSeparator();

            brushContextMenu.AddCommand(TileLang.ParticularText("Action", "Refresh Preview"))
                .Action(() => {
                    BrushUtility.RefreshPreviewIncludingDependencies(brush);
                });

            brushContextMenu.AddSeparator();

            brushContextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Create Duplicate")))
                .Action(() => {
                    var window = CreateBrushWindow.ShowWindow<DuplicateBrushCreator>();
                    window.SharedProperties["targetBrush"] = brush;
                });

            var brushDescriptor = BrushUtility.GetDescriptor(brush.GetType());
            brushContextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Create Alias")))
                .Enabled(brushDescriptor.SupportsAliases)
                .Action(() => {
                    var window = CreateBrushWindow.ShowWindow<AliasBrushCreator>();
                    window.SharedProperties["targetBrush"] = brush;
                });

            brushContextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Delete Brush")))
                .Action(() => {
                    TileSystemCommands.DeleteBrush(brush, ToolUtility.SharedBrushListModel.View == BrushListView.Tileset);
                });

            brushContextMenu.ShowAsContext();
        }

        internal void DrawBrushesGUI()
        {
            this.DrawToolbar();

            if (GUIUtility.keyboardControl == 0) {
                ToolManager.Instance.CheckForKeyboardShortcut();
                ToolUtility.CheckToolKeyboardShortcuts();
            }

            GUILayout.Space(-1);

            // Is selected brush (primary or secondary) about to be changed?
            this.brushList.Draw(false);

            this.DrawPrimarySecondaryBrushSwitcher();

            GUILayout.Space(5);
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Create")), EditorStyles.toolbarButton, RotorzEditorStyles.ContractWidth)) {
                CreateBrushWindow.ShowWindow().Focus();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(5);

            this.brushList.DrawToolbarButtons();

            GUILayout.EndHorizontal();
        }

        private void DrawPrimarySecondaryBrushSwitcher()
        {
            Rect r;

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(RotorzEditorStyles.Skin.MouseLeft);
            GUILayout.Box(GUIContent.none, RotorzEditorStyles.Instance.SelectedBrushPreviewBox);

            r = GUILayoutUtility.GetLastRect();
            if (ToolUtility.SelectedBrush != null) {
                RotorzEditorGUI.DrawBrushPreview(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), ToolUtility.SelectedBrush);
            }
            else {
                GUI.Label(r, TileLang.ParticularText("Action", "Erase"), RotorzEditorStyles.Instance.MiniCenteredLabel);
            }

            GUILayout.Space(10);

            GUILayout.Label(RotorzEditorStyles.Skin.MouseRight);
            GUILayout.Box(GUIContent.none, RotorzEditorStyles.Instance.SelectedBrushPreviewBox);
            r = GUILayoutUtility.GetLastRect();
            if (ToolUtility.SelectedBrushSecondary != null) {
                RotorzEditorGUI.DrawBrushPreview(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), ToolUtility.SelectedBrushSecondary);
            }
            else {
                GUI.Label(r, TileLang.ParticularText("Action", "Erase"), RotorzEditorStyles.Instance.MiniCenteredLabel);
            }

            GUILayout.Space(5);

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.SwitchPrimarySecondary,
                TileLang.ParticularText("Action", "Switch primary and secondary brushes (X)")
            )) {
                if (RotorzEditorGUI.HoverButton(content, GUILayout.Height(42))) {
                    Brush t = ToolUtility.SelectedBrush;
                    ToolUtility.SelectedBrush = ToolUtility.SelectedBrushSecondary;
                    ToolUtility.SelectedBrushSecondary = t;

                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        internal void RevealBrush(Brush brush)
        {
            this.brushList.RevealBrush(brush);
        }
    }
}
