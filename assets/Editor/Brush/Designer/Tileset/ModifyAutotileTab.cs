// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor.Internal
{
    internal class ModifyAutotileTab : ModifyTilesetTab
    {
        public AutotileTileset autotileTileset;

        public ModifyAutotileTab(TilesetDesigner designer) : base(designer)
        {
        }


        public override string Label {
            get { return TileLang.ParticularText("TilesetDesigner|TabLabel", "Modify Autotile"); }
        }


        #region Autotile Tileset Details

        /// <summary>
        /// Set to <c>true</c> to refresh atlas details.
        /// </summary>
        private bool invalidateAtlasDetails;
        /// <summary>
        /// Atlas details to display in user interface.
        /// </summary>
        /// <remarks>
        /// <para>A value of <c>null</c> when no details are to be shown.</para>
        /// </remarks>
        private string atlasDetails;


        /// <summary>
        /// Refreshes atlas details if necessary.
        /// </summary>
        /// <remarks>
        /// <para>These details are shown on the user interface to provide the user with an
        /// indication of atlas usage.</para>
        /// </remarks>
        private void RefreshAtlasDetails()
        {
            if (!this.invalidateAtlasDetails) {
                return;
            }

            this.invalidateAtlasDetails = false;

            // No details to show when no autotile artwork is specified
            if (this.inputNewAutotileArtwork /*Uncompressed*/ == null) {
                this.atlasDetails = null;
                return;
            }

            var autotileExpanderUtility = new AutotileExpanderUtility(
                layout: this.autotileTileset.AutotileLayout,
                autotileArtwork: this.inputNewAutotileArtworkUncompressed,
                tileWidth: this.inputTileWidth,
                tileHeight: this.inputTileHeight,
                innerJoins: this.autotileTileset.HasInnerJoins,
                border: this.inputBorderSize,
                clampEdges: false
            );

            int atlasWidth;
            int atlasHeight;
            int atlasUnused;

            autotileExpanderUtility.CalculateMetrics(out atlasWidth, out atlasHeight, out atlasUnused);

            int atlasUnusedPercentage = 100 * atlasUnused / (atlasWidth * atlasHeight);

            this.atlasDetails = string.Format(
                /* 0: width of atlas texture in pixels
                   1: height of atlas texture in pixels
                   2: percentage of unused area of atlas texture */
                TileLang.Text("Output Atlas: {0}x{1}   Unused: {2}%"),
                atlasWidth, atlasHeight, atlasUnusedPercentage
            );
        }

        #endregion


        #region Autotile Tileset

        private Texture2D inputNewAutotileArtwork;
        private Texture2D inputNewAutotileArtworkUncompressed;
        private Texture2D expandedAutotileAtlas;


        /// <summary>
        /// Load uncompressed version of autotile artwork.
        /// </summary>
        /// <remarks>
        /// <para>Specified autotile artwork asset may not necessarily be of the correct
        /// size due to texture importer settings. This function retrieves the uncompressed
        /// form and then generates the expanded autotile atlas for preview.</para>
        /// </remarks>
        /// <param name="artwork">Autotile artwork.</param>
        private void LoadUncompressedAutotileArtwork(Texture2D artwork)
        {
            this.ClearUncompressedAutotileArtwork();

            this.inputNewAutotileArtwork = artwork;
            this.inputNewAutotileArtworkUncompressed = EditorInternalUtility.LoadTextureUncompressed(artwork);

            this.ClearExpandedAutotileAtlas();

            this.invalidateAtlasDetails = true;
        }

        /// <summary>
        /// Clear previously uncompressed autotile artwork.
        /// </summary>
        private void ClearUncompressedAutotileArtwork()
        {
            Object.DestroyImmediate(this.inputNewAutotileArtworkUncompressed);
            this.inputNewAutotileArtworkUncompressed = null;
        }

        /// <summary>
        /// Clear previously expanded autotile atlas.
        /// </summary>
        private void ClearExpandedAutotileAtlas()
        {
            Object.DestroyImmediate(this.expandedAutotileAtlas);
            this.expandedAutotileAtlas = null;
        }

        /// <summary>
        /// Apply constraints to tile width, height and border inputs.
        /// </summary>
        private void ApplyAutotileConstraintsToInputs()
        {
            AutotileExpanderUtility.ApplyTileSizeConstraints(
                layout: this.autotileTileset.AutotileLayout,
                artwork: this.inputNewAutotileArtworkUncompressed,
                innerJoins: this.autotileTileset.HasInnerJoins,
                tileWidth: ref this.inputTileWidth,
                tileHeight: ref this.inputTileHeight,
                border: ref this.inputBorderSize
            );
        }

        /// <summary>
        /// Expand autotile atlas.
        /// </summary>
        /// <param name="borderSize">Border size in pixels.</param>
        private void ExpandAutotileArtwork(int borderSize = 0)
        {
            this.ClearExpandedAutotileAtlas();

            this.ApplyAutotileConstraintsToInputs();

            if (this.inputNewAutotileArtworkUncompressed != null) {
                this.expandedAutotileAtlas = AutotileExpanderUtility.ExpandAutotileArtwork(this.autotileTileset.AutotileLayout, this.inputNewAutotileArtworkUncompressed, this.inputTileWidth, this.inputTileHeight, this.autotileTileset.HasInnerJoins, borderSize, this.inputClampEdges);
            }

            int atlasWidth = 0;
            int atlasHeight = 0;

            if (this.expandedAutotileAtlas != null) {
                atlasWidth = this.expandedAutotileAtlas.width;
                atlasHeight = this.expandedAutotileAtlas.height;
            }

            this.inputTilesetMetrics.Calculate(atlasWidth, atlasHeight, this.inputTileWidth, this.inputTileHeight, borderSize, this.inputDelta);
        }

        #endregion


        #region New Atlas Properties

        public override void GatherInputsFromCurrent()
        {
            base.GatherInputsFromCurrent();

            this.autotileTileset = this.tilesetRecord.Tileset as AutotileTileset;

            this.ClearExpandedAutotileAtlas();

            if (this.inputNewAutotileArtwork != this.autotileTileset.rawTexture) {
                this.ClearUncompressedAutotileArtwork();
                this.inputNewAutotileArtwork = this.autotileTileset.rawTexture;
            }

            this.inputClampEdges = this.autotileTileset.ForceClampEdges;

            this.invalidateAtlasDetails = true;
        }

        public override bool HasModifiedInputs {
            get {
                return base.HasModifiedInputs
                    || this.inputClampEdges != this.autotileTileset.ForceClampEdges
                    // Make it easy to generate texture atlas if it is missing.
                    || this.autotileTileset.AtlasTexture == null
                    ;
            }
        }

        #endregion


        #region GUI

        public override void OnDisable()
        {
            base.OnDisable();

            this.ClearUncompressedAutotileArtwork();
            this.ClearExpandedAutotileAtlas();
        }

        protected override void OnModifyTilesetGUI()
        {
            AutotileLayout layout = this.autotileTileset.AutotileLayout;

            GUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            this.DrawAtlasTextureField();
            GUILayout.Space(15);
            this.DrawAtlasParametersGUI();
            GUILayout.Space(20);
            this.DrawEdgeCorrectionGUI();
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) {
                this.invalidateAtlasDetails = true;
            }

            this.ApplyAutotileConstraintsToInputs();

            // Automatically refresh atlas details as needed.
            this.RefreshAtlasDetails();

            if (this.atlasDetails != null) {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Space(150);
                GUILayout.Label(this.atlasDetails, EditorStyles.miniLabel);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawAtlasTextureField()
        {
            EditorGUI.BeginChangeCheck();
            this.inputNewAutotileArtwork = RotorzEditorGUI.AutotileArtworkField(this.inputNewAutotileArtwork, this.autotileTileset.AutotileLayout, this.autotileTileset.HasInnerJoins);
            if (EditorGUI.EndChangeCheck()) {
                this.LoadUncompressedAutotileArtwork(this.inputNewAutotileArtwork);

                // Recalculate tile size and expand autotile artwork.
                if (this.inputNewAutotileArtworkUncompressed != null) {
                    AutotileExpanderUtility.EstimateTileSize(this.autotileTileset.AutotileLayout, this.inputNewAutotileArtworkUncompressed, this.autotileTileset.HasInnerJoins, ref this.inputTileWidth, ref this.inputTileHeight);
                    this.ExpandAutotileArtwork();
                }
            }
        }

        private void DrawAtlasParametersGUI()
        {
            EditorGUIUtility.labelWidth = 1;
            EditorGUIUtility.fieldWidth = 105;

            GUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();
            {
                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Width (px)"));
                this.inputTileWidth = EditorGUILayout.IntField(this.inputTileWidth, GUILayout.Width(60));
                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Height (px)"));
                this.inputTileHeight = EditorGUILayout.IntField(this.inputTileHeight, GUILayout.Width(60));
            }
            if (EditorGUI.EndChangeCheck()) {
                this.ClearExpandedAutotileAtlas();

                // Automatically load uncompressed version of autotile artwork if it has
                // not already been loaded.
                if (this.inputNewAutotileArtworkUncompressed == null) {
                    this.LoadUncompressedAutotileArtwork(this.inputNewAutotileArtwork);
                }
            }

            GUILayout.Space(10);

            //this.alpha = GUILayout.Toggle(this.alpha, "Alpha Blending");

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Procedural"),
                TileLang.Text("Autotile brushes are always procedural, however atlas brushes of an autotile atlas can be non-procedural if desired.")
            )) {
                this.inputProcedural = GUILayout.Toggle(this.inputProcedural, content);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Clamp Edges"),
                TileLang.Text("Indicates if outer edges should always be clamped. Often not appropriate when secondary tile acts as ground.")
            )) {
                if (this.autotileTileset.AutotileLayout == AutotileLayout.Extended && this.inputBorderSize > 0) {
                    this.inputClampEdges = GUILayout.Toggle(this.inputClampEdges, content);
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawEdgeCorrectionGUI()
        {
            EditorGUIUtility.labelWidth = 1;
            EditorGUIUtility.fieldWidth = 125;

            GUILayout.BeginVertical();

            ExtraEditorGUI.AbovePrefixLabel(TileLang.Text("Edge Correction"));
            this.inputEdgeCorrectionPreset = (EdgeCorrectionPreset)EditorGUILayout.EnumPopup(this.inputEdgeCorrectionPreset);

            if (this.inputEdgeCorrectionPreset == EdgeCorrectionPreset.Custom) {
                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Border Size (px)"));
                this.inputBorderSize = EditorGUILayout.IntField(this.inputBorderSize, GUILayout.Width(60));

                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Delta (% of 1px)"));
                GUILayout.BeginHorizontal();
                {
                    this.inputDelta = Mathf.Clamp(EditorGUILayout.FloatField(this.inputDelta, GUILayout.Width(60)), 0f, 1f);

                    float newDelta = GUILayout.HorizontalSlider(this.inputDelta, 0f, +1f, GUILayout.Width(80));
                    if (newDelta != this.inputDelta) {
                        this.inputDelta = (float)((int)(newDelta * 100f)) / 100f;
                    }
                }
                GUILayout.EndHorizontal();
            }
            else {
                this.inputBorderSize = 0;
                this.inputDelta = (this.inputEdgeCorrectionPreset == EdgeCorrectionPreset.InsetUVs) ? 0.5f : 0f;

                EditorGUI.BeginDisabledGroup(true);

                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Border Size (px)"));
                EditorGUILayout.IntField(this.inputBorderSize, GUILayout.Width(60));

                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Delta (% of 1px)"));
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.FloatField(this.inputDelta, GUILayout.Width(60));
                    GUILayout.HorizontalSlider(this.inputDelta, 0f, +1f, GUILayout.Width(80));
                }
                GUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();
            }

            GUILayout.EndVertical();

        }

        protected override void DrawTilePreviews()
        {
            if (this.tilesetPreviews == null) {
                this.tilesetPreviews = new TilesetPreviewUtility(this.designer.Window);
            }

            // It is necessary to update tile previews when atlas or tile size changes.
            if (this.inputAtlasTexture != this.tileset.AtlasTexture || this.inputTileWidth != this.tileset.TileWidth || this.inputTileHeight != this.tileset.TileHeight) {
                // Have previews been generated?
                if (this.expandedAutotileAtlas != null) {
                    this.tilesetPreviews.DrawTilePreviews(this.autotileTileset.AutotileLayout, this.expandedAutotileAtlas, this.autotileTileset.HasInnerJoins, this.inputTilesetMetrics);
                }
            }
            else {
                // Draw previews from saved tileset.
                this.tilesetPreviews.DrawTilePreviews(this.autotileTileset.AutotileLayout, this.autotileTileset.AtlasTexture, this.autotileTileset.HasInnerJoins, this.autotileTileset as ITilesetMetrics);
            }
        }

        internal override void DrawButtonStrip()
        {
            GUILayout.Space(50);

            GUILayout.BeginVertical();

            if (this.HasModifiedInputs) {
                if (GUILayout.Button(TileLang.ParticularText("Action", "Apply Changes"), ExtraEditorStyles.Instance.BigButton)) {
                    this.OnApplyChanges();
                    GUIUtility.ExitGUI();
                }

                GUILayout.Space(5);

                if (GUILayout.Button(TileLang.ParticularText("Action", "Revert"), ExtraEditorStyles.Instance.BigButton)) {
                    this.OnRevert();
                    GUIUtility.ExitGUI();
                }
            }
            else {
                if (GUILayout.Button(TileLang.ParticularText("Action", "Regenerate"), ExtraEditorStyles.Instance.BigButton)) {
                    this.OnApplyChanges();
                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.EndVertical();
        }

        #endregion


        #region Actions

        protected override void OnApplyChanges()
        {
            // Do not proceed if no atlas texture was selected.
            if (this.inputNewAutotileArtwork == null) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "Autotile artwork was not specified"),
                    TileLang.Text("Please select artwork for autotile before proceeding."),
                    TileLang.ParticularText("Action", "Close")
                );
                return;
            }

            // Warn user if modified atlas will contain impossible brushes.
            if (BrushUtility.WouldHaveImpossibleTilesetBrushes(this.tileset, this.inputTileWidth, this.inputTileHeight, this.inputBorderSize)) {
                if (!EditorUtility.DisplayDialog(
                    TileLang.Text("Warning, brushes will be deleted"),
                    TileLang.Text("Modified atlas contains fewer tiles than previously. Previously created brushes that are out of range will be deleted.\n\nWould you like to proceed?"),
                    TileLang.ParticularText("Action", "Yes"),
                    TileLang.ParticularText("Action", "No")
                )) {
                    return;
                }
            }

            bool refreshProceduralMeshes = !this.inputProcedural && this.autotileTileset.procedural;

            // If raw uncompressed variation of autotile is not defined generate from
            // current selection.
            if (this.inputNewAutotileArtworkUncompressed == null) {
                if (this.inputNewAutotileArtwork == null) {
                    Debug.LogError(TileLang.ParticularText("Error", "Invalid autotile artwork was specified."));
                    return;
                }
                this.inputNewAutotileArtworkUncompressed = EditorInternalUtility.LoadTextureUncompressed(this.inputNewAutotileArtwork);
            }

            this.ExpandAutotileArtwork(this.inputBorderSize);

            string tilesetBasePath = this.tilesetRecord.AssetPath.Substring(0, this.tilesetRecord.AssetPath.LastIndexOf('/') + 1);

            // Save texture asset.
            string assetPath = AssetDatabase.GetAssetPath(this.autotileTileset.AtlasTexture);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(tilesetBasePath)) {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(tilesetBasePath + "atlas.png");
            }

            this.autotileTileset.AtlasTexture = EditorInternalUtility.SavePngAsset(assetPath, this.expandedAutotileAtlas);

            // Update and save material asset.
            if (this.autotileTileset.AtlasMaterial == null) {
                this.autotileTileset.AtlasMaterial = new Material(Shader.Find("Rotorz/Tileset/Opaque Unlit"));
                this.autotileTileset.AtlasMaterial.mainTexture = this.autotileTileset.AtlasTexture;

                assetPath = AssetDatabase.GenerateUniqueAssetPath(tilesetBasePath + "atlas.mat");
                AssetDatabase.CreateAsset(this.autotileTileset.AtlasMaterial, assetPath);
                AssetDatabase.ImportAsset(assetPath);
            }
            else {
                this.autotileTileset.AtlasMaterial.mainTexture = this.autotileTileset.AtlasTexture;
                EditorUtility.SetDirty(this.autotileTileset.AtlasMaterial);
            }

            // Calculate metrics for tileset.
            var metrics = new TilesetMetrics(this.autotileTileset.AtlasTexture, this.inputTileWidth, this.inputTileHeight, this.inputBorderSize, this.inputDelta);

            // Update properties of tileset.
            this.autotileTileset.procedural = this.inputProcedural;
            this.autotileTileset.ForceClampEdges = this.inputClampEdges;
            this.autotileTileset.rawTexture = this.inputNewAutotileArtwork;
            this.autotileTileset.SetMetricsFrom(metrics);

            this.ClearExpandedAutotileAtlas();

            EditorUtility.SetDirty(this.autotileTileset);

            // Delete "impossible" tile brushes in tileset.
            // For example, an extra brush for a tile that no longer exists.
            BrushUtility.DeleteImpossibleTilesetBrushes(this.tileset);

            // Ensure that non-procedural meshes are pre-generated if missing.
            if (refreshProceduralMeshes) {
                BrushUtility.RefreshNonProceduralMeshes(this.tileset);
            }

            ToolUtility.RepaintBrushPalette();

            // Update procedural meshes for tile systems in scene if necessary.
            // Note: Only update if procedural mode of tileset was not modified.
            if (this.inputProcedural && this.tileset.procedural) {
                foreach (TileSystem tileSystem in Object.FindObjectsOfType(typeof(TileSystem))) {
                    tileSystem.UpdateProceduralTiles(true);
                }
            }
        }

        protected override void OnRefreshPreviews()
        {
            this.ExpandAutotileArtwork();
            this.designer.Window.Repaint();
        }

        #endregion
    }
}
