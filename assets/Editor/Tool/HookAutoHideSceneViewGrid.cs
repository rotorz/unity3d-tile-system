// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using System;
using System.Reflection;
using UnityEditor;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Hooks into various events so that the scene view placement grid is automatically
    /// hidden whenever a tool is selected; and restored upon being deselected.
    /// </summary>
    [InitializeOnLoad]
    internal static class HookAutoHideSceneViewGrid
    {
        static HookAutoHideSceneViewGrid()
        {
            // Gain access to the non-public property `AnnotationUtility.showGrid`.
            // Thanks ShawnWhite!
            Type tyAnnotationUtility = typeof(SceneView).Assembly.GetType("UnityEditor.AnnotationUtility", false);
            if (tyAnnotationUtility != null) {
                s_piAnnotationUtilityShowGrid = tyAnnotationUtility.GetProperty("showGrid", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }

            ToolManager.Instance.ToolChanged += ToolManager_ToolChanged;

            var settingGroup = AssetSettingManagement.GetGroup("ToolManager");
            HookEnabled = settingGroup.Fetch<bool>("AutoHideSceneViewGrid", true);
            HookEnabled.ValueChanged += AutoHideSceneViewGrid_ValueChanged;

            s_RestoreAnnotationUtilityShowGrid = AnnotationUtilityShowGrid;
        }


        /// <summary>
        /// Gets a value indicating whether this feature is available. Will return a
        /// value of <c>false</c> if Unity rename or remove the required property.
        /// </summary>
        public static bool IsFeatureAvailable {
            get { return s_piAnnotationUtilityShowGrid != null; }
        }

        /// <summary>
        /// Gets the setting that indicates whether this hook is enabled.
        /// </summary>
        public static Setting<bool> HookEnabled { get; set; }

        /// <summary>
        /// Indicates whether the scene view grid should be restored when tool becomes inactive.
        /// </summary>
        private static bool s_RestoreAnnotationUtilityShowGrid;


        private static void AutoHideSceneViewGrid_ValueChanged(ValueChangedEventArgs<bool> args)
        {
            if (ToolManager.Instance.CurrentTool != null) {
                if (args.NewValue) {
                    AnnotationUtilityShowGrid = false;
                }
                else {
                    AnnotationUtilityShowGrid = s_RestoreAnnotationUtilityShowGrid;
                }
            }
        }

        private static void ToolManager_ToolChanged(ToolBase oldTool, ToolBase newTool)
        {
            if (oldTool == null && newTool != null) {
                s_RestoreAnnotationUtilityShowGrid = AnnotationUtilityShowGrid;
                if (HookEnabled.Value) {
                    AnnotationUtilityShowGrid = false;
                }
            }
            else if (newTool == null) {
                AnnotationUtilityShowGrid = s_RestoreAnnotationUtilityShowGrid;
            }
        }

        private static PropertyInfo s_piAnnotationUtilityShowGrid;
        private static bool AnnotationUtilityShowGrid {
            get {
                return s_piAnnotationUtilityShowGrid != null
                    ? (bool)s_piAnnotationUtilityShowGrid.GetValue(null, null)
                    : true;
            }
            set {
                if (s_piAnnotationUtilityShowGrid != null)
                    s_piAnnotationUtilityShowGrid.SetValue(null, value, null);
            }
        }
    }
}
