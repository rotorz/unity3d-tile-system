// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor.Internal
{
    internal class ModifyTilesetTab : ITilesetDesignerTab
    {
        protected TilesetDesigner designer;

        public TilesetAssetRecord tilesetRecord;
        public Tileset tileset;


        public ModifyTilesetTab(TilesetDesigner designer)
        {
            this.designer = designer;
        }


        public virtual string Label {
            get { return TileLang.ParticularText("TilesetDesigner|TabLabel", "Modify Tileset"); }
        }

        public Vector2 ScrollPosition { get; set; }


        #region New Tileset Properties

        protected Texture2D inputAtlasTexture;
        protected int inputTileWidth;
        protected int inputTileHeight;

        protected EdgeCorrectionPreset inputEdgeCorrectionPreset;
        protected int inputBorderSize;
        protected float inputDelta;

        protected bool inputProcedural;
        protected bool inputClampEdges;


        public virtual void GatherInputsFromCurrent()
        {
            this.tileset = this.tilesetRecord.Tileset;

            this.inputAtlasTexture = this.tileset.AtlasTexture;
            this.inputTileWidth = this.tileset.TileWidth;
            this.inputTileHeight = this.tileset.TileHeight;

            this.inputBorderSize = this.tileset.BorderSize;
            this.inputDelta = this.tileset.Delta;

            if (this.inputBorderSize == 0 && this.inputDelta == 0f) {
                this.inputEdgeCorrectionPreset = EdgeCorrectionPreset.DoNothing;
            }
            else if (this.inputBorderSize == 0 && this.inputDelta == 0.5f) {
                this.inputEdgeCorrectionPreset = EdgeCorrectionPreset.InsetUVs;
            }
            else {
                this.inputEdgeCorrectionPreset = EdgeCorrectionPreset.Custom;
            }

            this.inputProcedural = this.tileset.procedural;

            RotorzEditorGUI.ClearControlFocus();
        }


        public virtual bool HasModifiedInputs {
            get {
                return
                    this.inputAtlasTexture != this.tileset.AtlasTexture ||
                    this.inputTileWidth != this.tileset.TileWidth ||
                    this.inputTileHeight != this.tileset.TileHeight ||
                    this.inputBorderSize != this.tileset.BorderSize ||
                    this.inputDelta != this.tileset.Delta ||
                    this.inputProcedural != this.tileset.procedural
                    ;
            }
        }

        #endregion


        #region GUI

        public void OnNewTilesetRecord(TilesetAssetRecord record)
        {
            this.tilesetRecord = record;

            this.GatherInputsFromCurrent();
        }

        public void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public void OnFixedHeaderGUI()
        {
        }

        protected TilesetMetrics inputTilesetMetrics = new TilesetMetrics();

        private void RecalculateMetrics()
        {
            int atlasWidth;
            int atlasHeight;

            EditorInternalUtility.GetImageSize(this.inputAtlasTexture, out atlasWidth, out atlasHeight);

            this.inputTileWidth = Mathf.Clamp(this.inputTileWidth, 1, atlasWidth);
            this.inputTileHeight = Mathf.Clamp(this.inputTileHeight, 1, atlasHeight);

            this.inputTilesetMetrics.Calculate(this.inputAtlasTexture, this.inputTileWidth, this.inputTileHeight, this.inputBorderSize, this.inputDelta);
        }

        public void OnGUI()
        {
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            this.OnModifyTilesetGUI();
            this.DrawButtonStrip();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (this.inputProcedural != this.tileset.procedural) {
                EditorGUILayout.HelpBox(TileLang.Text("Tiles painted using this tileset will need to be refreshed when switching between procedural and non-procedural."), MessageType.Warning, true);
            }

            ExtraEditorGUI.SeparatorLight(marginTop: 12, marginBottom: 2, thickness: 3);

            this.DrawTilePreviewSection();
        }

        protected virtual void OnModifyTilesetGUI()
        {
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
                this.RecalculateMetrics();
            }
        }

        private void DrawAtlasTextureField()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(-1);

            this.inputAtlasTexture = EditorGUILayout.ObjectField(this.inputAtlasTexture, typeof(Texture), false, GUILayout.Width(130), GUILayout.Height(130)) as Texture2D;

            GUILayout.EndVertical();
        }

        private void DrawAtlasParametersGUI()
        {
            EditorGUIUtility.labelWidth = 1;
            EditorGUIUtility.fieldWidth = 105;

            GUILayout.BeginVertical();

            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Width (px)"));
            this.inputTileWidth = EditorGUILayout.IntField(this.inputTileWidth, GUILayout.Width(60));
            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Height (px)"));
            this.inputTileHeight = EditorGUILayout.IntField(this.inputTileHeight, GUILayout.Width(60));

            GUILayout.Space(10);

            //_alpha = GUILayout.Toggle(_alpha, "Alpha Blending");

            this.inputProcedural = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Procedural"), this.inputProcedural);

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

        internal virtual void DrawButtonStrip()
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

        protected TilesetPreviewUtility tilesetPreviews;

        private void DrawTilePreviewSection()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            RotorzEditorGUI.Title(TileLang.Text("Tile Previews"));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Refresh Previews"), RotorzEditorStyles.Instance.ButtonWide)) {
                this.OnRefreshPreviews();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            ExtraEditorGUI.SeparatorLight();
            GUILayout.Space(-5);

            GUILayout.BeginVertical(RotorzEditorStyles.Instance.PaddedScrollView);
            this.DrawTilePreviews();
            GUILayout.EndVertical();

            GUILayout.Space(-10);
        }

        protected virtual void DrawTilePreviews()
        {
            if (this.tilesetPreviews == null) {
                this.tilesetPreviews = new TilesetPreviewUtility(this.designer.Window);
            }

            ITilesetMetrics metrics = this.HasModifiedInputs
                ? this.inputTilesetMetrics
                : this.tileset as ITilesetMetrics;

            this.tilesetPreviews.DrawTilePreviews(this.inputAtlasTexture, metrics);
        }

        public void OnSideGUI()
        {
        }

        #endregion


        #region Actions

        internal void OnRevert()
        {
            this.GatherInputsFromCurrent();
        }

        protected virtual void OnApplyChanges()
        {
            // Do not proceed if no atlas texture was selected.
            if (this.inputAtlasTexture == null) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "Atlas texture was not specified"),
                    TileLang.Text("Please select an atlas texture before proceeding."),
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

            bool refreshNonProceduralMeshes = !this.inputProcedural && this.tileset.procedural;

            string tilesetBasePath = this.tilesetRecord.AssetPath.Substring(0, this.tilesetRecord.AssetPath.LastIndexOf('/') + 1);

            // Update and save material asset.
            if (this.tileset.AtlasMaterial == null) {
                this.tileset.AtlasMaterial = new Material(Shader.Find("Rotorz/Tileset/Opaque Unlit"));
                this.tileset.AtlasMaterial.mainTexture = this.inputAtlasTexture;

                string assetPath = AssetDatabase.GenerateUniqueAssetPath(tilesetBasePath + "atlas.mat");
                AssetDatabase.CreateAsset(this.tileset.AtlasMaterial, assetPath);
                AssetDatabase.ImportAsset(assetPath);
            }
            else {
                this.tileset.AtlasMaterial.mainTexture = this.inputAtlasTexture;
                EditorUtility.SetDirty(this.tileset.AtlasMaterial);
            }

            this.tileset.AtlasTexture = this.inputAtlasTexture;

            // Calculate metrics for tileset.
            this.RecalculateMetrics();

            // Update properties of tileset.
            this.tileset.procedural = this.inputProcedural;
            this.tileset.SetMetricsFrom(this.inputTilesetMetrics);

            EditorUtility.SetDirty(this.tileset);

            // Delete "impossible" tile brushes in tileset.
            // For example, an extra brush for a tile that no longer exists.
            BrushUtility.DeleteImpossibleTilesetBrushes(this.tileset);

            // Ensure that non-procedural meshes are pre-generated if missing.
            if (refreshNonProceduralMeshes) {
                BrushUtility.RefreshNonProceduralMeshes(this.tileset);
            }

            ToolUtility.RepaintBrushPalette();

            // Update procedural meshes for tile systems in scene if necessary.
            // Note: Only update if procedural mode of tileset was not modified.
            if (this.inputProcedural && this.tileset.procedural) {
                foreach (TileSystem system in Object.FindObjectsOfType(typeof(TileSystem))) {
                    system.UpdateProceduralTiles(true);
                }
            }
        }

        protected virtual void OnRefreshPreviews()
        {
            this.designer.Window.Repaint();
        }

        #endregion
    }
}
