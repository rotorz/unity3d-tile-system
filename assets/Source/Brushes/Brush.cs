// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// A brush is a template that defines how tiles can be painted and maintained on a
    /// tile system. Brushes are typically used in conjunction with tools when using the
    /// editor, however it is useful to note that brushes can also be orchestrated by
    /// custom scripts at runtime.
    /// </summary>
    /// <intro>
    /// <para>For information regarding creation and usage of brushes please refer to
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Brushes">Brushes</a>
    /// section of user guide.</para>
    /// </intro>
    /// <seealso cref="OrientedBrush"/>
    /// <seealso cref="AliasBrush"/>
    /// <seealso cref="TilesetBrush"/>
    /// <seealso cref="AutotileBrush"/>
    /// <seealso cref="EmptyBrush"/>
    public abstract class Brush : ScriptableObject, IDesignableObject, ISerializationCallbackReceiver
    {
        #region Shared Context

        private static GenericBrushContext s_SharedContext = new GenericBrushContext();

        /// <exclude/>
        public static IBrushContext GetSharedContext(Brush brush, TileSystem system, int row, int column)
        {
            var context = s_SharedContext;
            context.brush = brush;
            context.system = system;
            context.row = row;
            context.column = column;
            return context;
        }

        /// <exclude/>
        public static IBrushContext GetSharedContext(Brush brush, TileSystem system, TileIndex index)
        {
            var context = s_SharedContext;
            context.brush = brush;
            context.system = system;
            context.row = index.row;
            context.column = index.column;
            return context;
        }

        #endregion


        #region Object Factory

        /// <summary>
        /// Instantate prefab for tile using the current object factory. Uses the runtime object
        /// factory in play mode or the editor object factory in edit mode.
        /// </summary>
        /// <param name="prefab">The prefab asset.</param>
        /// <param name="tile">Data of associated tile.</param>
        /// <param name="system">Associated tile system.</param>
        /// <returns>
        /// The instantiated game object; or a value of <c>null</c>.
        /// </returns>
        /// <seealso cref="IObjectFactory.InstantiatePrefab">IObjectFactory.DestroyObject</seealso>
        public static GameObject InstantiatePrefabForTile(GameObject prefab, TileData tile, TileSystem system)
        {
            IObjectFactory factory = InternalUtility.Instance.ResolveObjectFactory();
            return factory.InstantiatePrefab(prefab, SharedObjectFactoryContext.GetShared(tile, system));
        }

        /// <summary>
        /// Destroy game object that is associated with tile using the current object factory.
        /// Uses the runtime object factory in play mode or the editor object factory in edit mode.
        /// </summary>
        /// <param name="tile">Data of associated tile.</param>
        /// <param name="system">Associated tile system.</param>
        /// <seealso cref="IObjectFactory.DestroyObject">IObjectFactory.DestroyObject</seealso>
        public static void DestroyTileGameObject(TileData tile, TileSystem system)
        {
            if (tile == null || tile.gameObject == null) {
                return;
            }

            IObjectFactory factory = InternalUtility.Instance.ResolveObjectFactory();
            factory.DestroyObject(tile.gameObject, SharedObjectFactoryContext.GetShared(tile, system));
        }

        #endregion


        #region ISerializationCallbackReceiver Implementation

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.OnBeforeSerialize();
        }

        /// <summary>
        /// See Unity documentation <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>
        /// for further information regarding this method.
        /// </summary>
        /// <remarks>
        /// <para>Always inherit base logic:</para>
        /// <code language="csharp"><![CDATA[
        /// protected override void OnAfterDeserialize()
        /// {
        ///     base.OnAfterDeserialize();
        ///
        ///     // Custom logic...
        /// }
        /// ]]></code>
        /// </remarks>
        protected virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// See Unity documentation <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>
        /// for further information regarding this method.
        /// </summary>
        /// <remarks>
        /// <para>Always inherit base logic:</para>
        /// <code language="csharp"><![CDATA[
        /// protected override void OnBeforeSerialize()
        /// {
        ///     base.OnBeforeSerialize();
        ///
        ///     // Custom logic...
        /// }
        /// ]]></code>
        /// </remarks>
        protected virtual void OnBeforeSerialize()
        {
        }

        #endregion


        // Used internally by brush database to determine whether brush is ready for use.
        [NonSerialized]
        public bool _ready = false;


        /// <summary>
        /// Indicates that a random variation should be used.
        /// </summary>
        /// <example>
        /// This constant can be used with <see cref="Paint">Paint</see> and is in fact the
        /// default when no variation is specified.
        /// <code language="csharp"><![CDATA[
        /// Brush brush = GetSelectedBrush();
        /// brush.Paint(tileSystem, row, column, RANDOM_VARIATION);
        /// ]]></code>
        /// </example>
        public const byte RANDOM_VARIATION = byte.MaxValue;


        /// <inheritdoc/>
        string IDesignableObject.DisplayName {
            get { return this.name; }
        }

        /// <inheritdoc/>
        public abstract string DesignableType { get; }

        /// <inheritdoc/>
        string IHistoryObject.HistoryName {
            get { return (this as IDesignableObject).DisplayName + " : " + this.DesignableType; }
        }

        /// <inheritdoc/>
        bool IHistoryObject.Exists {
            get { return this != null; }
        }


        /// <summary>
        /// Indicates whether immediate preview should be disabled for this brush.
        /// </summary>
        /// <remarks>
        /// <para>This is useful for occassions where the immediate preview is more of an
        /// annoyance than of use.</para>
        /// </remarks>
        public bool disableImmediatePreview = false;

        /// <summary>
        /// Allows custom preview image to be assigned to brush.
        /// </summary>
        /// <remarks>
        /// <para>Custom preview images can be used in three different ways:</para>
        /// <list type="bullet">
        ///    <item>Rendering brush previews for editor user interfaces at design time.</item>
        ///    <item>Rendering custom user interfaces at runtime. This might be useful
        ///    if creating a custom in-game level designer.</item>
        ///    <item>Both for rendering brush previews at design time and for in-game
        ///    user interfaces.</item>
        /// </list>
        /// <para>Ensure that <see cref="customPreviewDesignTime"/> is set to <c>true</c>
        /// if custom preview image is to be shown in editor user interfaces at design
        /// time. If custom preview images are only required at design time, ensure that
        /// preview image asset is placed within an "Editor" directory to exclude it from
        /// builds.</para>
        /// <para>Specify <c>null</c> to assume default preview rendering functionality.</para>
        /// </remarks>
        public Texture2D customPreviewImage;
        /// <summary>
        /// Indicates whether custom preview should be used at design time.
        /// </summary>
        /// <remarks>
        /// <para>Custom preview will be used at design time in preference to the default
        /// preview rendering functionality when set to <c>true</c>. If custom preview
        /// image is only required at design time, ensure that the preview image asset is
        /// placed within an "Editor" directory to exclude it from builds.</para>
        /// <para>Custom preview images can also be useful at runtime when implementing
        /// custom level designers. Custom preview images can be accessed by custom
        /// runtime scripts regardless of the value of this property.</para>
        /// </remarks>
        public bool customPreviewDesignTime = true;


        [SerializeField, FormerlySerializedAs("_static")]
        private bool isStatic;
        [SerializeField, FormerlySerializedAs("_smooth")]
        private bool isSmooth;


        /// <summary>
        /// Gets or sets a value that indicates if brush is static.
        /// </summary>
        /// <remarks>
        /// <para>Tiles painted with brushes that are marked as static can often be merged,
        /// with vertex snapping if threshold is specified, when tile system is built.</para>
        /// <para>This does not apply to tiles that are painted using procedural tileset
        /// brushes.</para>
        /// </remarks>
        public bool Static {
            get { return this.isStatic; }
            set { this.isStatic = value; }
        }
        /// <summary>
        /// Gets or sets a value that indicates whether the normals of tiles painted using
        /// brush should be smoothed when tile system is built.
        /// </summary>
        /// <remarks>
        /// <para>When set to <c>true</c>, the meshes of the game objects that are attached
        /// to tiles can be snapped and smoothed within the threshold specified using the
        /// <see cref="TileSystem.vertexSnapThreshold"/> field.</para>
        /// </remarks>
        public bool Smooth {
            get { return this.isSmooth; }
            set { this.isSmooth = value; }
        }


        /// <summary>
        /// Visibility of brush in user interfaces.
        /// </summary>
        public BrushVisibility visibility = BrushVisibility.Shown;
        /// <summary>
        /// Group that brush belong to.
        /// </summary>
        /// <remarks>
        /// <para>Brush groups can be used in conjunction with brush coalescing rules to
        /// control the way in which the tiles of multiple brushes orientate against one
        /// another.</para>
        /// <para>For example, it is possible to have one brush (A) orientate against
        /// another (B) whilst brush B does not orientate against brush A.</para>
        /// </remarks>
        public int group;

        /// <summary>
        /// Indicates whether brush should override the layer of game objects that are
        /// attached to painted tiles when applicable.
        /// </summary>
        /// <remarks>
        /// <para>Specifying <c>true</c> allows this brush to override the layer property
        /// of painted tile game objects. This only applies to tiles that have an attached
        /// game object.</para>
        /// </remarks>
        public bool overrideLayer = false;
        /// <summary>
        /// Layer to assign to game objects that are attached to painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>With most brush types the value of this property is only applied when
        /// <see cref="overrideLayer"/> is set to <c>true</c>.</para>
        /// </remarks>
        public int layer;

        /// <summary>
        /// Indicates whether brush should override the tag of game objects that are
        /// attached to painted tiles when applicable.
        /// </summary>
        /// <remarks>
        /// <para>Specifying <c>true</c> allows this brush to override the tag property of
        /// painted tile game objects. This only applies to tiles that have an attached
        /// game object.</para>
        /// </remarks>
        public bool overrideTag = false;
        /// <summary>
        /// Tag to assign to game objects that are attached to painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>With most brush types the value of this property is only applied when
        /// <see cref="overrideTag"/> is set to <c>true</c>.</para>
        /// </remarks>
        public string tag = "Untagged";

        [SerializeField, FormerlySerializedAs("category"), FormerlySerializedAs("_categoryId")]
        private int categoryId;

        /// <summary>
        /// Gets or sets identifier of the category that the brush belongs to.
        /// </summary>
        /// <remarks>
        /// <para>Brushes can be organized into categories which can then be used to
        /// filter brush listings. To categorize a brush simply specify the category
        /// identifier, or a value of zero if brush should be uncategorized.</para>
        /// </remarks>
        public int CategoryId {
            get { return this.categoryId; }
            set { this.categoryId = value; }
        }


        /// <summary>
        /// Indicates if legacy behaviour should be assumed when painting tiles on
        /// tile systems that have sideways facing tiles.
        /// </summary>
        /// <remarks>
        /// <para>Originally tiles were rotated by an additional 180° degrees for sideways
        /// facing tile systems though this provided inconsistent results for Unity 4.3 sprites
        /// and so this behaviour was changed.</para>
        /// <para>This field is automatically set to <c>true</c> for all brushes which were
        /// created before this change was made.</para>
        /// </remarks>
        /// <seealso cref="TileFacing.Sideways">TileFacing.Sideways</seealso>
        public bool forceLegacySideways = true;

        /// <summary>
        /// Indicates if this brush should override the transforms of target brushes.
        /// </summary>
        /// <remarks>
        /// <para>This field is not applicable to all brushes.</para>
        /// </remarks>
        public bool overrideTransforms = false;

        /// <summary>
        /// Indicates how painted tiles should be scaled.
        /// </summary>
        /// <remarks>
        /// <para>Changes made to this field will not be applied to previously painted
        /// tiles until they are refreshed.</para>
        /// </remarks>
        public ScaleMode scaleMode = ScaleMode.DontTouch;

        /// <summary>
        /// Indicates when prefab transform should be used to transform painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>Tiles which are painted from prefabs each have a transform component
        /// which is usually ignored by brushes. Setting this field to true indicates
        /// that the transform component should be used to offset, rotate and scale
        /// painted tiles.</para>
        /// <para>Changes made to this field will not be immediately applied to
        /// previously painted tiles though existing tiles can be refreshed.</para>
        /// <para>This field only applies to brushes that paint instances of prefabs.</para>
        /// </remarks>
        public bool applyPrefabTransform;

        /*/// <summary>
        /// Offset to apply when transforming painted tiles.
        /// </summary>
        public Vector3 transformOffset;

        /// <summary>
        /// Rotation to apply when transforming painted tiles.
        /// </summary>
        public Quaternion transformRotate;*/

        /// <summary>
        /// Scale to apply when transforming painted tiles.
        /// </summary>
        public Vector3 transformScale = Vector3.one;


        /// <summary>
        /// Bit mask representing custom user flags.
        /// </summary>
        /// <remarks>
        /// <para>First 16 bits of integer represent custom user flags.</para>
        /// </remarks>
        [SerializeField, HideInInspector]
        protected int userFlags;


        public virtual int TileFlags {
            get { return this.userFlags; }
            set { this.userFlags = value; }
        }


        /// <summary>
        /// Semi-colon delimited list of user flag names.
        /// </summary>
        /// <remarks>
        /// <para>Flag names <strong>must not</strong> include semi-colon characters.</para>
        /// </remarks>
        [SerializeField, HideInInspector]
        private string userFlagLabels;

        /// <summary>
        /// Gets or sets the array of user-defined flag labels.
        /// </summary>
        /// <remarks>
        /// <para>Empty or <c>null</c> entries indicate that user-defined label has not
        /// been specified for flag. Project-wide flag labels are not automatically
        /// assumed.</para>
        /// <para>The result of this property should be cached where possible.</para>
        /// </remarks>
        /// <value>
        /// Array of strings containing exactly 16 entries.
        /// </value>
        public string[] UserFlagLabels {
            get {
                if (string.IsNullOrEmpty(this.userFlagLabels)) {
                    return new string[16];
                }

                string[] split = this.userFlagLabels.Split(';');
                if (split.Length == 16) {
                    return split;
                }

                // Copy a maximum of 16 flag labels!
                int copyCount = Mathf.Clamp(split.Length, 0, 16);

                string[] labels = new string[16];
                for (int i = 0; i < copyCount; ++i) {
                    labels[i] = split[i];
                }
                return labels;
            }
            set {
                if (value.Length != 16) {
                    throw new ArgumentException("An array of exactly 16 user flag labels must be specified.", "value");
                }

                // Filter values; it's not possible to use semi-colons in user flag labels
                // since that is used as a delimiter.
                string[] filteredLabels = value.Select(
                    label => !string.IsNullOrEmpty(label)
                        ? label.Replace(";", "")
                        : ""
                ).ToArray();

                this.userFlagLabels = string.Join(";", filteredLabels);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether painted tiles should be flagged as
        /// solid.
        /// </summary>
        /// <remarks>
        /// <para>Tile can be marked as solid to assist manual tile based collision
        /// detection or pathfinding in custom user scripts.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if solid; otherwise <c>false</c>.
        /// </value>
        /// <seealso cref="TileData.SolidFlag">TileData.SolidFlag</seealso>
        /// <seealso cref="TileSystem.TileTraceSolid">TileSystem.TileTraceSolid</seealso>
        public bool SolidFlag {
            get { return (this.TileFlags & TileData.FLAG_SOLID) != 0; }
            set {
                if (value) {
                    this.TileFlags |= TileData.FLAG_SOLID;
                }
                else {
                    this.TileFlags &= ~TileData.FLAG_SOLID;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether layer and tag properties can be overridden by
        /// this <see cref="Rotorz.Tile.Brush"/>.
        /// </summary>
        /// <remarks>
        /// <para>This property is used by brush designer interfaces to determine whether
        /// tag and layer properties can be overridden. This capability only applies to
        /// certain brush types.</para>
        /// <para>When this property yields a value of <c>true</c> a small toggle button
        /// is shown adjacent to the "Tag" and "Layer" fields allowing them to explicitly
        /// override the properties of associated brushes and prefabs.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if layer and tag can be overridden; otherwise <c>false</c>.
        /// </value>
        /// <seealso cref="overrideTag"/>
        /// <seealso cref="overrideLayer"/>
        public abstract bool CanOverrideTagAndLayer { get; }

        /// <summary>
        /// Gets a value indicating whether brush automatically orientates tiles.
        /// </summary>
        /// <remarks>
        /// <para>Must return a value of <c>true</c> for brushes that make use of
        /// <see cref="TileData.orientationMask">TileData.orientationMask</see>.</para>
        /// </remarks>
        public abstract bool PerformsAutomaticOrientation { get; }

        /// <summary>
        /// Gets a value indicating whether to use wireframe cursor when painting in the
        /// editor. When <c>false</c> an alternative representation can be used instead.
        /// </summary>
        /// <remarks>
        /// <para>The wireframe cursor is generally better when painting 3D tiles whereas
        /// the alternative is generally better for painting 2D tiles.</para>
        /// </remarks>
        public virtual bool UseWireIndicatorInEditor {
            get { return true; }
        }

        private static TileData s_TempTile = new TileData();

        /// <summary>
        /// Prepare tile data.
        /// </summary>
        /// <remarks>
        /// <para>Tile data is generated and stored within the tile system data structure
        /// before a tile is actually created. The tile data is then later used by brushes
        /// to create the relevant output.</para>
        /// <para>This method can be overridden for custom selection of tile variation
        /// and orientation. Custom implementations must mark tile as non-empty when tile
        /// is to be painted as demonstrated in the following example.</para>
        /// <para>This method must only manipulate the specified tile.</para>
        /// </remarks>
        /// <example>
        /// <para>Example of custom implementation:</para>
        /// <code language="csharp"><![CDATA[
        /// protected override void PrepareTileData(IBrushContext context, TileData tile, int variationIndex, bool refresh)
        /// {
        ///     // Assume the default orientation and variation.
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">Tile that is to be prepared.</param>
        /// <param name="variationIndex">Hint at the desired variation index, however the
        /// value of this parameter can be ignored. For example, this parameter may be ignored
        /// if the specified variation is invalid.</param>
        protected internal virtual void PrepareTileData(IBrushContext context, TileData tile, int variationIndex)
        {
            // Note: Default implementation ignores `variationIndex` input
        }

        /// <summary>
        /// Create visual representation of tile.
        /// </summary>
        /// <remarks>
        /// <para>This method can create and attach a game object to the newly painted
        /// tile by assigning to the field <see cref="TileData.gameObject">tile.gameObject</see>.
        /// Custom implementations of this method can be left empty if no game object needs
        /// to be attached to tile.</para>
        /// <para>This method must only manipulate the specified tile.</para>
        /// </remarks>
        /// <example>
        /// <para>The following demonstrates how to instantiate a prefab and then associate
        /// the resulting game object with the tile. The associated game object can be
        /// later transformed by <see cref="ApplyTransforms"/>.</para>
        /// <code language="csharp"><![CDATA[
        /// // Field allows a prefab to be attached to tile.
        /// public GameObject attachPrefab;
        ///
        ///
        /// protected override void CreateTile(IBrushContext context, TileData tile)
        /// {
        ///     if (attachPrefab == null) {
        ///         return;
        ///     }
        ///
        ///     tile.gameObject = Object.Instantiate(attachPrefab);
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        protected internal abstract void CreateTile(IBrushContext context, TileData tile);

        /// <summary>
        /// Apply transforms to newly painted tile and orientate against tile system.
        /// </summary>
        /// <remarks>
        /// <para>Only called for tiles that have an associated game object.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        /// <param name="transformer">Brush used to transform painted tile.
        /// The <see cref="scaleMode"/> and <see cref="applyPrefabTransform"/> fields of transform
        /// brush should be used. <c>transformer</c> may refer to this brush, or it may refer
        /// to another (like alias or oriented for example).</param>
        protected internal virtual void ApplyTransforms(IBrushContext context, TileData tile, Brush transformer)
        {
            // Tile transform will initially represent prefab offset.
            var tileTransform = tile.gameObject.transform;
            // Get transformation matrix for tile (in local space).
            Matrix4x4 m = transformer.GetTransformMatrix(context.TileSystem, context.Row, context.Column, tile.Rotation, tileTransform);

            // Place tile into its respective chunk AFTER we have had the chance to
            // capture prefab offset from new tile transform instance.
            // NOTE: Chunk will definately exist at this stage.
            Transform newParent = context.TileSystem.GetChunkFromTileIndex(context.Row, context.Column).transform;
            tileTransform.SetParent(newParent, false);

            MathUtility.SetTransformFromMatrix(tileTransform, ref m);

            //!HACK: Force relatively small inaccuracy to disconnect scale from prefab.
            Vector3 s = tileTransform.localScale;
            tileTransform.localScale = new Vector3(
                MathUtility.NextAfter(s.x),
                MathUtility.NextAfter(s.y),
                MathUtility.NextAfter(s.z)
            );
        }

        internal static bool IdentityManualOffset(out Vector3 offsetPosition, out Quaternion offsetRotation, out Vector3 offsetScale)
        {
            offsetPosition = default(Vector3);
            offsetRotation = MathUtility.IdentityQuaternion;
            offsetScale = Vector3.one;
            return false;
        }

        /// <summary>
        /// Calculates offset from actual tile position and where tile would normally be
        /// positioned by brush. This is used to preserve manually tweaked offsets when
        /// tiles are refreshed.
        /// </summary>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        /// <param name="transform">Tile transform component.</param>
        /// <param name="offsetPosition">Manual position offset.</param>
        /// <param name="offsetRotation">Manual rotation offset.</param>
        /// <param name="offsetScale">Manual scale offset.</param>
        /// <param name="transformer">Brush used to transform painted tile.</param>
        /// <returns>
        /// A value of <c>true</c> if an offset was calculated; otherwise <c>false</c>.
        /// </returns>
        protected internal virtual bool CalculateManualOffset(IBrushContext context, TileData tile, Transform transform, out Vector3 offsetPosition, out Quaternion offsetRotation, out Vector3 offsetScale, Brush transformer)
        {
            Matrix4x4 normalPlacement = transformer.GetTransformMatrix(context.TileSystem, context.Row, context.Column, tile.Rotation, null);
            MathUtility.DecomposeMatrix(ref normalPlacement, out offsetPosition, out offsetRotation, out offsetScale);

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

        /// <summary>
        /// Gets matrix that describes transformation of tile painted using brush in local
        /// space of tile system.
        /// </summary>
        /// <param name="system">Target tile system.</param>
        /// <param name="row">Zero-based index of row.</param>
        /// <param name="column">Zero-based index of column.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°).</param>
        /// <param name="prefab">(Optional) Transform component of prefab which can be used when
        /// <see cref="applyPrefabTransform"/> is selected.</param>
        public Matrix4x4 GetTransformMatrix(TileSystem system, int row, int column, int rotation, Transform prefab = null)
        {
            Matrix4x4 tileMatrix;
            system.CalculateTileMatrix(out tileMatrix, row, column, rotation);

            // Apply prefab transform to brush offset?
            if (this.applyPrefabTransform && prefab != null) {
                tileMatrix *= Matrix4x4.TRS(prefab.localPosition, prefab.localRotation, prefab.localScale);
            }

            // Apply scale mode to transform?
            switch (this.scaleMode) {
                case ScaleMode.UseCellSize:
                    Vector3 cellSizeScale = system.CalculateCellSizeScale(rotation);
                    MathUtility.MultiplyMatrixByScale(ref tileMatrix, cellSizeScale);
                    break;

                case ScaleMode.Custom:
                    MathUtility.MultiplyMatrixByScale(ref tileMatrix, this.transformScale);
                    break;
            }

            // Fix for "Force Legacy Sideways" behaviour?
            if (this.forceLegacySideways && system.TilesFacing == TileFacing.Sideways) {
                tileMatrix *= MathUtility.RotateUpAxisBy180Matrix;
            }

            return tileMatrix;
        }

        /// <summary>
        /// Gets matrix that describes transformation of tile painted using brush in local
        /// space of tile system.
        /// </summary>
        /// <inheritdoc cref="GetTransformMatrix(TileSystem, int, int, int, Transform)"/>
        /// <param name="index">Index of tile.</param>
        public Matrix4x4 GetTransformMatrix(TileSystem system, TileIndex index, int rotation, Transform prefab = null)
        {
            return this.GetTransformMatrix(system, index.row, index.column, rotation, prefab);
        }

        /// <summary>
        /// Calculate offset translation, rotation and scale from current state of tile
        /// which can be applied to the transform component of a tile prefab (for prefab
        /// offsets).
        /// </summary>
        /// <remarks>
        /// <para>This method is essential for the "Use as Prefab Offset" feature.</para>
        /// </remarks>
        /// <param name="system">Target tile system.</param>
        /// <param name="row">Zero-based index of row.</param>
        /// <param name="column">Zero-based index of column.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°).</param>
        /// <param name="matrix">Matrix describing transform of attached game object.</param>
        /// <param name="positionOffset">Manual position offset.</param>
        /// <param name="rotationOffset">Manual rotation offset.</param>
        /// <param name="scaleOffset">Manual scale offset.</param>
        public void CalculatePrefabOffset(TileSystem system, int row, int column, int rotation, Matrix4x4 matrix, out Vector3 positionOffset, out Quaternion rotationOffset, out Vector3 scaleOffset)
        {
            // At the time of writing this comment the implementation of `GetTransformMatrix`
            // essentially calculates D = A * B * C. The purpose of this method is to find
            // the value of the matrix B = A^-1 * D * C^-1.

            // `CalculateManualOffset` finds the offset AFTER prefab offsets have already
            // been applied, whereas this method finds the offset as though prefab offsets
            // were never actually applied.

            // Remove fix for "Force Legacy Sideways" behaviour?
            if (this.forceLegacySideways && system.TilesFacing == TileFacing.Sideways) {
                matrix *= MathUtility.RotateUpAxisBy180Matrix;
            }

            // Remove scale mode from transform?
            switch (this.scaleMode) {
                case ScaleMode.UseCellSize:
                    Vector3 cellSizeScale = system.CalculateCellSizeScale(rotation);
                    MathUtility.MultiplyMatrixByScale(ref matrix, MathUtility.Inverse(cellSizeScale));
                    break;

                case ScaleMode.Custom:
                    MathUtility.MultiplyMatrixByScale(ref matrix, MathUtility.Inverse(this.transformScale));
                    break;
            }

            // Remove tile offset from transform.Matrix4x4 tileMatrix;
            Matrix4x4 tileMatrix;
            system.CalculateTileMatrix(out tileMatrix, row, column, rotation);
            matrix = tileMatrix.inverse * matrix;

            // Extract manual offset from transform.
            MathUtility.DecomposeMatrix(ref matrix, out positionOffset, out rotationOffset, out scaleOffset);
        }

        /// <summary>
        /// Post process newly painted tile.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation achives the following:</para>
        /// <list type="bullet">
        ///    <item>Associates brush with tile data.</item>
        ///    <item>Remaps materials of nested <c>MeshRenderer</c> components.</item>
        ///    <item>Hides wireframe of tile in edit mode.</item>
        /// </list>
        /// <para><strong>Important:</strong> Always call base method at start of function
        /// when overridden to add new functionality. Failure to do this may result with
        /// undesirable side effects.</para>
        /// <para>This method must only manipulate the specified tile.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        protected internal virtual void PostProcessTile(IBrushContext context, TileData tile)
        {
            if (tile == null) {
                return;
            }

            if (tile.gameObject != null) {
                var go = tile.gameObject;

                var materialMappings = this as IMaterialMappings;
                if (materialMappings != null) {
                    this.RemapMaterials(go, materialMappings);
                }

                // Assign tag and layer to tile.
                if (this.overrideTag) {
                    go.tag = this.tag;
                }
                if (this.overrideLayer) {
                    go.layer = this.layer;
                }

                InternalUtility.HideEditorWireframe(go);
            }
        }

        private static List<MeshRenderer> s_TempRendererList = new List<MeshRenderer>();

        private void RemapMaterials(GameObject go, IMaterialMappings mappings)
        {
            Material[] from = mappings.MaterialMappingFrom;
            Material[] to = mappings.MaterialMappingTo;

            int count = (from != null && to != null)
                ? Mathf.Min(from.Length, to.Length)
                : 0;

            if (count == 0)
                return;

            // Retrieve all mesh renderers from new tile.
            try {
                go.GetComponentsInChildren<MeshRenderer>(s_TempRendererList);

                foreach (var renderer in s_TempRendererList) {
                    Material[] sharedMaterials = renderer.sharedMaterials;
                    for (int i = 0; i < count; ++i) {
                        // Perform remapping for each material in renderer.
                        for (int j = 0; j < sharedMaterials.Length; ++j) {
                            if (sharedMaterials[j] == from[i]) {
                                sharedMaterials[j] = to[i];
                            }
                        }
                    }
                    renderer.sharedMaterials = sharedMaterials;
                }
            }
            finally {
                s_TempRendererList.Clear();
            }
        }

        /// <summary>
        /// Helper method that creates and processes a tile from tile data.
        /// </summary>
        /// <remarks>
        /// <para>This method is used internally when painting, refreshing and cycling tiles.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="tile">The tile.</param>
        private void PaintHelper(IBrushContext context, TileData tile)
        {
            var tileSystem = context.TileSystem;
            var chunk = tileSystem.GetChunkFromTileIndex(context.Row, context.Column);

            // Bypass when bulk edit mode is active.
            if (tileSystem.BulkEditMode) {
                tile.Dirty = true;
                chunk.Dirty = true;
            }
            else {
                this.CreateTile(context, tile);

                // Apply transforms to tile when game object is produced.
                if (tile.gameObject != null) {
                    this.ApplyTransforms(context, tile, this);
                    tile.HasGameObject = true;

                    // Mark game object as static?
                    if (this.Static) {
                        tile.gameObject.isStatic = this.Static;
                    }
                }
                else {
                    tile.HasGameObject = false;
                }

                tile.Dirty = false;

                // Perform final processing to tile.
                this.PostProcessTile(context, tile);

                // Invoke global event?
                PaintingUtility.RaiseTilePaintedEvent(context, tile);
            }

            // Does procedural mesh need to be updated?
            if (tile.Procedural) {
                chunk.ProceduralDirty = true;

                // Ensure that procedural mesh component is present.
                chunk.PrepareProceduralMesh();
            }
        }

        private TileData DoPaintTile(TileSystem system, int row, int column, TileData newTile, int variationIndex)
        {
            IBrushContext context = GetSharedContext(this, system, row, column);

            // Add user flags to tile.
            newTile.flags |= this.TileFlags;
            // Tile is not empty.
            newTile.Empty = false;

            TileData tile = system.GetTile(row, column);
            this.PrepareTileData(context, newTile, variationIndex);

            // Only proceed if replacement tile differs from existing tile.
            //
            // Note: Always overpaint tile that is missing its game object!
            //
            if (newTile.Empty || (tile != null && newTile.Equals(tile) && !tile.IsGameObjectMissing)) {
                return null;
            }

            // Update tile data (`tile` now refers to the new tile).
            tile = system.SetTileFrom(row, column, newTile);
            this.PaintHelper(context, tile);

            // Does procedural mesh need to be updated?
            if (tile.Procedural) {
                system.UpdateProceduralTiles();
            }

            return tile;
        }

        /// <summary>
        /// Paint tile using brush.
        /// </summary>
        /// <remarks>
        /// <para>This method can be used to paint tiles dynamically at runtime. Defaults
        /// to random variation when variation index is not specified.</para>
        /// <para>You may want to consider utilising the tile system bulk edit mode when
        /// painting and/or erasing multiple tiles in sequence (see <see cref="TileSystem.BeginBulkEdit"/>)
        /// for improved performance. Performance boost is most noticable when working
        /// with oriented and autotile brushes.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source code demonstrates how to paint a tile at runtime
        /// using a custom script:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// class CustomBehaviour : MonoBehaviour
        /// {
        ///     public TileSystem tileSystem;
        ///     public Brush brush;
        ///
        ///
        ///     private void Start()
        ///     {
        ///         // Paint tile at row 5, column 5.
        ///         this.brush.Paint(this.tileSystem, 5, 5);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="system">The tile system.</param>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="variationIndex">Zero-based index of variation to assume. Specify
        /// <see cref="RANDOM_VARIATION"/> to assume a random variation.</param>
        /// <returns>
        /// A <see cref="TileData"/> object that describes the tile that was painted.
        /// Returns <c>null</c> if no tile is painted.
        /// </returns>
        public TileData Paint(TileSystem system, int row, int column, int variationIndex)
        {
            TileData newTile = s_TempTile;
            newTile.Clear();
            newTile.brush = this;

            return this.DoPaintTile(system, row, column, newTile, variationIndex);
        }

        /// <summary>
        /// Paint tile using brush.
        /// </summary>
        /// <inheritdoc cref="Paint(TileSystem, int, int, int)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public TileData Paint(TileSystem system, TileIndex index, int variationIndex)
        {
            return this.Paint(system, index.row, index.column, variationIndex);
        }

        /// <summary>
        /// Paint tile using brush.
        /// </summary>
        /// <inheritdoc cref="Paint(TileSystem, int, int, int)"/>
        public TileData Paint(TileSystem system, int row, int column)
        {
            return this.Paint(system, row, column, RANDOM_VARIATION);
        }

        /// <summary>
        /// Paint tile using brush.
        /// </summary>
        /// <inheritdoc cref="Paint(TileSystem, int, int, int)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public TileData Paint(TileSystem system, TileIndex index)
        {
            return this.Paint(system, index.row, index.column, RANDOM_VARIATION);
        }

        /// <summary>
        /// Paint tile with simple rotation transformation.
        /// </summary>
        /// <remarks>
        /// <para>This method can be used to paint tiles dynamically at runtime. Defaults
        /// to random variation when variation index is not specified.</para>
        /// <para>You may want to consider utilising the tile system bulk edit mode when
        /// painting and/or erasing multiple tiles in sequence (see <see cref="TileSystem.BeginBulkEdit"/>)
        /// for improved performance. Performance boost is most noticable when working
        /// with oriented and autotile brushes.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source code demonstrates how to paint a tile rotated at
        /// 90° at runtime using a custom script:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using UnityEngine;
        ///
        /// class CustomBehaviour : MonoBehaviour
        /// {
        ///     public TileSystem tileSystem;
        ///     public Brush brush;
        ///
        ///
        ///     private void Start()
        ///     {
        ///         // Paint tile at row 5, column 5 with rotation index 1 (90°).
        ///         this.brush.PaintWithSimpleRotation(this.tileSystem, 5, 5, 1);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="system">The tile system.</param>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="rotation">Zero-based index of simple rotation (0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°).</param>
        /// <param name="variationIndex">Zero-based index of variation to assume. Specify
        /// <see cref="RANDOM_VARIATION"/> to assume a random variation.</param>
        /// <returns>
        /// A <see cref="TileData"/> object that describes the tile that was painted.
        /// Returns <c>null</c> if no tile is painted.
        /// </returns>
        public TileData PaintWithSimpleRotation(TileSystem system, int row, int column, int rotation, int variationIndex)
        {
            // <param name="flip">Indicates whether tile should be flipped (only supported by procedural tileset brushes).</param>

            TileData newTile = s_TempTile;
            newTile.Clear();
            newTile.brush = this;
            newTile.PaintedRotation = rotation;
            newTile.Rotation = rotation;

            return this.DoPaintTile(system, row, column, newTile, variationIndex);
        }

        /// <summary>
        /// Paint tile with simple rotation transformation.
        /// </summary>
        /// <inheritdoc cref="PaintWithSimpleRotation(TileSystem, int, int, int, int)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public TileData PaintWithSimpleRotation(TileSystem system, TileIndex index, int rotation, int variationIndex)
        {
            return this.PaintWithSimpleRotation(system, index.row, index.column, rotation, variationIndex);
        }

        /// <summary>
        /// Paint tile with simple rotation transformation.
        /// </summary>
        /// <inheritdoc cref="PaintWithSimpleRotation(TileSystem, int, int, int, int)"/>
        public TileData PaintWithSimpleRotation(TileSystem system, int row, int column, int rotation)
        {
            return this.PaintWithSimpleRotation(system, row, column, rotation, RANDOM_VARIATION);
        }

        /// <summary>
        /// Paint tile with simple rotation transformation.
        /// </summary>
        /// <inheritdoc cref="PaintWithSimpleRotation(TileSystem, int, int, int, int)"/>
        /// <param name="index">Zero-based index of tile.</param>
        public TileData PaintWithSimpleRotation(TileSystem system, TileIndex index, int rotation)
        {
            return this.PaintWithSimpleRotation(system, index.row, index.column, rotation, RANDOM_VARIATION);
        }

        /// <summary>
        /// Gets a value indicating whether transform of attached game object can be
        /// preserved when refreshing tiles.
        /// </summary>
        protected virtual bool CanPreserveTransform {
            get { return true; }
        }

        /// <summary>
        /// Helper method that refreshes an existing tile.
        /// </summary>
        /// <remarks>
        /// <para>This method is used internally when painting, refreshing and cycling
        /// tiles.</para>
        /// </remarks>
        /// <param name="context">Describes context of tile that is being painted.</param>
        /// <param name="existing">Existing tile data (always provided).</param>
        /// <param name="replacement">Replacement tile data (always provided).</param>
        /// <param name="flags">A bitwise combination of RefreshFlags values.</param>
        private void RefreshHelper(IBrushContext context, TileData existing, TileData replacement, RefreshFlags flags)
        {
            bool preserveTransform = (flags & RefreshFlags.PreserveTransform) == RefreshFlags.PreserveTransform;
            Transform t = (preserveTransform && existing.gameObject != null)
                ? existing.gameObject.transform
                : null;

            // Calculate manual offset between existing prefab and existing tile.
            Vector3 manualOffsetPosition;
            Quaternion manualOffsetRotation;
            Vector3 manualOffsetScale;

            bool hasManualOffset;
            if (t != null) {
                hasManualOffset = this.CalculateManualOffset(context, existing, t, out manualOffsetPosition, out manualOffsetRotation, out manualOffsetScale, this);
            }
            else {
                hasManualOffset = false;
                IdentityManualOffset(out manualOffsetPosition, out manualOffsetRotation, out manualOffsetScale);
            }

            // Typically the same as `applyPrefabTransform`.
            bool preserveSolidFlag = existing.SolidFlag;
            int preserveUserFlags = existing.UserFlags;

            // Store tile data (from this point `existing` refers to the new tile).
            context.TileSystem.SetTileFrom(context.Row, context.Column, replacement);
            // Refresh tile!
            this.PaintHelper(context, existing);

            // Restore original position, rotation and scale?
            if (hasManualOffset && existing.gameObject != null) {
                t = existing.gameObject.transform;
                t.localPosition += manualOffsetPosition;
                t.localRotation *= manualOffsetRotation;
                t.localScale = Vector3.Scale(t.localScale, manualOffsetScale);
            }

            // Restore original flags if requested.
            if ((flags & RefreshFlags.PreservePaintedFlags) == RefreshFlags.PreservePaintedFlags) {
                existing.SolidFlag = preserveSolidFlag;
                existing.UserFlags = preserveUserFlags;
            }
        }

        /// <summary>
        /// Refresh a tile.
        /// </summary>
        /// <remarks>
        /// <para>Previously painted tiles can be refreshed to reflect changes that have
        /// been made to the brush that painted them. Changes may also be applicable to a
        /// tile when an adjacent tile has been altered.</para>
        /// </remarks>
        /// <param name="system">The tile system.</param>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="flags">A bitwise combination of RefreshFlags values.</param>
        /// <returns>
        /// A <see cref="TileData"/> object that describes the tile that was painted.
        /// Returns <c>null</c> if no tile is painted.
        /// </returns>
        public TileData Refresh(TileSystem system, int row, int column, RefreshFlags flags = RefreshFlags.None)
        {
            TileData existingTile = system.GetTile(row, column);
            if (existingTile == null) {
                return null;
            }

            IBrushContext context = GetSharedContext(this, system, row, column);

            // Prepare new tile data using a temporary object.
            TileData replacementTile = s_TempTile;
            replacementTile.Clear();
            replacementTile.brush = this;

            // Add user flags to tile.
            replacementTile.flags |= this.TileFlags;
            // Tile is not empty.
            replacementTile.Empty = false;

            // Preserve custom rotation index when refreshing tile.
            replacementTile.PaintedRotation = existingTile.PaintedRotation;
            replacementTile.Rotation = existingTile.Rotation;

            this.PrepareTileData(context, replacementTile, existingTile.variationIndex);
            if (replacementTile.Empty) {
                return null;
            }

            // Only proceed if replacement tile differs from existing tile?
            bool force = (flags & RefreshFlags.Force) == RefreshFlags.Force;
            if (!force && replacementTile.Equals(existingTile) && !existingTile.IsGameObjectMissing) {
                return null;
            }

            // Refresh using existing (current) and replacement (temporary).
            this.RefreshHelper(context, existingTile, replacementTile, flags);

            return existingTile;
        }

        /// <summary>
        /// Refresh a tile.
        /// </summary>
        /// <inheritdoc cref="Refresh(TileSystem, int, int, RefreshFlags)"/>
        /// <param name="index">Index of tile.</param>
        public TileData Refresh(TileSystem system, TileIndex index, RefreshFlags flags = RefreshFlags.None)
        {
            return this.Refresh(system, index.row, index.column, flags);
        }

        /// <summary>
        /// Count the number of tile variations.
        /// </summary>
        /// <param name="orientationMask">Mask which describes orientation of tile.</param>
        /// <returns>
        /// Count of tile variations.
        /// </returns>
        /// <seealso cref="OrientationUtility"/>
        public virtual int CountTileVariations(int orientationMask)
        {
            return 1;
        }

        /// <summary>
        /// Get random tile variation.
        /// </summary>
        /// <param name="orientationMask">Mask which describes orientation of tile.</param>
        /// <returns>
        /// Zero-based index of tile variation; returns a value of -1 if no variations
        /// are defined for the specified orientation.
        /// </returns>
        /// <seealso cref="OrientationUtility"/>
        public virtual int PickRandomVariationIndex(int orientationMask)
        {
            return 0;
        }

        /// <summary>
        /// Wrap index of next variation as needed (only attempts to wrap value once
        /// and then assumes the first variation).
        /// </summary>
        /// <param name="context">Describes context of brush.</param>
        /// <param name="tile">Current data for tile.</param>
        /// <param name="nextVariation">Zero-based index of next desired variation.</param>
        /// <returns>
        /// Zero-based index of variation.
        /// </returns>
        public static int WrapVariationIndexForCycle(IBrushContext context, TileData tile, int nextVariation)
        {
            int orientation = OrientationUtility.DetermineTileOrientation(context.TileSystem, context.Row, context.Column, context.Brush, tile.PaintedRotation);
            int variationCount = context.Brush.CountTileVariations(orientation);

            if (nextVariation < 0) {
                nextVariation = Mathf.Max(0, variationCount + nextVariation);
            }
            else if (nextVariation >= variationCount) {
                nextVariation -= variationCount;
                if (nextVariation >= variationCount) {
                    nextVariation = 0;
                }
            }

            return nextVariation;
        }

        /// <summary>
        /// Cycle through tile variations and/or rotation indices.
        /// </summary>
        /// <remarks>
        /// <para>Automatically wraps variation/rotation indices around bounds.</para>
        /// </remarks>
        /// <param name="system">The tile system.</param>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        /// <param name="nextRotation">Zero-based index of next desired rotation.</param>
        /// <param name="nextVariation">Zero-based index of next desired variation.</param>
        /// <returns>
        /// A <see cref="TileData"/> object that describes the tile that was painted.
        /// Returns <c>null</c> if no tile is painted.
        /// </returns>
        public TileData CycleWithSimpleRotation(TileSystem system, int row, int column, int nextRotation, int nextVariation)
        {
            TileData existingTile = system.GetTile(row, column);
            if (existingTile == null) {
                return null;
            }

            bool updateProcedural = existingTile.Procedural;

            IBrushContext context = GetSharedContext(this, system, row, column);

            // Prepare new tile data using a temporary object.
            TileData replacementTile = s_TempTile;
            replacementTile.Clear();
            replacementTile.brush = this;

            // Add user flags to tile.
            replacementTile.flags |= this.TileFlags;
            // Tile is not empty.
            replacementTile.Empty = false;

            // Cycle to desired rotation.
            if (nextRotation > 3) {
                nextRotation = 0;
            }
            if (nextRotation < 0) {
                nextRotation = 3;
            }
            replacementTile.PaintedRotation = nextRotation;
            replacementTile.Rotation = nextRotation;

            // Wrap variation index as needed.
            nextVariation = WrapVariationIndexForCycle(context, replacementTile, nextVariation);

            this.PrepareTileData(context, replacementTile, nextVariation);

            // Only proceed if replacement tile differs from existing tile?
            if (replacementTile.Empty || replacementTile.Equals(existingTile)) {
                return null;
            }

            // Refresh using existing (current) and replacement (temporary).
            this.RefreshHelper(context, existingTile, replacementTile, RefreshFlags.PreservePaintedFlags | RefreshFlags.PreserveTransform);

            // Does procedural mesh need to be updated?
            if (updateProcedural || replacementTile.Procedural) {
                system.UpdateProceduralTiles();
            }

            return existingTile;
        }

        /// <summary>
        /// Cycle through tile variations and/or rotation indices.
        /// </summary>
        /// <inheritdoc cref="CycleWithSimpleRotation(TileSystem, int, int, int, int)"/>
        /// <param name="index">Index of tile.</param>
        public TileData CycleWithSimpleRotation(TileSystem system, TileIndex index, int nextRotation, int nextVariation)
        {
            return this.CycleWithSimpleRotation(system, index.row, index.column, nextRotation, nextVariation);
        }

        /// <summary>
        /// Cycle through tile variations.
        /// </summary>
        /// <remarks>
        /// <para>Automatically wraps variation index around bounds.</para>
        /// </remarks>
        /// <inheritdoc cref="CycleWithSimpleRotation(TileSystem, int, int, int, int)"/>
        public TileData Cycle(TileSystem system, int row, int column, int nextVariation)
        {
            TileData existingTile = system.GetTile(row, column);
            if (existingTile == null) {
                return null;
            }

            return this.CycleWithSimpleRotation(system, row, column, existingTile.PaintedRotation, nextVariation);
        }

        /// <summary>
        /// Cycle through tile variations.
        /// </summary>
        /// <inheritdoc cref="CycleWithSimpleRotation(TileSystem, int, int, int, int)"/>
        /// <param name="index">Index of tile.</param>
        public TileData Cycle(TileSystem system, TileIndex index, int nextVariation)
        {
            return this.Cycle(system, index.row, index.column, nextVariation);
        }

        /// <summary>
        /// Get state of custom user flag.
        /// </summary>
        /// <param name="flag">Number of user flag (1-16)</param>
        /// <returns>
        /// A value of <c>true</c> if flag is on; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If flag number was out of range.
        /// </exception>
        /// <seealso cref="TileData.GetUserFlag"/>
        public bool GetUserFlag(int flag)
        {
            if (flag < 1 || flag > 16) {
                throw new ArgumentOutOfRangeException("Flag number was out of range. Number should be between 1 and 16 inclusive.");
            }
            return (this.TileFlags & (1 << --flag)) != 0;
        }

        /// <summary>
        /// Set state of custom user flag.
        /// </summary>
        /// <param name="flag">Number of user flag (1-16)</param>
        /// <param name="on">Specify <c>true</c> to set flag, or <c>false</c> to unset flag.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If flag number was out of range.
        /// </exception>
        /// <seealso cref="TileData.SetUserFlag"/>
        public void SetUserFlag(int flag, bool on)
        {
            if (flag < 1 || flag > 16) {
                throw new ArgumentOutOfRangeException("Flag number was out of range. Number should be between 1 and 16 inclusive.");
            }
            if (on) {
                this.TileFlags |= (1 << --flag);
            }
            else {
                this.TileFlags &= ~(1 << --flag);
            }
        }

        /// <summary>
        /// Gets the nth material from available renderers.
        /// </summary>
        /// <param name="n">Index of material to find.</param>
        /// <returns>
        /// The nth material; or <c>null</c> if no materials were detected.
        /// </returns>
        public virtual Material GetNthMaterial(int n)
        {
            return null;
        }


        #region Immediate Preview

        /// <summary>
        /// Draws preview of tile using the Unity graphics or gizmos class.
        /// </summary>
        /// <param name="context">Describes context of tile that is being previewed.</param>
        /// <param name="previewTile">Data for preview tile.</param>
        /// <param name="previewMaterial">Material to use for preview.</param>
        /// <param name="transformer">Brush used to transform painted
        /// tile. The <see cref="scaleMode"/> and <see cref="applyPrefabTransform"/> fields
        /// of transform brush should be used. <c>transformer</c> may refer to this brush,
        /// or it may refer to another (like alias or oriented for example).</param>
        public virtual void OnDrawImmediatePreview(IBrushContext context, TileData previewTile, Material previewMaterial, Brush transformer)
        {
        }

        #endregion


        #region Messages and Events

        /// <summary>
        /// Invoked when brush first becomes active.
        /// </summary>
        /// <remarks>
        /// <para>Always inherit base functionality when overridden.</para>
        /// </remarks>
        public virtual void Awake()
        {
        }

        #endregion
    }
}
