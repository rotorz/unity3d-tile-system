// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Provides data about a brush asset.
    /// </summary>
    /// <seealso cref="BrushDatabase"/>
    /// <seealso cref="BrushDatabase.FindRecord"/>
    [Serializable]
    public sealed class BrushAssetRecord
    {
        [SerializeField]
        private string assetPath;

        [SerializeField]
        private Object mainAsset;
        [SerializeField]
        private Brush brush;
        [SerializeField]
        internal bool isMaster;


        /// <summary>
        /// Gets file path of brush asset.
        /// </summary>
        /// <remarks>
        /// <para>Brush is not necessarily the main asset of the referenced file. For
        /// example, <see cref="TilesetBrush">TilesetBrush</see> assets are usually
        /// contained within an <see cref="Tileset"/>.</para>
        /// </remarks>
        public string AssetPath {
            get { return this.assetPath; }
            internal set { this.assetPath = value; }
        }

        /// <summary>
        /// Gets the main asset object.
        /// </summary>
        /// <remarks>
        /// <para>Main asset may refer to brush or another container object.</para>
        /// </remarks>
        public Object MainAsset {
            get { return this.mainAsset; }
        }

        /// <summary>
        /// Gets display name of brush.
        /// </summary>
        /// <remarks>
        /// <para>Useful when presenting name of brush in user interfaces.</para>
        /// </remarks>
        public string DisplayName {
            get { return this.brush != null ? this.brush.name : "Untitled"; }
        }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        public Brush Brush {
            get { return this.brush; }
        }

        /// <summary>
        /// Gets a value that indicates if brush is a master brush.
        /// </summary>
        public bool IsMaster {
            get { return this.isMaster; }
        }

        /// <summary>
        /// Gets a value indicating whether brush is shown.
        /// </summary>
        public bool IsShown {
            get { return this.brush != null ? this.brush.visibility != BrushVisibility.Hidden : false; }
        }

        /// <summary>
        /// Gets a value indicating whether brush is hidden.
        /// </summary>
        public bool IsHidden {
            get { return this.brush != null ? this.brush.visibility == BrushVisibility.Hidden : true; }
        }

        /// <summary>
        /// Gets a value indicating whether brush has been favorited.
        /// </summary>
        public bool IsFavorite {
            get { return this.brush != null ? this.brush.visibility == BrushVisibility.Favorite : false; }
        }


        #region Construction

        internal BrushAssetRecord(string assetPath, Object mainAsset, Brush brush, bool master)
        {
            this.mainAsset = mainAsset;
            this.brush = brush;
            this.isMaster = master;

            this.AssetPath = assetPath;
        }

        #endregion
    }
}
