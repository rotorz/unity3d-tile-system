// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Specifies how nozzle indicator should be presented when interacting
    /// with tiles using the editor.
    /// </summary>
    /// <remarks>
    /// <para><img src="../art/nozzle-indicators.png" alt="Illustration of flat and wireframe nozzle indicators."/></para>
    /// </remarks>
    public enum NozzleIndicator
    {
        /// <summary>
        /// Chooses indicator mode based upon selected brush.
        /// </summary>
        /// <remarks>
        /// <para>Note: Custom brushes can customize this functionality by overriding
        /// <see cref="M:Rotorz.Tile.Brush.UseWireCursorInEditor"/>.</para>
        /// </remarks>
        Automatic = 0,

        /// <summary>
        /// Wireframe representation.
        /// </summary>
        Wireframe,

        /// <summary>
        /// Flat representation.
        /// </summary>
        Flat,
    }
}
