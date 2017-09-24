// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Custom editor for reduce collider options.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReduceColliderOptions))]
    internal sealed class ReduceColliderOptionsPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + 1);

            var propActive = property.FindPropertyRelative("isActive");

            ExtraEditorGUI.ToggleLeft(rect, propActive, label);
            rect.y = rect.yMax + 1;

            if (propActive.boolValue) {
                ++EditorGUI.indentLevel;

                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Snap Threshold")
                )) {
                    var propSnapThreshold = property.FindPropertyRelative("snapThreshold");
                    EditorGUI.PropertyField(rect, propSnapThreshold, content);
                    rect.y = rect.yMax + 1;
                }

                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Keep Separate")
                )) {
                    var propKeepSeparate = property.FindPropertyRelative("keepSeparate");
                    DrawKeepSeparateField(rect, propKeepSeparate, content);
                    rect.y = rect.yMax + 1;
                }

                using (var content = ControlContent.Basic(
                    TileLang.ParticularText("Property", "Include tiles flagged as solid")
                )) {
                    var propIncludeSolidTiles = property.FindPropertyRelative("includeSolidTiles");
                    ExtraEditorGUI.ToggleLeft(rect, propIncludeSolidTiles, content);
                    rect.y = rect.yMax + 1;

                    if (propIncludeSolidTiles.boolValue) {
                        ++EditorGUI.indentLevel;

                        using (var content2 = ControlContent.Basic(
                            TileLang.ParticularText("Property", "Collider Type")
                        )) {
                            var propSolidTileColliderType = property.FindPropertyRelative("solidTileColliderType");
                            EditorGUI.PropertyField(rect, propSolidTileColliderType, content2);
                        }

                        --EditorGUI.indentLevel;
                    }
                }

                --EditorGUI.indentLevel;
            }
        }

        private static void DrawKeepSeparateField(Rect position, SerializedProperty property, GUIContent label)
        {
            bool initialShowMixedValue = EditorGUI.showMixedValue;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            KeepSeparateColliderFlag keepSeparateFlags = (KeepSeparateColliderFlag)EditorGUI.EnumMaskField(position, label, (KeepSeparateColliderFlag)property.intValue);
            EditorGUI.showMixedValue = initialShowMixedValue;
            if (EditorGUI.EndChangeCheck()) {
                property.intValue = (int)keepSeparateFlags & 0x03;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 1;

            var propActive = property.FindPropertyRelative("isActive");
            if (propActive.boolValue) {
                lineCount += 3;

                var propIncludeSolidTiles = property.FindPropertyRelative("includeSolidTiles");
                if (propIncludeSolidTiles.boolValue) {
                    ++lineCount;
                }
            }

            return (EditorGUIUtility.singleLineHeight + 2) * lineCount + 5;
        }
    }
}
