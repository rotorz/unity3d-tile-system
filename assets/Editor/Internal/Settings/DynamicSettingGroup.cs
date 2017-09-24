// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings.Specialized;
using System;
using System.Collections.Generic;

namespace Rotorz.Settings
{
    internal sealed class DynamicSettingGroup : IDynamicSettingGroup
    {
        private Dictionary<string, ISetting> _settings = new Dictionary<string, ISetting>();


        public DynamicSettingGroup(string key, SettingManager manager)
        {
            this.Key = key;
            this.Manager = manager;
        }


        public SettingManager Manager { get; private set; }


        #region IDynamicSettingGroup Members

        private bool _sealed;

        /// <inheritdoc/>
        public bool IsSealed {
            get { return this._sealed; }
        }

        /// <inheritdoc/>
        public void Seal()
        {
            this._sealed = true;
        }

        private void CheckSealed()
        {
            if (this.IsSealed) {
                throw new InvalidOperationException("Cannot add or remove setting declarations because setting group is sealed.");
            }
        }

        #endregion


        #region ISettingGroup Members

        /// <inheritdoc/>
        public string Key { get; private set; }

        /// <inheritdoc/>
        public Setting<T> Lookup<T>(string key)
        {
            ISetting setting;
            return this._settings.TryGetValue(key, out setting)
                ? setting as Setting<T>
                : null;
        }

        /// <inheritdoc/>
        public ISetting LookupSettingWithoutType(string key)
        {
            ISetting setting;
            this._settings.TryGetValue(key, out setting);
            return setting;
        }

        /// <inheritdoc/>
        public bool IsDefined(string key)
        {
            return this._settings.ContainsKey(key);
        }

        /// <inheritdoc/>
        public IEnumerable<ISetting> Settings {
            get { return this._settings.Values; }
        }

        /// <inheritdoc/>
        public void RestoreDefaultValues()
        {
            foreach (var setting in this._settings.Values) {
                setting.RestoreDefaultValue();
            }

            this.Manager.Adapter.DeleteUnreferencedSettings(this);
        }

        /// <inheritdoc/>
        public void DiscardChanges()
        {
            foreach (var setting in this._settings.Values) {
                setting.DiscardChanges();
            }
        }

        #endregion


        #region ISettingStore Members

        /// <inheritdoc/>
        public Setting<T> Fetch<T>(string key, T defaultValue, FilterValue<T> filter)
        {
            var settingWithoutType = this.LookupSettingWithoutType(key);
            var setting = settingWithoutType as Setting<T>;

            if (settingWithoutType == null) {
                // Cannot define new settings if store has been sealed!
                this.CheckSealed();

                var settingType = typeof(T);

                if (settingType.IsEnum) {
                    setting = new EnumSetting<T>(this, key, defaultValue, filter);
                }
                else if (settingType.IsClass) {
                    setting = new ObjectSetting<T>(this, key, defaultValue, filter);
                }
                else {
                    setting = new Setting<T>(this, key, defaultValue, filter);
                }

                this._settings[key] = setting;

                this.Manager.Adapter.LoadSetting(setting);
            }
            else {
                // Does existing setting match signature of one which is being defined?
                if (setting == null) {
                    throw new InvalidOperationException(string.Format("Setting '{0}' has already been defined with a different value type.", key));
                }
                if (!setting.Equals(setting.DefaultValue, defaultValue)) {
                    throw new InvalidOperationException(string.Format("Setting '{0}' has already been defined with a different default value.", key));
                }
                if (setting._filter != filter) {
                    throw new InvalidOperationException(string.Format("Setting '{0}' has already been defined with a different filter.", key));
                }
            }

            return setting;
        }

        #endregion
    }
}
