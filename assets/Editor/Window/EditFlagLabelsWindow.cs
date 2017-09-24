// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Window for specifying custom flag labels for brushes.
    /// </summary>
    internal sealed class EditFlagLabelsWindow : RotorzWindow
    {
        #region Window Management

        /// <summary>
        /// Display edit flag labels window.
        /// </summary>
        /// <param name="brush">Brush.</param>
        public static EditFlagLabelsWindow ShowWindow(Brush brush)
        {
            string windowTitle = (brush != null)
                ? string.Format(
                    /* 0: name of brush */
                    TileLang.Text("Flag Labels for '{0}'"),
                    brush.name
                )
                : TileLang.Text("Flag Labels");

            var window = GetUtilityWindow<EditFlagLabelsWindow>(
                title: windowTitle
            );
            window.Init(brush);

            return window;
        }

        #endregion


        private class Tab
        {
            public GUIContent Text;
            public string[] FlagLabels;
            public string Description;


            public Tab(string text, string[] flagLabels, string description)
            {
                this.Text = new GUIContent(text);
                this.FlagLabels = flagLabels;
                this.Description = description;
            }
        }


        private static int s_SelectedTab;

        [NonSerialized]
        private Brush brush;

        [NonSerialized]
        private List<Tab> tabs;
        [NonSerialized]
        private GUIContent[] tabContent;

        [NonSerialized]
        private Tab brushTab;
        [NonSerialized]
        private Tab projectTab;

        private void Init(Brush brush)
        {
            this.brush = brush;

            this.tabs = new List<Tab>();

            if (brush != null) {
                this.tabs.Add(this.brushTab = new Tab(
                    text: TileLang.Text("Brush"),
                    flagLabels: this.brush.UserFlagLabels,
                    description: TileLang.Text("Flag labels can be customized on a per brush basis. Leave blank to assume label from project tab.")
                ));
            }

            this.tabs.Add(this.projectTab = new Tab(
                text: TileLang.Text("Project"),
                flagLabels: ProjectSettings.Instance.FlagLabels,
                description: TileLang.Text("Flag labels are shared across entire project.") + "\n"
            ));

            this.tabContent = this.tabs.Select(tab => tab.Text).ToArray();
        }

        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.InitialSize = this.maxSize = this.minSize = new Vector2(400, 285);
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            s_SelectedTab = Mathf.Clamp(RotorzEditorGUI.TabSelector(s_SelectedTab, this.tabContent), 0, this.tabs.Count - 1);

            GUILayout.Space(4);

            GUILayout.Label(this.tabs[s_SelectedTab].Description, EditorStyles.wordWrappedLabel);

            GUILayout.Space(5);

            EditorGUIUtility.labelWidth = 70;
            GUILayout.BeginHorizontal();

            this.DrawFlagFields(0, 7);
            GUILayout.Space(5);
            this.DrawFlagFields(8, 15);

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            ExtraEditorGUI.Separator(marginTop: 0);

            this.OnGUI_Buttons();
            GUILayout.Space(5);
        }

        private void DrawFlagFields(int fromIndex, int toIndex)
        {
            GUILayout.BeginVertical(GUILayout.Width(190));

            string[] labels = this.tabs[s_SelectedTab].FlagLabels;

            for (int flagIndex = fromIndex; flagIndex <= toIndex; ++flagIndex) {
                GUILayout.BeginHorizontal();

                using (var content = ControlContent.Basic(
                    /* 0: number of flag */
                    string.Format(TileLang.Text("Flag #{0}"), flagIndex + 1)
                )) {
                    labels[flagIndex] = EditorGUILayout.TextField(content, labels[flagIndex] ?? "", RotorzEditorStyles.Instance.TextFieldRoundEdge)
                        .Replace(";", "");
                }

                if (string.IsNullOrEmpty(labels[flagIndex])) {
                    GUILayout.Label(GUIContent.none, RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButtonEmpty);
                }
                else {
                    if (GUILayout.Button(GUIContent.none, RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButton)) {
                        labels[flagIndex] = string.Empty;
                        GUIUtility.keyboardControl = 0;
                        GUIUtility.ExitGUI();
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void OnGUI_Buttons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Save"), ExtraEditorStyles.Instance.BigButton, RotorzEditorStyles.ContractWidth)) {
                if (this.brush != null && this.brushTab != null) {
                    this.brush.UserFlagLabels = this.brushTab.FlagLabels;
                    EditorUtility.SetDirty(this.brush);
                }

                ProjectSettings.Instance.FlagLabels = this.projectTab.FlagLabels;

                DesignerWindow.RepaintWindow();
                this.Close();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButton, RotorzEditorStyles.ContractWidth)) {
                this.Close();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
        }
    }
}
