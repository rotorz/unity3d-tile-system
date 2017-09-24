// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Identifies the target tile system(s) where replacement should take place.
    /// </summary>
    internal enum ReplaceByBrushTarget
    {
        /// <summary>
        /// Find and replace tiles in the active tile system.
        /// </summary>
        ActiveTileSystem = 0,

        /// <summary>
        /// Find and replace tiles in all selected tile systems.
        /// </summary>
        SelectedTileSystems = 1,

        /// <summary>
        /// Find and replace tiles in all tile systems.
        /// </summary>
        All = 2,
    }


    /// <summary>
    /// Find and replace brush selection delegate.
    /// </summary>
    /// <param name="target">Identifies the target tile system(s) where replacement should take place.</param>
    /// <param name="source">Search for tiles that were painted with source brush.</param>
    /// <param name="replacement">Repaint matching tiles with replacement brush.</param>
    internal delegate void FindReplaceDelegate(ReplaceByBrushTarget target, Brush source, Brush replacement);


    /// <summary>
    /// Replace by brush window.
    /// </summary>
    /// <remarks>
    /// Provides user interface to find tiles that were painted using a specific brush and
    /// then replace them using another. Tiles can also be erased by brush if desired.
    /// </remarks>
    /// <seealso cref="TileSystem.ReplaceByBrush"/>
    internal sealed class ReplaceByBrushWindow : RotorzWindow
    {
        #region Window Management

        /// <summary>
        /// Display the replace by brush window.
        /// </summary>
        /// <param name="callback">Callback is invoked when brushes have been selected.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static ReplaceByBrushWindow ShowWindow(FindReplaceDelegate callback)
        {
            var window = GetUtilityWindow<ReplaceByBrushWindow>();

            window.OnFindReplace += callback;

            return window;
        }

        #endregion


        /// <summary>
        /// Occurs when find and replace is undertaken.
        /// </summary>
        public event FindReplaceDelegate OnFindReplace;


        [SerializeField]
        private Brush sourceBrush;
        [SerializeField]
        private Brush replacementBrush;

        [NonSerialized]
        private GUIStyle arrowStyle;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(TileLang.ParticularText("Action", "Replace Tiles by Brush"));
            this.InitialSize = this.minSize = this.maxSize = new Vector2(450, 243);

            // Initialize "Source" brush to the selected brush.
            this.sourceBrush = ToolUtility.SelectedBrush;

            this.CheckSelection(true);
        }


        private bool hasActiveSystem = false;
        private int selectedSystemCount = 0;
        private ReplaceByBrushTarget targetSystems = ReplaceByBrushTarget.All;

        internal void CheckSelection(bool autoAdjust = false)
        {
            this.hasActiveSystem = (ToolUtility.ActiveTileSystem != null);
            this.selectedSystemCount = Selection.GetFiltered(typeof(TileSystem), SelectionMode.ExcludePrefab).Length;

            if (this.targetSystems == ReplaceByBrushTarget.ActiveTileSystem && !this.hasActiveSystem) {
                ++this.targetSystems;
            }
            if (this.targetSystems == ReplaceByBrushTarget.SelectedTileSystems && this.selectedSystemCount == 0) {
                ++this.targetSystems;
            }

            if (autoAdjust) {
                if (this.selectedSystemCount > 1) {
                    this.targetSystems = ReplaceByBrushTarget.SelectedTileSystems;
                }
                else if (this.hasActiveSystem) {
                    this.targetSystems = ReplaceByBrushTarget.ActiveTileSystem;
                }
            }
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            GUILayout.Space(10);

            if (Event.current.type == EventType.Layout) {
                this.CheckSelection();
            }

            GUILayout.BeginHorizontal(GUILayout.Height(200));
            GUILayout.Space(5);
            this.OnGUI_BrushSelection();
            GUILayout.Space(10);
            this.OnGUI_Buttons(Event.current);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            GUILayout.Label(TileLang.Text("Apply To Tile System(s):"));
            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(!this.hasActiveSystem);
            if (GUILayout.Toggle(this.targetSystems == ReplaceByBrushTarget.ActiveTileSystem, TileLang.ParticularText("Selection", "Active"), EditorStyles.radioButton)) {
                this.targetSystems = ReplaceByBrushTarget.ActiveTileSystem;
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(this.selectedSystemCount <= 0);
            if (GUILayout.Toggle(this.targetSystems == ReplaceByBrushTarget.SelectedTileSystems, TileLang.ParticularText("Selection", "Selected"), EditorStyles.radioButton)) {
                this.targetSystems = ReplaceByBrushTarget.SelectedTileSystems;
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            if (GUILayout.Toggle(this.targetSystems == ReplaceByBrushTarget.All, TileLang.ParticularText("Selection", "All"), EditorStyles.radioButton)) {
                this.targetSystems = ReplaceByBrushTarget.All;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnSelectionChange()
        {
            this.CheckSelection(true);
            this.Repaint();
        }

        private void OnGUI_BrushSelection()
        {
            if (this.arrowStyle == null) {
                this.arrowStyle = new GUIStyle(GUI.skin.label);
                this.arrowStyle.stretchHeight = true;
                this.arrowStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUILayout.BeginVertical();
            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Source Brush"), RotorzEditorStyles.Instance.BoldLabel);
            {
                this.sourceBrush = RotorzEditorGUI.BrushField(this.sourceBrush);
                EditorGUILayout.Space();
                this.sourceBrush = this.DrawBrushPreviewField(this.sourceBrush);
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.Label("=>", this.arrowStyle);
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Replacement Brush"), RotorzEditorStyles.Instance.BoldLabel);
            {
                this.replacementBrush = RotorzEditorGUI.BrushField(this.replacementBrush);
                EditorGUILayout.Space();
                this.replacementBrush = this.DrawBrushPreviewField(this.replacementBrush);
            }
            GUILayout.EndVertical();
        }

        private Brush DrawBrushPreviewField(Brush brush)
        {
            Rect position = GUILayoutUtility.GetRect(144, 144, 144, 144);

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;

            Brush draggedBrush = DragAndDrop.objectReferences.Length > 0
                ? DragAndDrop.objectReferences[0] as Brush
                : null;

            switch (e.GetTypeForControl(controlID)) {
                case EventType.Repaint:
                    Color restore = GUI.backgroundColor;
                    if (DragAndDrop.activeControlID == controlID) {
                        GUI.backgroundColor = new Color32(61, 128, 223, 255);
                    }

                    RotorzEditorStyles.Instance.TransparentBox.Draw(position, GUIContent.none, false, false, false, false);
                    GUI.backgroundColor = restore;

                    position.x += 2;
                    position.y += 2;
                    position.width -= 4;
                    position.height -= 4;
                    RotorzEditorGUI.DrawBrushPreview(position, brush);
                    break;

                case EventType.DragUpdated:
                    if (draggedBrush != null && position.Contains(e.mousePosition)) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        DragAndDrop.activeControlID = controlID;
                        GUIUtility.hotControl = 0;
                        this.Repaint();
                        e.Use();
                    }
                    break;

                case EventType.DragExited:
                    this.Repaint();
                    break;

                case EventType.DragPerform:
                    if (draggedBrush != null && position.Contains(e.mousePosition)) {
                        brush = draggedBrush;

                        DragAndDrop.AcceptDrag();
                        GUIUtility.hotControl = 0;

                        e.Use();
                    }
                    break;
            }

            return brush;
        }

        private void OnGUI_Buttons(Event e)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Replace"), ExtraEditorStyles.Instance.BigButton) || (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)) {
                this.OnButton_Replace();
            }

            GUILayout.Space(3);

            if (GUILayout.Button(TileLang.ParticularText("Action", "Close"), ExtraEditorStyles.Instance.BigButton)) {
                this.Close();
            }

            GUILayout.EndVertical();
        }

        private void OnButton_Replace()
        {
            // Verify inputs!
            if (this.sourceBrush == null) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "No source brush specified"),
                    TileLang.Text("Please select source brush to find tiles with."),
                    TileLang.ParticularText("Action", "Close")
                );
                return;
            }

            if (this.sourceBrush == this.replacementBrush) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "Source and replacement brushes are the same"),
                    TileLang.ParticularText("Error", "Cannot find and replace with same brush."),
                    TileLang.ParticularText("Action", "Close")
                    );
                return;
            }

            // Perform find and replace.
            if (this.OnFindReplace != null) {
                this.OnFindReplace(this.targetSystems, this.sourceBrush, this.replacementBrush);
            }
        }
    }
}
