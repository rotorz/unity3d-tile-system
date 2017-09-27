// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Json;
using System;
using System.IO;

namespace Rotorz.Settings.Persisted.Json
{
    /// <summary>
    /// Class which adapts JSON encoded configuration files to work with a <see cref="SettingManager"/>
    /// instance by implementing <see cref="PersistedSettingAdapter"/>.
    /// </summary>
    internal sealed class JsonSettingAdapter : PersistedSettingAdapter
    {
        /// <summary>
        /// Default relative path to assume if not specified when creating a <see cref="JsonSettingAdapter"/>
        /// using <see cref="JsonSettingAdapter.FromApplicationDataPath(string, string)"/>.
        /// By default the relative path "Settings.json" is assumed.
        /// </summary>
        public const string DefaultRelativePath = "Settings.json";


        #region Factory

        /// <summary>
        /// Create <see cref="JsonSettingAdapter"/> for specified path.
        /// </summary>
        /// <param name="path">Absolute path of configuration file on file system.</param>
        /// <returns>
        /// The new <see cref="JsonSettingAdapter"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="path"/> has a value of <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="path"/> is just an empty string.
        /// </exception>
        /// <seealso cref="FromApplicationDataPath(string, string, string)"/>
        /// <seealso cref="FromApplicationDataPath(string, string)"/>
        public static JsonSettingAdapter FromPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path == "")
                throw new ArgumentException("Invalid path was specified.", "path");

