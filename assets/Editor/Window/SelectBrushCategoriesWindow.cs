// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Brush category selection delegate.
    /// </summary>
    /// <param name="categories">
    /// Collection of category numbers indicating new category selection.
    /// </param>
    public delegate void BrushCategorySelected(ICollection<int> categories);


    /// <summary>
    /// Brush category selection window.
    /// </summary>
    /// <remarks>
    /// <para>Use to select one or more brush categories.</para>
    /// </remarks>
    public sealed class SelectBrushCategoriesWindow : RotorzWindow
    {
        #region Window Management

        /// <summary>
        /// Display brush category selection window.
        /// </summary>
        /// <param name="callback">Callback is invoked when a brush category is selected.</param>
        /// <param name="categories">Initial selection of brush categories.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static SelectBrushCategoriesWindow ShowWindow(BrushCategorySelected callback, ICollection<int> categories)
        {
            var window = GetUtilityWindow<SelectBrushCategoriesWindow>();

            window.CategorySelection = new HashSet<int>(categories);
            window.OnBrushCategorySelected += callback;

            window.ShowAuxWindow();

            return window;
        }

        #endregion


        /// <summary>
        /// Occurs when brush category was selected.
        /// </summary>
        public event BrushCategorySelected OnBrushCategorySelected;


        /// <summary>
        /// Gets collection of selected category numbers.
        /// </summary>
        public ICollection<int> CategorySelection { get; private set; }

        private Vector2 scrollPosition;

        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(TileLang.ParticularText("Action", "Select Brush Categories"));
            this.InitialSize = new Vector2(400, 320);
            this.minSize = new Vector2(360, 230);
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            this.OnGUI_BrushCategoryList();
            this.OnGUI_Buttons();
            GUILayout.EndHorizontal();
        }

        private void OnGUI_BrushCategoryList()
        {
            var projectSettings = ProjectSettings.Instance;

            GUILayout.BeginVertical();

            int[] categoryIds = projectSettings.CategoryIds;
            string[] categoryLabels = projectSettings.CategoryLabels;

            GUILayout.BeginVertical(GUI.skin.box);
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            // Enumerate brush categories.
            for (int i = 0, count = categoryLabels.Length; i < count; ++i) {
                int categoryNumber = categoryIds[i];
                if (GUILayout.Toggle(this.CategorySelection.Contains(categoryNumber), categoryLabels[i])) {
                    this.CategorySelection.Add(categoryNumber);
                }
                else {
                    this.CategorySelection.Remove(categoryNumber);
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            this.OnGUI_ListButtons();

            GUILayout.EndVertical();
        }

        private void OnGUI_ListButtons()
        {
            var projectSettings = ProjectSettings.Instance;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(TileLang.ParticularText("Action|Select", "All"), ExtraEditorStyles.Instance.BigButton)) {
                this.CategorySelection.Clear();
                foreach (int number in projectSettings.CategoryIds) {
                    this.CategorySelection.Add(number);
                }
            }
            if (GUILayout.Button(TileLang.ParticularText("Action|Select", "None"), ExtraEditorStyles.Instance.BigButton)) {
                this.CategorySelection.Clear();
            }
            if (GUILayout.Button(TileLang.ParticularText("Action|Select", "Invert"), ExtraEditorStyles.Instance.BigButton)) {
                var invertedSelection = projectSettings.CategoryIds
                    .Where(number => !this.CategorySelection.Contains(number));
                this.CategorySelection = new HashSet<int>(invertedSelection);
            }

            GUILayout.EndHorizontal();
        }

        private void OnGUI_Buttons()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button(TileLang.ParticularText("Action", "OK"), ExtraEditorStyles.Instance.BigButton)) {
                if (this.OnBrushCategorySelected != null) {
                    this.OnBrushCategorySelected(new HashSet<int>(this.CategorySelection));
                }
                this.Close();
                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButton)) {
                this.Close();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndVertical();
        }
    }
}
