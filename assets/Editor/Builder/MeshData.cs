// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Simple data representation of a mesh.
    /// </summary>
    internal sealed class MeshData
    {
        #region Utility

        internal static void TransformVertices(Vector3[] vertices, Matrix4x4 transform)
        {
            for (int i = 0; i < vertices.Length; ++i) {
                vertices[i] = transform.MultiplyPoint(vertices[i]);
            }
        }

        internal static void TransformNormals(Vector3[] normals, Matrix4x4 transform)
        {
            transform = transform.inverse.transpose;

            for (int i = 0; i < normals.Length; ++i) {
                normals[i] = transform.MultiplyVector(normals[i]).normalized;
            }
        }

        internal static void TransformTangents(Vector4[] tangents, Matrix4x4 transform)
        {
            transform = transform.inverse.transpose;

            for (int i = 0; i < tangents.Length; ++i) {
                Vector4 tangent = tangents[i];
                Vector3 p = new Vector3(tangent.x, tangent.y, tangent.z);
                p = transform.MultiplyVector(p).normalized;
                tangents[i] = new Vector4(p.x, p.y, p.z, tangent.w);
            }
        }

        #endregion


        internal bool Smooth;


        /// <summary>
        /// Vertex positions or a value of <c>null</c>.
        /// </summary>
        public Vector3[] Vertices;
        /// <summary>
        /// Normals or a value of <c>null</c>.
        /// </summary>
        public Vector3[] Normals;
        /// <summary>
        /// Mapping coordinates or a value of <c>null</c>.
        /// </summary>
        public Vector2[] Uv;
        /// <summary>
        /// Secondary mapping coordinates or a value of <c>null</c>.
        /// </summary>
        public Vector2[] Uv2;
        /// <summary>
        /// Vertex colors or a value of <c>null</c>.
        /// </summary>
        public Color32[] Colors32;
        /// <summary>
        /// Tangents or a value of <c>null</c>.
        /// </summary>
        public Vector4[] Tangents;

        /// <summary>
        /// Backup of original mesh normals.
        /// </summary>
        /// <remarks>
        /// <para>Populated by <see cref="CopyOriginalNormals"/>.</para>
        /// </remarks>
        public Vector3[] OriginalNormals;

        /// <summary>
        /// Collection of submeshes grouped by material.
        /// </summary>
        /// <remarks>
        /// <para>Each submesh is a triangle list.</para>
        /// </remarks>
        public Dictionary<Material, List<int>> Submeshes;


        /// <summary>
        /// Gets a value indicating whether mesh contains at least one vertex.
        /// </summary>
        public bool IsEmpty {
            get {
                return this.Vertices == null
                    || this.Vertices.Length == 0
                    || this.Submeshes == null
                    || this.Submeshes.Count == 0
                    ;
            }
        }

        /// <summary>
        /// Gets a value indicating whether mesh contains normals.
        /// </summary>
        public bool HasNormals {
            get { return this.Normals != null && this.Normals.Length > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether mesh contains mapping coordinates.
        /// </summary>
        public bool HasUV {
            get { return this.Uv != null && this.Uv.Length > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether mesh contains a second set of mapping coordinates.
        /// </summary>
        public bool HasUV2 {
            get { return this.Uv2 != null && this.Uv2.Length > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether mesh contains vertex colors.
        /// </summary>
        public bool HasColors32 {
            get { return this.Colors32 != null && this.Colors32.Length > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether mesh contains tangents.
        /// </summary>
        public bool HasTangents {
            get { return this.Tangents != null && this.Tangents.Length > 0; }
        }

        /// <summary>
        /// Gets the collection of materials, or a value of <c>null</c> if mesh
        /// doesn't contain any submeshes.
        /// </summary>
        public IEnumerable<Material> Materials {
            get {
                return this.Submeshes != null
                    ? this.Submeshes.Keys
                    : null;
            }
        }

        public void ApplyTransform(Matrix4x4 transform)
        {
            TransformVertices(this.Vertices, transform);

            if (this.HasNormals) {
                TransformNormals(this.Normals, transform);
            }
            if (this.HasTangents) {
                TransformTangents(this.Tangents, transform);
            }
        }


        /// <summary>
        /// Convert mesh data into a single mesh with one submesh per material.
        /// </summary>
        /// <returns>
        /// The mesh object.
        /// </returns>
        public Mesh ToMesh()
        {
            var mesh = new Mesh();

            mesh.vertices = this.Vertices;

            if (this.HasNormals) {
                mesh.normals = this.Normals;
            }
            if (this.HasUV) {
                mesh.uv = this.Uv;
            }
            if (this.HasUV2) {
                mesh.uv2 = this.Uv2;
            }
            if (this.HasColors32) {
                mesh.colors32 = this.Colors32;
            }
            if (this.HasTangents) {
                mesh.tangents = this.Tangents;
            }

            mesh.subMeshCount = this.Submeshes.Count;

            int submeshIndex = 0;
            foreach (List<int> submesh in this.Submeshes.Values) {
                mesh.SetTriangles(submesh.ToArray(), submeshIndex++);
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Convert mesh data into meshes on a per material basis.
        /// </summary>
        /// <param name="processor">Reuse an existing mesh processor, or alternatively
        /// specify <c>null</c> to create a new one.</param>
        /// <returns>
        /// Array of <c>Mesh</c> objects.
        /// </returns>
        public Mesh[] ToMeshPerMaterial(MeshProcessor processor)
        {
            if (processor == null) {
                processor = new MeshProcessor();
            }

            Mesh[] meshes = new Mesh[this.Submeshes.Count];
            int meshIndex = 0;

            // Array of vertex indices which is used to extract vertices
            // into separate mesh objects.
            int[] vertexMap = new int[this.Vertices.Length];
            int vertexMapIndex = 0;

            foreach (var material in this.Submeshes.Keys) {
                int[] triangles = this.Submeshes[material].ToArray();

                // Clear previously extracted vertices.
                processor.Reset();

                for (int i = 0; i < vertexMap.Length; ++i) {
                    vertexMap[i] = -1;
                }
                vertexMapIndex = 0;

                // Extract unique vertices from mesh.
                for (int ti = 0; ti < triangles.Length; ++ti) {
                    int vi = triangles[ti];

                    // Has this vertex already been extracted?
                    if (vertexMap[vi] == -1) {
                        vertexMap[vi] = vertexMapIndex++;

                        processor.Vertices.Add(this.Vertices[vi]);
                        if (this.HasNormals) {
                            processor.Normals.Add(this.Normals[vi]);
                        }
                        if (this.HasUV) {
                            processor.Uv.Add(this.Uv[vi]);
                        }
                        if (this.HasUV2) {
                            processor.Uv2.Add(this.Uv2[vi]);
                        }
                        if (this.HasColors32) {
                            processor.Colors32.Add(this.Colors32[vi]);
                        }
                        if (this.HasTangents) {
                            processor.Tangents.Add(this.Tangents[vi]);
                        }
                    }

                    // Triangle index must be remapped.
                    triangles[ti] = vertexMap[vi];
                }

                // Form mesh from extracted vertex data.
                var mesh = new Mesh();
                meshes[meshIndex++] = mesh;

                mesh.vertices = processor.Vertices.ToArray();

                if (this.HasNormals) {
                    mesh.normals = processor.Normals.ToArray();
                }
                if (this.HasUV) {
                    mesh.uv = processor.Uv.ToArray();
                }
                if (this.HasUV2) {
                    mesh.uv2 = processor.Uv2.ToArray();
                }
                if (this.HasColors32) {
                    mesh.colors32 = processor.Colors32.ToArray();
                }
                if (this.HasTangents) {
                    mesh.tangents = processor.Tangents.ToArray();
                }

                mesh.triangles = triangles;

                mesh.RecalculateBounds();
            }

            return meshes;
        }

        /*
          Ref: [1] http://forum.unity3d.com/threads/38984-How-to-Calculate-Mesh-Tangents
               [2] http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html
               [3] http://www.terathon.com/code/tangent.html
        */
        internal void CalculateTangents()
        {
            // Cannot calculate without UV's and normals!
            if (this.Uv == null || this.Normals == null) {
                return;
            }

            if (this.Tangents == null) {
                this.Tangents = new Vector4[this.Vertices.Length];
            }

            Vector3[] tan1 = new Vector3[this.Vertices.Length];
            Vector3[] tan2 = new Vector3[this.Vertices.Length];

            Vector4 tangent;

            foreach (List<int> submesh in this.Submeshes.Values) {
                int triangleVertexCount = submesh.Count;
                for (int i = 0; i < triangleVertexCount;) {
                    int i1 = submesh[i++];
                    int i2 = submesh[i++];
                    int i3 = submesh[i++];

                    Vector3 A = this.Vertices[i1];
                    Vector3 B = this.Vertices[i2];
                    Vector3 C = this.Vertices[i3];

                    Vector2 w1 = this.Uv[i1];
                    Vector2 w2 = this.Uv[i2];
                    Vector2 w3 = this.Uv[i3];

                    float x1 = B.x - A.x;
                    float x2 = C.x - A.x;
                    float y1 = B.y - A.y;
                    float y2 = C.y - A.y;
                    float z1 = B.z - A.z;
                    float z2 = C.z - A.z;

                    float s1 = w2.x - w1.x;
                    float s2 = w3.x - w1.x;
                    float t1 = w2.y - w1.y;
                    float t2 = w3.y - w1.y;

                    float r = 1f / (s1 * t2 - s2 * t1);
                    Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                    Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                    tan1[i1] += sdir;
                    tan1[i2] += sdir;
                    tan1[i3] += sdir;

                    tan2[i1] += tdir;
                    tan2[i2] += tdir;
                    tan2[i3] += tdir;
                }
            }

            for (int i = 0; i < this.Vertices.Length; ++i) {
                Vector3 n = this.Normals[i];
                Vector3 t = tan1[i];

                // Gram-Schmidt orthogonalize.
                Vector3.OrthoNormalize(ref n, ref t);

                tangent.x = t.x;
                tangent.y = t.y;
                tangent.z = t.z;
                tangent.w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0f) ? -1f : 1f;
                this.Tangents[i] = tangent;
            }
        }

        public void CopyOriginalNormals()
        {
            if (this.Normals == null) {
                this.OriginalNormals = null;
            }
            else if (this.OriginalNormals == null || this.OriginalNormals.Length != this.Normals.Length) {
                this.OriginalNormals = this.Normals.ToArray();
            }
            else {
                for (int i = 0; i < this.OriginalNormals.Length; ++i) {
                    this.OriginalNormals[i] = this.Normals[i];
                }
            }
        }
    }
}
