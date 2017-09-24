// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Brush designer window.
    /// </summary>
    public sealed class DesignerWindow : RotorzWindow, IHasCustomMenu, IHistoryManagerContext
    {
        #region Localized Content

        private static GUIContent s_content_HistoryBackButton;
        private static GUIContent s_content_HistoryForwardButton;
        private static GUIContent s_content_HistoryRecentButton;

        private static void AutoInitLocalizedContent()
        {
            if (s_content_HistoryBackButton != null) {
                return;
            }

            s_content_HistoryBackButton = new GUIContent(RotorzEditorStyles.Skin.LeftArrow, TileLang.ParticularText("Action", "Back"));
            s_content_HistoryForwardButton = new GUIContent(RotorzEditorStyles.Skin.RightArrow, TileLang.ParticularText("Action", "Forward"));
            s_content_HistoryRecentButton = new GUIContent(RotorzEditorStyles.Skin.RecentHistory, TileLang.ParticularText("Action", "Recent"));
        }

        #endregion


        #region Window Management

        /// <summary>
        /// Display the brush designer window.
        /// </summary>
        /// <returns>
        /// The window.
        /// </returns>
        public static DesignerWindow ShowWindow()
        {
            return GetWindow<DesignerWindow>();
        }

        /// <summary>
        /// Repaints the designer window.
        /// </summary>
        public static void RepaintWindow()
        {
            RepaintIfShown<DesignerWindow>();
        }

        #endregion


        /// <summary>
        /// Gets the brush list model.
        /// </summary>
        private BrushListModel BrushListModel { get; set; }


        #region Selection History

        /// <summary>
        /// Gets selection history.
        /// </summary>
        public HistoryManager History { get; private set; }

        [NonSerialized]
        private HistoryManager.State currentState;

        HistoryManager.State IHistoryManagerContext.UpdateCurrentState()
        {
            if (this.designerView == null || this.SelectedObject == null || !this.SelectedObject.Exists) {
                return null;
            }

            this.designerView.UpdateHistoryState(this.currentState);

            return this.currentState;
        }

        void IHistoryManagerContext.OnNavigateBack(HistoryManager.State state)
        {
            this.OnNavigate(state);
        }

        void IHistoryManagerContext.OnNavigateForward(HistoryManager.State state)
        {
            this.OnNavigate(state);
        }

        /// <summary>
        /// Invoked when history manager navigates back or forward.
        /// </summary>
        /// <param name="state">New current state.</param>
        private void OnNavigate(HistoryManager.State state)
        {
            this.currentState = state;
            this.SelectedObject = state.Object as IDesignableObject;
            this.designerView.RestoreHistoryState(state);
        }

        private static Setting<string> s_RecentInstanceIDs;

        /// <summary>
        /// Restore recent selection list from editor preferences.
        /// </summary>
        private void RestoreRecentSelectionList()
        {
            if (s_RecentInstanceIDs == null) {
                s_RecentInstanceIDs = AssetSettingManagement.GetGroup("Designer").Fetch<string>("RecentInstanceIDs", "");
            }

            if (s_RecentInstanceIDs == "") {
                return;
            }

            foreach (string instanceID in s_RecentInstanceIDs.Value.Split(',')) {
                try {
                    Object o = EditorUtility.InstanceIDToObject(int.Parse(instanceID, CultureInfo.InvariantCulture));
                    if (o is IDesignableObject) {
                        this.History.AddToRecent(o as IHistoryObject);
                    }
                }
                catch {
                    // Do nothing if an error occurred whilst attempting to parse instance ID.
                }
            }
        }

        /// <summary>
        /// Persist recent selection list to editor preferences.
        /// </summary>
        private void PersistRecentSelectionList()
        {
            if (this.History == null) {
                return;
            }

            s_RecentInstanceIDs.Value = string.Join(",",
                this.History.Recent
                    .Where(recent => recent is Object)
                    .Select(recent => (recent as Object).GetInstanceID().ToString())
                    .ToArray()
            );
        }

        #endregion


        #region Lock Button

        [SerializeField]
        private bool isLocked;

        /// <summary>
        /// Gets or sets whether designer selection is locked.
        /// </summary>
        /// <remarks>
        /// <para>When locked the user can freely navigate the brushes palette without
        /// adjusting the designer selection. This can be useful when dragging and dropping
        /// tileset brushes into orientations.</para>
        /// <para>Note: Lock can only be enabled when a designable object is selected.</para>
        /// </remarks>
        public bool IsLocked {
            get { return this.isLocked; }
            set {
                if (value == this.isLocked) {
                    return;
                }

                // Can only lock selection when there is a selection!
                if (value) {
                    this.isLocked = (this.SelectedObject != null && this.SelectedObject.Exists);
                }
                else {
                    this.isLocked = false;

                    // Automatically show selected brush or tileset.
                    if (BrushListModel.View == BrushListView.Tileset) {
                        TilesetBrush tilesetBrush = BrushListModel.SelectedBrush as TilesetBrush;
                        if (tilesetBrush != null && tilesetBrush.Tileset == BrushListModel.SelectedTileset) {
                            this.SelectedObject = tilesetBrush;
                        }
                        else {
                            this.SelectedObject = this.BrushListModel.SelectedTileset;
                        }
                    }
                    else {
                        this.SelectedObject = this.BrushListModel.SelectedBrush;
                    }
                }
            }
        }

        private static GUIStyle s_LockIconStyle;

        /// <summary>
        /// Display lock button at top right of window frame.
        /// </summary>
        /// <remarks>
        /// <para>This method is automatically detected and invoked by Unity.</para>
        /// </remarks>
        /// <param name="position">Position of button.</param>
        private void ShowButton(Rect position)
        {
            if (s_LockIconStyle == null) {
                s_LockIconStyle = "IN LockButton";
            }
            this.IsLocked = GUI.Toggle(position, this.IsLocked, GUIContent.none, s_LockIconStyle);
        }

        #endregion


        #region Properties

        [SerializeField]
        private int selectedObjectInstanceID;

        [NonSerialized]
        private IDesignableObject selectedObject;
        [NonSerialized]
        private DesignerView designerView;

        /// <summary>
        /// Indicates if selected object should be locked from further changes until
        /// next GUI repaint or manual intervention.
        /// </summary>
        [NonSerialized]
        private bool lockSelectedObject;


        /// <summary>
        /// Gets or sets the object that is currently being designed.
        /// </summary>
        /// <remarks>
        /// <para>Brush designer user interface is refreshed if needed.</para>
        /// </remarks>
        public IDesignableObject SelectedObject {
            get {
                var value = this.selectedObject;
                if (value != null && !value.Exists) {
                    value = this.selectedObject = null;
                }
                return value;
            }
            set {
                if (value != null && !value.Exists) {
                    value = null;
                }

                if (value == this.SelectedObject || this.lockSelectedObject) {
                    return;
                }

                // If nothing is currently selected, and new selection is same as previous,
                // simply restore the previous selection!
                if (this.SelectedObject == null && this.History.CanGoBack && value == this.History.PeekBack.Object) {
                    this.History.GoBack();
                    return;
                }

                var newSelection = value as Object;
                if (newSelection != null) {
                    this.selectedObjectInstanceID = newSelection.GetInstanceID();
                }

                this.History.Advance();

                try {
                    this.selectedObject = value;
                    this.lockSelectedObject = true;
                    this.OnSelectedObjectChanged();
                }
                finally {
                    // Allow another object to be selected.
                    this.lockSelectedObject = false;
                }
            }
        }

        #endregion


        #region Messages and Events

        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(TileLang.Text("Designer"));
            this.minSize = new Vector2(710, 250);
            this.InitialSize = new Vector2(747, 500);
            this.CenterWhenFirstShown = CenterMode.Once;

            this.wantsMouseMove = true;

            this.History = new HistoryManager(this);
            this.RestoreRecentSelectionList();

            this.BrushListModel = ToolUtility.SharedBrushListModel;
            this.BrushListModel.SelectedBrushChanged += this.BrushListModel_SelectedBrushChanged;
            this.BrushListModel.SelectedTilesetChanged += this.BrushListModel_SelectedTilesetChanged;

            // Restore previous selection.
            this.SelectedObject = EditorUtility.InstanceIDToObject(this.selectedObjectInstanceID) as IDesignableObject;

            EditorApplication.modifierKeysChanged += this.Repaint;
        }

        private void OnProjectChange()
        {
            this.History.Cleanup();
        }

        /// <inheritdoc/>
        protected override void DoDisable()
        {
            EditorApplication.modifierKeysChanged -= this.Repaint;

            this.PersistRecentSelectionList();
        }

        /// <inheritdoc/>
        protected override void DoDestroy()
        {
            this.selectedObject = null;

            // Remove event handlers to avoid memory leak.
            this.BrushListModel.SelectedBrushChanged -= this.BrushListModel_SelectedBrushChanged;
            this.BrushListModel.SelectedTilesetChanged -= this.BrushListModel_SelectedTilesetChanged;
        }

        private bool _clearFocusControl;

        private void BrushListModel_SelectedBrushChanged(Brush previous, Brush current)
        {
            if (!this.IsLocked) {
                this.SelectedObject = current;
            }
        }

        private void BrushListModel_SelectedTilesetChanged(Tileset previous, Tileset current)
        {
            var window = GetInstance<DesignerWindow>();
            if (window != null && !window.IsLocked) {
                if (current != null) {
                    window.SelectedObject = current;
                }
                else {
                    window.SelectedObject = this.BrushListModel.SelectedBrush;
                }
            }
        }

        private void OnSelectedObjectChanged()
        {
            // Finish up with previous brush editor.
            this.UnloadDesignerView();

            if (this.SelectedObject is Brush) {
                this.LoadBrushDesignerView();

                if (!this.IsLocked) {
                    // Make sure that brush is shown in brush list.
                    this.BrushListModel.SelectedBrush = this.SelectedObject as Brush;
                    ToolUtility.RevealBrush(this.BrushListModel.SelectedBrush, false);
                }
            }
            else if (this.SelectedObject is Tileset) {
                this.LoadTilesetDesignerView();

                if (!this.IsLocked) {
                    // Make sure that tileset is shown in brush list.
                    this.BrushListModel.View = BrushListView.Tileset;
                    this.BrushListModel.SelectedTileset = this.SelectedObject as Tileset;
                }
            }

            if (!this.History.IsNavigating) {
                if (this.designerView != null && this.SelectedObject != null && this.SelectedObject.Exists) {
                    this.currentState = this.designerView.CreateHistoryState();
                    this.History.AddToRecent(this.SelectedObject);
                }
            }

            // Clear active input control.
            this._clearFocusControl = true;

            this.Repaint();
        }

        #endregion


        #region GUI

        private Rect _brushDesignerPosition;

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            AutoInitLocalizedContent();

            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed") {
                // Repaint window when undo/redo occurs.
                this.Repaint();
            }

            this._brushDesignerPosition = new Rect(0, 0, this.position.width, this.position.height);

            this.DrawHistoryButtons();

            this.OnGUI_PreProcess();
            this.OnGUI_Keyboard();

            if (this.designerView != null && !this.designerView.IsValid) {
                this.UnloadDesignerView();
            }

            // Automatically unlock designer if no object is selected.
            if (this.IsLocked && this.SelectedObject == null) {
                this.IsLocked = false;
            }

            if (this.designerView != null) {
                this.OnGUI_DesignerView();
            }

            this.OnGUI_PostProcess();
        }

        private void OnGUI_PreProcess()
        {
            if (this._clearFocusControl) {
                GUIUtility.keyboardControl = 0;
                this._clearFocusControl = false;
            }
        }

        private void OnGUI_Keyboard()
        {
            if (GUIUtility.hotControl != 0) {
                return;
            }

            Event e = Event.current;
            if (e.type != EventType.KeyDown) {
                return;
            }

            switch (e.keyCode) {
                case KeyCode.F2:
                    CreateBrushWindow.ShowWindow();
                    e.Use();
                    break;

                case KeyCode.LeftArrow:
                    if (e.alt) {
                        this.History.GoBack();
                        e.Use();
                    }
                    break;
                case KeyCode.RightArrow:
                    if (e.alt) {
                        this.History.GoForward();
                        e.Use();
                    }
                    break;

                case KeyCode.E:
                    if (e.control) {
                        if (this.designerView != null && this.designerView.HasExtendedProperties) {
                            DesignerView.DisplayExtendedProperties = !DesignerView.DisplayExtendedProperties;
                            GUIUtility.keyboardControl = 0;
                            e.Use();
                        }
                    }
                    break;
            }

            // The following keyboard shortcuts do not work when window is locked.
            if (this.IsLocked) {
                return;
            }

            switch (e.keyCode) {
                case KeyCode.UpArrow:
                    if (e.control) {
                        this.SelectNextBrush(-1);
                        e.Use();
                    }
                    break;
                case KeyCode.PageUp:
                    this.SelectNextBrush(-1);
                    e.Use();
                    break;

                case KeyCode.DownArrow:
                    if (e.control) {
                        this.SelectNextBrush(+1);
                        e.Use();
                    }
                    break;
                case KeyCode.PageDown:
                    this.SelectNextBrush(+1);
                    e.Use();
                    break;

                case KeyCode.Home:
                    if (e.control) {
                        this.SelectBrushByIndex(0);
                        e.Use();
                    }
                    break;
                case KeyCode.End:
                    if (e.control) {
                        this.SelectBrushByIndex(BrushListModel.Records.Count - 1);
                        e.Use();
                    }
                    break;
            }
        }

        private void SelectNextBrush(int offset)
        {
            int index = this.BrushListModel.IndexOfRecord(this.BrushListModel.SelectedBrush);
            int correctedIndex = index + offset;

            // Prevent exceeding last brush in list.
            if (correctedIndex < 0) {
                correctedIndex = 0;

                // Display tileset when moving past start of list.
                if (this.BrushListModel.View == BrushListView.Tileset && this.BrushListModel.SelectedTileset != null) {
                    this.SelectedObject = this.BrushListModel.SelectedTileset;
                    return;
                }
            }
            else if (index == 0 && this.SelectedObject is Tileset) {
                // Do not offset selection, simply move from tileset to brush.
                correctedIndex -= offset;
            }
            if (correctedIndex >= this.BrushListModel.Records.Count) {
                return;
            }

            this.SelectBrushByIndex(correctedIndex);
        }

        private void SelectBrushByIndex(int index)
        {
            if (index < 0 && this.BrushListModel.View == BrushListView.Tileset) {
                this.SelectedObject = null;
                this.BrushListModel.ScrollPosition = 0f;
                ToolUtility.RepaintBrushPalette();
                this.Repaint();
            }
            else {
                Brush nextBrush = this.BrushListModel.Records[index].Brush;
                if (nextBrush != null) {
                    this.SelectedObject = nextBrush;
                }
            }
        }

        private void OnGUI_PostProcess()
        {
            RotorzEditorGUI.DrawHoverTip(this);
        }

        private Rect _recentHistoryButtonPosition;

        /// <summary>
        /// Draw selection history navigation buttons in fixed position of designer window.
        /// </summary>
        private void DrawHistoryButtons()
        {
            Rect backButtonPosition = new Rect(2, 4, 28, 21);
            Rect forwardButtonPosition = new Rect(backButtonPosition.xMax + 1, backButtonPosition.y, 28, 21);
            this._recentHistoryButtonPosition = new Rect(forwardButtonPosition.xMax + 1, backButtonPosition.y, 27, 21);

            EditorGUI.BeginDisabledGroup(!this.History.CanGoBack);
            if (RotorzEditorGUI.HoverButton(backButtonPosition, s_content_HistoryBackButton, RotorzEditorStyles.Instance.HistoryNavButton)) {
                this.History.GoBack();
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!this.History.CanGoForward);
            if (RotorzEditorGUI.HoverButton(forwardButtonPosition, s_content_HistoryForwardButton, RotorzEditorStyles.Instance.HistoryNavButton)) {
                this.History.GoForward();
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(this.History.Recent.Count == 0);
            if (EditorInternalUtility.DropdownMenu(this._recentHistoryButtonPosition, s_content_HistoryRecentButton, RotorzEditorStyles.Instance.HistoryNavButton)) {
                this.ShowRecentHistoryMenu();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ShowRecentHistoryMenu()
        {
            var recentHistoryMenu = new EditorMenu();

            this.History.Cleanup();

            recentHistoryMenu.AddCommand(this.SelectedObject.HistoryName)
                .Visible(this.SelectedObject != null && this.SelectedObject.Exists)
                .Checked(true)
                .Action(this.RecentHistoryMenu_Select, this.SelectedObject);

            foreach (IHistoryObject recent in this.History.Recent) {
                if (!ReferenceEquals(recent, this.SelectedObject)) {
                    recentHistoryMenu.AddCommand(recent.HistoryName)
                        .Checked(ReferenceEquals(recent, this.SelectedObject))
                        .Action(this.RecentHistoryMenu_Select, recent);
                }
            }

            recentHistoryMenu.AddSeparator();

            recentHistoryMenu.AddCommand(TileLang.ParticularText("Action", "Clear Recent History"))
                .Action(() => {
                    this.History.Clear();
                });

            this._recentHistoryButtonPosition.height -= 2;
            recentHistoryMenu.ShowAsDropdown(this._recentHistoryButtonPosition);
        }

        private void RecentHistoryMenu_Select(object target)
        {
            var obj = target as IDesignableObject;

            if (this.History.CanGoBack && ReferenceEquals(this.History.PeekBack.Object, obj)) {
                this.History.GoBack();
            }
            else if (this.History.CanGoForward && ReferenceEquals(this.History.PeekForward.Object, obj)) {
                this.History.GoForward();
            }
            else {
                this.SelectedObject = obj;
            }
        }

        private void OnGUI_DesignerView()
        {
            this.designerView.BeginView();

            this.designerView.OnFixedHeaderGUI();

            // Remove height of brush editor header GUI.
            Rect toolbarRect = GUILayoutUtility.GetLastRect();
            this._brushDesignerPosition.y += toolbarRect.yMax + 3;
            this._brushDesignerPosition.height -= toolbarRect.yMax + 3;

            // Update position of brush designer.
            this.designerView.viewPosition = this._brushDesignerPosition;

            GUILayout.BeginHorizontal();

            this.designerView.Draw();

            GUILayout.EndHorizontal();

            this.designerView.EndView();
        }

        #endregion


        #region Methods

        private void UnloadDesignerView()
        {
            if (this.designerView == null) {
                return;
            }

            RotorzEditorGUI.ClearHoverTip();

            this.designerView.OnDisable();
            this.designerView = null;

            this.History.Cleanup();
            this.SelectedObject = null;
        }

        private void LoadBrushDesignerView()
        {
            // Ensure that GUI is updated correctly.
            this._clearFocusControl = true;

            var selectedBrush = this.SelectedObject as Brush;

            // Create editor for brush!
            Type brushType = selectedBrush.GetType();
            var brushDescriptor = BrushUtility.GetDescriptor(brushType);
            if (brushDescriptor == null) {
                return;
            }

            this.designerView = brushDescriptor.CreateDesigner(selectedBrush);
            if (this.designerView != null) {
                this.designerView.Window = this;
                this.designerView.OnEnable();
            }
        }

        private void LoadTilesetDesignerView()
        {
            // Ensure that GUI is updated correctly.
            this._clearFocusControl = true;

            var tilesetDesigner = new TilesetDesigner();
            tilesetDesigner.Tileset = this.SelectedObject as Tileset;

            this.designerView = tilesetDesigner;
            this.designerView.Window = this;
            this.designerView.OnEnable();
        }

        #endregion


        #region IHasCustomMenu - Implementation

        /// <inheritdoc/>
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            var lockActionContent = new GUIContent(TileLang.ParticularText("Action", "Lock"));
            if (this.SelectedObject != null) {
                menu.AddItem(lockActionContent, this.IsLocked, () => {
                    this.IsLocked = !this.IsLocked;
                });
            }
            else {
                menu.AddDisabledItem(lockActionContent);
            }
        }

        #endregion
    }
}
