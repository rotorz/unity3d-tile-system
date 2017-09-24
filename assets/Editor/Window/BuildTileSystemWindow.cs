// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Build tile system window.
    /// </summary>
    internal sealed class BuildTileSystemWindow : RotorzWindow
    {
        #region Window Management

        /// <summary>
        /// Display the build window.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <returns>
        /// The window.
        /// </returns>
        public static BuildTileSystemWindow ShowWindow(TileSystem system)
        {
            var window = GetUtilityWindow<BuildTileSystemWindow>(
                title: string.Format(
                    /* 0: name of tile system */
                    TileLang.Text("Build Optimized Prefab from '{0}'"),
                    system.name
                )
            );

            window.tileSystem = system;

            return window;
        }

        #endregion


        private TileSystem tileSystem;

        [NonSerialized]
        private bool hasDrawnGUI;


        /// <summary>
        /// Gets the tile system.
        /// </summary>
        /// <value>
        /// The tile system.
        /// </value>
        public TileSystem TileSystem {
            get { return this.tileSystem; }
        }

        private string PrefabOutputPath {
            get { return this.TileSystem.LastBuildPrefabPath ?? ""; }
            set {
                if (this.TileSystem.LastBuildPrefabPath != value) {
                    string oldAutoDataPath = this.TileSystem.LastBuildPrefabPath + " (data)";
                    this.TileSystem.LastBuildPrefabPath = value;

                    // Automatically fill asset path for convenience.
                    if (this.DataOutputPath == "" || this.DataOutputPath == oldAutoDataPath) {
                        this.DataOutputPath = (value != "" ? value + " (data)" : "");
                    }

                    EditorUtility.SetDirty(this.TileSystem);
                }
            }
        }

        private string DataOutputPath {
            get { return this.TileSystem.LastBuildDataPath ?? ""; }
            set {
                if (this.TileSystem.LastBuildDataPath != value) {
                    this.TileSystem.LastBuildDataPath = value;
                    EditorUtility.SetDirty(this.TileSystem);
                }
            }
        }


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.InitialSize = this.minSize = this.maxSize = new Vector2(420, 170);
        }

        private void Update()
        {
            if (this.hasDrawnGUI) {
                // If tile system is missing, close window!
                if (this.TileSystem == null) {
                    this.Close();
                }
            }
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            this.hasDrawnGUI = true;

            if (this.tileSystem == null) {
                return;
            }

            GUISkin skin = GUI.skin;

            GUILayout.Space(15f);

            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Prefab Output Path:"));
            GUILayout.BeginHorizontal();
            {
                this.PrefabOutputPath = RotorzEditorGUI.RelativeAssetPathTextField(GUIContent.none, this.PrefabOutputPath, ".prefab");
                if (GUILayout.Button("...", GUILayout.Width(40))) {
                    GUIUtility.keyboardControl = 0;
                    this.PrefabOutputPath = this.DoSelectPrefabPath(
                        TileLang.ParticularText("Action", "Save Prefab Output"),
                        TileLang.Text("Specify path for prefab output"),
                        this.PrefabOutputPath
                    );
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Data Output Path:"));
            GUILayout.BeginHorizontal();
            {
                this.DataOutputPath = RotorzEditorGUI.RelativeAssetPathTextField(GUIContent.none, this.DataOutputPath, ".asset");
                if (GUILayout.Button("...", GUILayout.Width(40))) {
                    GUIUtility.keyboardControl = 0;
                    this.DataOutputPath = this.DoSelectAssetPath(
                        TileLang.ParticularText("Action", "Save Data Output"),
                        TileLang.Text("Specify path for mesh output"),
                        this.DataOutputPath
                    );
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            ExtraEditorGUI.Separator(marginBottom: 10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                this.OnGUI_Buttons();
                GUILayout.Space(5f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
        }

        private void OnGUI_Buttons()
        {
            if (GUILayout.Button(TileLang.ParticularText("Action", "Build"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.DoBuild();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(5f);

            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButton)) {
                this.Close();
                GUIUtility.ExitGUI();
            }
        }

        private string DoSelectPrefabPath(string title, string prompt, string path)
        {
            string assetPath = this.GetResolvedPrefabPath();
            if (assetPath == "") {
                assetPath = "Assets/" + this.TileSystem.name + ".prefab";
            }

            string assetFolder = Path.GetDirectoryName(assetPath);
            string assetFilename = Path.GetFileName(assetPath);

            assetPath = EditorUtility.SaveFilePanelInProject(title, assetFilename, "prefab", path, assetFolder);
            if (!string.IsNullOrEmpty(assetPath)) {
                path = assetPath.Substring(7, assetPath.Length - 7 - "prefab".Length - 1);
            }

            return path;
        }

        private string DoSelectAssetPath(string title, string prompt, string path)
        {
            string assetPath = this.GetResolvedDataPath();
            if (assetPath == "") {
                assetPath = "Assets/" + this.TileSystem.name + " (data).asset";
            }

            string assetFolder = Path.GetDirectoryName(assetPath);
            string assetFilename = Path.GetFileName(assetPath);

            assetPath = EditorUtility.SaveFilePanelInProject(title, assetFilename, "asset", path, assetFolder);
            if (!string.IsNullOrEmpty(assetPath)) {
                path = assetPath.Substring(7, assetPath.Length - 7 - "asset".Length - 1);
            }

            return path;
        }

        private void DoBuild()
        {
            this.PrefabOutputPath = this.PrefabOutputPath.Trim();
            this.DataOutputPath = this.DataOutputPath.Trim();

            string resolvedPrefabPath = this.GetResolvedPrefabPath();
            string resolvedDataPath = this.GetResolvedDataPath();

            if (this.ValidateAssetPath("prefab", resolvedPrefabPath) && this.ValidateAssetPath("data asset", resolvedDataPath)) {
                // Confirm action with user if prefab and/or data asset already exist.
                bool outputPrefabAlreadyExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), resolvedPrefabPath));
                bool outputDataAlreadyExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), resolvedDataPath));
                if (outputPrefabAlreadyExists || outputDataAlreadyExists) {
                    if (!EditorUtility.DisplayDialog(
                        TileLang.Text("Warning, Output prefab or data asset already exists!"),
                        TileLang.Text("Do you really want to overwrite?"),
                        TileLang.ParticularText("Action", "Yes"),
                        TileLang.ParticularText("Action", "No")
                    )) {
                        return;
                    }
                }

                BuildUtility.BuildPrefab(this.TileSystem, resolvedDataPath, resolvedPrefabPath);
                this.Close();
            }
        }

        private string GetResolvedPrefabPath()
        {
            return this.PrefabOutputPath != ""
                ? "Assets/" + this.PrefabOutputPath + ".prefab"
                : "";
        }

        private string GetResolvedDataPath()
        {
            return this.DataOutputPath != ""
                ? "Assets/" + this.DataOutputPath + ".asset"
                : "";
        }

        private bool ValidateAssetPath(string type, string assetPath)
        {
            if (string.IsNullOrEmpty(Path.GetFileName(assetPath)) || assetPath.EndsWith("/.asset") || assetPath.EndsWith("/.prefab")) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Input Error"),
                    string.Format(
                        /* 0: type of path */
                        TileLang.ParticularText("Error", "No filename was specified for {0}."),
                        type
                    ),
                    TileLang.ParticularText("Action", "Close")
                );
                return false;
            }
            if (assetPath.Contains("/..")) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Input Error"),
                    string.Format(
                        /* 0: type of path */
                        TileLang.ParticularText("Error", "Relative path is not allowed for {0}."),
                        type
                    ),
                    TileLang.ParticularText("Action", "Close")
                );
                return false;
            }
            if (assetPath.IndexOfAny(Path.GetInvalidPathChars()) != -1) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Input Error"),
                    string.Format(
                        /* 0: type of path */
                        TileLang.ParticularText("Error", "Invalid characters were found in {0} path."),
                        type
                    ),
                    TileLang.ParticularText("Action", "Close")
                );
                return false;
            }

            return true;
        }
    }
}
