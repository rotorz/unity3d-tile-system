// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Autotile brush creator interface.
    /// </summary>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    public sealed class AutotileCreator : BrushCreator
    {
        private static readonly AutotileLayout[] s_TabValues = {
            AutotileLayout.Basic,
            AutotileLayout.Extended
        };
        private static readonly GUIContent[] s_TabLabels = (
            from value in s_TabValues
            select new GUIContent(value.ToString())
        ).ToArray();

        private static int s_SelectedTabIndex = 0;
        private static AutotileLayout s_SelectedAutotileLayout = AutotileLayout.Basic;
        private static bool s_InnerJoins = true;

        private static int s_BorderSize = 1;
        private static float s_Delta;
        private static bool s_EnableClampEdges = false;


        private Texture2D autotileTexture;
        private Texture2D autotileTextureUncompressed;

        private Texture2D atlasTexture;
        private int tileWidth = 32;
        private int tileHeight = 32;
        private bool enableAlphaBlending = true;

        private EdgeCorrectionPreset edgeCorrectionPreset = EdgeCorrectionPreset.Custom;

        private int metricAtlasWidth;
        private int metricAtlasHeight;
        private int metricAtlasUnused;
        private int metricAtlasUnusedPercentage;
        private GUIContent metricSummary = new GUIContent();


        private TilesetMetrics metrics = new TilesetMetrics();
        private TilesetPreviewUtility previews;
        private bool clearPreviews;


        /// <summary>
        /// Initializes a new instance of the <see cref="AutotileCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public AutotileCreator(IBrushCreatorContext context)
            : base(context)
        {
        }


        /// <inheritdoc/>
        public override string Name {
            get { return TileLang.ParticularText("BrushCreator|TabLabel", "Autotile"); }
        }

        /// <inheritdoc/>
        public override string Title {
            get { return TileLang.Text("Create new autotile tileset"); }
        }


        /// <inheritdoc/>
        public override void OnDisable()
        {
            base.OnDisable();

            Object.DestroyImmediate(this.autotileTextureUncompressed);
            Object.DestroyImmediate(this.atlasTexture);
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            GUILayout.Space(-5f);

            this.DrawLayoutSelectionGUI();
            GUILayout.Space(3f);

            this.DrawTilesetNameField();
            this.DrawInnerJoinsField();

            ExtraEditorGUI.SeparatorLight(marginBottom: 7);

            this.DrawAtlasDetailsGUI();

            if (this.autotileTextureUncompressed == null) {
                ExtraEditorGUI.SeparatorLight(marginTop: 12, marginBottom: 5, thickness: 3);
                GUILayout.Label(TileLang.Text("No previews to display..."));
                return;
            }

            Rect r = EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(241f);

                if (GUILayout.Button(TileLang.ParticularText("Action", "Refresh Previews"), RotorzEditorStyles.ContractWidth)) {
                    this.OnButtonRefreshAtlas();
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUI.Label(new Rect(r.x, r.y + 4f, r.width, r.height), this.metricSummary, EditorStyles.miniLabel);

            ExtraEditorGUI.SeparatorLight(marginBottom: 5, thickness: 3);

            if (this.previews == null) {
                this.previews = new TilesetPreviewUtility(this.Context);
            }

            if (!this.previews.DrawTilePreviews(s_SelectedAutotileLayout, this.atlasTexture, s_InnerJoins, this.metrics)) {
                GUILayout.Label(TileLang.Text("No previews to display..."));
            }
        }

        /// <inheritdoc/>
        public override void OnButtonCreate()
        {
            string tilesetName = this.Context.GetSharedProperty(BrushCreatorSharedPropertyKeys.TilesetName, "");

            if (!this.ValidateInputs(tilesetName)) {
                return;
            }

            this.CreateAutotileTileset(tilesetName);

            this.Context.Close();
        }


        private void DrawInnerJoinsField()
        {
            EditorGUI.BeginChangeCheck();
            s_InnerJoins = GUILayout.Toggle(s_InnerJoins, TileLang.ParticularText("Property", "Artwork includes inner joins"), EditorStyles.toggle);
            if (EditorGUI.EndChangeCheck()) {
                this.RecalculateTileSize();
                this.RecalculateMetrics();
            }
        }

        private void DrawLayoutSelectionGUI()
        {
            Rect r = EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(-11f);

                EditorGUI.BeginChangeCheck();
                {
                    s_SelectedTabIndex = RotorzEditorGUI.TabSelector(s_SelectedTabIndex, s_TabLabels);
                    s_SelectedAutotileLayout = s_TabValues[s_SelectedTabIndex];
                }
                if (EditorGUI.EndChangeCheck()) {
                    this.RecalculateTileSize();
                }

                GUILayout.Space(-10f);
            }
            EditorGUILayout.EndHorizontal();

            // Draw selected icon at top-right of window.
            GUI.DrawTexture(new Rect(r.x + r.width - 45f, r.y - 32f, 40f, 53f), s_SelectedAutotileLayout == AutotileLayout.Extended ? RotorzEditorStyles.Skin.AutotileExtendedIcon : RotorzEditorStyles.Skin.AutotileBasicIcon);
        }

        private void DrawAtlasDetailsGUI()
        {
            bool invalidateMetrics = false;
            this.clearPreviews = false;

            GUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                {
                    this.DrawAtlasTextureField();
                    GUILayout.Space(15f);
                    this.DrawAtlasParametersGUI();
                    GUILayout.Space(20f);
                    this.DrawEdgeCorrectionGUI();
                    GUILayout.FlexibleSpace();
                }
                invalidateMetrics = EditorGUI.EndChangeCheck();
            }
            GUILayout.EndHorizontal();

            // Metrics must be recalculated if any changes have been made.
            if (this.clearPreviews || invalidateMetrics) {
                AutotileExpanderUtility.ApplyTileSizeConstraints(s_SelectedAutotileLayout, this.autotileTextureUncompressed, s_InnerJoins, ref this.tileWidth, ref this.tileHeight, ref s_BorderSize);
                this.RecalculateMetrics();

                // Previews only need to be refreshed when tile size or atlas is changed.
                if (this.clearPreviews) {
                    this.ClearExpandedAutotileArtwork();
                }
            }
        }

        private void DrawAtlasTextureField()
        {
            EditorGUI.BeginChangeCheck();
            {
                this.autotileTexture = RotorzEditorGUI.AutotileArtworkField(this.autotileTexture, s_SelectedAutotileLayout, s_InnerJoins);
            }
            if (EditorGUI.EndChangeCheck()) {
                if (this.autotileTextureUncompressed != null) {
                    Object.DestroyImmediate(this.autotileTextureUncompressed);
                }

                this.autotileTextureUncompressed = EditorInternalUtility.LoadTextureUncompressed(this.autotileTexture);

                this.RecalculateTileSize();
                this.RecalculateMetrics();
            }
        }

        private void DrawAtlasParametersGUI()
        {
            EditorGUIUtility.fieldWidth = 105f;

            GUILayout.BeginVertical();
            {
                EditorGUI.BeginChangeCheck();
                {
                    ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Width (px)"));
                    this.tileWidth = EditorGUILayout.IntField(this.tileWidth, GUILayout.Width(60f));
                    ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Height (px)"));
                    this.tileHeight = EditorGUILayout.IntField(this.tileHeight, GUILayout.Width(60f));
                }
                if (EditorGUI.EndChangeCheck()) {
                    this.clearPreviews = true;
                }

                GUILayout.Space(10f);

                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Alpha Blending")
                )) {
                    this.enableAlphaBlending = EditorGUILayout.ToggleLeft(content, this.enableAlphaBlending);
                }

                if (s_SelectedAutotileLayout == AutotileLayout.Extended && s_BorderSize > 0) {
                    using (var content = ControlContent.Basic(
                        TileLang.ParticularText("Property", "Clamp Edges"),
                        TileLang.Text("Indicates if outer edges should always be clamped. Often not appropriate when secondary tile acts as ground.")
                    )) {
                        s_EnableClampEdges = EditorGUILayout.ToggleLeft(content, s_EnableClampEdges);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawEdgeCorrectionGUI()
        {
            EditorGUIUtility.fieldWidth = 125f;

            GUILayout.BeginVertical();
            {
                ExtraEditorGUI.AbovePrefixLabel(TileLang.Text("Edge Correction"));
                this.edgeCorrectionPreset = (EdgeCorrectionPreset)EditorGUILayout.EnumPopup(this.edgeCorrectionPreset);
                if (this.edgeCorrectionPreset == EdgeCorrectionPreset.Custom) {
                    ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Border Size (px)"));
                    s_BorderSize = EditorGUILayout.IntField(s_BorderSize, GUILayout.Width(60f));

                    ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Delta (% of 1px)"));
                    GUILayout.BeginHorizontal();
                    {
                        s_Delta = Mathf.Clamp(EditorGUILayout.FloatField(s_Delta, GUILayout.Width(60f)), 0f, 1f);

                        float newDelta = GUILayout.HorizontalSlider(s_Delta, 0f, +1f, GUILayout.Width(80f));
                        if (newDelta != s_Delta) {
                            s_Delta = (float)((int)(newDelta * 100f)) / 100f;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else {
                    s_BorderSize = 0;
                    s_Delta = (this.edgeCorrectionPreset == EdgeCorrectionPreset.InsetUVs) ? 0.5f : 0f;

                    EditorGUI.BeginDisabledGroup(true);
                    {
                        ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Border Size (px)"));
                        EditorGUILayout.IntField(s_BorderSize, GUILayout.Width(60f));

                        ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Delta (% of 1px)"));
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.FloatField(s_Delta, GUILayout.Width(60f));
                            GUILayout.HorizontalSlider(s_Delta, 0f, +1f, GUILayout.Width(80f));
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            GUILayout.EndVertical();
        }


        private void ClearExpandedAutotileArtwork()
        {
            if (this.atlasTexture != null) {
                Object.DestroyImmediate(this.atlasTexture);
            }
        }

        private void ExpandAutotileArtwork(int borderSize = 0)
        {
            this.ClearExpandedAutotileArtwork();

            AutotileExpanderUtility.ApplyTileSizeConstraints(s_SelectedAutotileLayout, this.autotileTextureUncompressed, s_InnerJoins, ref this.tileWidth, ref this.tileHeight, ref s_BorderSize);

            if (this.autotileTextureUncompressed != null) {
                this.atlasTexture = AutotileExpanderUtility.ExpandAutotileArtwork(s_SelectedAutotileLayout, this.autotileTextureUncompressed, this.tileWidth, this.tileHeight, s_InnerJoins, borderSize, s_EnableClampEdges);
                this.atlasTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            int atlasWidth = 0, atlasHeight = 0;
            if (this.atlasTexture != null) {
                atlasWidth = this.atlasTexture.width;
                atlasHeight = this.atlasTexture.height;
            }

            this.metrics.Calculate(atlasWidth, atlasHeight, this.tileWidth, this.tileHeight, borderSize, s_Delta);
        }

        private void RecalculateTileSize()
        {
            if (this.autotileTextureUncompressed == null) {
                return;
            }

            AutotileExpanderUtility.EstimateTileSize(s_SelectedAutotileLayout, this.autotileTextureUncompressed, s_InnerJoins, ref this.tileWidth, ref this.tileHeight);
            this.ExpandAutotileArtwork();
        }

        private void RecalculateMetrics()
        {
            if (this.autotileTextureUncompressed != null) {
                var autotileExpanderUtility = new AutotileExpanderUtility(s_SelectedAutotileLayout, this.autotileTextureUncompressed, this.tileWidth, this.tileHeight, s_InnerJoins, s_BorderSize, false);
                autotileExpanderUtility.CalculateMetrics(out this.metricAtlasWidth, out this.metricAtlasHeight, out this.metricAtlasUnused);

                this.metricAtlasUnusedPercentage = 100 * this.metricAtlasUnused / (this.metricAtlasWidth * this.metricAtlasHeight);

                this.metricSummary.text = string.Format(
                    /* 0: width of atlas texture in pixels
                       1: height of atlas texture in pixels
                       2: percentage of unused area of atlas texture */
                    TileLang.Text("Output Atlas: {0}x{1}   Unused: {2}%"),
                    this.metricAtlasWidth, this.metricAtlasHeight, this.metricAtlasUnusedPercentage
                );
            }
            else {
                this.metricAtlasUnusedPercentage = this.metricAtlasUnused = this.metricAtlasHeight = this.metricAtlasWidth = 0;
                this.metricSummary.text = string.Empty;
            }
        }

        private void OnButtonRefreshAtlas()
        {
            this.ExpandAutotileArtwork();
            this.Context.Repaint();
        }


        private bool ValidateInputs(string tilesetName)
        {
            if (!this.ValidateAssetName(tilesetName)) {
                return false;
            }

            if (this.atlasTexture == null) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Autotile artwork was not specified"),
                    TileLang.Text("Autotile artwork must be specified that can be expanded into an atlas."),
                    TileLang.ParticularText("Action", "OK")
                );
                return false;
            }

            return true;
        }

        private void CreateAutotileTileset(string tilesetName)
        {
            // Ensure that autotile artwork is re-expanded before proceeding to avoid
            // ignoring changes that have been made to user input.
            this.ExpandAutotileArtwork(s_BorderSize);

            // Create folder for autotile brush assets.
            string tilesetDirectoryName = tilesetName + " Autotile";
            string tilesetDirectoryPath = AssetDatabase.GenerateUniqueAssetPath(BrushUtility.GetBrushAssetPath() + tilesetDirectoryName);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + tilesetDirectoryPath);

            // Create material for tileset.
            var atlasTexture = EditorInternalUtility.SavePngAsset(tilesetDirectoryPath + "/atlas.png", this.atlasTexture);
            var atlasMaterial = BrushUtility.CreateTilesetMaterial(atlasTexture, this.enableAlphaBlending);

            AssetDatabase.CreateAsset(atlasMaterial, tilesetDirectoryPath + "/atlas.mat");
            AssetDatabase.ImportAsset(tilesetDirectoryPath + "/atlas.mat");

            // Calculate metrics for tileset.
            var tilesetMetrics = new TilesetMetrics(atlasTexture, this.tileWidth, this.tileHeight, s_BorderSize, s_Delta);

            // Create tileset.
            var autotileTileset = ScriptableObject.CreateInstance<AutotileTileset>();
            autotileTileset.Initialize(s_SelectedAutotileLayout, s_InnerJoins, atlasMaterial, atlasTexture, tilesetMetrics);
            autotileTileset.rawTexture = this.autotileTexture;
            autotileTileset.procedural = true;
            autotileTileset.ForceClampEdges = s_EnableClampEdges;

            Object.DestroyImmediate(this.atlasTexture);
            this.atlasTexture = null;

            // Save tileset and its material to asset file.
            string assetPath = tilesetDirectoryPath + "/" + tilesetName + ".asset";
            AssetDatabase.CreateAsset(autotileTileset, assetPath);

            AssetDatabase.ImportAsset(assetPath);

            // Ensure that changes are persisted immediately.
            AssetDatabase.SaveAssets();

            // Make sure that "Create Brushes" tab is shown.
            TilesetDesigner.s_SelectedTab = 0;
            ToolUtility.ShowTilesetInDesigner(autotileTileset);

            ToolUtility.RepaintBrushPalette();
        }
    }
}
