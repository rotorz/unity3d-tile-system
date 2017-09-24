// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Defines texture and hotspot for custom cursor.
    /// </summary>
    public struct CursorInfo
    {
        /// <summary>
        /// Type of mouse cursor.
        /// </summary>
        public MouseCursor Type;
        /// <summary>
        /// Cursor texture.
        /// </summary>
        public Texture2D Texture;
        /// <summary>
        /// Hotspot defines active point of cursor.
        /// </summary>
        public Vector2 Hotspot;


        /// <summary>
        /// Initialize new <see cref="CursorInfo"/>.
        /// </summary>
        /// <param name="texture">Cursor texture.</param>
        /// <param name="hotspot">Active point of cursor.</param>
        public CursorInfo(Texture2D texture, Vector2 hotspot)
        {
            this.Type = MouseCursor.CustomCursor;
            this.Texture = texture;
            this.Hotspot = hotspot;
        }

        /// <summary>
        /// Initialize new <see cref="CursorInfo"/>.
        /// </summary>
        /// <param name="texture">Cursor texture.</param>
        /// <param name="hotspotX">Active X point of cursor.</param>
        /// <param name="hotspotY">Active Y point of cursor.</param>
        public CursorInfo(Texture2D texture, float hotspotX, float hotspotY)
            : this(texture, new Vector2(hotspotX, hotspotY))
        {
        }
    }
}
