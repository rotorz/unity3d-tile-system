// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Extension methods for <see cref="UnityEngine.Event"/>.
    /// </summary>
    internal static class EventExtensions
    {
        /// <summary>
        /// Determines whether current event is related to mouse input but filtered for a
        /// specific control.
        /// </summary>
        /// <param name="e">The event.</param>
        /// <param name="controlID">Unique identifier for control.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating whether mouse event is applicable to
        /// the specified control.
        /// </returns>
        public static bool IsMouseForControl(this Event e, int controlID)
        {
            var eventType = e.GetTypeForControl(controlID);
            return eventType == EventType.MouseDown
                || eventType == EventType.MouseUp
                || eventType == EventType.MouseMove
                || eventType == EventType.MouseDrag;
        }
    }
}
