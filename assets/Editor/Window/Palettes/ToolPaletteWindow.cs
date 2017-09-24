// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Tool palette provides the interface for selecting and configuring tools
    /// which can be used to interact with tile systems.
    /// </summary>
    /// <remarks>
    /// <para>Custom tools can be registered using the <see cref="Rotorz.Tile.Editor.ToolManager"/>
    /// class and likewise the provided tools can be unregistered if they are
    /// not wanted.</para>
    /// </remarks>
    /// <seealso cref="Rotorz.Tile.Editor.ToolUtility"/>
    /// <seealso cref="Rotorz.Tile.Editor.ToolManager"/>
    /// <seealso cref="Rotorz.Tile.Editor.ToolBase"/>
    internal sealed class ToolPaletteWindow : RotorzWindow, IHasCustomMenu
    {
        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.wantsMouseMove = true;

            this.titleContent = new GUIContent(TileLang.Text("Tools"));
            this.minSize = new Vector2(255, 80);
        }

        /// <inheritdoc/>
        protected override void DoDestroy()
        {
            base.DoDestroy();

            ToolManager.Instance.CurrentTool = null;
        }


        private bool clearInputFocus = false;

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            // Generate a control ID for the palette window so that keyboard focus can be
            // easily removed from the active control.
            int paletteControlID = EditorGUIUtility.GetControlID(FocusType.Keyboard);
            if (this.clearInputFocus || Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                this.clearInputFocus = false;
                EditorGUIUtility.keyboardControl = paletteControlID;
                this.Repaint();
            }

            if (GUIUtility.keyboardControl == paletteControlID) {
                ToolManager.Instance.CheckForKeyboardShortcut();
                ToolUtility.CheckToolKeyboardShortcuts();
            }

            this.OnToolSelectorGUI();

            RotorzEditorGUI.DrawHoverTip(this);
        }


        #region Tool Selection

        private static GUIStyle s_ToolButtonStyle;

        [NonSerialized]
        private List<ToolBase> filteredToolList = new List<ToolBase>();

        private void FilterRegisteredTools()
        {
            var tools = ToolManager.Instance.Tools;
            int totalCount = tools.Count;

            this.filteredToolList.Clear();

            for (int i = 0; i < totalCount; ++i) {
                var tool = tools[i];
                if (tool.Visible) {
                    this.filteredToolList.Add(tool);
                }
            }
        }

        private Vector2 scrolling;

        private bool ToolButton(GUIContent content, bool on = false)
        {
            Rect position = GUILayoutUtility.GetRect(content, s_ToolButtonStyle);

            int controlID = RotorzEditorGUI.GetHoverControlID(position, content.image != null ? content.text : null);
            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (Event.current.button == 0 && position.Contains(Event.current.mousePosition)) {
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
                    if (content.image != null) {
                        using (var tempContent = ControlContent.Basic(content.image)) {
                            s_ToolButtonStyle.Draw(position, tempContent, controlID, on);
                        }
                    }
                    else {
                        using (var tempContent = ControlContent.Basic(content.text)) {
                            s_ToolButtonStyle.Draw(position, tempContent, controlID, on);
                        }
                    }
                    break;
            }

            return false;
        }

        private Rect menuPosition;

        private void OnToolSelectorGUI()
        {
            GUILayout.Space(6);

            this.FilterRegisteredTools();

            // Calculate metrics.
            int buttonColumns = Screen.width / 46;
            if (buttonColumns > this.filteredToolList.Count + 1) {
                buttonColumns = this.filteredToolList.Count + 1;
            }

            // Prepare style for tool button.
            if (s_ToolButtonStyle == null) {
                s_ToolButtonStyle = new GUIStyle(RotorzEditorStyles.Instance.ToolButton);
            }
            s_ToolButtonStyle.fixedWidth = Mathf.FloorToInt((float)Screen.width / (float)buttonColumns) - 3;

            // Display tool items.
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.MenuButton,
                TileLang.ParticularText("Action", "Main Menu")
            )) {
                if (EditorInternalUtility.DropdownMenu(content, s_ToolButtonStyle)) {
                    EditorUtility.DisplayPopupMenu(this.menuPosition, "CONTEXT/_RTS_TOOLS_", new MenuCommand(this, 0));
                }
                if (Event.current.type == EventType.Repaint) {
                    this.menuPosition = GUILayoutUtility.GetLastRect();
                }
            }

            int currentColumn = 1;

            for (int i = 0; i < this.filteredToolList.Count; ++i) {
                var tool = this.filteredToolList[i];

                // Place tool button at start of new row upon overflowing width of palette.
                if (currentColumn++ >= buttonColumns) {
                    currentColumn = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4);
                }

                bool selected = ToolManager.Instance.CurrentTool == tool;

                // Get label for tool button and select icon based upon tool selection.
                var toolIconTexture = selected ? tool.IconActive : tool.IconNormal;
                using (var buttonContent = ControlContent.Basic(tool.Label, toolIconTexture)) {
                    // Fallback to 'normal' icon if 'active' icon is not specified.
                    if (selected && toolIconTexture == null) {
                        buttonContent.LabelContent.image = tool.IconNormal;
                    }

                    if (this.ToolButton(buttonContent, selected)) {
                        ToolManager.Instance.CurrentTool = !selected ? tool : null;
                    }
                }
            }
            GUILayout.EndHorizontal();

            ExtraEditorGUI.SeparatorLight(marginBottom: 0);

            this.scrolling = GUILayout.BeginScrollView(this.scrolling);

            if (ToolManager.Instance.CurrentTool != null) {
                GUILayout.Space(6);

                float restoreLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 130;
                {
                    var tool = ToolManager.Instance.CurrentTool;
                    tool.OnToolOptionsGUI();

                    if (tool.HasAdvancedToolOptionsGUI) {
                        GUILayout.Space(-2);

                        tool.ShowAdvancedOptionsGUI = GUILayout.Toggle(tool.ShowAdvancedOptionsGUI, TileLang.ParticularText("Section", "Advanced"), RotorzEditorStyles.Instance.FlatToggle);
                        if (tool.ShowAdvancedOptionsGUI) {
                            tool.OnAdvancedToolOptionsGUI();
                        }
                    }
                }
                EditorGUIUtility.labelWidth = restoreLabelWidth;
            }
            else {
                GUILayout.Space(6 + 2);
                GUILayout.Label(TileLang.Text("No tool selected"), EditorStyles.miniLabel);
            }

            GUILayout.Space(3);
            GUILayout.FlexibleSpace();

            GUILayout.EndScrollView();
        }

        #endregion


        #region IHasCustomMenu Members

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(TileLang.ParticularText("Action", "Reset Tool Options")), false, () => {
                this.clearInputFocus = true;

                foreach (var tool in ToolManager.Instance.Tools) {
                    tool.Options.RestoreDefaultValues();
                }
                ToolUtility.RepaintToolPalette();
            });
        }

        #endregion
    }
}
