// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility class for rendering brush previews.
    /// </summary>
    [Serializable]
    internal sealed class BrushPreviewRenderUtility
    {
        private static readonly Vector2 DefaultPreviewRotation = new Vector2(30f, -20f);


        [NonSerialized]
        private PreviewRenderUtility previewUtility;

        [SerializeField]
        private Color ambientLightingColor = new Color(0.1f, 0.1f, 0.1f, 0f);
        [SerializeField]
        private Vector2 previewRotation = DefaultPreviewRotation;
        [SerializeField]
        private float rangeFactor = 3.8f;


        /// <summary>
        /// Gets or sets ambient color of lighting.
        /// </summary>
        public Color AmbientLightingColor {
            get { return this.ambientLightingColor; }
            set { this.ambientLightingColor = value; }
        }
        /// <summary>
        /// Gets or sets rotation of preview.
        /// </summary>
        public Vector2 PreviewRotation {
            get { return this.previewRotation; }
            set { this.previewRotation = value; }
        }
        /// <summary>
        /// Gets or sets range factor. Lower values provide close-ups whilst larger
        /// values show tile preview from greater distance.
        /// </summary>
        public float RangeFactor {
            get { return this.rangeFactor; }
            set { this.rangeFactor = value; }
        }

        /// <summary>
        /// Gets array of preview lights.
        /// </summary>
        public Light[] Lights {
            get {
                this.Setup();
                return this.previewUtility.lights;
            }
        }


        #region Temporary scene for capturing preview

        [NonSerialized]
        private bool hasSetup;

        private TileSystem tileSystem;

        /// <summary>
        /// Setup temporary scene for rendering previews.
        /// </summary>
        /// <remarks>
        /// <para>Temporary scene includes a 1x1 tile system, one camera and two directional
        /// light sources.</para>
        /// </remarks>
        private void Setup()
        {
            if (this.hasSetup) {
                return;
            }

            this.hasSetup = true;

            this.previewUtility = new PreviewRenderUtility(true);
            this.previewUtility.cameraFieldOfView = 30f;

            // Camera should only render layer 3 to avoid rendering scene!
            this.previewUtility.camera.clearFlags = CameraClearFlags.Nothing;
            this.previewUtility.camera.cullingMask = 1 << 3;

            // Present lights with similar settings as default.
            this.previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);

            // This must be two times more intense than with Unity 4.
            this.previewUtility.lights[0].intensity = 1.4f;
            this.previewUtility.lights[1].intensity = 1.4f;

            // Create simple 1x1 tile system for painting!
            var tileSystemGO = new GameObject("{Preview}Tile System");
            tileSystemGO.hideFlags = HideFlags.HideAndDontSave;
            this.tileSystem = tileSystemGO.AddComponent<TileSystem>();
            this.tileSystem.CreateSystem(1, 1, 1, 1, 1, 1, 1);
        }

        /// <summary>
        /// Cleanup scene again!
        /// </summary>
        public void Cleanup()
        {
            if (!this.hasSetup) {
                return;
            }

            // Destroy temporary tile system object since it is no longer required!
            if (this.tileSystem != null) {
                Object.DestroyImmediate(this.tileSystem.gameObject);
            }

            this.previewUtility.Cleanup();
        }

        #endregion


        #region Utility functions

        /// <summary>
        /// Recursively set layer for entire object hierarchy.
        /// </summary>
        /// <param name="obj">Root object of hierarchy.</param>
        private static void SetRenderLayerRecursive(Transform obj)
        {
            var go = obj.gameObject;
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = 3;

            foreach (Transform transform in obj) {
                SetRenderLayerRecursive(transform);
            }
        }

        /// <summary>
        /// Get bounds of renderable object. This method considers entire object hierarchy.
        /// </summary>
        /// <param name="bounds">Initial bounds that are to be updated.</param>
        /// <param name="obj">Root object of hierarchy.</param>
        private static void GetRenderableBoundsRecursive(ref Bounds bounds, Transform obj)
        {
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true)) {
                if (bounds.extents == Vector3.zero) {
                    bounds = renderer.bounds;
                }
                else {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }

        #endregion


        #region Preview generation

        private bool _restoreFogSetting;


        /// <summary>
        /// Prepare preview scene to render one brush preview.
        /// </summary>
        private void BeginPreviewScene()
        {
            this.Setup();

            // Apply temporary lighting override.
            UnityEditorInternal.InternalEditorUtility.SetCustomLighting(this.Lights, this.AmbientLightingColor);

            // Temporarily disable fog.
            this._restoreFogSetting = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
        }

        /// <summary>
        /// Finish up rendering brush preview.
        /// </summary>
        private void EndPreviewScene()
        {
            // Remove temporary lighting override.
            UnityEditorInternal.InternalEditorUtility.RemoveCustomLighting();
            // Restore former fog setting.
            Unsupported.SetRenderSettingsUseFogNoDirty(this._restoreFogSetting);
        }

        /// <summary>
        /// Generate preview for brush by painting a tile onto the temporary tile system.
        /// Then render using preview camera.
        /// </summary>
        /// <param name="brush">The brush which is to be previewed.</param>
        /// <returns>
        /// A value of <c>true</c> if preview was generated; otherwise <c>false</c>.
        /// </returns>
        private bool GeneratePreview(Brush brush)
        {
            try {
                // Paint preview tile.
                brush.Paint(this.tileSystem, TileIndex.zero, 0);
                // Adjust layer of generated game objects.
                SetRenderLayerRecursive(this.tileSystem.transform);

                // Update position of camera to encapsulate bounds of preview object.
                Bounds bounds = new Bounds();
                GetRenderableBoundsRecursive(ref bounds, this.tileSystem.transform);
                //Debug.Log("Generate preview for " + brush.name);
                // Is there nothing to show?
                if (bounds.size == Vector3.zero) {
                    return false;
                }

                Vector2 rotation = this.PreviewRotation;

                // Does tile contain any sprite objects?
                bool containsSprite = this.tileSystem.transform.GetComponentInChildren<SpriteRenderer>() != null;
                if (containsSprite) {
                    rotation.x -= DefaultPreviewRotation.x;
                    rotation.y -= DefaultPreviewRotation.y;
                }

                float magnitude = bounds.extents.magnitude;
                float num = magnitude * this.RangeFactor;
                var quaternion = Quaternion.Euler(-rotation.y, -rotation.x, 0f);
                var position = bounds.center - quaternion * (Vector3.forward * num);

                this.previewUtility.camera.transform.position = position;
                this.previewUtility.camera.transform.rotation = quaternion;
                this.previewUtility.camera.nearClipPlane = num - magnitude * 1.1f;
                this.previewUtility.camera.farClipPlane = num + magnitude * 1.1f;

                // Actually render preview!
                this.previewUtility.camera.Render();

                return true;
            }
            finally {
                // Erase unwanted preview tile.
                this.tileSystem.EraseAllTiles();
            }
        }

        /// <summary>
        /// Update render texture with brush preview.
        /// </summary>
        /// <param name="brush">The brush which is to be previewed.</param>
        /// <param name="width">Width of texture in pixels.</param>
        /// <param name="height">Height of texture in pixels.</param>
        /// <param name="backgroundStyle">Style to fill background.</param>
        /// <returns>
        /// The renderable texture instance.
        /// </returns>
        public Texture UpdateRenderTexture(Brush brush, int width, int height, GUIStyle backgroundStyle)
        {
            try {
                this.BeginPreviewScene();

                this.previewUtility.BeginPreview(new Rect(0, 0, width, height), backgroundStyle);
                this.GeneratePreview(brush);
                return this.previewUtility.EndPreview();
            }
            finally {
                this.EndPreviewScene();
            }
        }

        /// <summary>
        /// Render preview for brush and return static texture.
        /// </summary>
        /// <param name="brush">The brush which is to be previewed.</param>
        /// <param name="width">Width of texture in pixels.</param>
        /// <param name="height">Height of texture in pixels.</param>
        /// <returns>
        /// The static <c>Texture2D</c> instance; or a value of <c>null</c> if no preview was generated.
        /// </returns>
        public Texture2D CreateStaticTexture(Brush brush, int width, int height)
        {
            try {
                this.BeginPreviewScene();

                this.previewUtility.BeginStaticPreview(new Rect(0, 0, width, height));
                bool result = this.GeneratePreview(brush);
                var previewTexture = this.previewUtility.EndStaticPreview();
                return result ? previewTexture : null;
            }
            finally {
                this.EndPreviewScene();
            }
        }

        #endregion
    }
}
