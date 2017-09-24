// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <exclude/>
    public static class TileSystemCommands
    {
        internal static void DeleteBrush(Brush brush, bool selectParentTileset)
        {
            var brushRecord = BrushDatabase.Instance.FindRecord(brush);
            if (brushRecord == null) {
                return;
            }

            RotorzEditorGUI.ClearControlFocus();

            if (EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Delete Brush"),
                string.Format(
                    /* 0: name of brush */
                    TileLang.Text("Deleting a brush that has been previously used in a tile system may cause damage to that tile system.\n\n'{0}'\n\nDo you really want to delete this brush?"),
                    brushRecord.DisplayName
                ),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                // Determine whether tileset should be selected in designer.
                var designerWindow = RotorzWindow.GetInstance<DesignerWindow>();
                if (designerWindow != null && ReferenceEquals(brushRecord.Brush, designerWindow.SelectedObject)) {
                    designerWindow.SelectedObject = selectParentTileset && brushRecord.Brush is TilesetBrush
                        ? (brushRecord.Brush as TilesetBrush).Tileset
                        : null;
                }

                BrushUtility.DeleteBrush(brushRecord.Brush);
            }
        }

        private static int ReplaceByBrush(TileSystem system, Brush source, Brush replacement)
        {
            Undo.RegisterFullObjectHierarchyUndo(system.gameObject, TileLang.ParticularText("Action", "Replace by Brush"));
            return system.ReplaceByBrush(source, replacement);
        }

        /// <summary>
        /// Display user interface to replace tiles by brush in a tile system.
        /// </summary>
        /// <exclude/>
        public static void Command_ReplaceByBrush()
        {
            ReplaceByBrushWindow.ShowWindow((target, source, replacement) => {
                // Perform find and replace.
                int replaced = 0;
                switch (target) {
                    default:
                    case ReplaceByBrushTarget.ActiveTileSystem:
                        if (ToolUtility.ActiveTileSystem != null && ToolUtility.ActiveTileSystem.IsEditable && !ToolUtility.ActiveTileSystem.Locked) {
                            replaced = ReplaceByBrush(ToolUtility.ActiveTileSystem, source, replacement);
                        }
                        break;

                    case ReplaceByBrushTarget.SelectedTileSystems:
                        foreach (Object ob in Selection.GetFiltered(typeof(TileSystem), SelectionMode.ExcludePrefab)) {
                            TileSystem system = ob as TileSystem;
                            if (system.IsEditable && !system.Locked) {
                                replaced += ReplaceByBrush(system, source, replacement);
                            }
                        }
                        break;

                    case ReplaceByBrushTarget.All:
                        foreach (var tileSystem in ToolUtility.GetAllTileSystemsInScene()) {
                            if (tileSystem.IsEditable && !tileSystem.Locked) {
                                replaced += ReplaceByBrush(tileSystem, source, replacement);
                            }
                        }
                        break;
                }

                string message = string.Format(
                    /* 0: quantity of tiles replaced */
                    TileLang.Text("Replace by Brush :: Matched and replaced {0} tile(s)."),
                    replaced
                );
                Debug.Log(message + "\n");
            });
        }

        /// <summary>
        /// Display user interface for refreshing tiles in a tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        internal static void Command_Refresh(TileSystem system)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            RefreshTilesWindow.ShowWindow(system);
        }

        /// <summary>
        /// Display user interface for repairing broken and dirty tiles in a tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        internal static void Command_Repair(TileSystem system)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            system.UpdateProceduralTiles(true);

            // First check tile system for broken tiles.
            int count = system.ScanBrokenTiles(RepairAction.JustCount);
            if (count == 0) {
                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Action", "Repair Tiles"),
                    string.Format(
                        /* 0: name of tile system */
                        TileLang.Text("No broken tiles were detected in '{0}'."),
                        system.name
                    ),
                    TileLang.ParticularText("Action", "Close")
                );
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                TileLang.ParticularText("Action", "Repair Tiles"),
                string.Format(
                    /* 0: name of tile system
                       1: quantity of broken tiles */
                    TileLang.Text("Detected {1} broken tiles where associated game object is missing.\n\nThis usually occurs when tiles are deleted manually. Use erase tool (or erase functionality of paint tool) instead of deleting tile objects manually.\n\nBroken tiles can often be repaired by force refreshing them, or alternatively erased."),
                    system.name, count
                ),
                TileLang.ParticularText("Action", "Erase"),
                TileLang.ParticularText("Action", "Cancel"),
                TileLang.ParticularText("Action", "Repair")
            );
            if (choice == 1) {
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(system.gameObject, TileLang.ParticularText("Action", "Repair Tiles"));

            if (choice == 0) {
                system.ScanBrokenTiles(RepairAction.Erase);
            }
            else {
                system.ScanBrokenTiles(RepairAction.ForceRefresh);
            }
        }

        /// <summary>
        /// Display user interface to clear a tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        internal static void Command_Clear(TileSystem system)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            if (!EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Clear Tiles"),
                string.Format(
                    /* 0: name of tile system */
                    TileLang.Text("Do you really want to clear all tiles from '{0}'?"),
                    system.name
                ),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                return;
            }

            // Register undo event.
            Undo.RegisterFullObjectHierarchyUndo(system.gameObject, TileLang.ParticularText("Action", "Clear Tiles"));
            // Erase all tiles from selected system.
            system.EraseAllTiles();
        }

        /// <summary>
        /// Display user interface to force refresh all plops associated with a tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        internal static void Command_RefreshPlops(TileSystem system)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            if (!EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Refresh Plops"),
                string.Format(
                    /* 0: name of tile system */
                    TileLang.Text("Do you want to force refresh all plops associated with '{0}'?"),
                    system.name
                ),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                return;
            }

            // Refresh all plops which are associated with tile system.
            foreach (var plop in UnityEngine.Resources.FindObjectsOfTypeAll<PlopInstance>()) {
                if (plop.Owner == system && plop.Brush != null) {
                    PlopUtility.RefreshPlop(system, plop);
                }
            }
        }

        /// <summary>
        /// Display user interface to clear all plops associated with a tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        internal static void Command_ClearPlops(TileSystem system)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            if (!EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Clear Plops"),
                string.Format(
                    /* 0: name of tile system */
                    TileLang.Text("Do you want to clear all plops associated with '{0}'?"),
                    system.name
                ),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                return;
            }

            // Clear all plops which are associated with tile system.
            foreach (var plop in UnityEngine.Resources.FindObjectsOfTypeAll<PlopInstance>()) {
                if (plop.Owner == system) {
                    Undo.DestroyObjectImmediate(plop.gameObject);
                }
            }

            // Clear plop groups from tile system.
            foreach (var plopGroup in system.GetComponentsInChildren<PlopGroup>()) {
                var plopGroupTransform = plopGroup.transform;
                if (plopGroupTransform.childCount == 0 && plopGroupTransform.GetComponents<Component>().Length == 2) {
                    Undo.DestroyObjectImmediate(plopGroup.gameObject);
                }
            }
        }

        /// <summary>
        /// Display user interface for building prefab from a tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="system"/> is <c>null</c>.
        /// </exception>
        internal static void Command_BuildPrefab(TileSystem system)
        {
            if (system == null) {
                throw new ArgumentNullException("system");
            }

            BuildTileSystemWindow.ShowWindow(system);
        }
    }
}
