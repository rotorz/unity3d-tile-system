// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using UnityEditor;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Asset processor detects new brush and tileset assets upon being imported to
    /// automatically update brush database.
    /// </summary>
    internal sealed class BrushDatabaseRescanProcessor : AssetPostprocessor
    {
        public static bool s_EnableScanner = false;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!s_EnableScanner) {
                return;
            }

            // Check for new brush/tileset assets.
            if (ContainsNewBrushOrTileset(importedAssets)) {
                BrushDatabase.Instance.Rescan();
                ToolUtility.RepaintBrushPalette();
            }

            // Check for deleted brush/tileset assets.
            BrushDatabase.Instance.ClearMissingRecords();
            ToolUtility.RepaintBrushPalette();
            DesignerWindow.RepaintWindow();
        }

        private static bool ContainsNewBrushOrTileset(string[] assets)
        {
            var interestingAssetGuids = new HashSet<string>(AssetDatabase.FindAssets("t:Rotorz.Tile.Brush t:Rotorz.Tile.Tileset"));

            foreach (string path in assets) {
                if (!interestingAssetGuids.Contains(AssetDatabase.AssetPathToGUID(path))) {
                    continue;
                }

                // Is this a new brush asset?
                var brush = AssetDatabase.LoadAssetAtPath(path, typeof(Brush)) as Brush;
                if (brush != null && BrushDatabase.Instance.FindRecord(brush) == null) {
                    return true;
                }

                // Is this a new tileset asset?
                var tileset = AssetDatabase.LoadAssetAtPath(path, typeof(Tileset)) as Tileset;
                if (tileset != null) {
                    var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(tileset);
                    if (tilesetRecord == null) {
                        return true;
                    }

                    // Perhaps brush was added or removed from tileset...
                    int brushCount = 0;
                    foreach (var tilesetAsset in AssetDatabase.LoadAllAssetsAtPath(tilesetRecord.AssetPath)) {
                        if (tilesetAsset is TilesetBrush) {
                            ++brushCount;
                        }
                    }
                    if (tilesetRecord.BrushRecords.Count != brushCount) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
