// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Utility class to assist with the drawing of immediate in-editor tile previews.
    /// </summary>
    public static class ImmediatePreviewUtility
    {
        /// <summary>
        /// Tile data for preview tile.
        /// </summary>
        private static TileData s_PreviewTileData = new TileData();


        /// <summary>
        /// Get data for immediate tile preview.
        /// </summary>
        /// <param name="context">Context in which brush is being used.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°).</param>
        /// <returns>
        /// Data for preview tile.
        /// </returns>
        public static TileData GetPreviewTileData(IBrushContext context, Brush brush, int rotation)
        {
            s_PreviewTileData.Clear();
            s_PreviewTileData.brush = brush;
            s_PreviewTileData.flags = brush.TileFlags;
            s_PreviewTileData.PaintedRotation = rotation;
            s_PreviewTileData.Rotation = rotation;
            brush.PrepareTileData(context, s_PreviewTileData, 0);
            return s_PreviewTileData;
        }


        private static Material s_PreviewMaterialDefault;
        private static Material s_PreviewMaterialSeeThrough;


        private static void InitializePreviewMaterials()
        {
            Shader previewShader = Shader.Find("Rotorz/Preview");
            if (previewShader == null) {
                Debug.Log("Preview shader 'Rotorz/Preview' was not found. Please ensure that 'Rotorz/Tile System/Shaders/Preview.shader' was unpacked into your project.");
                previewShader = Shader.Find("Diffuse");
            }
            s_PreviewMaterialDefault = new Material(previewShader);
            s_PreviewMaterialDefault.hideFlags = HideFlags.HideAndDontSave;

            Shader previewSeeThroughShader = Shader.Find("Rotorz/Preview See Through");
            if (previewSeeThroughShader == null) {
                Debug.Log("Preview shader 'Rotorz/Preview See Through' was not found. Please ensure that 'Rotorz/Tile System/Shaders/PreviewSeeThrough.shader' was unpacked into your project.");
                previewSeeThroughShader = Shader.Find("Diffuse");
            }
            s_PreviewMaterialSeeThrough = new Material(previewSeeThroughShader);
            s_PreviewMaterialSeeThrough.hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// Gets the current preview material.
        /// </summary>
        /// <remarks>
        /// <para>A different material will be retrieved based upon the state of property
        /// <see cref="IsSeeThroughPreviewMaterial"/>. Color of material should not be
        /// changed, though the texture can be changed as needed.</para>
        /// <para><b>Note:</b> This property only applies to editor usage. The value of
        /// this property is generally useful when implementing <see cref="M:Rotorz.Tile.Editor.ToolBase.OnDrawGizmos"/>
        /// for custom tools.</para>
        /// </remarks>
        public static Material PreviewMaterial {
            get {
                if (s_PreviewMaterialDefault == null) {
                    InitializePreviewMaterials();
                }
                return IsSeeThroughPreviewMaterial ? s_PreviewMaterialSeeThrough : s_PreviewMaterialDefault;
            }
        }

        /// <summary>
        /// Gets or sets transformation matrix for drawing immediate previews.
        /// </summary>
        public static Matrix4x4 Matrix { get; set; }

        /// <summary>
        /// Gets or sets whether see-through preview mode is active.
        /// </summary>
        /// <remarks>
        /// <para>Note: This property only applies to editor usage. The value of this
        /// property is generally useful when implementing <see cref="M:Rotorz.Tile.Editor.ToolBase.OnDrawGizmos"/>
        /// for custom tools.</para>
        /// </remarks>
        public static bool IsSeeThroughPreviewMaterial { get; set; }

        /// <exclude/>
        public static void DrawNow(Material previewMaterial, Mesh mesh, Matrix4x4 matrix)
        {
            int passCount = previewMaterial.passCount;
            for (int pass = 0; pass < passCount; ++pass) {
                previewMaterial.SetPass(pass);
                Graphics.DrawMeshNow(mesh, matrix);
            }
        }

        /// <exclude/>
        public static void DrawNow(Material previewMaterial, Mesh mesh, Renderer renderer, IMaterialMappings materialMappings, Matrix4x4 matrix)
        {
            if (renderer == null || !renderer.enabled) {
                return;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null) {
                return;
            }

            for (int submesh = 0; submesh < materials.Length; ++submesh) {
                // Remap material?
                var m = materials[submesh];
                if (materialMappings != null) {
                    m = materialMappings.RemapMaterial(m);
                }

                // Use preview material when specified; otherwise just use material from renderer.
                if (previewMaterial != null) {
                    previewMaterial.mainTexture = m.mainTexture;
                    m = previewMaterial;
                }

                int passCount = m.passCount;
                for (int pass = 0; pass < passCount; ++pass) {
                    m.SetPass(pass);
                    Graphics.DrawMeshNow(mesh, matrix, submesh);
                }
            }
        }

        private static void DrawNow(Material previewMaterial, SpriteRenderer renderer, Matrix4x4 matrix)
        {
            if (renderer == null || renderer.sprite == null) {
                return;
            }

            var sprite = renderer.sprite;
            var m = renderer.sharedMaterial;

            // Use preview material when specified; otherwise just use material from renderer.
            if (previewMaterial != null) {
                previewMaterial.mainTexture = sprite.texture;
                m = previewMaterial;
            }

            var mesh = SpritePreviewMesh;
            mesh.Clear();
            mesh.vertices = sprite.vertices.Select(vertex => (Vector3)vertex).ToArray();
            mesh.uv = sprite.uv;
            mesh.triangles = sprite.triangles.Select(index => (int)index).ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            int passCount = m.passCount;
            for (int pass = 0; pass < passCount; ++pass) {
                m.SetPass(pass);
                Graphics.DrawMeshNow(mesh, matrix, 0);
            }
        }

        private static Mesh SharedMeshFromRenderer(Renderer renderer)
        {
            var skinnedRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedRenderer != null) {
                return skinnedRenderer.sharedMesh;
            }

            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null) {
                return meshFilter.sharedMesh;
            }

            return null;
        }

        private static readonly List<Renderer> s_DrawNowRendererComponents = new List<Renderer>();

        /// <exclude/>
        public static void DrawNow(Material previewMaterial, Transform obj, Matrix4x4 matrix, IMaterialMappings materialMappings)
        {
            Matrix4x4 modelMatrix;
            Matrix4x4 inversePrefabMatrix = matrix * obj.localToWorldMatrix.inverse;

            try {
                s_DrawNowRendererComponents.Clear();
                obj.GetComponentsInChildren<Renderer>(true, s_DrawNowRendererComponents);

                foreach (var renderer in s_DrawNowRendererComponents) {
                    // Skip inactive renderers!
                    if (!renderer.enabled) {
                        continue;
                    }

                    // Calculate model matrix for tile preview.
                    Transform subObject = renderer.transform;
                    if (subObject != obj) {
                        modelMatrix = inversePrefabMatrix * subObject.localToWorldMatrix;
                    }
                    else {
                        modelMatrix = matrix;
                    }

                    // Attempt to render preview!
                    var spriteRenderer = renderer as SpriteRenderer;
                    if (spriteRenderer != null) {
                        DrawNow(previewMaterial, spriteRenderer, modelMatrix);
                        continue;
                    }

                    var mesh = SharedMeshFromRenderer(renderer);
                    if (mesh == null) {
                        continue;
                    }

                    DrawNow(previewMaterial, mesh, renderer, materialMappings, modelMatrix);
                }
            }
            finally {
                s_DrawNowRendererComponents.Clear();
            }
        }


        #region Quad Preview Mesh

        /// <summary>
        /// Vertex buffer for quad preview mesh.
        /// </summary>
        private static readonly Vector3[] s_QuadPreviewVertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(-0.5f, +0.5f, 0f),
            new Vector3(+0.5f, +0.5f, 0f),
            new Vector3(+0.5f, -0.5f, 0f),
        };

        /// <summary>
        /// Triangle buffer for quad preview mesh.
        /// </summary>
        private static readonly int[] s_QuadPreviewTriangles = new int[] {
            0, 1, 2,
            2, 3, 0,
        };


        /// <summary>
        /// UV buffer for quad preview mesh.
        /// </summary>
        private static Vector2[] s_QuadPreviewUvs = new Vector2[4];

        /// <summary>
        /// Reusable mesh for rendering immediate quad previews.
        /// </summary>
        private static Mesh s_QuadPreviewMesh;

        /// <summary>
        /// Reusable mesh for rendering immediate sprite previews.
        /// </summary>
        private static Mesh s_SpritePreviewMesh;

        /// <summary>
        /// Gets reusable mesh for rendering immediate quad previews.
        /// </summary>
        private static Mesh QuadPreviewMesh {
            get {
                if (s_QuadPreviewMesh == null) {
                    s_QuadPreviewMesh = new Mesh();
                    s_QuadPreviewMesh.name = "{rts}SimpleQuadPreview";
                    s_QuadPreviewMesh.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_QuadPreviewMesh;
            }
        }

        /// <summary>
        /// Gets reusable mesh for rendering immediate sprite previews.
        /// </summary>
        private static Mesh SpritePreviewMesh {
            get {
                if (s_SpritePreviewMesh == null) {
                    s_SpritePreviewMesh = new Mesh();
                    s_SpritePreviewMesh.name = "{rts}SimpleSpritePreview";
                    s_SpritePreviewMesh.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_SpritePreviewMesh;
            }
        }

        /// <summary>
        /// Calculate texture coordinates from sprite.
        /// </summary>
        /// <param name="sprite">Sprite.</param>
        /// <returns>
        /// The texture coordinates.
        /// </returns>
        internal static Rect CalculateQuadTexCoords(Sprite sprite)
        {
            Rect texCoords = sprite.textureRect;

            var texture = sprite.texture;

            texCoords.x /= texture.width;
            texCoords.width /= texture.width;
            texCoords.y /= texture.height;
            texCoords.height /= texture.height;

            return texCoords;
        }

        /// <summary>
        /// Updates the quad preview mesh.
        /// </summary>
        /// <param name="texCoords">Texture coordinates.</param>
        /// <returns>
        /// The quad preview mesh.
        /// </returns>
        internal static Mesh UpdateQuadPreviewMesh(Rect texCoords)
        {
            // 1 -- > 2
            // ^      |
            // |      |
            // 0 <--- 3

            s_QuadPreviewUvs[0].x = texCoords.x;
            s_QuadPreviewUvs[0].y = texCoords.y;
            s_QuadPreviewUvs[1].x = texCoords.x;
            s_QuadPreviewUvs[1].y = texCoords.y + texCoords.height;
            s_QuadPreviewUvs[2].x = texCoords.x + texCoords.width;
            s_QuadPreviewUvs[2].y = texCoords.y + texCoords.height;
            s_QuadPreviewUvs[3].x = texCoords.x + texCoords.width;
            s_QuadPreviewUvs[3].y = texCoords.y;

            // Update actual mesh.
            var mesh = QuadPreviewMesh;
            mesh.Clear();

            mesh.vertices = s_QuadPreviewVertices;
            mesh.uv = s_QuadPreviewUvs;
            mesh.triangles = s_QuadPreviewTriangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        #endregion
    }
}
