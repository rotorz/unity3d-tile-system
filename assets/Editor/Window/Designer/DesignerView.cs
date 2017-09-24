// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Base class for designer view.
    /// </summary>
    /// <remarks>
    /// <para>This class is intended for internal use. If you are looking to create a
    /// custom brush designer then please see <see cref="BrushDesignerView"/> instead.</para>
    /// </remarks>
    public abstract class DesignerView
    {
        internal const float ExtendedPropertiesPanelWidth = 270;


        #region User Settings

        static DesignerView()
        {
            var settings = AssetSettingManagement.GetGroup("Designer");

            s_setting_DisplayExtendedProperties = settings.Fetch<bool>("DisplayExtendedProperties", false);
        }


        private static readonly Setting<bool> s_setting_DisplayExtendedProperties;


        /// <summary>
        /// Gets or sets whether extended properties should be displayed.
        /// </summary>
        public static bool DisplayExtendedProperties {
            get { return s_setting_DisplayExtendedProperties; }
            set { s_setting_DisplayExtendedProperties.Value = value; }
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets parent window of brush designer.
        /// </summary>
        /// <remarks>
        /// <para>May be <c>null</c> when not associated with a window.</para>
        /// </remarks>
        public DesignerWindow Window { get; set; }

        /// <summary>
        /// Position of designer view.
        /// </summary>
        public Rect viewPosition;
        /// <summary>
        /// Scrolling offset for designer GUI.
        /// </summary>
        protected internal Vector2 viewScrollPosition;

        /// <summary>
        /// Gets a value indicating whether designer view has extended properties.
        /// </summary>
        public virtual bool HasExtendedProperties {
            get { return true; }
        }

        #endregion


        #region History States

        /// <summary>
        /// Represents a state in history for designer selection.
        /// </summary>
        /// <remarks>
        /// <para>Custom history states can be implemented to better preserve the state of
        /// customer designer views if needed; otherwise the default implementation can be
        /// assumed.</para>
        /// </remarks>
        public class HistoryState : HistoryManager.State
        {
            /// <summary>
            /// List of unique section names that are expanded.
            /// </summary>
            private List<string> expandedSections = new List<string>();


            /// <summary>
            /// Initialize a new instance of the <see cref="HistoryState"/> class.
            /// </summary>
            /// <param name="selected">Selected designable object.</param>
            public HistoryState(IDesignableObject selected)
                : base(selected)
            {
            }


            /// <summary>
            /// Gets or sets scroll position of main designer view.
            /// </summary>
            public Vector2 ScrollPosition { get; set; }
            /// <summary>
            /// Gets or sets scroll position of extended properties panel.
            /// </summary>
            public Vector2 ExtendedPropertiesScrollPosition { get; set; }


            /// <summary>
            /// Set state of expanded section.
            /// </summary>
            /// <example>
            /// <para>It is advised to add a unique vendor specific prefix to the section
            /// name to avoid conflicts in the future.</para>
            /// <code language="csharp"><![CDATA[
            /// historyState.SetExpandedSectionState("MyCustomSection", true);
            /// ]]></code>
            /// </example>
            /// <param name="name">Unique name of section.</param>
            /// <param name="expanded">Indicates whether section is expanded or not.</param>
            public void SetExpandedSectionState(string name, bool expanded)
            {
                if (expanded) {
                    if (!this.expandedSections.Contains(name)) {
                        this.expandedSections.Add(name);
                    }
                }
                else if (this.expandedSections.Contains(name)) {
                    this.expandedSections.Remove(name);
                }
            }

            /// <summary>
            /// Gets state of expanded section.
            /// </summary>
            /// <param name="name">Unique name of section.</param>
            /// <returns>
            /// A value of <c>true</c> if section is expanded; otherwise <c>false</c>.
            /// </returns>
            public bool GetExpandedSectionState(string name)
            {
                return this.expandedSections.Contains(name);
            }

        }


        /// <summary>
        /// Create state for selection history.
        /// </summary>
        /// <para>A designer view can subclass <see cref="HistoryManager.State"/> so that
        /// additional data can be persisted into the selection history state. It is important
        /// that only user interface data is persisted.</para>
        /// <para>Return a value of <c>null</c> to prevent selection state from being placed
        /// into selection history.</para>
        /// <example>
        /// <para>The following source code demonstrates how to create a custom selection
        /// history state:</para>
        /// <code language="csharp"><![CDATA[
        /// public override SelectionHistory.State CreateSelectionHistoryState()
        /// {
        ///     return new CustomHistoryState();
        /// }
        /// ]]></code>
        /// </example>
        /// <returns>
        /// Selection history state.
        /// </returns>
        /// <seealso cref="UpdateHistoryState"/>
        /// <seealso cref="RestoreHistoryState"/>
        public virtual HistoryManager.State CreateHistoryState()
        {
            return new HistoryState(this.Window.SelectedObject);
        }

        /// <summary>
        /// Persist state of user interface for selection history.
        /// </summary>
        /// <remarks>
        /// <para>Base functionality should be invoked when method is overridden.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source code demonstrates how to persist additional data:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void PersistSelectionHistoryState(SelectionHistory.State state)
        /// {
        ///     base.PersistSelectionHistoryState(state);
        ///
        ///     CustomHistoryState customState = (CustomHistoryState)state;
        ///     customState.ShowCustomToolbar = this.ShowCustomToolbar;
        /// }]]>
        /// </code>
        /// </example>
        /// <param name="state">Selection history state.</param>
        /// <seealso cref="CreateHistoryState"/>
        /// <seealso cref="RestoreHistoryState"/>
        public virtual void UpdateHistoryState(HistoryManager.State state)
        {
            var historyState = state as HistoryState;
            if (historyState == null) {
                return;
            }

            historyState.ScrollPosition = this.viewScrollPosition;
            historyState.ExtendedPropertiesScrollPosition = this.scrollingExtendedProperties;
        }

        /// <summary>
        /// Restore state from selection history.
        /// </summary>
        /// <remarks>
        /// <para>Base functionality should be invoked when method is overridden.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source code demonstrates how to restore additional data:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void RestoreSelectionHistoryState(SelectionHistory.State state)
        /// {
        ///     base.RestoreSelectionHistoryState(state);
        ///
        ///     CustomHistoryState customState = (CustomHistoryState)state;
        ///     this.ShowCustomToolbar = customState.ShowCustomToolbar;
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="state">Selection history state.</param>
        /// <seealso cref="CreateHistoryState"/>
        /// <seealso cref="UpdateHistoryState"/>
        public virtual void RestoreHistoryState(HistoryManager.State state)
        {
            var historyState = state as HistoryState;
            if (historyState == null) {
                return;
            }

            this.viewScrollPosition = historyState.ScrollPosition;
            this.scrollingExtendedProperties = historyState.ExtendedPropertiesScrollPosition;
        }

        #endregion


        #region Messages and Events

        /// <summary>
        /// Occurs when designer view is initialized for first time.
        /// </summary>
        /// <remarks>
        /// <para>It is important that base functionality is inherited when overriding.</para>
        /// </remarks>
        /// <example>
        /// <para>In the following example we cast brush to a more suitable form:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomBrushDesigner : BrushDesigner
        /// {
        ///     private CustomTileBrush customBrush;
        ///
        ///
        ///     public override void OnEnable()
        ///     {
        ///         base.OnEnable();
        ///         this.customBrush = brush as CustomTileBrush;
        ///     }
        ///
        ///
        ///     // Remainder of implementation...
        /// }
        /// ]]></code>
        /// </example>
        public virtual void OnEnable()
        {
        }

        /// <summary>
        /// Occurs when designer view is no longer required.
        /// </summary>
        /// <remarks>
        /// <para>It is important that base functionality is inherited when overriding.</para>
        /// </remarks>
        public virtual void OnDisable()
        {
        }

        /// <summary>
        /// Add items to designer menu.
        /// </summary>
        /// <param name="menu">The menu.</param>
        public virtual void AddItemsToMenu(EditorMenu menu)
        {
        }

        /// <summary>
        /// Occurs when header GUI is rendered and for GUI event handling.
        /// </summary>
        /// <remarks>
        /// <para>This means that your <see cref="OnFixedHeaderGUI"/> implementation might
        /// be called several times per frame (one call per event).</para>
        /// <para>Custom designer windows should not place controls within the area described
        /// by the following rectangle to avoid overlapping the selection history navigation
        /// buttons:</para>
        /// <code language="csharp"><![CDATA[
        /// Rect menuArea = new Rect(0, 0, 93, 28);
        /// ]]></code>
        /// </remarks>
        public virtual void OnFixedHeaderGUI()
        {
        }

        /// <summary>
        /// Occurs when rendering and handling GUI events of designer.
        /// </summary>
        /// <remarks>
        /// <para>This means that your <see cref="OnGUI"/> implementation might
        /// be called several times per frame (one call per event).</para>
        /// </remarks>
        /// <example>
        /// <para>Simple brush designer with material mappings:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomBrushDesigner : BrushDesigner
        /// {
        ///     private GameObject addGO;
        ///
        ///
        ///     public override void OnGUI()
        ///     {
        ///         // Important controls do not need to be placed into foldout so
        ///         // as to ensure that they are easily accessible.
        ///         this.addGO = EditorGUILayout.ObjectField("Add Object", this.addGO, typeof(GameObject), false);
        ///         if (GUILayout.Button("Add")) {
        ///             // Do something!
        ///         }
        ///
        ///         // Remaining controls should be placed into foldouts.
        ///         this.Section_MaterialMapper();
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public virtual void OnGUI()
        {
        }

        /// <summary>
        /// Occurs when rendering and handling GUI events of extended properties.
        /// </summary>
        /// <remarks>
        /// <para>This means that your <see cref="OnExtendedPropertiesGUI"/> implementation
        /// might be called several times per frame (one call per event).</para>
        /// </remarks>
        public virtual void OnExtendedPropertiesGUI()
        {
        }

        #endregion


        #region Menu Button

        /// <summary>
        /// Use to draw menu button somewhere within designer window.
        /// </summary>
        /// <remarks>
        /// <para>Custom menu items can be added to menu by providing a custom implementation
        /// of <see cref="AddItemsToMenu"/>.</para>
        /// </remarks>
        /// <param name="position">Position of button in space of editor window.</param>
        /// <param name="tooltip">Tooltip text.</param>
        protected void DrawMenuButton(Rect position, string tooltip = null)
        {
            using (var content = ControlContent.Basic(RotorzEditorStyles.Skin.GearButton, tooltip)) {
                if (EditorInternalUtility.DropdownMenu(position, content, RotorzEditorStyles.Instance.FlatButton)) {
                    this.DisplayMenu(new Rect(position.x, position.y, position.width, position.height - 1));
                    GUIUtility.ExitGUI();
                }
            }
        }

        /// <summary>
        /// Prepare and display drop down menu.
        /// </summary>
        /// <param name="position">Position of menu button.</param>
        private void DisplayMenu(Rect position)
        {
            var menu = new EditorMenu();
            this.AddItemsToMenu(menu);
            menu.ShowAsDropdown(position);
        }

        #endregion


        #region Methods

        private static bool s_HasVerticalScrollBar;

        internal virtual void Draw()
        {
            GUILayout.Space(3);
            this.viewPosition.width -= 3;

            // Remove size of extended properties panel.
            if (this.HasExtendedProperties) {
                this.viewPosition.width -= DisplayExtendedProperties
                    ? ExtendedPropertiesPanelWidth
                    : RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden.fixedWidth;
            }

            // Apply scrollbars to designer position.
            GUISkin skin = GUI.skin;
            if (s_HasVerticalScrollBar) {
                this.viewPosition.width -= skin.verticalScrollbar.fixedWidth;
            }
            this.viewPosition.height -= skin.horizontalScrollbar.fixedHeight;

            this.viewScrollPosition = EditorGUILayout.BeginScrollView(this.viewScrollPosition);
            {
                GUILayout.BeginVertical();
                GUILayout.Space(2);
                this.OnGUI();
                GUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint) {
                    Rect contentRect = GUILayoutUtility.GetLastRect();

                    // Determine which scrollbars are now visible.
                    bool verticalScrollBar = contentRect.height > this.viewPosition.height + 2;

                    // Refresh!
                    if (verticalScrollBar != s_HasVerticalScrollBar) {
                        s_HasVerticalScrollBar = verticalScrollBar;
                        this.Repaint();
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(1);

            if (this.HasExtendedProperties) {
                this.DrawExtendedPropertiesGUI();
            }
        }

        private Vector2 scrollingExtendedProperties;

        internal void DrawExtendedPropertiesGUI()
        {
            float leaderSpace = -1;

            string extendedPropertiesLabel = TileLang.ParticularText("Section", "Extended Properties");

            if (DisplayExtendedProperties) {
                // Display expanded "Extended Properties" panel.
                Rect hsplitPos = EditorGUILayout.BeginVertical(GUILayout.Width(ExtendedPropertiesPanelWidth));

                GUILayout.Space(leaderSpace);

                DisplayExtendedProperties = GUILayout.Toggle(true, extendedPropertiesLabel, RotorzEditorStyles.Instance.ExtendedProperties_TitleShown);

                this.scrollingExtendedProperties = EditorGUILayout.BeginScrollView(this.scrollingExtendedProperties, RotorzEditorStyles.Instance.ExtendedProperties_ScrollView);
                {
                    float restoreLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 105;

                    GUILayout.BeginVertical(RotorzEditorStyles.Instance.ExtendedPropertiesLeader);
                    {
                        this.BeginChangeCheck();
                        this.BeginExtendedProperties();
                        ExtraEditorGUI.SeparatorLight();
                        this.OnExtendedPropertiesGUI();
                    }
                    GUILayout.EndVertical();

                    this.EndExtendedProperties();
                    this.EndChangeCheck();

                    EditorGUIUtility.labelWidth = restoreLabelWidth;
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();

                hsplitPos.y += 15;
                hsplitPos.height -= 15;

                hsplitPos.width = 6;
                GUI.Box(hsplitPos, GUIContent.none, RotorzEditorStyles.Instance.ExtendedProperties_HSplit);
            }
            else {
                // Display collapsed "Extended Properties" panel.
                GUILayout.BeginVertical(GUILayout.Width(RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden.fixedWidth));
                GUILayout.Space(leaderSpace);
                DisplayExtendedProperties = GUILayout.Button(GUIContent.none, RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden);

                Rect position = GUILayoutUtility.GetLastRect();
                position.x += RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden.padding.left;
                position.y += RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden.padding.top;
                position.width -= RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden.padding.horizontal;
                position.height -= RotorzEditorStyles.Instance.ExtendedProperties_TitleHidden.padding.vertical;
                RotorzEditorGUI.VerticalLabel(position, extendedPropertiesLabel, EditorStyles.whiteLabel);

                GUILayout.Space(4);
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Gets a value indicating whether view is valid.
        /// </summary>
        protected internal abstract bool IsValid { get; }

        /// <summary>
        /// Begin view GUI.
        /// </summary>
        protected internal virtual void BeginView()
        {
        }

        /// <summary>
        /// End view GUI.
        /// </summary>
        protected internal virtual void EndView()
        {
        }

        /// <summary>
        /// Begin extended properties GUI.
        /// </summary>
        /// <remarks>
        /// <para>Occurs when rendering and handling GUI events of extended properties. This
        /// means that your <see cref="BeginExtendedProperties"/> implementation might be
        /// called several times per frame (one call per event).</para>
        /// </remarks>
        protected internal virtual void BeginExtendedProperties()
        {
        }

        /// <summary>
        /// End extended properties GUI.
        /// </summary>
        /// <remarks>
        /// <para>Occurs when rendering and handling GUI events of extended properties. This
        /// means that your <see cref="EndExtendedProperties"/> implementation might be
        /// called several times per frame (one call per event).</para>
        /// </remarks>
        protected internal virtual void EndExtendedProperties()
        {
        }

        /// <summary>
        /// Begin checking for changes to input controls.
        /// </summary>
        /// <remarks>
        /// <para>Marks beginning of change checker so that brush can be marked as
        /// dirty when changes are detected by the accompanying call of <see cref="EndChangeCheck"/>.</para>
        /// <para>Not all changes can be detected and in such circumstances <see cref="SetDirty"/>
        /// should be used. Please refer to Unity documentation (lookup <c>EditorGUI.BeginChangeCheck</c>
        /// for a better understanding of how change checking works.</para>
        /// </remarks>
        /// <example>
        /// <para>Checking for changes:</para>
        /// <code language="csharp"><![CDATA[
        /// this.BeginChangeCheck();
        ///
        /// brush.someProperty = EditorGUILayout.IntField("Some Property", brush.someProperty);
        ///
        /// this.EndChangeCheck();
        /// ]]></code>
        /// </example>
        /// <seealso cref="EndChangeCheck"/>
        /// <seealso cref="SetDirty"/>
        protected void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }

        /// <summary>
        /// End checking for change to input controls.
        /// </summary>
        /// <remarks>
        /// <para>Marks end of change checker so that brush can be automatically marked
        /// as dirty when changes are detected.</para>
        /// <para>Not all changes can be detected and in such circumstances <see cref="SetDirty"/>
        /// should be used. Please refer to Unity documentation (lookup <c>EditorGUI.EndChangeCheck</c>
        /// for a better understanding of how change checking works).</para>
        /// </remarks>
        /// <seealso cref="BeginChangeCheck"/>
        /// <seealso cref="SetDirty"/>
        /// <returns>
        /// A value of <c>true</c> if changes were detected; otherwise <c>false</c>.
        /// </returns>
        protected bool EndChangeCheck()
        {
            if (EditorGUI.EndChangeCheck()) {
                this.SetDirty();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set target asset as dirty so that Unity can save changes.
        /// </summary>
        public abstract void SetDirty();

        /// <summary>
        /// Foldout title that ignores effect of horizontal scrolling.
        /// </summary>
        /// <remarks>
        /// <para>Designed for use within context of <see cref="OnGUI"/> only.</para>
        /// </remarks>
        /// <param name="foldout">Current state of foldout.</param>
        /// <param name="label">Label of foldout header.</param>
        /// <returns>
        /// A value of <c>true</c> when foldout is expanded; otherwise <c>false</c>.
        /// </returns>
        protected bool FixedTitleFoldout(bool foldout, string label)
        {
            Rect position = GUILayoutUtility.GetRect(0, RotorzEditorStyles.Instance.FoldoutTitle.fixedHeight, RotorzEditorStyles.Instance.FoldoutTitle);
            position.x += this.viewScrollPosition.x;
            position.width = this.viewPosition.width;
            return GUI.Toggle(position, foldout, label, RotorzEditorStyles.Instance.FoldoutTitle);
        }

        /// <summary>
        /// Marks start of fixed section that ignores effect of horizontal scrolling.
        /// </summary>
        /// <remarks>
        /// <para>Fixed sections are not suitable when horizontal scrolling is
        /// required within section itself. The "Material Mapper" is an example
        /// of a fixed section because it does not respond to horizontal scrolling:</para>
        /// <para><img src="../art/material-mapper-fixed-section.png" alt="Material Mapper: Example of fixed section"/></para>
        /// </remarks>
        /// <seealso cref="EndFixedSection"/>
        protected void BeginFixedSection()
        {
            GUILayout.BeginHorizontal();

            // Offset section content by scroll offset to give the illusion
            // that the content is fixed.
            GUILayout.Space(this.viewScrollPosition.x);

            GUILayout.BeginVertical(GUILayout.Width(this.viewPosition.width));
        }

        /// <summary>
        /// Marks end of fixed section.
        /// </summary>
        /// <seealso cref="BeginFixedSection"/>
        protected void EndFixedSection()
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Focus editor window.
        /// </summary>
        protected void Focus()
        {
            if (this.Window != null) {
                this.Window.Focus();
            }
        }

        /// <summary>
        /// Repaint editor window.
        /// </summary>
        protected void Repaint()
        {
            if (this.Window != null) {
                this.Window.Repaint();
            }
        }

        #endregion
    }
}
