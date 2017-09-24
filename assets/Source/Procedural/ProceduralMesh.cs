// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Procedural mesh component is automatically attached to chunks when procedural
    /// 2D tiles are painted.
    /// </summary>
    [AddComponentMenu(""), DisallowMultipleComponent, ExecuteInEditMode]
    public sealed class ProceduralMesh : MonoBehaviour
    {
        [SerializeField, HideInInspector, FormerlySerializedAs("_chunk")]
        private Chunk chunk;

        [SerializeField, HideInInspector, FormerlySerializedAs("_persist")]
        private bool persist;


        public Mesh mesh;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private bool updatedOnce;
        private bool hasNormals;


        /// <summary>
        /// Gets a value indicating if procedural mesh has been updated at least once.
        /// </summary>
        public bool HasUpdatedOnce {
            get { return this.updatedOnce; }
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isEditor && !Application.isPlaying) {
                this.InitialUpdateMesh();
            }
        }
#endif

        private void OnDestroy()
        {
            //!HACK: Workaround bug in Unity where delayed action of Object.Destroy
            //       is not respected when stopping play mode in-editor.
            InternalUtility.DestroyImmediate(this.mesh);
        }

        /// <summary>
        /// Update procedural mesh from tile data for first time.
        /// </summary>
        /// <remarks>
        /// <para>Will only update mesh if mesh has not previously been updated since it
        /// became active. This can be used in conjunction with <see cref="TileSystem.updateProceduralAtStart"/>
        /// to postpone generation of procedural tiles which can help to reduce level
        /// loading times.</para>
        /// </remarks>
        public void InitialUpdateMesh()
        {
            if (!this.updatedOnce) {
                this.UpdateMesh();
            }
        }

        private void InitializeMeshComponents(bool persist)
        {
            // Inherit layer from tile system.
            if (this.chunk != null && this.chunk.TileSystem != null) {
                this.gameObject.layer = this.chunk.TileSystem.gameObject.layer;
            }

            // Ensure that mesh filter is present.
            this.meshFilter = this.GetComponent<MeshFilter>();
            if (this.meshFilter == null) {
                this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
            }

            // Ensure that mesh renderer is present.
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            if (this.meshRenderer == null) {
                this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            }

            // Update sorting layers if needed.
            if (this.meshRenderer.sortingLayerID != this.chunk.TileSystem.SortingLayerID) {
                this.meshRenderer.sortingLayerID = this.chunk.TileSystem.SortingLayerID;
            }
            if (this.meshRenderer.sortingOrder != this.chunk.TileSystem.SortingOrder) {
                this.meshRenderer.sortingOrder = this.chunk.TileSystem.SortingOrder;
            }

            // Update persistance of existing mesh?
            if (this.mesh != null) {
                this.mesh.hideFlags = persist ? 0 : HideFlags.DontSave;
            }
            this.persist = persist;
        }

        /// <summary>
        /// Update procedural mesh from tile data.
        /// </summary>
        /// <param name="persist">A value of <c>true</c> indicates that generated mesh
        /// should be persisted at design time; <c>false</c> should always be specified
        /// at runtime.</param>
        public void UpdateMesh(bool persist = false)
        {
            // Attempt to recover if chunk reference is missing...
            if (this.chunk == null) {
                this.chunk = transform.parent.GetComponent<Chunk>();
            }
            if (this.chunk == null || this.chunk.tiles == null) {
                return;
            }

            this.chunk.ProceduralDirty = false;

            var tileSystem = this.chunk.TileSystem;
            if (tileSystem == null) {
                return;
            }

            this.InitializeMeshComponents(persist);
            this.updatedOnce = true;

            try {
                ProceduralMeshUpdater updater = ProceduralMeshUpdater.Instance;
                updater.Clear();

                // Fetch frequently used properties from tile system.
                int chunkWidth = tileSystem.ChunkWidth;
                int chunkHeight = tileSystem.ChunkHeight;

                TileData tile;

                Vector3 cellSize = tileSystem.CellSize;
                Vector3 origin;

                for (int row = 0, ti = 0; row < chunkHeight; ++row) {
                    for (int column = 0; column < chunkWidth; ++column, ++ti) {
                        tile = this.chunk.tiles[ti];
                        if (tile == null || !tile.Procedural || tile.tileset == null) {
                            continue;
                        }

                        // Calculate origin of first vertex of tile.
                        origin = new Vector3(
                            column * cellSize.x,
                            row * -cellSize.y,
                            0f
                        );

                        updater.AddFromTileIndex(origin, cellSize.x, cellSize.y, tile.tileset, tile);
                    }
                }

                // Only generate normals the first time.
                this.ApplyMesh(updater, tileSystem.addProceduralNormals);
                updater.Clear();
            }
            catch (IndexOutOfRangeException) {
                Debug.LogError(string.Format("Chunk size of '{0}' is too large for procedural mesh.", this.chunk.TileSystem.name), this.chunk.TileSystem);
            }
        }

        /// <summary>
        /// Update procedural mesh UVs from tile data.
        /// </summary>
        public void UpdateMeshUVs()
        {
            // Attempt to recover if chunk reference is missing...
            if (this.chunk == null || this.mesh == null) {
                this.UpdateMesh();
                return;
            }

            var tileSystem = this.chunk.TileSystem;
            if (tileSystem == null) {
                return;
            }

            var proceduralMeshUpdater = ProceduralMeshUpdater.Instance;
            proceduralMeshUpdater.Clear();

            TileData tile;

            for (int ti = 0; ti < this.chunk.tiles.Length; ++ti) {
                tile = this.chunk.tiles[ti];
                if (tile == null || !tile.Procedural || tile.tileset == null) {
                    continue;
                }

                proceduralMeshUpdater.UpdateUVsFromTileIndex(tile.tileset, tile);
            }

            proceduralMeshUpdater.UpdateMeshUVs(this.mesh);
            proceduralMeshUpdater.Clear();
        }

        private void ApplyMesh(ProceduralMeshUpdater updater, bool addProceduralNormals)
        {
            if (!updater.HasMeshData) {
                // No mesh was actually generated!
                if (this.mesh != null) {
                    this.mesh.Clear();
                }
                if (this.meshRenderer != null) {
                    this.meshRenderer.sharedMaterials = EmptyArray<Material>.Instance;
                }
                return;
            }

            if (this.mesh == null) {
                this.mesh = new Mesh();
                this.mesh.name = "RTS: Procedural Mesh";

                this.meshFilter.sharedMesh = this.mesh;

                // Mesh might need to be persisted!
                this.mesh.hideFlags = this.persist ? 0 : HideFlags.DontSave;
            }

            bool optimizeAtRuntime = (Application.isPlaying && !this.chunk.TileSystem.MarkProceduralDynamic);
            if (optimizeAtRuntime) {
                this.mesh.MarkDynamic();
            }

            if (this.hasNormals && !addProceduralNormals) {
                this.mesh.Clear(false);
                this.hasNormals = false;
            }

            updater.UpdateMesh(this.mesh, addProceduralNormals);
            this.hasNormals = addProceduralNormals;

            if (!this.meshRenderer.sharedMaterials.SequenceEqual(updater.Materials)) {
                this.meshRenderer.sharedMaterials = updater.Materials;
            }
        }
    }
}
