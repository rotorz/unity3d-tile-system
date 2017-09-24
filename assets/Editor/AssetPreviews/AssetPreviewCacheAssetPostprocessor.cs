// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;

namespace Rotorz.Tile.Editor
{
    internal sealed class AssetPreviewCacheAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            ClearCachedAssetPreviews(importedAssets);
            ClearCachedAssetPreviews(deletedAssets);
        }

        private static void ClearCachedAssetPreviews(string[] assetPaths)
        {
            foreach (string assetPath in assetPaths) {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                AssetPreviewCache.ClearCachedAssetPreviewFile(guid);
            }
        }
    }
}
