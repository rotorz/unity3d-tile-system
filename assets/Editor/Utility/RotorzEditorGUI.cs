// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Tile.Internal;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Provides additional editor GUI utility functionality.
    /// </summary>
    public static class RotorzEditorGUI
    {
        #region Hover Controls

        internal static int HoverControlID { get; set; }

        #region Hover Tip

        private static Rect s_HoverTipPosition;
        private static bool s_HoverTipReverse;

        private static TextProvider s_HoverTipProvider;
        private static object s_HoverTipContext;
        private static string s_HoverTipText;

        /// <exclude/>
        public static void ClearHoverTip()
        {
            s_HoverTipProvider = null;
            s_HoverTipContext = null;
            s_HoverTipText = null;
        }

        #endregion


        private static void SetHoverControl(int controlID, Rect position, TextProvider tipProvider, object tipContext)
        {
            if (EditorInternalUtility.HoverWindow != EditorWindow.mouseOverWindow) {
                // Repaint previous hovered window?
                if (EditorInternalUtility.HoverWindow != null) {
                    EditorInternalUtility.HoverWindow.Repaint();
                }

                EditorInternalUtility.HoverWindow = EditorWindow.mouseOverWindow;
            }

            HoverControlID = controlID;

            // Get tip text using specified provider.
            if (tipProvider != null && tipContext != null) {
                if (s_HoverTipProvider != tipProvider || s_HoverTipContext != tipContext) {
                    // Update hover tip provider and context.
                    s_HoverTipProvider = tipProvider;
                    s_HoverTipContext = tipContext;

                    // Clear cached hover tip text?
                    s_HoverTipText = null;
                }

                // Calculate position of hover control in screen space and determine whether tip
                // should spring upwards instead of downwards.
                Rect hoverTipPosition = position;
                Rect visibleRect = ExtraEditorGUI.VisibleRect;

                if (visibleRect.yMax < hoverTipPosition.yMax) {
                    s_HoverTipReverse = (visibleRect.yMax - hoverTipPosition.y) < hoverTipPosition.height / 2;
                    hoverTipPosition.yMax = visibleRect.yMax;
                }
                else {
                    s_HoverTipReverse = false;
                }

                s_HoverTipPosition = ExtraEditorGUI.GUIToScreenRect(hoverTipPosition);

                if (EditorInternalUtility.HoverTipStage == EditorInternalUtility.HoverTipState.NotShown || EditorInternalUtility.HoverTipStage == EditorInternalUtility.HoverTipState.SkipFirst) {
                    EditorInternalUtility.HoverTipStage = EditorInternalUtility.HoverTipState.SkipFirst;
                }
            }
        }

        #endregion


        #region Hover Buttons

        /// <summary>
        /// Gets unique ID for hover control and automatically updates hover state.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <example>
        /// <para>The following demonstrates how to use this function:</para>
        /// <code language="csharp"><![CDATA[
        /// private void DrawHoverButton(Rect position)
        /// {
        ///     int controlID = RotorzEditorGUI.GetHoverControlID(position, "Foo Bar");
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="position">Position of control in space of editor window.</param>
        /// <param name="tipProvider">An object which provides tooltip text from given context.</param>
        /// <param name="tipContext">Context object for specified tip provider.</param>
        /// <returns>
        /// Unique ID for hover control.
        /// </returns>
        internal static int GetHoverControlID(Rect position, TextProvider tipProvider, object tipContext)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive, position);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    ClearHoverTip();
                    break;

                case EventType.MouseMove:
                    bool isHover = position.Contains(Event.current.mousePosition);
                    bool wasHover = (HoverControlID == controlID && EditorInternalUtility.HoverWindow == EditorWindow.mouseOverWindow);

                    if (isHover) {
                        // Only proceed if mouse pointer is within visible control area.
                        Rect visibleRect = ExtraEditorGUI.VisibleRect;
                        isHover = visibleRect.Contains(Event.current.mousePosition);

                        position.xMax = Mathf.Min(position.xMax, visibleRect.xMax);
                    }

                    if (isHover != wasHover) {
                        // If hover tip was primed, reset it since it was not drawn!
                        if (EditorInternalUtility.HoverTipStage == EditorInternalUtility.HoverTipState.ReadyToShow) {
                            EditorInternalUtility.HoverTipStage = EditorInternalUtility.HoverTipState.SkipFirst;
                        }

                        if (isHover) {
                            SetHoverControl(controlID, position, tipProvider, tipContext);
                        }

                        // Repaint hovered window.
                        if (EditorInternalUtility.HoverWindow != null) {
                            EditorInternalUtility.HoverWindow.Repaint();
                        }

                        if (wasHover) {
                            EditorInternalUtility.HoverWindow = null;
                            HoverControlID = 0;
                            ClearHoverTip();
                        }
                    }
                    break;
            }

            return controlID;
        }

        /// <inheritdoc cref="GetHoverControlID(Rect, TextProvider, object)"/>
        internal static int GetHoverControlID(Rect position)
        {
            return GetHoverControlID(position, null, null);
        }

        /// <inheritdoc cref="GetHoverControlID(Rect, TextProvider, object)"/>
        /// <param name="tipText">Tooltip text.</param>
        internal static int GetHoverControlID(Rect position, string tipText)
        {
            if (!string.IsNullOrEmpty(tipText)) {
                return GetHoverControlID(position, TextProviders.FromString, tipText);
            }
            else {
                return GetHoverControlID(position, null, null);
            }
        }

        internal const int HoverTipSpacing = 0;

        private static void CancelHoverTip()
        {
            if (EditorInternalUtility.HoverTipStage == EditorInternalUtility.HoverTipState.Shown) {
                EditorInternalUtility.HoverTipStage = EditorInternalUtility.HoverTipState.ReadyToHide;
                EditorInternalUtility.ReadyToHideTime = EditorApplication.timeSinceStartup;
            }
            else {
                EditorInternalUtility.HoverTipStage = EditorInternalUtility.HoverTipState.NotShown;
            }
        }

        internal static void DrawHoverTip(EditorWindow window)
        {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            // Note: The following does not work properly in Unity 4.3.
            //       GUI.tooltip appears to be set when a control is focused.
            /*
            // Hide hover tip if regular tooltip is shown.
            if (!string.IsNullOrEmpty(GUI.tooltip))
                ClearHoverTip();
            */

            if (s_HoverTipProvider == null) {
                CancelHoverTip();
                return;
            }

            // If hovering for first time, skip first repaint to introduce delay.
            if (EditorInternalUtility.HoverTipStage == EditorInternalUtility.HoverTipState.SkipFirst) {
                EditorInternalUtility.HoverTipStage = EditorInternalUtility.HoverTipState.ReadyToShow;
                return;
            }

            if (s_HoverTipText == null) {
                // Attempt to get tooltip text from provider.
                s_HoverTipText = s_HoverTipProvider(s_HoverTipContext);
                if (string.IsNullOrEmpty(s_HoverTipText)) {
                    CancelHoverTip();
                    return;
                }
            }

            if (GUIUtility.hotControl == 0 && HoverControlID != 0 && EditorInternalUtility.HoverWindow == window) {
                Rect windowPosition = window.position;
                using (var tipContent = ControlContent.Basic(s_HoverTipText)) {
                    RotorzEditorStyles.Instance.Tooltip.wordWrap = false;
                    Vector2 size = RotorzEditorStyles.Instance.Tooltip.CalcSize(tipContent);

                    // Adjust size of tooltip if it overflows width of window.
                    if (size.x >= windowPosition.width - 2) {
                        size.x = windowPosition.width - 2;
                        RotorzEditorStyles.Instance.Tooltip.wordWrap = true;
                        size.y = RotorzEditorStyles.Instance.Tooltip.CalcHeight(tipContent, size.x);
                    }

                    Vector2 hoverPosition = EditorGUIUtility.ScreenToGUIPoint(new Vector2(s_HoverTipPosition.x, s_HoverTipPosition.y));
                    Rect position = new Rect(hoverPosition.x, hoverPosition.y + s_HoverTipPosition.height - HoverTipSpacing, Mathf.Max(s_HoverTipPosition.width, size.x), size.y);
                    Rect arrow = new Rect(position.x + s_HoverTipPosition.width / 2f - 6f, position.y, 11, 5);

                    size.y += 5; // the arrow :)

                    // Tiny offset for tiny content!
                    if (s_HoverTipPosition.width < 14) {
                        position.x -= 4;
                    }

                    // Move tooltip leftward if overflowing right of window.
                    if (position.xMax >= windowPosition.width) {
                        position.x = windowPosition.width - position.width - 1;
                    }
                    // Move tooltip rightward if overflowing left of window.
                    position.x = Mathf.Max(0, position.x);

                    // Move tooltip upward if overflowing bottom of window.
                    if (s_HoverTipReverse || windowPosition.height - position.y < position.height) {
                        position.y = hoverPosition.y - size.y + HoverTipSpacing;
                        arrow.y = position.y + position.height + 5;
                        arrow.height = -arrow.height;
                    }
                    else {
                        // Make room for arrow.
                        position.y += 5;
                    }
                    // Move tooltip downward if overflowing top of window.
                    position.y = Mathf.Max(0, position.y);

                    RotorzEditorStyles.Instance.Tooltip.Draw(position, tipContent, false, false, false, false);

                    // Draw arrow.
                    GUI.DrawTexture(arrow, RotorzEditorStyles.Skin.TooltipArrow);

                    EditorInternalUtility.HoverTipStage = EditorInternalUtility.HoverTipState.Shown;
                }
            }
        }

        /// <summary>
        /// Draw button which changes when mouse pointer enters or leaves it.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="position">Position of button.</param>
        /// <param name="content">Content.</param>
        /// <param name="style">GUI style.</param>
        /// <returns>
        /// A value of <c>true</c> if button was clicked; otherwise <c>false</c>.
        /// </returns>
        internal static bool HoverButton(Rect position, GUIContent content, GUIStyle style)
        {
            int controlID = GetHoverControlID(position, content.tooltip);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = controlID;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        return position.Contains(Event.current.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    using (var drawContent = ControlContent.Basic(content.text, content.image)) {
                        style.Draw(position, drawContent, controlID);
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Draw button which changes when mouse pointer enters or leaves it.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="position">Position of button.</param>
        /// <param name="content">Content.</param>
        /// <returns>
        /// A value of <c>true</c> if button was clicked; otherwise <c>false</c>.
        /// </returns>
        internal static bool HoverButton(Rect position, GUIContent content)
        {
            return HoverButton(position, content, RotorzEditorStyles.Instance.FlatButton);
        }

        /// <summary>
        /// Draw button which changes when mouse pointer enters or leaves it.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="content">Content.</param>
        /// <param name="style">GUI style.</param>
        /// <param name="options">GUI layout options.</param>
        /// <returns>
        /// A value of <c>true</c> if button was clicked; otherwise <c>false</c>.
        /// </returns>
        internal static bool HoverButton(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect position = GUILayoutUtility.GetRect(content, style, options);
            return HoverButton(position, content, style);
        }

        /// <summary>
        /// Draw button which changes when mouse pointer enters or leaves it.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="content">Content.</param>
        /// <param name="options">GUI layout options.</param>
        /// <returns>
        /// A value of <c>true</c> if button was clicked; otherwise <c>false</c>.
        /// </returns>
        internal static bool HoverButton(GUIContent content, params GUILayoutOption[] options)
        {
            return HoverButton(content, RotorzEditorStyles.Instance.FlatButton, options);
        }

        /// <summary>
        /// Draw button which changes when mouse pointer enters or leaves it.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="position">Position of button.</param>
        /// <param name="content">Content.</param>
        /// <param name="style">GUI style.</param>
        /// <returns>
        /// A value of <c>true</c> if button was clicked; otherwise <c>false</c>.
        /// </returns>
        internal static bool ImmediateHoverButton(Rect position, GUIContent content, GUIStyle style)
        {
            int controlID = GetHoverControlID(position, content.tooltip);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        Event.current.Use();
                        return true;
                    }
                    break;

                case EventType.Repaint:
                    using (var tempContent = ControlContent.Basic(content.text, content.image)) {
                        style.Draw(position, tempContent, controlID);
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Draw button which changes when mouse pointer enters or leaves it.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="position">Position of button.</param>
        /// <param name="content">Content.</param>
        /// <returns>
        /// A value of <c>true</c> if button was clicked; otherwise <c>false</c>.
        /// </returns>
        internal static bool ImmediateHoverButton(Rect position, GUIContent content)
        {
            return ImmediateHoverButton(position, content, RotorzEditorStyles.Instance.FlatButton);
        }

        /// <summary>
        /// Non-functionality hover button which has stuck into "hover" state.
        /// </summary>
        /// <param name="position">Position of button.</param>
        /// <param name="content">Content.</param>
        internal static void StickyHoverButton(Rect position, GUIContent content)
        {
            int controlID = GetHoverControlID(position, content.tooltip);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    using (var tempContent = ControlContent.Basic(content.text, content.image)) {
                        RotorzEditorStyles.Instance.FlatButton.Draw(position, tempContent, true, true, false, false);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draw toggle button which responds instantly upon mouse down.
        /// </summary>
        /// <remarks>
        /// <para><b>Requirement:</b> You must set <c>wantsMouseEvents</c> for editor window in
        /// order for this to work.</para>
        /// </remarks>
        /// <param name="position">Position of button.</param>
        /// <param name="content">Content.</param>
        /// <param name="value">Value of toggle.</param>
        /// <param name="style">GUI style.</param>
        /// <returns>
        /// Modified value of toggle.
        /// </returns>
        internal static bool HoverToggle(Rect position, GUIContent content, bool value, GUIStyle style)
        {
            int controlID = GetHoverControlID(position, content.tooltip);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        value = !value;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    using (var tempContent = ControlContent.Basic(content.text, content.image)) {
                        style.Draw(position, tempContent, controlID, value);
                    }
                    break;
            }

            return value;
        }

        /// <inheritdoc cref="HoverToggle(Rect, GUIContent, bool, GUIStyle)"/>
        internal static bool HoverToggle(Rect position, GUIContent content, bool value)
        {
            return HoverToggle(position, content, value, RotorzEditorStyles.Instance.SmallFlatButton);
        }

        #endregion


        #region Decoration

        /// <summary>
        /// Draw simple light grey vertical separator.
        /// </summary>
        internal static void VerticalSeparatorLight()
        {
            Rect position = GUILayoutUtility.GetRect(7, 19 + 6 + 5);
            position.x += 3;
            position.y -= 2;
            position.width = 1;

            ExtraEditorGUI.SeparatorLight(position);
        }

        /// <summary>
        /// Output description text using small label font.
        /// </summary>
        /// <param name="label">Label text.</param>
        internal static void MiniFieldDescription(string label)
        {
            Color restore = GUI.contentColor;
            GUI.color = new Color32(92, 92, 92, 255);
            GUILayout.Label(label, RotorzEditorStyles.Instance.WhiteWordWrappedMiniLabel);
            GUI.color = restore;
        }

        /// <summary>
        /// Output large title text.
        /// </summary>
        /// <example>
        /// <para>The following code demonstrates how to output title text.</para>
        /// <para><img src="../art/title-text.jpg" alt="Example title text."/></para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show() {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private void OnGUI() {
        ///         RotorzEditorGUI.Title("Large Title");
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="text">Title text.</param>
        public static void Title(string text)
        {
            GUILayout.Label(text, RotorzEditorStyles.Instance.TitleLabel);
        }

        /// <summary>
        /// Output large title text.
        /// </summary>
        /// <example>
        /// <para>The following code demonstrates how to output title text.</para>
        /// <para><img src="../art/title-text.jpg" alt="Example title text."/></para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///    [MenuItem("Examples/GUI")]
        ///    private static void Show()
        ///    {
        ///        GetWindow<TestWindow>();
        ///    }
        ///
        ///
        ///    // It is better to keep local cache for these guys!
        ///    private GUIContent someContent = new GUIContent("Large Title");
        ///
        ///
        ///    private void OnGUI()
        ///    {
        ///        RotorzEditorGUI.Title(this.someContent);
        ///    }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="content">Label content for title.</param>
        public static void Title(GUIContent content)
        {
            GUILayout.Label(content, RotorzEditorStyles.Instance.TitleLabel);
        }

        #endregion


        #region Sections

        /// <summary>
        /// Draw expandable section with emphasized title like header.
        /// </summary>
        /// <overloads>There are three overloads for this function.</overloads>
        /// <example>
        /// <para>Usage example:</para>
        /// <para><img src="../art/title-foldout.png" alt="Example of title foldout."/></para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private bool expanded = true;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         GUILayout.Label("Example of expandable section:");
        ///         this.expanded = RotorzEditorGUI.FoldoutSection(this.expanded, "Extra Stuff", this.YourSectionGUI);
        ///     }
        ///
        ///     private void YourSectionGUI()
        ///     {
        ///         GUILayout.Label("Some additional information...");
        ///         GUILayout.Button("Do Something");
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="foldout">Current state of foldout.</param>
        /// <param name="label">Label of foldout header.</param>
        /// <param name="callback">Callback for drawing contents of GUI section.</param>
        /// <param name="paddedStyle">Style for padding section area.</param>
        /// <param name="titleStyle">Style for section title.</param>
        /// <returns>
        /// A value of <c>true</c> when foldout is expanded; otherwise <c>false</c>.
        /// </returns>
        public static bool FoldoutSection(bool foldout, string label, Action callback, GUIStyle paddedStyle, GUIStyle titleStyle)
        {
            bool newFoldout = GUILayout.Toggle(foldout, label, titleStyle);

            // Remove keyboard focus when foldout is toggled.
            if (newFoldout != foldout) {
                GUIUtility.keyboardControl = 0;
            }

            if (newFoldout) {
                GUILayout.BeginVertical(paddedStyle);
                callback();
                GUILayout.EndVertical();
                return true;
            }
            return false;
        }

        /// <inheritdoc cref="FoldoutSection(bool,string,Action,GUIStyle,GUIStyle)"/>
        public static bool FoldoutSection(bool foldout, string label, Action callback, GUIStyle paddedStyle)
        {
            return FoldoutSection(foldout, label, callback, paddedStyle, RotorzEditorStyles.Instance.FoldoutTitle);
        }

        /// <inheritdoc cref="FoldoutSection(bool,string,Action,GUIStyle,GUIStyle)"/>
        public static bool FoldoutSection(bool foldout, string label, Action callback)
        {
            return FoldoutSection(foldout, label, callback, RotorzEditorStyles.Instance.FoldoutSectionPadded, RotorzEditorStyles.Instance.FoldoutTitle);
        }

        #endregion


        #region Custom Field Types

        /// <summary>
        /// Gets ID of the control which is associated with the brush picker window.
        /// </summary>
        /// <returns>
        /// Unique control ID.
        /// </returns>
        /// <seealso cref="ShowBrushPicker"/>
        /// <seealso cref="BrushPickerSelectedBrush"/>
        public static int BrushPickerControlID { get; internal set; }

        /// <summary>
        /// Gets or sets brush which is selected in brush picker window.
        /// </summary>
        /// <remarks>
        /// <para>Always check <see cref="BrushPickerControlID"/> before attempting to access
        /// this property.</para>
        /// </remarks>
        /// <returns>
        /// The selected <see cref="Brush"/> instance.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If attempting to access property when brush picker is not shown.
        /// </exception>
        /// <seealso cref="ShowBrushPicker"/>
        /// <seealso cref="BrushPickerControlID"/>
        public static Brush BrushPickerSelectedBrush {
            get {
                if (BrushPickerControlID == 0 || BrushPickerWindow.Instance == null) {
                    throw new InvalidOperationException("Brush picker window is not shown. Always check 'BrushPickerControlID' first.");
                }
                return BrushPickerWindow.Instance.SelectedBrush;
            }
            set {
                if (BrushPickerControlID == 0 || BrushPickerWindow.Instance == null) {
                    throw new InvalidOperationException("Brush picker window is not shown. Always check 'BrushPickerControlID' first.");
                }
                BrushPickerWindow.Instance.SelectedBrush = value;
            }
        }

        /// <summary>
        /// Show brush picker window and associate with custom control.
        /// </summary>
        /// <remarks>
        /// <para>The following commands are executed upon selecting a brush:</para>
        /// <list type="bullet">
        /// <item><c>Rotorz.TileSystem.BrushPickerUpdated</c> - Brush selection changed.</item>
        /// <item><c>Rotorz.TileSystem.BrushPickerClosed</c> - Brush picker window was closed.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <para>This example demonstrates how to implement a custom brush selector button.
        /// When the button is clicked the brush picker window is shown.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class BrushPickerExampleWindow : EditorWindow
        /// {
        ///     private Brush someBrush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.someBrush = BrushPickerButton("Pick Brush", this.someBrush);
        ///     }
        ///
        ///
        ///     // Custom brush picker button control!
        ///     public static Brush BrushPickerButton(string text, Brush selectedBrush)
        ///     {
        ///         int pickerID = GUIUtility.GetControlID(FocusType.Passive);
        ///
        ///         // Respond to brush selection event.
        ///         if (Event.current.type == EventType.ExecuteCommand) {
        ///             if (Event.current.commandName == "Rotorz.TileSystem.BrushPickerUpdated") {
        ///                 // Update brush selection!
        ///                 if (selectedBrush != RotorzEditorGUI.BrushPickerSelectedBrush) {
        ///                     selectedBrush = RotorzEditorGUI.BrushPickerSelectedBrush;
        ///                     GUI.changed = true;
        ///                 }
        ///
        ///                 // Remember, picker window may still be shown!
        ///
        ///                 // Accept event and repaint this window.
        ///                 Event.current.Use();
        ///             }
        ///         }
        ///
        ///         // Draw button control itself!
        ///         if (GUILayout.Button(text)) {
        ///             // Display brush picker window!
        ///             RotorzEditorGUI.ShowBrushPicker(selectedBrush, true, true, pickerID);
        ///         }
        ///
        ///         // Return brush selection (possibly modified!).
        ///         return selectedBrush;
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="brush">Currently selected brush.</param>
        /// <param name="allowAlias">Indicates whether alias brushes can be selected.</param>
        /// <param name="allowMaster">Indicates whether master brushes can be selected.</param>
        /// <param name="controlID">Unique ID of custom control.</param>
        /// <seealso cref="BrushPickerSelectedBrush"/>
        /// <seealso cref="BrushPickerControlID"/>
        public static void ShowBrushPicker(Brush brush, bool allowAlias, bool allowMaster, int controlID)
        {
            BrushPickerControlID = controlID;
            BrushPickerWindow.ShowWindow(brush, allowAlias, allowMaster);
        }


        private static GUIContent s_BrushFieldMixedValueContent = new GUIContent("�", RotorzEditorStyles.Skin.ToolPaint);

        private static Brush DoBrushField(Rect position, int controlID, GUIContent content, Brush brush, bool allowAlias, bool allowMaster, GUIStyle style)
        {
            if (EditorGUI.showMixedValue) {
                brush = null;
                content = s_BrushFieldMixedValueContent;
            }

            var initialBrush = brush;

            EventType eventType = Event.current.GetTypeForControl(controlID);

            switch (eventType) {
                case EventType.Repaint:
                    style.Draw(position, content, false, false, DragAndDrop.activeControlID == controlID, GUIUtility.keyboardControl == controlID);

                    // Ensure that brush selection window reflects currently selected brush.
                    if (BrushPickerControlID == controlID && BrushPickerWindow.Instance != null) {
                        BrushPickerWindow.Instance.SelectedBrush = brush;
                    }
                    break;

                case EventType.MouseDown:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition)) {
                        GUIUtility.keyboardControl = controlID;

                        ShowBrushPicker(brush, allowAlias, allowMaster, controlID);
                        Event.current.Use();
                        GUIUtility.ExitGUI();
                    }
                    break;

                case EventType.KeyDown:
                    if (GUI.enabled && GUIUtility.keyboardControl == controlID) {
                        switch (Event.current.keyCode) {
                            case KeyCode.Backspace:
                            case KeyCode.Delete:
                                // Clear brush selection when backspace or delete key is pressed.
                                brush = null;
                                Event.current.Use();
                                break;

                            case KeyCode.Space:
                            case KeyCode.Return:
                            case KeyCode.KeypadEnter:
                                // Display brush selection interface when activation key is pressed.
                                ShowBrushPicker(brush, allowAlias, allowMaster, controlID);
                                Event.current.Use();
                                GUIUtility.ExitGUI();
                                break;
                        }
                    }
                    break;

                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length > 0) {
                        Brush dragBrush = DragAndDrop.objectReferences[0] as Brush;
                        if (dragBrush != null) {
                            if (eventType == EventType.DragUpdated) {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                                DragAndDrop.activeControlID = controlID;
                            }
                            else {
                                DragAndDrop.AcceptDrag();
                                DragAndDrop.visualMode = DragAndDropVisualMode.None;
                                brush = dragBrush;
                            }
                            Event.current.Use();
                        }
                    }
                    break;

                case EventType.ExecuteCommand:
                    if (Event.current.commandName == "Rotorz.TileSystem.BrushPickerUpdated" && BrushPickerControlID == controlID) {
                        brush = BrushPickerSelectedBrush;
                        Event.current.Use();
                    }
                    break;
            }

            // Is alias brush permitted?
            if (!allowAlias && brush is AliasBrush) {
                brush = null;
            }

            // Sanitize inputs if brush selection seems to have changed.
            if (brush != initialBrush) {
                // Is master brush permitted?
                if (!allowMaster && brush != null) {
                    var record = BrushDatabase.Instance.FindRecord(brush);
                    if (record != null && record.IsMaster) {
                        brush = null;
                    }
                }

                // Indicate if brush was actually changed!
                if (brush != initialBrush) {
                    GUI.changed = true;
                }
            }

            return brush;
        }

        /// <summary>
        /// Brush selection field with label for manual position.
        /// </summary>
        /// <overloads>
        ///    <summary>
        ///    Brush selection field for custom user interfaces with overloads for both
        ///    automatic and manual layout modes.
        ///    </summary>
        /// </overloads>
        /// <remarks>
        /// <para>Field is presented in a similar way to the standard object field.
        /// When clicked the brush picker window is shown allowing the user to
        /// make a selection more easily. Brushes can also be dragged and dropped from
        /// other brush lists onto the field.</para>
        /// <para><img src="../art/brush-field-label.jpg" alt="Brush selection field with label."/></para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to use the brush field with label.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private Brush selectedBrush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.selectedBrush = RotorzEditorGUI.BrushField(
        ///             new Rect(3, 3, 215, 20),
        ///             "Pick Brush",
        ///             this.selectedBrush
        ///         );
        ///
        ///         GUI.Label(new Rect(3, 25, 215, 20), "Preview:");
        ///
        ///         RotorzEditorGUI.DrawBrushPreview(
        ///             new Rect(0, 41, 200, 200),
        ///             this.selectedBrush
        ///         );
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="position">Position for brush field control.</param>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="brush">The brush the field shows.</param>
        /// <param name="allowAlias">Indicates whether alias brushes can be selected.</param>
        /// <param name="allowMaster">Indicates whether master brushes can be selected.</param>
        /// <returns>
        /// The brush that has been set by the user.
        /// </returns>
        public static Brush BrushField(Rect position, string label, Brush brush, bool allowAlias = true, bool allowMaster = true)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Keyboard, position);

            if (!string.IsNullOrEmpty(label)) {
                using (var prefixLabelContent = ControlContent.Basic(label)) {
                    position = EditorGUI.PrefixLabel(position, controlID, prefixLabelContent);
                }
            }

            using (var content = ControlContent.Basic(
                labelText: brush == null ? TileLang.Text("(None)") : brush.name,
                image: RotorzEditorStyles.Skin.ToolPaint
            )) {
                return DoBrushField(position, controlID, content, brush, allowAlias, allowMaster, RotorzEditorStyles.Instance.BrushField);
            }
        }

        /// <summary>
        /// Brush selection field for manual position.
        /// </summary>
        /// <remarks>
        /// <para>Field is presented in a similar way to the standard object field.
        /// When clicked the brush picker window is shown allowing the user to
        /// make a selection more easily. Brushes can also be dragged and dropped from
        /// other brush lists onto the field.</para>
        /// <para><img src="../art/brush-field.jpg" alt="Brush selection field."/></para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to use the brush field.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private Brush selectedBrush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.selectedBrush = RotorzEditorGUI.BrushField(
        ///             new Rect(3, 3, 215, 20),
        ///             this.selectedBrush
        ///         );
        ///
        ///         GUI.Label(new Rect(3, 25, 215, 20), "Preview:");
        ///
        ///         RotorzEditorGUI.DrawBrushPreview(
        ///             new Rect(0, 41, 200, 200),
        ///             this.selectedBrush
        ///         );
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="position">Position for brush field control.</param>
        /// <param name="brush">The brush the field shows.</param>
        /// <param name="allowAlias">Indicates whether alias brushes can be selected.</param>
        /// <param name="allowMaster">Indicates whether master brushes can be selected.</param>
        /// <returns>
        /// The brush that has been set by the user.
        /// </returns>
        public static Brush BrushField(Rect position, Brush brush, bool allowAlias = true, bool allowMaster = true)
        {
            return BrushField(position, null, brush, allowAlias, allowMaster);
        }

        /// <summary>
        /// Brush selection field with label for automatic layout.
        /// </summary>
        /// <remarks>
        /// <para>Field is presented in a similar way to the standard object field.
        /// When clicked the brush picker window is shown allowing the user to
        /// make a selection more easily. Brushes can also be dragged and dropped from
        /// other brush lists onto the field.</para>
        /// <para><img src="../art/brush-field-label.jpg" alt="Brush selection field with label."/></para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to use the brush field with
        /// a label using automatic layout.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private Brush selectedBrush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.selectedBrush = RotorzEditorGUI.BrushField(
        ///             "Pick Brush",
        ///             this.selectedBrush
        ///         );
        ///
        ///         GUILayout.Label("Preview:");
        ///
        ///         Rect position = GUILayoutUtility.GetRect(
        ///             GUIContent.none,
        ///             GUIStyle.none,
        ///             GUILayout.Width(200),
        ///             GUILayout.Height(200)
        ///         );
        ///
        ///         RotorzEditorGUI.DrawBrushPreview(position, this.selectedBrush);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="brush">The brush the field shows.</param>
        /// <param name="allowAlias">Indicates whether alias brushes can be selected.</param>
        /// <param name="allowMaster">Indicates whether master brushes can be selected.</param>
        /// <param name="options">
        /// An optional list of layout options that specify extra layouting properties.
        /// Any values passed in here will override settings defined by the style.
        /// <para>See: <a href="http://docs.unity3d.com/Documentation/ScriptReference/GUILayout.html">http://docs.unity3d.com/Documentation/ScriptReference/GUILayout.html</a></para>
        /// </param>
        /// <returns>
        /// The brush that has been set by the user.
        /// </returns>
        public static Brush BrushField(string label, Brush brush, bool allowAlias = true, bool allowMaster = true, params GUILayoutOption[] options)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, RotorzEditorStyles.Instance.BrushField, options);
            return BrushField(position, label, brush, allowAlias, allowMaster);
        }

        /// <summary>
        /// Brush selection field for automatic layout.
        /// </summary>
        /// <remarks>
        /// <para>Field is presented in a similar way to the standard object field.
        /// When clicked the brush picker window is shown allowing the user to
        /// make a selection more easily. Brushes can also be dragged and dropped from
        /// other brush lists onto the field.</para>
        /// <para><img src="../art/brush-field.jpg" alt="Brush selection field."/></para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to use the brush field with
        /// automatic layout.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private Brush selectedBrush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.selectedBrush = RotorzEditorGUI.BrushField(this.selectedBrush);
        ///
        ///         GUILayout.Label("Preview:");
        ///
        ///         Rect position = GUILayoutUtility.GetRect(
        ///             GUIContent.none,
        ///             GUIStyle.none,
        ///             GUILayout.Width(200),
        ///             GUILayout.Height(200)
        ///         );
        ///
        ///         RotorzEditorGUI.DrawBrushPreview(position, this.selectedBrush);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="brush">The brush the field shows.</param>
        /// <param name="allowAlias">Indicates whether alias brushes can be selected.</param>
        /// <param name="allowMaster">Indicates whether master brushes can be selected.</param>
        /// <param name="options">
        /// An optional list of layout options that specify extra layouting properties.
        /// Any values passed in here will override settings defined by the style.
        /// <para>See: <a href="http://docs.unity3d.com/Documentation/ScriptReference/GUILayout.html">http://docs.unity3d.com/Documentation/ScriptReference/GUILayout.html</a></para>
        /// </param>
        /// <returns>
        /// The brush that has been set by the user.
        /// </returns>
        public static Brush BrushField(Brush brush, bool allowAlias = true, bool allowMaster = true, params GUILayoutOption[] options)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, RotorzEditorStyles.Instance.BrushField, options);
            return BrushField(position, null, brush, allowAlias, allowMaster);
        }

        /// <summary>
        /// Variation of the <see cref="EditorGUILayout.ObjectField"/> which shows a
        /// larger thumbnail with an autotile layout overlay when empty.
        /// </summary>
        /// <param name="artwork">The current autotile artwork (raw input).</param>
        /// <param name="layout">Indicates which autotile layout overlay to display.</param>
        /// <param name="innerJoins">Indicates if the autotile layout includes inner joins.</param>
        /// <returns>
        /// The autotile artwork (raw input) that has been set by the user.
        /// </returns>
        public static Texture2D AutotileArtworkField(Texture2D artwork, AutotileLayout layout, bool innerJoins)
        {
            Texture2D previewTexture;
            float previewTextureHeight;

            switch (layout) {
                default:
                case AutotileLayout.Basic:
                    GUILayout.Space(20);
                    previewTexture = RotorzEditorStyles.Skin.AutotileBasicPreview;
                    previewTextureHeight = innerJoins ? 128 : 85;
                    break;

                case AutotileLayout.Extended:
                    GUILayout.Space(15);
                    previewTexture = RotorzEditorStyles.Skin.AutotileExtendedPreview;
                    previewTextureHeight = innerJoins ? 128 : 96;
                    break;
            }

            GUILayout.BeginVertical();
            GUILayout.Space(-1);

            artwork = EditorGUILayout.ObjectField(artwork, typeof(Texture2D), false, GUILayout.Width(previewTexture.width + 2), GUILayout.Height(previewTextureHeight + 2)) as Texture2D;

            // Only display autotile preview image if no texture is selected.
            if (artwork == null && Event.current.type == EventType.Repaint) {
                Rect previewPosition = GUILayoutUtility.GetLastRect();
                previewPosition.x += 1;
                previewPosition.y += 1;
                previewPosition.width -= 2;
                previewPosition.height -= 2;

                Rect texCoords = new Rect(0, 0, 1, previewTextureHeight / 128f);

                GUI.DrawTextureWithTexCoords(previewPosition, previewTexture, texCoords);

                if (!innerJoins) {
                    // Draw edge outline if no inner joins are present.
                    Color restoreColor = GUI.color;
                    GUI.color = EditorGUIUtility.isProSkin
                        ? new Color(130f / 255f, 130f / 255f, 130f / 255f)
                        : Color.white;

                    previewPosition.height = 1;
                    GUI.DrawTexture(previewPosition, EditorGUIUtility.whiteTexture);

                    GUI.color = restoreColor;
                }
            }

            GUILayout.EndVertical();

            return artwork;
        }

        internal static string SearchField(Rect position, string content)
        {
            position.width -= 15;
            content = EditorGUI.TextField(position, content, RotorzEditorStyles.Instance.SearchTextField);

            position.x += position.width;
            position.width = 15;
            if (GUI.Button(position, GUIContent.none, content != string.Empty ? RotorzEditorStyles.Instance.SearchCancelButton : RotorzEditorStyles.Instance.SearchCancelButtonEmpty)) {
                content = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            return content;
        }

        /*
        internal static string MiniSearchField(Rect position, string content, params GUILayoutOption[] options) {
            position.width -= 15;
            content = EditorGUI.TextField(position, content, RotorzEditorStyles.toolbarSearchTextField);

            position.x += position.width;
            position.width = 15;
            if (GUI.Button(position, GUIContent.none, content != string.Empty ? RotorzEditorStyles.toolbarSearchCancelButton : RotorzEditorStyles.toolbarSearchCancelButtonEmpty)) {
                content = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            return content;
        }
        */

        #endregion


        #region Rotation Selector

        /// <summary>
        /// Draw control which allows user to visualize and select a rotation.
        /// </summary>
        /// <remarks>
        /// <para>As with other controls, the value of <c>GUI.changed</c> becomes <c>true</c> when
        /// user input has changed.</para>
        /// </remarks>
        /// <param name="position">Position for rotation selector control.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0�, 1 = 90�, 2 = 180�, 3 = 270�).</param>
        /// <returns>
        /// The updated rotation state.
        /// </returns>
        internal static int RotationSelector(Rect position, int rotation)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            Vector2 center = position.center;

            Rect button0 = new Rect(center.x - 15, center.y - 15, 15, 15);
            Rect button1 = new Rect(center.x, center.y - 15, 15, 15);
            Rect button2 = new Rect(center.x, center.y, 15, 15);
            Rect button3 = new Rect(center.x - 15, center.y, 15, 15);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.Repaint:
                    Rect texCoords = new Rect(rotation * 39f / 156f, 0f, 39f / 156f, 1f);
                    GUI.DrawTextureWithTexCoords(position, RotorzEditorStyles.Skin.RotationSelector, texCoords);
                    break;

                case EventType.MouseDown:
                    // Temporarily rotate GUI around center point of rotation selector to read
                    // mouse pointer and compare with rotation selector buttons.
                    Matrix4x4 restoreMatrix = GUI.matrix;
                    GUIUtility.RotateAroundPivot(45, center);

                    Vector3 mousePosition = Event.current.mousePosition;

                    GUI.matrix = restoreMatrix;

                    // Find out if a selector button was clicked!
                    int newRotation = rotation;

                    if (button0.Contains(mousePosition)) {
                        newRotation = 0;
                        Event.current.Use();
                    }
                    else if (button1.Contains(mousePosition)) {
                        newRotation = 1;
                        Event.current.Use();
                    }
                    else if (button2.Contains(mousePosition)) {
                        newRotation = 2;
                        Event.current.Use();
                    }
                    else if (button3.Contains(mousePosition)) {
                        newRotation = 3;
                        Event.current.Use();
                    }

                    if (newRotation != rotation) {
                        rotation = newRotation;
                        GUI.changed = true;
                    }
                    break;
            }

            //!DEBUG
            /*
            Color restoreColor = GUI.color;
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(button0, GUIContent.none);
            GUI.Box(new Rect(button1, GUIContent.none);
            GUI.Box(new Rect(button2, GUIContent.none);
            GUI.Box(new Rect(button3, GUIContent.none);
            GUI.color = restoreColor;
            */

            return rotation;
        }

        #endregion


        #region Brush Categories

        private static void PopulateCategoryFieldMenu(ICustomPopupContext<int> context)
        {
            var popup = context.Popup;

            popup.AddOption(ProjectSettings.Instance.GetCategoryLabel(0), context, 0);
            popup.AddSeparator();

            foreach (var category in ProjectSettings.Instance.Categories) {
                popup.AddOption(category.Label, context, category.Id);
            }

            popup.AddSeparator();
            popup.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Manage Categories")))
                .Action(() => {
                    var projectSettings = ProjectSettings.Instance;

                    Selection.objects = new Object[] { projectSettings };
                    projectSettings.CollapseAllSections();
                    projectSettings.ExpandBrushCategoriesSection = true;
                });
        }

        /// <summary>
        /// Make a brush category selection field.
        /// </summary>
        /// <param name="label">Label in front of field.</param>
        /// <param name="categoryId">Brush category shown in field.</param>
        /// <param name="options">
        /// An optional list of layout options that specify extra layouting properties.
        /// Any values passed in here will override settings defined by the style.
        /// Please refer to Unity documentation for further details.
        /// </param>
        /// <returns>
        /// Brush category selected by user.
        /// </returns>
        public static int BrushCategoryField(GUIContent label, int categoryId, params GUILayoutOption[] options)
        {
            string categoryLabelText = ProjectSettings.Instance.GetCategoryLabel(categoryId);
            using (var valueLabel = ControlContent.Basic(categoryLabelText)) {
                return CustomPopupGUI.Popup(label, categoryId, valueLabel, PopulateCategoryFieldMenu, options: options);
            }
        }

        /// <summary>
        /// Make a brush category selection field.
        /// </summary>
        /// <remarks>
        /// <para>This field makes it easy for users to select a brush category in
        /// custom user interfaces. This field includes the ability to manage
        /// brush categories.</para>
        /// <para><img src="../art/brush-category-field-label.jpg" alt="Brush category selection field with label."/></para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to use the brush category field
        /// with label using automatic layout.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private int selectedCategory;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.selectedCategory = RotorzEditorGUI.BrushCategoryField(
        ///             "Pick Category",
        ///             this.selectedCategory
        ///         );
        ///
        ///         GUILayout.Label("Category #: " + this.selectedCategory);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="label">Label in front of field.</param>
        /// <param name="categoryId">Brush category shown in field.</param>
        /// <param name="options">
        /// An optional list of layout options that specify extra layouting properties.
        /// Any values passed in here will override settings defined by the style.
        /// Please refer to Unity documentation for further details.
        /// </param>
        /// <returns>
        /// Brush category selected by user.
        /// </returns>
        public static int BrushCategoryField(string label, int categoryId, params GUILayoutOption[] options)
        {
            using (var labelContent = ControlContent.Basic(label)) {
                return BrushCategoryField(labelContent, categoryId, options);
            }
        }

        /// <summary>
        /// Make a brush category selection field.
        /// </summary>
        /// <remarks>
        /// <para>This field makes it easy for users to select a brush category in
        /// custom user interfaces. This field includes the ability to manage
        /// brush categories.</para>
        /// <para><img src="../art/brush-category-field.jpg" alt="Brush category selection field."/></para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to use the brush category field
        /// with automatic layout.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private int selectedCategory;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.selectedCategory = RotorzEditorGUI.BrushCategoryField(
        ///             this.selectedCategory
        ///         );
        ///
        ///         GUILayout.Label("Category #: " + this.selectedCategory);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="categoryId">Brush category shown in field.</param>
        /// <param name="options">An optional list of layout options that specify extra
        /// layouting properties. Any values passed in here will override settings defined
        /// by the style. Please refer to Unity documentation for further details.</param>
        /// <returns>
        /// Brush category selected by user.
        /// </returns>
        public static int BrushCategoryField(int categoryId, params GUILayoutOption[] options)
        {
            return BrushCategoryField(GUIContent.none, categoryId, options);
        }

        #endregion


        #region Stripping Options UI

        internal static int GetMixedStrippingOptionsMask(TileSystem[] systems)
        {
            int mask = 0;
            int firstOptions = systems[0].StrippingOptions;
            for (int i = 1; i < systems.Length; ++i) {
                int options = systems[i].StrippingOptions;
                mask |= firstOptions ^ options;
            }
            return mask;
        }

        internal static int GetMixedStrippingOptionsMask(TileSystemPreset[] presets)
        {
            int mask = 0;
            int firstOptions = presets[0].StrippingOptions;
            for (int i = 1; i < presets.Length; ++i) {
                int options = presets[i].StrippingOptions;
                mask |= firstOptions ^ options;
            }
            return mask;
        }

        internal static int StrippingOptions(StrippingPreset preset, int strippingOptions, int mixedOptionsMask)
        {
            // Ensure that stripping preset is honoured
            if (preset != StrippingPreset.Custom) {
                strippingOptions = StripFlagUtility.GetPresetOptions(preset);
            }

            int initialStrippingOptions = strippingOptions;
            bool initialShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.BeginDisabledGroup(preset != StrippingPreset.Custom);

            bool temp;

            bool stripSystemComponent = (strippingOptions & StripFlag.STRIP_TILE_SYSTEM) != 0;
            bool stripChunkMap = (strippingOptions & StripFlag.STRIP_CHUNK_MAP) != 0;
            bool stripTileData = (strippingOptions & StripFlag.STRIP_TILE_DATA) != 0;
            bool stripBrushRefs = (strippingOptions & StripFlag.STRIP_BRUSH_REFS) != 0;
            bool stripEmptyObjects = (strippingOptions & StripFlag.STRIP_EMPTY_OBJECTS) != 0;
            bool stripCombinedEmpty = (strippingOptions & StripFlag.STRIP_COMBINED_EMPTY) != 0;
            bool stripChunks = (strippingOptions & StripFlag.STRIP_CHUNKS) != 0;
            bool stripEmptyChunks = (strippingOptions & StripFlag.STRIP_EMPTY_CHUNKS) != 0;
            bool stripPlopComponents = (strippingOptions & StripFlag.STRIP_PLOP_COMPONENTS) != 0;

            GUILayout.Space(2);

            ++EditorGUI.indentLevel;
            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_TILE_SYSTEM) != 0;
            stripSystemComponent = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip tile system component"), stripSystemComponent);
            ++EditorGUI.indentLevel;
            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_CHUNK_MAP) != 0;
            temp = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip chunk map"), stripChunkMap);
            if (temp != stripChunkMap) {
                stripChunkMap = temp;
                if (!temp) {
                    // Deselect dependencies
                    stripSystemComponent = false;
                    stripChunks = false;
                }
            }
            ++EditorGUI.indentLevel;
            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_TILE_DATA) != 0;
            temp = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip tile data"), stripTileData);
            if (temp != stripTileData) {
                stripTileData = temp;
                if (!temp) {
                    // Deselect dependencies
                    stripSystemComponent = false;
                    stripChunks = false;
                    stripChunkMap = false;
                }
            }

            ++EditorGUI.indentLevel;
            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_BRUSH_REFS) != 0;
            temp = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip brush references"), stripBrushRefs);
            if (temp != stripBrushRefs) {
                stripBrushRefs = temp;
                if (!temp) {
                    // Deselect dependencies
                    stripSystemComponent = false;
                    stripChunks = false;
                    stripChunkMap = false;
                    stripTileData = false;
                }
            }
            --EditorGUI.indentLevel;
            --EditorGUI.indentLevel;
            --EditorGUI.indentLevel;

            ExtraEditorGUI.SeparatorLight();

            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_EMPTY_OBJECTS) != 0;
            stripEmptyObjects = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip empty objects"), stripEmptyObjects);
            ++EditorGUI.indentLevel;
            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_COMBINED_EMPTY) != 0;
            temp = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip empty objects after combine"), stripCombinedEmpty);
            if (temp != stripCombinedEmpty) {
                stripCombinedEmpty = temp;
                if (!temp) {
                    // Deselect dependencies
                    stripEmptyObjects = false;
                }
            }
            --EditorGUI.indentLevel;

            ExtraEditorGUI.SeparatorLight();

            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_CHUNKS) != 0;
            stripChunks = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip chunks and reparent tiles"), stripChunks);
            ++EditorGUI.indentLevel;
            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_EMPTY_CHUNKS) != 0;
            temp = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip empty chunks"), stripEmptyChunks);
            if (temp != stripEmptyChunks) {
                stripEmptyChunks = temp;
                if (!temp) {
                    // Deselect dependencies
                    stripChunks = false;
                }
            }
            --EditorGUI.indentLevel;

            ExtraEditorGUI.SeparatorLight();

            EditorGUI.showMixedValue = (mixedOptionsMask & StripFlag.STRIP_PLOP_COMPONENTS) != 0;
            stripPlopComponents = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Strip plop instance / group components"), stripPlopComponents);
            --EditorGUI.indentLevel;

            EditorGUI.EndDisabledGroup();
            EditorGUI.showMixedValue = initialShowMixedValue;

            // Build new options mask?
            if (preset == StrippingPreset.Custom)
                strippingOptions = StripFlagUtility.PreFilterStrippingOptions(
                    (stripSystemComponent ? StripFlag.STRIP_TILE_SYSTEM : 0) |
                    (stripChunkMap ? StripFlag.STRIP_CHUNK_MAP : 0) |
                    (stripTileData ? StripFlag.STRIP_TILE_DATA : 0) |
                    (stripBrushRefs ? StripFlag.STRIP_BRUSH_REFS : 0) |
                    (stripEmptyObjects ? StripFlag.STRIP_EMPTY_OBJECTS : 0) |
                    (stripCombinedEmpty ? StripFlag.STRIP_COMBINED_EMPTY : 0) |
                    (stripChunks ? StripFlag.STRIP_CHUNKS : 0) |
                    (stripEmptyChunks ? StripFlag.STRIP_EMPTY_CHUNKS : 0) |
                    (stripPlopComponents ? StripFlag.STRIP_PLOP_COMPONENTS : 0)
                );

            return initialStrippingOptions ^ strippingOptions;
        }

        /// <summary>
        /// Present user interface to input stripping preset.
        /// </summary>
        /// <example>
        /// Integrating stripping options into an editor GUI:
        /// <code language="csharp"><![CDATA[
        /// private void OnGUI()
        /// {
        ///     // Place stripping options into local variables
        ///     StrippingPreset preset = this.tileSystem.StrippingPreset;
        ///     int options = this.tileSystem.StrippingOptions;
        ///
        ///     // Display stripping options GUI
        ///     RotorzEditorGUI.StrippingOptions(ref preset, ref options);
        ///
        ///     // Assign new stripping options to tile system (order is important!)
        ///     this.tileSystem.StrippingPreset = preset;
        ///     if (preset == StrippingPreset.Custom) {
        ///         this.tileSystem.StrippingOptions = options;
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="preset">Type of stripping preset.</param>
        /// <param name="strippingOptions">Per-preset stripping options.</param>
        public static void StrippingOptions(ref StrippingPreset preset, ref int strippingOptions)
        {
            // Ensure that stripping preset is honoured
            if (preset != StrippingPreset.Custom) {
                strippingOptions = StripFlagUtility.GetPresetOptions(preset);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Stripping Preset"),
                TileLang.Text("Custom level of stripping can be applied to tile system upon build.")
            )) {
                preset = (StrippingPreset)EditorGUILayout.EnumPopup(content, preset);
            }

            int diff = (int)StrippingOptions(preset, strippingOptions, 0);
            int addBits = diff & ~strippingOptions;
            int removeBits = diff & strippingOptions;
            strippingOptions = (strippingOptions & ~removeBits) | addBits;
        }

        #endregion


        #region Tab Selector

        /// <summary>
        /// Draw tab selector for automatic layout.
        /// </summary>
        /// <param name="selectedIndex">Zero-based index of active tab.</param>
        /// <param name="tabs">Array of tab labels.</param>
        /// <returns>
        /// Zero-based index of selected tab.
        /// </returns>
        public static int TabSelector(int selectedIndex, GUIContent[] tabs)
        {
            GUILayout.BeginHorizontal();

            int newSelectedIndex = selectedIndex;
            int controlID;
            Rect tab;

            for (int i = 0; i < tabs.Length; ++i) {
                tab = GUILayoutUtility.GetRect(tabs[i], RotorzEditorStyles.Instance.Tab);
                controlID = GUIUtility.GetControlID(FocusType.Passive, tab);
                switch (Event.current.GetTypeForControl(controlID)) {
                    case EventType.MouseDown:
                        if (GUI.enabled && tab.Contains(Event.current.mousePosition)) {
                            newSelectedIndex = i;
                            GUIUtility.keyboardControl = 0;
                            Event.current.Use();
                        }
                        break;

                    case EventType.Repaint:
                        RotorzEditorStyles.Instance.Tab.Draw(tab, tabs[i], false, false, i == selectedIndex, false);
                        break;
                }
            }

            GUILayout.Box(GUIContent.none, RotorzEditorStyles.Instance.TabBackground);
            GUILayout.EndHorizontal();

            if (newSelectedIndex != selectedIndex) {
                GUI.changed = true;
            }

            return newSelectedIndex;
        }

        #endregion


        #region Vertical Tab Selector

        /// <summary>
        /// Draw vertical tab selector.
        /// </summary>
        /// <param name="position">Position of tab selector.</param>
        /// <param name="selectedIndex">Zero-based index of active tab.</param>
        /// <param name="tabs">Array of tab labels.</param>
        /// <returns>
        /// Zero-based index of selected tab.
        /// </returns>
        internal static int VerticalTabSelector(Rect position, int selectedIndex, GUIContent[] tabs)
        {
            if (Event.current.type == EventType.Repaint) {
                GUI.skin.box.Draw(new Rect(
                    position.x - 1,
                    position.y - 1,
                    position.width + 1,
                    position.height + 2
                ), false, false, false, false);
            }

            Rect tab = new Rect(
                position.x + 4,
                position.y + 40,
                position.width - 5,
                32
            );

            int controlID;

            for (int i = 0, j = 0; i < tabs.Length; ++i) {
                // Display seperator for `null` when at least one tab has been drawn.
                if (j > 0 && tabs[i] == null) {
                    ExtraEditorGUI.SeparatorLight(new Rect(tab.x, tab.y, tab.width, 1));
                    tab.y += 3;
                    continue;
                }

                controlID = GUIUtility.GetControlID(FocusType.Passive, tab);
                switch (Event.current.GetTypeForControl(controlID)) {
                    case EventType.MouseDown:
                        if (tab.Contains(Event.current.mousePosition)) {
                            selectedIndex = i;
                            GUIUtility.keyboardControl = 0;
                            Event.current.Use();
                        }
                        break;

                    case EventType.Repaint:
                        RotorzEditorStyles.Instance.ListSectionElement.Draw(tab, tabs[i], false, false, i == selectedIndex, false);
                        break;
                }

                tab.y += 34;
                ++j;
            }

            return selectedIndex;
        }

        /// <summary>
        /// Draw vertical tab selector for automatic layout.
        /// </summary>
        /// <param name="selectedIndex">Zero-based index of active tab.</param>
        /// <param name="tabs">Array of tab labels.</param>
        /// <param name="options">Layout options, width should always be specified.</param>
        /// <returns>
        /// Zero-based index of selected tab.
        /// </returns>
        internal static int VerticalTabSelector(int selectedIndex, GUIContent[] tabs, params GUILayoutOption[] options)
        {
            Rect position = GUILayoutUtility.GetRect(0, Screen.height, options);
            return VerticalTabSelector(position, selectedIndex, tabs);
        }

        #endregion


        #region Mini Slider

        private static int MiniSliderValueFromEvent(Rect position, int minValue, int maxValue)
        {
            int range = maxValue - minValue;

            float visualRange = position.width - 2;
            float normalizedValue = Mathf.Clamp01((Event.current.mousePosition.x - position.x - 1) / visualRange);

            return Mathf.Min(maxValue, minValue + Mathf.RoundToInt(normalizedValue * range));
        }

        internal static int MiniSlider(Rect position, int value, int minValue, int maxValue)
        {
            int newValue = value;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (Event.current.button == 0 && position.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = controlID;
                        newValue = MiniSliderValueFromEvent(position, minValue, maxValue);
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        newValue = MiniSliderValueFromEvent(position, minValue, maxValue);
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    int range = maxValue - minValue;
                    int rangeValue = value - minValue;

                    Color restoreColor = GUI.color;

                    Rect innerPosition = new Rect {
                        x = position.x + 1,
                        y = position.y + 1,
                        width = ((position.width - 2) * rangeValue) / (float)range,
                        height = position.height - 2
                    };
                    GUI.color = RotorzEditorStyles.Instance.MiniSliderFillColor;
                    GUI.DrawTexture(innerPosition, EditorGUIUtility.whiteTexture);

                    innerPosition.x = innerPosition.xMax;
                    innerPosition.width = position.width - innerPosition.width - 2;
                    GUI.color = RotorzEditorStyles.Instance.MiniSliderEmptyColor;
                    GUI.DrawTexture(innerPosition, EditorGUIUtility.whiteTexture);

                    // Draw middle marker!
                    if (rangeValue < range / 2) {
                        GUI.color = RotorzEditorStyles.Instance.MiniSliderMarkerColor;
                        GUI.DrawTexture(new Rect(position.x + position.width / 2 - 1, position.y + (position.height - 3) / 2, 1, 3), EditorGUIUtility.whiteTexture);
                    }

                    GUI.color = restoreColor;
                    RotorzEditorStyles.Instance.MiniSliderBorder.Draw(position, GUIContent.none, false, false, false, false);
                    break;
            }

            if (newValue != value) {
                GUI.changed = true;
                value = newValue;
            }

            return value;
        }

        #endregion


        #region Sorting Layer Field

        private static void PopulateSortingLayerFieldMenu(ICustomPopupContext<int> context)
        {
            var popup = context.Popup;

            foreach (var sortingLayer in SortingLayer.layers) {
                popup.AddOption(sortingLayer.name, context, sortingLayer.id);
            }
        }

        internal static int SortingLayerField(Rect position, GUIContent label, int sortingLayerID)
        {
            string sortingLayerName = SortingLayer.IDToName(sortingLayerID);
            using (var valueLabel = ControlContent.Basic(sortingLayerName)) {
                return CustomPopupGUI.Popup(position, label, sortingLayerID, valueLabel, PopulateSortingLayerFieldMenu);
            }
        }

        internal static void SortingLayerField(Rect position, SerializedProperty property, GUIContent label)
        {
            bool initialShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            int newValue = SortingLayerField(position, label, property.intValue);
            if (EditorGUI.EndChangeCheck()) {
                property.intValue = newValue;
            }

            EditorGUI.showMixedValue = initialShowMixedValue;

            EditorGUI.EndProperty();
        }

        internal static int SortingLayerField(GUIContent label, int sortingLayerID)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup);
            return SortingLayerField(position, label, sortingLayerID);
        }

        internal static void SortingLayerField(SerializedProperty property, GUIContent label)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup);
            SortingLayerField(position, property, label);
        }

        #endregion


        #region Asset Previews

        internal static bool DrawBrushPreviewHelper(Rect output, BrushAssetRecord record, bool selected)
        {
            // Just display question mark if brush record is missing
            if (record == null) {
                if (Event.current.type == EventType.Repaint) {
                    var labelStyle = selected
                        ? RotorzEditorStyles.Instance.SelectedPreviewLabel
                        : RotorzEditorStyles.Instance.PreviewLabel;

                    using (var tempContent = ControlContent.Basic("?")) {
                        labelStyle.Draw(output, tempContent, 0);
                    }
                }
                return true;
            }

            // Assume custom preview?
            if (record.Brush.customPreviewDesignTime && record.Brush.customPreviewImage != null) {
                GUI.DrawTexture(output, record.Brush.customPreviewImage);
                return true;
            }

            // Present the cached preview?
            var preview = AssetPreviewCache.GetAssetPreview(record.Brush);
            if (preview != null) {
                GUI.DrawTexture(output, preview);
                return true;
            }

            // Attempt to draw preview using brush descriptor
            BrushDescriptor descriptor = BrushUtility.GetDescriptor(record.Brush.GetType());
            if (descriptor != null && descriptor.DrawPreview(output, record, selected)) {
                return true;
            }

            GUI.DrawTexture(output, RotorzEditorStyles.Skin.FallbackBrushPreview);

            return false;
        }

        /// <summary>
        /// Draw brush preview to GUI.
        /// </summary>
        /// <remarks>
        /// <para>Draws name of the brush as fallback if no preview is drawn. Consider
        /// using <see cref="DrawBrushPreviewWithoutFallbackLabel(Rect, BrushAssetRecord, bool)"/>
        /// instead if this fallback behavior is not desired.</para>
        /// </remarks>
        /// <example>
        /// <para>The following code demonstrates how to draw a brush preview given a
        /// brush record.</para>
        /// <para><img src="../art/draw-brush-preview.jpg" alt="Drawing a brush preview."/></para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private Brush brush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.brush = RotorzEditorGUI.BrushField(new Rect(3, 3, 215, 20), this.brush);
        ///         var brushRecord = BrushDatabase.Instance.FindRecord(this.brush);
        ///
        ///         GUI.Label(new Rect(3, 25, 215, 20), "Preview:");
        ///
        ///         RotorzEditorGUI.DrawBrushPreview(
        ///             new Rect(0, 41, 200, 200),
        ///             brushRecord
        ///         );
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="output">Output position of brush preview.</param>
        /// <param name="record">The brush record.</param>
        /// <param name="selected">Indicates if preview is highlighted.</param>
        /// <seealso cref="DrawBrushPreviewWithoutFallbackLabel(Rect, BrushAssetRecord, bool)"/>
        public static void DrawBrushPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            // Only draw brush preview for paint events
            if (Event.current.type != EventType.Repaint || !ExtraEditorGUI.VisibleRect.Overlaps(output)) {
                return;
            }

            if (record == null) {
                using (var tempContent = ControlContent.Basic("?")) {
                    RotorzEditorStyles.Instance.PreviewLabel.Draw(output, tempContent, 0);
                }
                return;
            }

            if (!DrawBrushPreviewHelper(output, record, selected)) {
                // Fallback, just draw label
                using (var tempContent = ControlContent.Basic(record.DisplayName)) {
                    RotorzEditorStyles.Instance.PreviewLabel.Draw(output, tempContent, 0);
                }
            }

            // Draw overlay icon for alias brush
            if (record.Brush is AliasBrush) {
                GUI.DrawTexture(new Rect(output.x, output.y + output.height - 8, 8, 8), RotorzEditorStyles.Skin.Overlay_Alias);
            }
        }

        /// <summary>
        /// Draw brush preview to GUI without assuming a fallback label.
        /// </summary>
        /// <param name="output">Output position of brush preview.</param>
        /// <param name="record">The brush record.</param>
        /// <param name="selected">Indicates if preview is highlighted.</param>
        /// <seealso cref="DrawBrushPreview(Rect, BrushAssetRecord, bool)"/>
        public static void DrawBrushPreviewWithoutFallbackLabel(Rect output, BrushAssetRecord record, bool selected)
        {
            if (record == null) {
                return;
            }

            // Only draw brush preview for paint events
            if (Event.current.type != EventType.Repaint || !ExtraEditorGUI.VisibleRect.Overlaps(output)) {
                return;
            }

            if (!DrawBrushPreviewHelper(output, record, selected)) {
                return;
            }

            // Draw overlay icon for alias brush
            if (record.Brush is AliasBrush) {
                GUI.DrawTexture(new Rect(output.x, output.y + output.height - 8, 8, 8), RotorzEditorStyles.Skin.Overlay_Alias);
            }
        }

        /// <inheritdoc cref="DrawBrushPreview(Rect, BrushAssetRecord, bool)"/>
        public static void DrawBrushPreview(Rect output, BrushAssetRecord record)
        {
            DrawBrushPreview(output, record, false);
        }

        /// <summary>
        /// Draw brush preview to GUI.
        /// </summary>
        /// <example>
        /// <para>The following code demonstrates how to draw a brush preview.</para>
        /// <para><img src="../art/draw-brush-preview.jpg" alt="Drawing a brush preview."/></para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class TestWindow : EditorWindow
        /// {
        ///     [MenuItem("Examples/GUI")]
        ///     private static void Show()
        ///     {
        ///         GetWindow<TestWindow>();
        ///     }
        ///
        ///
        ///     private Brush brush;
        ///
        ///
        ///     private void OnGUI()
        ///     {
        ///         this.brush = RotorzEditorGUI.BrushField(new Rect(3, 3, 215, 20), this.brush);
        ///
        ///         GUI.Label(new Rect(3, 25, 215, 20), "Preview:");
        ///
        ///         RotorzEditorGUI.DrawBrushPreview(
        ///             new Rect(0, 41, 200, 200),
        ///             this.brush
        ///         );
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="output">Output position of brush preview.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="selected">Indicates if preview is highlighted.</param>
        public static void DrawBrushPreview(Rect output, Brush brush, bool selected)
        {
            DrawBrushPreview(output, BrushDatabase.Instance.FindRecord(brush), selected);
        }

        /// <inheritdoc cref="DrawBrushPreview(Rect, Brush, bool)"/>
        public static void DrawBrushPreview(Rect output, Brush brush)
        {
            DrawBrushPreview(output, brush, false);
        }

        #endregion


        #region Information Boxes (Alternative)

        internal static void InfoBox(string message, MessageType type = MessageType.Info)
        {
            EditorGUILayout.HelpBox(message, type);
        }

        internal static bool InfoBoxClosable(string message, MessageType type = MessageType.Info)
        {
            bool stayOpen = true;

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(message, type);
            if (GUILayout.Button(GUIContent.none, RotorzEditorStyles.Instance.SmallRemoveButton)) {
                stayOpen = false;
            }
            GUILayout.EndHorizontal();

            return stayOpen;
        }

        #endregion


        #region Localization

        /// <summary>
        /// Sets <see cref="EditorGUIUtility.labelWidth"/> so that it fills approximately
        /// 50% of the available horizontal space.
        /// </summary>
        internal static void UseExtendedLabelWidthForLocalization()
        {
            EditorGUIUtility.labelWidth = ExtraEditorGUI.VisibleRect.width * 0.48f;
        }

        #endregion


        #region AssetPathTextField

        private static int s_RelativeAssetPathTextFieldHint = "EditorTextField".GetHashCode();

        // Note: `path` is relative to "{Project}/Assets/"
        internal static string RelativeAssetPathTextField(GUIContent label, Rect position, string path, string extension, bool showClearButton = true)
        {
            using (var assetPathPrefixContent = ControlContent.Basic("Assets/")) {
                // Display a prefix label?
                if (label != null && label != GUIContent.none) {
                    position = EditorGUI.PrefixLabel(position, label);
                }

                var fieldStyle = RotorzEditorStyles.Instance.TextFieldRoundEdge;
                var textStyle = RotorzEditorStyles.Instance.TransparentTextField;
                var buttonStyle = showClearButton && path != "" ? RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButton : RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButtonEmpty;

                int controlID = EditorGUIUtility.GetControlID(s_RelativeAssetPathTextFieldHint, FocusType.Passive);
                int realTextControlID = controlID + 1;

                // Make room for cancel button!
                position.width -= buttonStyle.fixedWidth;

                // Draw background for text field.
                if (Event.current.type == EventType.Repaint) {
                    GUI.contentColor = EditorGUIUtility.isProSkin ? Color.black : new Color(0f, 0f, 0f, 0.5f);
                    fieldStyle.Draw(position, assetPathPrefixContent, realTextControlID);
                    GUI.contentColor = Color.white;
                }

                // Draw actual text field.
                Rect textPosition = position;
                float prefixWidth = fieldStyle.CalcSize(assetPathPrefixContent).x - 2;
                textPosition.x += prefixWidth;
                textPosition.y += 1;
                textPosition.width -= prefixWidth;

                EditorGUI.BeginChangeCheck();
                path = EditorGUI.TextField(textPosition, path, textStyle);
                if (EditorGUI.EndChangeCheck()) {
                    // Normalize directory separation characters.
                    path = path.Replace('\\', '/');
                }

                // Draw trailing extension!
                if (Event.current.type == EventType.Repaint) {
                    Rect extensionPosition = textPosition;
                    float pathWidth = CalcRelativeAssetPathTextFieldWidth(textStyle, path);
                    extensionPosition.x += pathWidth;
                    extensionPosition.width -= pathWidth;

                    GUI.contentColor = EditorGUIUtility.isProSkin ? Color.black : new Color(0f, 0f, 0f, 0.5f);
                    EditorStyles.label.Draw(extensionPosition, extension, false, false, false, false);
                    GUI.contentColor = Color.white;
                }

                // Displays clear input button or end of text field.
                position.x += position.width;
                position.width = buttonStyle.fixedWidth;
                position.height = buttonStyle.fixedHeight;
                if (GUI.Button(position, GUIContent.none, buttonStyle)) {
                    if (path != "") {
                        path = "";
                        GUI.changed = true;
                        GUIUtility.keyboardControl = 0;
                    }
                }

                return path;
            }
        }

        internal static string RelativeAssetPathTextField(GUIContent label, string path, string extension, bool showClearButton = true)
        {
            Rect position = EditorGUILayout.GetControlRect();
            return RotorzEditorGUI.RelativeAssetPathTextField(label, position, path, extension, showClearButton);
        }

        internal static string RelativeAssetPathTextField(GUIContent label, Rect position, string path, bool showClearButton = true)
        {
            return RelativeAssetPathTextField(label, position, path, "", showClearButton);
        }

        internal static string RelativeAssetPathTextField(GUIContent label, string path, bool showClearButton = true)
        {
            return RelativeAssetPathTextField(label, path, "", showClearButton);
        }

        private static float CalcRelativeAssetPathTextFieldWidth(GUIStyle style, string text)
        {
            using (var tempContent1 = ControlContent.Basic(text + "."))
            using (var tempContent2 = ControlContent.Basic(".")) {
                return style.CalcSize(tempContent1).x - EditorStyles.whiteLabel.CalcSize(tempContent2).x;
            }
        }

        #endregion


        #region Vertical Label

        internal static void VerticalLabel(Rect position, GUIContent content, GUIStyle style)
        {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            Matrix4x4 restoreMatrix = GUI.matrix;

            GUIUtility.RotateAroundPivot(90f, position.position);
            if (SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 9.0")) {
                GUI.matrix *= Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0f), Quaternion.identity, Vector3.one);
            }

            float temp = position.height;
            position.height = position.width;
            position.width = temp;
            position.x -= position.width;
            position.y -= position.height;

            GUI.matrix *= Matrix4x4.TRS(new Vector3(position.width, 0), Quaternion.identity, Vector3.one);

            style.Draw(position, content, false, false, false, false);

            GUI.matrix = restoreMatrix;
        }

        internal static void VerticalLabel(Rect position, string label, GUIStyle style)
        {
            using (var content = ControlContent.Basic(label)) {
                VerticalLabel(position, content, style);
            }
        }

        #endregion


        #region Misc

        internal static void ClearControlFocus()
        {
            if (Event.current != null) {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
            }
        }

        #endregion
    }
}
