// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Basic event handler.
    /// </summary>
    public delegate void BasicHandler();
    /// <summary>
    /// Prefab event handler.
    /// </summary>
    /// <param name="prefab">Reference to prefab object.</param>
    public delegate void PrefabHandler(Object prefab);
    /// <summary>
    /// Tile system event handler.
    /// </summary>
    /// <param name="system">Tile system instance.</param>
    public delegate void TileSystemHandler(TileSystem system);


    /// <summary>
    /// Utility functionality for optimising tile systems.
    /// </summary>
    /// <remarks>
    /// <para>There are a number of events that can be used to perform additional
    /// processing during build process. The events occur in the following order:</para>
    /// <list type="bullet">
    ///    <item><see cref="BuildSceneStart"/> or <see cref="BuildPrefabStart"/></item>
    ///    <item><see cref="PrepareTileSystem"/> (for each tile system)</item>
    ///    <item><see cref="FinalizeTileSystem"/> (for each tile system)</item>
    ///    <item><see cref="BuildSceneComplete"/> or <see cref="BuildPrefabComplete"/></item>
    /// </list>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Tile-System-Optimization">Optimization</a>
    /// section of user guide for further information regarding the optimization of tile
    /// systems.</para>
    /// </remarks>
    public static class BuildUtility
    {
        #region Events

        /// <summary>
        /// Occurs when preparing to build and strip a tile system.
        /// </summary>
        /// <remarks>
        /// <para>This event is invoked before building a tile system to handle
        /// pre-build event for tile system.</para>
        /// </remarks>
        /// <seealso cref="IBuildContext"/>
        public static event BuildEventDelegate PrepareTileSystem;
        /// <summary>
        /// Occurs when tile system has been built and stripped.
        /// </summary>
        /// <remarks>
        /// <para>This event is invoked after building a tile system to handle
        /// post-build event for tile system.</para>
        /// </remarks>
        /// <seealso cref="IBuildContext"/>
        public static event BuildEventDelegate FinalizeTileSystem;

        /// <summary>
        /// Occurs before building scene.
        /// </summary>
        public static event BasicHandler BuildSceneStart;
        /// <summary>
        /// Occurs after scene has been built.
        /// </summary>
        public static event BasicHandler BuildSceneComplete;

        /// <summary>
        /// Occurs before building prefab from tile system.
        /// </summary>
        public static event TileSystemHandler BuildPrefabStart;
        /// <summary>
        /// Occurs after building prefab from tile system.
        /// </summary>
        public static event PrefabHandler BuildPrefabComplete;

        #endregion


        #region Build Tile System

        /// <summary>
        /// Build tile system and apply stripping rules.
        /// </summary>
        /// <param name="builder">Tile system builder.</param>
        /// <param name="system">Tile system.</param>
        private static void BuildTileSystemHelper(TileSystemBuilder builder, TileSystem system)
        {
            builder.SetContext(system);

            if (PrepareTileSystem != null) {
                PrepareTileSystem(builder);
            }

            builder.Build(system);

            if (FinalizeTileSystem != null) {
                FinalizeTileSystem(builder);
            }
        }

        /// <summary>
        /// Build tile system and apply stripping rules.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="progressHandler">Progress handler delegate.</param>
        /// <returns>Tile system builder that was used to build tile system.</returns>
        private static TileSystemBuilder BuildTileSystemHelper(TileSystem system, ProgressDelegate progressHandler = null)
        {
            var builder = new TileSystemBuilder(progressHandler);
            BuildTileSystemHelper(builder, system);
            return builder;
        }

        /// <summary>
        /// Build tile system and apply stripping rules.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="progressHandler">Progress handler delegate.</param>
        public static void BuildTileSystem(TileSystem system, ProgressDelegate progressHandler = null)
        {
            BuildTileSystemHelper(system, progressHandler);
        }

        #endregion


        #region Build Scene

        private static string SaveBuildSceneAs()
        {
            string currentScenePath = EditorApplication.currentScene.Replace("Assets/", Application.dataPath + "/");
            int fileNameIndex = currentScenePath.LastIndexOf('/');

            // Prompt user to save built scene.
            string outputPath;
            while (true) {
                // Prompt user to save scene.
                outputPath = EditorUtility.SaveFilePanel(
                    title: TileLang.ParticularText("Action", "Save Built Scene"),
                    directory: currentScenePath.Substring(0, fileNameIndex),
                    defaultName: currentScenePath.Substring(fileNameIndex + 1).Replace(".unity", "_build.unity"),
                    extension: "unity"
                );
                // Make output path relative to project.
                outputPath = outputPath.Replace(Application.dataPath, "Assets");

                // Attempt to save scene.
                if (!string.IsNullOrEmpty(outputPath)) {
                    if (outputPath == EditorApplication.currentScene) {
                        if (EditorUtility.DisplayDialog(
                            TileLang.Text("Error"),
                            TileLang.ParticularText("Error", "Cannot overwrite current scene with built scene."),
                            TileLang.ParticularText("Action", "Choose Other"),
                            TileLang.ParticularText("Action", "Cancel")
                        )) {
                            continue;
                        }
                        else {
                            return null;
                        }
                    }
                    // Ensure that built scene will be placed within "Assets" directory.
                    else if (outputPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) {
                        break;
                    }
                }

                // Allow user to retry!
                if (!EditorUtility.DisplayDialog(
                    TileLang.Text("Error"),
                    TileLang.ParticularText("Error", "Was unable to save built scene.\n\nWould you like to specify an alternative filename?"),
                    TileLang.ParticularText("Action", "Yes"),
                    TileLang.ParticularText("Action", "No")
                )) {
                    return null;
                }
            }

            return outputPath;
        }

        /// <summary>
        /// Build all tile systems in scene.
        /// </summary>
        /// <remarks>
        /// <para>Each tile system in scene is built in sequence and then the built
        /// version of the scene is saved. Various user interfaces will appear during
        /// the build process.</para>
        /// </remarks>
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Build Scene"),
                TileLang.Text("Scene must be saved before tile systems can be built.\n\nWould you like to save the scene now?"),
                TileLang.ParticularText("Action", "Save and Proceed"),
                TileLang.ParticularText("Action", "Cancel")
            )) {
                //EditorUtility.DisplayDialog(ProductInfo.name, "Could not build scene because it was not saved.", "Close");
                return;
            }

            // Force user to save scene
            if (!EditorSceneManager.SaveOpenScenes()) {
                //EditorUtility.DisplayDialog(ProductInfo.name, "Could not build scene because it was not saved.", "Close");
                return;
            }

            string originalScenePath = EditorApplication.currentScene;

            if (!EditorUtility.DisplayDialog(
                string.Format(
                    /* 0: path of scene */
                    TileLang.Text("Building Scene '{0}'"),
                    originalScenePath
                ),
                TileLang.Text("Open scene was saved successfully.\n\nProceed to save built variation of scene."),
                TileLang.ParticularText("Action", "Proceed"),
                TileLang.ParticularText("Action", "Cancel")
            )) {
                Debug.Log(TileLang.Text("Building of tile systems was cancelled."));
                return;
            }

            // Prompt user to save built scene.
            string outputPath = SaveBuildSceneAs();
            if (outputPath == null) {
                return;
            }

            // Save output scene straight away!
            if (!EditorApplication.SaveScene(outputPath)) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Error"),
                    string.Format(
                        /* 0: path to output asset */
                        TileLang.ParticularText("Error", "Was unable to save output scene '{0}'"),
                        outputPath
                    ),
                    TileLang.ParticularText("Action", "Close")
                );
                return;
            }

            if (BuildSceneStart != null) {
                BuildSceneStart();
            }

            bool hasErrors = false;

            // Fetch all tile systems in scene.
            TileSystem[] systems = GameObject.FindObjectsOfType<TileSystem>();

            float overallTaskCount = 1f;
            float overallTaskOffset = 0f;
            float overallTaskRatio = 0f;

            TileSystemBuilder builder = new TileSystemBuilder();

            builder.progressHandler = delegate (string title, string status, float percentage) {
                float progress = (builder.Task + overallTaskOffset) * overallTaskRatio;
                return EditorUtility.DisplayCancelableProgressBar(title, status, progress);
            };

            // Count tasks in advance.
            foreach (TileSystem system in systems) {
                // Skip non-editable tile systems and output warning message to console.
                if (!system.IsEditable) {
                    Debug.LogWarning(string.Format(
                        /* 0: name of tile system */
                        TileLang.Text("Skipping non-editable tile system '{0}'."),
                        system.name
                    ), system);
                    continue;
                }

                overallTaskCount += builder.CountTasks(system);
            }

            overallTaskRatio = 1f / overallTaskCount;

            try {
                // Build each tile system in turn.
                foreach (TileSystem system in systems) {
                    // Skip non-editable tile systems.
                    if (!system.IsEditable) {
                        continue;
                    }

                    BuildTileSystemHelper(builder, system);

                    // Adjust overall task offset.
                    overallTaskOffset += builder.TaskCount;
                }

                builder.progressHandler(
                    TileLang.Text("Building tile systems"),
                    TileLang.Text("Cleaning up..."),
                    progress: (overallTaskCount - 1f) * overallTaskRatio
                );
                builder = null;

                UnityEngine.Resources.UnloadUnusedAssets();
                GC.Collect();
            }
            catch (Exception ex) {
                Debug.LogError(ex.ToString());
                hasErrors = true;
            }
            finally {
                // Finished with progress bar!
                EditorUtility.ClearProgressBar();
            }

            // Save output scene.
            if (!EditorApplication.SaveScene(outputPath)) {
                Debug.LogError(string.Format(
                    /* 0: scene file path */
                    TileLang.ParticularText("Error", "Was unable to save output scene '{0}'"),
                    outputPath
                ));
            }

            // Raise event for end of build process.
            if (BuildSceneComplete != null) {
                BuildSceneComplete();
            }

            // Rollback changes.
            EditorApplication.OpenScene(originalScenePath);

            if (hasErrors) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Warning"),
                    TileLang.Text("Errors were encountered whilst building scene. Please check console for any additional information."),
                    TileLang.ParticularText("Action", "Close")
                );
            }
        }

        #endregion


        /// <summary>
        /// Build optimized prefab from a tile system.
        /// </summary>
        /// <remarks>
        /// <para>Presents various user interfaces.</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <param name="dataPath">Output path for generated data asset.</param>
        /// <param name="prefabPath">Output path for generated prefab asset.</param>
        public static void BuildPrefab(TileSystem system, string dataPath, string prefabPath)
        {
            // Destroy mesh if it already exists.
            AssetDatabase.DeleteAsset(dataPath);
            // Destroy prefab if it already exists.
            AssetDatabase.DeleteAsset(prefabPath);

            // Duplicate tile system for processing.
            var duplicateTileSystemGO = GameObject.Instantiate(system.gameObject);
            var duplicateTileSystem = duplicateTileSystemGO.GetComponent<TileSystem>();

            InternalUtility.EnableProgressHandler = true;

            // Raise event for start of building prefab.
            if (BuildPrefabStart != null) {
                BuildPrefabStart(duplicateTileSystem);
            }

            try {
                string tileSystemName = duplicateTileSystem.name;

                var tileSystemBuilder = BuildTileSystemHelper(duplicateTileSystem, InternalUtility.CancelableProgressHandler);

                // Save generated meshes to asset.
                TileSystemPrefabAsset assetData = ScriptableObject.CreateInstance<TileSystemPrefabAsset>();
                AssetDatabase.CreateAsset(assetData, dataPath);
                foreach (var mesh in tileSystemBuilder.GeneratedMeshes) {
                    AssetDatabase.AddObjectToAsset(mesh, assetData);
                }
                AssetDatabase.ImportAsset(dataPath);

                EditorUtility.DisplayProgressBar(
                    string.Format(
                        /* 0: name of tile system */
                        TileLang.Text("Building tile system '{0}'..."),
                        tileSystemName
                    ),
                    TileLang.Text("Saving prefab and data asset..."),
                    progress: 1f
                );

                // Create prefab output.
                var newPrefab = PrefabUtility.CreatePrefab(prefabPath, duplicateTileSystemGO, ReplacePrefabOptions.ConnectToPrefab);

                // Raise event for end of building prefab.
                if (BuildPrefabComplete != null) {
                    BuildPrefabComplete(newPrefab);
                }
            }
            catch {
                throw;
            }
            finally {
                // Destroy duplicate tile system.
                Object.DestroyImmediate(duplicateTileSystemGO);
                // Finished with progress bar!
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
