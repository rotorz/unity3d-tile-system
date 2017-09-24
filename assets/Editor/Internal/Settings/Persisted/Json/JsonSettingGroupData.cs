// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotorz.Settings.Persisted.Json
{
    internal sealed class JsonSettingGroupData : ISettingSerializer
    {
        private Dictionary<string, JsonNode> _settings = new Dictionary<string, JsonNode>();


        public JsonSettingGroupData(JsonSettingData owner, string key)
        {
            this.Owner = owner;
            this.Key = key;
        }


        public JsonSettingData Owner { get; private set; }
        public string Key { get; private set; }


        public bool Sync(ISettingGroup group, JsonObjectNode jsonGroup)
        {
            bool dirty = false;

            this._settings.Clear();

            if (group != null) {
                // Serialize modified properties.
                foreach (ISetting setting in group.Settings) {
                    if (setting.IsDirty) {
                        try {
                            setting.Serialize(this);
                            dirty = true;
                        }
                        catch (Exception ex) {
                            this.Owner.LogFeedback(MessageFeedbackType.Error, string.Format("Was unable to serialize setting '{0}.{1}', skipping...", setting.GroupKey, setting.Key), ex);
                        }
                    }
                }
            }

            if (!this._clean) {
                if (jsonGroup != null) {
                    // Merge freshly loaded setting data with setting data.
                    foreach (var jsonSetting in jsonGroup) {
                        if (!this._settings.ContainsKey(jsonSetting.Key)) {
                            this._settings[jsonSetting.Key] = jsonSetting.Value;
                        }
                    }
                }
                this._clean = false;
            }

            return dirty;
        }

        public JsonObjectNode ToJsonObject()
        {
            if (this._settings.Count == 0) {
                return null;
            }

            var jsonGroup = new JsonObjectNode();
            foreach (var settingData in this._settings) {
                jsonGroup[settingData.Key] = settingData.Value;
            }
            return jsonGroup;
        }


        #region ISettingGroupData Members

        private bool _clean;

        public void DeleteAllSettings()
        {
            this._clean = true;
            this._settings.Clear();
        }

        public void DeleteUnreferencedSettings(ISettingGroup group)
        {
            if (group == null) {
                throw new ArgumentNullException("group");
            }

            this._clean = true;

            // Fetch list of keys for settings which are not referenced by group.
            var keys = from key in this._settings.Keys
                       where !@group.IsDefined(key)
                       select key;

            foreach (var key in keys.ToArray()) {
                this._settings.Remove(key);
            }
        }

        #endregion


        #region ISettingSerializer Members

        /// <inheritdoc/>
        void ISettingSerializer.Serialize<T>(ISetting setting, T value)
        {
            this._settings[setting.Key] = JsonUtility.ConvertFrom(value);
        }

        /// <inheritdoc/>
        T ISettingSerializer.Deserialize<T>(ISetting setting, T defaultValue)
        {
            JsonNode serializedValue;
            return this._settings.TryGetValue(setting.Key, out serializedValue)
                ? serializedValue.ConvertTo<T>()
                : defaultValue;
        }

        #endregion
    }
}
