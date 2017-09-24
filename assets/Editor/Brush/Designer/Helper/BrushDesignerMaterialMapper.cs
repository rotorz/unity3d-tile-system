// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Helper class for rendering material mapper GUI.
    /// </summary>
    internal sealed class BrushDesignerMaterialMapper
    {
        private static Material s_DefaultDiffuseMaterial;


        static BrushDesignerMaterialMapper()
        {
            // Little hack to find a reference to the "Default" (Unity 5) material.
            var tempGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var renderer = tempGO.GetComponent<MeshRenderer>();
            s_DefaultDiffuseMaterial = renderer.sharedMaterial;
            Object.DestroyImmediate(tempGO);
        }


        private readonly GUIStyle materialPreviewStyle;

        private readonly Brush brush;
        private readonly IMaterialMappings mappings;


        public BrushDesignerMaterialMapper(BrushDesignerView designer)
        {
            this.brush = designer.Brush;
            this.mappings = designer.Brush as IMaterialMappings;

            if (this.mappings.MaterialMappingFrom == null) {
                this.mappings.MaterialMappingFrom = new Material[0];
            }
            if (this.mappings.MaterialMappingTo == null) {
                this.mappings.MaterialMappingTo = new Material[0];
            }

            // Prepare material preview style.
            this.materialPreviewStyle = new GUIStyle(GUI.skin.box);
            this.materialPreviewStyle.fixedHeight = this.materialPreviewStyle.fixedWidth = 114;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        /// <remarks>
        /// <para>This means that your OnGUI implementation might be called several
        /// times per frame (one call per event).</para>
        /// </remarks>
        public void OnGUI()
        {
            // Show material mappings.
            int mappingCount = Mathf.Min(this.mappings.MaterialMappingFrom.Length, this.mappings.MaterialMappingTo.Length);
            for (int i = 0; i < mappingCount; ++i) {
                this.DrawMaterialMappingEntry(i);
                if (i + 1 < mappingCount) {
                    ExtraEditorGUI.SeparatorLight(marginBottom: 5);
                }
            }

            // Button to define new mapping.
            if (GUILayout.Button(TileLang.ParticularText("Action", "Add Material Mapping"), ExtraEditorStyles.Instance.BigButton)) {
                this.OnAddMaterialMapping();
            }
        }

        private void DrawMaterialMappingEntry(int index)
        {
            Rect containerPosition = EditorGUILayout.BeginHorizontal(GUILayout.Height(138f));
            GUILayout.Label(GUIContent.none);

            EditorGUI.BeginChangeCheck();

            Rect fromFieldPosition = containerPosition;
            fromFieldPosition.width = 114f;
            fromFieldPosition.height -= 6f;
            var newFromMaterial = this.DrawMaterialField(fromFieldPosition, this.mappings.MaterialMappingFrom[index]);

            if (Event.current.type == EventType.Repaint) {
                Rect arrowPosition = new Rect(containerPosition.x + 121f, containerPosition.y + 44f, 29f, 22f);
                GUI.DrawTexture(arrowPosition, RotorzEditorStyles.Skin.MappingArrow);
            }

            Rect toFieldPosition = containerPosition;
            toFieldPosition.x += 156f;
            toFieldPosition.width = 114f;
            toFieldPosition.height -= 6f;
            var newToMaterial = this.DrawMaterialField(toFieldPosition, this.mappings.MaterialMappingTo[index]);

            // Has material mapping changed?
            if (EditorGUI.EndChangeCheck()) {
                this.mappings.MaterialMappingFrom[index] = newFromMaterial;
                this.mappings.MaterialMappingTo[index] = newToMaterial;

                // Changes have been made!
                EditorUtility.SetDirty(this.brush);
                // Refresh brush preview.
                BrushUtility.RefreshPreviewIncludingDependencies(this.brush);
            }

            // Button to remove material mapping.
            Rect removeButtonPosition = new Rect(containerPosition.x + 300f, containerPosition.y + 3f, 112f, 33f);
            if (GUI.Button(removeButtonPosition, TileLang.ParticularText("Action", "Remove"))) {
                this.RemoveMaterialMapping(index);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private Material DrawMaterialField(Rect position, Material selectedMaterial)
        {
            if (Event.current.type == EventType.Repaint) {
                Rect previewPosition = position;
                previewPosition.width = 114;
                previewPosition.height = 114;

                if (ExtraEditorGUI.VisibleRect.Overlaps(previewPosition)) {
                    var materialPreviewTexture = AssetPreviewCache.GetAssetPreview(selectedMaterial);
                    this.materialPreviewStyle.Draw(previewPosition, materialPreviewTexture, false, false, false, false);
                }
            }

            position.y += 120;
            position.height = 16;
            return EditorGUI.ObjectField(position, selectedMaterial, typeof(Material), false) as Material;
        }

        public void OnAddMaterialMapping()
        {
            Undo.RecordObject(this.brush, TileLang.ParticularText("Action", "Add Material Mapping"));

            Material[] from = this.mappings.MaterialMappingFrom;
            Material[] to = this.mappings.MaterialMappingTo;

            // Fetch nth material from default variation.
            Material def = this.brush.GetNthMaterial(from.Length) ?? s_DefaultDiffuseMaterial;

            ArrayUtility.Add<Material>(ref from, def);
            ArrayUtility.Add<Material>(ref to, def);

            this.mappings.MaterialMappingFrom = from;
            this.mappings.MaterialMappingTo = to;

            // Changes have been made!
            EditorUtility.SetDirty(this.brush);
            // Refresh brush preview.
            BrushUtility.RefreshPreviewIncludingDependencies(this.brush);
        }

        /// <summary>
        /// Remove material mapping from brush.
        /// </summary>
        /// <param name="index">
        /// Zero-based index of mapping to remove.
        /// </param>
        public void RemoveMaterialMapping(int index)
        {
            Undo.RecordObject(this.brush, TileLang.ParticularText("Action", "Remove Material Mapping"));

            Material[] from = this.mappings.MaterialMappingFrom;
            Material[] to = this.mappings.MaterialMappingTo;

            if (index < 0 || index >= from.Length) {
                return;
            }

            // Remove material mapping.
            ArrayUtility.RemoveAt<Material>(ref from, index);
            ArrayUtility.RemoveAt<Material>(ref to, index);

            this.mappings.MaterialMappingFrom = from;
            this.mappings.MaterialMappingTo = to;

            // Changes have been made!
            EditorUtility.SetDirty(this.brush);
            // Refresh brush preview.
            BrushUtility.RefreshPreviewIncludingDependencies(this.brush);
        }
    }
}
