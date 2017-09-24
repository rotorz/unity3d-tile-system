// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor.Internal
{
    /// <exclude/>
    public static class TransformUtility
    {
        private static GameObject GetPrefabInstance()
        {
            if (Selection.gameObjects.Length != 1) {
                return null;
            }

            return PrefabUtility.FindRootGameObjectWithSameParentPrefab(Selection.gameObjects[0]);
        }

        private static Transform GetTileGameObject(Transform obj)
        {
            if (obj.parent != null && obj.parent.name == "tile") {
                obj = obj.parent;
            }

            var chunkTransform = obj.parent;
            if (chunkTransform == null) {
                return null;
            }

            return (chunkTransform.GetComponent<Chunk>() != null)
                ? obj
                : null;
        }

        public static void UseAsPrefabOffset()
        {
            var attachedGameObject = GetPrefabInstance();
            if (attachedGameObject == null || attachedGameObject != Selection.activeGameObject) {
                return;
            }

            var attachedTransform = attachedGameObject.transform;
            var tileTransform = GetTileGameObject(attachedTransform);
            if (tileTransform == null) {
                return;
            }

            // Get chunk component.
            var chunk = tileTransform.parent.GetComponent<Chunk>();
            if (chunk.TileSystem == null) {
                return;
            }

            var prefab = PrefabUtility.GetPrefabParent(attachedGameObject) as GameObject;
            var prefabTransform = prefab.transform;

            var tileSystem = chunk.TileSystem;

            // Find object within chunk.
            TileIndex index = chunk.FindTileIndexFromGameObject(attachedTransform);
            if (index != TileIndex.invalid) {
                TileData tile = tileSystem.GetTile(index);
                if (tile.brush == null) {
                    return;
                }

                Vector3 positionOffset, scaleOffset;
                Quaternion rotationOffset;

                // Convert matrix of attachment into local space of tile system.
                Matrix4x4 matrix = Matrix4x4.TRS(attachedTransform.localPosition, attachedTransform.localRotation, attachedTransform.localScale);
                if (attachedTransform.parent != tileSystem.transform) {
                    matrix = tileSystem.transform.worldToLocalMatrix * attachedTransform.parent.localToWorldMatrix * matrix;
                }
                tile.brush.CalculatePrefabOffset(tileSystem, index.row, index.column, tile.Rotation, matrix, out positionOffset, out rotationOffset, out scaleOffset);

                Undo.RecordObject(prefabTransform, TileLang.ParticularText("Action", "Use as Prefab Offset"));
                prefabTransform.localPosition = positionOffset;
                prefabTransform.localRotation = rotationOffset;
                prefabTransform.localScale = scaleOffset;

                EditorUtility.SetDirty(prefabTransform);

                // Should "Apply Prefab Transform" be enabled for brush?
                if (!tile.brush.applyPrefabTransform) {
                    if (EditorUtility.DisplayDialog(
                        TileLang.ParticularText("Action", "Apply Prefab Transform"),
                        string.Format(
                            /* 0: name of brush */
                            TileLang.Text("Prefab offset will have no effect unless 'Apply Prefab Transform' is enabled for brush.\n\nWould you like to enable this for '{0}'?"),
                            tile.brush.name
                        ),
                        TileLang.ParticularText("Action", "Yes"),
                        TileLang.ParticularText("Action", "No")
                    )) {
                        Undo.RecordObject(tile.brush, "");

                        tile.brush.applyPrefabTransform = true;
                        EditorUtility.SetDirty(tile.brush);

                        // Designer window should be repainted since the toggle may be exposed!
                        RotorzWindow.RepaintIfShown<DesignerWindow>();
                    }
                }
            }
        }

        public static bool UseAsPrefabOffset_Validate()
        {
            var attachedGameObject = GetPrefabInstance();
            if (attachedGameObject == null || attachedGameObject != Selection.activeGameObject) {
                return false;
            }

            var tileTransform = GetTileGameObject(attachedGameObject.transform);
            if (tileTransform == null) {
                return false;
            }

            var tileGameObject = tileTransform.gameObject;

            // Get chunk component.
            var chunk = tileTransform.parent.GetComponent<Chunk>();
            if (chunk.TileSystem == null) {
                return false;
            }

            // Find object within chunk.
            foreach (var tile in chunk.tiles) {
                if (tile != null && tile.gameObject == tileGameObject) {
                    return true;
                }
            }

            return false;
        }
    }
}
