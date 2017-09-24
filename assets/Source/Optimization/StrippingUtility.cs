// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Linq;
using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Functionality for stripping unwanted aspects of a tile system.
    /// </summary>
    public static class StrippingUtility
    {
        /// <summary>
        /// Apply runtime stripping options to a tile system.
        /// </summary>
        /// <remarks>
        /// <para>At runtime only a basic level of stripping will be automatically applied
        /// to tile system upon <c>Awake</c> when runtime stripping is enabled. Tile systems
        /// should be optimised to take full advantage of the provided stripping capabilities.</para>
        /// </remarks>
        /// <param name="tileSystem">Tile system.</param>
        public static void ApplyRuntimeStripping(TileSystem tileSystem)
        {
            if (tileSystem.chunks != null) {
                // Does tile data need to be stripped?
                if (tileSystem.StripSystemComponent || tileSystem.StripChunkMap || tileSystem.StripTileData) {
                    foreach (var chunk in tileSystem.chunks) {
                        if (chunk == null) {
                            continue;
                        }

                        chunk.tiles = null;
                        InternalUtility.Destroy(chunk);
                    }

                    // Tile system is no longer editable.
                    tileSystem.isEditable = false;
                }
                // If not, should brush references be stripped from tile data?
                else if (tileSystem.StripBrushReferences) {
                    foreach (var chunk in tileSystem.chunks) {
                        if (chunk == null || chunk.tiles == null) {
                            continue;
                        }

                        foreach (var tile in chunk.tiles) {
                            if (tile != null) {
                                tile.brush = null;
                            }
                        }
                    }
                }

                // Is chunk map to be stripped?
                if (tileSystem.StripChunkMap) {
                    tileSystem.chunks = null;

                    // Tile system is no longer editable.
                    tileSystem.isEditable = false;
                }
            }

            // Should this tile system component be stripped?
            if (tileSystem.StripSystemComponent) {
                InternalUtility.Destroy(tileSystem);
            }

            // Runtime stripping has already been applied!
            tileSystem.applyRuntimeStripping = false;
        }

        /// <summary>
        /// Reparent children of game object to another game object.
        /// </summary>
        /// <param name="parent">Transform of game object.</param>
        /// <param name="newParent">Transform of new parent game object.</param>
        private static void ReparentChildren(Transform parent, Transform newParent)
        {
            int i = parent.childCount;
            while (i-- > 0) {
                var child = parent.GetChild(0);
                if (child == null) {
                    continue;
                }

                child.SetParent(newParent);
            }
        }

        /// <summary>
        /// Strip chunk game objects from tile system and reparent tile objects into game
        /// object of tile system.
        /// </summary>
        /// <param name="tileSystem">Tile system.</param>
        private static void StripChunks(TileSystem tileSystem)
        {
            if (tileSystem.chunks != null) {
                foreach (var chunk in tileSystem.chunks) {
                    if (chunk == null) {
                        continue;
                    }

                    ReparentChildren(chunk.transform, tileSystem.transform);

                    // Dereference tile data and destroy associated game object.
                    chunk.tiles = null;
                    InternalUtility.Destroy(chunk.gameObject);
                }

                // Clear chunk map.
                tileSystem.chunks = null;
            }

            // Tile system is no longer editable.
            tileSystem.isEditable = false;
        }

        /// <summary>
        /// Strip game object when it contains no components and has no children.
        /// </summary>
        /// <param name="obj">Transform of game object to consider stripping.</param>
        /// <returns>
        /// A value of <c>true</c> if game object was stripped; otherwise <c>false</c>.
        /// </returns>
        public static bool StripEmptyGameObject(Transform obj)
        {
            if (obj.childCount == 0) {
                var components = obj.GetComponents<Component>();
                if (components.Length == 1 && components[0] is Transform) {
                    InternalUtility.Destroy(obj.gameObject);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Recursively strip empty game objects.
        /// </summary>
        /// <param name="root">Transform of root game object.</param>
        private static void StripEmptyGameObjectsRecursive(Transform root)
        {
            // Perform stripping on any child objects first.
            var children = root.OfType<Transform>().ToArray();
            foreach (var child in children) {
                StripEmptyGameObjectsRecursive(child);
            }

            StripEmptyGameObject(root);
        }

        /// <summary>
        /// Remove empty chunk game objects.
        /// </summary>
        /// <param name="tileSystem">Tile system.</param>
        private static void StripEmptyChunks(TileSystem tileSystem)
        {
            if (tileSystem.chunks == null) {
                return;
            }

            Transform chunkTransform;
            Component[] components;
            int extraCount;

            for (int i = 0; i < tileSystem.chunks.Length; ++i) {
                if (tileSystem.chunks[i] == null) {
                    continue;
                }

                chunkTransform = tileSystem.chunks[i].transform;

                // Strip chunk if it doesn't contain any child game objects and if it
                // doesn't contain extra components.
                if (chunkTransform.childCount == 0) {
                    components = chunkTransform.GetComponents<Component>();

                    // Count extra components (other than Transform and Chunk).
                    extraCount = components.Length;
                    for (int j = 0; j < components.Length; ++j) {
                        if (components[j] is Transform || components[j] is Chunk) {
                            --extraCount;
                        }
                    }

                    // Chunk can be stripped if there are no extra components.
                    if (extraCount == 0) {
                        InternalUtility.Destroy(chunkTransform.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Strip plop instance and group components for tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        private static void StripPlopComponents(TileSystem system)
        {
            // Strip `PlopGroup` components from tile system.
            foreach (var plopGroup in system.GetComponentsInChildren<PlopGroup>()) {
                InternalUtility.Destroy(plopGroup);
            }

            // Strip `PlopInstance` components which are associated from tile system.
            foreach (var plopInstance in Resources.FindObjectsOfTypeAll<PlopInstance>()) {
                if (plopInstance.Owner == system) {
                    InternalUtility.Destroy(plopInstance);
                }
            }
        }

        /// <summary>
        /// Strip brush references from plop instances.
        /// </summary>
        /// <param name="system">Tile system.</param>
        private static void StripBrushReferencesFromPlopComponents(TileSystem system)
        {
            foreach (var plopInstance in Resources.FindObjectsOfTypeAll<PlopInstance>()) {
                if (plopInstance.Owner == system) {
                    plopInstance.Brush = null;
                }
            }
        }

        /// <summary>
        /// Strip unwanted aspects of tile system with specified stripping options.
        /// </summary>
        /// <remarks>
        /// <para>Chunks cannot be stripped from tile system when combine method is set
        /// to <see cref="BuildCombineMethod.ByChunk"/>.</para>
        /// </remarks>
        /// <param name="tileSystem">Tile system.</param>
        public static void ApplyStripping(TileSystem tileSystem)
        {
            // Strip chunks from tile system?
            if (tileSystem.StripChunks && tileSystem.combineMethod != BuildCombineMethod.ByChunk) {
                StripChunks(tileSystem);
            }

            if (tileSystem.StripPlopComponents) {
                StripPlopComponents(tileSystem);
            }
            else if (tileSystem.StripBrushReferences) {
                StripBrushReferencesFromPlopComponents(tileSystem);
            }

            // Strip empty objects?
            if (tileSystem.StripEmptyObjects) {
                var children = tileSystem.transform.OfType<Transform>().ToArray();
                foreach (var child in children) {
                    StripEmptyGameObjectsRecursive(child);
                }
            }

            // Strip empty chunks?
            if (tileSystem.StripEmptyChunks) {
                StripEmptyChunks(tileSystem);
            }

            // Finally, apply runtime stripping.
            ApplyRuntimeStripping(tileSystem);
        }
    }
}
