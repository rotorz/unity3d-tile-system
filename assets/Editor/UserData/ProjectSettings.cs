// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.EditorExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Project wide settings.
    /// </summary>
    public sealed class ProjectSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Singleton

        private static string ProjectSettingsAssetPath {
            get { return PackageUtility.GetDataAssetPath("@rotorz/unity3d-tile-system", null, "ProjectSettings.asset"); }
        }

        private static ProjectSettings s_Instance;


        /// <summary>
        /// Gets the <see cref="ProjectSettings"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="ProjectSettings"/> asset is automatically created if it
        /// does not already exist.</para>
        /// <para>This asset is located inside at the following path in your project
        /// "Assets/Rotorz/User Data/Tile System/Project Settings.asset".</para>
        /// </remarks>
        public static ProjectSettings Instance {
            get {
                if (s_Instance == null) {
                    SetupInstance();
                }
                return s_Instance;
            }
        }


        private static void SetupInstance()
        {
            s_Instance = LoadAsset();
            if (s_Instance == null) {
                s_Instance = CreateAsset();
            }
        }

        private static ProjectSettings LoadAsset()
        {
            return AssetDatabase.LoadAssetAtPath(ProjectSettingsAssetPath, typeof(ProjectSettings)) as ProjectSettings;
        }

        private static ProjectSettings CreateAsset()
        {
            // Do not proceed if the project settings asset file already exists!
            string absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), ProjectSettingsAssetPath);
            if (File.Exists(absoluteFilePath)) {
                // It looks like the project settings asset may already exist.
                Debug.LogError(string.Format("Cannot create project settings asset because file already exists: '{0}'\nTry to reimport <color=red>TileSystem.Editor.dll</color> by right-clicking it in the <color=red>Project</color> window and then selecting <color=red>Reimport</color>.", ProjectSettingsAssetPath));
                return null;
            }

            var instance = CreateInstance<ProjectSettings>();
            AssetDatabase.CreateAsset(instance, ProjectSettingsAssetPath);
            AssetDatabase.Refresh();
            return instance;
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectSettings"/> class.
        /// </summary>
        public ProjectSettings()
        {
            this.InitializeFlagLabels();
        }


        #region ISerializationCallbackReceiver Members

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.categoryMap.Clear();
            foreach (var info in this.categories) {
                // Automatically fix newly added category identifiers.
                if (info.Id == 0 || this.categoryMap.ContainsKey(info.Id)) {
                    info.Id = this.NextUniqueId();
                }

                // Sanitize category labels.
                info.Label = CategoryLabelUtility.SanitizeCategoryLabel(info.Label);

                this.categoryMap[info.Id] = info;
            }

            // Category 0 is not allowed!
            if (this.categoryMap.ContainsKey(0)) {
                this.DeleteCategory(0);
            }

            ++this.CategoryRevisionCounter;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        #endregion


        #region Brush Creator

        [SerializeField, FormerlySerializedAs("_brushesFolderRelativePath")]
        private string brushesFolderRelativePath;


        /// <summary>
        /// Gets or sets path of the folder where new brushes and tilesets are created
        /// relative to the 'Assets' folder. This must be a sub-folder somewhere inside
        /// 'Assets'.
        /// </summary>
        /// <example>
        /// <para>Reset to the default path:</para>
        /// <code language="csharp"><![CDATA[
        /// var projectSettings = ProjectSettings.Instance;
        /// projectSettings.BrushesFolderAssetPath = "Plugins/PackageData/@rotorz/unity3d-tile-system/Brushes";
        /// EditorUtility.SetDirty(projectSettings);
        /// ]]></code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="value"/> is an invalid path.
        /// </exception>
        /// <seealso cref="BrushUtility.GetBrushAssetPath()"/>
        public string BrushesFolderRelativePath {
            get {
                if (!string.IsNullOrEmpty(this.brushesFolderRelativePath)) {
                    try {
                        return SanitizeBrushesFolderRelativePath(this.brushesFolderRelativePath);
                    }
                    catch (Exception ex) {
                        Debug.LogError("Path to brushes folder was invalid (see below); reverting to default.");
                        Debug.LogException(ex);
                    }
                }

                // Assume the default path for creating brushes and tilesets.
                this.brushesFolderRelativePath = GetDefaultBrushesFolderRelativePath();
                EditorUtility.SetDirty(this);

                return this.brushesFolderRelativePath;
            }
            set {
                this.brushesFolderRelativePath = SanitizeBrushesFolderRelativePath(value);
            }
        }

        private static string GetDefaultBrushesFolderRelativePath()
        {
            return AssetPathUtility.ConvertToAssetsRelativePath(PackageUtility.ResolveDataAssetPath("@rotorz/unity3d-tile-system", "Brushes"));
        }

        private static string SanitizeBrushesFolderRelativePath(string value)
        {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            if (value.Contains("/../") || value.StartsWith("../")) {
                throw new ArgumentException("Path contains invalid characters.", "value");
            }

            // Remove trailing slash for consistency.
            return value.TrimEnd('/');
        }

        #endregion


        #region Tileset Creator

        [SerializeField, FormerlySerializedAs("_expandTilesetCreatorSection")]
        private bool expandTilesetCreatorSection;

        [SerializeField, FormerlySerializedAs("_opaqueTilesetMaterialTemplate")]
        private Material opaqueTilesetMaterialTemplate;
        [SerializeField, FormerlySerializedAs("_transparentTilesetMaterialTemplate")]
        private Material transparentTilesetMaterialTemplate;


        /// <summary>
        /// Gets or sets a value indicating whether the "Tileset Creator" section of the
        /// project settings inspector is expanded.
        /// </summary>
        public bool ExpandTilesetCreatorSection {
            get { return this.expandTilesetCreatorSection; }
            set { this.expandTilesetCreatorSection = value; }
        }

        /// <summary>
        /// Gets or sets the material that will be used as a template when new opaque
        /// tileset materials are created. A value of <c>null</c> specifies that the
        /// default is to be assumed.
        /// </summary>
        public Material OpaqueTilesetMaterialTemplate {
            get { return this.opaqueTilesetMaterialTemplate; }
            set {
                if (this.opaqueTilesetMaterialTemplate == value) {
                    return;
                }
                this.opaqueTilesetMaterialTemplate = value;
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Gets or sets the material that will be used as a template when new transparent
        /// tileset materials are created. A value of <c>null</c> specifies that the
        /// default is to be assumed.
        /// </summary>
        public Material TransparentTilesetMaterialTemplate {
            get { return this.transparentTilesetMaterialTemplate; }
            set {
                if (this.transparentTilesetMaterialTemplate == value) {
                    return;
                }
                this.transparentTilesetMaterialTemplate = value;
                EditorUtility.SetDirty(this);
            }
        }

        #endregion


        #region Brush Categories

        /// <summary>
        /// The value of this counter increments when category changes are detected.
        /// </summary>
        public int CategoryRevisionCounter { get; private set; }


        [SerializeField, FormerlySerializedAs("_expandBrushCategoriesSection")]
        private bool expandBrushCategoriesSection;
        [SerializeField, FormerlySerializedAs("_showCategoryIds")]
        private bool showCategoryIds;
        [SerializeField, FormerlySerializedAs("_categories")]
        private List<BrushCategoryInfo> categories = new List<BrushCategoryInfo>();
        [SerializeField, FormerlySerializedAs("_nextCategoryId")]
        private int nextCategoryId = 1;

        [NonSerialized]
        private Dictionary<int, BrushCategoryInfo> categoryMap = new Dictionary<int, BrushCategoryInfo>();


        /// <summary>
        /// Gets or sets a value indicating whether the "Brush Categories" section of the
        /// project settings inspector is expanded.
        /// </summary>
        public bool ExpandBrushCategoriesSection {
            get { return this.expandBrushCategoriesSection; }
            set { this.expandBrushCategoriesSection = value; }
        }

        /// <summary>
        /// Gets or sets whether category identifiers are shown in the inspector.
        /// </summary>
        internal bool ShowCategoryIds {
            get { return this.showCategoryIds; }
            set { this.showCategoryIds = value; }
        }

        internal BrushCategoryInfo[] Categories {
            get { return this.categories.ToArray(); }
        }

        /// <summary>
        /// Gets the collection of brush category ids.
        /// </summary>
        public int[] CategoryIds {
            get { return this.categories.Select(info => info.Id).ToArray(); }
        }

        /// <summary>
        /// Gets the collection of brush category labels.
        /// </summary>
        public string[] CategoryLabels {
            get { return this.categories.Select(info => info.Label).ToArray(); }
        }


        private int NextUniqueId()
        {
            while (++this.nextCategoryId < int.MaxValue) {
                if (!this.categoryMap.ContainsKey(this.nextCategoryId)) {
                    return this.nextCategoryId;
                }
            }

            // Ran out of unique id's REALLY?!
            // Okay... start from zero again.
            this.nextCategoryId = 0;
            while (++this.nextCategoryId < int.MaxValue) {
                if (!this.categoryMap.ContainsKey(this.nextCategoryId)) {
                    return this.nextCategoryId;
                }
            }

            // User is crazy having so many categories!
            throw new InvalidOperationException("Cannot allocate new category id.");
        }

        /// <summary>
        /// Adds a new brush category.
        /// </summary>
        /// <param name="label">Label for category.</param>
        /// <returns>
        /// The unique category identifier.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="label"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="label"/> is an empty string.
        /// </exception>
        /// <seealso cref="SetCategoryLabel(int, string)"/>
        public int AddCategory(string label)
        {
            if (label == null) {
                throw new ArgumentNullException("label");
            }
            if (label == "") {
                throw new ArgumentException("Was empty.", "label");
            }

            Undo.RecordObject(this, TileLang.ParticularText("Action", "Add Category"));

            var info = new BrushCategoryInfo(this.NextUniqueId(), label);

            this.categories.Add(info);
            EditorUtility.SetDirty(this);

            this.categoryMap[info.Id] = info;
            ++this.CategoryRevisionCounter;

            return info.Id;
        }

        /// <summary>
        /// Sets label of a specific category.
        /// </summary>
        /// <param name="id">Identifies the category.</param>
        /// <param name="label">The new category label.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="id"/> is zero or a negative value.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="label"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="label"/> is an empty string.
        /// </exception>
        /// <seealso cref="AddCategory(string)"/>
        public void SetCategoryLabel(int id, string label)
        {
            if (id <= 0) {
                throw new ArgumentOutOfRangeException("id", id, (string)null);
            }
            if (label == null) {
                throw new ArgumentNullException("label");
            }
            if (label == "") {
                throw new ArgumentException("Was empty.", "label");
            }

            Undo.RecordObject(this, TileLang.ParticularText("Action", "Set Category Label"));

            BrushCategoryInfo info;
            if (!this.categoryMap.TryGetValue(id, out info)) {
                info = new BrushCategoryInfo(id);
                this.categories.Add(info);
                this.categoryMap[id] = info;
            }

            info.Label = label;
            EditorUtility.SetDirty(this);

            ++this.CategoryRevisionCounter;
        }

        /// <summary>
        /// Deletes an unwanted brush category.
        /// </summary>
        /// <param name="id">Identifies the category.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If <paramref name="id"/> is zero or a negative value.
        /// </exception>
        public void DeleteCategory(int id)
        {
            if (id <= 0) {
                throw new ArgumentOutOfRangeException("id", id, (string)null);
            }

            int index = this.categories.FindIndex(info => info.Id == id);
            if (index != -1) {
                Undo.RecordObject(this, TileLang.ParticularText("Action", "Delete Category"));
                this.categories.RemoveAt(index);
                EditorUtility.SetDirty(this);

                this.categoryMap.Remove(id);
                ++this.CategoryRevisionCounter;
            }
        }

        /// <summary>
        /// Gets label of a specific category.
        /// </summary>
        /// <param name="id">Identifies the category.</param>
        /// <returns>
        /// The category label.
        /// </returns>
        public string GetCategoryLabel(int id)
        {
            if (id == 0) {
                return TileLang.ParticularText("Status", "Uncategorized");
            }

            BrushCategoryInfo info;
            if (this.categoryMap.TryGetValue(id, out info)) {
                return info.Label ?? "";
            }
            else {
                return TileLang.ParticularText("Status", "(Unknown Category)");
            }
        }

        /// <summary>
        /// Sorts brush categories by label either ascending or descending.
        /// </summary>
        /// <param name="ascending">A value of <c>true</c> indicates that categories
        /// should be sorted in ascending order; otherwise, categories will be sorted
        /// in descending order.</param>
        public void SortCategoriesByLabel(bool ascending)
        {
            Undo.RecordObject(this, TileLang.ParticularText("Action", "Sort Categories"));

            var comparer = Comparer<string>.Default;
            if (ascending) {
                this.categories.Sort((a, b) => comparer.Compare(a.Label, b.Label));
            }
            else {
                this.categories.Sort((a, b) => comparer.Compare(b.Label, a.Label));
            }

            EditorUtility.SetDirty(this);
        }

        #endregion


        #region Flag Labels

        [SerializeField, FormerlySerializedAs("_flagLabels")]
        private string[] flagLabels;


        private void InitializeFlagLabels()
        {
            // Initialize flag labels array with empty strings.
            this.flagLabels = new string[16];
            for (int i = 0; i < 16; ++i) {
                this.flagLabels[i] = "";
            }
        }


        /// <summary>
        /// Gets or sets the collection of labels for the 16 general purpose brush flags.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="value"/> does not contain 16 strings.
        /// </exception>
        public string[] FlagLabels {
            get { return this.flagLabels.ToArray(); }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                if (value.Length != 16) {
                    throw new ArgumentException("Must contain 16 flag labels.", "value");
                }

                Undo.RecordObject(this, TileLang.ParticularText("Action", "Update Flag Labels"));

                for (int i = 0; i < value.Length; ++i) {
                    string newLabel = value[i];
                    this.flagLabels[i] = newLabel != null ? newLabel.Trim() : "";
                }

                EditorUtility.SetDirty(this);
            }
        }

        #endregion


        /// <summary>
        /// Collapses all expandable sections in project settings inspector.
        /// </summary>
        public void CollapseAllSections()
        {
            this.ExpandTilesetCreatorSection = false;
            this.ExpandBrushCategoriesSection = false;
        }
    }
}
