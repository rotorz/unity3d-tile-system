// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <exclude/>
    [InitializeOnLoad]
    public static class AssetPreviewCache
    {
        private const string LibraryRelativePreviewCacheFolderPath = "Library/Rotorz/PreviewCache";
        private const double PreviewLifetimeInSeconds = 20.0;


        private static double s_LastCheckExpiredPreviewTime;

        private static Dictionary<string, PreviewInfo> s_LoadedAssetPreviews = new Dictionary<string, PreviewInfo>();
        private static object s_Lock = new object();


        static AssetPreviewCache()
        {
            EditorApplication.update += EditorApplication_Update;
        }


        private static void EditorApplication_Update()
        {
            if (EditorApplication.timeSinceStartup - s_LastCheckExpiredPreviewTime < PreviewLifetimeInSeconds) {
                return;
            }

            //Debug.Log("Checking for expired previews");
            if (s_LoadedAssetPreviews.Values.Any(x => x.HasExpired)) {
                //Debug.Log("Found one or more expired previews!");
                UnloadExpiredAssetPreviews();
            }

            s_LastCheckExpiredPreviewTime = EditorApplication.timeSinceStartup;
        }


        private struct PreviewInfo
        {
            public Texture2D PreviewTexture;
            public double ExpireTime;


            public bool HasBeenDestroyed {
                get { return !ReferenceEquals(this.PreviewTexture, null) && this.PreviewTexture == null; }
            }

            public bool HasExpired {
                get { return (this.ExpireTime < EditorApplication.timeSinceStartup && !ReferenceEquals(this.PreviewTexture, null)) || this.HasBeenDestroyed; }
            }
        }


        public static Texture2D GetAssetPreview(Object targetObject)
        {
            // No target object means no preview :)
            if (targetObject == null) {
                return null;
            }

            string assetPath = AssetDatabase.GetAssetPath(targetObject);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string cacheFilePath = GetAssetPreviewCacheFilePath(targetObject);

            if (File.Exists(cacheFilePath)) {
                DateTime cacheFileTime = File.GetLastWriteTime(cacheFilePath);
                DateTime assetFileTime = File.GetLastWriteTime(assetPath);
                if (cacheFileTime < assetFileTime) {
                    ClearCachedAssetPreviewFile(guid);
                }
            }

            var previewInfo = new PreviewInfo();
            try {
                if (TryGetInMemoryPreview(guid, out previewInfo) && !previewInfo.HasBeenDestroyed) {
                    // Already loaded asset preview into memory; let's use that!
                    return previewInfo.PreviewTexture;
                }

                // Attempt to load asset preview from cache file inside "Library" folder.
                //Debug.Log(string.Format("Loading preview for asset '{0}' ({1}).", targetObject.name, guid));
                if (File.Exists(cacheFilePath)) {
                    previewInfo.PreviewTexture = InternalEditorUtility.LoadSerializedFileAndForget(cacheFilePath).OfType<Texture2D>().FirstOrDefault();
                    return previewInfo.PreviewTexture;
                }

                // Attempt to generate asset preview.
                //Debug.Log(string.Format("Generating preview for asset '{0}' ({1}).", targetObject.name, guid));
                var generatedPreviewTexture = GenerateAssetPreview(targetObject);
                if (generatedPreviewTexture != null) {
                    //Debug.Log(string.Format("Saving preview for asset '{0}' ({1}).", targetObject.name, guid));
                    EnsurePathExists(cacheFilePath);
                    InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { generatedPreviewTexture }, cacheFilePath, false);
                }

                previewInfo.PreviewTexture = generatedPreviewTexture;
                return previewInfo.PreviewTexture;
            }
            finally {
                if (previewInfo.PreviewTexture != null) {
                    previewInfo.PreviewTexture.hideFlags = HideFlags.HideAndDontSave;
                }

                previewInfo.ExpireTime = EditorApplication.timeSinceStartup + PreviewLifetimeInSeconds;
                SetInMemoryPreview(guid, previewInfo);
            }
        }

        private static void UnloadExpiredAssetPreviews()
        {
            foreach (var entry in s_LoadedAssetPreviews.ToArray()) {
                if (entry.Value.HasExpired) {
                    UnloadAssetPreview(entry.Key);
                    //Debug.Log(string.Format("Preview expired ({0}).", entry.Key));
                }
            }
        }

        public static void UnloadAllAssetPreviews()
        {
            foreach (string guid in s_LoadedAssetPreviews.Keys.ToArray()) {
                UnloadAssetPreview(guid);
            }
        }

        public static void UnloadAssetPreview(string guid)
        {
            PreviewInfo previewInfo;
            if (TryGetInMemoryPreview(guid, out previewInfo)) {
                if (previewInfo.PreviewTexture != null) {
                    Object.DestroyImmediate(previewInfo.PreviewTexture);
                }

                lock (s_Lock) {
                    s_LoadedAssetPreviews.Remove(guid);
                }
            }
        }

        public static void ClearAllCachedAssetPreviewFiles()
        {
            // Clear preview cache files.
            if (Directory.Exists(LibraryRelativePreviewCacheFolderPath)) {
                try {
                    foreach (string fileName in Directory.GetFiles(LibraryRelativePreviewCacheFolderPath)) {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            // Unload all asset previews from memory.
            UnloadAllAssetPreviews();
        }

        public static void ClearCachedAssetPreviewFile(string guid)
        {
            // Clear preview cache file.
            string cacheFilePath = GetAssetPreviewCacheFilePath(guid);
            try {
                if (File.Exists(cacheFilePath)) {
                    File.Delete(cacheFilePath);
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }

            // Unload asset preview from memory.
            UnloadAssetPreview(guid);
        }

        public static void ClearCachedAssetPreviewFile(Object obj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            ClearCachedAssetPreviewFile(guid);
        }

        private static bool TryGetInMemoryPreview(string guid, out PreviewInfo info)
        {
            lock (s_Lock) {
                return s_LoadedAssetPreviews.TryGetValue(guid, out info);
            }
        }

        private static void SetInMemoryPreview(string guid, PreviewInfo info)
        {
            lock (s_Lock) {
                s_LoadedAssetPreviews[guid] = info;
            }
        }

        private static void EnsurePathExists(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName)) {
                Directory.CreateDirectory(directoryName);
            }
        }

        private static string GetAssetPreviewCacheFilePath(string guid)
        {
            return Path.Combine(LibraryRelativePreviewCacheFolderPath, guid + ".asset");
        }

        private static string GetAssetPreviewCacheFilePath(Object targetObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(targetObject);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            return GetAssetPreviewCacheFilePath(guid);
        }

        private static Texture2D GenerateAssetPreview(Object targetObject)
        {
            var editor = UnityEditor.Editor.CreateEditor(targetObject);
            if (editor != null) {
                var restoreAmbientLight = RenderSettings.ambientLight;
                var restoreAmbientMode = RenderSettings.ambientMode;
                var restoreAmbientIntensity = RenderSettings.ambientIntensity;
                var restoreSkybox = RenderSettings.skybox;
                try {
                    RenderSettings.ambientLight = new Color(0.212f, 0.227f, 0.259f, 1f);
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                    RenderSettings.ambientIntensity = 2f;
                    // I would rather use the default skybox here; but for some reason it doesn't work :(
                    RenderSettings.skybox = null;//AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
                    string assetPath = AssetDatabase.GetAssetPath(targetObject);
                    Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    return editor.RenderStaticPreview(assetPath, subAssets, 256, 256);
                }
                finally {
                    RenderSettings.ambientLight = restoreAmbientLight;
                    RenderSettings.ambientMode = restoreAmbientMode;
                    RenderSettings.ambientIntensity = restoreAmbientIntensity;
                    RenderSettings.skybox = restoreSkybox;
                    Object.DestroyImmediate(editor);
                }
            }
            return null;
        }
    }
}
