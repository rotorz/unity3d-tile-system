// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Settings
{
    /// <summary>
    /// Interface for a setting store.
    /// </summary>
    public interface ISettingStore
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
        /// <example>
        /// <para>Custom filter can be specified upon fetching a setting:</para>
        /// <code language="csharp"><![CDATA[
        /// public void PrepareSettings(ISettingStore store)
        /// {
        ///     SomeSetting = store.Fetch<int>("SomeSetting", 42,
        ///         filter: (value) => Math.Max(0, value)
        ///     );
        /// }
        /// ]]></code>
        /// </example>
        /// <typeparam name="T">Type of value which setting holds.</typeparam>
        /// <param name="key">Unique key for setting.</param>
        /// <param name="defaultValue">Default value of setting.</param>
        /// <param name="filter">An optional filter which is applied to filter
        /// setting value before assignment.</param>
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
        Setting<T> Fetch<T>(string key, T defaultValue, FilterValue<T> filter);
    }
}
