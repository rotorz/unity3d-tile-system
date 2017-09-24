// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Settings.Persisted
{
    /// <summary>
    /// Interface for an object which can be used to serialize and/or deserialize
    /// the value of a specified setting.
    /// </summary>
    /// <exclude/>
    internal interface ISettingSerializer
    {
        /// <summary>
        /// Serialize setting value to persisted data store.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="setting">Setting which is being persisted.</param>
        /// <param name="value">Current value of setting.</param>
        void Serialize<T>(ISetting setting, T value);

        /// <summary>
        /// Deserialize setting value from persisted data store if possible; otherwise
        /// should simply return specified default value.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="setting">Setting which is being restored.</param>
        /// <param name="defaultValue">Default value of setting.</param>
        /// <returns>
        /// The deserialized value; otherwise, the default value.
        /// </returns>
        T Deserialize<T>(ISetting setting, T defaultValue);
    }
}
