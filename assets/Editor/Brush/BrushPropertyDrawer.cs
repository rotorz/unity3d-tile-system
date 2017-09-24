// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Adds custom inspector field for brush selection.
    /// </summary>
    [CustomPropertyDrawer(typeof(Brush))]
    [CustomPropertyDrawer(typeof(BrushPropertyAttribute))]
    internal sealed class BrushPropertyDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as BrushPropertyAttribute;

            bool initialShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            var brush = RotorzEditorGUI.BrushField(
                position,
                label.text,
                property.objectReferenceValue as Brush,
                attr != null ? attr.AllowAlias : true,
                attr != null ? attr.AllowMaster : true
            );
            if (EditorGUI.EndChangeCheck()) {
                property.objectReferenceValue = brush;
            }

            EditorGUI.showMixedValue = initialShowMixedValue;
        }
    }
}
