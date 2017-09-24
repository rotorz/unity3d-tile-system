// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile
{
    /// <summary>
    /// Suggests whether colliders should be kept separate under certain scenarios.
    /// </summary>
    [Flags]
    public enum KeepSeparateColliderFlag
    {
        /// <summary>
        /// Colliders with unique tags should not be combined.
        /// </summary>
        /// <remarks>
        /// <para>Option is ignored for colliders that are automatically generated
        /// for tiles which have been flagged "Solid".</para>
        /// </remarks>
        ByTag = 0x01,

        /// <summary>
        /// Colliders with unique layers should not be combined.
        /// </summary>
        /// <remarks>
        /// <para>Option is ignored for colliders that are automatically generated
        /// for tiles which have been flagged "Solid".</para>
        /// </remarks>
        ByLayer = 0x02,
    }
}
