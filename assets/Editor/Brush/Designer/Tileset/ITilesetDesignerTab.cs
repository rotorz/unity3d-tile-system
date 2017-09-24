// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor.Internal
{
    internal interface ITilesetDesignerTab
    {
        string Label { get; }

        Vector2 ScrollPosition { get; set; }

        void OnNewTilesetRecord(TilesetAssetRecord record);

        void OnEnable();
        void OnDisable();

        void OnFixedHeaderGUI();
        void OnGUI();

        void OnSideGUI();
    }
}
