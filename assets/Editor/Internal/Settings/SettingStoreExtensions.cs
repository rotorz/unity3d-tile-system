// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Settings
{
    /// <summary>
    /// Extension methods for <see cref="ISettingStore"/>.
    /// </summary>
    public static class SettingStoreExtensions
    {
        /// <summary>
        /// Fetch setting from store with the specified signature.
        /// </summary>
        /// <remarks>
        /// <para>Returns existing setting instance if setting has already been
        /// fetched from store; though an <c>InvalidOperationException</c> is
        /// thrown if attempting to fetch the same setting with a different
        /// signature. Consider using <see cref="ISettingGroup.Lookup{T}"/> instead.</para>
        /// </remarks>
        /// <typeparam name="T">Type of value which setting holds.</typeparam>
        /// <param name="store">Setting store.</param>
        /// <param name="key">Unique key for setting.</param>
        /// <param name="defaultValue">Default value of setting.</param>
        /// <returns>
        /// The <see cref="Setting{T}"/> instance.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// <para>Thrown if any of the following conditions are met:</para>
        /// <list type="bullet">
        /// <item>No further settings can be defined.</item>
        /// <item>Setting has already been defined with a different default value.</item>
        /// <item>Setting has already been defined with a different filter.</item>
        /// </list>
        /// </exception>
        public static Setting<T> Fetch<T>(this ISettingStore store, string key, T defaultValue)
        {
            return store.Fetch<T>(key, defaultValue, null);
        }
    }
}
