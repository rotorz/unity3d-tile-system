// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    internal sealed class SharedObjectFactoryContext : IObjectFactoryContext
    {
        private static SharedObjectFactoryContext s_Shared = new SharedObjectFactoryContext();


        public static IObjectFactoryContext GetShared(TileData tile, TileSystem system)
        {
            s_Shared.tileSystem = system;
            s_Shared.tile = tile;
            return s_Shared;
        }


        private TileSystem tileSystem;
        private TileData tile;


        TileSystem IObjectFactoryContext.TileSystem {
            get { return this.tileSystem; }
        }

        TileData IObjectFactoryContext.Tile {
            get { return this.tile; }
        }

        Brush IObjectFactoryContext.Brush {
            get { return this.tile != null ? this.tile.brush : null; }
        }
    }
}
