// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    internal class GenericBrushContext : IBrushContext
    {
        public TileSystem system;
        public int row;
        public int column;
        public Brush brush;


        #region IBrushContext Implementation

        /// <inheritdoc/>
        public TileSystem TileSystem {
            get { return this.system; }
        }

        /// <inheritdoc/>
        public int Row {
            get { return this.row; }
        }

        /// <inheritdoc/>
        public int Column {
            get { return this.column; }
        }

        /// <inheritdoc/>
        public Brush Brush {
            get { return this.brush; }
        }

        #endregion
    }
}
