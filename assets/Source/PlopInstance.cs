// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Automatically added to objects which are plopped using the plop tool making it easy
    /// to identify whether an object was plopped.
    /// </summary>
    /// <remarks>
    /// <para>By default this component is removed when upon awakening for game builds
    /// (removal does not happen in-editor) though this behaviour can be disabled by
    /// deselecting "Remove on Awake" in the inspector.</para>
    /// </remarks>
    [AddComponentMenu("")]
    public sealed class PlopInstance : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("_owner")]
        [Tooltip("Tile system which was active when this object was plopped.")]
        private TileSystem owner;


        /// <summary>
        /// Gets or sets the tile system that owns the plop instance. This is typically
        /// the tile system that was active when the plop was created.
        /// </summary>
        public TileSystem Owner {
            get { return this.owner; }
            set { this.owner = value; }
        }


        #region Tile Data (needed for cycle functionality)

        [SerializeField, HideInInspector, FormerlySerializedAs("_plopPointOffset")]
        private Vector3 plopPointOffset;
        [SerializeField, HideInInspector, FormerlySerializedAs("_brush")]
        private Brush brush;

        /// <summary>
        /// Holds variation index, rotation and painted rotation.
        /// </summary>
        [SerializeField, HideInInspector, FormerlySerializedAs("_data")]
        private int data;

        private void SetDataBits(int offset, int mask, int value)
        {
            this.data = (this.data & ~(mask << offset)) | ((value & mask) << offset);
        }
        private int GetDataBits(int offset, int mask)
        {
            return (this.data >> offset) & mask;
        }

        /// <summary>
        /// Gets or sets offset from plop point to transform position of plop in local
        /// space of tile system.
        /// </summary>
        public Vector3 PlopPointOffset {
            get { return this.plopPointOffset; }
            set { this.plopPointOffset = value; }
        }

        /// <summary>
        /// Gets point in local space of tile system at which tile was plopped.
        /// </summary>
        public Vector3 PlopPoint {
            get { return transform.localPosition - this.plopPointOffset; }
        }

        /// <summary>
        /// Gets or sets brush which was used to create plop.
        /// </summary>
        public Brush Brush {
            get { return this.brush; }
            set { this.brush = value; }
        }

        /// <summary>
        /// Gets or sets zero-based index of tile variation which was used to create plop.
        /// </summary>
        public int VariationIndex {
            get { return this.GetDataBits(0, 0xFF); }
            set { this.SetDataBits(0, 0xFF, value); }
        }

        /// <summary>
        /// Gets or sets Zero-based index of simple rotation (0-3) which was used to create plop.
        /// </summary>
        public int PaintedRotation {
            get { return this.GetDataBits(8, 0x03); }
            set { this.SetDataBits(8, 0x03, value); }
        }

        /// <summary>
        /// Gets or sets zero-based index of actual rotation (0-3) of plop.
        /// </summary>
        public int Rotation {
            get { return this.GetDataBits(10, 0x03); }
            set { this.SetDataBits(10, 0x03, value); }
        }

        /// <summary>
        /// Get <see cref="TileData"/> representation of plop instance.
        /// </summary>
        /// <returns>
        /// New <see cref="TileData"/> instance.
        /// </returns>
        public TileData ToTileData()
        {
            var tile = new TileData();

            tile.brush = this.Brush;
            tile.variationIndex = (byte)this.VariationIndex;
            tile.PaintedRotation = this.PaintedRotation;
            tile.Rotation = this.Rotation;

            return tile;
        }

        #endregion
    }
}
