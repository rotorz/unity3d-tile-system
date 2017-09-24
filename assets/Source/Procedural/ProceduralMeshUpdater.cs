// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using UnityEngine;

namespace Rotorz.Tile
{
    internal sealed class ProceduralMeshUpdater
    {
        #region Singleton

        private static ProceduralMeshUpdater s_Instance;

        /// <summary>
        /// Gets the one and only procedural mesh updater instance.
        /// </summary>
        public static ProceduralMeshUpdater Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = new ProceduralMeshUpdater();
                }
                return s_Instance;
            }
        }

        private ProceduralMeshUpdater()
        {
        }

        #endregion


        private readonly List<Vector3> vertexBuffer = new List<Vector3>();
        private readonly List<Vector2> uvBuffer = new List<Vector2>();
        private readonly List<Vector3> normalBuffer = new List<Vector3>();

        private List<Material> materials = new List<Material>();
        private List<List<int>> triBuffer = new List<List<int>>();
        private Vector2[] uvQuadBuffer = new Vector2[4];
        private int submeshCount;



        /// <summary>
        /// Gets a value indicating whether updater has generated mesh data.
        /// </summary>
        public bool HasMeshData {
            get { return this.materials.Count != 0; }
        }

        /// <summary>
        /// Gets array of materials that can be assigned to mesh renderer.
        /// </summary>
        /// <remarks>
        /// <para>Remains <c>null</c> until <see cref="UpdateMesh"/> is called.</para>
        /// </remarks>
        public Material[] Materials { get; private set; }

        /// <summary>
        /// Clear procedural mesh updater ready to begin.
        /// </summary>
        public void Clear()
        {
            this.vertexBuffer.Clear();
            this.uvBuffer.Clear();
            this.normalBuffer.Clear();

            this.submeshCount = 0;

            this.materials.Clear();

            this.Materials = null;
        }

        /// <summary>
        /// Update mesh from procedurally generated data.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <param name="addNormals">Indicates if normals should be added for mesh.</param>
        public void UpdateMesh(Mesh mesh, bool addNormals = false)
        {
            mesh.Clear();

            mesh.SetVertices(this.vertexBuffer);
            mesh.SetUVs(0, this.uvBuffer);

            mesh.subMeshCount = this.submeshCount;

            for (int i = 0; i < this.submeshCount; ++i) {
                mesh.SetTriangles(this.triBuffer[i], i);
            }

            if (addNormals) {
                // Only prepare normal buffer if it is actually going to be used!
                var normal = new Vector3(0f, 0f, -1f);
                for (int i = 0; i < this.vertexBuffer.Count; ++i) {
                    this.normalBuffer.Add(normal);
                }
                mesh.SetNormals(this.normalBuffer);
            }

            // If possible recycle previously returned array
            if (this.Materials != null && this.materials.Count == this.Materials.Length) {
                this.materials.CopyTo(this.Materials);
            }
            else {
                this.Materials = this.materials.ToArray();
            }
        }

        /// <summary>
        /// Update mesh UVs from procedurally generated data.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public void UpdateMeshUVs(Mesh mesh)
        {
            mesh.SetUVs(0, this.uvBuffer);
        }

        /*
        /// <summary>
        /// Add vertex to mesh.
        /// </summary>
        /// <param name="position">Position of vertex in local space relative to
        /// tile system.</param>
        /// <param name="uv">UV coordinate.</param>
        /// <returns>
        /// Zero-based index of vertex that can be used to form triangles.
        /// </returns>
        public int AddVertex(Vector3 position, Vector2 uv)
        {
            this.vertexBuffer[this.vertexIndex] = position;
            this.uvBuffer[this.vertexIndex] = uv;

            return this.vertexIndex++;
        }

        /// <summary>
        /// Add triangle to mesh.
        /// </summary>
        /// <param name="vert1">Zero-based index of first vertex.</param>
        /// <param name="vert2">Zero-based index of second vertex.</param>
        /// <param name="vert3">Zero-based index of third vertex.</param>
        /// <param name="submesh">Zero-based index of submesh.</param>
        public void AddTriangle(int vert1, int vert2, int vert3, int submesh)
        {
            List<int> tris = this.triBuffer[submesh];
            tris.Add(vert1);
            tris.Add(vert2);
            tris.Add(vert3);
        }
        */

        /// <summary>
        /// Gets index of submesh for a given material.
        /// </summary>
        /// <remarks>
        /// <para>Submesh is automatically added when one does not already exist for the
        /// specified material.</para>
        /// </remarks>
        /// <param name="material">The material.</param>
        /// <returns>
        /// Zero-based index of submesh.
        /// </returns>
        public int GetSubmeshIndex(Material material)
        {
            int index = this.materials.IndexOf(material);
            if (index == -1) {
                index = this.materials.Count;
                this.materials.Add(material);

                if (this.submeshCount >= this.triBuffer.Count) {
                    this.triBuffer.Add(new List<int>());
                }
                else {
                    this.triBuffer[this.submeshCount].Clear();
                }

                ++this.submeshCount;
            }
            return index;
        }

        /// <summary>
        /// Update procedural tile using specified tile index.
        /// </summary>
        /// <remarks>
        /// <para>Outputs a quad (2 triangles and 4 vertices) with UV mapping coordinates
        /// that are calculated for specified tile.</para>
        /// </remarks>
        /// <param name="origin">Origin vertex of output tile.</param>
        /// <param name="tileSizeX">Width of tile.</param>
        /// <param name="tileSizeY">Height of tile.</param>
        /// <param name="tileset">The tileset.</param>
        /// <param name="tile">Tile data.</param>
        public void AddFromTileIndex(Vector3 origin, float tileSizeX, float tileSizeY, Tileset tileset, TileData tile)
        {
            // Calculate index of row and column of brush in tileset
            float atlasColumn = (float)(tile.tilesetIndex % tileset.columnCount);
            float atlasRow = (float)(tile.tilesetIndex / tileset.columnCount);

            float x = tileset.borderU + tileset.tileIncrementU * atlasColumn;
            float y = tileset.borderV + tileset.tileIncrementV * atlasRow;

            // Generate vertices for tile quad
            int firstVertexIndex = this.vertexBuffer.Count;
            int uvIndex = tile.Rotation;

            // First vertex
            this.vertexBuffer.Add(new Vector3(origin.x, origin.y - tileSizeY, 0f));
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.deltaU, 1f - (y + tileset.tileHeightUV - tileset.deltaV));
            if (++uvIndex == 4) {
                uvIndex = 0;
            }

            // Second vertex
            this.vertexBuffer.Add(new Vector3(origin.x, origin.y, 0f));
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.deltaU, 1f - (y + tileset.deltaV));
            if (++uvIndex == 4) {
                uvIndex = 0;
            }

            // Third vertex
            this.vertexBuffer.Add(new Vector3(origin.x + tileSizeX, origin.y, 0f));
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.tileWidthUV - tileset.deltaU, 1f - (y + tileset.deltaV));
            if (++uvIndex == 4) {
                uvIndex = 0;
            }

            // Fourth vertex
            this.vertexBuffer.Add(new Vector3(origin.x + tileSizeX, origin.y - tileSizeY, 0f));
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.tileWidthUV - tileset.deltaU, 1f - (y + tileset.tileHeightUV - tileset.deltaV));

            // Copy UVs into output data.
            this.uvBuffer.Add(this.uvQuadBuffer[0]);
            this.uvBuffer.Add(this.uvQuadBuffer[1]);
            this.uvBuffer.Add(this.uvQuadBuffer[2]);
            this.uvBuffer.Add(this.uvQuadBuffer[3]);

            // Generate triangles for tile quad
            int submeshIndex = this.GetSubmeshIndex(tileset.AtlasMaterial);
            List<int> tris = this.triBuffer[submeshIndex];

            // First triangle
            tris.Add(firstVertexIndex);
            tris.Add(firstVertexIndex + 1);
            tris.Add(firstVertexIndex + 2);
            // Second triangle
            tris.Add(firstVertexIndex + 2);
            tris.Add(firstVertexIndex + 3);
            tris.Add(firstVertexIndex);
        }

        /// <summary>
        /// Update procedural tile using specified tile index.
        /// </summary>
        /// <remarks>
        /// <para>Outputs a quad (2 triangles and 4 vertices) with UV mapping coordinates
        /// that are calculated for specified tile.</para>
        /// </remarks>
        /// <param name="tileset">The tileset.</param>
        /// <param name="tile">Tile data.</param>
        public void UpdateUVsFromTileIndex(Tileset tileset, TileData tile)
        {
            // Calculate index of row and column of brush in tileset
            float atlasColumn = (float)(tile.tilesetIndex % tileset.columnCount);
            float atlasRow = (float)(tile.tilesetIndex / tileset.columnCount);

            float x = tileset.borderU + tileset.tileIncrementU * atlasColumn;
            float y = tileset.borderV + tileset.tileIncrementV * atlasRow;

            int uvIndex = tile.Rotation;

            // First vertex
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.deltaU, 1f - (y + tileset.tileHeightUV - tileset.deltaV));
            if (++uvIndex == 4) {
                uvIndex = 0;
            }

            // Second vertex
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.deltaU, 1f - (y + tileset.deltaV));
            if (++uvIndex == 4) {
                uvIndex = 0;
            }

            // Third vertex
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.tileWidthUV - tileset.deltaU, 1f - (y + tileset.deltaV));
            if (++uvIndex == 4) {
                uvIndex = 0;
            }

            // Fourth vertex
            this.uvQuadBuffer[uvIndex] = new Vector2(x + tileset.tileWidthUV - tileset.deltaU, 1f - (y + tileset.tileHeightUV - tileset.deltaV));

            // Copy UVs into output data.
            this.uvBuffer.Add(this.uvQuadBuffer[0]);
            this.uvBuffer.Add(this.uvQuadBuffer[1]);
            this.uvBuffer.Add(this.uvQuadBuffer[2]);
            this.uvBuffer.Add(this.uvQuadBuffer[3]);
        }
    }
}
