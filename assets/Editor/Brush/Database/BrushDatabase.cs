// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Brush database provides easy access to brush and tileset records in editor scripts.
    /// </summary>
    public sealed class BrushDatabase : ScriptableObject
    {
        #region Singleton

        private static BrushDatabase s_Instance;

        /// <summary>
        /// Gets the one and only brush database instance.
        /// </summary>
        public static BrushDatabase Instance {
            get {
                if (s_Instance == null) {
                    Console.WriteLine("Loading brushes...");

                    s_Instance = UnityEngine.Resources.FindObjectsOfTypeAll<BrushDatabase>().FirstOrDefault();
                    if (s_Instance == null) {
                        s_Instance = ScriptableObject.CreateInstance<BrushDatabase>();
                    }
                    else {
                        Console.WriteLine("Brush database already loaded.");
                    }
                }

                // Ensure that instance has been enabled.
                if (!s_Instance.hasEnabled) {
                    s_Instance.OnEnable();
                }

                return s_Instance;
            }
        }

        // Prevent instantiation!
        private BrushDatabase()
        {
        }

        #endregion


        /// <summary>
        /// The time (in ticks) when brush database was last scanned.
        /// </summary>
        internal static long s_TimeLastUpdated;

        /// <summary>
        /// Collection of brush records that are sorted by name.
        /// </summary>
        [NonSerialized]
        private BrushAssetRecord[] brushRecords = { };
        [NonSerialized]
        private ReadOnlyCollection<BrushAssetRecord> brushRecordsReadOnly;


        /// <summary>
        /// Gets a read-only list of brush records that are sorted by display name.
        /// </summary>
        /// <value>
        /// List of brush records.
        /// </value>
        public ReadOnlyCollection<BrushAssetRecord> BrushRecords {
            get {
                if (this.brushRecordsReadOnly == null) {
                    this.brushRecordsReadOnly = new ReadOnlyCollection<BrushAssetRecord>(this.brushRecords);
                }
                return this.brushRecordsReadOnly;
            }
        }


        /// <summary>
        /// Collection of tileset records.
        /// </summary>
        [NonSerialized]
        private TilesetAssetRecord[] tilesetRecords = { };
        [NonSerialized]
        private ReadOnlyCollection<TilesetAssetRecord> tilesetRecordsReadOnly;


        /// <summary>
        /// Gets a read-only list of tileset records that are sorted by display name.
        /// </summary>
        /// <value>
        /// List of tileset records.
        /// </value>
        public ReadOnlyCollection<TilesetAssetRecord> TilesetRecords {
            get {
                if (this.tilesetRecordsReadOnly == null) {
                    this.tilesetRecordsReadOnly = new ReadOnlyCollection<TilesetAssetRecord>(this.tilesetRecords);
                }
                return this.tilesetRecordsReadOnly;
            }
        }


        #region Setup

        private const int HasInitializedCode = 2;

        [SerializeField]
        private int hasInitialized;
        [NonSerialized]
        private bool hasEnabled;


        private void OnEnable()
        {
            if (this.hasEnabled) {
                return;
            }
            this.hasEnabled = true;

            s_Instance = this;
            this.hideFlags = HideFlags.DontSave;

            if (this.hasInitialized < HasInitializedCode) {
                this.OnInitialize();

                this.hasInitialized = HasInitializedCode;
                Console.WriteLine("Initialized brush database.");
            }
            else {
                this.Rescan();
            }

            BrushDatabaseRescanProcessor.s_EnableScanner = true;
        }

        private void OnInitialize()
        {
            AssetDatabase.SaveAssets();
            this.Rescan();
        }

        #endregion


        #region Methods

        /// <summary>
        /// Rescan brush assets.
        /// </summary>
        public void Rescan()
        {
            Console.WriteLine("Scanning for brushes and tilesets...");

            var newRecords = new List<BrushAssetRecord>();
            var tilesetRecords = new List<TilesetAssetRecord>();

            // Read records for brushes.
            foreach (var guid in AssetDatabase.FindAssets("t:Rotorz.Tile.Brush")) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                this.ScanBrush(assetPath, newRecords);
            }

            // Read records for tilesets.
            foreach (var guid in AssetDatabase.FindAssets("t:Rotorz.Tile.Tileset")) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var tilesetRecord = this.ScanAtlas(assetPath, newRecords);
                if (tilesetRecord != null) {
                    tilesetRecords.Add(tilesetRecord);
                }
            }

            // Sort prefabs by name.
            this.brushRecords = newRecords.ToArray();
            this.brushRecordsReadOnly = null;
            this.SortBrushRecords();

            // Sort tilesets by name.
            this.tilesetRecords = tilesetRecords.ToArray();
            this.tilesetRecordsReadOnly = null;
            this.SortTilesetRecords();

            // Note: Old brush records are retained until brushes have been rescanned.
            s_TimeLastUpdated = System.DateTime.Now.Ticks;
        }

        private void SortBrushRecords()
        {
            ++s_TimeLastUpdated;
            Array.Sort(this.brushRecords, (x, y) => string.Compare(x.DisplayName, y.DisplayName));
        }

        private void SortTilesetRecords()
        {
            ++s_TimeLastUpdated;
            Array.Sort(this.tilesetRecords, (x, y) => string.Compare(x.DisplayName, y.DisplayName));
        }

        /// <summary>
        /// Clear records for any missing brush and tileset assets.
        /// </summary>
        internal void ClearMissingRecords()
        {
            Console.WriteLine("Clearing missing brush/tileset records...");
            this.ClearMissingBrushRecords();
            this.ClearMissingTilesetRecords();
            ++s_TimeLastUpdated;
        }

        private void ClearMissingBrushRecords()
        {
            List<BrushAssetRecord> brushRecords = null;

            for (int i = 0; i < this.brushRecords.Length; ++i) {
                var brushRecord = this.brushRecords[i];

                if (brushRecord.Brush == null) {
                    if (brushRecords == null) {
                        // Copy all prior non-missing brush records to new list.
                        brushRecords = new List<BrushAssetRecord>();
                        for (int j = 0; j < i; ++j) {
                            brushRecords.Add(this.brushRecords[j]);
                        }
                    }
                }
                else if (brushRecords != null) {
                    brushRecords.Add(brushRecord);
                }
            }

            if (brushRecords != null) {
                this.brushRecords = brushRecords.ToArray();
                this.brushRecordsReadOnly = new ReadOnlyCollection<BrushAssetRecord>(this.brushRecords);
            }
        }

        private void ClearMissingTilesetRecords()
        {
            var tilesetRecords = new List<TilesetAssetRecord>();
            var brushRecords = new List<BrushAssetRecord>();

            for (int i = 0; i < this.tilesetRecords.Length; ++i) {
                var tilesetRecord = this.tilesetRecords[i];
                if (tilesetRecord.Tileset == null) {
                    continue;
                }

                tilesetRecords.Add(tilesetRecord);

                // Clear missing brush records from tileset.
                brushRecords.Clear();
                foreach (var brushRecord in tilesetRecord.BrushRecords) {
                    if (brushRecord.Brush != null) {
                        brushRecords.Add(brushRecord);
                    }
                }
                if (brushRecords.Count != tilesetRecord.BrushRecords.Count) {
                    tilesetRecord.SetBrushRecords(brushRecords.ToArray());
                }
            }

            if (tilesetRecords.Count != this.tilesetRecords.Length) {
                this.tilesetRecords = tilesetRecords.ToArray();
                this.tilesetRecordsReadOnly = new ReadOnlyCollection<TilesetAssetRecord>(this.tilesetRecords);
            }
        }

        /// <summary>
        /// Find index of brush record.
        /// </summary>
        /// <remarks>
        /// <para>This function is slightly slower than <see cref="FindRecord"/> since
        /// it verifies whether record entries have been replaced with a value of <c>null</c>.
        /// This is important when rescanning a project for brushes since records are
        /// recycled where possible.</para>
        /// </remarks>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// Zero-based index of brush record; otherwise a value of -1 if not found.
        /// </returns>
        private int FindRecordIndexWithNullChecks(Brush brush)
        {
            if (brush != null) {
                for (int i = 0; i < this.brushRecords.Length; ++i) {
                    if (this.brushRecords[i] != null && this.brushRecords[i].Brush == brush) {
                        return i;
                    }
                }
            }
            return -1;
        }

        private BrushAssetRecord AddBrushRecord(Object mainAsset, Brush brush, string assetPath, bool master)
        {
            BrushAssetRecord record;

            int recordIndex = this.FindRecordIndexWithNullChecks(brush);
            if (recordIndex != -1) {
                record = this.brushRecords[recordIndex];

                // Master and asset path may have changed.
                record.AssetPath = assetPath;
                record.isMaster = master;

                // Remove record from old list since we want to recycle it!
                this.brushRecords[recordIndex] = null;
            }
            else {
                // Create new record.
                record = new BrushAssetRecord(assetPath, mainAsset, brush, master);
            }

            // Make sure that brush is awake!
            if (!brush._ready) {
                brush._ready = true;
                brush.Awake();
            }

            return record;
        }

        private void ScanBrush(string assetPath, IList<BrushAssetRecord> newRecords)
        {
            var brush = AssetDatabase.LoadMainAssetAtPath(assetPath) as Brush;
            if (brush == null) {
                return;
            }

            // Is this a master brush?
            bool master = assetPath.Contains("/Master/");

            var brushRecord = this.AddBrushRecord(brush, brush, assetPath, master);
            newRecords.Add(brushRecord);
        }

        private TilesetAssetRecord ScanAtlas(string assetPath, IList<BrushAssetRecord> newRecords)
        {
            Tileset tileset = null;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var asset in assets) {
                tileset = asset as Tileset;
                if (tileset != null) {
                    break;
                }
            }

            if (tileset == null) {
                return null;
            }

            // Is this a master brush?
            bool master = assetPath.Contains("/Master/");

            var tilesetRecord = new TilesetAssetRecord(assetPath, tileset, master);
            var brushRecords = new List<BrushAssetRecord>();

            foreach (var asset in assets) {
                var tilesetBrush = asset as TilesetBrush;
                if (tilesetBrush == null) {
                    continue;
                }

                var tilesetBrushRecord = this.AddBrushRecord(tileset, tilesetBrush, assetPath, master);
                newRecords.Add(tilesetBrushRecord);

                brushRecords.Add(tilesetBrushRecord);
            }

            // Sort atlas brush index by atlas index.
            brushRecords.Sort((x, y) => {
                int a = (x.Brush as TilesetBrush).tileIndex;
                int b = (y.Brush as TilesetBrush).tileIndex;

                return (a == b)
                    ? x.DisplayName.CompareTo(y.DisplayName)
                    : a - b;
            });
            tilesetRecord.SetBrushRecords(brushRecords.ToArray());

            return tilesetRecord;
        }

        /// <summary>
        /// Index of record in sorted (by display name) list of brushes.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// Zero-based index of record; otherwise a value of -1 if not found.
        /// </returns>
        public int IndexOfRecord(Brush brush)
        {
            for (int i = 0; i < this.brushRecords.Length; ++i) {
                if (this.brushRecords[i].Brush == brush) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find record for specified brush asset.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// Brush record when found; otherwise a value of <c>null</c>.
        /// </returns>
        public BrushAssetRecord FindRecord(Brush brush)
        {
            if (brush != null) {
                for (int i = 0; i < this.brushRecords.Length; ++i) {
                    if (this.brushRecords[i].Brush == brush) {
                        return this.brushRecords[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Rename brush asset.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="newName">New name for brush.</param>
        /// <returns>
        /// Name that was assigned to brush.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If unable to rename brush.
        /// </exception>
        public string RenameBrush(Brush brush, string newName)
        {
            newName = newName.Trim();
            if (string.IsNullOrEmpty(newName)) {
                return brush.name;
            }

            var brushRecord = this.FindRecord(brush);
            if (brushRecord == null) {
                return newName;
            }

            string newAssetName = newName;

            // If renaming asset file itself.
            if (brushRecord.MainAsset == brushRecord.Brush) {
                // Return current brush name if an error occurred whilst renaming asset.
                string error = AssetDatabase.RenameAsset(brushRecord.AssetPath, newAssetName);
                if (!string.IsNullOrEmpty(error)) {
                    throw new ArgumentException(error.Contains("does already exist")
                        ? "Another asset already exists with specified name."
                        : error
                    );
                }

                brushRecord.AssetPath = AssetDatabase.GetAssetPath(brush);
            }
            else {
                // Does another brush already exist with the same name?
                var tileset = brushRecord.MainAsset as Tileset;
                if (tileset != null) {
                    var tilesetRecord = this.FindTilesetRecord(tileset);
                    if (tilesetRecord != null && !tilesetRecord.IsNameUnique(newName, brush)) {
                        throw new ArgumentException("Tileset already contains a brush with that name.");
                    }
                }
            }

            brush.name = newAssetName;
            EditorUtility.SetDirty(brush);

            this.SortBrushRecords();

            return brush.name;
        }

        /// <summary>
        /// Rename tileset asset.
        /// </summary>
        /// <param name="tileset">The tileset.</param>
        /// <param name="newName">New name for tileset.</param>
        /// <returns>
        /// Name that was assigned to tileset.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If unable to rename tileset.
        /// </exception>
        public string RenameTileset(Tileset tileset, string newName)
        {
            newName = newName.Trim();
            if (string.IsNullOrEmpty(newName)) {
                return tileset.name;
            }

            var tilesetRecord = this.FindTilesetRecord(tileset);
            if (tilesetRecord == null) {
                return newName;
            }

            // Return current tileset name if an error occurred whilst renaming asset.
            string error = AssetDatabase.RenameAsset(tilesetRecord.AssetPath, newName);
            if (!string.IsNullOrEmpty(error)) {
                throw new ArgumentException(error.Contains("does already exist")
                    ? "Another asset already exists with specified name."
                    : error
                );
            }

            tileset.name = newName;
            EditorUtility.SetDirty(tileset);

            // Force update of atlas record display name.
            tilesetRecord.AssetPath = "";
            tilesetRecord.AssetPath = AssetDatabase.GetAssetPath(tileset);

            this.SortTilesetRecords();

            AssetDatabase.SaveAssets();

            return tileset.name;
        }

        /// <summary>
        /// Find record for specified tileset.
        /// </summary>
        /// <param name="tileset">The tileset.</param>
        /// <returns>
        /// The tileset record.
        /// </returns>
        public TilesetAssetRecord FindTilesetRecord(Tileset tileset)
        {
            if (tileset != null) {
                for (int i = 0; i < this.tilesetRecords.Length; ++i) {
                    if (this.tilesetRecords[i].Tileset == tileset) {
                        return this.tilesetRecords[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a read-only list of brushes contained within tileset.
        /// </summary>
        /// <param name="tileset">The tileset.</param>
        /// <returns>
        /// List of tileset brushes.
        /// </returns>
        public IList<BrushAssetRecord> GetTilesetBrushes(Tileset tileset)
        {
            var tilesetRecord = this.FindTilesetRecord(tileset);
            if (tilesetRecord == null) {
                return new BrushAssetRecord[0];
            }
            return tilesetRecord.BrushRecords;
        }

        #endregion
    }
}
