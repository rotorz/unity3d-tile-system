// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Window for creating new brushes and tilesets.
    /// </summary>
    public sealed class CreateBrushWindow : RotorzWindow, IBrushCreatorContext
    {
        /// <summary>
        /// Display the "Create Brush" window.
        /// </summary>
        /// <returns>
        /// The window.
        /// </returns>
        public static CreateBrushWindow ShowWindow()
        {
            return GetUtilityWindow<CreateBrushWindow>();
        }

        /// <summary>
        /// Shows the brush creation window with the specified creator type selected.
        /// </summary>
        /// <remarks>
        /// <para>Assumes first creator tab when the specified type is not registered.</para>
        /// </remarks>
        /// <returns>
        /// The brush creation window.
        /// </returns>
        /// <typeparam name="TCreator">Type of the creator tab to select initially.</typeparam>
        public static CreateBrushWindow ShowWindow<TCreator>()
        {
            var window = ShowWindow();

            window.SelectedBrushCreatorType = typeof(TCreator);

            return window;
        }


        private static GUIStyle s_TitleStyle;
        private static GUIStyle s_TitleHeaderStyle;
        private static GUIStyle s_PaddedView;

        private static void AutoInitCustomStyles()
        {
            if (s_TitleStyle != null) {
                return;
            }

            s_TitleStyle = new GUIStyle(RotorzEditorStyles.Instance.TitleLabel);
            s_TitleStyle.normal.textColor = new Color(0.72f, 0.72f, 0.72f);
            s_TitleStyle.normal.background = RotorzEditorStyles.Skin.TabBackground;
            s_TitleStyle.margin = new RectOffset();
            s_TitleStyle.padding = new RectOffset(7, 7, 10, 5);

            s_TitleHeaderStyle = new GUIStyle();
            s_TitleHeaderStyle.normal.background = RotorzEditorStyles.Skin.TabBackground;
            s_TitleHeaderStyle.stretchWidth = true;

            s_PaddedView = new GUIStyle(GUI.skin.scrollView);
            s_PaddedView.padding = new RectOffset(10, 10, 0, 0);
        }


        private IGrouping<BrushCreatorGroup, BrushCreator>[] brushCreatorGroups = { };
        private int selectedBrushCreatorIndex = -2;
        private BrushCreator selectedBrushCreator;

        private Vector2 scrolling;
        private bool hintShouldFocusPrimaryNameControl = true;


        /// <inheritdoc/>
        public string PrimaryAssetNameControlName {
            get { return "[CreateBrushWindow.PrimaryAssetName]"; }
        }

        /// <inheritdoc/>
        public IDictionary<string, object> SharedProperties { get; private set; }

        /// <summary>
        /// Gets or sets the type of brush creator that is currently selected.
        /// </summary>
        public Type SelectedBrushCreatorType {
            get {
                return this.selectedBrushCreator != null
                    ? this.selectedBrushCreator.GetType()
                    : null;
            }
            set {
                var brushCreators = this.BrushCreatorInstancesOrdered;

                int newSelectedBrushCreatorIndex = brushCreators.FindIndex(x => x.GetType() == value);
                if (newSelectedBrushCreatorIndex == this.selectedBrushCreatorIndex) {
                    return;
                }

                this.selectedBrushCreatorIndex = newSelectedBrushCreatorIndex;

                this.UpdateSelectedBrushCreatorView();
            }
        }

        private List<BrushCreator> BrushCreatorInstancesOrdered {
            get { return this.brushCreatorGroups.SelectMany(x => x).ToList(); }
        }


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(TileLang.ParticularText("Action", "Create Brush / Tileset"));
            this.InitialSize = this.minSize = new Vector2(600, 370);

            this.SharedProperties = new Dictionary<string, object>();

            this.ConstructRegisteredBrushCreators();
            this.UpdateSelectedBrushCreatorView();
        }

        /// <inheritdoc/>
        protected override void DoDisable()
        {
            foreach (var brushCreator in this.brushCreatorGroups.SelectMany(x => x)) {
                brushCreator.OnDisable();
            }
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            AutoInitCustomStyles();

            // Make labels as short as possible to simplify layout within brush creators.
            EditorGUIUtility.labelWidth = 1f;

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(100f));
                {
                    GUILayout.Space(10f);
                    this.DrawBrushCreatorTabs();
                }
                GUILayout.EndVertical();

                GUILayout.Space(-3f);

                GUILayout.BeginVertical();
                {
                    if (this.selectedBrushCreator != null) {
                        this.scrolling = GUILayout.BeginScrollView(this.scrolling);
                        this.DrawSelectedBrushCreator();
                        GUILayout.EndScrollView();

                        ExtraEditorGUI.Separator(marginTop: 0, marginBottom: 7);

                        GUILayout.BeginHorizontal();
                        this.selectedBrushCreator.OnButtonGUI();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5f);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            if (this.hintShouldFocusPrimaryNameControl && Event.current.type == EventType.Layout) {
                this.hintShouldFocusPrimaryNameControl = false;
                EditorGUI.FocusTextInControl(this.PrimaryAssetNameControlName);
            }
        }


        private void DrawBrushCreatorTabs()
        {
            GUI.Box(new Rect(-1f, -1f, 109f, Screen.height + 6f), GUIContent.none);
            GUILayout.Space(30f);

            for (int i = 0; i < this.brushCreatorGroups.Length; ++i) {
                // Draw a separator between each group.
                if (i != 0) {
                    ExtraEditorGUI.SeparatorLight();
                }

                // Draw brush creator tabs in group.
                foreach (var brushCreator in this.brushCreatorGroups[i]) {
                    if (TabButton(brushCreator == this.selectedBrushCreator, brushCreator.Name)) {
                        this.SelectedBrushCreatorType = brushCreator.GetType();
                        this.hintShouldFocusPrimaryNameControl = true;
                    }
                }
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawSelectedBrushCreator()
        {
            // Allow user to use return key to trigger asset creation.
            if (ExtraEditorGUI.AcceptKeyboardReturn()) {
                this.selectedBrushCreator.OnButtonCreate();
                return;
            }

            // Draw creator title.
            Color initialBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            EditorGUILayout.BeginVertical(s_TitleHeaderStyle);
            {
                GUILayout.Box(this.selectedBrushCreator.Title, s_TitleStyle);
            }
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = initialBackgroundColor;


            // Draw creator GUI.
            EditorGUIUtility.wideMode = false;
            GUILayout.BeginVertical(s_PaddedView);
            {
                GUILayout.Space(5);
                this.selectedBrushCreator.OnGUI();
            }
            GUILayout.EndVertical();
        }


        private void UpdateSelectedBrushCreatorView()
        {
            var brushCreators = this.BrushCreatorInstancesOrdered;

            var newSelection = (uint)this.selectedBrushCreatorIndex < brushCreators.Count
                ? brushCreators[this.selectedBrushCreatorIndex]
                : brushCreators.FirstOrDefault();

            // Selection has not changed, bail!
            if (newSelection == this.selectedBrushCreator) {
                return;
            }


            if (Event.current != null) {
                GUIUtility.keyboardControl = 0;
            }


            // Hide the currently selected brush creator.
            if (this.selectedBrushCreator != null) {
                this.selectedBrushCreator.OnHidden();
            }

            // Update the selected brush creator.
            this.selectedBrushCreator = newSelection;

            // Show the brush creator that became selected!
            if (this.selectedBrushCreator != null) {
                this.selectedBrushCreator.OnShown();
            }
        }


        private void ConstructRegisteredBrushCreators()
        {
            var brushCreatorInstances =
                from brushCreatorType in BrushCreator.s_Register
                select this.ConstructBrushCreatorInstance(brushCreatorType);

            var groupedBrushCreators =
                from brushCreatorInstance in brushCreatorInstances
                group brushCreatorInstance by BrushCreatorGroupAttribute.GetAnnotatedGroupOfType(brushCreatorInstance.GetType());

            this.brushCreatorGroups = groupedBrushCreators
                .OrderBy(x => x.Key)
                .ToArray();
        }

        private BrushCreator ConstructBrushCreatorInstance(Type brushCreatorType)
        {
            var constructorSignature = new Type[] { typeof(IBrushCreatorContext) };
            var constructorArgs = new object[] { this };

            return this.ConstructBrushCreatorInstance(brushCreatorType, constructorSignature, constructorArgs);
        }

        private BrushCreator ConstructBrushCreatorInstance(Type brushCreatorType, Type[] constructorSignature, object[] constructorArgs)
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var constructor = brushCreatorType.GetConstructor(bindingAttr, null, constructorSignature, null);

            // Instantiate brush creator using constructor and reflection.
            var brushCreator = constructor.Invoke(constructorArgs) as BrushCreator;
            brushCreator.OnEnable();
            return brushCreator;
        }


        private static bool TabButton(bool active, string label)
        {
            Rect position = GUILayoutUtility.GetRect(110f, 110f, 32f, 32f);
            int controlID = GUIUtility.GetControlID(FocusType.Passive, position);

            position.width -= 3;

            var style = RotorzEditorStyles.Instance.ListSectionElement;

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.Repaint:
                    style.Draw(position, label, false, false, active, false);
                    break;

                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        Event.current.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
