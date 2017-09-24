// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Collections;
using Rotorz.Games.EditorExtensions;
using Rotorz.Games.UnityEditorExtensions;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Inspector for <see cref="ProjectSettings"/> assets.
    /// </summary>
    [CustomEditor(typeof(ProjectSettings))]
    internal sealed class ProjectSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty propertyExpandTilesetCreatorSection;
        private SerializedProperty propertyBrushesFolderRelativePath;
        private SerializedProperty propertyOpaqueTilesetMaterialTemplate;
        private SerializedProperty propertyTransparentTilesetMaterialTemplate;

        private SerializedProperty propertyCategories;
        private SerializedProperty propertyExpandBrushCategoriesSection;
        private SerializedProperty propertyShowCategoryIds;

        private ReorderableListControl categoriesListControl;
        private SerializedPropertyAdaptor categoriesListAdaptor;


        private void OnEnable()
        {
            this.propertyExpandTilesetCreatorSection = this.serializedObject.FindProperty("expandTilesetCreatorSection");
            this.propertyBrushesFolderRelativePath = this.serializedObject.FindProperty("brushesFolderRelativePath");
            this.propertyOpaqueTilesetMaterialTemplate = this.serializedObject.FindProperty("opaqueTilesetMaterialTemplate");
            this.propertyTransparentTilesetMaterialTemplate = this.serializedObject.FindProperty("transparentTilesetMaterialTemplate");

            this.propertyCategories = this.serializedObject.FindProperty("categories");
            this.propertyExpandBrushCategoriesSection = this.serializedObject.FindProperty("expandBrushCategoriesSection");
            this.propertyShowCategoryIds = this.serializedObject.FindProperty("showCategoryIds");

            this.categoriesListControl = new ReorderableListControl();
            this.categoriesListControl.ItemInserted += this.OnNewCategoryInserted;
            this.categoriesListAdaptor = new SerializedPropertyAdaptor(this.propertyCategories);
        }

        private void OnNewCategoryInserted(object sender, ItemInsertedEventArgs args)
        {
            var newElementProperty = this.categoriesListAdaptor[args.ItemIndex];
            var labelProperty = newElementProperty.FindPropertyRelative("label");
            labelProperty.stringValue = "New Category";
        }

        protected override void OnHeaderGUI()
        {
            Rect position = GUILayoutUtility.GetRect(0, 80);
            if (Event.current.type == EventType.Repaint) {
                GUI.skin.box.Draw(position, GUIContent.none, false, false, false, false);
                GUI.DrawTexture(new Rect(7, 7, 64, 64), RotorzEditorStyles.Skin.Badge);

                Rect labelPosition = new Rect(80, 7, position.width - 80, position.height);
                string labelText = string.Format(
                    /* 0: name of product */
                    TileLang.Text("Project Settings - {0}"),
                    ProductInfo.Name
                );
                EditorStyles.largeLabel.Draw(labelPosition, labelText, false, false, false, false);
            }

            GUILayout.Space(5);
        }

        public override bool UseDefaultMargins()
        {
            // Do not assume default margins for inspector.
            return false;
        }

        public override void OnInspectorGUI()
        {
            bool initialWideMode = EditorGUIUtility.wideMode;
            float initialLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.wideMode = true;
            RotorzEditorGUI.UseExtendedLabelWidthForLocalization();

            this.serializedObject.Update();

            this.propertyExpandTilesetCreatorSection.boolValue = RotorzEditorGUI.FoldoutSection(
                foldout: this.propertyExpandTilesetCreatorSection.boolValue,
                label: TileLang.Text("Brush and Tileset Creator"),
                callback: this.DrawCreatorSection,
                paddedStyle: RotorzEditorStyles.Instance.InspectorSectionPadded
            );

            this.propertyExpandBrushCategoriesSection.boolValue = RotorzEditorGUI.FoldoutSection(
                foldout: this.propertyExpandBrushCategoriesSection.boolValue,
                label: TileLang.Text("Brush Categories"),
                callback: this.DrawCategoriesSections,
                paddedStyle: RotorzEditorStyles.Instance.InspectorSectionPadded
            );

            EditorGUIUtility.wideMode = initialWideMode;
            EditorGUIUtility.labelWidth = initialLabelWidth;

            this.serializedObject.ApplyModifiedProperties();
        }


        #region Section: Brush and Tileset Creator

        private void DrawCreatorSection()
        {
            GUILayout.Label(TileLang.Text("Brushes"), EditorStyles.boldLabel);
            {
                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Brushes Folder")
                )) {
                    this.propertyBrushesFolderRelativePath.stringValue = RotorzEditorGUI.RelativeAssetPathTextField(content, this.propertyBrushesFolderRelativePath.stringValue, false);
                }

                GUILayout.Space(3);
                Rect totalButtonPosition = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(true, 20));

                Rect buttonPosition = totalButtonPosition;
                buttonPosition.x += EditorGUIUtility.labelWidth;
                buttonPosition.width = (buttonPosition.width - EditorGUIUtility.labelWidth) / 2f - 2;

                if (GUI.Button(buttonPosition, TileLang.OpensWindow(TileLang.ParticularText("Action", "Browse")))) {
                    this.BrushesFolder_Browse_Clicked();
                }

                buttonPosition.x = buttonPosition.xMax + 4;
                if (GUI.Button(buttonPosition, TileLang.ParticularText("Action", "Reset"))) {
                    this.BrushesFolder_Reset_Clicked();
                }
            }

            GUILayout.Label(TileLang.Text("Tilesets"), EditorStyles.boldLabel);
            {
                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Opaque Material Template")
                )) {
                    EditorGUILayout.PropertyField(this.propertyOpaqueTilesetMaterialTemplate, content);
                }

                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Transparent Material Template")
                )) {
                    EditorGUILayout.PropertyField(this.propertyTransparentTilesetMaterialTemplate, content);
                }

                RotorzEditorGUI.InfoBox(TileLang.Text("Default materials are created when no material templates are specified."));
            }
        }

        private void BrushesFolder_Browse_Clicked()
        {
            while (true) {
                string absoluteAssetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
                string absoluteBrushesFolderPath = Path.Combine(absoluteAssetsPath, this.propertyBrushesFolderRelativePath.stringValue);
                string defaultFolderName = "";

                if (!Directory.Exists(absoluteBrushesFolderPath)) {
                    absoluteBrushesFolderPath = absoluteAssetsPath;
                    defaultFolderName = "Brushes";
                }

                absoluteBrushesFolderPath = EditorUtility.OpenFolderPanel(
                    TileLang.Text("Select folder where new brushes and tilesets should be created:"),
                    absoluteBrushesFolderPath,
                    defaultFolderName
                );
                if (string.IsNullOrEmpty(absoluteBrushesFolderPath)) {
                    return;
                }

                absoluteAssetsPath = absoluteAssetsPath.Replace(Path.DirectorySeparatorChar, '/') + '/';
                if (!absoluteBrushesFolderPath.StartsWith(absoluteAssetsPath)) {
                    EditorUtility.DisplayDialog(
                        TileLang.ParticularText("Error", "One or more inputs were invalid"),
                        TileLang.Text("Brushes folder must be a sub-folder somewhere inside 'Assets'."),
                        TileLang.ParticularText("Action", "OK")
                    );
                    continue;
                }

                absoluteBrushesFolderPath = absoluteBrushesFolderPath.Substring(absoluteAssetsPath.Length);
                this.propertyBrushesFolderRelativePath.stringValue = absoluteBrushesFolderPath;

                return;
            }
        }

        private void BrushesFolder_Reset_Clicked()
        {
            this.propertyBrushesFolderRelativePath.stringValue = AssetPathUtility.ConvertToAssetsRelativePath(PackageUtility.ResolveDataAssetPath("@rotorz/unity3d-tile-system", "Brushes"));
        }

        #endregion


        #region Section: Brush Categories

        private void DrawCategoriesSections()
        {
            this.DrawCategoryListToolbar();
            GUILayout.Space(-6);
            this.categoriesListControl.Draw(this.categoriesListAdaptor);
        }

        private void DrawCategoryListToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            this.propertyShowCategoryIds.boolValue = GUILayout.Toggle(this.propertyShowCategoryIds.boolValue, TileLang.ParticularText("Action", "Show Id"), EditorStyles.toolbarButton);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(RotorzEditorStyles.Skin.SortAsc, EditorStyles.toolbarButton)) {
                ProjectSettings.Instance.SortCategoriesByLabel(true);
            }
            if (GUILayout.Button(RotorzEditorStyles.Skin.SortDesc, EditorStyles.toolbarButton)) {
                ProjectSettings.Instance.SortCategoriesByLabel(false);
            }

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
