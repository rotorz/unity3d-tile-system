// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{

    /// <summary>
    /// Details of event that is being handled by a tool.
    /// </summary>
    /// <remarks>
    /// <para>For further information regarding tool events please refer to the following:</para>
    /// <list type="bullet">
    ///     <item><see cref="ToolBase.OnTool">ToolBase.OnTool</see></item>
    ///     <item><see cref="ToolBase.OnToolInactive">ToolBase.OnToolInactive</see></item>
    /// </list>
    /// </remarks>
    public sealed class ToolEvent
    {
        internal ToolEvent()
        {
        }


        /// <summary>
        /// Gets the type of event.
        /// </summary>
        public EventType Type { get; internal set; }

        /// <summary>
        /// Indicates when left button is pressed.
        /// </summary>
        public bool IsLeftButtonPressed { get; internal set; }

        /// <summary>
        /// Indicates when right button is pressed.
        /// </summary>
        public bool IsRightButtonPressed { get; internal set; }


        /// <summary>
        /// Indicates if left button was pressed before GUI event.
        /// </summary>
        public bool WasLeftButtonPressed { get; internal set; }

        /// <summary>
        /// Indicates if right button was pressed before GUI event.
        /// </summary>
        public bool WasRightButtonPressed { get; internal set; }


        /// <summary>
        /// Gets position of mouse pointer in screen coordinates.
        /// </summary>
        public Vector2 MousePointerScreenPoint { get; internal set; }

        /// <summary>
        /// Gets position of mouse pointer projected on tile system in local space of tile system.
        /// </summary>
        public Vector3 MousePointerLocalPoint { get; internal set; }


        /// <summary>
        /// Gets or sets index of active tile.
        /// </summary>
        public TileIndex MousePointerTileIndex { get; set; }

        /// <summary>
        /// Last known index of tile at mouse position.
        /// </summary>
        internal TileIndex LastMousePointerTileIndex;
    }
}
