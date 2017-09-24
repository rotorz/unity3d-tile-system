// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Represents the method that will handle brush events.
    /// </summary>
    /// <param name="brush">The brush in question.</param>
    public delegate void BrushEventHandler(Brush brush);

    /// <summary>
    /// Utility class for rendering brush list.
    /// </summary>
    /// <example>
    /// <para>The following example demonstrates how to implement an editor window allowing
    /// the user to set the selected brush:</para>
    /// <code language="csharp"><![CDATA[
    /// using Rotorz.Tile;
    /// using Rotorz.Tile.Editor;
    /// using UnityEditor;
    /// using UnityEngine;
    ///
    /// public class BrushListExampleWindow : EditorWindow
    /// {
    ///     [MenuItem("Window/Brush List Example")]
    ///     public static void ShowWindow()
    ///     {
    ///         GetWindow<BrushListExampleWindow>("Brush List Example Window");
    ///     }
    ///
    ///
    ///     private BrushListControl BrushList { get; set; }
    ///
    ///
    ///     private void OnEnable()
    ///     {
    ///         // Create brush list control for this window.
    ///         this.BrushList = new BrushListControl(this);
    ///         this.BrushList.EnableDragAndDrop = true;
    ///         this.BrushList.DragThreshold = 16;
    ///         this.BrushList.BrushClicked += this.BrushList_BrushClicked;
    ///
    ///         // Initialize model for brush list.
    ///         this.BrushList.Model = new BrushListModel();
    ///     }
    ///
    ///     private void BrushList_BrushClicked(Brush brush)
    ///     {
    ///         // Set selected brush ready for painting.
    ///         ToolUtility.SelectedBrush = brush;
    ///         // We have successfully handled this event.
    ///         Event.current.Use();
    ///     }
    ///
    ///     private void OnGUI()
    ///     {
    ///         // Display brush list using layout engine.
    ///         this.BrushList.Draw(true);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public sealed class BrushListControl
    {
        #region Zoom Modes

        private static readonly BrushListZoomMode[] ZoomModeItems = new BrushListZoomMode[] {
            BrushListZoomMode.Automatic,
            BrushListZoomMode.BestFit,
            BrushListZoomMode.Custom,
        };

        private static GUIContent[] ZoomModeLabels = {
            ControlContent.Basic(TileLang.ParticularText("Zoom Mode", "Automatic")),
            ControlContent.Basic(TileLang.ParticularText("Zoom Mode", "Best Fit")),
            ControlContent.Basic(TileLang.ParticularText("Zoom Mode", "Custom")),
        };

        #endregion


        /// <summary>
        /// Raised whenever mouse button is pressed whilst mouse pointer is overlapping
        /// brush button in list.
        /// </summary>
        /// <remarks>
        /// <para>The Unity <a href="http://docs.unity3d.com/Documentation/ScriptReference/Event.html">Event</a>
        /// class can be used to discover additional information about event.</para>
        /// <para><b>Do not</b> invoke <c>Event.current.Use()</c> from within your <c>BrushMouseDown</c>
        /// event handler since this will have already been invoked automatically.</para>
        /// </remarks>
        /// <seealso cref="BrushContextMenu"/>
        /// <seealso cref="BrushClicked"/>
        public event BrushEventHandler BrushMouseDown;

        /// <summary>
        /// Raised when brush button is clicked whilst mouse pointer is overlapping brush
        /// button in list.
        /// </summary>
        /// <remarks>
        /// <para>The Unity <a href="http://docs.unity3d.com/Documentation/ScriptReference/Event.html">Event</a>
        /// class can be used to discover additional information about event.</para>
        /// <para>You <b>should</b> invoke <c>Event.current.Use()</c> from within your <c>BrushClicked</c>
        /// event handler to override the default functionality. For example, invoke <c>Event.current.Use()</c>
        /// to prevent context menu from being shown upon right-clicking if desired.</para>
        /// </remarks>
        /// <example>
        /// <para>The following demonstrates how to adjust the currently selected brush upon
        /// clicking on a brush:</para>
        /// <code language="csharp"><![CDATA[
        /// brushList.BrushClicked += delegate(Brush brush)
        /// {
        ///     // Left click to select brush.
        ///     if (Event.current.button == 0) {
        ///         ToolUtility.SelectedBrush = brush;
        ///         brushList.ScrollToBrush(brush);
        ///     }
        /// };
        /// ]]></code>
        /// </example>
        /// <seealso cref="BrushMouseDown"/>
        /// <seealso cref="BrushContextMenu"/>
        public event BrushEventHandler BrushClicked;

        /// <summary>
        /// Raised to display context menu for a brush.
        /// </summary>
        /// <remarks>
        /// <para>Context menu can be constructed using the <a href="http://docs.unity3d.com/Documentation/ScriptReference/EditorMenu.html">EditorMenu</a>
        /// class provided by the Unity API.</para>
        /// <para><b>Do not</b> invoke <c>Event.current.Use()</c> from within your <c>BrushContextMenu</c>
        /// event handler since this will be called automatically after context menu is shown.</para>
        /// </remarks>
        /// <example>
        /// <para>A custom context menu can be implemented as demonstrated in the following
        /// source:</para>
        /// <code language="csharp"><![CDATA[
        /// brushList.BrushContextMenu += delegate(Brush brush)
        /// {
        ///     // Do not attempt to display context menu for blank item.
        ///     if (brush == null) {
        ///         return;
        ///     }
        ///
        ///     var menu = new EditorMenu();
        ///
        ///     menu.AddCommand("Foo")
        ///         .Action(() => {
        ///             Debug.Log("Do something!");
        ///         });
        ///
        ///     menu.ShowAsContext();
        /// };
        /// ]]></code>
        ///
        /// <para>It is useful to adjust brush selection upon pressing right mouse button
        /// down (before releasing mouse button to display context menu) so that brush is
        /// clearly highlighted when context menu is shown. This can be achieved using the
        /// <see cref="BrushMouseDown">BrushMouseDown</see> event like demonstrated below:</para>
        /// <code language="csharp"><![CDATA[
        /// brushList.BrushMouseDown += delegate(Brush brush)
        /// {
        ///     // Adjust brush selection upon pressing right mouse button on brush.
        ///     if (Event.current.button == 1) {
        ///         ToolUtility.SelectedBrush = brush;
        ///     }
        /// };
        /// ]]></code>
        /// </example>
        /// <seealso cref="BrushMouseDown"/>
        /// <seealso cref="BrushClicked"/>
        public event BrushEventHandler BrushContextMenu;

        /// <summary>
        /// Gets window that list appears on.
        /// </summary>
        public EditorWindow Window { get; private set; }

        /// <summary>
        /// Gets or sets model of brush list control.
        /// </summary>
        public BrushListModel Model { get; set; }

        /// <summary>
        /// Indicates whether hidden brushes can be shown.
        /// </summary>
        public bool CanShowHidden;

        /// <summary>
        /// Label used for <c>null</c> brush.
        /// </summary>
        /// <remarks>
        /// <para>Specify <c>null</c> to exclude extra button.</para>
        /// </remarks>
        public string EmptyLabel;

        /// <summary>
        /// Indicates if drag and drop is enabled for brushes.
        /// </summary>
        public bool EnableDragAndDrop = false;
        /// <summary>
        /// The number of pixels of mouse movement before dragging begins.
        /// </summary>
        /// <remarks>
        /// <para>Setting this to <c>0</c> means that dragging happens immediately, however
        /// selecting brushes is harder. A higher value makes it easier to select brushes.</para>
        /// </remarks>
        public float DragThreshold = 7f;

        /// <summary>
        /// Gets or sets whether view tabs are shown.
        /// </summary>
        /// <remarks>
        /// <para>When <c>true</c> a separate tab is presented for each visible view;
        /// otherwise views are shown in popup menu alongside tilesets.</para>
        /// </remarks>
        public bool ShowViewTabs { get; set; }
        /// <summary>
        /// Gets or sets whether tileset context menu is available.
        /// </summary>
        public bool ShowTilesetContextMenu { get; set; }


        /// <summary>
        /// Previous scroll position so that change can be detected.
        /// </summary>
        private float lastScrollPosition = 0f;


        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="Rotorz.Tile.Editor.BrushListControl"/> class.
        /// </summary>
        /// <param name="window">Window that list belongs to.</param>
        public BrushListControl(EditorWindow window)
        {
            this.Window = window;

            // Mouse move is required for tooltip to work.
            window.wantsMouseMove = true;
        }

        #endregion


        #region Visible Views

        private BrushListView visibleViews;

        private BrushListView[] viewTabs;
        private string[] viewTabLabels;

        /// <summary>
        /// Gets or sets views that are visible.
        /// </summary>
        public BrushListView VisibleViews {
            get { return this.visibleViews; }
            set {
                if (value == this.visibleViews) {
                    return;
                }

                // Count the number of flags in value.
                int count = 0, mask = (int)value;
                while (mask != 0) {
                    if ((mask & 0x01) == 1) {
                        ++count;
                    }
                    mask >>= 1;
                }

                this.viewTabs = new BrushListView[count];
                this.viewTabLabels = new string[count];

                int i = 0;

                if ((value & BrushListView.Brushes) == BrushListView.Brushes) {
                    this.viewTabs[i] = BrushListView.Brushes;
                    this.viewTabLabels[i++] = TileLang.ParticularText("BrushList|View", "Brushes");
                }
                if ((value & BrushListView.Tileset) == BrushListView.Tileset) {
                    this.viewTabs[i] = BrushListView.Tileset;
                    this.viewTabLabels[i++] = TileLang.ParticularText("BrushList|View", "Tileset");
                }
                if ((value & BrushListView.Master) == BrushListView.Master) {
                    this.viewTabs[i] = BrushListView.Master;
                    this.viewTabLabels[i++] = TileLang.ParticularText("BrushList|View", "Master");
                }

                this.visibleViews = value;
            }
        }

        #endregion


        #region Methods

        internal bool doScrollToBrush;
        internal Brush scrollToBrush;

        /// <summary>
        /// Reveal a brush by switching view, altering filter options and
        /// scrolling to brush as needed.
        /// </summary>
        /// <param name="brush">The brush.</param>
        public void RevealBrush(Brush brush)
        {
            if (!this.SwitchViewForBrush(brush)) {
                return;
            }
            this.AdjustFilteringForBrush(brush);
            this.ScrollToBrush(brush);

            if (this.Window != null) {
                this.Window.Repaint();
            }
        }

        /// <summary>
        /// Scroll so that specified brush is visible.
        /// </summary>
        /// <remarks>
        /// <para>This can only happen when brush is actually listed. For example, nothing
        /// will happen if brush is excluded from current list due to a filter.</para>
        /// </remarks>
        /// <param name="brush">The brush.</param>
        public void ScrollToBrush(Brush brush)
        {
            this.doScrollToBrush = true;
            this.scrollToBrush = brush;

            if (this.Window != null) {
                this.Window.Repaint();
            }
        }

        /// <summary>
        /// If necessary switch to view where specified brush could be seen.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// A value of <c>true</c> if brush can be seen from a view; otherwise a value
        /// of <c>false</c> if brush was invalid or cannot be seen from available views.
        /// </returns>
        public bool SwitchViewForBrush(Brush brush)
        {
            var brushRecord = BrushDatabase.Instance.FindRecord(brush);
            if (brushRecord == null) {
                return false;
            }

            // Automatically select tileset and tileset view?
            if (brush is TilesetBrush) {
                // Do not switch to tileset view if brushes view is currently selected.
                // Reason: This could cause frustration!
                // However, do change view when tileset brush is excluded from current view!
                if (this.Model.View != BrushListView.Brushes || !this.Model.Records.Contains(brushRecord)) {
                    this.Model.View = BrushListView.Tileset;
                }

                this.Model.SelectedTileset = (brush as TilesetBrush).Tileset;
            }
            else if (this.Model.View == BrushListView.Tileset) {
                this.Model.View = BrushListView.Brushes;
            }

            if (brushRecord.IsMaster) {
                // If "Master" view is visible, select it!
                if ((this.VisibleViews & BrushListView.Master) == BrushListView.Master) {
                    this.Model.View = BrushListView.Master;
                }
                else {
                    // Master view is not available, master brushes cannot be selected!
                    return false;
                }
            }
            else if (this.Model.View == BrushListView.Master) {
                this.Model.View = BrushListView.Brushes;
            }

            if (this.Window != null) {
                this.Window.Repaint();
            }

            return true;
        }

        /// <summary>
        /// If necessary adjust filtering so that the specified brush can be seen.
        /// </summary>
        /// <param name="brush">The brush.</param>
        public void AdjustFilteringForBrush(Brush brush)
        {
            var brushRecord = BrushDatabase.Instance.FindRecord(brush);
            if (brushRecord == null) {
                return;
            }

            // Clear search filter text when brush is excluded.
            if (this.Model.SearchFilterText != "" && brushRecord.DisplayName.IndexOf(this.Model.SearchFilterText, StringComparison.OrdinalIgnoreCase) == -1) {
                this.Model.SearchFilterText = string.Empty;
            }

            // Remove category filtering?
            if (!this.Model.ApplyCategoryFilter(brush)) {
                this.Model.CategoryFiltering = CategoryFiltering.None;
            }

            if (this.Window != null) {
                this.Window.Repaint();
            }
        }

        #endregion


        #region Drag and Drop

        // Indicates if drag operation is permitted.
        private bool permitDragOperation;
        // Anchor of drag operation.
        private Vector2 dragAnchor;


        private void CheckForDragBrush(Event e, BrushAssetRecord record)
        {
            if (e.type == EventType.MouseDown) {
                this.permitDragOperation = true;
                this.dragAnchor = e.mousePosition;
            }
            else if (e.type == EventType.MouseDrag && this.permitDragOperation && Vector2.Distance(this.dragAnchor, e.mousePosition) > this.DragThreshold) {
                this.permitDragOperation = false;
                GUIUtility.hotControl = 0;

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { record.Brush };
                DragAndDrop.paths = new string[0];

                DragAndDrop.StartDrag(TileLang.FormatDragObjectTitle(record.DisplayName, TileLang.Text("Brush")));

                e.Use();
            }
        }

        #endregion


        #region GUI

        private bool isFirstTime = true;

        /// <summary>
        /// Draw brush list and process its GUI events.
        /// </summary>
        /// <param name="position">Position of brush list.</param>
        public void Draw(Rect position)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            EventType eventType = Event.current.GetTypeForControl(controlID);

            position.x += 2;
            position.width -= 4;

            this.DrawCategoryAndSearchField(ref position);

            if (this.ShowViewTabs && this.Model.View == BrushListView.Tileset) {
                ExtraEditorGUI.SeparatorLight(new Rect(position.x, position.y + 1, position.width, 1));
                position.y += 6;
                position.height -= 6;
                this.DrawTilesetFilter(ref position);
            }

            --position.x;
            position.width += 4;
            --position.height;

            // Draw background behind tile area.
            if (Event.current.type == EventType.Repaint) {
                RotorzEditorStyles.Instance.ListViewBox.Draw(position, GUIContent.none, false, false, false, false);
            }

            if (this.Model.CategoryFiltering != CategoryFiltering.None || this.Model.FilterFavorite) {
                this.DrawFilterCaption(ref position, this.Model.SelectedBrush);
            }

            if (Event.current.type == EventType.Repaint) {
                ++position.x;
                ++position.y;
                position.width -= 2;
                position.height -= 2;

                this.Position = position;
            }
            else {
                // Use previous position if current one is invalid.
                position = this.Position;
            }

            // Note: Do not scroll to brush if this is the first time because the size
            //       of the list area has not been calculated!

            switch (this.Model.Presentation) {
                default:
                case BrushListPresentation.List:
                    this.CalculateListMetrics(position);
                    if (!this.isFirstTime && this.doScrollToBrush) {
                        this.DoScrollToBrush();
                    }
                    this.DrawList(Event.current, position, this.Model.SelectedBrush);
                    break;

                case BrushListPresentation.Icons:
                    this.CalculateIconsMetrics(position);
                    if (!this.isFirstTime && this.doScrollToBrush) {
                        this.DoScrollToBrush();
                    }
                    this.DrawIcons(Event.current, position, this.Model.SelectedBrush);
                    break;
            }

            this.DrawLowerToolbar();

            // Has scroll position changed?
            if (this.lastScrollPosition != this.Model.ScrollPosition) {
                // Hide hover tip!
                RotorzEditorGUI.ClearHoverTip();
                this.lastScrollPosition = this.Model.ScrollPosition;
            }

            if (eventType == EventType.Repaint && this.isFirstTime) {
                this.isFirstTime = false;
                this.Window.Repaint();
            }
        }

        /// <summary>
        /// Draw brush list and process its GUI events.
        /// </summary>
        /// <param name="showTabs">Indicates if tabs should be shown.</param>
        public void Draw(bool showTabs = true)
        {
            GUILayout.BeginVertical();
            if (showTabs) {
                this.DrawToolbar();
            }
            this.Draw(GUILayoutUtility.GetRect(0, Screen.width, 0, Screen.height));
            GUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            this.DrawToolbarButtons();
            GUILayout.EndHorizontal();
        }

        private Rect viewMenuPosition;
        private Rect contextMenuPosition;

        /// <summary>
        /// Draw tab buttons when implementing a custom toolbar.
        /// </summary>
        public void DrawToolbarButtons()
        {
            if (this.ShowViewTabs) {
                for (int i = 0; i < this.viewTabs.Length; ++i) {
                    using (var tabLabelContent = ControlContent.Basic(this.viewTabLabels[i])) {
                        Rect position = GUILayoutUtility.GetRect(tabLabelContent, RotorzEditorStyles.Instance.Tab);
                        int controlID = GUIUtility.GetControlID(FocusType.Passive, position);

                        switch (Event.current.GetTypeForControl(controlID)) {
                            case EventType.MouseDown:
                                if (position.Contains(Event.current.mousePosition)) {
                                    this.TabButton_SetSelectedView(this.viewTabs[i]);
                                    Event.current.Use();
                                }
                                break;

                            case EventType.Repaint:
                                RotorzEditorStyles.Instance.Tab.Draw(position, tabLabelContent, false, false, this.viewTabs[i] == this.Model.View, false);
                                break;
                        }
                    }
                }

                GUILayout.Label(GUIContent.none, RotorzEditorStyles.Instance.TabBackground);
            }
            else {
                Rect menuPosition = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarPopup);

                // Popup field text.
                string text;
                switch (this.Model.View) {
                    case BrushListView.Brushes:
                        text = TileLang.ParticularText("BrushList|View", "Brushes");
                        break;
                    case BrushListView.Master:
                        text = TileLang.ParticularText("BrushList|View", "Master");
                        break;
                    case BrushListView.Tileset:
                        TilesetAssetRecord selectedRecord = BrushDatabase.Instance.FindTilesetRecord(this.Model.SelectedTileset);
                        if (selectedRecord != null) {
                            text = selectedRecord.DisplayName;
                        }
                        else {
                            this.Model.View = BrushListView.Brushes;
                            text = TileLang.ParticularText("BrushList|View", "Brushes");
                        }
                        break;
                    default:
                        text = TileLang.ParticularText("Status|Unknown", "?");
                        break;
                }

                // Display popup field.
                using (var menuContent = ControlContent.Basic(text)) {
                    if (EditorInternalUtility.DropdownMenu(menuPosition, menuContent, EditorStyles.toolbarPopup)) {
                        this.ShowFilterMenuDropdown(this.viewMenuPosition);
                    }
                }
                if (Event.current.type == EventType.Repaint) {
                    this.viewMenuPosition = GUILayoutUtility.GetLastRect();
                    this.viewMenuPosition.height -= 2;
                }

                if (this.ShowTilesetContextMenu) {
                    EditorGUI.BeginDisabledGroup(this.Model.View != BrushListView.Tileset);
                    {
                        using (var content = ControlContent.Basic(
                            RotorzEditorStyles.Skin.ContextMenu,
                            TileLang.Text("Tileset Context Menu")
                        )) {
                            if (EditorInternalUtility.DropdownMenu(content, RotorzEditorStyles.Instance.ToolbarButtonNoStretch)) {
                                this.OnTilesetContextMenu(this.contextMenuPosition);
                            }
                            if (Event.current.type == EventType.Repaint) {
                                this.contextMenuPosition = GUILayoutUtility.GetLastRect();
                                this.contextMenuPosition.height -= 2;
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        private void TabButton_SetSelectedView(BrushListView view)
        {
            if (view != this.Model.View) {
                this.Model.View = view;

                // If switched to tileset view then select tileset that corresponds
                // with selected brush.
                var tilesetBrush = this.Model.SelectedBrush as TilesetBrush;
                if (tilesetBrush != null && this.Model.View == BrushListView.Tileset) {
                    this.Model.SelectedTileset = tilesetBrush.Tileset;
                }

                this.ScrollToBrush(this.Model.SelectedBrush);

                GUIUtility.ExitGUI();
            }
        }

        private void DrawLowerToolbar()
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(-6);

            if (GUILayout.Toggle(this.Model.Presentation == BrushListPresentation.List, RotorzEditorStyles.Skin.ListView_List, EditorStyles.toolbarButton, GUILayout.Width(26))) {
                if (this.Model.Presentation != BrushListPresentation.List) {
                    this.Model.Presentation = BrushListPresentation.List;
                    this.ScrollToBrush(this.Model.SelectedBrush);
                    GUIUtility.ExitGUI();
                }
            }

            if (GUILayout.Toggle(this.Model.Presentation == BrushListPresentation.Icons, RotorzEditorStyles.Skin.ListView_Icons, EditorStyles.toolbarButton, GUILayout.Width(26))) {
                if (this.Model.Presentation != BrushListPresentation.Icons) {
                    this.Model.Presentation = BrushListPresentation.Icons;
                    this.ScrollToBrush(this.Model.SelectedBrush);
                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.FlexibleSpace();

            // Show zooming interface?
            if (this.Model.Presentation == BrushListPresentation.Icons) {
                GUILayout.Label(RotorzEditorStyles.Skin.ZoomIcon);

                if (this.Model.ZoomMode == BrushListZoomMode.Custom) {
                    GUILayout.Space(-3);
                    Rect slider = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton, GUILayout.Width(100));
                    this.Model.ZoomTileSize = Mathf.FloorToInt(GUI.HorizontalSlider(slider, this.Model.ZoomTileSize, this.Model.MinimumZoomTileSize, this.Model.MaximumZoomTileSize));
                    GUILayout.Space(3);
                }
                else {
                    GUILayout.Space(-9);
                    GUIContent zoomModeLabelContent = ZoomModeLabels[Mathf.Clamp((int)this.Model.ZoomMode, 0, ZoomModeLabels.Length)];
                    Rect zoomModeLabel = GUILayoutUtility.GetRect(zoomModeLabelContent, EditorStyles.toolbarButton);
                    --zoomModeLabel.y;
                    GUI.Label(zoomModeLabel, zoomModeLabelContent, RotorzEditorStyles.Instance.MiniCenteredLabel);
                }

                this.DrawZoomMenuButton();

                GUILayout.Space(-5);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(-3);
        }

        private void DrawZoomMenuButton()
        {
            using (var content = ControlContent.Basic(RotorzEditorStyles.Skin.DownArrow)) {
                GUIStyle style = EditorStyles.toolbarButton;
                Rect position = GUILayoutUtility.GetRect(content, style);

                if (EditorInternalUtility.DropdownMenu(position, content, style)) {
                    var zoomMenu = new EditorMenu();

                    for (int i = 0; i < ZoomModeItems.Length; ++i) {
                        BrushListZoomMode zoomMode = ZoomModeItems[i];
                        zoomMenu.AddCommand(ZoomModeLabels[i].text)
                            .Checked(this.Model.ZoomMode == zoomMode)
                            .Action(() => {
                                this.Model.ZoomMode = zoomMode;
                                this.Window.Repaint();
                            });
                    }

                    zoomMenu.ShowAsDropdown(new Rect(position.x, position.y, position.width, position.height - 2));
                }
            }
        }

        private void DoScrollToBrush()
        {
            int brushIndex = this.Model.IndexOfRecord(this.scrollToBrush);
            if (brushIndex == -1) {
                this.Model.ScrollPosition = 0f;
            }
            else {
                int row = brushIndex / this.Columns;
                float scrollPosition = this.itemSize.y * row;

                // Need to offset scrolling when "Empty Label" is present.
                if (!string.IsNullOrEmpty(this.EmptyLabel)) {
                    scrollPosition += this.emptyLabelHeight;
                }

                // Only update scroll position if brush item is out of view.
                if (scrollPosition < this.Model.ScrollPosition) {
                    this.Model.ScrollPosition = scrollPosition - this.itemSize.y * 0.3f;
                }
                else if (scrollPosition + this.itemSize.y > this.Model.ScrollPosition + this.ListPosition.height) {
                    this.Model.ScrollPosition = scrollPosition - this.ListPosition.height + this.itemSize.y * 1.35f;
                }
            }

            this.doScrollToBrush = false;
            this.scrollToBrush = null;
        }

        #endregion


        #region Category and Name Filtering

        internal void DisplayCategoryFilterPopup(Rect position)
        {
            var projectSettings = ProjectSettings.Instance;

            int[] categoryIds = projectSettings.CategoryIds;
            string[] categoryLabels = projectSettings.CategoryLabels;

            var filterMenu = new EditorMenu();

            // Allow tileset brushes to be hidden in "Brushes" and "Master" views?
            if (!this.Model.HideTilesetBrushes) {
                filterMenu.AddCommand(TileLang.ParticularText("Action", "Hide Tileset Brushes"))
                    .Enabled(this.Model.View != BrushListView.Tileset)
                    .Checked(this.Model.FilterHideTilesetBrushes)
                    .Action(() => {
                        this.Model.FilterHideTilesetBrushes = !this.Model.FilterHideTilesetBrushes;
                    });

                filterMenu.AddCommand(TileLang.ParticularText("Action", "Always Show Favorite"))
                    .Enabled(this.Model.View != BrushListView.Tileset)
                    .Checked(this.Model.FilterAlwaysShowFavorite)
                    .Action(() => {
                        this.Model.FilterAlwaysShowFavorite = !this.Model.FilterAlwaysShowFavorite;
                    });
            }

            // Hidden brushes can only be filtered when enabled!
            if (this.CanShowHidden) {
                filterMenu.AddCommand(TileLang.ParticularText("Action", "Show Hidden"))
                    .Checked(this.Model.ShowHidden)
                    .Action(() => {
                        this.Model.ShowHidden = !this.Model.ShowHidden;
                    });
            }

            filterMenu.AddSeparator();

            filterMenu.AddCommand(TileLang.ParticularText("Action", "Filter Favorite"))
                .Checked(this.Model.FilterFavorite)
                .Action(() => {
                    this.Model.FilterFavorite = !this.Model.FilterFavorite;
                });

            filterMenu.AddSeparator();

            if (categoryLabels.Length > 0) {
                int selectedCategoryNumber = -1;
                if (this.Model.CategoryFiltering == CategoryFiltering.Selection && this.Model.CategorySelection.Count != 0) {
                    selectedCategoryNumber = this.Model.CategorySelection.First();
                }

                filterMenu.AddCommand(TileLang.ParticularText("Action", "All Categories"))
                    .Checked(this.Model.CategoryFiltering == CategoryFiltering.None)
                    .Action(() => {
                        this.Model.CategoryFiltering = CategoryFiltering.None;
                    });

                for (int i = 0; i < categoryIds.Length; ++i) {
                    int number = categoryIds[i];
                    string label = categoryLabels[i];

                    filterMenu.AddCommand(label)
                        .Checked(selectedCategoryNumber == number)
                        .Action(() => {
                            this.Model.SetCategorySelection(number);
                        });
                }

                filterMenu.AddCommand(TileLang.ParticularText("Action", "Uncategorized"))
                    .Checked(selectedCategoryNumber == 0)
                    .Action(() => {
                        this.Model.SetCategorySelection(0);
                    });

                filterMenu.AddSeparator();

                filterMenu.AddCommand(TileLang.ParticularText("Action", "Custom"))
                    .Checked(this.Model.CategoryFiltering == CategoryFiltering.CustomSelection)
                    .Action(() => {
                        if (this.Model.CustomCategorySelection.Count == 0) {
                            // Display "Select Brush Categories" window.
                            SelectBrushCategoriesWindow.ShowWindow(OnBrushCategoryMaskSelected, this.Model.CustomCategorySelection);
                        }
                        else {
                            this.Model.CategoryFiltering = CategoryFiltering.CustomSelection;
                        }
                    });

                filterMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Select Custom")))
                    .Action(() => {
                        // Display "Select Brush Categories" window.
                        SelectBrushCategoriesWindow.ShowWindow(OnBrushCategoryMaskSelected, this.Model.CustomCategorySelection);
                    });
            }
            else {
                filterMenu.AddCommand(TileLang.Text("(No Categories)"));

                // Disable filter control when no categories are in use.
                this.Model.CategoryFiltering = CategoryFiltering.None;
            }

            filterMenu.ShowAsDropdown(position);
        }

        private void OnBrushCategoryMaskSelected(ICollection<int> selection)
        {
            if (selection.Count == 0) {
                // Simply do nothing!
                return;
            }

            // Adjust custom category selection.
            this.Model.CategoryFiltering = CategoryFiltering.CustomSelection;
            this.Model.CustomCategorySelection.Clear();
            foreach (int categoryNumber in selection) {
                this.Model.CustomCategorySelection.Add(categoryNumber);
            }

            this.Window.Repaint();
        }

        #endregion


        #region Filtering GUI

        private void DrawCategoryAndSearchField(ref Rect position)
        {
            position.y += 3;

            Rect r = new Rect(position.x, position.y, 30, 20);
            using (var buttonContent = ControlContent.Basic(
                RotorzEditorStyles.Skin.FilterIcon,
                TileLang.ParticularText("Action", "Filter Category")
            )) {
                if (EditorInternalUtility.DropdownMenu(r, buttonContent, RotorzEditorStyles.Instance.FlatButton)) {
                    this.DisplayCategoryFilterPopup(r);
                }
            }

            r.x += 30 + 5;
            r.y += 1;
            r.width = position.width - 30 - 5 - 3;
            r.height = 15;

            GUI.SetNextControlName("SearchFilter");
            this.Model.SearchFilterText = RotorzEditorGUI.SearchField(r, this.Model.SearchFilterText);

            position.y += 22;
            position.height -= 22 - 3;
        }

        private void DrawFilterCaption(ref Rect position, Brush brush)
        {
            Color restoreColor = GUI.contentColor;
            GUI.contentColor = EditorGUIUtility.isProSkin
                ? new Color(0.7f, 0.7f, 0.7f)
                : new Color(0.3f, 0.3f, 0.3f);

            Rect r = new Rect(position.x, position.y + 2, position.width - 23, 18);
            position.y += 20;
            position.height -= 20;

            if (this.Model.FilterDescription != "") {
                GUI.Label(r, this.Model.FilterDescription, EditorStyles.whiteLabel);
            }

            if (GUI.Button(new Rect(r.x + r.width, r.y, 22, 15), GUIContent.none, RotorzEditorStyles.Instance.SmallRemoveButton)) {
                this.Model.CategoryFiltering = CategoryFiltering.None;
                this.Model.FilterFavorite = false;
                this.ScrollToBrush(brush);
                GUIUtility.ExitGUI();
            }

            ExtraEditorGUI.SeparatorLight(new Rect(r.x, r.y + 17, position.width, 1));

            GUI.contentColor = restoreColor;
        }

        private void DrawTilesetFilter(ref Rect position)
        {
            var selectedTilesetRecord = BrushDatabase.Instance.FindTilesetRecord(this.Model.SelectedTileset);
            EditorGUI.BeginDisabledGroup(BrushDatabase.Instance.TilesetRecords.Count == 0);

            Rect r = new Rect(position.x + 5, position.y, position.width - 10, EditorStyles.popup.fixedHeight + 5);
            position.y += r.height;
            position.height -= r.height;

            using (var content = ControlContent.Basic(
                TileLang.Text("Tileset")
            )) {
                GUI.Label(r, content);

                if (this.ShowTilesetContextMenu) {
                    // Remove width of tileset options button.
                    EditorGUI.BeginDisabledGroup(this.Model.SelectedTileset == null);
                    using (var menuContent = ControlContent.Basic(
                        RotorzEditorStyles.Skin.SmallGearButton
                    )) {
                        if (EditorInternalUtility.DropdownMenu(new Rect(r.xMax - 27, r.y - 3, 32, r.height + 1), menuContent, RotorzEditorStyles.Instance.FlatButton)) {
                            this.OnTilesetContextMenu(new Rect(r.xMax - 27, r.y - 3, 32, r.height + 1));
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    r.width -= 28;
                }

                // Offset by size of label.
                float labelWidth = GUI.skin.label.CalcSize(content).x + 5;
                r.x += labelWidth;
                r.width -= labelWidth;
                r.height -= 5;

                // Display popup field.
                using (var menuContent = ControlContent.Basic(
                    labelText: selectedTilesetRecord != null
                        ? selectedTilesetRecord.DisplayName
                        : TileLang.Text("(None)")
                )) {
                    if (EditorInternalUtility.DropdownMenu(r, menuContent, EditorStyles.popup)) {
                        this.ShowFilterMenuDropdown(r);
                    }
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnTilesetContextMenu(Rect position)
        {
            var tilesetContextMenu = new EditorMenu();

            tilesetContextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Show in Designer")))
                .Action(() => {
                    ToolUtility.ShowTilesetInDesigner(this.Model.SelectedTileset);
                });

            tilesetContextMenu.AddSeparator();

            tilesetContextMenu.AddCommand(TileLang.ParticularText("Action", "Reveal Material"))
                .Action(() => {
                    var tileset = this.Model.SelectedTileset;
                    if (tileset != null && tileset.AtlasMaterial != null) {
                        EditorInternalUtility.FocusInspectorWindow();
                        EditorGUIUtility.PingObject(tileset.AtlasMaterial);
                        Selection.activeObject = tileset.AtlasMaterial;
                    }
                });

            tilesetContextMenu.AddCommand(TileLang.ParticularText("Action", "Reveal Texture"))
                .Action(() => {
                    var tileset = this.Model.SelectedTileset;
                    if (tileset != null && tileset.AtlasMaterial != null && tileset.AtlasMaterial.mainTexture != null) {
                        EditorInternalUtility.FocusInspectorWindow();
                        EditorGUIUtility.PingObject(tileset.AtlasMaterial.mainTexture);
                        Selection.activeObject = tileset.AtlasMaterial.mainTexture;
                    }
                });

            tilesetContextMenu.AddSeparator();

            tilesetContextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Delete Tileset")))
                .Action(() => {
                    DeleteTilesetWindow.ShowWindow(this.Model.SelectedTileset);
                });

            tilesetContextMenu.ShowAsDropdown(position);
        }

        private void ShowFilterMenuDropdown(Rect position)
        {
            var tilesetMenu = new EditorMenu();

            // When tab buttons are present, do not show views in filter drop-down.
            if (this.ShowViewTabs) {
                tilesetMenu.AddCommand(TileLang.ParticularText("Action|Select", "None"))
                    .Checked(this.Model.SelectedTileset == null)
                    .Action(() => {
                        this.Model.SelectedTileset = null;
                    });
            }
            else {
                tilesetMenu.AddCommand(TileLang.ParticularText("BrushList|View", "Brushes"))
                    .Checked(this.Model.View == BrushListView.Brushes)
                    .Action(() => {
                        this.Model.SelectedTileset = null;
                        this.Model.View = BrushListView.Brushes;
                        this.ScrollToBrush(this.Model.SelectedBrush);
                    });

                tilesetMenu.AddCommand(TileLang.ParticularText("BrushList|View", "Master"))
                    .Visible((this.VisibleViews & BrushListView.Master) == BrushListView.Master)
                    .Checked(this.Model.View == BrushListView.Master)
                    .Action(() => {
                        this.Model.SelectedTileset = null;
                        this.Model.View = BrushListView.Master;
                        this.ScrollToBrush(this.Model.SelectedBrush);
                    });
            }

            // Only attempt to add tilesets to menu if there are any!
            if (BrushDatabase.Instance.TilesetRecords.Count > 0) {
                tilesetMenu.AddSeparator();

                foreach (var tilesetRecord in BrushDatabase.Instance.TilesetRecords) {
                    var tileset = tilesetRecord.Tileset;
                    tilesetMenu.AddCommand(tilesetRecord.DisplayName)
                        .Checked(this.Model.View == BrushListView.Tileset && tileset == this.Model.SelectedTileset)
                        .Action(() => {
                            this.Model.SelectedTileset = tileset;
                            this.Model.View = BrushListView.Tileset;
                            this.ScrollToBrush(this.Model.SelectedBrush);
                        });
                }
            }

            tilesetMenu.ShowAsDropdown(position);
        }

        #endregion


        #region Metrics

        /// <summary>
        /// Gets position of list control.
        /// </summary>
        public Rect Position { get; private set; }
        /// <summary>
        /// Gets position of view area of list control.
        /// </summary>
        public Rect ListPosition { get; private set; }
        /// <summary>
        /// Gets total position of inner list area.
        /// </summary>
        public Rect ListArea { get; private set; }

        /// <summary>
        /// Gets the number of columns of brushes.
        /// </summary>
        internal int Columns { get; private set; }


        private Vector2 itemSize;
        private float emptyLabelHeight;

        #endregion


        private void DrawBrushButton(Rect position, GUIContent customLabel, BrushAssetRecord record, Brush selectedBrush, GUIStyle style)
        {
            var brush = record != null ? record.Brush : null;

            // Use `record.DisplayName` for hover tips.
            var hoverTipProvider = (brush != null && this.Model.Presentation == BrushListPresentation.Icons)
                ? TextProviders.FromBrushAssetRecordDisplayName
                : null;
            Rect hoverControlPosition = new Rect(position.x, position.y + 4, position.width, position.height - 7);
            int controlID = RotorzEditorGUI.GetHoverControlID(hoverControlPosition, hoverTipProvider, record);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = controlID;
                        Event.current.Use();

                        if (this.EnableDragAndDrop) {
                            this.permitDragOperation = brush != null;
                            this.dragAnchor = Event.current.mousePosition;
                        }

                        if (this.BrushMouseDown != null) {
                            this.BrushMouseDown(brush);
                        }
                    }
                    break;

                case EventType.MouseUp:
                    this.permitDragOperation = false;

                    if (GUIUtility.hotControl == controlID) {
                        GUIUtility.hotControl = 0;

                        if (position.Contains(Event.current.mousePosition)) {
                            if (this.BrushClicked != null) {
                                this.BrushClicked(brush);
                            }

                            // If event was not handled by `BrushClicked` handler, check for context menu.
                            // Note: We are not using EventType.ContextClick since this does not redraw
                            //       properly on Mac. This causes the menu font to appear smaller.
                            if (Event.current.type != EventType.Used && Event.current.button == 1) {
                                // Only proceed if mouse position is within scroll area.
                                float mouseScreenY = Event.current.mousePosition.y - this.Model.ScrollPosition;
                                if (mouseScreenY >= 0f && mouseScreenY < this.ListPosition.height) {
                                    if (this.BrushContextMenu != null) {
                                        this.BrushContextMenu(brush);
                                    }
                                    Event.current.Use();
                                }
                            }
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        RotorzEditorGUI.ClearHoverTip();

                        if (this.permitDragOperation && Vector2.Distance(this.dragAnchor, Event.current.mousePosition) > this.DragThreshold) {
                            this.permitDragOperation = false;
                            GUIUtility.hotControl = 0;

                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new Object[] { record.Brush };
                            DragAndDrop.paths = new string[0];

                            DragAndDrop.StartDrag(TileLang.FormatDragObjectTitle(record.DisplayName, TileLang.Text("Brush")));
                        }

                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    if (ExtraEditorGUI.VisibleRect.Overlaps(position)) {
                        bool isSelectedBrush = brush == selectedBrush;

                        if (customLabel != null) {
                            style.Draw(position, customLabel, controlID, isSelectedBrush);
                        }
                        else if (this.Model.Presentation == BrushListPresentation.List) {
                            using (var tempContent = ControlContent.Basic(record.DisplayName)) {
                                style.Draw(position, tempContent, controlID, isSelectedBrush);
                            }
                        }
                        else {
                            style.Draw(position, GUIContent.none, controlID, isSelectedBrush);
                        }
                    }
                    break;
            }
        }


        #region Presentation: List

        private void CalculateListMetrics(Rect position)
        {
            float itemHeight = 53;

            int itemCount = this.Model.Records.Count;
            if (!string.IsNullOrEmpty(this.EmptyLabel)) {
                ++itemCount;
            }

            Rect listArea = new Rect(0, 0, position.width - 4, itemCount * itemHeight + 5);

            // Is a vertical scrollbar needed?
            if (this.ListArea.height > position.height) {
                float scrollbarWidth = GUI.skin.verticalScrollbar.fixedWidth;
                listArea.width -= scrollbarWidth;
                position.width -= scrollbarWidth;
            }

            this.ListPosition = position;
            this.ListArea = listArea;

            this.Columns = 1;
            this.itemSize = new Vector2(listArea.width, itemHeight);

            this.emptyLabelHeight = !string.IsNullOrEmpty(this.EmptyLabel)
                ? itemHeight
                : 0;
        }

        private void DrawList(Event e, Rect position, Brush selectedBrush)
        {
            Color restoreBackground = GUI.backgroundColor;
            Color activeBackground = ToolManager.Instance.CurrentTool != null ? Color.red : RotorzEditorStyles.SelectedHighlightColor;

            float itemHeight = this.itemSize.y;

            Vector2 scrollPosition = new Vector2(0, this.Model.ScrollPosition);
            Rect listArea = this.ListArea;
            listArea.width = 1;
            this.Model.ScrollPosition = GUI.BeginScrollView(this.Position, scrollPosition, listArea).y;

            position.x = 2;
            position.y = 3;
            position.width = this.itemSize.x;
            position.height = itemHeight;

            // Add blank button at start?
            if (!string.IsNullOrEmpty(this.EmptyLabel)) {
                if (selectedBrush == null) {
                    GUI.backgroundColor = activeBackground;
                }

                using (var content = ControlContent.Basic(this.EmptyLabel)) {
                    this.DrawBrushButton(position, content, null, selectedBrush, RotorzEditorStyles.Instance.ListViewButton);
                }

                GUI.backgroundColor = restoreBackground;

                position.y += itemHeight;
            }

            Rect textureRect = new Rect(5, 0, 48, 48);

            bool encounteredMissingReference = false;

            // Cancel permission to drag and drop?
            if (e.type == EventType.MouseMove) {
                this.permitDragOperation = false;
            }

            foreach (var record in this.Model.Records) {
                if (record.Brush == null) {
                    encounteredMissingReference = true;
                    continue;
                }

                GUI.backgroundColor = (selectedBrush == record.Brush)
                    ? activeBackground
                    : restoreBackground;

                this.DrawBrushButton(position, null, record, selectedBrush, RotorzEditorStyles.Instance.ListViewButton);

                // Draw preview image.
                if (e.type == EventType.Repaint) {
                    textureRect.y = position.y + 2;
                    RotorzEditorGUI.DrawBrushPreviewWithoutFallbackLabel(textureRect, record, selectedBrush == record.Brush && selectedBrush != null);
                }

                position.y += itemHeight;
            }

            GUI.backgroundColor = restoreBackground;

            GUI.EndScrollView();

            if (encounteredMissingReference && e.type == EventType.Repaint) {
                Debug.LogWarning(TileLang.Text("Missing brush reference was encountered whilst drawing list."));
                BrushDatabase.Instance.ClearMissingRecords();
            }
        }

        #endregion


        #region Presentation: Icons

        private Vector2 tileIconSize;

        private Vector2 CalculateZoomTileSize(int listWidth, Rect position)
        {
            Vector2 zoomTileSize;

            switch (this.Model.ZoomMode) {
                default:
                case BrushListZoomMode.Automatic:
                case BrushListZoomMode.BestFit:
                    bool isTilesetView = this.Model.View == BrushListView.Tileset && this.Model.SelectedTileset != null;
                    if (isTilesetView && this.Model.ZoomMode != BrushListZoomMode.BestFit) {
                        zoomTileSize = new Vector2(this.Model.SelectedTileset.TileWidth, this.Model.SelectedTileset.TileHeight);
                    }
                    else {
                        int padding = 6;
                        int tileSize = 60 + padding;

                        // Divide into equal columns of 64px wide.
                        int columnCount = Mathf.Max(1, listWidth / tileSize);
                        // Calculate the amount of unused "wasted" horizontal space.
                        int wasted = listWidth - (columnCount * tileSize);
                        // Redistribute wasted space across columns.
                        int size = tileSize + wasted / columnCount - padding;

                        zoomTileSize = new Vector2(size, size);
                    }
                    break;

                case BrushListZoomMode.Custom:
                    zoomTileSize = new Vector2(this.Model.ZoomTileSize, this.Model.ZoomTileSize);
                    break;
            }

            return zoomTileSize;
        }

        private void CalculateIconsMetrics(Rect position)
        {
            // Always make room for the vertical scrollbar!
            position.width -= GUI.skin.verticalScrollbar.fixedWidth + 2;

            this.tileIconSize = this.CalculateZoomTileSize((int)position.width, position);
            Vector2 itemSize = new Vector2(this.tileIconSize.x + 6, this.tileIconSize.y + 6);

            int itemCount = this.Model.Records.Count;

            Rect listArea = new Rect(0, 0, position.width, position.height);

            // Calculate the maximum number of icons that can be shown in one view.
            this.Columns = (int)listArea.width / (int)itemSize.x;
            int rows = (int)listArea.height / (int)itemSize.y;

            if (this.Columns == 0) {
                return;
            }

            rows = itemCount / this.Columns;
            if (itemCount % this.Columns != 0) {
                ++rows;
            }

            this.ListPosition = position;
            listArea.height = rows * itemSize.y + 7;

            // Add room for extra button?
            if (!string.IsNullOrEmpty(this.EmptyLabel)) {
                this.emptyLabelHeight = 28 + 4;
                listArea.height += this.emptyLabelHeight;
            }
            else {
                this.emptyLabelHeight = 0;
            }

            this.ListArea = listArea;

            this.itemSize = itemSize;
        }

        private void DrawIcons(Event e, Rect position, Brush selectedBrush)
        {
            bool isRepaintEvent = (Event.current.type == EventType.Repaint);

            Color restoreBackground = GUI.backgroundColor;
            Color activeBackground = ToolManager.Instance.CurrentTool != null ? Color.red : RotorzEditorStyles.SelectedHighlightColor;

            if (this.Columns == 0) {
                return;
            }

            float itemWidth = this.itemSize.x;
            float itemHeight = this.itemSize.y;

            int i = 0;

            Vector2 scrollPosition = new Vector2(0, this.Model.ScrollPosition);
            Rect listArea = this.ListArea;
            listArea.width = 1;
            this.Model.ScrollPosition = GUI.BeginScrollView(this.Position, scrollPosition, listArea).y;

            Rect textureRect = new Rect(0, 0, 0, 0);

            position.x = 2;
            position.y = 3;
            position.width = this.ListArea.width - 1;
            position.height = 28;

            // Add blank button to disable painting?
            if (!string.IsNullOrEmpty(this.EmptyLabel)) {
                if (selectedBrush == null) {
                    GUI.backgroundColor = activeBackground;
                }

                using (var content = ControlContent.Basic(this.EmptyLabel)) {
                    this.DrawBrushButton(position, content, null, selectedBrush, RotorzEditorStyles.Instance.ListViewExtaButton);
                }

                GUI.backgroundColor = restoreBackground;

                position.y += position.height + 3;
            }

            // Use stronger highlight color for tile icons!
            activeBackground = ToolManager.Instance.CurrentTool != null ? Color.red : RotorzEditorStyles.SelectedHighlightStrongColor;

            position.width = itemWidth;
            position.height = itemHeight - 1;

            bool encounteredMissingReference = false;

            // Cancel permission to drag and drop?
            if (e.type == EventType.MouseMove) {
                this.permitDragOperation = false;
            }

            foreach (var record in this.Model.Records) {
                if (record.Brush == null) {
                    encounteredMissingReference = true;
                    continue;
                }

                if (i++ == this.Columns) {
                    i = 1;
                    position.x = 2;
                    position.y += itemHeight;
                }

                // Draw preview image.
                if (isRepaintEvent) {
                    textureRect.x = position.x + 3;
                    textureRect.y = position.y + 3;
                    textureRect.width = this.tileIconSize.x;
                    textureRect.height = this.tileIconSize.y;

                    RotorzEditorGUI.DrawBrushPreview(textureRect, record, selectedBrush == record.Brush);
                }

                GUI.backgroundColor = (selectedBrush == record.Brush)
                    ? activeBackground
                    : restoreBackground;

                this.DrawBrushButton(position, GUIContent.none, record, selectedBrush, RotorzEditorStyles.Instance.ListViewIconButton);

                position.x += itemWidth;
            }

            GUI.backgroundColor = restoreBackground;

            GUI.EndScrollView();

            if (encounteredMissingReference && e.type == EventType.Repaint) {
                Debug.LogWarning(TileLang.Text("Missing brush reference was encountered whilst drawing list."));
                BrushDatabase.Instance.ClearMissingRecords();
            }
        }

        #endregion
    }
}
