// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;

namespace Rotorz.Settings
{
    /// <summary>
    /// Interface for a group of settings.
    /// </summary>
    internal interface ISettingGroup
    {
        /// <summary>
        /// Gets unique key which identifies this group.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Lookup setting by key and cast to specified type.
        /// </summary>
        /// <remarks>
        /// <para>Whilst each setting within group has a unique key, setting will only
        /// be found if its underlying value type matches <typeparamref name="T"/>.
        /// Use <see cref="IsDefined(string)"/> to determine whether a setting has been
        /// defined regardless of its underlying value type.</para>
        /// </remarks>
        /// <typeparam name="T">Value type of setting.</typeparam>
        /// <param name="key">Key of setting.</param>
        /// <returns>
        /// The <see cref="Setting{T}"/> if found; otherwise, a value of <c>null</c>.
        /// </returns>
        Setting<T> Lookup<T>(string key);

        /// <summary>
        /// Determines whether setting is defined with specified key.
        /// </summary>
        /// <param name="key">Key of setting.</param>
        /// <returns>
        /// A value of <c>true</c> if setting is defined; otherwise, a value of <c>false</c>.
        /// </returns>
        bool IsDefined(string key);

        /// <summary>
        /// Gets enumerable collection of settings.
        /// </summary>
        IEnumerable<ISetting> Settings { get; }

        /// <summary>
        /// Restore all settings within this group to their default values.
        /// </summary>
        void RestoreDefaultValues();

        /// <summary>
        /// Discard any changes which have been to settings within this group.
        /// </summary>
        void DiscardChanges();
    }
}
