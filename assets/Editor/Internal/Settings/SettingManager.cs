// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings.Persisted;
using System;
using System.Collections.Generic;

namespace Rotorz.Settings
{
    /// <summary>
    /// Setting manager provides provides methods for managing settings and provides
    /// access to setting groups.
    /// </summary>
    /// <remarks>
    /// <para>Exposure of setting manager should be kept to a minimum; for instance,
    /// instances of this class or its adapter should not be exposed within public APIs
    /// so that usage is properly encapsulated. At the very most only sealed <see cref="ISettingGroup"/>
    /// instances should be exposed; otherwise third-party code could incorrectly seal
    /// groups which are not supposed to be sealed.</para>
    /// </remarks>
    /// <example>
    /// <para>Example implementation of setting management:</para>
    /// <code language="csharp"><![CDATA[
    /// internal static class MyApplicationSettings
    /// {
    ///     private static JsonSettingAdapter s_JsonSettingAdapter;
    ///     private static SettingManager s_SettingManager;
    ///
    ///
    ///     static MyApplicationSettings()
    ///     {
    ///         // Initialize adapter for JSON configuration file which will be stored
    ///         // in operating system specific application data directory; for instance,
    ///         // on Windows this example would resolve to the path:
    ///         //
    ///         //    C:\Users\{Name}\AppData\Roaming\Example\My Application\Settings.json
    ///         //
    ///         s_JsonSettingAdapter = JsonSettingAdapter.FromApplicationDataPath("Example", "My Application");
    ///
    ///         // Attempt to parse JSON encoded configuration file. Simply resort to
    ///         // default configuration if a syntax error was encountered.
    ///         try {
    ///             s_JsonSettingAdapter.Load();
    ///         }
    ///         catch (Rotorz.Json.JsonParserException ex) {
    ///             // In a real application it might be wise to create a backup copy
    ///             // of the broken configuration file at this stage.
    ///
    ///             Console.WriteLine(ex.Message);
    ///             Console.WriteLine("Reverting to default configuration...");
    ///         }
    ///
    ///         // Create setting manager using our setting adapter from above.
    ///         s_SettingManager = new SettingManager(s_JsonSettingAdapter);
    ///
    ///         // Prepare application-wide settings.
    ///         GeneralSettings = PrepareSettings("General", PrepareGeneralSettings);
    ///         EditingSettings = PrepareSettings("Editing", PrepareEditingSettings);
    ///     }
    ///
    ///     public static IDynamicSettingGroup GetGroup(string key)
    ///     {
    ///         return s_SettingManager.GetGroup(key);
    ///     }
    ///
    ///     // At the very minimal this method should be invoked when user exits from
    ///     // application; though it might also be desirable to save settings at
    ///     // regular intervals to avoid data loss (i.e. if application crashes).
    ///     public static void SaveSettings()
    ///     {
    ///         s_SettingManager.Save();
    ///     }
    ///
    ///     #region Utility
    ///
    ///     // Utility method which prepares a group of settings and then seals group
    ///     // to prevent any further settings from being fetched.
    ///     public static ISettingGroup PrepareSettings(string key, Action<ISettingStore> preparer)
    ///     {
    ///         var settingGroup = GetGroup(key);
    ///         preparer(settingGroup);
    ///         settingGroup.Seal();
    ///         return settingGroup;
    ///     }
    ///
    ///     #endregion
    ///
    ///     public static ISettingGroup GeneralSettings { get; private set; }
    ///     public static ISettingGroup EditingSettings { get; private set; }
    ///
    ///     #region General Settings
    ///
    ///     private static void PrepareGeneralSettings(ISettingStore store)
    ///     {
    ///         DisplayExtraTips = store.Fetch<bool>("DisplayExtraTips", true);
    ///     }
    ///
    ///     public static Setting<bool> DisplayExtraTips { get; private set; }
    ///
    ///     #endregion
    ///
    ///     #region Editing Settings
    ///
    ///     private static void PrepareEditingSettings(ISettingStore store)
    ///     {
    ///         MaximumLineLength = store.Fetch<int>("MaximumLineLength", 0,
    ///             filter: (value) => Math.Max(0, value)
    ///         );
    ///         TrimTrailingSpaceWhenSaving = store.Fetch<bool>("TrimTrailingSpaceWhenSaving", false);
    ///     }
    ///
    ///     public static Setting<int> MaximumLineLength { get; private set; }
    ///     public static Setting<bool> TrimTrailingSpaceWhenSaving { get; private set; }
    ///
    ///     #endregion
    ///
    /// }
    /// ]]></code>
    /// <para>Elsewhere in the application it is then possible to access these settings:</para>
    /// <code language="csharp"><![CDATA[
    /// if (MyApplicationSettings.TrimTrailingSpaceWhenSaving)
    /// {
    ///     content = content.TrimEnd();
    /// }
    /// ]]></code>
    /// <para>Application settings can be reset per group:</para>
    /// <code language="csharp"><![CDATA[
    /// private void resetGeneralSettings_Click(object sender, EventArgs e)
    /// {
    ///     MyApplicationSettings.GeneralSettings.RestoreDefaultValues();
    /// }
    ///
    /// private void resetEditorSettings_Click(object sender, EventArgs e)
    /// {
    ///     MyApplicationSettings.EditingSettings.RestoreDefaultValues();
    /// }
    /// ]]></code>
    /// </example>
    internal sealed class SettingManager
    {
        /// <summary>
        /// Initialize new <see cref="SettingManager"/> instance.
        /// </summary>
        /// <param name="adapter">Adapter for persisted setting data source.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if adapter has already been bound to another setting manager.
        /// </exception>
        public SettingManager(PersistedSettingAdapter adapter)
        {
            if (adapter == null) {
                throw new ArgumentNullException("adapter");
            }

            adapter.BindToManager(this);
            this.Adapter = adapter;
        }

