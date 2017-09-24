// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings.Persisted;

namespace Rotorz.Settings
{
    /// <summary>
    /// Interface for an individual setting.
    /// </summary>
    internal interface ISetting
    {
        /// <summary>
        /// Gets unique key identifying group that contains this setting.
        /// </summary>
        string GroupKey { get; }
        /// <summary>
        /// Gets unique key identifying this setting within its group.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Restore setting to its default value.
        /// </summary>
        void RestoreDefaultValue();

        /// <summary>
        /// Discard any changes which have been made to this setting.
        /// </summary>
        /// <remarks>
        /// <para>Attempts to reload setting value from persisted data store;
        /// otherwise reverts to default value.</para>
        /// </remarks>
        void DiscardChanges();

        /// <summary>
        /// Gets a value indicating whether this setting has become dirty.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Manually mark setting as dirty.
        /// </summary>
        /// <remarks>
        /// <para>This can be useful to indicate that a setting has been indirectly
        /// changed via some property if setting addresses a class type. Though it
        /// is better for such classes to implement <see cref="IDirtyableObject"/>
        /// so that this behaviour can be encapsulated.</para>
        /// </remarks>
        void MarkDirty();

        /// <summary>
        /// Serialize current value of this setting.
        /// </summary>
        /// <remarks>
        /// <para>This method should pass current value of setting to the provided
        /// serializer like shown in the following example:</para>
        /// <code language="csharp"><![CDATA[
        /// private int _value;
        ///
        /// private void ISetting.Serialize(ISettingSerializer serializer)
        /// {
        ///     _dirty = false;
        ///     serializer.Serialize(this, _value);
        /// }
        /// ]]></code>
        /// </remarks>
        /// <param name="serializer">Setting serializer.</param>
        void Serialize(ISettingSerializer serializer);

        /// <summary>
        /// Deserialize persisted value of this setting.
        /// </summary>
        /// <remarks>
        /// <para>This method should assume deserialized value of setting using the
        /// provide serializer like shown in the following example:</para>
        /// <code language="csharp"><![CDATA[
        /// public int DefaultValue { get; private set; }
        ///
        /// private int _value;
        ///
        /// private void ISetting.Deserialize(ISettingSerializer serializer)
        /// {
        ///     _dirty = false;
        ///     _value = serializer.Deserialize(this, DefaultValue);
        /// }
        /// ]]></code>
        /// </remarks>
        /// <param name="serializer">Setting serializer.</param>
        void Deserialize(ISettingSerializer serializer);
    }
}
