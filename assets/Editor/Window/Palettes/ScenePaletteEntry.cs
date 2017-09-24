// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine.SceneManagement;

namespace Rotorz.Tile.Editor
{
    internal struct ScenePaletteEntry
    {
        public static ScenePaletteEntry ForSceneHeader(Scene scene)
        {
            return new ScenePaletteEntry {
                IsHeader = true,
                Scene = scene,
                TileSystem = null
            };
        }

        public static ScenePaletteEntry ForTileSystem(TileSystem tileSystem)
        {
            return new ScenePaletteEntry {
                IsHeader = false,
                Scene = tileSystem.gameObject.scene,
                TileSystem = tileSystem
            };
        }


        public bool IsHeader;
        public Scene Scene;
        public TileSystem TileSystem;


        public int SceneOrder {
            get { return this.IsHeader ? int.MinValue : this.TileSystem.sceneOrder; }
        }
    }
}
