// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal sealed class RefreshTilesWindow : RotorzWindow
    {
        #region Window Management

        public static void ShowWindow(TileSystem system)
        {
            var window = GetUtilityWindow<RefreshTilesWindow>(
                title: string.Format(
                    /* 0: name of the tile system */
                    TileLang.ParticularText("Action", "Refresh Tiles in '{0}'"),
                    system.name
                )
            );

            window.tileSystem = system;

            window.ShowAuxWindow();
        }

        #endregion


        private TileSystem tileSystem;

        private static bool s_ForceRefreshTiles;
        private static bool s_UpdateProcedural;
        private static bool s_PreserveManualOffset = true;
        private static bool s_PreserveFlags = true;

        private GUIStyle paddedArea1Style;
        private GUIStyle paddedArea2Style;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.InitialSize = this.maxSize = this.minSize = new Vector2(400, 268);

            this.paddedArea1Style = new GUIStyle();
            this.paddedArea1Style.padding = new RectOffset(15, 15, 0, 0);

            this.paddedArea2Style = new GUIStyle();
            this.paddedArea2Style.padding = new RectOffset(15, 0, 0, 0);
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginVertical(this.paddedArea1Style);
            RotorzEditorGUI.Title(TileLang.ParticularText("Action", "Refresh Tiles"));
            GUILayout.Space(5);
            GUILayout.Label(TileLang.Text("Tiles are replaced if obvious changes are detected. You can force refresh all tiles if necessary."), EditorStyles.wordWrappedLabel);

            ExtraEditorGUI.SeparatorLight(marginTop: 5, marginBottom: 5, thickness: 3);

            GUILayout.BeginVertical(this.paddedArea2Style);
            s_ForceRefreshTiles = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Force refresh all tiles"), s_ForceRefreshTiles);

            GUILayout.Space(2);
            EditorGUI.indentLevel += 2;
            if (!s_ForceRefreshTiles) {
                s_UpdateProcedural = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Update procedural tiles"), s_UpdateProcedural);
            }
            else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Update procedural tiles"), true);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel -= 2;
            GUILayout.Space(2);
            s_PreserveManualOffset = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Preserve manual offsets"), s_PreserveManualOffset);
            GUILayout.Space(2);
            s_PreserveFlags = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Preserve painted user flags"), s_PreserveFlags);
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox(TileLang.Text("Some manual changes may be lost when refreshing tiles."), MessageType.Warning, true);

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Refresh"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.OnButtonRefresh();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(3);
            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.Close();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
        }

        private void OnButtonRefresh()
        {
            Undo.RegisterFullObjectHierarchyUndo(this.tileSystem.gameObject, TileLang.ParticularText("Action", "Refresh Tiles"));

            RefreshFlags flags = RefreshFlags.None;

            if (s_ForceRefreshTiles) {
                flags |= RefreshFlags.Force;
            }
            if (s_UpdateProcedural) {
                flags |= RefreshFlags.UpdateProcedural;
            }
            if (s_PreserveFlags) {
                flags |= RefreshFlags.PreservePaintedFlags;
            }
            if (s_PreserveManualOffset) {
                flags |= RefreshFlags.PreserveTransform;
            }

            this.tileSystem.RefreshAllTiles(flags);

            this.Close();
        }
    }
}
