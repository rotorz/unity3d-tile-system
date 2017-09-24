// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Tileset creator interface.
    /// </summary>
    /// <intro>
    /// <para>For information regarding the creation and usage of tilesets please refer to
    /// the <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Tilesets">Tilesets</a>
    /// section of the user guide.</para>
    /// </intro>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    public sealed class TilesetCreator : BrushCreator
    {
        private Texture2D tilesetTexture;

        private int tileWidth = 32;
        private int tileHeight = 32;
        private bool alpha = true;
        private bool procedural = true;

        private string otherTilesetName;

        private EdgeCorrectionPreset edgeCorrectionPreset = EdgeCorrectionPreset.InsetUVs;
        private int borderSize;
        private float delta;

        private TilesetMetrics tilesetMetrics = new TilesetMetrics();
        private TilesetPreviewUtility tilesetPreviewUtility;


        /// <summary>
        /// Initializes a new instance of the <see cref="TilesetCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public TilesetCreator(IBrushCreatorContext context)
            : base(context)
        {
        }


        /// <inheritdoc/>
        public override string Name {
            get { return TileLang.ParticularText("BrushCreator|TabLabel", "Tileset"); }
        }

        /// <inheritdoc/>
        public override string Title {
            get { return TileLang.Text("Create new tileset"); }
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            GUILayout.Label(
                TileLang.Text("Two-dimensional tile brushes can be created from a tileset."),
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(10f);

            this.DrawTilesetNameField();

            GUILayout.Space(5f);

            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginHorizontal();
                {
                    this.DrawAtlasTextureField();
                    GUILayout.Space(7f);
                    this.DrawAtlasParametersGUI();
                    GUILayout.Space(20f);
                    this.DrawEdgeCorrectionGUI();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck()) {
                this.RecalculateMetrics();
            }

            // Display warning message if another similar tileset already exists.
            if (!string.IsNullOrEmpty(this.otherTilesetName)) {
                EditorGUILayout.HelpBox(string.Format(
                    /* 0: name of the similar tileset */
                    TileLang.Text("Another similar tileset '{0}' exists that uses the same atlas texture."),
                    this.otherTilesetName
                ), MessageType.Warning, true);
            }

            // Display warning message if texture is not power-of-two.
            if (this.tilesetTexture != null && (!Mathf.IsPowerOfTwo(this.tilesetTexture.width) || !Mathf.IsPowerOfTwo(this.tilesetTexture.height) || this.tilesetTexture.width != this.tilesetTexture.height)) {
                EditorGUILayout.HelpBox(TileLang.Text("Texture is not power of two and/or is not square."), MessageType.Warning, true);
            }

            ExtraEditorGUI.SeparatorLight(marginTop: 12, marginBottom: 5, thickness: 3);

            if (this.tilesetPreviewUtility == null) {
                this.tilesetPreviewUtility = new TilesetPreviewUtility(this.Context);
            }

            if (!this.tilesetPreviewUtility.DrawTilePreviews(this.tilesetTexture, this.tilesetMetrics)) {
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

            this.CreateTileset(tilesetName);

            this.Context.Close();
        }


        private void RecalculateMetrics()
        {
            int atlasWidth, atlasHeight;
            EditorInternalUtility.GetImageSize(this.tilesetTexture, out atlasWidth, out atlasHeight);

            this.tileWidth = Mathf.Clamp(this.tileWidth, 1, atlasWidth);
            this.tileHeight = Mathf.Clamp(this.tileHeight, 1, atlasHeight);

            this.tilesetMetrics.Calculate(this.tilesetTexture, this.tileWidth, this.tileHeight, this.borderSize, this.delta);
        }

        private void DrawAtlasTextureField()
        {
            var newTilesetTexture = EditorGUILayout.ObjectField(this.tilesetTexture, typeof(Texture), false, GUILayout.Width(120), GUILayout.Height(120)) as Texture2D;
            if (newTilesetTexture != this.tilesetTexture) {
                this.tilesetTexture = newTilesetTexture;
                this.otherTilesetName = null;

                // Find out if another tileset already uses specified texture.
                foreach (var tilesetRecord in BrushDatabase.Instance.TilesetRecords) {
                    if (tilesetRecord.Tileset != null && tilesetRecord.Tileset.AtlasTexture == newTilesetTexture) {
                        this.otherTilesetName = tilesetRecord.DisplayName;
                        break;
                    }
                }
            }
        }

        private void DrawAtlasParametersGUI()
        {
            EditorGUIUtility.fieldWidth = 105f;

            GUILayout.BeginVertical();
            {
                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Width (px)"));
                this.tileWidth = EditorGUILayout.IntField(this.tileWidth, GUILayout.Width(60f));

                ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tile Height (px)"));
                this.tileHeight = EditorGUILayout.IntField(this.tileHeight, GUILayout.Width(60f));

                GUILayout.Space(10f);

                this.alpha = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Alpha Blending"), this.alpha);
                this.procedural = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Procedural"), this.procedural);
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
                    this.borderSize = EditorGUILayout.IntField(this.borderSize);

                    ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Delta (% of 1px)"));
                    GUILayout.BeginHorizontal();
                    {
                        this.delta = Mathf.Clamp(EditorGUILayout.FloatField(this.delta, GUILayout.Width(60f)), 0f, 1f);

                        float newDelta = GUILayout.HorizontalSlider(this.delta, 0f, +1f, GUILayout.Width(80f));
                        if (newDelta != this.delta) {
                            this.delta = (float)((int)(newDelta * 100f)) / 100f;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else {
                    int presetBorderSize = 0;
                    float presetDelta = (this.edgeCorrectionPreset == EdgeCorrectionPreset.InsetUVs) ? 0.5f : 0f;

                    EditorGUI.BeginDisabledGroup(true);
                    {
                        ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Border Size (px)"));
                        EditorGUILayout.IntField(presetBorderSize, GUILayout.Width(60f));

                        ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Delta (% of 1px)"));
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.FloatField(presetDelta, GUILayout.Width(60f));
                            GUILayout.HorizontalSlider(presetDelta, 0f, +1f, GUILayout.Width(80f));
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            GUILayout.EndVertical();
        }


        private bool ValidateInputs(string tilesetName)
        {
            if (!this.ValidateAssetName(tilesetName)) {
                return false;
            }

            foreach (var tilesetRecord in BrushDatabase.Instance.TilesetRecords) {
                if (tilesetRecord.DisplayName == tilesetName) {
                    EditorUtility.DisplayDialog(
                        TileLang.Text("Tileset already exists"),
                        TileLang.Text("Please specify unique name for tileset."),
                        TileLang.ParticularText("Action", "OK")
                    );
                    return false;
                }
            }

            if (this.tilesetTexture == null) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Texture was not specified"),
                    TileLang.Text("Texture must be specified before creating tileset."),
                    TileLang.ParticularText("Action", "OK")
                );
                return false;
            }

            return true;
        }

        private void CreateTileset(string tilesetName)
        {
            // Create folder for atlas assets.
            string atlasFolder = AssetDatabase.GenerateUniqueAssetPath(BrushUtility.GetBrushAssetPath() + tilesetName);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + atlasFolder);

            // Create material for tileset.
            var atlasMaterial = BrushUtility.CreateTilesetMaterial(this.tilesetTexture, this.alpha);

            AssetDatabase.CreateAsset(atlasMaterial, atlasFolder + "/atlas.mat");
            AssetDatabase.ImportAsset(atlasFolder + "/atlas.mat");

            // Use edge correction preset?
            if (this.edgeCorrectionPreset != EdgeCorrectionPreset.Custom) {
                this.borderSize = 0;
                this.delta = (this.edgeCorrectionPreset == EdgeCorrectionPreset.InsetUVs) ? 0.5f : 0f;
            }

            // Calculate metrics for tileset.
            this.RecalculateMetrics();

            // Create tileset.
            var tileset = ScriptableObject.CreateInstance<Tileset>();
            tileset.Initialize(atlasMaterial, this.tilesetTexture, this.tilesetMetrics);
            tileset.procedural = this.procedural;

            // Save tileset and its material to asset file.
            string assetPath = atlasFolder + "/" + tilesetName + ".asset";
            AssetDatabase.CreateAsset(tileset, assetPath);
            AssetDatabase.ImportAsset(assetPath);

            // Ensure that changes are persisted immediately.
            AssetDatabase.SaveAssets();

            // Make sure that "Create Brushes" tab is shown.
            TilesetDesigner.s_SelectedTab = 0;
            ToolUtility.ShowTilesetInDesigner(tileset);

            ToolUtility.RepaintBrushPalette();
        }
    }
}
