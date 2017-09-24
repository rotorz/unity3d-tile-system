// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal sealed class DeleteTilesetWindow : RotorzWindow
    {
        #region Window Management

        internal static void ShowWindow(Tileset tileset)
        {
            var window = GetUtilityWindow<DeleteTilesetWindow>(
                title: string.Format("{0} '{1}'", TileLang.ParticularText("Action", "Delete Tileset"), tileset.name)
            );

            window.tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(tileset);
            window.headingText = "   " + window.tilesetRecord.DisplayName;

            window.ShowAuxWindow();
        }

        #endregion


        private TilesetAssetRecord tilesetRecord;
        private string headingText;

        private GUIStyle paddedArea1Style;
        private GUIStyle paddedArea2Style;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.InitialSize = this.maxSize = this.minSize = new Vector2(420, 278);

            this.paddedArea1Style = new GUIStyle();
            this.paddedArea1Style.padding = new RectOffset(10, 15, 0, 0);

            this.paddedArea2Style = new GUIStyle();
            this.paddedArea2Style.padding = new RectOffset(15, 0, 0, 0);
        }


        private bool shouldDeleteAtlasTexture;
        private bool shouldDeleteAtlasMaterial;
        private bool shouldDeleteMeshes;


        /// <inheritdoc/>
        protected override void DoGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Space(69);
            GUI.DrawTexture(new Rect(10, 10, 59, 52), RotorzEditorStyles.Skin.Caution);

            var tileset = this.tilesetRecord.Tileset;
            var autotileTileset = tileset as AutotileTileset;

            string assetPath = this.tilesetRecord.AssetPath;
            string assetFolderPath = assetPath.Substring(0, assetPath.LastIndexOf("/"));
            string assetFolderPathBase = assetFolderPath + "/";
            int slashCount = assetFolderPathBase.CountSubstrings('/');

            GUILayout.BeginVertical(this.paddedArea1Style);
            {
                this.OnGUI_Title();

                GUILayout.BeginVertical(this.paddedArea2Style);
                {
                    if (autotileTileset != null && autotileTileset.AtlasTexture != null) {
                        string t = AssetDatabase.GetAssetPath(tileset.AtlasTexture);
                        if (t.StartsWith(assetFolderPathBase) && slashCount == t.CountSubstrings('/')) {
                            GUILayout.Space(2);
                            this.shouldDeleteAtlasTexture = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Delete associated atlas texture"), this.shouldDeleteAtlasTexture);
                        }
                    }

                    if (tileset.AtlasMaterial != null) {
                        string t = AssetDatabase.GetAssetPath(tileset.AtlasMaterial);
                        if (t.StartsWith(assetFolderPathBase) && slashCount == t.CountSubstrings('/')) {
                            GUILayout.Space(2);
                            this.shouldDeleteAtlasMaterial = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Delete associated material"), this.shouldDeleteAtlasMaterial);
                        }
                    }

                    if (tileset.tileMeshAsset != null) {
                        string t = AssetDatabase.GetAssetPath(tileset.tileMeshAsset);
                        if (t.StartsWith(assetFolderPathBase) && slashCount == t.CountSubstrings('/')) {
                            GUILayout.Space(2);
                            this.shouldDeleteMeshes = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Delete non-procedural mesh assets"), this.shouldDeleteMeshes);
                        }
                    }
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            this.OnGUI_ButtonStrip();
        }

        private void OnGUI_Title()
        {
            GUILayout.BeginVertical();
            {
                RotorzEditorGUI.Title(TileLang.ParticularText("Action", "Delete Tileset"));
                GUILayout.Space(5);
                GUILayout.Label(this.headingText, RotorzEditorStyles.Instance.BoldLabel);
                GUILayout.Space(7);
                GUILayout.Label(TileLang.Text("Caution, proceeding will delete tileset which will cause damage to any scenes or assets that require it.\n\nAll contained brushes will also be deleted."), EditorStyles.wordWrappedLabel);
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();

            ExtraEditorGUI.SeparatorLight(thickness: 3);
        }

        private void OnGUI_ButtonStrip()
        {
            ExtraEditorGUI.Separator(marginBottom: 10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Delete"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnButtonDelete();
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

        private void OnButtonDelete()
        {
            if (this.tilesetRecord.Tileset != null) {
                DeleteTilesetFlag flags = 0;

                if (this.shouldDeleteAtlasTexture) {
                    flags |= DeleteTilesetFlag.DeleteTexture;
                }
                if (this.shouldDeleteAtlasMaterial) {
                    flags |= DeleteTilesetFlag.DeleteMaterial;
                }
                if (this.shouldDeleteMeshes) {
                    flags |= DeleteTilesetFlag.DeleteMeshAssets;
                }

                BrushUtility.DeleteTileset(this.tilesetRecord.Tileset, flags);
            }

            this.Close();
        }
    }
}
