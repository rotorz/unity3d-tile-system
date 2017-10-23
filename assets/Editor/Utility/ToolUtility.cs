// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility functions that are useful when implementing custom tools.
    /// </summary>
    [InitializeOnLoad]
    public static class ToolUtility
    {
        static ToolUtility()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            var group = AssetSettingManagement.GetGroup("ToolUtility");
            PrepareSettings(group);
            group.Seal();
        }


        #region Settings

        private static void PrepareSettings(ISettingStore store)
        {
            s_SharedBrushListModel = store.Fetch<BrushListModel>("SharedBrushListModel", null,
                filter: (value) => {
                    if (value == null) {
                        value = new BrushListModel();
                    }
                    value.HideAliasBrushes = false;
                    return value;
                }
            );

            s_setting_ActiveTileSystemInstanceID = store.Fetch<int>("ActiveTileSystemInstanceID", 0);

            s_setting_Rotation = store.Fetch<int>("Rotation", 0,
                filter: (value) => Mathf.Clamp(value, 0, 3)
            );
            s_setting_RandomizeVariations = store.Fetch<bool>("RandomizeVariations", true);
            s_setting_RandomizeVariations.ValueChanged += (args) => {
                if (ToolManager.Instance.CurrentTool != null) {
                    SceneView.RepaintAll();
                }
            };

            s_setting_BrushNozzle = store.Fetch<BrushNozzle>("BrushNozzle", BrushNozzle.Round);

            s_setting_FillCenter = store.Fetch<bool>("FillCenter", true);
            s_setting_PaintAroundExistingTiles = store.Fetch<bool>("PaintAroundExistingTiles", false);
        }

        private static Setting<BrushListModel> s_SharedBrushListModel;

        private static Setting<int> s_setting_ActiveTileSystemInstanceID;

        private static Setting<int> s_setting_Rotation;
        private static Setting<bool> s_setting_RandomizeVariations;

        private static Setting<BrushNozzle> s_setting_BrushNozzle;

        private static Setting<bool> s_setting_FillCenter;
        private static Setting<bool> s_setting_PaintAroundExistingTiles;


        #endregion

        /// <summary>
        /// Gets brush list model which is used by the brushes palette and shared with other
        /// user interfaces such as the designer window.
        /// </summary>
        /// <remarks>
        /// <para>This model allows multiple interfaces to communicate with the brushes
        /// palette and can be used regardless of whether the brushes palette is actually
        /// shown.</para>
        /// </remarks>
        /// <example>
        /// <para>Filter brushes in palette by those whose name contains the string "platform":</para>
        /// <code language="csharp"><![CDATA[
        /// ToolUtility.SharedBrushListModel.SearchFilterText = "platform";
        /// ToolUtility.RepaintBrushPalette();
        /// ]]></code>
        /// </example>
        public static BrushListModel SharedBrushListModel {
            get { return s_SharedBrushListModel.Value; }
        }


        #region Events

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            // Ensure that tool is deactivated when switching between play/edit mode.
            ToolManager.Instance.CurrentTool = null;

            // Assembly will get reloaded real soon, so let's save user settings
            // just in case Unity explodes.
            AssetSettingManagement.SaveSettings();

            // Attempt to recover previously active tile system editor preferences.
            int instanceID = s_setting_ActiveTileSystemInstanceID.Value;
            ToolUtility.ActiveTileSystem = (instanceID != -1)
                ? EditorUtility.InstanceIDToObject(instanceID) as TileSystem
                : null;

            RepaintScenePalette();
        }

        private static void OnUndoRedoPerformed()
        {
            if (ToolManager.Instance.CurrentTool == null || ActiveTileSystem == null) {
                // Restore visibility of selected wireframe.
                RestoreSelectedWireframe();
            }
            else {
                // Ensure that selected wireframe is hidden for active tile system.
                foreach (var renderer in ActiveTileSystem.gameObject.GetComponentsInChildren<Renderer>()) {
                    EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Hidden);
                }
            }
        }

        #endregion


        #region Tile Systems

        /// <summary>
        /// Gets list of all active and non-active tile systems in the current scene.
        /// </summary>
        /// <remarks>
        /// <para>Tile systems which have been hidden using <a href="https://docs.unity3d.com/Documentation/ScriptReference/HideFlags.html"><c>HideFlags</c></a>
        /// are excluded from returned list.</para>
        /// </remarks>
        /// <returns>
        /// Read-only collection of tile systems in scene order.
        /// </returns>
        public static IList<TileSystem> GetAllTileSystemsInScene()
        {
            return EditorTileSystemUtility.AllTileSystemsInScene;
        }

        /// <summary>
        /// Finds nearest tile system in parent hierarchy of specified object. Does not
        /// attempt to find tile system component from prefabs.
        /// </summary>
        /// <remarks>
        /// <para>Always returns a value of <c>null</c> for tile system components which
        /// reside within prefabs.</para>
        /// </remarks>
        /// <param name="transform">Transform component of input object.</param>
        /// <returns>
        /// The nearest <see cref="TileSystem"/> component; otherwise a value of <c>null</c>.
        /// </returns>
        public static TileSystem FindParentTileSystem(Transform transform)
        {
            // Does not find parent component of tile systems which are stored inside prefabs.
            if (transform == null || EditorUtility.IsPersistent(transform)) {
                return null;
            }

            TileSystem system = null;

            while (transform != null) {
                system = transform.GetComponent<TileSystem>();
                if (system != null) {
                    return system;
                }

                transform = transform.parent;
            }

            // Being extra careful since Unity overloads equality operators.
            return system != null ? system : null;
        }

        /// <summary>
        /// Gets all selected tile system components.
        /// </summary>
        /// <returns>
        /// Array of tile system components.
        /// </returns>
        public static TileSystem[] GetSelectedTileSystems()
        {
            return (TileSystem[])Selection.GetFiltered(typeof(TileSystem), SelectionMode.Unfiltered);
        }

        #endregion


        #region Tile System Selection

        private static TileSystem s_ActiveTileSystem;

        /// <summary>
        /// Gets the active tile system.
        /// </summary>
        /// <remarks>
        /// <para>Active tile system is typically highlighted within the scene palette
        /// window and is automatically selected upon activating a tool.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>null</c> when no tile system is active.
        /// </value>
        /// <seealso cref="SelectTileSystem(TileSystem)"/>
        /// <seealso cref="SelectPreviousTileSystem()"/>
        /// <seealso cref="SelectNextTileSystem()"/>
        /// <seealso cref="SelectNthTileSystem(int)"/>
        /// <seealso cref="SelectActiveOrParentTileSystem()"/>
        public static TileSystem ActiveTileSystem {
            get { return s_ActiveTileSystem; }
            internal set {
                if (!InternalUtility.AreSameUnityObjects(value, s_ActiveTileSystem)) {
                    s_ActiveTileSystem = value;
                    s_setting_ActiveTileSystemInstanceID.Value = (value != null ? value.GetInstanceID() : -1);
                }
            }
        }

        /// <summary>
        /// Select and activate tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <seealso cref="SelectPreviousTileSystem()"/>
        /// <seealso cref="SelectNextTileSystem()"/>
        /// <seealso cref="SelectNthTileSystem(int)"/>
        /// <seealso cref="SelectActiveOrParentTileSystem()"/>
        /// <seealso cref="ActiveTileSystem"/>
        public static void SelectTileSystem(TileSystem system)
        {
            // No tile system is to be selected!
            if (system == null) {
                // If the active tile system is selected, deselect it!
                if (ActiveTileSystem != null) {
                    if (Selection.activeGameObject == ActiveTileSystem.gameObject) {
                        Selection.objects = new Object[0];
                    }
                    ActiveTileSystem = null;
                }
                return;
            }

            ActiveTileSystem = system;

            // Automatically adjust user selection if needed.
            if (Selection.activeGameObject != system.gameObject || Selection.objects.Length != 1) {
                Selection.objects = new Object[] { system.gameObject };
                Selection.activeGameObject = system.gameObject;

                RevealTileSystem(system, false);
                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Select and activate nth visible tile system.
        /// </summary>
        /// <remarks>
        /// <para>Ordering of tile systems can be customized using drag and drop to
        /// reorder them using the scene palette window (see <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Scene-Palette#user-content-reordering-tile-systems">Reordering Tile Systems</a>).</para>
        /// </remarks>
        /// <param name="n">Zero-based index of tile system in scene order.</param>
        /// <seealso cref="SelectPreviousTileSystem()"/>
        /// <seealso cref="SelectNextTileSystem()"/>
        /// <seealso cref="SelectTileSystem(TileSystem)"/>
        /// <seealso cref="SelectActiveOrParentTileSystem()"/>
        /// <seealso cref="ActiveTileSystem"/>
        public static void SelectNthTileSystem(int n)
        {
            var tileSystems = GetAllTileSystemsInScene();
            if (n < 0 || n >= tileSystems.Count) {
                return;
            }
            if (tileSystems[n] == null || !tileSystems[n].gameObject.activeInHierarchy) {
                return;
            }

            SelectTileSystem(tileSystems[n]);
        }

        /// <summary>
        /// Select and activate previous tile system in scene.
        /// </summary>
        /// <remarks>
        /// <para>Ordering of tile systems can be customized using drag and drop to
        /// reorder them using the scene palette window (see <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Scene-Palette#user-content-reordering-tile-systems">Reordering Tile Systems</a>).</para>
        /// </remarks>
        /// <seealso cref="SelectNextTileSystem()"/>
        /// <seealso cref="SelectNthTileSystem(int)"/>
        /// <seealso cref="SelectTileSystem(TileSystem)"/>
        /// <seealso cref="SelectActiveOrParentTileSystem()"/>
        /// <seealso cref="ActiveTileSystem"/>
        public static void SelectPreviousTileSystem()
        {
            var tileSystems = GetAllTileSystemsInScene();
            if (tileSystems.Count < 2) {
                return;
            }

            int index = tileSystems.IndexOf(ActiveTileSystem);
            if (index == -1) {
                return;
            }

            int previousIndex = index;
            while (true) {
                if (--previousIndex < 0) {
                    previousIndex = tileSystems.Count - 1;
                }
                if (previousIndex == index) {
                    break;
                }

                if (tileSystems[previousIndex].gameObject.activeInHierarchy) {
                    SelectTileSystem(tileSystems[previousIndex]);
                    break;
                }
            }
        }

        /// <summary>
        /// Select and activate next tile system in scene.
        /// </summary>
        /// <remarks>
        /// <para>Ordering of tile systems can be customized using drag and drop to
        /// reorder them using the scene palette window (see <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Scene-Palette#user-content-reordering-tile-systems">Reordering Tile Systems</a>).</para>
        /// </remarks>
        /// <seealso cref="SelectPreviousTileSystem()"/>
        /// <seealso cref="SelectNthTileSystem(int)"/>
        /// <seealso cref="SelectTileSystem(TileSystem)"/>
        /// <seealso cref="SelectActiveOrParentTileSystem()"/>
        /// <seealso cref="ActiveTileSystem"/>
        public static void SelectNextTileSystem()
        {
            var tileSystems = GetAllTileSystemsInScene();
            if (tileSystems.Count < 2) {
                return;
            }

            int index = tileSystems.IndexOf(ActiveTileSystem);
            if (index == -1) {
                return;
            }

            int nextIndex = index;
            while (true) {
                if (++nextIndex >= tileSystems.Count) {
                    nextIndex = 0;
                }
                if (nextIndex == index) {
                    break;
                }

                if (tileSystems[nextIndex].gameObject.activeInHierarchy) {
                    SelectTileSystem(tileSystems[nextIndex]);
                    break;
                }
            }
        }

        /// <summary>
        /// Select and activate nearest tile system in parent hierarchy of selected object.
        /// </summary>
        /// <seealso cref="SelectTileSystem(TileSystem)"/>
        /// <seealso cref="SelectPreviousTileSystem()"/>
        /// <seealso cref="SelectNextTileSystem()"/>
        /// <seealso cref="SelectNthTileSystem(int)"/>
        /// <seealso cref="ActiveTileSystem"/>
        public static void SelectActiveOrParentTileSystem()
        {
            var tileSystem = ActiveTileSystem;

            // If no tile system is currently active, search the parent hierarchy of the
            // active game object for a tile system and select it.
            if (tileSystem == null && Selection.activeTransform != null) {
                tileSystem = ToolUtility.FindParentTileSystem(Selection.activeTransform);
            }

            if (tileSystem != null) {
                SelectTileSystem(tileSystem);
            }
        }

        #endregion


        #region Tile System Selected Wireframe

        /// <summary>
        /// Set of tile systems for which selected wireframe has been hidden.
        /// </summary>
        private static HashSet<TileSystem> s_TileSystemsWithHiddenSelectedWireframe = new HashSet<TileSystem>();

        /// <summary>
        /// Hide selection wireframe for specified tile system (if hasn't already).
        /// </summary>
        /// <param name="system">The tile system.</param>
        /// <seealso cref="RestoreSelectedWireframe()"/>
        internal static void HideSelectedWireframe(TileSystem system)
        {
            if (!s_TileSystemsWithHiddenSelectedWireframe.Contains(system)) {
                s_TileSystemsWithHiddenSelectedWireframe.Add(system);
                foreach (var renderer in system.gameObject.GetComponentsInChildren<Renderer>()) {
                    EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Hidden);
                }
            }
        }

        /// <summary>
        /// Restore selection wireframe for tile systems which were previously hidden
        /// using <see cref="HideSelectedWireframe(TileSystem)"/>.
        /// </summary>
        /// <seealso cref="HideSelectedWireframe(TileSystem)"/>
        internal static void RestoreSelectedWireframe()
        {
            // Bail early to avoid allocation of enumerator.
            if (s_TileSystemsWithHiddenSelectedWireframe.Count == 0) {
                return;
            }

            foreach (var tileSystem in s_TileSystemsWithHiddenSelectedWireframe) {
                if (tileSystem != null) {
                    foreach (var renderer in tileSystem.gameObject.GetComponentsInChildren<Renderer>()) {
                        EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Wireframe | EditorSelectedRenderState.Highlight);
                    }
                }
            }
            s_TileSystemsWithHiddenSelectedWireframe.Clear();
        }

        #endregion


        #region Shared Tool Preferences

        /// <summary>
        /// Gets or sets whether brush variations should be randomized when painted.
        /// </summary>
        public static bool RandomizeVariations {
            get { return s_setting_RandomizeVariations; }
            set { s_setting_RandomizeVariations.Value = value; }
        }

        /// <summary>
        /// Gets or sets simple rotation which should be applied to tiles or plops when painted.
        /// </summary>
        /// <value>
        /// <para>Zero-based index of simple rotation (0 to 3 inclusive):</para>
        /// <list type="bullet">
        /// <item>0 = 0°</item>
        /// <item>1 = 90°</item>
        /// <item>2 = 180°</item>
        /// <item>3 = 270°</item>
        /// </list>
        /// </value>
        public static int Rotation {
            get { return s_setting_Rotation; }
            set { s_setting_Rotation.Value = value; }
        }

        /// <summary>
        /// Gets or sets brush nozzle.
        /// </summary>
        public static BrushNozzle BrushNozzle {
            get { return s_setting_BrushNozzle; }
            set { s_setting_BrushNozzle.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether center of shapes should be filled.
        /// </summary>
        /// <remarks>
        /// <para>This applies to the rectangle tool.</para>
        /// </remarks>
        public static bool FillCenter {
            get { return s_setting_FillCenter; }
            set { s_setting_FillCenter.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether tiles should be painted around existing tiles or whether
        /// existing tiles should be replaced.
        /// </summary>
        /// <remarks>
        /// <para>This applies to tools which paint tiles including paint, line,
        /// rectangle and spray.</para>
        /// </remarks>
        public static bool PaintAroundExistingTiles {
            get { return s_setting_PaintAroundExistingTiles; }
            set { s_setting_PaintAroundExistingTiles.Value = value; }
        }


        /// <exclude/>
        public static void CheckToolKeyboardShortcuts()
        {
            if (ToolManager.Instance.CurrentTool == null) {
                return;
            }

            ToolManager.Instance.CurrentTool.OnCheckKeyboardShortcuts();

            // Proceed to check regular keyboard shortcuts.
            if (Event.current.type != EventType.KeyDown) {
                return;
            }

            switch (Event.current.keyCode) {
                // Select next/previous tile system in scene.
                case KeyCode.PageUp:
                    SelectPreviousTileSystem();
                    break;
                case KeyCode.PageDown:
                    SelectNextTileSystem();
                    break;

                // Simple tile rotation shortcut keys:
                case KeyCode.M:
                    if (Rotation != 0) {
                        Rotation = 0;
                        RepaintToolPalette();
                    }
                    break;
                case KeyCode.Comma:
                    if (--Rotation < 0)
                        Rotation = 3;
                    RepaintToolPalette();
                    break;
                case KeyCode.Period:
                    if (++Rotation > 3)
                        Rotation = 0;
                    RepaintToolPalette();
                    break;

                default:
                    return;
            }

            Event.current.Use();
        }

        #endregion


        #region Active Tile

        /// <summary>
        /// Gets or sets zero-based index of the active tile.
        /// </summary>
        /// <seealso cref="ActiveTile"/>
        public static TileIndex ActiveTileIndex { get; set; }

        /// <summary>
        /// Gets the active tile.
        /// </summary>
        /// <remarks>
        /// <para>Active tile is visualised with a red wireframe cube when editing
        /// tile system at design time.</para>
        /// </remarks>
        /// <value>
        /// The active tile, or <c>null</c> if no tile is active.
        /// </value>
        /// <seealso cref="ActiveTileIndex"/>
        public static TileData ActiveTile {
            get {
                return ActiveTileSystem != null
                    ? ActiveTileSystem.GetTileOrNull(ActiveTileIndex)
                    : null;
            }
        }

        #endregion


        #region Active Plop

        private static PlopInstance s_ActivePlop;
        private static PlopInstance s_PreviouslyPlopped;

        /// <summary>
        /// Gets or sets plop which is active for current tool.
        /// </summary>
        /// <remarks>
        /// <para>This property is reset to a value of <c>null</c> each time a different
        /// tool is selected to avoid confusion of state when switching between tools.</para>
        /// <para>The default implementation of <see cref="ToolBase.OnSceneGUI">ToolBase.OnSceneGUI</see>
        /// outlines the active plop (when set) instead of presenting the default nozzle
        /// indicator. This is generally convenient since tools which make use of this
        /// property will typically want to provide some visual feedback to the user.</para>
        /// </remarks>
        public static PlopInstance ActivePlop {
            get {
                if (s_ActivePlop == null) {
                    s_ActivePlop = null;
                }
                return s_ActivePlop;
            }
            set {
                s_ActivePlop = value != null ? value : null;
            }
        }

        /// <summary>
        /// Gets or sets reference to the previously plopped object.
        /// </summary>
        public static PlopInstance PreviouslyPlopped {
            get {
                if (s_PreviouslyPlopped == null) {
                    s_PreviouslyPlopped = null;
                }
                return s_PreviouslyPlopped;
            }
            set {
                s_PreviouslyPlopped = value != null ? value : null;
            }
        }

        #endregion


        /// <summary>
        /// Repaint all palette windows.
        /// </summary>
        public static void RepaintPaletteWindows()
        {
            RepaintToolPalette();
            RepaintBrushPalette();
            RepaintScenePalette();
        }


        #region Tool and Brush Palettes

        /// <summary>
        /// Gets or sets primary selected brush for use with tools. This brush is
        /// typically used in conjunction with the primary mouse button.
        /// </summary>
        /// <value>
        /// Brush reference or a value of <c>null</c> for erase.
        /// </value>
        public static Brush SelectedBrush {
            get { return SharedBrushListModel.SelectedBrush; }
            set { SharedBrushListModel.SelectedBrush = value; }
        }

        /// <summary>
        /// Gets or sets secondary selected brush for use with tools. This brush is
        /// typically used in conjunction with the secondary mouse button.
        /// </summary>
        /// <value>
        /// Brush reference or a value of <c>null</c> for erase.
        /// </value>
        public static Brush SelectedBrushSecondary {
            get { return SharedBrushListModel.SelectedBrushSecondary; }
            set { SharedBrushListModel.SelectedBrushSecondary = value; }
        }


        /// <summary>
        /// Reveal brush in brushes palette where possible.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="showWindow">Indicates if brush palette window should be shown
        /// if not already shown.</param>
        public static void RevealBrush(Brush brush, bool showWindow = true)
        {
            var brushPalette = RotorzWindow.GetInstance<BrushPaletteWindow>();
            if (showWindow && brushPalette == null) {
                brushPalette = RotorzWindow.GetWindow<BrushPaletteWindow>();
            }

            if (brushPalette != null) {
                brushPalette.RevealBrush(brush);
            }
        }

        /// <summary>
        /// Show and focus tool palette window.
        /// </summary>
        [MenuItem("Window/Rotorz Tile System")]
        private static void ShowToolPalette()
        {
            RotorzWindow.GetWindow<ToolPaletteWindow>();
        }

        /// <summary>
        /// Show tool palette window.
        /// </summary>
        /// <param name="focus">Indicates if window should be focused.</param>
        public static void ShowToolPalette(bool focus = true)
        {
            if (focus || RotorzWindow.GetInstance<ToolPaletteWindow>() == null) {
                RotorzWindow.GetWindow<ToolPaletteWindow>();
            }
        }

        /// <summary>
        /// Repaint tool palette window if shown.
        /// </summary>
        public static void RepaintToolPalette()
        {
            RotorzWindow.RepaintIfShown<ToolPaletteWindow>();
        }

        /// <summary>
        /// Show brush palette window.
        /// </summary>
        /// <remarks>
        /// <para>Tool palette window is shown instead when combined.</para>
        /// </remarks>
        /// <param name="focus">Indicates if window should be focused.</param>
        public static void ShowBrushPalette(bool focus = true)
        {
            if (focus || RotorzWindow.GetInstance<BrushPaletteWindow>() == null) {
                RotorzWindow.GetWindow<BrushPaletteWindow>();
            }
        }

        /// <summary>
        /// Repaint brush palette window if shown.
        /// </summary>
        public static void RepaintBrushPalette()
        {
            RotorzWindow.RepaintIfShown<BrushPaletteWindow>();
        }

        #endregion


        #region Scene Palette

        /// <summary>
        /// Reveal tile system in scene palette.
        /// </summary>
        /// <param name="system">The tile system.</param>
        /// <param name="showWindow">Indicates if scene palette window should be shown if not already.</param>
        public static void RevealTileSystem(TileSystem system, bool showWindow = true)
        {
            var palette = RotorzWindow.GetInstance<ScenePaletteWindow>();

            if (showWindow && palette == null) {
                palette = RotorzWindow.GetWindow<ScenePaletteWindow>();
            }

            if (palette != null) {
                palette.ScrollToTileSystem(system);
                palette.Repaint();
            }
        }

        /// <summary>
        /// Show scene palette window.
        /// </summary>
        /// <param name="focus">Indicates if window should be focused.</param>
        public static void ShowScenePalette(bool focus = true)
        {
            if (focus || RotorzWindow.GetInstance<ScenePaletteWindow>() == null) {
                RotorzWindow.GetWindow<ScenePaletteWindow>();
            }
        }

        /// <summary>
        /// Repaint scene palette window if shown.
        /// </summary>
        public static void RepaintScenePalette()
        {
            RotorzWindow.RepaintIfShown<ScenePaletteWindow>();
        }

        #endregion


        #region Designer Window

        /// <summary>
        /// Show brush in designer window.
        /// </summary>
        /// <param name="brush">Brush asset.</param>
        public static void ShowBrushInDesigner(Brush brush)
        {
            // Ensure that designer window is shown.
            DesignerWindow designer = DesignerWindow.ShowWindow();
            // Show brush in designer.
            designer.SelectedObject = brush;

            // Focus designer window?
            if (EditorWindow.focusedWindow != designer) {
                designer.Focus();
            }
        }

        /// <summary>
        /// Show tileset in designer window.
        /// </summary>
        /// <param name="tileset">Tileset asset.</param>
        public static void ShowTilesetInDesigner(Tileset tileset)
        {
            // Ensure that designer window is shown.
            DesignerWindow designer = DesignerWindow.ShowWindow();
            // Show tileset in designer.
            designer.SelectedObject = tileset;

            // Focus designer window?
            if (EditorWindow.focusedWindow != designer) {
                designer.Focus();
            }
        }

        #endregion
    }
}
