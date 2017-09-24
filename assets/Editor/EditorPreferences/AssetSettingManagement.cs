// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using Rotorz.Settings.Persisted.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Management for settings specific to this asset.
    /// </summary>
    internal sealed class AssetSettingManagement : ScriptableObject
    {
        private const string SettingStore_VendorName = "Rotorz";
        private const string SettingStore_AssetName = "unity3d-tile-system";

        private static AssetSettingManagement s_Instance;

        private static AssetSettingManagement Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = ScriptableObject.CreateInstance<AssetSettingManagement>();
                }
                return s_Instance;
            }
        }

        private void OnEnable()
        {
            this.hideFlags = HideFlags.DontSave;

            this.InitSettingManager();
        }

        private void OnDisable()
        {
            this.CleanupSettingManager();
        }


        #region API

        public static IDynamicSettingGroup GetGroup(string key)
        {
            return Instance.settingManager.GetGroup(key);
        }

        public static void SaveSettings()
        {
            Instance.settingManager.Save();
        }

        #endregion


        #region Setting Manager

        [NonSerialized]
        private JsonSettingAdapter jsonSettingAdapter;
        [NonSerialized]
        private SettingManager settingManager;


        private void InitSettingManager()
        {
            try {
                this.jsonSettingAdapter = JsonSettingAdapter.FromApplicationDataPath(SettingStore_VendorName, SettingStore_AssetName);
                this.jsonSettingAdapter.Load();
            }
            catch (Rotorz.Json.JsonParserException ex) {
                Debug.LogError("JsonParserException: Was unable to parse '" + SettingStore_AssetName + "' configuration.\nSettings.json" + ex.Message);

                try {
                    // Create backup of invalid configuration file since it will be
                    // automatically overwritten when settings are next saved.
                    if (File.Exists(this.jsonSettingAdapter.Path)) {
                        string backupPath = GetUniqueFilePath(this.jsonSettingAdapter.Path + ".bak");
                        File.Move(this.jsonSettingAdapter.Path, backupPath);
                    }
                }
                catch (Exception ex2) {
                    Debug.LogError("Failed to move invalid '" + SettingStore_AssetName + "' configuration (see exception in next log entry).");
                    Debug.LogException(ex2);
                }
            }

            this.settingManager = new SettingManager(this.jsonSettingAdapter);
            this.settingManager.MessageFeedback += this._settingManager_MessageFeedback;
        }

        private void CleanupSettingManager()
        {
            if (this.settingManager != null) {
                this.settingManager.Save();
                this.settingManager.MessageFeedback -= this._settingManager_MessageFeedback;
                this.settingManager = null;
            }
            this.jsonSettingAdapter = null;
        }

        private void _settingManager_MessageFeedback(object sender, MessageFeedbackEventArgs args)
        {
            string message = args.Message;
            if (args.Exception != null) {
                message += "\nSee editor log for further details.";
            }

            switch (args.FeedbackType) {
                default:
                case MessageFeedbackType.Information:
                    Debug.Log(message);
                    break;
                case MessageFeedbackType.Warning:
                    Debug.LogWarning(message);
                    break;
                case MessageFeedbackType.Error:
                    Debug.LogError(message);
                    break;
            }

            // Output additional information to editor log file.
            if (args.Exception != null) {
                Console.WriteLine(args.Exception.ToString());
            }
        }

        private static string GetUniqueFilePath(string filePath)
        {
            if (File.Exists(filePath)) {
                var existingPaths = new HashSet<string>(Directory.GetFiles(Path.GetDirectoryName(filePath)));

                string baseFilePath = filePath + ".";
                int counter = 1;

                do {
                    filePath = baseFilePath + counter;
                    ++counter;
                }
                while (existingPaths.Contains(filePath));
            }
            return filePath;
        }

        #endregion
    }


    /// <summary>
    /// This asset modification processor is needed to workaround a bug in Unity where
    /// OnDisable and OnDestroy messages are not respected for ScriptableObject's when
    /// closing the Unity editor.
    /// </summary>
    internal sealed class SaveSettingsModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] assetPaths)
        {
            // We really don't want to crash the Unity asset processor!
            try {
                AssetSettingManagement.SaveSettings();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
            return assetPaths;
        }
    }
}
