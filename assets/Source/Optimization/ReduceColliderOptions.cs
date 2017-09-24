// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Options which are considered when reducing colliders.
    /// </summary>
    [Serializable]
    public sealed class ReduceColliderOptions
    {
        [SerializeField, FormerlySerializedAs("_active")]
        private bool isActive = true;

        /// <summary>
        /// Indicates whether colliders should be reduced when building tile system.
        /// </summary>
        /// <seealso cref="SnapThreshold"/>
        public bool Active {
            get { return this.isActive; }
            set { this.isActive = value; }
        }


        [SerializeField, FormerlySerializedAs("_snapThreshold")]
        private float snapThreshold = 0.01f;
        [SerializeField, FormerlySerializedAs("_keepSeparate")]
        private KeepSeparateColliderFlag keepSeparate = KeepSeparateColliderFlag.ByTag | KeepSeparateColliderFlag.ByLayer;
        [SerializeField, FormerlySerializedAs("_includeSolidTiles")]
        private bool includeSolidTiles = false;
        [SerializeField, FormerlySerializedAs("_solidTileColliderType")]
        private ColliderType solidTileColliderType = ColliderType.BoxCollider3D;


        /// <summary>
        /// Collider bounds are automatically snapped within given threshold upon
        /// reduction. Default is usually recommended but can be disabled with zero.
        /// </summary>
        /// <seealso cref="Active"/>
        public float SnapThreshold {
            get { return this.snapThreshold; }
            set { this.snapThreshold = value; }
        }

        /// <summary>
        /// Zero or more flags indicating which colliders should be kept separate.
        /// </summary>
        public KeepSeparateColliderFlag KeepSeparate {
            get { return this.keepSeparate; }
            set { this.keepSeparate = value; }
        }

        /// <summary>
        /// Indicates if colliders should be added to tiles which are flagged as solid.
        /// This can help to further reduce the number of colliders.
        /// </summary>
        public bool IncludeSolidTiles {
            get { return this.includeSolidTiles; }
            set { this.includeSolidTiles = value; }
        }

        /// <summary>
        /// Indicates the type of collider to assume for tiles which are flagged as solid.
        /// </summary>
        public ColliderType SolidTileColliderType {
            get { return this.solidTileColliderType; }
            set { this.solidTileColliderType = value; }
        }


        /// <summary>
        /// Restore default values.
        /// </summary>
        public void SetDefaults()
        {
            this.isActive = true;

            this.snapThreshold = 0.01f;
            this.keepSeparate = KeepSeparateColliderFlag.ByTag | KeepSeparateColliderFlag.ByLayer;
            this.includeSolidTiles = false;
            this.solidTileColliderType = ColliderType.BoxCollider3D;
        }

        /// <summary>
        /// Set option values from another options instance.
        /// </summary>
        /// <param name="options">Options.</param>
        public void SetFrom(ReduceColliderOptions options)
        {
            this.isActive = options.isActive;

            this.snapThreshold = options.snapThreshold;
            this.keepSeparate = options.keepSeparate;
            this.includeSolidTiles = options.includeSolidTiles;
            this.solidTileColliderType = options.solidTileColliderType;
        }
    }
}
