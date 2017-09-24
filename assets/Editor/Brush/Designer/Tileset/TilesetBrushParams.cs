// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor.Internal
{
    internal struct TilesetBrushParams
    {
        public bool IsSelected;
        public string Name;
        public int Count;
        public string ErrorMessage;


        public bool HasErrorMessage {
            get { return !string.IsNullOrEmpty(this.ErrorMessage); }
        }
    }
}
