// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal sealed class CleanupTilesetMeshesWindow : RotorzWindow
    {
        #region Window Management

        public static void ShowWindow(Tileset tileset)
        {
            var window = GetUtilityWindow<CleanupTilesetMeshesWindow>(
                title: string.Format("{0} '{1}'", TileLang.ParticularText("Action", "Cleanup Non-Procedural Meshes"), tileset.name)
            );

            window.tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(tileset);
            window.headingText = "   " + window.tilesetRecord.DisplayName;

            window.ShowAuxWindow();
        }

        #endregion


        private TilesetAssetRecord tilesetRecord;
        private string headingText;

        private GUIStyle paddedAreaStyle;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.InitialSize = this.maxSize = this.minSize = new Vector2(450, 218);

            this.paddedAreaStyle = new GUIStyle();
            this.paddedAreaStyle.padding = new RectOffset(10, 15, 0, 0);
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(69);
            GUI.DrawTexture(new Rect(10, 10, 59, 52), RotorzEditorStyles.Skin.Caution);

            GUILayout.BeginVertical(this.paddedAreaStyle);
            {
                GUILayout.BeginVertical();
                {
                    RotorzEditorGUI.Title(TileLang.ParticularText("Action", "Cleanup Non-Procedural Meshes"));
                    GUILayout.Space(5);
                    GUILayout.Label(this.headingText, RotorzEditorStyles.Instance.BoldLabel);
                    GUILayout.Space(7);
                    GUILayout.Label(TileLang.Text("Mesh assets are generated for non-procedural tileset brushes and often not needed after brushes are deleted.\n\nMeshes not referenced by at least one tileset brush can be removed. Mesh will be missing for previously painted tiles."), EditorStyles.wordWrappedLabel);
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            this.OnGUI_ButtonStrip();
        }

        private void OnGUI_ButtonStrip()
        {
            ExtraEditorGUI.Separator(marginTop: 0, marginBottom: 10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Cleanup"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnButtonCleanup();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(3);
            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButton)) {
                this.Close();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
        }

        private void OnButtonCleanup()
        {
            if (this.tilesetRecord.Tileset != null) {
                BrushUtility.CleanupTilesetMeshes(this.tilesetRecord.Tileset);
            }

            this.Close();
        }
    }
}
