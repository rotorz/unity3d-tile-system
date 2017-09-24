// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Build event delegate.
    /// </summary>
    /// <param name="context">Object that describes context of build process.</param>
    public delegate void BuildEventDelegate(IBuildContext context);

    /// <summary>
    /// Progress delegate.
    /// </summary>
    /// <param name="title">Status title.</param>
    /// <param name="message">Status message.</param>
    /// <param name="progress">A number ranging between 0.0 and 1.0 representing
    /// the level of progress.</param>
    /// <returns>
    /// A value of <c>true</c> if processing should be cancelled; otherwise, a
    /// value of <c>false</c>.
    /// </returns>
    public delegate bool ProgressDelegate(string title, string message, float progress);


    internal sealed class TileSystemBuilder : IBuildContext
    {
        private TileSystem tileSystem;
        private GameObject tileSystemGameObject;

        private int combineChunkWidth;
        private int combineChunkHeight;


        /// <summary>
        /// List of generated output meshes.
        /// </summary>
        public List<Mesh> GeneratedMeshes = new List<Mesh>();


        public TileSystemBuilder()
        {
        }

        public TileSystemBuilder(ProgressDelegate progressHandler)
        {
            this.progressHandler = progressHandler;
        }


        #region IBuildContext implementation

        /// <inheritdoc/>
        TileSystem IBuildContext.TileSystem {
            get { return this.tileSystem; }
        }

        GameObject IBuildContext.TileSystemGameObject {
            get { return this.tileSystemGameObject; }
        }

        /// <inheritdoc/>
        BuildCombineMethod IBuildContext.Method {
            get { return this.tileSystem.combineMethod; }
        }

        /// <inheritdoc/>
        int IBuildContext.CombineChunkWidth {
            get { return this.combineChunkWidth; }
        }

        /// <inheritdoc/>
        int IBuildContext.CombineChunkHeight {
            get { return this.combineChunkHeight; }
        }

        #endregion


        #region Progress Feedback

        public ProgressDelegate progressHandler;
        private string progressTitle;
        private string progressStatus;

        private float currentTaskNumber;
        private float taskCount;
        private float taskNumberToPercentageFactor;


        /// <summary>
        /// Gets number of current task.
        /// </summary>
        public float Task {
            get { return this.currentTaskNumber; }
        }
        /// <summary>
        /// Gets the total number of tasks.
        /// </summary>
        public float TaskCount {
            get { return this.taskCount; }
        }


        public int CountTasks(TileSystem system)
        {
            int count = 0;

            // Optimize tile colliders?
            if (system.ReduceColliders.Active) {
                ++count;
            }

            // Prepare Map.
            count += system.RowCount;
            // Snap and smooth.
            count += system.RowCount;

            // Merge meshes into chunks.
            this.PrepareChunkData(system);
            count += this.combineChunkRows * this.combineChunkColumns;

            // Stripping.
            ++count;

            return count;
        }

        /// <summary>
        /// Update progress message and advance.
        /// </summary>
        /// <param name="message">New progress message.</param>
        /// <param name="tasksCompleted">Number of tasks that were completed.</param>
        /// <exception cref="System.OperationCanceledException">
        /// If operation is to be canceled.
        /// </exception>
        private void ReportProgress(string message, float tasksCompleted = 0f)
        {
            this.currentTaskNumber += tasksCompleted;

            if (this.progressHandler != null) {
                this.progressStatus = message;
                if (this.progressHandler(this.progressTitle, message, this.currentTaskNumber * this.taskNumberToPercentageFactor)) {
                    throw new OperationCanceledException();
                }
            }
        }

        /// <summary>
        /// Update progress message and advance.
        /// </summary>
        /// <param name="tasksCompleted">Number of tasks that were completed.</param>
        /// <exception cref="System.OperationCanceledException">
        /// If operation is to be canceled.
        /// </exception>
        private void ReportProgress(float tasksCompleted = 1f)
        {
            this.currentTaskNumber += tasksCompleted;

            if (this.progressHandler != null) {
                if (this.progressHandler(this.progressTitle, this.progressStatus, this.currentTaskNumber * this.taskNumberToPercentageFactor)) {
                    throw new OperationCanceledException();
                }
            }
        }

        #endregion


        #region Builder

        private MeshProcessor meshProcessor = new MeshProcessor();


        internal void SetContext(TileSystem system)
        {
            this.tileSystem = system;
            this.tileSystemGameObject = system.gameObject;
        }

        public void Build(TileSystem system)
        {
            // Notes for multi-threading in the future:
            //   1. Prepare map using multiple processors.
            //   2. Wait until map is prepared.
            //   3. Perform snap and smooth using multiple processors by dividing map
            //      into buckets.
            //   4. Wait until buckets have been processed.
            //   5. Perform snap and smooth between edges of buckets (stitching).
            //   6. Merge meshes using multiple processors.
            //   7. Wait until finished.

            this.progressTitle = string.Format(
                /* 0: name of tile system */
                TileLang.Text("Building tile system '{0}'..."),
                system.name
            );
            this.SetContext(system);

            // Strip build paths from optimized version of tile system.
            system.LastBuildPrefabPath = "";
            system.LastBuildDataPath = "";

            this.GeneratedMeshes.Clear();

            // Prepare chunk data and count tasks
            this.currentTaskNumber = 0;
            this.taskCount = this.CountTasks(system);
            this.taskNumberToPercentageFactor = 1f / (float)this.taskCount;

            if (system.ReduceColliders.Active) {
                this.ReportProgress(TileLang.Text("Optimizing tile colliders..."));
                this.ReduceTileColliders();
            }

            if (system.combineMethod != BuildCombineMethod.None) {
                // Prepare mesh data from tile meshes
                this.ReportProgress(TileLang.Text("Preparing tile meshes..."));
                this.PrepareMap();

                // Perform vertex snapping and smoothing
                this.ReportProgress(TileLang.Text("Snapping vertices..."));
                this.PerformSnapAndSmooth();

                // Merge meshes into chunks
                this.ReportProgress(TileLang.Text("Finalizing chunks..."));
                this.PerformMergeIntoChunks();
            }

            this.OptimizeProceduralMeshes(system);

            // Apply stripping to tile system!
            this.ReportProgress(TileLang.Text("Applying stripping rules..."));
            StrippingUtility.ApplyStripping(system);
            this.ReportProgress();

            // Indicates that tile system has been built
            system.isBuilt = true;
        }

        #endregion


        #region Procedural Meshes

        private void OptimizeProceduralMeshes(TileSystem system)
        {
            // StripBrushReferences - Can still be stripped because they are not needed
            //                        to paint procedural meshes :-)

            if (system.pregenerateProcedural) {
                this.ReportProgress(TileLang.Text("Generating procedural tiles..."));
                this.PregenerateProceduralMeshes(system);
            }
            else {
                // Find out if tile system contains any procedural tiles.
                bool containsProceduralTiles = this.DoesContainProceduralTiles(system.Chunks);

                // Automatically downgrade stripping options if needed.
                if (containsProceduralTiles) {
                    // Only proceed if stripping options will actually be reduced.
                    // Note: For more accurate log message!
                    if (system.StripSystemComponent || system.StripChunkMap || system.StripChunks || system.StripTileData) {// || system.StripBrushReferences)
                        this.ReduceStrippingOptionsForProceduralMeshGeneration(system);
                    }
                }
                else {
                    // Remove redundant procedural mesh components.
                    this.DestroyGeneratedProceduralMeshes(system);
                }
            }

        }

        private void PregenerateProceduralMeshes(TileSystem system)
        {
            // Generate procedural meshes on a per chunk basis
            foreach (Chunk chunk in system.Chunks) {
                if (chunk != null) {
                    ProceduralMesh proceduralMesh = chunk.ProceduralMesh;
                    if (proceduralMesh != null) {
                        // Note: When tile data is stripped the procedural mesh cannot
                        // be updated again - it becomes a static mesh.

                        // Update procedural mesh from tile data
                        proceduralMesh.UpdateMesh(true);

                        // Generate second set of UVs?
                        //!TODO: `Unwrapping` requires `UnityEditor` reference, can this be avoided?
                        if (this.tileSystem.GenerateSecondUVs && this.tileSystem.StripTileData) {
                            this.GenerateSecondUVs(proceduralMesh.mesh);
                        }

                        // Keep track of all generated output meshes!
                        this.GeneratedMeshes.Add(proceduralMesh.mesh);
                    }
                }
                this.ReportProgress();
            }
        }

        private bool DoesContainProceduralTiles(Chunk[] chunks)
        {
            if (chunks != null) {
                foreach (var chunk in chunks) {
                    if (chunk == null || chunk.tiles == null) {
                        continue;
                    }

                    foreach (var tile in chunk.tiles) {
                        if (tile == null || tile.Empty) {
                            continue;
                        }
                        if (tile.Procedural) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void DestroyGeneratedProceduralMeshes(TileSystem system)
        {
            foreach (var proceduralMesh in system.GetComponentsInChildren<ProceduralMesh>()) {
                var transform = proceduralMesh.transform;

                Object.DestroyImmediate(transform.GetComponent<MeshFilter>());
                Object.DestroyImmediate(transform.GetComponent<MeshRenderer>());

                Object.DestroyImmediate(proceduralMesh.mesh);

                Object.DestroyImmediate(proceduralMesh);

                StrippingUtility.StripEmptyGameObject(transform);
            }
        }

        private void ReduceStrippingOptionsForProceduralMeshGeneration(TileSystem system)
        {
            system.StrippingPreset = StrippingPreset.Custom;
            system.StripSystemComponent = false;
            system.StripChunkMap = false;
            system.StripChunks = false;
            system.StripTileData = false;
            //system.StripBrushReferences = false;

            // Log this decision to console to assist with debugging
            string message = string.Format(
                /* 0: name of tile system */
                TileLang.Text("Stripping options were reduced for '{0}' so that procedural tiles can be generated at runtime. See documentation for 'Pre-generate Procedural'."),
                system.name
            );
            Debug.Log(message, system);
        }

        #endregion


        #region Colliders

        private void ReduceTileColliders()
        {
            ReduceColliderUtility.Optimize(this.tileSystem);
        }

        #endregion


        #region Chunks

        private int combineChunkRows;
        private int combineChunkColumns;


        private void PrepareChunkData(TileSystem system)
        {
            switch (system.combineMethod) {
                case BuildCombineMethod.CustomChunkInTiles:
                    this.combineChunkWidth = system.combineChunkWidth;
                    this.combineChunkHeight = system.combineChunkHeight;
                    break;

                // Note: Still need correct number of chunk rows and columns
                //       for non-mesh combine building!
                case BuildCombineMethod.None:
                case BuildCombineMethod.ByChunk:
                    this.combineChunkWidth = system.ChunkWidth;
                    this.combineChunkHeight = system.ChunkHeight;
                    break;

                case BuildCombineMethod.ByTileSystem:
                    this.combineChunkWidth = system.ColumnCount;
                    this.combineChunkHeight = system.RowCount;
                    break;
            }

            this.combineChunkRows = system.RowCount / this.combineChunkHeight + (system.RowCount % this.combineChunkHeight > 0 ? 1 : 0);
            this.combineChunkColumns = system.ColumnCount / this.combineChunkWidth + (system.ColumnCount % this.combineChunkWidth > 0 ? 1 : 0);
        }


        private MeshData[,] combineChunkMeshes;

        private void MergeChunkMeshes()
        {
            MeshProcessor processor = this.meshProcessor;
            TileSystem system = this.tileSystem;

            Matrix4x4 correctionMatrix = system.transform.worldToLocalMatrix;

            this.combineChunkMeshes = new MeshData[this.combineChunkRows, this.combineChunkColumns];
            for (int chunkRow = 0; chunkRow < this.combineChunkRows; ++chunkRow) {
                for (int chunkColumn = 0; chunkColumn < this.combineChunkColumns; ++chunkColumn) {
                    MeshData chunkMesh = null;

                    int startRow = chunkRow * this.combineChunkHeight;
                    int startColumn = chunkColumn * this.combineChunkWidth;
                    int endRow = Mathf.Min(system.RowCount, startRow + this.combineChunkHeight);
                    int endColumn = Mathf.Min(system.ColumnCount, startColumn + this.combineChunkWidth);

                    for (int row = startRow; row < endRow; ++row) {
                        for (int column = startColumn; column < endColumn; ++column) {
                            MeshData tileMesh = this.map[row, column];
                            if (tileMesh == null) {
                                continue;
                            }

                            if (chunkMesh == null) {
                                chunkMesh = tileMesh;
                                processor.Mount(chunkMesh);
                            }
                            else {
                                processor.AppendMesh(tileMesh);
                            }
                        }
                    }

                    if (chunkMesh != null) {
                        processor.Apply();
                        this.combineChunkMeshes[chunkRow, chunkColumn] = chunkMesh;

                        chunkMesh.ApplyTransform(correctionMatrix);
                        if (chunkMesh.HasTangents) {
                            chunkMesh.CalculateTangents();
                        }
                    }

                    this.ReportProgress();
                }
            }

            this.map = null;
        }

        private void GenerateSecondUVs(Mesh mesh)
        {
            UnwrapParam unwrapParam = new UnwrapParam();
            unwrapParam.hardAngle = this.tileSystem.SecondUVsHardAngle;
            unwrapParam.packMargin = this.tileSystem.SecondUVsPackMargin;
            unwrapParam.angleError = this.tileSystem.SecondUVsAngleError;
            unwrapParam.areaError = this.tileSystem.SecondUVsAreaError;
            Unwrapping.GenerateSecondaryUVSet(mesh, unwrapParam);
        }

        private void SaveChunkMeshToGameObject(GameObject go, MeshData chunkMesh)
        {
            var filter = go.GetComponent<MeshFilter>();
            if (filter == null) {
                filter = go.AddComponent<MeshFilter>();
            }

            var outputMesh = chunkMesh.ToMesh();

            // Keep track of all generated output meshes!
            this.GeneratedMeshes.Add(outputMesh);

            // Generate second set of UVs?
            //!TODO: `Unwrapping` requires `UnityEditor` reference, can this be avoided?
            if (this.tileSystem.GenerateSecondUVs) {
                this.GenerateSecondUVs(outputMesh);
            }

            filter.sharedMesh = outputMesh;

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) {
                renderer = go.AddComponent<MeshRenderer>();
            }

            renderer.sharedMaterials = chunkMesh.Materials.ToArray();

            // Mark combined mesh as static
            go.isStatic = true;
        }

        // Same as `SaveChunkMeshToGameObject` but for individual mesh per material
        private void SaveChunkMeshToGameObjectPerMaterial(GameObject go, MeshData chunkMesh)
        {
            var chunkParentTransform = go.transform;

            Material[] outputMaterials = chunkMesh.Materials.ToArray();
            Mesh[] outputMeshes = chunkMesh.ToMeshPerMaterial(this.meshProcessor);

            for (int submesh = 0; submesh < outputMeshes.Length; ++submesh) {
                Mesh outputMesh = outputMeshes[submesh];

                // Keep track of all generated output meshes!
                this.GeneratedMeshes.Add(outputMesh);

                // Generate second set of UVs?
                //!TODO: `Unwrapping` requires `UnityEditor` reference, can this be avoided?
                if (this.tileSystem.GenerateSecondUVs) {
                    this.GenerateSecondUVs(outputMesh);
                }

                // Generate mesh components
                var submeshGO = new GameObject("_" + submesh);
                submeshGO.transform.SetParent(chunkParentTransform, false);

                // Mark combined mesh as static
                submeshGO.isStatic = true;

                var filter = submeshGO.AddComponent<MeshFilter>();
                filter.sharedMesh = outputMesh;

                var renderer = submeshGO.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = outputMaterials[submesh];
            }

            // Mark combined mesh as static
            go.isStatic = true;
        }

        private void GenerateChunkGameObjects()
        {
            bool storeInOriginalChunk = (this.combineChunkWidth == this.tileSystem.ChunkWidth && this.combineChunkHeight == this.tileSystem.ChunkHeight);

            var tileSystemTransform = this.tileSystem.transform;

            for (int chunkRow = 0; chunkRow < this.combineChunkRows; ++chunkRow) {
                for (int chunkColumn = 0; chunkColumn < this.combineChunkColumns; ++chunkColumn) {
                    MeshData chunkMesh = this.combineChunkMeshes[chunkRow, chunkColumn];
                    if (chunkMesh == null) {
                        continue;
                    }

                    GameObject mergedChunkGO;
                    if (storeInOriginalChunk) {
                        mergedChunkGO = this.tileSystem.GetChunk(chunkRow, chunkColumn).gameObject;
                    }
                    else {
                        mergedChunkGO = new GameObject("_merge");
                        mergedChunkGO.transform.SetParent(tileSystemTransform, false);
                    }

                    if (this.tileSystem.combineIntoSubmeshes) {
                        this.SaveChunkMeshToGameObject(mergedChunkGO, chunkMesh);
                    }
                    else {
                        this.SaveChunkMeshToGameObjectPerMaterial(mergedChunkGO, chunkMesh);
                    }
                }
            }
        }

        private void PerformMergeIntoChunks()
        {
            this.MergeChunkMeshes();
            this.GenerateChunkGameObjects();

            this.combineChunkMeshes = null;
        }

        #endregion


        #region Tile Mesh Map

        private MeshData[,] map;

        private void PrepareMap()
        {
            this.map = new MeshData[this.tileSystem.RowCount, this.tileSystem.ColumnCount];

            for (int row = 0; row < this.tileSystem.RowCount; ++row) {
                for (int column = 0; column < this.tileSystem.ColumnCount; ++column) {
                    TileData tile = this.tileSystem.GetTile(row, column);
                    if (tile == null || tile.gameObject == null || tile.brush == null || !tile.brush.Static) {
                        continue;
                    }

                    MeshFilter[] filters = tile.gameObject.GetComponentsInChildren<MeshFilter>();
                    if (filters.Length == 0) {
                        continue;
                    }

                    // Merge tile meshes into a single mesh
                    MeshData meshData = new MeshData();
                    this.meshProcessor.Mount(meshData);

                    foreach (MeshFilter filter in filters) {
                        MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                        if (renderer == null || !renderer.enabled || filter.sharedMesh == null) {
                            continue;
                        }

                        this.meshProcessor.AppendMesh(filter.transform, filter, renderer);

                        // Renderer and filter components are no longer needed
                        Object.DestroyImmediate(renderer);
                        Object.DestroyImmediate(filter);
                    }

                    // Strip empty tile!
                    if (this.tileSystem.StripCombinedEmptyObjects) {
                        StrippingUtility.StripEmptyGameObject(tile.gameObject.transform);
                    }

                    this.meshProcessor.Apply();
                    if (!meshData.IsEmpty) {
                        meshData.Smooth = tile.brush.Smooth;
                        meshData.CopyOriginalNormals();

                        this.map[row, column] = meshData;
                    }
                }

                this.ReportProgress();
            }
        }

        #endregion


        #region Clusters

        private List<ClusterVertex> clusterVertices = new List<ClusterVertex>();

        private void AddToCluster(MeshData data)
        {
            ClusterVertex clusterVertex = new ClusterVertex(data, 0);
            for (clusterVertex.Index = 0; clusterVertex.Index < data.Vertices.Length; ++clusterVertex.Index) {
                this.clusterVertices.Add(clusterVertex);
            }
        }

        private void GetNonSmoothCluster(int startRow, int startColumn)
        {
            this.clusterVertices.Clear();

            int endRow = Mathf.Min(this.tileSystem.RowCount, startRow + 2);
            int endColumn = Mathf.Min(this.tileSystem.ColumnCount, startColumn + 2);

            for (int row = startRow; row < endRow; ++row)
                for (int column = startColumn; column < endColumn; ++column) {
                    MeshData data = this.map[row, column];
                    if (data == null || data.Smooth) {
                        continue;
                    }

                    this.AddToCluster(data);
                }
        }

        private MeshData GetSmoothCluster(int startRow, int startColumn)
        {
            this.clusterVertices.Clear();

            MeshData startData = this.map[startRow, startColumn];
            if (startData == null) {
                return null;
            }

            int endRow = Mathf.Min(this.tileSystem.RowCount, startRow + 2);
            int endColumn = Mathf.Min(this.tileSystem.ColumnCount, startColumn + 2);

            startRow = Mathf.Max(0, startRow - 1);
            startColumn = Mathf.Max(0, startColumn - 1);

            for (int row = startRow; row < endRow; ++row)
                for (int column = startColumn; column < endColumn; ++column) {
                    MeshData data = this.map[row, column];
                    if (data == null || !data.Smooth) {
                        continue;
                    }

                    this.AddToCluster(data);
                }

            return startData;
        }

        private List<ClusterVertex> snappedVertices = new List<ClusterVertex>();

        private void FindSnappedVertices(int offset, float threshold)
        {
            Vector3 a = this.clusterVertices[offset].Position;

            ClusterVertex v = this.clusterVertices[offset];

            this.snappedVertices.Clear();
            this.snappedVertices.Add(v);

            int clusterCount = this.clusterVertices.Count;

            v.Data = null;
            this.clusterVertices[offset] = v;

            for (++offset; offset < clusterCount; ++offset) {
                v = this.clusterVertices[offset];
                if (v.Data == null) {
                    continue;
                }

                if (Vector3.Distance(a, v.Position) <= threshold) {
                    this.snappedVertices.Add(v);

                    // Do not attempt to build additional vertex groups using `v` as seed!
                    v.Data = null;
                    this.clusterVertices[offset] = v;
                }
            }
        }

        private void ApplyVertexSnappingToCluster(float threshold)
        {
            int clusterCount = this.clusterVertices.Count;
            for (int i = 0; i < clusterCount; ++i) {
                if (this.clusterVertices[i].Data == null) {
                    continue;
                }

                this.FindSnappedVertices(i, threshold);
                if (this.snappedVertices.Count > 1) {
                    // Snap vertex position
                    Vector3 midpoint = Vector3.zero;
                    foreach (ClusterVertex v in this.snappedVertices) {
                        midpoint += v.Position;
                    }
                    midpoint /= this.snappedVertices.Count;

                    foreach (ClusterVertex v in this.snappedVertices) {
                        v.Data.Vertices[v.Index] = midpoint;
                    }
                }
            }
        }

        private void ApplySmoothingToCluster(MeshData startData, float threshold)
        {
            int clusterCount = this.clusterVertices.Count;

            Vector4 originalNormal;

            for (int i = 0; i < clusterCount; ++i) {
                if (this.clusterVertices[i].Data == null) {
                    continue;
                }

                this.FindSnappedVertices(i, threshold);
                if (this.snappedVertices.Count > 1) {
                    // Snap vertex position
                    Vector3 midpoint = Vector3.zero;
                    Vector3 normal = Vector3.zero;

                    foreach (ClusterVertex v in this.snappedVertices) {
                        midpoint += v.Position;
                        originalNormal = v.OriginalNormal;

                        normal.x += originalNormal.x;
                        normal.y += originalNormal.y;
                        normal.z += originalNormal.z;
                    }

                    midpoint /= this.snappedVertices.Count;
                    normal = normal.normalized;

                    // Smooth vertex normal
                    foreach (ClusterVertex v in this.snappedVertices) {
                        // Snapping should happen anyhow!
                        v.Data.Vertices[v.Index] = midpoint;
                        // Only perform smoothing on start data!
                        if (v.Data == startData) {
                            v.Data.Normals[v.Index] = normal;
                        }
                    }
                }
            }
        }

        private void PerformSnapAndSmooth()
        {
            float threshold = this.tileSystem.vertexSnapThreshold;
            bool staticSnapping = this.tileSystem.staticVertexSnapping;

            int endRow = this.tileSystem.RowCount - 1;
            int endColumn = this.tileSystem.ColumnCount - 1;

            for (int row = 0; row < endRow; ++row) {
                for (int column = 0; column < endColumn; ++column) {
                    if (staticSnapping) {
                        this.GetNonSmoothCluster(row, column);
                        this.ApplyVertexSnappingToCluster(threshold);
                    }

                    MeshData startData = this.GetSmoothCluster(row, column);
                    if (startData != null) {
                        this.ApplySmoothingToCluster(startData, threshold);
                    }
                }

                this.ReportProgress();
            }
        }

        #endregion
    }
}
