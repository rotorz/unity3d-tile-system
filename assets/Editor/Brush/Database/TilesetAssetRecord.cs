// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Provides data about a tileset asset.
    /// </summary>
    /// <seealso cref="BrushDatabase"/>
    /// <seealso cref="BrushDatabase.FindTilesetRecord"/>
    [Serializable]
    public sealed class TilesetAssetRecord
    {
        [SerializeField]
        private string assetPath;
        private string displayName;

        [SerializeField]
        private Tileset tileset;
        [SerializeField]
        internal bool isMaster;

        [SerializeField]
        private BrushAssetRecord[] brushRecords;
        private ReadOnlyCollection<BrushAssetRecord> brushRecordsReadOnly;


        internal void SetBrushRecords(BrushAssetRecord[] records)
        {
            this.brushRecords = records;
        }


        /// <summary>
        /// Gets file path of tileset asset.
        /// </summary>
        public string AssetPath {
            get { return this.assetPath; }
            internal set {
                if (value != this.assetPath) {
                    this.assetPath = value;
                    this.displayName = this.tileset.name;

                    if (this.tileset is AutotileTileset) {
                        this.displayName += " (Autotile)";
                    }
                }
            }
        }

        /// <summary>
        /// Gets display name of tileset.
        /// </summary>
        /// <remarks>
        /// <para>Useful when presenting name of tileset in user interfaces.</para>
        /// </remarks>
        public string DisplayName {
            get { return this.displayName; }
        }

        /// <summary>
        /// Gets the tileset.
        /// </summary>
        public Tileset Tileset {
            get { return this.tileset; }
        }
        /// <summary>
        /// Gets a value that indicates whether tileset contains master brushes.
        /// </summary>
        public bool IsMaster {
            get { return this.isMaster; }
        }

        /// <summary>
        /// Gets list of brush records contained within tileset.
        /// </summary>
        public ReadOnlyCollection<BrushAssetRecord> BrushRecords {
            get {
                if (this.brushRecordsReadOnly == null) {
                    this.brushRecordsReadOnly = new ReadOnlyCollection<BrushAssetRecord>(this.brushRecords);
                }
                return this.brushRecordsReadOnly;
            }
        }


        #region Construction

        internal TilesetAssetRecord(string assetPath, Tileset tileset, bool master)
        {
            this.tileset = tileset;
            this.isMaster = master;

            this.AssetPath = assetPath;
        }

        #endregion


        /// <summary>
        /// Find first brush that matches specified name.
        /// </summary>
        /// <param name="name">Brush name.</param>
        /// <returns>
        /// The <see cref="Brush"/> when found; otherwise <c>null</c>.
        /// </returns>
        public BrushAssetRecord FindBrushByName(string name)
        {
            if (this.BrushRecords != null) {
                foreach (var record in this.brushRecords) {
                    if (record != null && record.Brush.name == name) {
                        return record;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether brush name is unique for given brush.
        /// </summary>
        /// <remarks>
        /// <para>Specified brush name is unique when no other brush has
        /// that name (except for the specified target brush).</para>
        /// </remarks>
        /// <param name="name">New name for brush.</param>
        /// <param name="target">The brush that is to be named. Can specify <c>null</c>
        /// when considering unique name for no particular brush.</param>
        /// <returns>
        /// A value of <c>true</c> when specified brush name was unique; otherwise <c>false</c>.
        /// </returns>
        internal bool IsNameUnique(string name, Brush target)
        {
            if (this.BrushRecords != null) {
                foreach (var record in this.brushRecords) {
                    if (record != null && record.Brush != target && record.Brush.name == name) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
