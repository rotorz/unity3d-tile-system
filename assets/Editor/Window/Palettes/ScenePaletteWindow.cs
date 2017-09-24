// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Collections;
using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Scene palette lists all editable tile systems that exist in the current scene
    /// to make it easier for users to switch between them when painting.
    /// </summary>
    internal sealed class ScenePaletteWindow : RotorzWindow
    {
        [SerializeField]
        private Rect scrollViewPosition;
        [SerializeField]
        private Vector2 scrollPosition;

        [NonSerialized]
        private List<ScenePaletteEntry> sceneEntries = new List<ScenePaletteEntry>();
        [NonSerialized]
        private ReorderableListControl systemsListControl;
        [NonSerialized]
        private ScenePaletteTileSystemsListAdaptor systemsListAdaptor;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(TileLang.Text("Scene"));
            this.minSize = new Vector2(255, 100);
            this.autoRepaintOnSceneChange = true;
        }

        private void Update()
        {
            if (EditorTileSystemUtility.s_ShouldRepaintScenePalette) {
                this.Repaint();
            }
        }

        private void OnFocus()
        {
            if (this.systemsListAdaptor != null) {
                this.systemsListAdaptor.CanBeginEditingNameOnMouseUp = false;
            }
        }

        private void OnLostFocus()
        {
            if (this.systemsListAdaptor != null && this.systemsListAdaptor.IsEditingName) {
                this.systemsListAdaptor.EndEditingName(true);
            }
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            if (Event.current.type == EventType.Layout) {
                this.sceneEntries.Clear();

                var groupedTileSystems = EditorTileSystemUtility.AllTileSystemsInScene
                    .GroupBy(x => x.gameObject.scene);

                foreach (var group in groupedTileSystems) {
                    if (EditorSceneManager.sceneCount > 1) {
                        this.sceneEntries.Add(ScenePaletteEntry.ForSceneHeader(group.Key));
                    }

                    foreach (var tileSystem in group) {
                        this.sceneEntries.Add(ScenePaletteEntry.ForTileSystem(tileSystem));
                    }
                }

                //this._sceneEntries.AddRange(EditorTileSystemUtility.AllTileSystemsInScene);//.Where(x => EditorSceneManager.GetActiveScene() == x.gameObject.scene));
                EditorTileSystemUtility.s_ShouldRepaintScenePalette = false;
            }

            this.DrawToolbar();
            this.DrawTileSystemList();

            // Do not process keyboard input during rename mode.
            if (!this.systemsListAdaptor.IsEditingName) {
                this.OnKeyboardGUI();
            }
        }

        private void AutoInitializeTileSystemsListControl()
        {
            if (this.systemsListControl != null) {
                return;
            }

            var flags = ReorderableListFlags.HideAddButton | ReorderableListFlags.HideRemoveButtons | ReorderableListFlags.DisableContextMenu;
            this.systemsListControl = new ReorderableListControl(flags);
            this.systemsListControl.ContainerStyle = new GUIStyle();
            this.systemsListControl.ContainerStyle.margin = new RectOffset();
            this.systemsListControl.ContainerStyle.padding = new RectOffset();
            this.systemsListControl.HorizontalLineColor = ExtraEditorStyles.Skin.SeparatorLightColor;
            this.systemsListControl.HorizontalLineAtEnd = true;

            this.systemsListAdaptor = new ScenePaletteTileSystemsListAdaptor(this, this.sceneEntries);
        }

        private void DrawTileSystemList()
        {
            this.AutoInitializeTileSystemsListControl();

            Rect scrollViewPosition = new Rect(0, 0, position.width, position.height);
            scrollViewPosition.yMin = GUILayoutUtility.GetLastRect().yMax;
            if (Event.current.type == EventType.Repaint) {
                this.scrollViewPosition = scrollViewPosition;
            }

            Rect listControlPosition = new Rect(0, 0, scrollViewPosition.width, this.systemsListControl.CalculateListHeight(this.systemsListAdaptor));
            Rect viewRect = listControlPosition;
            viewRect.height += 7;
            if (viewRect.height > scrollViewPosition.height) {
                viewRect.width -= GUI.skin.verticalScrollbar.fixedWidth + GUI.skin.verticalScrollbar.margin.horizontal;
            }

            bool isMouseDownEvent = Event.current.type == EventType.MouseDown;
            try {
                this.scrollPosition = GUI.BeginScrollView(scrollViewPosition, this.scrollPosition, viewRect);
                this.systemsListControl.Draw(listControlPosition, this.systemsListAdaptor);
                GUI.EndScrollView();
            }
            finally {
                if (isMouseDownEvent) {
                    this.systemsListAdaptor.CanBeginEditingNameOnMouseUp = true;
                }
            }

            // Clear selection when mouse is clicked in empty area of list.
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                if (scrollViewPosition.Contains(Event.current.mousePosition)) {
                    ToolUtility.SelectTileSystem(null);
                    Event.current.Use();
                }
            }
        }

        private void OnKeyboardGUI()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            if (Event.current.GetTypeForControl(controlID) != EventType.KeyDown) {
                return;
            }

            if (GUIUtility.keyboardControl == 0) {
                ToolManager.Instance.CheckForKeyboardShortcut();
                ToolUtility.CheckToolKeyboardShortcuts();
            }
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Create Tile System")), EditorStyles.toolbarButton)) {
                CreateTileSystemWindow.ShowWindow();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Build")), RotorzEditorStyles.Instance.ToolbarButtonPadded)) {
                BuildUtility.BuildScene();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(RotorzEditorStyles.Skin.SortAsc, EditorStyles.toolbarButton)) {
                EditorTileSystemUtility.SortTileSystemsAscending();
                this.Repaint();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(RotorzEditorStyles.Skin.SortDesc, EditorStyles.toolbarButton)) {
                EditorTileSystemUtility.SortTileSystemsDescending();
                this.Repaint();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
        }

        private void OnSelectionChange()
        {
            var selectedGameObject = Selection.activeGameObject;
            if (selectedGameObject == null) {
                return;
            }

            var selectedTileSystem = selectedGameObject.GetComponent<TileSystem>();
            if (selectedTileSystem == null) {
                return;
            }

            this.ScrollToTileSystem(selectedTileSystem);
            this.Repaint();
        }

        /// <summary>
        /// Scroll to a specific tile system in the list of tile systems.
        /// </summary>
        /// <param name="system">Tile system.</param>
        public void ScrollToTileSystem(TileSystem system)
        {
            if (system == null) {
                return;
            }
            if (this.systemsListAdaptor == null) {
                return;
            }

            // Can scrolling be avoided if item is already within view?
            Rect itemPosition = this.systemsListAdaptor.GetItemPosition(system);
            if (itemPosition == default(Rect)) {
                return;
            }

            if (itemPosition.y < this.scrollPosition.y) {
                this.scrollPosition.y = itemPosition.y - 5;
                this.Repaint();
            }
            else if (itemPosition.yMax > this.scrollPosition.y + this.scrollViewPosition.height) {
                this.scrollPosition.y = Mathf.Max(0, itemPosition.yMax - this.scrollViewPosition.height + 5);
                this.Repaint();
            }
        }
    }
}
