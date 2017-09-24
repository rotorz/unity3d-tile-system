// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.EditorExtensions;
using Rotorz.Games.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor.Internal
{
    /// <exclude/>
    [InitializeOnLoad]
    public static class UnityIntegrationUtility
    {
        static UnityIntegrationUtility()
        {
            GenerateIfMissing();
        }


        #region Generator

        /// <summary>
        /// Gets absolute path to generated integration template file.
        /// </summary>
        private static string GeneratedUnityIntegrationTemplatePath {
            get { return PackageUtility.ResolveAssetPath("@rotorz/unity3d-tile-system", "Editor/Generated", "UnityIntegration.cs.template"); }
        }

        /// <summary>
        /// Gets absolute path to generated integration script file.
        /// </summary>
        private static string GeneratedUnityIntegrationScriptPath {
            get { return PackageUtility.ResolveAssetPath("@rotorz/unity3d-tile-system", "Editor/Generated", "UnityIntegration.cs"); }
        }

        /// <summary>
        /// Generate editor integration script if it doesn't already exist.
        /// </summary>
        /// <remarks>
        /// <para>Users can exclude the generated integration script from source control;
        /// but this means that the script will need to be generated each time they
        /// checkout.</para>
        /// </remarks>
        private static void GenerateIfMissing()
        {
            if (!File.Exists(GeneratedUnityIntegrationScriptPath)) {
                Regenerate();
            }
        }

        internal static void Regenerate()
        {
            string scriptTemplate = File.ReadAllText(GeneratedUnityIntegrationTemplatePath, Encoding.UTF8);
            string generatedScript = scriptTemplate;

            // Parse constants in template:
            generatedScript = Regex.Replace(generatedScript, @"\{\{([^\}]+)\}\}", TemplateConstantMatcher);
            // Parse localization strings in template:
            generatedScript = Regex.Replace(generatedScript, @"(TileLang.(OpensWindow)\s*\(\s*)?TileLang\.(Text|ParticularText)\s*\(\s*""([^""]*)""\s*(\,\s*""([^""]*)""\s*)?\)(\s*\))?", TemplateTextMatcher);

            // It is fine to overwrite this path because it should be nuked whenever the
            // package is updated or re-installed.
            File.WriteAllText(GeneratedUnityIntegrationScriptPath, generatedScript, Encoding.UTF8);

            AssetDatabase.Refresh();
        }

        private static string TemplateConstantMatcher(Match match)
        {
            string constantName = match.Groups[1].Value.Trim();
            switch (constantName) {
                case "__TimeNow__":
                    return DateTime.Now.Ticks.ToString();

                case "__LanguageCultureName__":
                    return PackageLanguageManager.PreferredCulture.Name;

                case "__ProductName__":
                    return ProductInfo.Name;

                default:
                    throw new KeyNotFoundException(string.Format("Unexpected constant '{0}'.", constantName));
            }
        }

        private static string TemplateTextMatcher(Match match)
        {
            string text;

            string textFunctionName = match.Groups[3].Value;
            switch (textFunctionName) {
                case "Text":
                    text = TileLang.Text(match.Groups[4].Value);
                    break;

                case "ParticularText":
                    text = TileLang.ParticularText(match.Groups[4].Value, match.Groups[6].Value);
                    break;

                default:
                    throw new KeyNotFoundException(string.Format("Unexpected text function '{0}'.", textFunctionName));
            }

            if (match.Groups[2].Value == "OpensWindow") {
                text = TileLang.OpensWindow(text);
            }

            text = "\"" + text + "\"";

            if (!match.Groups[2].Success) {
                // No wrapping function was added, so any trailing parenthesis does not
                // belong to that, simply copy to output.
                text = text + match.Groups[7].Value;
            }

            return text;
        }

        public static void CheckPreferredLanguage(string languageCultureName, string languageVer)
        {
            if (languageCultureName != PackageLanguageManager.PreferredCulture.Name || languageVer != TileLang.Text("__LanguageVer__")) {
                // Unity integration script probably needs to be updated!
                Debug.Log("Localizing Unity integration...");
                Regenerate();
                Debug.Log("Localizing complete.");
            }
        }

        #endregion


        #region Tool Menu Commands

        public static void ToolMenu_CreateTileSystem()
        {
            CreateTileSystemWindow.ShowWindow();
        }

        public static void ToolMenu_CreateBrushOrTileset()
        {
            CreateBrushWindow.ShowWindow();
        }

        public static void ToolMenu_UseAsPrefabOffset()
        {
            TransformUtility.UseAsPrefabOffset();
        }
        public static bool ToolMenu_UseAsPrefabOffset_Validate()
        {
            return TransformUtility.UseAsPrefabOffset_Validate();
        }

        public static void ToolMenu_ReplaceByBrush()
        {
            TileSystemCommands.Command_ReplaceByBrush();
        }

        public static void ToolMenu_BuildScene()
        {
            BuildUtility.BuildScene();
        }

        public static void ToolMenu_RescanBrushes()
        {
            // Refresh asset database.
            AssetDatabase.Refresh();

            // Automatically detect new brushes.
            BrushDatabase.Instance.Rescan();

            // Repaint windows that may have been affected.
            ToolUtility.RepaintPaletteWindows();
            DesignerWindow.RepaintWindow();
        }

        public static void ToolMenu_ClearPreviewCache()
        {
            AssetPreviewCache.ClearAllCachedAssetPreviewFiles();
        }

        public static void ToolMenu_EditorWindows_Designer()
        {
            DesignerWindow.ShowWindow().Focus();
        }

        public static void ToolMenu_EditorWindows_Scene()
        {
            ToolUtility.ShowScenePalette();
        }

        public static void ToolMenu_EditorWindows_Brushes()
        {
            ToolUtility.ShowBrushPalette();
        }

        public static void ToolMenu_OnlineResources_Repository()
        {
            Help.BrowseURL("https://github.com/rotorz/unity3d-tile-system");
        }

        public static void ToolMenu_OnlineResources_UserGuide()
        {
            Help.BrowseURL("https://github.com/rotorz/unity3d-tile-system/wiki/");
        }

        public static void ToolMenu_OnlineResources_API()
        {
            Help.BrowseURL("http://rotorz.com/unity/tile-system/api");
        }

        public static void ToolMenu_OnlineResources_Releases()
        {
            Help.BrowseURL("https://github.com/rotorz/unity3d-tile-system/releases");
        }

        public static void ToolMenu_Preferences()
        {
            EditorPreferencesWindow.ShowWindow();
        }

        public static void ToolMenu_About()
        {
            AboutWindow.ShowWindow();
        }

        #endregion


        #region Context Menu Commands - TileSystem Component

        public static void ContextMenu_TileSystem_ToggleLock(MenuCommand command)
        {
            var tileSystem = command.context as TileSystem;
            if (tileSystem != null) {
                tileSystem.Locked = !tileSystem.Locked;
                ToolUtility.RepaintScenePalette();

                // If a tool is selected then we need to repaint all scene views since the
                // visual status of the tool may have just changed!
                if (ToolManager.Instance.CurrentTool != null) {
                    SceneView.RepaintAll();
                }
            }
        }

        #endregion


        #region Context Menu Commands - Transform Component

        public static void ContextMenu_Transform_SelectTileSystem()
        {
            if (Selection.gameObjects.Length == 1) {
                var tileSystem = ToolUtility.FindParentTileSystem(Selection.activeTransform);
                if (tileSystem != null) {
                    Selection.objects = new Object[] { tileSystem.gameObject };
                }
            }
        }

        public static bool ContextMenu_Transform_SelectTileSystem_Validate()
        {
            return Selection.gameObjects.Length == 1 && ToolUtility.FindParentTileSystem(Selection.activeTransform) != null;
        }

        public static void ContextMenu_Transform_UseAsPrefabOffset()
        {
            TransformUtility.UseAsPrefabOffset();
        }

        public static bool ContextMenu_Transform_UseAsPrefabOffset_Validate()
        {
            return TransformUtility.UseAsPrefabOffset_Validate();
        }

        #endregion
    }
}
