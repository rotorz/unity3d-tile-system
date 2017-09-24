// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor.Internal
{
    internal class TilesetInfoTab : ITilesetDesignerTab
    {
        public TilesetAssetRecord tilesetRecord;
        public Tileset tileset;
        public AutotileTileset autotileTileset;

        private Vector2 scrollingAtlasPreview;
        private Vector2 scrollingInfo;


        public TilesetInfoTab(TilesetDesigner designer)
        {
        }


        public string Label {
            get { return TileLang.ParticularText("TilesetDesigner|TabLabel", "Info"); }
        }

        public Vector2 ScrollPosition { get; set; }


        #region GUI

        public void OnNewTilesetRecord(TilesetAssetRecord record)
        {
            this.tilesetRecord = record;
            this.tileset = record.Tileset;
            this.autotileTileset = this.tileset as AutotileTileset;
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void OnFixedHeaderGUI()
        {
        }

        public void OnGUI()
        {
            GUILayout.Space(10);

            string assetPath = Path.GetDirectoryName(this.tilesetRecord.AssetPath) + "/";

            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Asset Path:"), RotorzEditorStyles.Instance.BoldLabel);
            EditorGUILayout.SelectableLabel(assetPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            string buttonTextShowInOS = TileLang.OpensWindow(
                Application.platform == RuntimePlatform.OSXEditor
                    ? TileLang.ParticularText("Action", "Show In Finder")
                    : TileLang.ParticularText("Action", "Show In Explorer"));
            if (GUILayout.Button(buttonTextShowInOS, RotorzEditorStyles.Instance.ButtonWide)) {
                EditorUtility.OpenWithDefaultApp(assetPath);
                GUIUtility.ExitGUI();
            }

            var atlasTexture = this.tileset.AtlasTexture;
            if (atlasTexture == null) {
                GUILayout.Space(10);
                GUILayout.Label(TileLang.Text("Atlas texture is missing."));
                return;
            }

            if (atlasTexture.width != atlasTexture.height || !Mathf.IsPowerOfTwo(atlasTexture.width)) {
                GUILayout.Space(3);
                EditorGUILayout.HelpBox(TileLang.Text("Atlas texture is not square and/or not a power of two size. This can lead to poor quality results."), MessageType.Warning, true);
                GUILayout.Space(3);
            }
            else {
                GUILayout.Space(5);
            }

            this.DrawAtlasTexture();
        }

        private void DrawAtlasTexture()
        {
            GUILayout.Space(10);

            RotorzEditorGUI.Title(TileLang.Text("Atlas Texture"));
            ExtraEditorGUI.SeparatorLight(marginBottom: 0);

            this.scrollingAtlasPreview = GUILayout.BeginScrollView(this.scrollingAtlasPreview);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Box(this.tileset.AtlasTexture);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        public void OnSideGUI()
        {
            var atlasTexture = this.tileset.AtlasTexture;

            float restoreLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 85;

            Rect position = EditorGUILayout.BeginVertical(GUILayout.Width(210));
            GUILayout.Space(6);

            this.scrollingInfo = EditorGUILayout.BeginScrollView(this.scrollingInfo, RotorzEditorStyles.Instance.PaddedScrollView);

            if (this.autotileTileset != null) {
                GUILayout.Label(TileLang.Text("Autotile Atlas"), RotorzEditorStyles.Instance.BoldLabel);
            }
            else {
                GUILayout.Label(TileLang.Text("Atlas"), RotorzEditorStyles.Instance.BoldLabel);
            }

            ++EditorGUI.indentLevel;

            if (this.autotileTileset != null) {
                EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Layout"), this.autotileTileset.AutotileLayout.ToString());
                EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Inner Joins"), TileLang.FormatYesNoStatus(this.autotileTileset.HasInnerJoins));
                GUILayout.Space(6);
            }

            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Type"), this.tileset.procedural ? TileLang.Text("Procedural") : TileLang.Text("Non-Procedural"));

            if (atlasTexture != null) {
                EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Width"), TileLang.FormatPixelMetric(atlasTexture.width));
                EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Height"), TileLang.FormatPixelMetric(atlasTexture.height));
                GUILayout.Space(6);
            }

            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Rows"), this.tileset.Rows.ToString());
            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Columns"), this.tileset.Columns.ToString());
            --EditorGUI.indentLevel;

            GUILayout.Space(6);

            GUILayout.Label(TileLang.ParticularText("Property", "Tile Size"), RotorzEditorStyles.Instance.BoldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Width"), TileLang.FormatPixelMetric(this.tileset.TileWidth));
            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Height"), TileLang.FormatPixelMetric(this.tileset.TileHeight));
            --EditorGUI.indentLevel;

            GUILayout.Space(6);

            GUILayout.Label(TileLang.Text("Edge Correction"), RotorzEditorStyles.Instance.BoldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Border"), TileLang.FormatPixelMetric(this.tileset.BorderSize));
            EditorGUILayout.LabelField(TileLang.ParticularText("Property", "Delta"), TileLang.FormatPixelFractionMetric(this.tileset.Delta));
            --EditorGUI.indentLevel;

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint) {
                RotorzEditorStyles.Instance.HorizontalSplitter.Draw(
                    new Rect(position.x, position.y, position.width - 6, position.height),
                    GUIContent.none,
                    false, false, false, false
                );
            }

            EditorGUIUtility.labelWidth = restoreLabelWidth;
        }

        #endregion
    }
}
