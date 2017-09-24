// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Define orientation delegate.
    /// </summary>
    /// <param name="orientation">Bit representation of orientation.</param>
    public delegate void DefineOrientationDelegate(int orientation);

    /// <summary>
    /// Define orientation delegate.
    /// </summary>
    /// <param name="orientation">Bit representation of orientation.</param>
    /// <param name="rotationalSymmetry">Indicates whether orientation will have rotation symmetry.</param>
    public delegate void DefineOrientationDelegate2(int orientation, bool rotationalSymmetry);


    /// <summary>
    /// User interface for selecting an orientation.
    /// </summary>
    /// <intro>
    /// <para><img src="../art/select-orientation-window.png"/></para>
    /// <para>See <see cref="ShowWindow(DefineOrientationDelegate)"/> for example of usage.</para>
    /// </intro>
    public sealed class DefineOrientationWindow : EditorWindow
    {
        /// <summary>
        /// The define orientation window instance.
        /// </summary>
        private static DefineOrientationWindow Instance { get; set; }


        #region Window Management

        /// <summary>
        /// Gets owner of define orientation window.
        /// </summary>
        /// <remarks>
        /// <para>This is the window which was focused when orientation definition window
        /// was shown. Owner window is automatically focused upon making a selection.</para>
        /// </remarks>
        internal static EditorWindow OwnerWindow { get; private set; }

        /// <summary>
        /// Initialize window for display as drop-down or auxiliary window.
        /// </summary>
        /// <param name="height">Height of window in pixels.</param>
        private static void Init(int height)
        {
            OwnerWindow = EditorWindow.focusedWindow;

            if (Instance == null) {
                Instance = CreateInstance<DefineOrientationWindow>();

                Vector2 size = new Vector2(335, height);
                Instance.minSize = size;
                Instance.maxSize = size;
            }
        }

        private static void DoShowWindow(string title)
        {
            Init(219);

            Instance.onDefineOrientation = null;
            Instance.onDefineOrientation2 = null;

            Instance.ShownAsDropDown = false;
            Instance.titleContent.text = title;

            // Display window in center of screen.
            Vector2 size = Instance.minSize;
            Instance.position = new Rect(
                (Screen.currentResolution.width - size.x) / 2,
                (Screen.currentResolution.height - size.y) / 2,
                size.x,
                size.y
            );

            Instance.ShowAuxWindow();
        }

        /// <summary>
        /// Display define orientation selection window with custom title.
        /// </summary>
        /// <example>
        /// <para>The following source code demonstrates how to use this window with a custom title:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public static class SelectOrientationExample
        /// {
        ///     [MenuItem("Window/Select Orientation Example")]
        ///     private static void DoExample()
        ///     {
        ///         DefineOrientationWindow.ShowWindow("Find Orientation", OnSelectOrientation);
        ///     }
        ///
        ///     private static void OnSelectOrientation(int orientation)
        ///     {
        ///         Debug.Log("Orientation Mask: " + orientation);
        ///         Debug.Log("Orientation Name: " + OrientationUtility.NameFromMask(orientation));
        ///     }
        ///
        /// }
        /// ]]></code>
        ///
        /// <para>Orientation selection window can also be prefilled with an orientation upon being shown:</para>
        /// <code language="csharp"><![CDATA[
        /// var window = DefineOrientationWindow.ShowWindow("Find Orientation", OnSelectOrientation);
        /// window.Orientation = OrientationUtility.MaskFromName("11100111");
        /// ]]></code>
        ///
        /// </example>
        /// <param name="title">Title for window.</param>
        /// <param name="callback">Invoked when orientation is defined.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static DefineOrientationWindow ShowWindow(string title, DefineOrientationDelegate callback)
        {
            DoShowWindow(title);
            Instance.onDefineOrientation = callback;
            return Instance;
        }

        /// <summary>
        /// Display orientation selection window.
        /// </summary>
        /// <example>
        /// <para>The following source code demonstrates how to use this window:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public static class SelectOrientationExample
        /// {
        ///     [MenuItem("Window/Select Orientation Example")]
        ///     private static void DoExample()
        ///     {
        ///         DefineOrientationWindow.ShowWindow(OnSelectOrientation);
        ///     }
        ///
        ///     private static void OnSelectOrientation(int orientation)
        ///     {
        ///         Debug.Log("Orientation Mask: " + orientation);
        ///         Debug.Log("Orientation Name: " + OrientationUtility.NameFromMask(orientation));
        ///     }
        /// }
        /// ]]></code>
        ///
        /// <para>Orientation selection window can also be prefilled with an orientation upon being shown:</para>
        /// <code language="csharp"><![CDATA[
        /// DefineOrientationWindow window = DefineOrientationWindow.ShowWindow(OnSelectOrientation);
        /// window.Orientation = OrientationUtility.MaskFromName("11100111");
        /// ]]></code>
        ///
        /// </example>
        /// <param name="callback">Invoked when orientation is defined.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static DefineOrientationWindow ShowWindow(DefineOrientationDelegate callback)
        {
            return ShowWindow(TileLang.ParticularText("Action", "Select Orientation"), callback);
        }

        /// <summary>
        /// Display define orientation selection window with custom title.
        /// </summary>
        /// <param name="title">Title for window.</param>
        /// <param name="callback">Invoked when orientation is defined.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static DefineOrientationWindow ShowWindow(string title, DefineOrientationDelegate2 callback)
        {
            DoShowWindow(title);
            Instance.onDefineOrientation2 = callback;
            return Instance;
        }

        /// <summary>
        /// Display orientation selection window.
        /// </summary>
        /// <param name="callback">Invoked when orientation is defined.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static DefineOrientationWindow ShowWindow(DefineOrientationDelegate2 callback)
        {
            return ShowWindow(TileLang.ParticularText("Action", "Select Orientation"), callback);
        }

        /// <summary>
        /// Show define orientation window as drop-down.
        /// </summary>
        /// <param name="buttonRect">Rectangle of button in screen space.</param>
        /// <param name="title">Title text for drop-down.</param>
        /// <param name="callback">Invoked when orientation is defined.</param>
        internal static void ShowAsDropDown(Rect buttonRect, string title, DefineOrientationDelegate2 callback)
        {
            Init(234);

            Instance.onDefineOrientation = null;
            Instance.onDefineOrientation2 = callback;

            Instance.ShownAsDropDown = true;
            Instance.titleContent.text = title;
            Instance.ShowAsDropDown(buttonRect, new Vector2(325, 234));
        }

        #endregion


        /// <summary>
        /// Indicates whether window is shown as a drop-down.
        /// </summary>
        private bool ShownAsDropDown { get; set; }

        /// <summary>
        /// Occurs when orientation is defined.
        /// </summary>
        private DefineOrientationDelegate onDefineOrientation;
        /// <summary>
        /// Occurs when orientation is defined with rotational symmetry.
        /// </summary>
        private DefineOrientationDelegate2 onDefineOrientation2;

        /// <summary>
        /// Gets or sets bit representation of defined orientation.
        /// </summary>
        public int Orientation { get; set; }
        /// <summary>
        /// Gets or sets whether rotational symmetry is selected.
        /// </summary>
        /// <remarks>
        /// <para>This only applies when a <see cref="DefineOrientationDelegate2"/> callback
        /// is specified.</para>
        /// </remarks>
        public bool RotationalSymmetry { get; set; }


        #region Messages

        private void OnEnable()
        {
            this.wantsMouseMove = true;
        }

        private void OnDisable()
        {
            this.onDefineOrientation = null;
            this.onDefineOrientation2 = null;

            if (OwnerWindow != null) {
                OwnerWindow.Repaint();
                OwnerWindow = null;
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            switch (Event.current.type) {
                case EventType.KeyDown:
                    this.OnKeyDownEvent(Event.current.keyCode);
                    break;

                case EventType.Repaint:
                    if (this.ShownAsDropDown) {
                        RotorzEditorStyles.Instance.WindowGreyBorder.Draw(new Rect(0, 0, position.width, position.height), GUIContent.none, false, false, false, false);
                        ExtraEditorGUI.SeparatorLight(new Rect(1, 21, position.width - 2, 1));
                    }
                    break;
            }

            if (this.ShownAsDropDown) {
                GUILayout.Space(-7);
                GUILayout.Label(this.titleContent, EditorStyles.boldLabel);
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal();

            GUILayout.Space(5);

            this.DrawOrientationButtons();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            {
                GUILayout.Space(2);
                this.DrawDialogButtons();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.EndHorizontal();

            RotorzEditorGUI.DrawHoverTip(this);
        }

        #endregion


        #region Dialog Buttons

        private void OnKeyDownEvent(KeyCode key)
        {
            switch (key) {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    Event.current.Use();
                    this.OnButtonOK();
                    break;

                case KeyCode.Escape:
                    Event.current.Use();
                    this.OnButtonCancel();
                    break;
            }
        }

        private void DrawDialogButtons()
        {
            if (GUILayout.Button(TileLang.ParticularText("Action", "OK"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnButtonOK();
            }

            GUILayout.Space(3);

            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnButtonCancel();
            }

            if (this.onDefineOrientation2 != null) {
                GUILayout.FlexibleSpace();

                Rect togglePosition = GUILayoutUtility.GetRect(0, 34);
                togglePosition.x += (togglePosition.width - 60) / 2;
                togglePosition.width = 60;

                using (var toggleContent = ControlContent.Basic(
                    image: this.RotationalSymmetry
                        ? RotorzEditorStyles.Skin.ToggleRotationalSymmetryOn
                        : RotorzEditorStyles.Skin.ToggleRotationalSymmetry
                        ,
                    tipText: TileLang.ParticularText("Action", "Toggle Rotational Symmetry")
                )) {
                    this.RotationalSymmetry = RotorzEditorGUI.HoverToggle(togglePosition, toggleContent, this.RotationalSymmetry, GUI.skin.button);
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Invert"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnButtonInvert();
            }
        }

        private void OnButtonOK()
        {
            if (this.onDefineOrientation != null) {
                this.onDefineOrientation(this.Orientation);
            }
            else {
                this.onDefineOrientation2(this.Orientation, this.RotationalSymmetry);
            }

            this.OnButtonCancel();
        }

        private void OnButtonCancel()
        {
            if (OwnerWindow != null) {
                OwnerWindow.Focus();
            }

            this.Close();
        }

        private void OnButtonInvert()
        {
            this.Orientation = ~this.Orientation & 0xFF;
            this.Repaint();
        }

        #endregion


        #region Orientation Selection

        private int orientationGroupControlID;

        private int lastBoxControlID;
        private bool paintBoxState;

        private void DrawOrientationButtons()
        {
            Color restoreBackground = GUI.backgroundColor;

            Rect position = GUILayoutUtility.GetRect(205, 205 - 13);
            position.x += 3;
            position.y += 1;

            Rect box = new Rect(position.x, position.y, 63, 63);

            this.orientationGroupControlID = GUIUtility.GetControlID(FocusType.Passive, position);
            switch (Event.current.GetTypeForControl(this.orientationGroupControlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        GUIUtility.hotControl = this.orientationGroupControlID;
                        this.Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == this.orientationGroupControlID) {
                        GUIUtility.hotControl = 0;
                        this.lastBoxControlID = 0;
                    }
                    break;
            }

            int bit;

            for (int i = 0; i < 8; ++i) {
                bit = 1 << i;

                if (this.BoxToggle(box, (this.Orientation & bit) != 0)) {
                    this.Orientation |= bit;
                }
                else {
                    this.Orientation &= ~bit;
                }

                box.x += box.width + 4;

                if (i == 3) {
                    // Draw center box!
                    GUI.backgroundColor = Color.black;
                    GUI.Box(box, GUIContent.none, RotorzEditorStyles.Instance.OrientationBox);

                    box.x += box.width + 4;
                }
                else if (i == 2 || i == 4) {
                    box.x = position.x;
                    box.y += box.height + 4;
                }
            }

            GUI.backgroundColor = restoreBackground;
        }

        private bool BoxToggle(Rect position, bool value)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive, position);

            Color activeColor = this.RotationalSymmetry ? new Color(0, 40, 255) : Color.green;

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        if (this.lastBoxControlID == 0) {
                            this.paintBoxState = !value;
                        }
                        this.lastBoxControlID = controlID;
                        return this.paintBoxState;
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == this.orientationGroupControlID && this.lastBoxControlID != controlID && position.Contains(Event.current.mousePosition)) {
                        this.lastBoxControlID = controlID;
                        Event.current.Use();
                        return this.paintBoxState;
                    }
                    break;

                case EventType.Repaint:
                    GUI.backgroundColor = value ? activeColor : Color.white;
                    RotorzEditorStyles.Instance.OrientationBox.Draw(position, GUIContent.none, false, false, false, false);
                    break;
            }

            return value;
        }

        #endregion
    }
}
