// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Can be extended to provide a custom immediate preview implementation which
    /// can then be attached to the root-most component of a tile prefab to override
    /// immediate preview of oriented brushes.
    /// </summary>
    /// <remarks>
    /// <para>Immediate preview should be drawn using the <a href="http://docs.unity3d.com/ScriptReference/Graphics.html"><c>Graphics</c></a>
    /// class from the <c>UnityEngine</c> API.</para>
    /// </remarks>
    [AddComponentMenu("")]
    public abstract class CustomImmediatePreview : MonoBehaviour
    {
        /// <summary>
        /// Method is invoked to draw immediate preview.
        /// </summary>
        /// <remarks>
        /// <para>Immediate preview should be drawn using the <a href="http://docs.unity3d.com/ScriptReference/Graphics.html"><c>Graphics</c></a>
        /// class from the <c>UnityEngine</c> API.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being previewed.</param>
        /// <param name="previewTile">Data for preview tile.</param>
        /// <param name="previewMaterial">Material to use for preview.</param>
        /// <param name="matrix">Matrix which describes transformation of tile.</param>
        /// <returns>
        /// A value of <c>true</c> if custom immediate preview was drawn; otherwise a value
        /// of <c>false</c> indicating that default implementation should be assumed.
        /// </returns>
        public abstract bool DrawImmediatePreview(IBrushContext context, TileData previewTile, Material previewMaterial, Matrix4x4 matrix);
    }
}
