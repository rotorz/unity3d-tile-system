// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings.Persisted;
using System;
using System.Collections.Generic;

namespace Rotorz.Settings
{
    /// <summary>
    /// Event arguments which are provided for <see cref="ValueChangedEventHandler{T}"/> events.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    public sealed class ValueChangedEventArgs<T>
    {
        /// <summary>
        /// Initializes new <see cref="ValueChangedEventArgs{T}"/> instance.
        /// </summary>
        /// <param name="newValue">New value.</param>
        /// <param name="previousValue">Previous value.</param>
        public ValueChangedEventArgs(T newValue, T previousValue)
        {
            this.PreviousValue = previousValue;
            this.NewValue = newValue;
        }


        /// <summary>
        /// Gets the new value.
        /// </summary>
        public T NewValue { get; private set; }
        /// <summary>
        /// Gets the previous value.
        /// </summary>
        public T PreviousValue { get; private set; }

    }


    /// <summary>
    /// Delegate which is invoked whenever value has been changed.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="args">Event arguments contains new and previous values.</param>
    public delegate void ValueChangedEventHandler<T>(ValueChangedEventArgs<T> args);


    /// <summary>
    /// Delegate which can be used to filter value before assignment.
    /// </summary>
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
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="value">Proposed value.</param>
    /// <returns>
    /// Filtered value.
    /// </returns>
    public delegate T FilterValue<T>(T value);


    /// <summary>
    /// Setting from configuration.
    /// </summary>
    /// <typeparam name="T">Type of value addressed by setting.</typeparam>
    /// <seealso cref="SettingManager"/>
    public class Setting<T> : ISetting
    {
        private DynamicSettingGroup _group;

        private T _value;
        internal readonly FilterValue<T> _filter;


        /// <summary>
        /// Initialize new <see cref="Setting{T}"/> instance.
        /// </summary>
        /// <param name="group">Group for which this setting belongs to.</param>
        /// <param name="key">Unique key used to identify this setting.</param>
        /// <param name="defaultValue">Default value of setting which can be restored.</param>
        /// <param name="filter">Optional delegate which can be used to filter values
        /// on assignment if specified. It is important to note that default values
        /// are also filtered on assignment.</param>
        internal Setting(DynamicSettingGroup group, string key, T defaultValue, FilterValue<T> filter)
        {
            this._group = group;

            this.Key = key;
            this.DefaultValue = defaultValue;

            this._filter = filter;
        }


        /// <inheritdoc/>
        public string GroupKey {
            get { return this._group.Key; }
        }

        /// <inheritdoc/>
        public string Key { get; private set; }

        /// <summary>
        /// Gets or sets value of setting.
        /// </summary>
        /// <remarks>
        /// <para>Filtering may be applied to value during assignment if filtering
        /// was specified at time of fetching setting from group.</para>
        /// <para>Setting is marked as dirty when value is changed which indicates
        /// that setting should be saved when synchronization next occurs.</para>
        /// <para>The <see cref="ValueChanged"/> event is raised upon changing value
        /// of setting to something different. Subscribers of this event are provided
        /// with both the new and previous values.</para>
        /// </remarks>
        public T Value {
            get { return this._value; }
            set {
                if (this._filter != null) {
                    value = this._filter(value);
                }

                if (!Equals(value, this._value)) {
                    T previousValue = this._value;

                    this._value = value;
                    this.MarkDirty();

                    if (this.ValueChanged != null) {
                        var args = new ValueChangedEventArgs<T>(value, previousValue);
                        this.ValueChanged(args);
                    }
                }
            }
        }


        /// <summary>
        /// Occurs whenever the <see cref="Value"/> property is changed providing
        /// access to the new and previous values. This might be used to repaint or
        /// update user interfaces to synchronize whenever value of setting changes.
        /// </summary>
        public event ValueChangedEventHandler<T> ValueChanged;


        internal bool IsValueChangedHandlerRegistered(ValueChangedEventHandler<T> handler)
        {
            Delegate proposedHandler = handler;
            foreach (Delegate existingHandler in this.ValueChanged.GetInvocationList()) {
                if (existingHandler == proposedHandler) {
                    return true;
                }
            }
            return false;
        }

