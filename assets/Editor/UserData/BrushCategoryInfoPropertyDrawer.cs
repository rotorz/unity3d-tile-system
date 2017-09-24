// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="BrushCategoryInfo"/> data.
    /// </summary>
    [CustomPropertyDrawer(typeof(BrushCategoryInfo))]
    internal sealed class BrushCategoryInfoPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProperty = property.FindPropertyRelative("id");
            var labelProperty = property.FindPropertyRelative("label");

            float initialLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 64;

            if (ProjectSettings.Instance.ShowCategoryIds) {
                using (var content = ControlContent.Basic(
                    labelText: string.Format(
                        /* 0: category id */
                        TileLang.Text("Id: {0}"),
                        idProperty.intValue
                    )
                )) {
                    position = EditorGUI.PrefixLabel(position, content);
                }
            }

            labelProperty.stringValue = EditorGUI.TextField(position, labelProperty.stringValue);

            EditorGUIUtility.labelWidth = initialLabelWidth;
        }
    }
}