            var adapter = new JsonSettingAdapter();
            adapter.Path = path;
            return adapter;
        }

        /// <summary>
        /// Create <see cref="JsonSettingAdapter"/> for the specified special path.
        /// </summary>
        /// <param name="specialFolder">Special path on operating system.</param>
        /// <param name="vendorName">Unique string which identifies vendor (i.e. "Rotorz").</param>
        /// <param name="applicationName">Unique string which identifies vendor
        /// application (i.e. "unity3d-tile-system").</param>
        /// <param name="relativePath">Relative path of configuration file (i.e. "Settings.json").
        /// This path can include additional sub directories.</param>
        /// <returns>
        /// The new <see cref="JsonSettingAdapter"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="vendorName"/>, <paramref name="applicationName"/> or
        /// <paramref name="relativePath"/> has a value of <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="vendorName"/>, <paramref name="applicationName"/> or
        /// <paramref name="relativePath"/> is just an empty string.
        /// </exception>
        /// <seealso cref="FromPath(string)"/>
        /// <seealso cref="FromApplicationDataPath(string, string, string)"/>
        /// <seealso cref="FromApplicationDataPath(string, string)"/>
        private static JsonSettingAdapter FromSpecialPath(Environment.SpecialFolder specialFolder, string vendorName, string applicationName, string relativePath)
        {
            if (vendorName == null)
                throw new ArgumentNullException("vendorName");
            if (applicationName == null)
                throw new ArgumentNullException("applicationName");
            if (relativePath == null)
                throw new ArgumentNullException("relativePath");

            if (vendorName == "")
                throw new ArgumentException("vendorName");
            if (applicationName == "")
                throw new ArgumentException("applicationName");
            if (relativePath == "")
                throw new ArgumentException("relativePath");

            return FromPath(PathUtility.Combine(
                Environment.GetFolderPath(specialFolder),
                vendorName,
                applicationName,
                relativePath
            ));
        }

        /// <summary>
        /// Create <see cref="JsonSettingAdapter"/> for vendor application path within
        /// user application data directory.
        /// </summary>
        /// <remarks>
        /// <para>The destination directory is resolved using <c>Environment.SpecialFolder.ApplicationData</c>
        /// which varies by operating system.</para>
        /// <para>For instance, the following example:</para>
        /// <code language="csharp"><![CDATA[
        /// var adapter = JsonSettingAdapter.FromApplicationDataPath(
        ///     "Rotorz", "unity3d-tile-system", "Settings.json"
        /// );
        /// ]]></code>
        /// <para>produces the paths:</para>
        /// <list type="bullet">
        /// <item><b>Windows:</b> C:\Users\{UserName}\AppData\Roaming\Rotorz\unity3d-tile-system\Settings.json</item>
        /// <item><b>OS X:</b> /Users/macuser/.config/Rotorz/unity3d-tile-system/Settings.json</item>
        /// </list>
        /// </remarks>
        /// <param name="vendorName">Unique string which identifies vendor (i.e. "Rotorz").</param>
        /// <param name="applicationName">Unique string which identifies vendor
        /// application (i.e. "unity3d-tile-system").</param>
        /// <param name="relativePath">Relative path of configuration file (i.e. "Settings.json").
        /// This path can include additional sub directories.</param>
        /// <returns>
        /// The new <see cref="JsonSettingAdapter"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="vendorName"/>, <paramref name="applicationName"/> or
        /// <paramref name="relativePath"/> has a value of <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="vendorName"/>, <paramref name="applicationName"/> or
        /// <paramref name="relativePath"/> is just an empty string.
        /// </exception>
        /// <seealso cref="FromPath(string)"/>
        /// <seealso cref="FromApplicationDataPath(string, string)"/>
        public static JsonSettingAdapter FromApplicationDataPath(string vendorName, string applicationName, string relativePath)
        {
            return FromSpecialPath(Environment.SpecialFolder.ApplicationData, vendorName, applicationName, DefaultRelativePath);
        }

        /// <summary>
        /// Create <see cref="JsonSettingAdapter"/> for vendor application path within
        /// user application data directory but assume "Settings.json" file.
        /// </summary>
        /// <remarks>
        /// <para>The destination directory is resolved using <c>Environment.SpecialFolder.ApplicationData</c>
        /// which varies by operating system.</para>
        /// <para>For instance, the following example:</para>
        /// <code language="csharp"><![CDATA[
        /// var adapter = JsonSettingAdapter.FromApplicationDataPath(
        ///     "Rotorz", "unity3d-tile-system"
        /// );
        /// ]]></code>
        /// <para>produces the paths (where "Settings.json" is derived from <see cref="DefaultRelativePath"/>):</para>
        /// <list type="bullet">
        /// <item><b>Windows:</b> C:\Users\{UserName}\AppData\Roaming\Rotorz\unity3d-tile-system\Settings.json</item>
        /// <item><b>OS X:</b> /Users/macuser/.config/Rotorz/unity3d-tile-system/Settings.json</item>
        /// </list>
        /// </remarks>
        /// <param name="vendorName">Unique string which identifies vendor (i.e. "Rotorz").</param>
        /// <param name="applicationName">Unique string which identifies vendor
        /// application (i.e. "unity3d-tile-system").</param>
        /// <returns>
        /// The new <see cref="JsonSettingAdapter"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="vendorName"/> or <paramref name="applicationName"/>
        /// has a value of <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="vendorName"/> or <paramref name="applicationName"/>
        /// is just an empty string.
        /// </exception>
        /// <seealso cref="FromPath(string)"/>
        /// <seealso cref="FromApplicationDataPath(string, string, string)"/>
        public static JsonSettingAdapter FromApplicationDataPath(string vendorName, string applicationName)
        {
            return FromApplicationDataPath(vendorName, applicationName, DefaultRelativePath);
        }

        #endregion


        private JsonSettingData _data = new JsonSettingData();


        /// <summary>
        /// Initialize new <see cref="JsonSettingAdapter"/> instance.
        /// </summary>
        private JsonSettingAdapter()
        {
        }


        /// <inheritdoc/>
        protected override void OnBindToManager()
        {
            this._data.MessageFeedback += (sender, args) => {
                Manager.LogFeedback(args.FeedbackType, args.Message, args.Exception);
            };
        }

        /// <summary>
        /// Gets or sets absolute path of configuration on file system.
        /// </summary>
        /// <remarks>
        /// <para>This path can be customized prior to invoking <see cref="Load()"/> or
        /// <see cref="SaveDirtySettings()"/> allowing for a sort of
        /// "Save As..." feature for exporting and importing user configurations.</para>
        /// </remarks>
        /// <seealso cref="Load()"/>
        /// <seealso cref="SaveDirtySettings()"/>
        public string Path { get; set; }


        /// <inheritdoc/>
        public override void DeleteAllSettingsInGroup(string groupKey)
        {
            var groupData = this._data.GetGroupData(groupKey);
            if (groupData != null) {
                groupData.DeleteAllSettings();
            }
        }

        /// <inheritdoc/>
        public override void DeleteAllSettings()
        {
            foreach (string groupKey in this._data.GroupKeys) {
                this._data.GetGroupData(groupKey).DeleteAllSettings();
            }
        }

        /// <inheritdoc/>
        public override void DeleteUnreferencedSettings(ISettingGroup group)
        {
            if (group == null) {
                throw new ArgumentNullException("group");
            }

            var groupData = this._data.GetGroupData(group.Key);
            if (groupData != null) {
                groupData.DeleteUnreferencedSettings(group);
            }
        }

        /// <inheritdoc/>
        protected override ISettingSerializer GetSettingSerializer(ISetting setting)
        {
            return this._data.GetSettingSerializer(setting);
        }

        /// <summary>
        /// Attempt to load settings using specified <see cref="Path"/> if configuration
        /// file actually exists; otherwise does nothing.
        /// </summary>
        /// <exception cref="JsonParserException">
        /// Thrown if a syntax error was encountered whilst attempting to parse
        /// input content. Exception contains identifies the source of the error
        /// by providing the line number and position.
        /// </exception>
        /// <seealso cref="Path"/>
        /// <seealso cref="SaveDirtySettings()"/>
        public void Load()
        {
            if (File.Exists(this.Path)) {
                using (var stream = new FileStream(this.Path, FileMode.Open, FileAccess.Read)) {
                    this._data.Sync(JsonUtility.ReadFrom(stream));
                }
            }
        }

        /// <inheritdoc/>
        /// <seealso cref="Path"/>
        /// <seealso cref="Load()"/>
        public override void SaveDirtySettings()
        {
            if (string.IsNullOrEmpty(this.Path)) {
                throw new InvalidOperationException("Cannot save settings because 'Path' string is null or empty.");
            }

            JsonNode jsonSettings = new JsonObjectNode();

            if (File.Exists(this.Path)) {
                using (var stream = new FileStream(this.Path, FileMode.Open, FileAccess.Read)) {
                    jsonSettings = JsonUtility.ReadFrom(stream);
                }
            }

            if (this._data.Sync(jsonSettings, this.Manager)) {
                // Ensure that path exists!
                string directoryName = System.IO.Path.GetDirectoryName(this.Path);
                if (!Directory.Exists(directoryName)) {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(this.Path, this._data.ToJson());
            }
        }
    }
}
