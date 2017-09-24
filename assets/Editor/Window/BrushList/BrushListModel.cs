// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Represents the method that will handle list view changed events.
    /// </summary>
    /// <param name="previous">Previously selected view.</param>
    /// <param name="current">Currently selected view.</param>
    public delegate void ViewChangedHandler(BrushListView previous, BrushListView current);

    /// <summary>
    /// Represents the method that will handle brush selection changed events.
    /// </summary>
    /// <param name="previous">Previously selected brush.</param>
    /// <param name="current">Currently selected brush.</param>
    public delegate void BrushChangedHandler(Brush previous, Brush current);

    /// <summary>
    /// Represents the method that will handle tileset selection changed events.
    /// </summary>
    /// <param name="previous">Previously selected tileset.</param>
    /// <param name="current">Currently selected tileset.</param>
    public delegate void TilesetChangedHandler(Tileset previous, Tileset current);


    /// <summary>
    /// Model for brush list control which allows brushes to be filtered by
    /// various criteria. It is also possible to access the collection of
    /// filtered brushes.
    /// </summary>
    /// <seealso cref="BrushListControl"/>
    public sealed class BrushListModel : IDirtyableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrushListModel"/> class.
        /// </summary>
        public BrushListModel()
        {
            this.HasQueryChanged = true;
            this.Presentation = BrushListPresentation.List;
            this.View = BrushListView.Brushes;
        }


        #region Brush Selection

        private Brush selectedBrush;
        private Brush selectedBrushSecondary;

        [SettingProperty("SelectedBrushInstanceID")]
        private int selectedBrushInstanceID;
        [SettingProperty("SelectedBrushSecondaryInstanceID")]
        private int selectedBrushSecondaryInstanceID;


        /// <summary>
        /// Raised when value of <see cref="View"/> is changed.
        /// </summary>
        public event ViewChangedHandler ViewChanged;
        /// <summary>
        /// Raised when value of <see cref="SelectedBrush"/> is changed.
        /// </summary>
        public event BrushChangedHandler SelectedBrushChanged;
        /// <summary>
        /// Raised when value of <see cref="SelectedBrushSecondary"/> is changed.
        /// </summary>
        public event BrushChangedHandler SelectedBrushSecondaryChanged;


        /// <summary>
        /// Gets or sets the primary selected brush.
        /// </summary>
        public Brush SelectedBrush {
            get { return this.selectedBrush; }
            set {
                if (value != this.selectedBrush) {
                    Brush previous = this.selectedBrush;
                    this.selectedBrush = value;
                    this.selectedBrushInstanceID = value != null ? value.GetInstanceID() : 0;

                    this.isDirty = true;

                    // Raise event handler.
                    if (this.SelectedBrushChanged != null) {
                        this.SelectedBrushChanged(previous, value);
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the secondary selected brush.
        /// </summary>
        public Brush SelectedBrushSecondary {
            get { return this.selectedBrushSecondary; }
            set {
                if (value != this.selectedBrushSecondary) {
                    Brush previous = this.selectedBrushSecondary;
                    this.selectedBrushSecondary = value;
                    this.selectedBrushSecondaryInstanceID = value != null ? value.GetInstanceID() : 0;

                    this.isDirty = true;

                    // Raise event handler.
                    if (this.SelectedBrushSecondaryChanged != null) {
                        this.SelectedBrushSecondaryChanged(previous, value);
                    }
                }
            }
        }

        #endregion


        #region Tileset Selection

        private Tileset selectedTileset;

        [SettingProperty("SelectedTilesetInstanceID")]
        private int selectedTilesetInstanceID;


        /// <summary>
        /// Raised when value of <see cref="SelectedTileset"/> is changed.
        /// </summary>
        public event TilesetChangedHandler SelectedTilesetChanged;


        /// <summary>
        /// Gets or sets the selected tileset.
        /// </summary>
        /// <remarks>
        /// <para>Set to <c>null</c> to clear filter.</para>
        /// </remarks>
        public Tileset SelectedTileset {
            get { return this.selectedTileset; }
            set {
                if (value != this.selectedTileset) {
                    Tileset previous = this.selectedTileset;
                    this.selectedTileset = value;
                    this.selectedTilesetInstanceID = value != null ? value.GetInstanceID() : 0;

                    this.isDirty = this.HasQueryChanged = true;

                    // Raise event handler.
                    if (this.SelectedTilesetChanged != null) {
                        this.SelectedTilesetChanged(previous, value);
                    }
                }
            }
        }

        #endregion


        #region List Presentation

        [SettingProperty("Presentation")]
        private BrushListPresentation presentation;
        [SettingProperty("View")]
        private BrushListView view;
        [SettingProperty("ScrollPosition")]
        private float scrollPosition;


        /// <summary>
        /// Gets or sets presentation of list view.
        /// </summary>
        public BrushListPresentation Presentation {
            get { return this.presentation; }
            set {
                if (value != this.presentation) {
                    this.presentation = value;
                    this.isDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets list view.
        /// </summary>
        public BrushListView View {
            get { return this.view; }
            set {
                if (value != this.view) {
                    BrushListView previous = this.view;
                    this.view = value;
                    this.isDirty = this.HasQueryChanged = true;

                    // Raise event handler.
                    if (this.ViewChanged != null) {
                        this.ViewChanged(previous, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets scroll position of list view.
        /// </summary>
        public float ScrollPosition {
            get { return this.scrollPosition; }
            set {
                if (value != this.scrollPosition) {
                    this.scrollPosition = value;
                    this.isDirty = true;
                }
            }
        }

        #endregion


        #region Zooming

        private BrushListZoomMode zoomMode;
        private int zoomTileSize = 64;
        private int minimumZoomTileSize = 32;
        private int maximumZoomTileSize = 256;


        /// <summary>
        /// Gets or sets zoom mode for list view.
        /// </summary>
        /// <remarks>
        /// <para>Zoom options are only available for the icons presentation.</para>
        /// </remarks>
        /// <seealso cref="ZoomTileSize"/>
        [SettingProperty]
        public BrushListZoomMode ZoomMode {
            get { return this.zoomMode; }
            set {
                if (value != this.zoomMode) {
                    this.zoomMode = value;
                    this.isDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets the minimum value of the <see cref="ZoomTileSize"/> property.
        /// </summary>
        public int MinimumZoomTileSize {
            get { return this.minimumZoomTileSize; }
            set { this.minimumZoomTileSize = Mathf.Max(8, value); }
        }

        /// <summary>
        /// Gets the maximum value of the <see cref="ZoomTileSize"/> property.
        /// </summary>
        public int MaximumZoomTileSize {
            get { return this.maximumZoomTileSize; }
            set { this.maximumZoomTileSize = Mathf.Clamp(value, this.MinimumZoomTileSize, int.MaxValue); }
        }

        /// <summary>
        /// Gets or sets tile size for custom zoom mode.
        /// </summary>
        /// <seealso cref="ZoomMode"/>
        [SettingProperty]
        public int ZoomTileSize {
            get { return this.zoomTileSize; }
            set {
                value = Mathf.Clamp(value, this.MinimumZoomTileSize, this.MaximumZoomTileSize);
                if (value != this.zoomTileSize) {
                    this.zoomTileSize = value;
                    this.isDirty = true;
                }
            }
        }

        #endregion


        #region Brush Filtering

        private bool hideAliasBrushes;
        private bool hideTilesetBrushes;
        private bool showHidden;

        private bool filterFavorite;

        private bool filterHideTilesetBrushes;
        private bool filterAlwaysShowFavorite = true;

        private string searchFilterText = string.Empty;


        [SettingProperty("CategoryFiltering")]
        private CategoryFiltering categoryFiltering;
        [SettingProperty("CategorySelection")]
        private CategorySet categorySelection;
        [SettingProperty("CustomCategorySelection")]
        private CategorySet customCategorySelection;


        private class CategorySet : ICollection<int>
        {
            internal BrushListModel model;
            private HashSet<int> categoryNumbers = new HashSet<int>();


            private void MarkChanged()
            {
                // The model will be null when category set is first being deserialized.
                if (this.model == null) {
                    return;
                }

                this.model.isDirty = this.model.HasQueryChanged = true;
            }


            #region ICollection<int> Members

            public void Add(int categoryNumber)
            {
                if (this.categoryNumbers.Add(categoryNumber)) {
                    this.MarkChanged();
                }
            }

            public void Clear()
            {
                if (this.categoryNumbers.Count != 0) {
                    this.categoryNumbers.Clear();
                    this.MarkChanged();
                }
            }

            public bool Contains(int categoryNumber)
            {
                return this.categoryNumbers.Contains(categoryNumber);
            }

            public void CopyTo(int[] array, int arrayIndex)
            {
                this.categoryNumbers.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return this.categoryNumbers.Count; }
            }

            bool ICollection<int>.IsReadOnly {
                get { return false; }
            }

            public bool Remove(int categoryNumber)
            {
                if (this.categoryNumbers.Remove(categoryNumber)) {
                    this.MarkChanged();
                    return true;
                }
                return false;
            }

            #endregion


            #region IEnumerable<int> Members

            public IEnumerator<int> GetEnumerator()
            {
                return this.categoryNumbers.GetEnumerator();
            }

            #endregion


            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.categoryNumbers.GetEnumerator();
            }

            #endregion

        }

        /// <summary>
        /// Gets or sets a value indicating whether brushes should be re-queried.
        /// </summary>
        public bool HasQueryChanged { get; set; }

        /// <summary>
        /// Gets or sets whether alias brushes should be hidden.
        /// </summary>
        [SettingProperty]
        public bool HideAliasBrushes {
            get { return this.hideAliasBrushes; }
            set {
                if (value != this.hideAliasBrushes) {
                    this.hideAliasBrushes = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets whether tileset brushes should be hidden except in case of
        /// tileset view where tileset brushes are always shown.
        /// </summary>
        [SettingProperty]
        public bool HideTilesetBrushes {
            get { return this.hideTilesetBrushes; }
            set {
                if (value != this.hideTilesetBrushes) {
                    this.hideTilesetBrushes = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets whether hidden brushes should be shown.
        /// </summary>
        [SettingProperty]
        public bool ShowHidden {
            get { return this.showHidden; }
            set {
                if (value != this.showHidden) {
                    this.showHidden = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether only favorite brushes should be shown.
        /// </summary>
        [SettingProperty]
        public bool FilterFavorite {
            get { return this.filterFavorite; }
            set {
                if (value != this.filterFavorite) {
                    this.filterFavorite = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether tileset brushes should be hidden from "Brushes" and
        /// "Master" view.
        /// </summary>
        /// <remarks>
        /// <para>Unlike <see cref="HideTilesetBrushes"/> the value of this field can
        /// be changed using the brush list GUI.</para>
        /// </remarks>
        [SettingProperty]
        public bool FilterHideTilesetBrushes {
            get { return this.filterHideTilesetBrushes; }
            set {
                if (value != this.filterHideTilesetBrushes) {
                    this.filterHideTilesetBrushes = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }
        /// <summary>
        /// Indicates whether favorite brushes should always be shown.
        /// </summary>
        /// <remarks>
        /// <para>Favorite brushes are shown regardless of what has been specified
        /// for <see cref="FilterHideTilesetBrushes"/>.</para>
        /// </remarks>
        [SettingProperty]
        public bool FilterAlwaysShowFavorite {
            get { return this.filterAlwaysShowFavorite; }
            set {
                if (value != this.filterAlwaysShowFavorite) {
                    this.filterAlwaysShowFavorite = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets search filter text. Brushes are only shown if their name
        /// contains the filter text.
        /// </summary>
        /// <remarks>
        /// <para>Assign empty string or a value of <c>null</c> to clear filter.</para>
        /// </remarks>
        [SettingProperty]
        public string SearchFilterText {
            get { return this.searchFilterText; }
            set {
                if (value != this.searchFilterText) {
                    this.searchFilterText = value;
                    this.isDirty = this.HasQueryChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets type of category filtering.
        /// </summary>
        /// <example>
        /// <para>Clear any category filtering:</para>
        /// <code language="csharp"><![CDATA[
        /// model.CategoryFiltering = CategoryFiltering.None;
        /// ]]></code>
        ///
        /// <para>Show all brushes from category 4:</para>
        /// <code language="csharp"><![CDATA[
        /// model.CategoryFiltering = CategoryFiltering.Selection;
        /// model.CategorySelection.Clear();
        /// model.CategorySelection.Add(1);
        /// ]]></code>
        ///
        /// <para>Show all uncategorized brushes:</para>
        /// <code language="csharp"><![CDATA[
        /// model.CategoryFiltering = CategoryFiltering.Selection;
        /// model.CategorySelection.Clear();
        /// model.CategorySelection.Add(0);
        /// ]]></code>
        ///
        /// <para>Show all brushes from categories 7 and 8:</para>
        /// <code language="csharp"><![CDATA[
        /// model.CategoryFiltering = CategoryFiltering.Selection;
        /// model.CategorySelection.Add(7);
        /// model.CategorySelection.Add(8);
        /// ]]></code>
        /// </example>
        /// <seealso cref="CategorySelection"/>
        public CategoryFiltering CategoryFiltering {
            get { return this.categoryFiltering; }
            set {
                if (value != this.categoryFiltering) {
                    this.categoryFiltering = value;
                    this.isDirty = this.HasQueryChanged = true;

                    this.CategorySelection.Clear();
                }
            }
        }

        /// <summary>
        /// Gets selection of categories which should be used to filter brushes when
        /// <see cref="CategoryFiltering"/> is set to a value of <see cref="Editor.CategoryFiltering.Selection"/>.
        /// </summary>
        /// <example>
        /// <para>Show all brushes from categories 7 and 8:</para>
        /// <code language="csharp"><![CDATA[
        /// model.CategoryFiltering = CategoryFiltering.Selection;
        /// model.CategorySelection.Add(7);
        /// model.CategorySelection.Add(8);
        /// ]]></code>
        /// </example>
        /// <seealso cref="CategoryFiltering"/>
        /// <seealso cref="CustomCategorySelection"/>
        public ICollection<int> CategorySelection {
            get {
                // Ensure that category selection set exists.
                var selection = this.categorySelection;
                if (selection == null) {
                    this.categorySelection = selection = new CategorySet();
                }

                // Ensure that selection is associated with this model.
                selection.model = this;

                return selection;
            }
        }

        /// <summary>
        /// Gets custom selection of categories which should be used to filter
        /// brushes when <see cref="CategoryFiltering"/> is set to a value of <see cref="Editor.CategoryFiltering.CustomSelection"/>.
        /// </summary>
        /// <example>
        /// <para>Show all brushes from categories 7 and 8:</para>
        /// <code language="csharp"><![CDATA[
        /// model.CategoryFiltering = CategoryFiltering.CustomSelection;
        /// model.CategorySelection.Add(7);
        /// model.CategorySelection.Add(8);
        /// ]]></code>
        /// </example>
        /// <seealso cref="CategoryFiltering"/>
        /// <seealso cref="CategorySelection"/>
        public ICollection<int> CustomCategorySelection {
            get {
                // Ensure that custom category selection set exists.
                var selection = this.customCategorySelection;
                if (selection == null) {
                    this.customCategorySelection = selection = new CategorySet();
                }

                // Ensure that selection is associated with this model.
                selection.model = this;

                return selection;
            }
        }

        /// <summary>
        /// Set category selection.
        /// </summary>
        /// <param name="categoryNumber">Number of category to select or a value of zero for uncategorized.</param>
        public void SetCategorySelection(int categoryNumber)
        {
            this.CategoryFiltering = Editor.CategoryFiltering.Selection;
            this.CategorySelection.Clear();
            this.CategorySelection.Add(categoryNumber);
        }

        /// <summary>
        /// Gets description for filter label.
        /// </summary>
        public string FilterDescription { get; private set; }

        #endregion


        #region Brush Records

        private List<BrushAssetRecord> records;
        private ReadOnlyCollection<BrushAssetRecord> recordsReadOnly;
        private long databaseVersion;


        /// <summary>
        /// Gets list of brush records.
        /// </summary>
        /// <remarks>
        /// <para>Brushes are automatically queried when changes are made to the filtering
        /// properties exposed by this class.</para>
        /// </remarks>
        public ReadOnlyCollection<BrushAssetRecord> Records {
            get {
                if (this.records == null || this.HasQueryChanged || this.databaseVersion != BrushDatabase.s_TimeLastUpdated) {
                    this.databaseVersion = BrushDatabase.s_TimeLastUpdated;

                    if (this.records == null) {
                        this.records = new List<BrushAssetRecord>(BrushDatabase.Instance.BrushRecords.Count);
                        this.recordsReadOnly = new ReadOnlyCollection<BrushAssetRecord>(this.records);
                    }

                    this.ApplyFilter(this.records);
                }
                return this.recordsReadOnly;
            }
        }

        /// <summary>
        /// Finds zero-based index of record for specified brush.
        /// </summary>
        /// <param name="brush">Brush for which to locate record of.</param>
        /// <returns>
        /// Zero-based index of brush record; otherwise a value of <c>-1</c> if not found.
        /// </returns>
        public int IndexOfRecord(Brush brush)
        {
            for (int i = 0; i < this.records.Count; ++i) {
                if (this.records[i].Brush == brush) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Applies category filtering to brush.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// A value of <c>true</c> if brush is visible; otherwise a value of <c>false</c>.
        /// </returns>
        public bool ApplyCategoryFilter(Brush brush)
        {
            switch (this.CategoryFiltering) {
                default:
                case Editor.CategoryFiltering.None:
                    return true;
                case Editor.CategoryFiltering.Selection:
                    return this.CategorySelection.Contains(brush.CategoryId);
                case Editor.CategoryFiltering.CustomSelection:
                    return this.CustomCategorySelection.Contains(brush.CategoryId);
            }
        }

        /// <summary>
        /// Apply filtering to brush database and output brushes to specified list.
        /// </summary>
        /// <param name="output">List will be replaced with records of filtered brushes.</param>
        public void ApplyFilter(List<BrushAssetRecord> output)
        {
            this.HasQueryChanged = false;

            this.UpdateFilterDescription();
            var records = BrushDatabase.Instance.BrushRecords;

            // Prepare filtered list for first time?
            output.Clear();

            bool applyNameFilter = (this.searchFilterText != "");
            bool isTilesetView = (this.View == BrushListView.Tileset);
            bool isMasterView = (this.View == BrushListView.Master);

            // Clear value of selected tileset if it appears to have been discarded
            // Remember, Unity overrides the `==` operator :/
            if (this.selectedTileset == null) {
                this.selectedTileset = null;
            }

            if (isTilesetView) {
                var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(this.selectedTileset);
                if (tilesetRecord == null) {
                    return;
                }

                records = tilesetRecord.BrushRecords;
            }

            foreach (var brushRecord in records) {
                if (brushRecord.Brush == null) {
                    continue;
                }

                if (brushRecord.IsMaster && !(isMasterView || (isTilesetView && brushRecord.Brush is TilesetBrush))) {
                    continue;
                }
                if ((brushRecord.IsHidden && !this.showHidden) || (this.filterFavorite && !brushRecord.IsFavorite)) {
                    continue;
                }

                // Skip alias brushes?
                if (this.hideAliasBrushes && brushRecord.Brush is AliasBrush) {
                    continue;
                }

                // Skip tileset brushes?
                bool isTilesetBrush = ReferenceEquals(brushRecord.Brush.GetType(), typeof(TilesetBrush));
                if ((this.hideTilesetBrushes || (this.filterHideTilesetBrushes && (!brushRecord.IsFavorite || !this.filterAlwaysShowFavorite))) && !isTilesetView && isTilesetBrush) {
                    continue;
                }

                // Only show master brushes in master brush view!
                if (isMasterView && !brushRecord.IsMaster) {
                    continue;
                }

                // Apply category filtering
                if (!this.ApplyCategoryFilter(brushRecord.Brush)) {
                    continue;
                }

                // Filter brush by name.
                if (applyNameFilter && brushRecord.DisplayName.IndexOf(this.searchFilterText, StringComparison.OrdinalIgnoreCase) == -1) {
                    continue;
                }

                // We want to keep this brush!
                output.Add(brushRecord);
            }
        }

        /// <summary>
        /// Update filter description from filtering properties.
        /// </summary>
        private void UpdateFilterDescription()
        {
            var filters = new List<string>();

            switch (this.CategoryFiltering) {
                default:
                case CategoryFiltering.None:
                    break;

                case CategoryFiltering.Selection:
                    if (this.CategorySelection.Count == 1) {
                        filters.Add(ProjectSettings.Instance.GetCategoryLabel(this.CategorySelection.First()));
                    }
                    else {
                        filters.Add(TileLang.Text("Multiple Categories"));
                    }
                    break;

                case CategoryFiltering.CustomSelection:
                    filters.Add(TileLang.Text("Custom"));
                    break;
            }

            if (this.filterFavorite) {
                filters.Add(TileLang.Text("Favorite"));
            }

            this.FilterDescription = filters.Count != 0
                ? string.Format(
                    /* 0: current filter description */
                    TileLang.Text("Filter: {0}"),
                    string.Join("; ", filters.ToArray())
                )
                : "";
        }

        #endregion


        #region Editor Settings

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.selectedBrush = EditorUtility.InstanceIDToObject(this.selectedBrushInstanceID) as Brush;
            this.selectedBrushSecondary = EditorUtility.InstanceIDToObject(this.selectedBrushSecondaryInstanceID) as Brush;
            this.selectedTileset = EditorUtility.InstanceIDToObject(this.selectedTilesetInstanceID) as Tileset;

            this.isDirty = false;
        }

        #endregion


        #region IDirtyableObject Members

        private bool isDirty;

        bool IDirtyableObject.IsDirty {
            get { return this.isDirty; }
        }

        void IDirtyableObject.MarkClean()
        {
            this.isDirty = false;
        }

        #endregion
    }
}
