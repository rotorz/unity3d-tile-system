// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Empty tiles do not have any visual representation, game object or components. These
    /// can be useful when creating oriented tiles that require gaps (inner orientation, for
    /// example). A master brush "Empty Variation" is provided for this very purpose.
    /// </summary>
    /// <intro>
    /// <para>Refer to <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Empty-Brushes">Empty Brushes</a>
    /// section of user guide for further information.</para>
    /// </intro>
    /// <seealso cref="OrientedBrush"/>
    /// <seealso cref="AliasBrush"/>
    /// <seealso cref="TilesetBrush"/>
    /// <seealso cref="AutotileBrush"/>
    public sealed class EmptyBrush : Brush
    {
        /// <inheritdoc/>
        public override string DesignableType {
            get { return "Empty Brush"; }
        }


        /// <summary>
        /// Indicates whether box colliders should be added to painted tiles.
        /// </summary>
        /// <remarks>
        /// <para>Adding colliders to individual tile objects will make the process
        /// of painting tiles considerably slower. This feature should be used
        /// carefully.</para>
        /// <para>Custom tile based collision detection logic is often more efficient
        /// and can be implemented using tile flags (and of course the "Solid" flag
        /// where applicable).</para>
        /// </remarks>
        /// <seealso cref="colliderType"/>
        public bool addCollider;
        /// <summary>
        /// The type of collider associated with brush.
        /// </summary>
        /// <seealso cref="addCollider"/>
        public ColliderType colliderType = ColliderType.BoxCollider3D;

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


        /// <inheritdoc/>
        public override bool CanOverrideTagAndLayer {
            get { return false; }
        }

        /// <inheritdoc/>
        public override bool PerformsAutomaticOrientation {
            get { return false; }
        }

        /// <inheritdoc/>
        public override void Awake()
        {
            base.Awake();

            this.scaleMode = ScaleMode.UseCellSize;
        }


        /// <inheritdoc/>
        protected internal override bool CalculateManualOffset(IBrushContext context, TileData tile, Transform transform, out Vector3 offsetPosition, out Quaternion offsetRotation, out Vector3 offsetScale, Brush transformer)
        {
            return IdentityManualOffset(out offsetPosition, out offsetRotation, out offsetScale);
        }

        /// <inheritdoc/>
        protected internal override void CreateTile(IBrushContext context, TileData tile)
        {
            // Attach object to tile?
            if (this.addCollider || this.alwaysAddContainer) {
                tile.gameObject = new GameObject("tile");

                if (this.addCollider) {
                    switch (this.colliderType) {
                        case Tile.ColliderType.BoxCollider2D:
                            tile.gameObject.AddComponent<BoxCollider2D>();
                            break;

                        case Tile.ColliderType.BoxCollider3D:
                            tile.gameObject.AddComponent<BoxCollider>();
                            break;
                    }
                }
            }
        }
    }
}
