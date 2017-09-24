// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes context that tool is being used within.
    /// </summary>
    /// <seealso cref="ToolBase.OnTool">ToolBase.OnTool</seealso>
    /// <seealso cref="ToolBase.OnToolInactive">ToolBase.OnToolInactive</seealso>
    public interface IToolContext
    {
        /// <summary>
        /// Gets the tool manager.
        /// </summary>
        ToolManager ToolManager { get; }

        /// <summary>
        /// Gets the current tool.
        /// </summary>
        ToolBase Tool { get; }

        /// <summary>
        /// Gets the tile system.
        /// </summary>
        TileSystem TileSystem { get; }
    }
}
