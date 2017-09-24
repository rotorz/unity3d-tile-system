// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Json;
using System;
using System.Collections.Generic;

namespace Rotorz.Settings.Persisted.Json
{
    internal sealed class JsonSettingData
    {
        private Dictionary<string, JsonSettingGroupData> _groups = new Dictionary<string, JsonSettingGroupData>();


        public IEnumerable<string> GroupKeys {
            get { return this._groups.Keys; }
        }


        private JsonSettingGroupData GetGroupData(string groupKey, bool autoCreate)
        {
            JsonSettingGroupData groupData;
            this._groups.TryGetValue(groupKey, out groupData);
            if (groupData == null && autoCreate) {
                groupData = new JsonSettingGroupData(this, groupKey);
                this._groups[groupKey] = groupData;
            }
            return groupData;
        }

        public JsonSettingGroupData GetGroupData(string groupKey)
        {
            return this.GetGroupData(groupKey, false);
        }

        public ISettingSerializer GetSettingSerializer(ISetting setting)
        {
            return this.GetGroupData(setting.GroupKey, true);
        }

        private Dictionary<string, JsonObjectNode> ReadData(JsonNode rootNode)
        {
            var settingData = new Dictionary<string, JsonObjectNode>();

            var jsonData = rootNode as JsonObjectNode;
            if (jsonData != null) {
                foreach (var jsonGroup in jsonData) {
                    if (jsonGroup.Value is JsonObjectNode) {
                        settingData[jsonGroup.Key] = jsonGroup.Value as JsonObjectNode;
                    }
                }
            }

            return settingData;
        }

        private JsonObjectNode ToJsonObject()
        {
            var jsonData = new JsonObjectNode();
            foreach (var groupData in this._groups.Values) {
                var jsonGroup = groupData.ToJsonObject();
                if (jsonGroup != null) {
                    jsonData[groupData.Key] = jsonGroup;
                }
            }
            return jsonData;
        }

        public string ToJson()
        {
            return this.ToJsonObject().ToString();
        }

        public bool Sync(JsonNode rootNode, SettingManager manager = null)
        {
            bool save = false;

            // Load setting data from file.
            var freshSettingData = this.ReadData(rootNode);

            // Synchronize modified settings with freshly read setting data.
            foreach (var groupData in this._groups.Values) {
                ISettingGroup group = (manager != null ? manager.GetGroup(groupData.Key) : null);

                JsonObjectNode freshGroupData;
                freshSettingData.TryGetValue(groupData.Key, out freshGroupData);

                save |= groupData.Sync(group, freshGroupData);
            }

            // Read preferences for newly found groups.
            foreach (var freshGroupData in freshSettingData) {
                if (this._groups.ContainsKey(freshGroupData.Key)) {
                    continue;
                }

                var groupData = this.GetGroupData(freshGroupData.Key, true);
                groupData.Sync(null, freshGroupData.Value);
            }

            return save;
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
