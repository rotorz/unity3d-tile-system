// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.EditorExtensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility functions to asset with the management and usage of tile system presets.
    /// </summary>
    /// <seealso cref="TileSystemPreset"/>
    /// <seealso cref="TileSystem"/>
    public static class TileSystemPresetUtility
    {
        #region Preset Management

        /// <summary>
        /// Gets the default preset GUID based upon whether the project is in 2D or 3D mode.
        /// </summary>
        internal static string DefaultPresetGUID {
            get { return EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode3D ? "F:3D" : "F:2D"; }
        }

        /// <summary>
        /// Determines whether the specified name is valid for a tile system preset.
        /// </summary>
        /// <remarks>
        /// <para>The name of a preset may contain alphanumeric characters (A-Z, a-z, 0-9),
        /// underscores _, hyphens - and spaces.</para>
        /// </remarks>
        /// <param name="presetName">The candidate preset name.</param>
        /// <returns>
        /// A value of <c>true</c> if the name was valid; otherwise, a value of <c>false</c>
        /// if the name was invalid or <c>null</c>.
        /// </returns>
        public static bool IsValidPresetName(string presetName)
        {
            if (presetName == null) {
                return false;
            }
            presetName = presetName.Trim();
            return presetName != "" && Regex.IsMatch(presetName, @"^[a-z0-9_\- ]+$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified tile system preset is located within the
        /// main "User Data" directory structure.
        /// </summary>
        /// <param name="preset">Tile system preset.</param>
        /// <returns>
        /// A value of <c>true</c> if the preset is located in the main "User Data"
        /// directory; otherwise, a value of <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="preset"/> is <c>null</c>.
        /// </exception>
        public static bool IsUserPreset(TileSystemPreset preset)
        {
            if (preset == null) {
                throw new ArgumentNullException("preset");
            }

            string presetAssetPath = AssetDatabase.GetAssetPath(preset);
            string userPresetsBasePath = PackageUtility.ResolveDataAssetPath("@rotorz/unity3d-tile-system", "Presets") + "/";
            return presetAssetPath.StartsWith(userPresetsBasePath);
        }

        internal static string GetPresetGUID(TileSystemPreset preset)
        {
            return preset != null
                ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(preset))
                : null;
        }

        /// <summary>
        /// Loads the <see cref="TileSystemPreset"/> from the specified preset asset GUID.
        /// </summary>
        /// <param name="presetGuid">GUID of the preset.</param>
        /// <returns>
        /// The <see cref="TileSystemPreset"/> reference when found; otherwise, a value
        /// of <c>null</c> if preset does not exist.
        /// </returns>
        public static TileSystemPreset LoadPresetFromGUID(string presetGuid)
        {
            return presetGuid != "F:3D" && presetGuid != "F:2D"
                ? AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(presetGuid), typeof(TileSystemPreset)) as TileSystemPreset
                : null;
        }

        /// <summary>
        /// Gets an array of all the <see cref="TileSystemPreset"/> assets in the project.
        /// </summary>
        /// <returns>
        /// Array of <see cref="TileSystemPreset"/> assets.
        /// </returns>
        public static TileSystemPreset[] GetPresets()
        {
            return (
                from assetGuid in AssetDatabase.FindAssets("t:Rotorz.Tile.Editor.TileSystemPreset")
                select AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGuid), typeof(TileSystemPreset)) as TileSystemPreset
            ).ToArray();
        }

        private static TileSystemPreset SavePresetAsHelper(TileSystemPreset source, string presetAssetPath)
        {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (string.IsNullOrEmpty(presetAssetPath)) {
                throw new ArgumentNullException("presetAssetPath");
            }

            // Load existing preset if it exists.
            var preset = AssetDatabase.LoadAssetAtPath(presetAssetPath, typeof(TileSystemPreset)) as TileSystemPreset;
            if (preset != null) {
                EditorUtility.CopySerialized(source, preset);
                EditorUtility.SetDirty(preset);
                AssetDatabase.SaveAssets();
            }
            else {
                preset = Object.Instantiate(source) as TileSystemPreset;
                preset.name = Regex.Match(presetAssetPath, @"/([a-z0-9_\- ]+)\.asset$", RegexOptions.IgnoreCase).Groups[1].Value;
                AssetDatabase.CreateAsset(preset, presetAssetPath);
                AssetDatabase.Refresh();
            }

            return preset;
        }

        /// <summary>
        /// Creates a new tile system preset by copying the properties of the source
        /// preset. If a preset already exists with the same name then it will be
        /// overwritten.
        /// </summary>
        /// <param name="source">Source preset.</param>
        /// <param name="presetName">Name of the new preset.</param>
        /// <returns>
        /// A reference to the new <see cref="TileSystemPreset"/> asset.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <list type="bullet">
        /// <item>If <paramref name="source"/> is <c>null</c>.</item>
        /// <item>If <paramref name="presetName"/> is <c>null</c>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="presetName"/> is not a valid preset name.
        /// </exception>
        public static TileSystemPreset CreatePreset(TileSystemPreset source, string presetName)
        {
            if (presetName == null) {
                throw new ArgumentNullException("presetName");
            }
            if (!IsValidPresetName(presetName)) {
                throw new ArgumentException("Invalid preset name.", "presetName");
            }

            string presetAssetPath = PackageUtility.GetDataAssetPath("@rotorz/unity3d-tile-system", "Presets", presetName.Trim() + ".asset");
            return SavePresetAsHelper(source, presetAssetPath);
        }

        /// <summary>
        /// Overwrites an existing tile system preset by copying the properties of the
        /// source preset into the destination preset.
        /// </summary>
        /// <param name="source">Source preset.</param>
        /// <param name="dest">Destination preset.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <list type="bullet">
        /// <item>If <paramref name="source"/> is <c>null</c>.</item>
        /// <item>If <paramref name="dest"/> is <c>null</c>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="dest"/> is not a persisted asset file.
        /// </exception>
        public static void OverwritePreset(TileSystemPreset source, TileSystemPreset dest)
        {
            if (dest == null) {
                throw new ArgumentNullException("dest");
            }
            if (!EditorUtility.IsPersistent(dest)) {
                throw new InvalidOperationException("Cannot overwrite non-persistent preset.");
            }

            string presetAssetPath = AssetDatabase.GetAssetPath(dest);
            SavePresetAsHelper(source, presetAssetPath);
        }

        /// <summary>
        /// Deletes an unwanted tile system preset.
        /// </summary>
        /// <param name="preset">Tile system preset.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="preset"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// If <paramref name="preset"/> is not a known asset file.
        /// </exception>
        public static void DeletePreset(TileSystemPreset preset)
        {
            if (preset == null) {
                throw new ArgumentNullException("preset");
            }
            if (!AssetDatabase.Contains(preset)) {
                throw new InvalidOperationException(string.Format("Cannot delete preset '{0}' because it is not an asset file.", preset.name));
            }

            string assetPath = AssetDatabase.GetAssetPath(preset);
            AssetDatabase.MoveAssetToTrash(assetPath);

            PackageUtility.DeleteDataFolderIfEmpty("@rotorz/unity3d-tile-system", "Presets");
        }

        #endregion


        #region Tile System Creation

        /// <summary>
        /// Creates a new tile system using properties from the specified preset.
        /// </summary>
        /// <remarks>
        /// <para>This method does not automatically record the new game object with
        /// Unity's undo system. If undo functionality is desired then the callee should
        /// do this.</para>
        /// </remarks>
        /// <param name="preset">Tile system preset.</param>
        /// <returns>
        /// A new game object with an initialized <see cref="TileSystem"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="preset"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// <list type="bullet">
        /// <item>If preset defines an invalid name for a tile system.</item>
        /// <item>If preset defines a tile system with less than one cell.</item>
        /// </list>
        /// </exception>
        public static GameObject CreateTileSystemFromPreset(TileSystemPreset preset)
        {
            if (preset == null) {
                throw new ArgumentNullException("preset");
            }

            string name = preset.SystemName.Trim();

            if (string.IsNullOrEmpty(name)) {
                throw new InvalidOperationException("Invalid name for tile system.");
            }
            if (preset.Rows < 1 || preset.Columns < 1) {
                throw new InvalidOperationException("Tile system must have at least one cell.");
            }

            // Create empty game object and add tile system component.
            var go = new GameObject(name);
            var tileSystem = go.AddComponent<TileSystem>();
            tileSystem.CreateSystem(preset.TileWidth, preset.TileHeight, preset.TileDepth, preset.Rows, preset.Columns, preset.ChunkWidth, preset.ChunkHeight);

            TransformTileSystemUsingPreset(preset, go.transform);
            SetTileSystemPropertiesFromPreset(preset, tileSystem);

            // Place at end of the scene palette listing.
            tileSystem.sceneOrder = int.MaxValue;

            ToolUtility.RepaintScenePalette();

            return go;
        }

        private static void TransformTileSystemUsingPreset(TileSystemPreset preset, Transform tileSystemTransform)
        {
            switch (preset.Direction) {
                default:
                case WorldDirection.Forward:
                    break;
                case WorldDirection.Backward:
                    tileSystemTransform.Rotate(Vector3.up, 180f, Space.Self);
                    break;
                case WorldDirection.Up:
                    tileSystemTransform.Rotate(Vector3.right, 90f, Space.Self);
                    break;
                case WorldDirection.Down:
                    tileSystemTransform.Rotate(Vector3.right, 270f, Space.Self);
                    break;
                case WorldDirection.Left:
                    tileSystemTransform.Rotate(Vector3.up, 90f, Space.Self);
                    break;
                case WorldDirection.Right:
                    tileSystemTransform.Rotate(Vector3.up, 270f, Space.Self);
                    break;
            }
        }

        private static void SetTileSystemPropertiesFromPreset(TileSystemPreset preset, TileSystem tileSystem)
        {
            // Grid
            tileSystem.TilesFacing = preset.TilesFacing;

            // Stripping
            tileSystem.StrippingPreset = preset.StrippingPreset;
            if (preset.StrippingPreset == StrippingPreset.Custom) {
                tileSystem.StrippingOptions = preset.StrippingOptions;
            }

            // Build Options
            tileSystem.combineMethod = preset.CombineMethod;
            tileSystem.combineChunkWidth = preset.CombineChunkWidth;
            tileSystem.combineChunkHeight = preset.CombineChunkHeight;
            tileSystem.combineIntoSubmeshes = preset.CombineIntoSubmeshes;
            tileSystem.staticVertexSnapping = preset.StaticVertexSnapping;
            tileSystem.vertexSnapThreshold = preset.VertexSnapThreshold;

            tileSystem.GenerateSecondUVs = preset.GenerateSecondUVs;
            tileSystem.SecondUVsHardAngle = preset.GenerateSecondUVsParams.hardAngle;
            tileSystem.SecondUVsPackMargin = preset.GenerateSecondUVsParams.packMargin;
            tileSystem.SecondUVsAngleError = preset.GenerateSecondUVsParams.angleError;
            tileSystem.SecondUVsAreaError = preset.GenerateSecondUVsParams.areaError;

            tileSystem.pregenerateProcedural = preset.PregenerateProcedural;

            tileSystem.ReduceColliders.SetFrom(preset.ReduceColliders);

            // Runtime Options
            tileSystem.hintEraseEmptyChunks = preset.HintEraseEmptyChunks;
            tileSystem.applyRuntimeStripping = preset.ApplyRuntimeStripping;

            tileSystem.updateProceduralAtStart = preset.UpdateProceduralAtStart;
            tileSystem.MarkProceduralDynamic = preset.MarkProceduralDynamic;
            tileSystem.addProceduralNormals = preset.AddProceduralNormals;

            tileSystem.SortingLayerID = preset.SortingLayerID;
            tileSystem.SortingOrder = preset.SortingOrder;
        }

        #endregion
    }
}
