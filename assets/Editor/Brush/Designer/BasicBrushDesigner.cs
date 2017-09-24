// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Basic brush designer is required by alias brush designer adaptor as fallback when
    /// no valid target brush is specified.
    /// </summary>
    /// <exclude/>
    internal sealed class BasicBrushDesigner : BrushDesignerView
    {
        /// <inheritdoc/>
        public override void OnGUI()
        {
            this.Section_MaterialMapper();
        }
    }
}
