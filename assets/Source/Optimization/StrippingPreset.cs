// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Identifies desired level of stripping.
    /// </summary>
    public enum StrippingPreset
    {
        /// <summary>
        /// Strip tile data and other unrequired functionality (default).
        /// </summary>
        StripRuntime,

        /// <summary>
        /// Strip tile data and other unrequired functionality with the exception of the
        /// tile system component.
        /// </summary>
        KeepSystemComponent,

        /// <summary>
        /// Perform stripping but maintain access to tile data at runtime.
        /// </summary>
        RuntimeAccess,

        /// <summary>
        /// Perform some stripping but maintain painting capabilities.
        /// </summary>
        RuntimePainting,

        /// <summary>
        /// Maximum level of stripping.
        /// </summary>
        StripEverything,

        /// <summary>
        /// No stripping should occur.
        /// </summary>
        NoStripping,

        /// <summary>
        /// Select custom stripping options.
        /// </summary>
        Custom,
    }
}