        internal virtual bool Equals(T lhs, T rhs)
        {
            return EqualityComparer<T>.Default.Equals(lhs, rhs);
        }

        /// <summary>
        /// Gets default value of setting.
        /// </summary>
        /// <seealso cref="RestoreDefaultValue()"/>
        internal T DefaultValue { get; private set; }

        /// <inheritdoc/>
        public void RestoreDefaultValue()
        {
            this.Value = this.DefaultValue;
        }

        private bool _dirty;

        /// <summary>
        /// Gets a value indicating whether state of value object is dirty. This
        /// property only applies to reference types.
        /// </summary>
        internal virtual bool IsObjectStateDirty {
            get { return false; }
        }

        /// <inheritdoc/>
        public bool IsDirty {
            get { return this._dirty || this.IsObjectStateDirty; }
        }

        /// <summary>
        /// Manually mark setting as dirty.
        /// </summary>
        /// <remarks>
        /// <para>There is no need to manually invoke this method after changing
        /// <see cref="Value"/> property since this occurs automatically.</para>
        /// <para>This can be useful to indicate that a setting has been indirectly
        /// changed via some property if setting addresses a class type. Though it
        /// is better for such classes to implement <see cref="IDirtyableObject"/>
        /// so that this behaviour can be encapsulated.</para>
        /// </remarks>
        /// <inheritdoc/>
        public void MarkDirty()
        {
            this._dirty = true;
        }

        /// <inheritdoc/>
        public void DiscardChanges()
        {
            // No changes have been recorded, bail!
            if (!this.IsDirty) {
                return;
            }

            T previousValue = this._value;

            this._group.Manager.Adapter.LoadSetting(this);

            if (this.ValueChanged != null) {
                var args = new ValueChangedEventArgs<T>(this._value, previousValue);
                this.ValueChanged(args);
            }
        }

        /// <inheritdoc/>
        void ISetting.Serialize(ISettingSerializer serializer)
        {
            this._dirty = false;

            this.Serialize(serializer);
        }

        /// <inheritdoc/>
        void ISetting.Deserialize(ISettingSerializer serializer)
        {
            bool markClean = true;

            // Attempt to deserialize value, but resort to default value if
            // any exceptions are thrown whilst attempting to deserialize.

            T value;
            try {
                value = this.Deserialize(serializer);
            }
            catch (Exception ex) {
                this._group.Manager.LogFeedback(MessageFeedbackType.Error, string.Format("Was unable to read setting '{0}.{1}' so reverting to default value.", this.GroupKey, this.Key), ex);
                value = this.DefaultValue;

                // This change should be saved since input configuration file
                // contains an invalid value and user has been informed that the
                // default value has been restored.
                //
                // Maintaining the original value would allow the user to manually
                // correct the issue; but users who were happy for this setting to
                // be reverted would be re-warned each time they reopen Unity.

                markClean = false;
                this._dirty = true;
            }

            if (this._filter != null) {
                value = this._filter(value);
            }

            this._value = value;

            if (markClean) {
                this._dirty = false;

                if (!typeof(T).IsValueType) {
                    var dirtyableValue = value as IDirtyableObject;
                    if (dirtyableValue != null) {
                        dirtyableValue.MarkClean();
                    }
                }
            }
        }

        internal virtual void Serialize(ISettingSerializer serializer)
        {
            serializer.Serialize<T>(this, this.Value);
        }

        internal virtual T Deserialize(ISettingSerializer serializer)
        {
            return serializer.Deserialize<T>(this, this.DefaultValue);
        }

        /// <summary>
        /// Operator which implicitly casts setting to its contained value.
        /// </summary>
        /// <example>
        /// <para>Implicitly access value of setting:</para>
        /// <code language="csharp"><![CDATA[
        /// public Setting<int> FavouriteNumber { get; private set; }
        ///
        /// private void Foo()
        /// {
        ///     // The following:
        ///     int number = FavouriteNumber;
        ///     // is equivalent to:
        ///     int number = FavouriteNumber.Value;
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="setting">The setting.</param>
        /// <returns>
        /// Value of the specified setting.
        /// </returns>
        public static implicit operator T(Setting<T> setting)
        {
            return setting.Value;
        }
    }
}
