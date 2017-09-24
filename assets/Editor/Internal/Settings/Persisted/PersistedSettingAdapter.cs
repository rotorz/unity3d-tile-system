// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Settings.Persisted
{
    /// <summary>
    /// Base class for an adapter which allows a <see cref="SettingManager"/> to
    /// load, save and delete settings from a data store.
    /// </summary>
    internal abstract class PersistedSettingAdapter
    {
        /// <summary>
        /// Gets associated the setting manager.
        /// </summary>
        protected SettingManager Manager { get; private set; }


        internal void BindToManager(SettingManager manager)
        {
            if (this.Manager != null) {
                throw new InvalidOperationException("Adapter has already been bound to a setting manager.");
            }

            this.Manager = manager;

            this.OnBindToManager();
        }


        /// <summary>
        /// Occurs when adapter is bound to setting manager.
        /// </summary>
        protected virtual void OnBindToManager() { }

        /// <summary>
        /// Delete persisted state for all settings within a specific group.
        /// </summary>
        /// <remarks>
        /// <para>This method should only interact with persisted state within the
        /// associated data store and <strong>must not</strong> make any modifications
        /// to the associated <see cref="SettingManager"/>, <see cref="ISettingGroup"/>
        /// or <see cref="ISetting"/> instances.</para>
        /// </remarks>
        /// <param name="groupKey">Group key.</param>
        public abstract void DeleteAllSettingsInGroup(string groupKey);
        /// <summary>
        /// Delete persisted state for all settings in all groups.
        /// </summary>
        /// <remarks>
        /// <para>This method should only interact with persisted state within the
        /// associated data store and <strong>must not</strong> make any modifications
        /// to the associated <see cref="SettingManager"/>, <see cref="ISettingGroup"/>
        /// or <see cref="ISetting"/> instances.</para>
        /// </remarks>
        public abstract void DeleteAllSettings();

        /// <summary>
        /// Delete persisted state for all settings within a specific group which
        /// have not been fetched from the data store. This might be useful for
        /// truncating settings which have become obsolete.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="ISettingGroup"/> input includes a collection of
        /// settings which have been fetched from the data store. Persisted data for
        /// settings which are not included within this group are considered unreferenced
        /// and thus should be deleted.</para>
        /// <para>In a well designed application all required settings should have
        /// already been fetched for the specified <see cref="ISettingGroup"/> since
        /// sealed setting groups cannot fetch additional settings, and non-sealed
        /// setting groups should never be exposed in the public API.</para>
        /// <para>This method is used upon invoking <see cref="ISettingGroup.RestoreDefaultValues">ISettingGroup.RestoreDefaultValues</see>
        /// to provide consistent results for the end-user. For instance, if the end-user
        /// is using multiple versions or instances of the same application and triggers
        /// an action to "Reset Default Settings"; it would be inconsistent if only some
        /// settings had been reset when opening a different version or instance of the
        /// same application.</para>
        /// <para>This method should only interact with persisted state within the
        /// associated data store and <strong>must not</strong> make any modifications
        /// to the associated <see cref="SettingManager"/>, <see cref="ISettingGroup"/>
        /// or <see cref="ISetting"/> instances.</para>
        /// </remarks>
        /// <param name="group">The setting group.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="group"/> has a value of <c>null</c>.
        /// </exception>
        public abstract void DeleteUnreferencedSettings(ISettingGroup group);

        /// <summary>
        /// Get serializer for the specified setting.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <returns>
        /// Must return av alid <see cref="ISettingSerializer"/> instance. Returning
        /// a value of <c>null</c> will lead to a <see cref="System.NullReferenceException"/>.
        /// </returns>
        protected abstract ISettingSerializer GetSettingSerializer(ISetting setting);

        /// <summary>
        /// Load persisted data for the specified setting.
        /// </summary>
        /// <remarks>
        /// <para>Default value should be assumed if errors occur whilst attempting
        /// to deserialize persisted data to avoid introducing unusual problems
        /// within application.</para>
        /// </remarks>
        /// <param name="setting">The setting.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="setting"/> has a value of <c>null</c>.
        /// </exception>
        public void LoadSetting(ISetting setting)
        {
            if (setting == null) {
                throw new ArgumentNullException("setting");
            }

            // Attempt to deserialize value, but resort to default value if
            // any exceptions are thrown whilst attempting to deserialize.

            try {
                var serializer = this.GetSettingSerializer(setting);
                if (serializer == null) {
                    throw new NullReferenceException();
                }

                setting.Deserialize(serializer);
            }
            catch (Exception ex) {
                this.Manager.LogFeedback(MessageFeedbackType.Warning, string.Format("Was unable to read setting '{0}.{1}' so reverting to default value.\nSee editor log for further details.", setting.GroupKey, setting.Key), ex);

                // Whilst the following will cause `ValueChanged` event to occur;
                // in most cases there will be no subscribers of `ValueChanged` at
                // this stage since setting is probably being fetched.
                setting.RestoreDefaultValue();
            }
        }

        /// <summary>
        /// Save all settings which have been marked as dirty.
        /// </summary>
        /// <remarks>
        /// <para>Settings are typically only marked dirty if their value has
        /// been modified in some way. Settings which have been serialized or
        /// deserialized are no longer marked as dirty to avoid unnecessary
        /// updating of persisted setting state.</para>
        /// </remarks>
        /// <seealso cref="ISetting.IsDirty"/>
        public abstract void SaveDirtySettings();
    }
}
