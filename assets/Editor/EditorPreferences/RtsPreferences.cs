// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// User preferences.
    /// </summary>
    internal static class RtsPreferences
    {
        static RtsPreferences()
        {
            PrepareSettingGroup("Tools", PrepareSettings_Tools);
            PrepareSettingGroup("Painting", PrepareSettings_Painting);
            PrepareSettingGroup("Grid", PrepareSettings_Grid);
            PrepareSettingGroup("Misc", PrepareSettings_Misc);
        }


        private static readonly List<ISettingGroup> s_PreferenceGroups = new List<ISettingGroup>();

        private static void PrepareSettingGroup(string key, Action<ISettingStore> prepare)
        {
            var group = AssetSettingManagement.GetGroup(key);
            s_PreferenceGroups.Add(group);
            prepare(group);
            group.Seal();
        }


        /// <summary>
        /// Reset user preferences.
        /// </summary>
        public static void ResetToDefaultValues()
        {
            foreach (var group in s_PreferenceGroups) {
                group.RestoreDefaultValues();
            }

            ToolManagementSettings.RestoreToolVisibility();
            ToolManagementSettings.ResetToolOrdering();
        }


        #region Tools

        private static void PrepareSettings_Tools(ISettingStore store)
        {
            AutoShowToolPalette = store.Fetch<bool>("AutoShowToolPalette", false);
            AutoShowToolPalette.ValueChanged += (args) => {
                // Immediately show tool palette if a tool is activated!
                if (args.NewValue && ToolManager.Instance.CurrentTool != null) {
                    ToolUtility.ShowToolPalette(false);
                }
            };
        }


        /// <summary>
        /// User preference which indicates whether tool palette should be shown
        /// automatically when a tool is activated.
        /// </summary>
        public static Setting<bool> AutoShowToolPalette { get; private set; }

        #endregion


        #region Painting

        private static void PrepareSettings_Painting(ISettingStore store)
        {
            EraseEmptyChunksPreference = store.Fetch<EraseEmptyChunksPreference>(
                "EraseEmptyChunksPreference", Editor.EraseEmptyChunksPreference.Yes
            );
            EraseEmptyChunksPreference.ValueChanged += (args) => {
                EditorInternalUtility.Instance.eraseEmptyChunks = (int)args.NewValue;
            };

            ToolPreferredNozzleIndicator = store.Fetch<NozzleIndicator>(
                "ToolPreferredNozzleIndicator", NozzleIndicator.Automatic
            );

            ToolWireframeColor = store.Fetch<Color>(
                "ToolWireframeColor", new Color(1f, 0f, 0f, 0.55f)
            );
            ToolShadedColor = store.Fetch<Color>(
                "ToolShadedColor", new Color(1f, 0f, 0f, 0.07f)
            );

            ToolImmediatePreviews = store.Fetch<bool>(
                "ToolImmediatePreviews", true
            );
            ToolImmediatePreviewsTintColor = store.Fetch<Color>(
                "ToolImmediatePreviewsTintColor", new Color(1f, 0.33f, 0.33f, 0.7f)
            );
            ToolImmediatePreviewsSeeThrough = store.Fetch<bool>(
                "ToolImmediatePreviewsSeeThrough", false
            );
        }


        public static Setting<EraseEmptyChunksPreference> EraseEmptyChunksPreference { get; private set; }


        /// <summary>
        /// User preference indicating the preferred nozzle indicator.
        /// </summary>
        /// <remarks>
        /// <para>This property identifies the preferred nozzle indicator that to
        /// represent the active tile when using tools. The value of this property
        /// is more of a suggestion than a definitive setting which is honored when
        /// possible.</para>
        /// <para><img src="../art/nozzle-indicators.png" alt="Illustration of flat and wireframe nozzle indicators."/></para>
        /// </remarks>
        public static Setting<NozzleIndicator> ToolPreferredNozzleIndicator { get; private set; }

        /// <summary>
        /// User preference for color of wireframe tool indicators.
        /// </summary>
        public static Setting<Color> ToolWireframeColor { get; private set; }
        /// <summary>
        /// User preference for color of filled tool indicators.
        /// </summary>
        public static Setting<Color> ToolShadedColor { get; private set; }

        /// <summary>
        /// User preference indicating whether immediate previews should be shown.
        /// </summary>
        /// <remarks>
        /// <para><img src="../art/immediate-previews.png" alt="Illustration of immediate preview for 2D and 3D tiles."/></para>
        /// </remarks>
        public static Setting<bool> ToolImmediatePreviews { get; private set; }

        /// <summary>
        /// User preference for color used to tint immediate previews.
        /// </summary>
        /// <remarks>
        /// <para>The tint color is assigned to the <c>color</c> property of the
        /// preview material. The visual effect of this can be changed by modifying
        /// the immediate preview shaders:</para>
        /// <list type="bullet">
        ///     <item><c>Assets/Rotorz/Tile System/Shaders/Preview.shader</c></item>
        ///     <item><c>Assets/Rotorz/Tile System/Shaders/PreviewSeeThrough.shader</c></item>
        /// </list>
        /// </remarks>
        public static Setting<Color> ToolImmediatePreviewsTintColor { get; private set; }

        /// <summary>
        /// User preference indicating whether immediate previews are see-through by default.
        /// </summary>
        /// <remarks>
        /// <para>Immediate previews are generally rendered using the included
        /// shader "Rotorz/Preview". A second shader "Rotorz/Preview See Through"
        /// is included which causes the immediate preview to be rendered in front of
        /// other objects present in a scene.</para>
        /// <para>Set this property to <c>true</c> to use see-through version of
        /// shader by default. Holding the "Control" key when using a tool temporarily
        /// inverses the effect of this property.</para>
        /// <para><img src="../art/preview-see-through-shader.png" alt="Illustration of 'Rotorz/Preview' and 'Rotorz/Preview See Through' shaders."/></para>
        /// </remarks>
        public static Setting<bool> ToolImmediatePreviewsSeeThrough { get; private set; }

        #endregion


        #region Grid

        private static void PrepareSettings_Grid(ISettingStore store)
        {
            BackgroundGridColor = store.Fetch<Color>(
                "BackgroundGridColor", new Color32(0, 0, 0, 15)
            );
            MajorGridColor = store.Fetch<Color>(
                "MajorGridColor", new Color32(255, 255, 255, 15)
            );
            MinorGridColor = store.Fetch<Color>(
                "MinorGridColor", new Color32(255, 255, 255, 5)
            );
            ChunkGridColor = store.Fetch<Color>(
                "ChunkGridColor", new Color32(70, 192, 255, 30)
            );

            ShowActiveTileSystem = store.Fetch<bool>(
                "ShowActiveTileSystem", true
            );
            ShowGrid = store.Fetch<bool>(
                "ShowGrid", true
            );
            ShowChunks = store.Fetch<bool>(
                "ShowChunks", true
            );
        }


        /// <summary>
        /// User preference for color of grid background.
        /// </summary>
        public static Setting<Color> BackgroundGridColor { get; private set; }
        /// <summary>
        /// User preference for color of grid lines at major intervals.
        /// </summary>
        public static Setting<Color> MajorGridColor { get; private set; }
        /// <summary>
        /// User preference for color of grid lines at minor intervals.
        /// </summary>
        public static Setting<Color> MinorGridColor { get; private set; }
        /// <summary>
        /// User preference for color of grid lines at chunk boundaries.
        /// </summary>
        public static Setting<Color> ChunkGridColor { get; private set; }

        /// <summary>
        /// User preference indicating whether the active tile system should be highlighted.
        /// </summary>
        public static Setting<bool> ShowActiveTileSystem { get; private set; }
        /// <summary>
        /// User preference indicating whether grid lines should be shown.
        /// </summary>
        public static Setting<bool> ShowGrid { get; private set; }
        /// <summary>
        /// User preference indicating whether chunk boundaries should be shown.
        /// </summary>
        public static Setting<bool> ShowChunks { get; private set; }

        #endregion


        #region Misc

        private static void PrepareSettings_Misc(ISettingStore store)
        {
            ControlContent.TrailingTipsVisibleChanged += () => {
                RotorzWindow.RepaintIfShown<DesignerWindow>();
            };

            DisableCustomCursors = store.Fetch<bool>(
                "DisableCustomCursors", false
            );

            AlwaysCenterUtilityWindows = store.Fetch<bool>(
                "AlwaysCenterUtilityWindows", false
            );
        }


        /// <summary>
        /// User preference indicating whether custom cursors should be disabled.
        /// </summary>
        public static Setting<bool> DisableCustomCursors { get; private set; }

        /// <summary>
        /// User preference which indicates whether utility windows should always be
        /// centered upon being shown.
        /// </summary>
        public static Setting<bool> AlwaysCenterUtilityWindows { get; private set; }

        #endregion
    }
}
