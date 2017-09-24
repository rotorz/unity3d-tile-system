// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility functionality for interacting with brush assets.
    /// </summary>
    [InitializeOnLoad]
    public static class BrushUtility
    {
        static BrushUtility()
        {
            // Register built-in brush kinds.
            RegisterDescriptor<Brush, BasicBrushDesigner, AliasBrushDesigner>();
            RegisterDescriptor(new OrientedBrushDescriptor());
            RegisterDescriptor(new AliasBrushDescriptor());
            RegisterDescriptor(new TilesetBrushDescriptor());
            RegisterDescriptor(new AutotileBrushDescriptor());
            RegisterDescriptor(new EmptyBrushDescriptor());
        }


        #region Brush Descriptors

        private static Dictionary<Type, BrushDescriptor> s_Descriptors = new Dictionary<Type, BrushDescriptor>();


        /// <summary>
        /// Register custom brush descriptor.
        /// </summary>
        /// <remarks>
        /// <para>A brush descriptor maps a brush with its corresponding designer along
        /// with its specialized alias designer. A custom brush descriptor can also
        /// override the way in which previews are generated and/or presented.</para>
        /// <para>The specified brush descriptor will be used in place of existing descriptor
        /// if one has already been registered for the associated brush type.</para>
        /// </remarks>
        /// <param name="descriptor">Custom brush descriptor.</param>
        public static void RegisterDescriptor(BrushDescriptor descriptor)
        {
            if (s_Descriptors.ContainsKey(descriptor.BrushType)) {
                s_Descriptors[descriptor.BrushType] = descriptor;
            }
            else {
                s_Descriptors.Add(descriptor.BrushType, descriptor);
            }
        }

        /// <summary>
        /// Register custom brush descriptor.
        /// </summary>
        /// <remarks>
        /// <para>A brush descriptor maps a brush with its corresponding designer along
        /// with its specialized alias designer.</para>
        /// <para>The specified brush descriptor will be used in place of existing descriptor
        /// if one has already been registered for the associated brush type.</para>
        /// </remarks>
        /// <typeparam name="TBrush">Type of brush.</typeparam>
        /// <typeparam name="TDesigner">Type of brush designer.</typeparam>
        /// <typeparam name="TAliasDesigner">Type of alias brush designer.</typeparam>
        /// <returns>
        /// The brush descriptor.
        /// </returns>
        public static BrushDescriptor RegisterDescriptor<TBrush, TDesigner, TAliasDesigner>()
        {
            var descriptor = new BrushDescriptor(typeof(TBrush), typeof(TDesigner), typeof(TAliasDesigner));
            RegisterDescriptor(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Register custom brush descriptor.
        /// </summary>
        /// <remarks>
        /// <para>A brush descriptor maps a brush with its corresponding designer.</para>
        /// <para>The specified brush descriptor will be used in place of existing descriptor
        /// if one has already been registered for the associated brush type.</para>
        /// </remarks>
        /// <typeparam name="TBrush">Type of brush.</typeparam>
        /// <typeparam name="TDesigner">Type of brush designer.</typeparam>
        /// <returns>
        /// The brush descriptor.
        /// </returns>
        public static BrushDescriptor RegisterDescriptor<TBrush, TDesigner>()
        {
            var descriptor = new BrushDescriptor(typeof(TBrush), typeof(TDesigner), null);
            RegisterDescriptor(descriptor);
            return descriptor;
        }

        /// <summary>
        /// Get descriptor for specific type of brush.
        /// </summary>
        /// <typeparam name="TBrush">Type of brush.</typeparam>
        /// <returns>
        /// The brush descriptor; or <c>null</c> when if not registered.
        /// </returns>
        public static BrushDescriptor GetDescriptor<TBrush>() where TBrush : Brush
        {
            Type key = typeof(TBrush);
            return s_Descriptors.ContainsKey(key)
                ? s_Descriptors[key]
                : null;
        }

        /// <summary>
        /// Get descriptor for specific type of brush.
        /// </summary>
        /// <param name="brushType">Type of brush.</param>
        /// <returns>
        /// The brush descriptor; or <c>null</c> when if not registered.
        /// </returns>
        public static BrushDescriptor GetDescriptor(Type brushType)
        {
            return s_Descriptors.ContainsKey(brushType)
                ? s_Descriptors[brushType]
                : null;
        }

        #endregion


        #region Previews

        /// <summary>
        /// The brush preview renderer utility.
        /// </summary>
        private static BrushPreviewRenderUtility s_BrushPreviewRenderUtility;

        /// <summary>
        /// Gets brush preview renderer utility instance. Renderer utility is automatically
        /// instantiated if it doesn't already exists.
        /// </summary>
        private static BrushPreviewRenderUtility BrushPreviewRenderUtility {
            get {
                if (s_BrushPreviewRenderUtility == null) {
                    s_BrushPreviewRenderUtility = new BrushPreviewRenderUtility();
                }
                return s_BrushPreviewRenderUtility;
            }
        }


        internal static Texture2D CreateBrushPreview(Brush brush, int width, int height)
        {
            return BrushPreviewRenderUtility.CreateStaticTexture(brush, width, height);
        }

        /// <summary>
        /// Find all brushes which depend upon the specified brush.
        /// </summary>
        /// <remarks>
        /// <para>This function currently only considers alias brushes.</para>
        /// </remarks>
        /// <param name="brush">The specified brush.</param>
        /// <returns>
        /// Enumerable collection of brush records.
        /// </returns>
        private static IEnumerable<BrushAssetRecord> FindDependencyBrushes(Brush brush)
        {
            return BrushDatabase.Instance.BrushRecords.Where(record => {
                var aliasBrush = record.Brush as AliasBrush;
                return aliasBrush != null && aliasBrush.target == brush;
            });
        }

        /// <summary>
        /// Refresh preview image for specified brush and also refresh preview image
        /// for brushes which depend upon the specified brush.
        /// </summary>
        /// <remarks>
        /// <para>At the moment this only includes alias brushes which target the
        /// specified brush. In the future this may be updated to include oriented
        /// brushes which nest the specified brush.</para>
        /// </remarks>
        /// <param name="brush">The brush.</param>
        internal static void RefreshPreviewIncludingDependencies(Brush brush)
        {
            BrushAssetRecord record = BrushDatabase.Instance.FindRecord(brush);
            if (record != null) {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(brush));

                // Refresh preview any alias brushes which target this brush.
                foreach (var dependencyRecord in FindDependencyBrushes(record.Brush)) {
                    RefreshPreview(dependencyRecord.Brush);
                }

                ToolUtility.RepaintBrushPalette();
            }
        }

        /// <summary>
        /// Refresh preview image for brush.
        /// </summary>
        /// <param name="brush">The brush.</param>
        public static void RefreshPreview(Brush brush)
        {
            AssetPreviewCache.ClearCachedAssetPreviewFile(brush);
        }

        #endregion


        /// <summary>
        /// Gets default collider type for default editor behavior.
        /// </summary>
        /// <remarks>
        /// <para>See <c>EditorSettings.defaultBehaviorMode</c>.</para>
        /// </remarks>
        public static ColliderType AutomaticColliderType {
            get {
                return EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode3D
                    ? ColliderType.BoxCollider3D
                    : ColliderType.BoxCollider2D;
            }
        }

        /// <summary>
        /// Creates duplicate of an existing brush.
        /// </summary>
        /// <param name="name">Name of brush.</param>
        /// <param name="existing">Existing brush that is to be duplicated.</param>
        /// <returns>
        /// The duplicate brush; or <c>null</c> if an error occurred.
        /// </returns>
        public static Brush DuplicateBrush(string name, Brush existing)
        {
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("No name specified.");
                return null;
            }

            if (existing == null) {
                return null;
            }

            var record = BrushDatabase.Instance.FindRecord(existing);
            if (record == null) {
                return null;
            }

            var descriptor = GetDescriptor(existing.GetType());
            if (descriptor == null) {
                return null;
            }

            // Attempt to duplicate brush.
            return descriptor.DuplicateBrush(name, record);
        }

        private static void SaveBrushAsset(Brush brush, string assetPath)
        {
            AssetDatabase.CreateAsset(brush, assetPath);
            AssetDatabase.ImportAsset(assetPath);

            // Add default "brush" label to brush asset.
            AssetDatabase.SetLabels(brush, new string[] { "brush" });
        }

        /// <summary>
        /// Gets base path for the folder where new brush and tileset assets are created.
        /// Folder is automatically created if it does not already exist.
        /// </summary>
        /// <seealso cref="ProjectSettings.BrushesFolderRelativePath"/>
        public static string GetBrushAssetPath()
        {
            string brushesFolderAssetPath = "Assets/" + ProjectSettings.Instance.BrushesFolderRelativePath;
            EditorInternalUtility.EnsureThatAssetFolderExists(brushesFolderAssetPath);
            return brushesFolderAssetPath + "/";
        }

        /// <summary>
        /// Creates new oriented brush asset.
        /// </summary>
        /// <remarks>
        /// <para>Oriented brush will be initialized with the default orientation "00000000"
        /// without any variations present. The default orientation can be accessed easily
        /// via <see cref="Rotorz.Tile.OrientedBrush.DefaultOrientation">OrientedBrush.DefaultOrientation</see>.
        /// Additional orientations can be added using <see cref="Rotorz.Tile.OrientedBrush.AddOrientation(int)">OrientedBrush.AddOrientation</see>.</para>
        /// <para>Brush asset should be marked as dirty once you have finished making
        /// modifications using <see cref="UnityEditor.EditorUtility.SetDirty">UnityEditor.EditorUtility.SetDirty</see>.</para>
        /// </remarks>
        /// <param name="name">Name of brush.</param>
        /// <returns>
        /// The brush; or <c>null</c> if an error has occurred.
        /// </returns>
        public static OrientedBrush CreateOrientedBrush(string name)
        {
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("No name specified.");
                return null;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(GetBrushAssetPath() + name + ".asset");

            // Create new brush.
            var orientedBrush = ScriptableObject.CreateInstance<OrientedBrush>();
            orientedBrush.AddOrientation(0);

            orientedBrush.forceLegacySideways = false;

            SaveBrushAsset(orientedBrush, assetPath);
            return orientedBrush;
        }

        /// <summary>
        /// Creates new alias brush asset.
        /// </summary>
        /// <remarks>
        /// <para>Brush asset should be marked as dirty once you have finished making
        /// modifications using <see cref="M:UnityEditor.EditorUtility.SetDirty">UnityEditor.EditorUtility.SetDirty</see>.</para>
        /// </remarks>
        /// <param name="name">Name of brush.</param>
        /// <param name="target">Target for alias brush. Specify <c>null</c> for none.</param>
        /// <returns>
        /// The brush; or <c>null</c> if an error has occurred.
        /// </returns>
        public static AliasBrush CreateAliasBrush(string name, Brush target = null)
        {
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("No name specified.");
                return null;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(GetBrushAssetPath() + name + ".asset");

            // Create new brush.
            var aliasBrush = ScriptableObject.CreateInstance<AliasBrush>();
            aliasBrush.target = target;

            aliasBrush.forceLegacySideways = false;

            if (target != null) {
                BrushDescriptor descriptor = BrushUtility.GetDescriptor(target.GetType());
                if (descriptor == null || !descriptor.SupportsAliases) {
                    Debug.LogError("No alias designer was registered for '" + target.GetType().FullName + "'");
                    return null;
                }

                aliasBrush.RevertToTarget();
            }

            SaveBrushAsset(aliasBrush, assetPath);
            return aliasBrush;
        }

        /// <summary>
        /// Creates new empty brush asset.
        /// </summary>
        /// <remarks>
        /// <para>Brush asset should be marked as dirty once you have finished making
        /// modifications using <see cref="M:UnityEditor.EditorUtility.SetDirty">UnityEditor.EditorUtility.SetDirty</see>.</para>
        /// </remarks>
        /// <param name="name">Name of brush.</param>
        /// <returns>
        /// The brush; or <c>null</c> if an error occurred.
        /// </returns>
        public static EmptyBrush CreateEmptyBrush(string name)
        {
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("No name specified.");
                return null;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(GetBrushAssetPath() + name + ".asset");

            // Create new brush.
            var emptyBrush = ScriptableObject.CreateInstance<EmptyBrush>();

            emptyBrush.forceLegacySideways = false;

            // Set collider type based upon editor behavior mode.
            switch (EditorSettings.defaultBehaviorMode) {
                case EditorBehaviorMode.Mode2D:
                    emptyBrush.colliderType = ColliderType.BoxCollider2D;
                    break;
                case EditorBehaviorMode.Mode3D:
                    emptyBrush.colliderType = ColliderType.BoxCollider3D;
                    break;
            }

            SaveBrushAsset(emptyBrush, assetPath);
            return emptyBrush;
        }

        internal static bool s_DirtyMesh;

        /// <summary>
        /// Creates new tileset brush asset.
        /// </summary>
        /// <remarks>
        /// <para>Brush asset should be marked as dirty once you have finished making
        /// modifications using <see cref="M:UnityEditor.EditorUtility.SetDirty">UnityEditor.EditorUtility.SetDirty</see>.</para>
        /// </remarks>
        /// <param name="name">Name of brush.</param>
        /// <param name="tileset">The tileset.</param>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <param name="procedural">Indicates if tileset brush should be procedural.</param>
        /// <returns>
        /// The brush; or <c>null</c> if an error has occurred.
        /// </returns>
        public static TilesetBrush CreateTilesetBrush(string name, Tileset tileset, int tileIndex, InheritYesNo procedural)
        {
            s_DirtyMesh = false;

            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("No name specified.");
                return null;
            }

            // Create new brush.
            var tilesetBrush = ScriptableObject.CreateInstance<TilesetBrush>();
            tilesetBrush.Initialize(tileset);
            tilesetBrush.name = name;
            tilesetBrush.tileIndex = tileIndex;
            tilesetBrush.procedural = procedural;

            tilesetBrush.forceLegacySideways = false;

            // Set collider type based upon editor behavior mode.
            switch (EditorSettings.defaultBehaviorMode) {
                case EditorBehaviorMode.Mode2D:
                    tilesetBrush.colliderType = ColliderType.BoxCollider2D;
                    break;
                case EditorBehaviorMode.Mode3D:
                    tilesetBrush.colliderType = ColliderType.BoxCollider3D;
                    break;
            }

            if (procedural == InheritYesNo.No || (procedural == InheritYesNo.Inherit && !tileset.procedural)) {
                if (EnsureTilesetMeshExists(tileset, tileIndex)) {
                    s_DirtyMesh = true;
                }
            }

            AssetDatabase.AddObjectToAsset(tilesetBrush, tileset);
            EditorUtility.SetDirty(tilesetBrush);
            EditorUtility.SetDirty(tileset);

            return tilesetBrush;
        }

        /// <summary>
        /// Delete brush asset.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// A value of <c>true</c> when brush was deleted; otherwise <c>false</c>.
        /// </returns>
        public static bool DeleteBrush(Brush brush)
        {
            if (brush == null) {
                return false;
            }

            var record = BrushDatabase.Instance.FindRecord(brush);
            if (record == null) {
                return false;
            }

            var descriptor = GetDescriptor(brush.GetType());
            if (descriptor == null) {
                return false;
            }

            // Attempt to delete brush.
            return descriptor.DeleteBrush(record);
        }

        private static bool IsDirectoryEmpty(string path)
        {
            // Directory is not empty because it does not exist.
            if (!Directory.Exists(path)) {
                return false;
            }

            string[] entries = Directory.GetFileSystemEntries(path);
            return !(entries != null && entries.Length > 0);
        }

        /// <summary>
        /// Delete tileset and associated assets.
        /// </summary>
        /// <remarks>
        /// <para>Can only delete tileset assets of type <see cref="Tileset"/>
        /// or <see cref="AutotileTileset"/>.</para>
        /// </remarks>
        /// <param name="tileset">The tileset.</param>
        /// <param name="flags">Optional deletion flags.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="tileset"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If attempting to delete an unsuported type of tileset.
        /// </exception>
        /// <exception cref="System.Exception">
        /// If tileset record was not found for specified tileset.
        /// </exception>
        public static void DeleteTileset(Tileset tileset, DeleteTilesetFlag flags = 0)
        {
            if (tileset == null) {
                throw new ArgumentNullException("tileset");
            }

            Type tilesetType = tileset.GetType();
            if (tilesetType != typeof(Tileset) && tilesetType != typeof(AutotileTileset)) {
                throw new ArgumentException("This utility function cannot delete tileset of type '" + tilesetType.FullName + "'", "tileset");
            }

            var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(tileset);
            if (tilesetRecord == null) {
                throw new Exception("Tileset record was not found for '" + tileset.name + "'");
            }

            var autotileTileset = tileset as AutotileTileset;

            string assetPath = tilesetRecord.AssetPath;
            string assetFolderPath = assetPath.Substring(0, assetPath.LastIndexOf("/"));
            string assetFolderPathBase = assetFolderPath + "/";
            int slashCount = assetFolderPathBase.CountSubstrings('/');

            // Note: Only delete assets where:
            //  - Asset path begins with base path of tileset asset.
            //  - Asset path includes the same number of slashes (to avoid deleting
            //    those that are nested within folders).

            // Delete associated texture?
            if (autotileTileset != null && (flags & DeleteTilesetFlag.DeleteTexture) == DeleteTilesetFlag.DeleteTexture && autotileTileset.AtlasTexture != null) {
                string t = AssetDatabase.GetAssetPath(tileset.AtlasTexture);
                if (t.StartsWith(assetFolderPathBase) && slashCount == t.CountSubstrings('/')) {
                    AssetDatabase.DeleteAsset(t);
                }
            }

            // Delete associated material?
            if ((flags & DeleteTilesetFlag.DeleteMaterial) == DeleteTilesetFlag.DeleteMaterial && tileset.AtlasMaterial != null) {
                string t = AssetDatabase.GetAssetPath(tileset.AtlasMaterial);
                if (t.StartsWith(assetFolderPathBase) && slashCount == t.CountSubstrings('/')) {
                    AssetDatabase.DeleteAsset(t);
                }
            }

            // Delete associated mesh assets?
            if ((flags & DeleteTilesetFlag.DeleteMeshAssets) == DeleteTilesetFlag.DeleteMeshAssets && tileset.tileMeshAsset != null) {
                string t = AssetDatabase.GetAssetPath(tileset.tileMeshAsset);
                if (t.StartsWith(assetFolderPathBase) && slashCount == t.CountSubstrings('/')) {
                    AssetDatabase.DeleteAsset(t);
                }
            }

            // Delete tileset and contained brush assets.
            AssetDatabase.DeleteAsset(assetPath);

            // Delete tileset folder if it is empty.
            if (IsDirectoryEmpty(Directory.GetCurrentDirectory() + "/" + assetFolderPath)) {
                AssetDatabase.DeleteAsset(assetFolderPath);
            }

            ToolUtility.RepaintBrushPalette();
            DesignerWindow.RepaintWindow();
        }

        internal static Material CreateTilesetMaterial(Texture2D atlasTexture, bool alpha)
        {
            Material template;
            string shaderName;

            if (alpha) {
                template = ProjectSettings.Instance.TransparentTilesetMaterialTemplate;
                shaderName = "Rotorz/Tileset/Transparent Unlit";
            }
            else {
                template = ProjectSettings.Instance.OpaqueTilesetMaterialTemplate;
                shaderName = "Rotorz/Tileset/Opaque Unlit";
            }

            var material = template != null
                ? (Material)Object.Instantiate(template)
                : new Material(Shader.Find(shaderName));

            material.mainTexture = atlasTexture;

            return material;
        }

        private static void CreateNonProceduralMeshAsset(Tileset tileset)
        {
            if (tileset.tileMeshAsset == null) {
                tileset.tileMeshAsset = ScriptableObject.CreateInstance<TilesetMeshAsset>();

                var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(tileset);

                // Create mesh asset if it does not already exist!
                string meshAssetPath = tilesetRecord.AssetPath.ReplaceLast(".asset", ".mesh.asset");
                AssetDatabase.CreateAsset(tileset.tileMeshAsset, meshAssetPath);
            }
        }

        /// <summary>
        /// Ensure that mesh exists for specified tile index.
        /// </summary>
        /// <remarks>
        /// <para>It is necessary to call <see cref="UnityEditor.AssetDatabase.ImportAsset(string)"/>
        /// for tileset asset if this function returns <c>true</c> (when all changes have
        /// been made to tileset).</para>
        /// </remarks>
        /// <param name="tileset">The tileset.</param>
        /// <param name="tileIndex">Zero-based index of tile in tileset.</param>
        /// <returns>
        /// A value of <c>true</c> if tile mesh was created; otherwise <c>false</c>.
        /// </returns>
        internal static bool EnsureTilesetMeshExists(Tileset tileset, int tileIndex)
        {
            // Mesh already exists, bail!
            if (tileset.GetTileMesh(tileIndex) != null) {
                return false;
            }

            if (tileset.tileMeshAsset == null) {
                CreateNonProceduralMeshAsset(tileset);
            }

            Mesh mesh = tileset.PrepareTileMesh(tileIndex);
            AssetDatabase.AddObjectToAsset(mesh, tileset.tileMeshAsset);

            EditorUtility.SetDirty(tileset);

            return true;
        }

        /// <summary>
        /// Cleanup tileset mesh assets that are not referenced by tileset brushes.
        /// </summary>
        /// <remarks>
        /// <para>Meshes will become missing in scenes and assets where tile meshes
        /// were previously used (i.e. where previously used tileset brushes have been
        /// deleted). Previously painted tiles should be refreshed.</para>
        /// </remarks>
        /// <param name="tileset">The tileset.</param>
        public static void CleanupTilesetMeshes(Tileset tileset)
        {
            if (tileset == null) {
                throw new ArgumentNullException("tileset");
            }

            // No mesh assets to cleanup!
            if (tileset.tileMeshAsset == null || tileset.tileMeshes == null) {
                return;
            }

            var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(tileset);
            if (tilesetRecord == null) {
                throw new Exception("Tileset record was not found for '" + tileset.name + "'");
            }

            int keep = 0, lastIndex = 0;
            for (int i = 0; i < tileset.tileMeshes.Length; ++i) {
                Mesh m = tileset.tileMeshes[i];
                if (m == null) {
                    continue;
                }

                // Find out if at least one tileset brush uses this mesh.
                foreach (BrushAssetRecord record in tilesetRecord.BrushRecords) {
                    TilesetBrush brush = record.Brush as TilesetBrush;

                    // Only consider non-procedural tileset brushes!
                    if (brush != null && brush.tileIndex == i && !brush.IsProcedural) {
                        // Do not delete mesh asset!
                        m = null;
                        ++keep;
                        break;
                    }
                }

                if (m != null) {
                    tileset.tileMeshes[i] = null;
                    EditorUtility.SetDirty(tileset);

                    Object.DestroyImmediate(m, true);
                }
                else {
                    // Index of last mesh that was kept.
                    lastIndex = i;
                }
            }

            // Were any mesh assets required?
            if (keep == 0) {
                // Remove asset altogether.
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tileset.tileMeshAsset));
                tileset.tileMeshAsset = null;
            }
            else {
                // Shrink tile mesh map if possible.
                Array.Resize(ref tileset.tileMeshes, lastIndex + 1);

                // Re-import mesh asset.
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tileset.tileMeshAsset));
            }

            DesignerWindow.RepaintWindow();
        }

        internal static bool WouldHaveImpossibleTilesetBrushes(Tileset tileset, int tileWidth, int tileHeight, int borderSize)
        {
            var atlasTexture = tileset.AtlasTexture;
            if (atlasTexture == null) {
                return false;
            }

            // Calculate the maximum number of tiles that could fit in tileset atlas.
            int outerTileWidth = tileWidth + borderSize * 2;
            int outerTileHeight = tileHeight + borderSize * 2;

            int rows = atlasTexture.height / outerTileHeight;
            int columns = atlasTexture.width / outerTileWidth;
            int tileCount = rows * columns;

            foreach (var record in BrushDatabase.Instance.GetTilesetBrushes(tileset)) {
                if (record.Brush.GetType() != typeof(TilesetBrush)) {
                    continue;
                }

                var tilesetBrush = record.Brush as TilesetBrush;
                if (tilesetBrush.tileIndex >= tileCount) {
                    return true;
                }
            }

            return false;
        }

        internal static bool HasImpossibleTilesetBrushes(Tileset tileset)
        {
            int tileCount = tileset.Rows * tileset.Columns;

            foreach (var record in BrushDatabase.Instance.GetTilesetBrushes(tileset)) {
                if (record.Brush.GetType() != typeof(TilesetBrush)) {
                    continue;
                }

                var tilesetBrush = record.Brush as TilesetBrush;
                if (tilesetBrush.tileIndex >= tileCount) {
                    return true;
                }
            }

            return false;
        }

        internal static void DeleteImpossibleTilesetBrushes(Tileset tileset)
        {
            if (!HasImpossibleTilesetBrushes(tileset)) {
                return;
            }

            int tileCount = tileset.Rows * tileset.Columns;
            int removedCount = 0;

            AssetDatabase.StartAssetEditing();

            foreach (var record in BrushDatabase.Instance.GetTilesetBrushes(tileset)) {
                if (record.Brush.GetType() != typeof(TilesetBrush)) {
                    continue;
                }

                var tilesetBrush = record.Brush as TilesetBrush;
                if (tilesetBrush.tileIndex < tileCount) {
                    continue;
                }

                DeleteBrush(tilesetBrush);
                ++removedCount;
            }

            // Ensure that changes are persisted immediately.
            AssetDatabase.StopAssetEditing();
            if (removedCount != 0) {
                AssetDatabase.SaveAssets();
            }
        }

        internal static void PrepareMissingNonProceduralMeshes(Tileset tileset)
        {
            bool dirty = false;

            foreach (var record in BrushDatabase.Instance.GetTilesetBrushes(tileset)) {
                if (record.Brush.GetType() != typeof(TilesetBrush)) {
                    continue;
                }

                var tilesetBrush = record.Brush as TilesetBrush;
                if (!tilesetBrush.IsProcedural) {
                    // Ensure that required procedural mesh exists!
                    if (EnsureTilesetMeshExists(tileset, tilesetBrush.tileIndex)) {
                        dirty = true;
                    }
                }
            }

            if (dirty) {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tileset.tileMeshAsset));
            }
        }

        internal static void RefreshNonProceduralMeshes(Tileset tileset)
        {
            bool dirty = false;

            var tilesetBrushes = BrushDatabase.Instance.GetTilesetBrushes(tileset);

            try {
                int tileCount = tileset.Rows * tileset.Columns;
                float ratio = 1f / (float)tileCount;

                for (int i = 0; i < tileCount; ++i) {
                    // Note: Also refresh prior non-procedural meshes (even though brush is now
                    //       procedural) to avoid breaking existing scenes and assets.
                    bool refresh = tileset.GetTileMesh(i) != null;

                    if (i % 10 == 0)
                        EditorUtility.DisplayProgressBar("Updating Tileset", "Refreshing non-procedural tile mesh assets...", (float)i * ratio);

                    // Find out if a non-procedural tile mesh must be created for this tile.
                    if (!refresh) {
                        foreach (var record in tilesetBrushes) {
                            if (record.Brush.GetType() != typeof(TilesetBrush)) {
                                continue;
                            }

                            var tilesetBrush = record.Brush as TilesetBrush;

                            if (!tilesetBrush.IsProcedural) {
                                refresh = true;
                                break;
                            }
                        }
                    }

                    // At least one brush requires this mesh?
                    if (refresh) {
                        // Find out if mesh asset will need to be saved.
                        bool saveAsset = tileset.GetTileMesh(i) == null;

                        if (tileset.tileMeshAsset == null) {
                            CreateNonProceduralMeshAsset(tileset);
                        }

                        // Ensure that tile mesh is refreshed.
                        Mesh mesh = tileset.RefreshTileMesh(i);
                        if (saveAsset) {
                            AssetDatabase.AddObjectToAsset(mesh, tileset.tileMeshAsset);
                        }

                        dirty = true;
                    }
                }

                if (dirty) {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tileset.tileMeshAsset));
                    EditorUtility.SetDirty(tileset);
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
