// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <exclude/>
    internal static class EditorTileSystemUtility
    {
        #region AllTileSystemsInScene

        internal static bool s_ShouldRepaintScenePalette = false;

        private static HashSet<TileSystem> s_AllTileSystemsInScene = new HashSet<TileSystem>();
        private static List<TileSystem> s_NonHiddenTileSystemsInScene = new List<TileSystem>();
        private static ReadOnlyCollection<TileSystem> s_NonHiddenTileSystemsInSceneReadOnly = new ReadOnlyCollection<TileSystem>(s_NonHiddenTileSystemsInScene);

        // Let's cache the delegate instances to avoid lots of allocations in tight loops!
        private static Predicate<TileSystem> s_PruneMissing = (system) => system == null;
        private static Comparison<TileSystem> s_SortBySceneOrder = (a, b) =>
            a.gameObject.scene == b.gameObject.scene
                ? a.sceneOrder - b.sceneOrder
                : string.Compare(a.gameObject.scene.name, b.gameObject.scene.name);

        /// <summary>
        /// Prune all missing tile systems from register.
        /// </summary>
        private static void PruneMissingTileSystems()
        {
            if (s_AllTileSystemsInScene.RemoveWhere(s_PruneMissing) > 0) {
                s_ShouldRepaintScenePalette = true;
            }
        }

        private static void RefreshNonHiddenTileSystemsInScene()
        {
            // Gather list of all tile systems in scene that are not hidden.
            s_NonHiddenTileSystemsInScene.Clear();
            foreach (var tileSystem in s_AllTileSystemsInScene) {
                if (tileSystem.hideFlags == HideFlags.None) {
                    s_NonHiddenTileSystemsInScene.Add(tileSystem);
                }
            }

            RefreshOrderingOfNonHiddenTileSystems();
        }

        /// <summary>
        /// Refresh ordering of tile systems in scene using scene order.
        /// </summary>
        private static void RefreshOrderingOfNonHiddenTileSystems()
        {
            s_NonHiddenTileSystemsInScene.Sort(s_SortBySceneOrder);

            if (s_NonHiddenTileSystemsInScene.Count == 0) {
                return;
            }

            var currentScene = s_NonHiddenTileSystemsInScene[0].gameObject.scene;
            int sceneIndex = 0;

            // Normalize scene order numbers in tile system objects.
            int count = s_NonHiddenTileSystemsInScene.Count;
            for (int i = 0; i < count; ++i) {
                var tileSystem = s_NonHiddenTileSystemsInScene[i];

                if (tileSystem.gameObject.scene != currentScene) {
                    currentScene = tileSystem.gameObject.scene;
                    sceneIndex = 0;
                }

                if (tileSystem.sceneOrder != sceneIndex) {
                    tileSystem.sceneOrder = sceneIndex;
                    EditorUtility.SetDirty(tileSystem);
                }

                ++sceneIndex;
            }
        }

        /// <summary>
        /// Gets read-only collection of all tile systems that are present within scene.
        /// </summary>
        internal static IList<TileSystem> AllTileSystemsInScene {
            get {
                if (TileSystemUtility.TileSystemListingDirty) {
                    TileSystemUtility.TileSystemListingDirty = false;

                    s_AllTileSystemsInScene.Clear();

                    var tileSystemsInScene =
                        from system in UnityEngine.Resources.FindObjectsOfTypeAll<TileSystem>()
                        where !IsPrefab(system)
                        select system;
                    foreach (var tileSystem in tileSystemsInScene) {
                        if (s_AllTileSystemsInScene.Add(tileSystem)) {
                            s_ShouldRepaintScenePalette = true;
                        }
                    }
                }
                else {
                    PruneMissingTileSystems();
                }

                RefreshNonHiddenTileSystemsInScene();
                return s_NonHiddenTileSystemsInSceneReadOnly;
            }
        }

        #endregion


        #region Ordering

        public static void ApplySceneOrders(IEnumerable<TileSystem> tileSystems)
        {
            int index = 0;
            foreach (var tileSystem in tileSystems) {
                if (tileSystem.sceneOrder != index) {
                    tileSystem.sceneOrder = index;
                    EditorUtility.SetDirty(tileSystem);
                }
                ++index;
            }
        }

        public static void SortTileSystemsAscending()
        {
            var tileSystems = EditorTileSystemUtility.AllTileSystemsInScene;
            Undo.RecordObjects(tileSystems.Cast<Object>().ToArray(), TileLang.ParticularText("Action", "Reorder Tile Systems"));
            ApplySceneOrders(tileSystems.OrderBy(system => system.name));
        }

        public static void SortTileSystemsDescending()
        {
            var tileSystems = EditorTileSystemUtility.AllTileSystemsInScene;
            Undo.RecordObjects(tileSystems.Cast<Object>().ToArray(), TileLang.ParticularText("Action", "Reorder Tile Systems"));
            ApplySceneOrders(tileSystems.OrderByDescending(system => system.name));
        }

        public static bool ReorderTileSystem(TileSystem system, int sceneOrder)
        {
            if (system.sceneOrder == sceneOrder) {
                return false;
            }

            var tileSystems = EditorTileSystemUtility.AllTileSystemsInScene;
            if (!tileSystems.Contains(system)) {
                return false;
            }

            // Adjust scene order of tile systems.
            foreach (var tileSystem in tileSystems) {
                if (tileSystem.sceneOrder >= sceneOrder) {
                    Undo.RecordObject(tileSystem, TileLang.ParticularText("Action", "Reorder Tile Systems"));
                    ++tileSystem.sceneOrder;
                }
            }
            system.sceneOrder = sceneOrder;

            ApplySceneOrders(tileSystems.OrderBy(s => s.sceneOrder));

            return true;
        }

        #endregion


        private static bool IsPrefab(Object obj)
        {
            return PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab;
        }
    }
}
