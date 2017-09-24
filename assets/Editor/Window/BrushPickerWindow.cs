// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using System;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Brush selection window.
    /// </summary>
    /// <remarks>
    /// <para>This window can be used to integrate brush selection into a custom editor GUI.</para>
    /// </remarks>
    internal sealed class BrushPickerWindow : RotorzWindow
    {
        #region Window Management

        /// <summary>
        /// Gets active instance of window; otherwise <c>null</c> if window is not active.
        /// </summary>
        internal static BrushPickerWindow Instance { get; private set; }

        /// <summary>
        /// Indicates whether search field should be focused.
        /// </summary>
        private static bool s_FocusSearchField;


        internal static void ShowWindow(Brush brush, bool allowAlias, bool allowMaster)
        {
            s_RestoreFocusWindow = EditorWindow.focusedWindow;

            // Show window but only adjust size and position first time window is shown.
            Instance = GetUtilityWindow<BrushPickerWindow>();

            var model = Instance.brushList.Model;
            model.HideAliasBrushes = !allowAlias;
            model.SelectedBrush = brush;

            Instance.brushList.VisibleViews = allowMaster
                ? BrushListView.Brushes | BrushListView.Tileset | BrushListView.Master
                : BrushListView.Brushes | BrushListView.Tileset;

            Instance.brushList.RevealBrush(brush);

            Instance.ShowAuxWindow();

            // Indicate that search field should become focused.
            s_FocusSearchField = true;
        }

        #endregion


        [NonSerialized]
        private BrushListControl brushList;


        /// <summary>
        /// Gets or sets selected brush.
        /// </summary>
        internal Brush SelectedBrush {
            get { return this.brushList.Model.SelectedBrush; }
            set { this.brushList.Model.SelectedBrush = value; }
        }

        // the window for which to restore focus to.
        private static EditorWindow s_RestoreFocusWindow;

        private static readonly Setting<BrushListModel> s_BrushPickerModel
            = AssetSettingManagement.GetGroup("BrushPickerWindow")
                .Fetch<BrushListModel>("BrushListModel", null,
                    filter: (value) => {
                        if (value == null)
                            value = new BrushListModel();
                        return value;
                    }
                );

        /// <inheritdoc/>
        protected override void DoEnable()
        {
            Instance = this;

            this.titleContent = new GUIContent(TileLang.ParticularText("Action", "Select Brush"));
            this.InitialSize = new Vector2(320, 463);
            this.minSize = new Vector2(320, 463 - 174);

            this.brushList = new BrushListControl(this);
            this.brushList.Model = s_BrushPickerModel;
            this.brushList.Model.SelectedBrushChanged += this._brushList_Model_SelectedBrushChanged;
            this.brushList.EmptyLabel = TileLang.Text("(None)");
            this.brushList.ShowViewTabs = true;
            this.brushList.BrushMouseDown += this._brushList_BrushMouseDown;
            this.brushList.BrushClicked += this._brushList_BrushClicked;
        }

        /// <inheritdoc/>
        protected override void DoDisable()
        {
            if (s_RestoreFocusWindow != null) {
                s_RestoreFocusWindow.Focus();

                var commandEvent = EditorGUIUtility.CommandEvent("Rotorz.TileSystem.BrushPickerClosed");
                s_RestoreFocusWindow.SendEvent(commandEvent);

                s_RestoreFocusWindow = null;
            }

            // Disconnect from brush field.
            RotorzEditorGUI.BrushPickerControlID = 0;
        }

        /// <inheritdoc/>
        protected override void DoDestroy()
        {
            Instance = null;

            // Remove event handlers to avoid memory leak.
            this.brushList.Model.SelectedBrushChanged -= this._brushList_Model_SelectedBrushChanged;
            this.brushList.BrushMouseDown -= this._brushList_BrushMouseDown;
            this.brushList.BrushClicked -= this._brushList_BrushClicked;
        }

        private void _brushList_BrushMouseDown(Brush brush)
        {
            // User has double clicked on list?
            if (Event.current.clickCount == 2) {
                Event.current.Use();
                GUIUtility.hotControl = 0;

                this.Close();
                GUIUtility.ExitGUI();
            }
        }

        private void _brushList_BrushClicked(Brush brush)
        {
            Event.current.Use();

            if (brush != this.brushList.Model.SelectedBrush) {
                this.brushList.ScrollToBrush(brush);

                // Brush selection changed.
                this.brushList.Model.SelectedBrush = brush;
                this.UpdateBrushSelection(brush);
                GUIUtility.ExitGUI();
            }
        }

        private void _brushList_Model_SelectedBrushChanged(Brush preview, Brush current)
        {
            this.Repaint();
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            this.DoKeyboardInput();

            GUILayout.BeginHorizontal();

            this.brushList.Draw();

            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            RotorzEditorGUI.DrawHoverTip(this);

            if (s_FocusSearchField) {
                EditorGUI.FocusTextInControl("SearchFilter");
                s_FocusSearchField = false;
            }
        }

        private void DoKeyboardInput()
        {
            // Allow keyboard input to override that of the search field.

            if (Event.current.type == EventType.KeyDown) {
                var model = this.brushList.Model;

                int selectedIndex = model.SelectedBrush != null
                    ? model.IndexOfRecord(model.SelectedBrush)
                    : -1;
                int newSelectedIndex = selectedIndex;

                switch (Event.current.keyCode) {
                    case KeyCode.UpArrow:
                        newSelectedIndex -= this.brushList.Columns;
                        Event.current.Use();
                        break;
                    case KeyCode.DownArrow:
                        newSelectedIndex += selectedIndex == -1 ? 1 : this.brushList.Columns;
                        Event.current.Use();
                        break;
                    case KeyCode.LeftArrow:
                        --newSelectedIndex;
                        Event.current.Use();
                        break;
                    case KeyCode.RightArrow:
                        ++newSelectedIndex;
                        Event.current.Use();
                        break;

                    case KeyCode.Home:
                        newSelectedIndex = selectedIndex == 0 ? -1 : 0;
                        Event.current.Use();
                        break;
                    case KeyCode.End:
                        newSelectedIndex = model.Records.Count;
                        Event.current.Use();
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        Event.current.Use();
                        this.Close();
                        GUIUtility.ExitGUI();
                        break;
                }

                // Ensure that selected index stays within range!
                newSelectedIndex = Mathf.Clamp(newSelectedIndex, string.IsNullOrEmpty(this.brushList.EmptyLabel) ? 0 : -1, model.Records.Count - 1);

                // Has brush selection changed?
                if (newSelectedIndex != selectedIndex) {
                    Brush newBrushSelection = newSelectedIndex != -1
                        ? model.Records[newSelectedIndex].Brush
                        : null;

                    // Update brush selection and make sure that it's visible.
                    model.SelectedBrush = newBrushSelection;
                    this.brushList.ScrollToBrush(newBrushSelection);

                    this.UpdateBrushSelection(newBrushSelection);
                }
            }
        }

        private void UpdateBrushSelection(Brush brush)
        {
            if (s_RestoreFocusWindow != null) {
                var commandEvent = EditorGUIUtility.CommandEvent("Rotorz.TileSystem.BrushPickerUpdated");
                s_RestoreFocusWindow.SendEvent(commandEvent);
            }
            this.Repaint();
        }
    }
}
