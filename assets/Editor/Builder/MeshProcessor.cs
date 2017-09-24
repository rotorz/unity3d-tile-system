// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal sealed class MeshProcessor
    {
        #region Utility

        private static T[] ArrayOrNull<T>(T[] array)
        {
            return array != null && array.Length > 0 ? array : null;
        }

        // Clear existing list or create new list.
        private static void ResetList<T>(ref List<T> list)
        {
            if (list == null) {
                list = new List<T>();
            }
            else {
                list.Clear();
            }
        }

        // Pad as necessary.
        private static void PadBefore<T>(ref List<T> dest, int destLength, IList<T> src, int srcLength)
        {
            if (srcLength == 0 || dest.Count > 0 || src == null || src.Count == 0) {
                return;
            }

            // If destination list is empty then we will need to pad!
            while (destLength-- > 0) {
                dest.Add(default(T));
            }
        }

        private static void CopyOrPad<T>(List<T> dest, T[] src, int index)
        {
            if (src != null) {
                dest.Add(src[index]);
            }
            else if (dest.Count > 0) {
                dest.Add(default(T));
            }
        }

        // Copy items from source to destination, or pad as necessary.
        private static void CopyOrPad<T>(ref List<T> dest, int destLength, IList<T> src, int srcLength)
        {
            if (srcLength == 0) {
                return;
            }

            // If destination list already contains values, but source list doesn't,
            // then we will need to pad!
            if (src == null || src.Count == 0) {
                if (dest.Count > 0) {
                    while (srcLength-- > 0) {
                        dest.Add(default(T));
                    }
                }
                return;
            }

            // If destination list is empty then we will need to pad!
            if (dest.Count == 0) {
                while (destLength-- > 0) {
                    dest.Add(default(T));
                }
            }

            dest.AddRange(src);
        }

        // Copy item from source to destination when source contains items,
        // otherwise nullify destination array.
        private static void CopyToArrayOrEmpty<T>(ref T[] dest, List<T> src)
        {
            dest = (src != null && src.Count > 0)
                ? src.ToArray()
                : null;
        }

        #endregion


        private MeshData mountedMeshData;

        /// <summary>
        /// List of vertex positions.
        /// </summary>
        /// <remarks>
        /// <para>This variable must <b>not</b> be set to <c>null</c>.</para>
        /// </remarks>
        public List<Vector3> Vertices;

        /// <summary>
        /// List of vertex normals (Optional).
        /// </summary>
        /// <remarks>
        /// <para>List can be empty or <c>null</c> when no vertex normals are present,
        /// otherwise there must be exactly the same number of normals as there are
        /// vertex positions.</para>
        /// </remarks>
        public List<Vector3> Normals;

        /// <summary>
        /// List of vertex primary UV texture coordinates. (Optional)
        /// </summary>
        /// <remarks>
        /// <para>List can be empty or <c>null</c> when no UV coordinates are present,
        /// otherwise there must be exactly the same number of UV coordinates as there are
        /// vertex positions.</para>
        /// </remarks>
        public List<Vector2> Uv;

        /// <summary>
        /// List of vertex secondary UV texture coordinates (Optional).
        /// </summary>
        /// <remarks>
        /// <para>List can be empty or <c>null</c> when no UV coordinates are present,
        /// otherwise there must be exactly the same number of UV coordinates as there are
        /// vertex positions.</para>
        /// </remarks>
        public List<Vector2> Uv2;

        /// <summary>
        /// List of vertex colors using the <see cref="UnityEngine.Color32"/> representation (Optional).
        /// </summary>
        /// <remarks>
        /// <para>List can be empty or <c>null</c> when no colors are present, otherwise
        /// there must be exactly the same number of colors as there are vertex positions.</para>
        /// </remarks>
        public List<Color32> Colors32;

        /// <summary>
        /// List of vertex tangents (Optional).
        /// </summary>
        /// <remarks>
        /// <para>List can be empty or <c>null</c> when no tangents are present, otherwise
        /// there must be exactly the same number of tangents as there are vertex positions.</para>
        /// </remarks>
        public List<Vector4> Tangents;

        /// <summary>
        /// Collection of submesh triangle lists that are indexed by material.
        /// </summary>
        public Dictionary<Material, List<int>> Submeshes;


        /// <summary>
        /// Do not apply changes to mounted mesh data and reset mesh processor.
        /// </summary>
        public void Reset()
        {
            this.mountedMeshData = null;

            ResetList(ref this.Vertices);
            ResetList(ref this.Normals);
            ResetList(ref this.Uv);
            ResetList(ref this.Uv2);
            ResetList(ref this.Colors32);
            ResetList(ref this.Tangents);

            this.Submeshes = null;
        }

        /// <summary>
        /// Mount the specified mesh data so that it can be processed.
        /// </summary>
        /// <remarks>
        /// <para>Use <see cref="Apply"/> once all changes have been made to mesh data
        /// so that they are applied.</para>
        /// <para>Any changes made to previously mounted mesh data will be discarded unless
        /// they were explicitly applied using <see cref="Apply"/>.</para>
        /// </remarks>
        /// <param name="data">Mesh data.</param>
        public void Mount(MeshData data)
        {
            this.Reset();

            this.mountedMeshData = data;
            this.Submeshes = new Dictionary<Material, List<int>>();

            if (data.IsEmpty) {
                return;
            }

            this.AppendMesh(data);
        }

        /// <summary>
        /// Append mesh data to end of mounted mesh.
        /// </summary>
        /// <remarks>
        /// <para>This method is useful when combining meshes. Mesh data is combined on a
        /// per material basis.</para>
        /// </remarks>
        /// <param name="data">Mesh data to copy from.</param>
        public void AppendMesh(MeshData data)
        {
            if (data == null || data.IsEmpty) {
                return;
            }

            int destVertexCount = this.Vertices.Count;
            int srcVertexCount = data.Vertices.Length;

            this.Vertices.AddRange(data.Vertices);

            // Add padding before (or in place of) optional vertex data?
            CopyOrPad(ref this.Normals, destVertexCount, data.Normals, srcVertexCount);
            CopyOrPad(ref this.Uv, destVertexCount, data.Uv, srcVertexCount);
            CopyOrPad(ref this.Uv2, destVertexCount, data.Uv2, srcVertexCount);
            CopyOrPad(ref this.Colors32, destVertexCount, data.Colors32, srcVertexCount);
            CopyOrPad(ref this.Tangents, destVertexCount, data.Tangents, srcVertexCount);

            // Copy and triangles from source mesh into this mesh.
            foreach (KeyValuePair<Material, List<int>> submesh in data.Submeshes) {
                if (!this.Submeshes.ContainsKey(submesh.Key)) {
                    this.Submeshes.Add(submesh.Key, new List<int>());
                }

                int[] sourceTriangles = submesh.Value.ToArray();
                for (int j = 0; j < sourceTriangles.Length; ++j) {
                    sourceTriangles[j] += destVertexCount;
                }
                this.Submeshes[submesh.Key].AddRange(sourceTriangles);
            }
        }


        private List<int> usedVertices = new List<int>();

        private void FindUsedVertices(int[] triangles)
        {
            this.usedVertices.Clear();
            foreach (int vert in triangles) {
                if (!this.usedVertices.Contains(vert)) {
                    this.usedVertices.Add(vert);
                }
            }
        }

        /// <summary>
        /// Append mesh to end of mounted mesh.
        /// </summary>
        /// <remarks>
        /// <para>This method is useful when combining meshes. Mesh data is combined on a
        /// per material basis.</para>
        /// </remarks>
        /// <param name="transform">Transform that is to be applied before mesh is combined</param>
        /// <param name="mesh">Mesh that is to be added to output</param>
        /// <param name="materials">List of materials used by mesh. These must be ordered by submesh.</param>
        public void AppendMesh(Matrix4x4 transform, Mesh mesh, Material[] materials)
        {
            // Skip when there are no materials!
            if (mesh.vertexCount == 0 || mesh.subMeshCount == 0 || materials.Length == 0) {
                return;
            }

            int submeshCount = Mathf.Min(mesh.subMeshCount, materials.Length);

            Vector3[] meshVertices = mesh.vertices;
            Vector3[] meshNormals = ArrayOrNull(mesh.normals);
            Vector2[] meshUV = ArrayOrNull(mesh.uv);
            Vector2[] meshUV2 = ArrayOrNull(mesh.uv2);
            Color32[] meshColors32 = ArrayOrNull(mesh.colors32);
            Vector4[] meshTangents = ArrayOrNull(mesh.tangents);

            MeshData.TransformVertices(meshVertices, transform);

            if (meshNormals != null && meshNormals.Length > 0) {
                MeshData.TransformNormals(meshNormals, transform);
            }
            if (meshTangents != null && meshTangents.Length > 0) {
                MeshData.TransformTangents(meshTangents, transform);
            }

            // Add padding before optional vertex data?
            int offset = this.Vertices.Count;
            PadBefore(ref this.Normals, offset, meshNormals, meshVertices.Length);
            PadBefore(ref this.Uv, offset, meshUV, meshVertices.Length);
            PadBefore(ref this.Uv2, offset, meshUV2, meshVertices.Length);
            PadBefore(ref this.Colors32, offset, meshColors32, meshVertices.Length);
            PadBefore(ref this.Tangents, offset, meshTangents, meshVertices.Length);

            // Initialize one submesh for each distinct material.
            for (int i = 0; i < submeshCount; ++i) {
                var material = materials[i];

                if (!this.Submeshes.ContainsKey(material)) {
                    this.Submeshes.Add(material, new List<int>());
                }

                int[] triangles = mesh.GetTriangles(i);
                this.FindUsedVertices(triangles);

                // Add vertex data required by this submesh.
                offset = this.Vertices.Count;
                foreach (int vertexIndex in this.usedVertices) {
                    this.Vertices.Add(meshVertices[vertexIndex]);

                    CopyOrPad(this.Normals, meshNormals, vertexIndex);
                    CopyOrPad(this.Uv, meshUV, vertexIndex);
                    CopyOrPad(this.Uv2, meshUV2, vertexIndex);
                    CopyOrPad(this.Colors32, meshColors32, vertexIndex);
                    CopyOrPad(this.Tangents, meshTangents, vertexIndex);
                }

                for (int j = 0; j < triangles.Length; ++j) {
                    triangles[j] = offset + this.usedVertices.IndexOf(triangles[j]);
                }
                this.Submeshes[material].AddRange(triangles);
            }
        }

        /// <summary>
        /// Append mesh to end of mounted mesh.
        /// </summary>
        /// <remarks>
        /// <para>This method is useful when combining meshes. Mesh data is combined on a
        /// per material basis.</para>
        /// </remarks>
        /// <param name="transform">Game object transform component</param>
        /// <param name="filter">Mesh filter component</param>
        /// <param name="renderer">Mesh renderer component</param>
        public void AppendMesh(Transform transform, MeshFilter filter, MeshRenderer renderer)
        {
            this.AppendMesh(transform.localToWorldMatrix, filter.sharedMesh, renderer.sharedMaterials);
        }

        /// <summary>
        /// Append mesh to end of mounted mesh.
        /// </summary>
        /// <remarks>
        /// <para>This method is useful when combining meshes. Mesh data is combined on a
        /// per material basis.</para>
        /// </remarks>
        /// <param name="go">Automatically extract mesh, materials and transform from game object.</param>
        public void AppendMesh(GameObject go)
        {
            if (go == null) {
                return;
            }

            // Retrieve all mesh filters in game object.
            foreach (var filter in go.GetComponentsInChildren<MeshFilter>()) {
                if (filter != null) {
                    this.AppendMesh(filter.transform.localToWorldMatrix, filter.sharedMesh, filter.GetComponent<Renderer>().sharedMaterials);
                }
            }
        }

        /// <summary>
        /// Apply mesh data in mesh processor to the mounted mesh data.
        /// </summary>
        /// <remarks>
        /// <para>Mesh processor will be reset so that it can be reused if necessary.</para>
        /// </remarks>
        public void Apply()
        {
            CopyToArrayOrEmpty(ref this.mountedMeshData.Vertices, this.Vertices);
            CopyToArrayOrEmpty(ref this.mountedMeshData.Normals, this.Normals);
            CopyToArrayOrEmpty(ref this.mountedMeshData.Uv, this.Uv);
            CopyToArrayOrEmpty(ref this.mountedMeshData.Uv2, this.Uv2);
            CopyToArrayOrEmpty(ref this.mountedMeshData.Colors32, this.Colors32);
            CopyToArrayOrEmpty(ref this.mountedMeshData.Tangents, this.Tangents);

            this.mountedMeshData.Submeshes = this.Submeshes;

            this.Reset();
        }
    }
}
