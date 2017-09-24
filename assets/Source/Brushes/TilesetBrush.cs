// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Tileset brushes can paint tiles from a tileset.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Tileset-Brushes">Tileset Brushes</a>
    /// section of user guide for further information.</para>
    /// </intro>
    /// <remarks>
    /// <para>This kind of brush is ideal for painting 2D tiles.</para>
    /// <para>Colliders and prefabs can be attached to painted tiles, though this
    /// functionality should not be used too excessively to avoid poor performance (this
    /// is relative to the number of tiles you expect to paint in a scene).</para>
    /// <para>Tileset brushes can be presented using a procedurally generated mesh, or
    /// alternatively using individual pre-generated tile meshes. There are use cases for
    /// both, however procedural meshes tend to offer better performance.</para>
    /// <para>Tileset brushes are contained within <see cref="Tileset"/> assets which make
    /// the transition between procedural and non-procedural brushes relatively
    /// straightforward. Pre-generated non-procedural meshes are acceses via the tileset
    /// asset and are not directly referenced by tileset brushes.</para>
    /// </remarks>
    /// <seealso cref="Tileset"/>
    /// <seealso cref="OrientedBrush"/>
    /// <seealso cref="AliasBrush"/>
    /// <seealso cref="AutotileBrush"/>
    /// <seealso cref="EmptyBrush"/>
    public class TilesetBrush : Brush
    {
        /// <inheritdoc/>
        public override string DesignableType {
            get { return "Tileset Brush"; }
        }


        [HideInInspector, SerializeField, FormerlySerializedAs("_tileset")]
        private Tileset tileset;

        /// <summary>
        /// Zero-based index of tile in tileset.
        /// </summary>
        public int tileIndex;
        /// <summary>
        /// Indicates whether brush is procedural or non-procedural.
        /// </summary>
        [HideInInspector, SerializeField, FormerlySerializedAs("_procedural")]
        public InheritYesNo procedural;

        /// <summary>
        /// Indicates whether box colliders should be added to painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>Adding colliders to individual tile objects will make the process of
        /// painting tiles considerably slower. This feature should be used carefully.</para>
        /// <para>Custom tile based collision detection logic is often more efficient and
        /// can be implemented using tile flags (and of course the "Solid" flag where
        /// applicable).</para>
        /// </remarks>
        /// <seealso cref="colliderType"/>
        public bool addCollider;
        /// <summary>
        /// The type of collider associated with brush.
        /// </summary>
        /// <seealso cref="addCollider"/>
        public ColliderType colliderType = ColliderType.BoxCollider3D;

        /// <summary>
        /// A prefab game object that should be added to painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>This field is optional and where possible should be left as <c>null</c>
        /// for best performance. Performance will vary depending upon the complexity of
        /// the attached prefab.</para>
        /// </remarks>
        /// <seealso cref="applySimpleRotationToAttachment"/>
        public GameObject attachPrefab;

        /// <summary>
        /// Indicates whether simple tile rotation should be applied to attached objects.
        /// </summary>
        /// <seealso cref="this.attachPrefab"/>
        public bool applySimpleRotationToAttachment;

        /// <summary>
        /// Indicates whether empty container object should be created despite not being needed
        /// by brush. Container objects are named "tile" and can be seen in tile system hierarchy.
        /// </summary>
        /// <remarks>
        /// <para>This property is sometimes useful in connection with custom user scripts.</para>
        /// <para>This property only applies to procedural tileset brushes since conatiner
        /// objects are always needed by non-procedural tileset brushes.</para>
        /// </remarks>
        public bool alwaysAddContainer;

        /// <summary>
        /// Gets the tileset that brush belongs to.
        /// </summary>
        public Tileset Tileset {
            get { return this.tileset; }
        }

        /// <summary>
        /// Gets a value indicating whether brush creates procedural or non-procedural
        /// tiles.
        /// </summary>
        public bool IsProcedural {
            get {
                switch (this.procedural) {
                    default:
                    case InheritYesNo.Inherit:
                        return this.tileset.procedural;

                    case InheritYesNo.Yes:
                        return true;

                    case InheritYesNo.No:
                        return false;
                }
            }
        }

        /// <inheritdoc/>
        public override bool CanOverrideTagAndLayer {
            get { return false; }
        }

        /// <inheritdoc/>
        public override bool PerformsAutomaticOrientation {
            get { return false; }
        }

        /// <inheritdoc/>
        public override bool UseWireIndicatorInEditor {
            get { return false; }
        }


        /// <summary>
        /// Initializes the tileset brush for the first time.
        /// </summary>
        /// <param name="tileset">Tileset that the tileset brush belongs to.</param>
        public virtual void Initialize(Tileset tileset)
        {
            if (this.tileset != null) {
                throw new InvalidOperationException("Brush has already been initialized.");
            }

            this.tileset = tileset;
        }


        /// <inheritdoc/>
        protected internal override bool CalculateManualOffset(IBrushContext context, TileData tile, Transform transform, out Vector3 offsetPosition, out Quaternion offsetRotation, out Vector3 offsetScale, Brush transformer)
        {
            var tileSystem = context.TileSystem;

            // Calculate normal position of tile within system.
            offsetPosition = tileSystem.LocalPositionFromTileIndex(context.Row, context.Column, true);
            // Turn tile to face away from system and rotate tile to align with system.
            offsetRotation = tileSystem.CalculateSimpleRotation(TileFacing.Sideways, tile.Rotation) * MathUtility.AngleAxis_180_Up;
            // Adjust scale of tile container.
            offsetScale = tileSystem.CalculateCellSizeScale(tile.Rotation);

            Vector3 localScale = transform.localScale;

            offsetPosition = transform.localPosition - offsetPosition;
            offsetRotation = Quaternion.Inverse(offsetRotation) * transform.localRotation;
            offsetScale = new Vector3(
                localScale.x / offsetScale.x,
                localScale.y / offsetScale.y,
                localScale.z / offsetScale.z
            );

            return true;
        }

        /// <inheritdoc/>
        protected internal override void PrepareTileData(IBrushContext context, TileData tile, int variationIndex)
        {
            tile.Procedural = this.IsProcedural;
            tile.tileset = this.tileset;
            tile.tilesetIndex = this.tileIndex;
        }

        /// <inheritdoc/>
        protected internal override void CreateTile(IBrushContext context, TileData tile)
        {
            // Tile is procedural regardless of which brush defined it.
            if (tile.Procedural) {
                this.CreateProceduralTile(context, tile, this.addCollider);
            }
            else {
                this.CreateNonProceduralTile(context, tile, this.addCollider);
            }
        }

        /// <summary>
        /// Create procedural tile.
        /// </summary>
        /// <remarks>
        /// <para>Creates container game object for tile when collider and/or attachment
        /// is to be added. Collider and/or attachment is also added when specified.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        /// <param name="addCollider">Indicates if collider should be added.</param>
        protected void CreateProceduralTile(IBrushContext context, TileData tile, bool addCollider)
        {
            // Attach object to tile?
            if (addCollider || this.attachPrefab != null) {
                tile.gameObject = new GameObject("tile");
                this.AddAttachments(context, tile, tile.gameObject, addCollider);
            }
            else if (this.alwaysAddContainer) {
                tile.gameObject = new GameObject("tile");
            }
        }

        /// <summary>
        /// Create non-procedural tile.
        /// </summary>
        /// <remarks>
        /// <para>Creates container game object for tile and adds the mesh filter and
        /// renderer components for tile mesh. Collider and/or attachment is also added
        /// when specified.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        /// <param name="addCollider">Indicates if collider should be added.</param>
        protected void CreateNonProceduralTile(IBrushContext context, TileData tile, bool addCollider)
        {
            // Create container object for tile and attach to chunk.
            var tileGO = new GameObject("tile");
            tile.gameObject = tileGO;

            if (addCollider || this.attachPrefab != null) {
                this.AddAttachments(context, tile, tileGO, addCollider);
            }

            var tileset = tile.tileset;
            int tilesetIndex = tile.tilesetIndex;

            if (tileset.tileMeshes != null && tilesetIndex >= 0 && tilesetIndex < tileset.tileMeshes.Length) {
                var filter = tileGO.AddComponent<MeshFilter>();
                filter.sharedMesh = tileset.tileMeshes[tilesetIndex];

                var renderer = tileGO.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = tileset.AtlasMaterial;
            }
        }

        private void AddAttachments(IBrushContext context, TileData tile, GameObject tileGO, bool addCollider)
        {
            // Add collider to tile game object?
            if (addCollider) {
                switch (this.colliderType) {
                    case Tile.ColliderType.BoxCollider2D:
                        tileGO.AddComponent<BoxCollider2D>();
                        break;

                    case Tile.ColliderType.BoxCollider3D:
                        tileGO.AddComponent<BoxCollider>();
                        break;
                }
            }

            // Attach prefab to tile?
            if (this.attachPrefab != null) {
                GameObject attachment = InstantiatePrefabForTile(this.attachPrefab, tile, context.TileSystem);
                if (attachment != null) {
                    attachment.transform.SetParent(tileGO.transform);
                }
            }
        }

        /// <inheritdoc/>
        protected internal override void ApplyTransforms(IBrushContext context, TileData tile, Brush transformer)
        {
            var tileSystem = context.TileSystem;
            var tileTransform = tile.gameObject.transform;

            // Place tile into its respective chunk.
            // NOTE: Chunk will definately exist at this stage.
            Transform newParent = tileSystem.GetChunkFromTileIndex(context.Row, context.Column).transform;
            tileTransform.SetParent(newParent, false);

            // Calculate position of tile within system.
            tileTransform.localPosition = tileSystem.LocalPositionFromTileIndex(context.Row, context.Column, true);
            // Turn tile to face away from system and rotate tile to align with system.
            tileTransform.localRotation = tileSystem.CalculateSimpleRotation(TileFacing.Sideways, tile.Rotation) * MathUtility.AngleAxis_180_Up;
            // Adjust scale of tile container.
            tileTransform.localScale = tileSystem.CalculateCellSizeScale(tile.Rotation);

            // Transform attachment?
            if (tileTransform.childCount == 1) {
                var attachmentTransform = tileTransform.GetChild(0);

                Matrix4x4 convertSystemToAttachmentSpaceMatrix = tileTransform.worldToLocalMatrix * tileSystem.transform.localToWorldMatrix;
                int rotation = this.applySimpleRotationToAttachment ? tile.Rotation : 0;

                Matrix4x4 attachmentMatrix = convertSystemToAttachmentSpaceMatrix * transformer.GetTransformMatrix(tileSystem, context.Row, context.Column, rotation, this.attachPrefab.transform);
                MathUtility.SetTransformFromMatrix(attachmentTransform, ref attachmentMatrix);
            }
        }

        /// <inheritdoc/>
        protected internal override void PostProcessTile(IBrushContext context, TileData tile)
        {
            if (tile == null) {
                return;
            }

            var go = tile.gameObject;

            if (go != null) {
                // Assign tag and layer to tile.
                go.tag = this.tag;
                go.layer = this.layer;

                InternalUtility.HideEditorWireframe(go);
            }
        }


        #region Immediate Preview

        /// <summary>
        /// Helper method to draw immediate preview of tileset tile.
        /// </summary>
        /// <remarks>
        /// <para>Attached prefab will also be included in preview when applicable.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being previewed.</param>
        /// <param name="previewMaterial">Material to use for preview.</param>
        /// <param name="previewTile">Data for preview tile.</param>
        /// <param name="transformer">Brush used to transform painted tile.
        /// The <see cref="Brush.scaleMode"/> and <see cref="Brush.applyPrefabTransform"/>
        /// fields of transform brush should be used. <c>transformer</c> may refer to this
        /// brush, or it may refer to another (like alias or oriented for example).</param>
        protected void DrawImmediateTilePreview(IBrushContext context, Material previewMaterial, TileData previewTile, Brush transformer)
        {
            var atlasTexture = Tileset.AtlasTexture;
            if (atlasTexture == null) {
                return;
            }

            // Prepare mesh for tile background.
            Rect texCoords = Tileset.CalculateTexCoords(previewTile.tilesetIndex);
            var mesh = ImmediatePreviewUtility.UpdateQuadPreviewMesh(texCoords);

            // Compute matrix for tile background.
            var tileSystem = context.TileSystem;

            Matrix4x4 matrix;
            tileSystem.CalculateTileMatrix(out matrix, context.Row, context.Column, TileFacing.Sideways, previewTile.Rotation);
            MathUtility.MultiplyMatrixByScale(ref matrix, tileSystem.CalculateCellSizeScale(previewTile.Rotation));

            matrix = ImmediatePreviewUtility.Matrix * matrix;

            // Draw tile background.
            previewMaterial.mainTexture = atlasTexture;
            ImmediatePreviewUtility.DrawNow(previewMaterial, mesh, matrix);

            // Draw preview of tile attachment?
            if (this.attachPrefab != null) {
                int rotation = this.applySimpleRotationToAttachment ? previewTile.PaintedRotation : 0;

                var attachmentTransform = this.attachPrefab.transform;
                matrix = tileSystem.transform.localToWorldMatrix * transformer.GetTransformMatrix(tileSystem, context.Row, context.Column, rotation, attachmentTransform);

                ImmediatePreviewUtility.DrawNow(previewMaterial, attachmentTransform, matrix, context.Brush as IMaterialMappings);
            }
        }

        /// <inheritdoc/>
        public override void OnDrawImmediatePreview(IBrushContext context, TileData previewTile, Material previewMaterial, Brush transformer)
        {
            this.DrawImmediateTilePreview(context, previewMaterial, previewTile, transformer);
        }

        #endregion
    }
}
