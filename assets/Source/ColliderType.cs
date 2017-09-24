// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Identifies a type of collider.
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// 2D version of box collider (see <see cref="UnityEngine.BoxCollider2D"/>).
        /// </summary>
        BoxCollider2D = 0,

        /// <summary>
        /// 3D version of box collider (see <see cref="UnityEngine.BoxCollider"/>).
        /// </summary>
        BoxCollider3D,
    }
}
