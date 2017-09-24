// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Settings
{
    /// <summary>
    /// Interface for a dynamic group of settings which can be extended until
    /// sealed by fetching additional settings.
    /// </summary>
    /// <remarks>
    /// <para>An <c>System.InvalidOperationException</c> is thrown when attempting
    /// to fetch settings from a sealed group.</para>
    /// </remarks>
    internal interface IDynamicSettingGroup : ISettingGroup, ISettingStore
    {
        /// <summary>
        /// Gets a value indicating whether group has been sealed.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Seal group so that no further settings can be fetched.
        /// </summary>
        void Seal();
    }
}
