// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <exclude/>
    internal static class PlopUtility
    {
        /// <summary>
        /// Determines whether brush can be used to plop tiles.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// A value of <c>true</c> if tiles can be plopped with specified brush; otherwise
        /// a value of <c>false</c>.
        /// </returns>
        public static bool CanPlopWithBrush(Brush brush)
        {
            // Do not even attempt to 'plop' tiles with a tileset brush.
            //!TODO: This could be improved!
            var alias = brush as AliasBrush;
            if (alias != null && alias.target is TilesetBrush) {
                return false;
            }

            return brush != null && !(brush is TilesetBrush) && !brush.disableImmediatePreview;
        }

        /// <summary>
        /// Calculate placement point from local point.
        /// </summary>
        /// <param name="system">The tile system.</param>
        /// <param name="localPoint">Local point within tile system.</param>
        /// <returns>
        /// Placement point for plop.
        /// </returns>
        public static Vector3 PositionFromPlopPoint(TileSystem system, Vector3 localPoint)
        {
            localPoint.x -= system.CellSize.x / 2f;
            localPoint.y += system.CellSize.y / 2f;
            return localPoint;
        }

        private static TileSystem s_TempSystem;

        private static TileSystem GetTempSystem(TileSystem activeSystem)
        {
            // Get or create a temporary 1x1 tile system.
            if (s_TempSystem == null) {
                var go = GameObject.Find("{{Plop Tool}} Temp System");
                if (go == null) {
                    go = EditorUtility.CreateGameObjectWithHideFlags("{{Plop Tool}} Temp System", HideFlags.HideAndDontSave);
                    s_TempSystem = go.AddComponent<TileSystem>();
                    s_TempSystem.CreateSystem(1, 1, 1, 1, 1, 1, 1);
                }
                else {
                    s_TempSystem = go.GetComponent<TileSystem>();
                }
            }

            // Mimic tile size and facing as active tile system.
            s_TempSystem.CellSize = activeSystem.CellSize;
            s_TempSystem.TilesFacing = activeSystem.TilesFacing;

            return s_TempSystem;
        }

        public static PlopInstance PaintPlop(TileSystem system, Vector3 localPoint, Brush brush, int rotation, int variation)
        {
            var tempSystem = GetTempSystem(system);

            try {
                // Align temporary tile system to mouse pointer.
                tempSystem.transform.SetParent(system.transform, false);
                tempSystem.transform.localPosition = PositionFromPlopPoint(system, localPoint);
                tempSystem.transform.localRotation = Quaternion.identity;
                tempSystem.transform.localScale = Vector3.one;

                // Paint tile!
                var tile = brush.PaintWithSimpleRotation(tempSystem, 0, 0, rotation, variation);
                if (tile != null && tile.gameObject != null) {
                    // We can have undo/redo for plops!
                    Undo.RegisterCreatedObjectUndo(tile.gameObject, TileLang.ParticularText("Action", "Plop Tile"));

                    // Indicate that painted game object is a plop!
                    var plop = tile.gameObject.AddComponent<PlopInstance>();
                    plop.Owner = system;

                    var tileTransform = tile.gameObject.transform;

                    // Disconnect game object from data structure of temporary system.
                    SetParent(tileTransform, system.transform);
                    tile.gameObject = null;

                    // Store some additional data for plop!
                    plop.PlopPointOffset = localPoint - tileTransform.localPosition;
                    plop.Brush = brush;
                    plop.VariationIndex = tile.variationIndex;
                    plop.PaintedRotation = rotation;
                    plop.Rotation = tile.Rotation;

                    return plop;
                }

                return null;
            }
            finally {
                // We must cleanup before we finish!
                tempSystem.EraseTile(0, 0);
                tempSystem.transform.SetParent(null, false);
            }
        }

        public static int CountPlopVariations(TileSystem system, PlopInstance plop)
        {
            if (plop == null || plop.Brush == null) {
                return 0;
            }

            int orientation = OrientationUtility.DetermineTileOrientation(system, TileIndex.zero, plop.Brush, plop.PaintedRotation);
            return plop.Brush.CountTileVariations(orientation);
        }

        public static PlopInstance CyclePlop(TileSystem system, PlopInstance plop, Brush brush, int nextRotation, int nextVariation)
        {
            Undo.RecordObject(plop, TileLang.ParticularText("Action", "Cycle Plop"));

            var parentTransform = plop.transform.parent;

            var tileData = plop.ToTileData();
            nextVariation = Brush.WrapVariationIndexForCycle(Brush.GetSharedContext(brush, GetTempSystem(system), TileIndex.zero), tileData, nextVariation);

            var newPlop = PaintPlop(system, plop.PlopPoint, brush, nextRotation, nextVariation);
            ErasePlop(plop);

            // New plop should have same parent as original plop.
            var newPlopTransform = newPlop.transform;
            if (newPlopTransform.parent != parentTransform) {
                newPlopTransform.SetParent(parentTransform);
            }

            return newPlop;
        }

        public static PlopInstance RefreshPlop(TileSystem system, PlopInstance plop)
        {
            return CyclePlop(system, plop, plop.Brush, plop.PaintedRotation, plop.VariationIndex);
        }

        public static void ErasePlop(PlopInstance plop)
        {
            var plopParent = plop.transform.parent;

            Undo.DestroyObjectImmediate(plop.gameObject);

            // Automatically erase container object when empty.
            AutoRemovePlopGroupIfEmpty(plopParent);
        }

        private static void SetParent(Transform obj, Transform system)
        {
            var plopTool = ToolManager.Instance.Find<PlopTool>();

            // Assume default value if plop tool is for some reason unregistered.
            PlopTool.Location location = (plopTool != null)
                ? plopTool.PlopLocation
                : PlopTool.Location.ChildOfTileSystem;

            switch (location) {
                default:
                case PlopTool.Location.GroupInsideTileSystem:
                    var group = system.Find(plopTool.PlopGroupName);
                    if (group == null) {
                        // Create 'Plops' group if necessary!
                        var groupGO = new GameObject(plopTool.PlopGroupName);
                        group = groupGO.transform;
                        group.SetParent(system, false);

                        groupGO.AddComponent<PlopGroup>();

                        Undo.RegisterCreatedObjectUndo(groupGO, "Create Group for Plops");
                    }
                    obj.SetParent(group);
                    break;

                case PlopTool.Location.ChildOfTileSystem:
                    obj.SetParent(system);
                    break;

                case PlopTool.Location.SceneRoot:
                    obj.SetParent(null);
                    break;
            }
        }

        private static void AutoRemovePlopGroupIfEmpty(Transform plopGroup)
        {
            // Was a valid group specified?
            if (plopGroup == null || plopGroup.GetComponent<PlopGroup>() == null) {
                return;
            }

            // Can only remove an empty group, i.e. one with no children and no extra components.
            if (plopGroup.childCount == 0 && plopGroup.GetComponents<Component>().Length <= 2) {
                Undo.DestroyObjectImmediate(plopGroup.gameObject);
            }
        }
    }
}