        /// <summary>
        /// Gets adapter for persisted setting data source.
        /// </summary>
        public PersistedSettingAdapter Adapter { get; private set; }

        private Dictionary<string, IDynamicSettingGroup> _groups = new Dictionary<string, IDynamicSettingGroup>();

        private IDynamicSettingGroup GetGroup(string key, bool create)
        {
            if (string.IsNullOrEmpty(key)) {
                throw new ArgumentException("Key must be a string with one or more characters.", "key");
            }
            if (key[0] == '{' || key[key.Length - 1] == '}') {
                throw new ArgumentException("Key must not start with '{' and must not end with '}'.", "key");
            }

            IDynamicSettingGroup group;
            if (!this._groups.TryGetValue(key, out group)) {
                if (create) {
                    group = new DynamicSettingGroup(key, this);
                    this._groups[key] = group;
                }
                else {
                    group = null;
                }
            }
            return group;
        }

        /// <summary>
        /// Get group with the specified key; group is created if it does not already exist.
        /// </summary>
        /// <param name="key">Group key.</param>
        /// <returns>
        /// The <see cref="IDynamicSettingGroup"/> instance.
        /// </returns>
        public IDynamicSettingGroup GetGroup(string key)
        {
            return this.GetGroup(key, true);
        }

        /// <summary>
        /// Get group with specified key;
        /// </summary>
        /// <param name="key">Group key.</param>
        /// <returns>
        /// The <see cref="IDynamicSettingGroup"/> instance; otherwise, a value of <c>null</c>
        /// if group has not been defined.
        /// </returns>
        internal IDynamicSettingGroup GetGroupIfExists(string key)
        {
            return this.GetGroup(key, false);
        }

        /// <summary>
        /// Reset all settings to their default values.
        /// </summary>
        public void RestoreDefaultValues()
        {
            foreach (var group in this._groups.Values) {
                group.RestoreDefaultValues();
            }
        }

        /// <summary>
        /// Discard any changes which have been made to settings.
        /// </summary>
        /// <remarks>
        /// <para>Setting values are be restored from persisted state if available;
        /// otherwise, default values are assumed.</para>
        /// </remarks>
        public void DiscardChanges()
        {
            foreach (var group in this._groups.Values) {
                group.DiscardChanges();
            }
        }

        /// <summary>
        /// Save changes which have been made to settings.
        /// </summary>
        /// <remarks>
        /// <para>Settings which have been marked as dirty will be synchronized using the
        /// <see cref="PersistedSettingAdapter"/> which is associated with this setting
        /// manager.</para>
        /// </remarks>
        public void Save()
        {
            this.Adapter.SaveDirtySettings();
        }

        /// <summary>
        /// Raised allowing application to log feedback messages.
        /// </summary>
        public event EventHandler<MessageFeedbackEventArgs> MessageFeedback;

        /// <summary>
        /// Log message for benefit of end user.
        /// </summary>
        /// <param name="type">Type of message.</param>
        /// <param name="message">Error message text.</param>
        /// <param name="exception">Associated exception when applicable; otherwise, a
        /// value of <c>null</c></param>
        public void LogFeedback(MessageFeedbackType type, string message, Exception exception)
        {
            if (this.MessageFeedback != null) {
                this.MessageFeedback(this, new MessageFeedbackEventArgs(type, message, exception));
            }
        }
    }
}
