// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// </summary>
    internal static class ToolManagementSettings
    {
        static ToolManagementSettings()
        {
            var settingGroup = AssetSettingManagement.GetGroup("ToolManager");

            Hidden = settingGroup.Fetch<HashSet<string>>("Hidden", null,
                filter: (value) => value ?? new HashSet<string>()
            );
            Order = settingGroup.Fetch<string[]>("Order", null,
                filter: (value) => value ?? new string[0]
            );
        }


        #region Hidden Tools

        private static Setting<HashSet<string>> Hidden { get; set; }

        /// <summary>
        /// Restore tool visibility to defaults.
        /// </summary>
        public static void RestoreToolVisibility()
        {
            Hidden.RestoreDefaultValue();
            foreach (var tool in ToolManager.Instance.Tools) {
                tool._visible = true;
            }
            ToolUtility.RepaintToolPalette();
        }

        /// <summary>
        /// Gets a value indicating whether tool is hidden.
        /// </summary>
        /// <param name="tool">Tool.</param>
        /// <returns>
        /// A value of <c>true</c> if tool is hidden; otherwise a value of <c>false</c>.
        /// </returns>
        public static bool IsToolHidden(ToolBase tool)
        {
            return Hidden.Value.Contains(tool.GetType().FullName);
        }

        /// <summary>
        /// Hide or unhide tool.
        /// </summary>
        /// <param name="tool">Tool.</param>
        /// <param name="hide">Specify a value of <c>true</c> to hide tool; otherwise
        /// a value of <c>false</c> to unhide tool.</param>
        public static void HideTool(ToolBase tool, bool hide)
        {
            if (tool._visible == !hide) {
                return;
            }

            tool._visible = !hide;

            if (hide) {
                Hidden.Value.Add(tool.GetType().FullName);
            }
            else {
                Hidden.Value.Remove(tool.GetType().FullName);
            }

            Hidden.MarkDirty();
        }

        #endregion


        #region Tool Ordering

        private static Setting<string[]> Order { get; set; }

        /// <summary>
        /// Assume ordering of tools within list as user preferred ordering.
        /// </summary>
        public static void SaveToolOrdering()
        {
            Order.Value = ToolManager.Instance.Tools
                .Select(tool => tool.GetType().FullName)
                .ToArray();
        }

        /// <summary>
        /// Apply user preferred ordering from setting.
        /// </summary>
        public static void LoadToolOrdering()
        {
            var manager = ToolManager.Instance;

            var toolList = manager.toolsInUserOrder;

            string[] ordering = ToolManagementSettings.Order;

            // Place tools in order registered at end of list when user ordering
            // has not been specified.
            int orderRegistered = ordering.Length;

            // Apply user ordering to tools.
            ToolBase[] toolsInUserOrder = manager.ToolsInOrderRegistered
                .Select(tool => {
                    int order = Array.IndexOf(ordering, tool.GetType().FullName);
                    if (order == -1) {
                        order = orderRegistered++;
                    }
                    return new KeyValuePair<ToolBase, int>(tool, order);
                })
                .OrderBy(pair => pair.Value)
                .Select(pair => pair.Key)
                .ToArray();

            toolList.Clear();
            toolList.AddRange(toolsInUserOrder);
        }

        /// <summary>
        /// Reset to default tool ordering (order of registration).
        /// </summary>
        public static void ResetToolOrdering()
        {
            var manager = ToolManager.Instance;

            var toolList = manager.toolsInUserOrder;
            toolList.Clear();
            toolList.AddRange(manager.ToolsInOrderRegistered);

            SaveToolOrdering();
        }

        #endregion
    }
}
