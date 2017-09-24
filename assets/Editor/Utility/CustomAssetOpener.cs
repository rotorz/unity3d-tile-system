// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEditor.Callbacks;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Listens for when the user double-clicks an asset file in Unity and responds by
    /// opening custom user interfaces for custom asset types.
    /// </summary>
    internal static class CustomAssetOpener
    {
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);

            var brush = asset as Brush;
            if (brush != null) {
                ToolUtility.ShowBrushInDesigner(brush);
                return true;
            }

            var tileset = asset as Tileset;
            if (tileset != null) {
                ToolUtility.ShowTilesetInDesigner(tileset);
                return true;
            }

            return false;
        }
    }
}
