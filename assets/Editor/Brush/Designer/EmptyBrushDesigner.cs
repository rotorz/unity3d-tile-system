// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Designer for <see cref="EmptyBrush"/> brushes.
    /// </summary>
    internal sealed class EmptyBrushDesigner : BrushDesignerView
    {
        /// <inheritdoc/>
        public override void OnExtendedPropertiesGUI()
        {
            var emptyBrush = Brush as EmptyBrush;

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Always Add Container"),
                TileLang.Text("Add tile container object even when not needed by brush.")
            )) {
                emptyBrush.alwaysAddContainer = EditorGUILayout.ToggleLeft(content, emptyBrush.alwaysAddContainer);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Add Collider"),
                TileLang.Text("Automatically adds box collider to painted tile.")
            )) {
                emptyBrush.addCollider = EditorGUILayout.ToggleLeft(content, emptyBrush.addCollider);
                if (emptyBrush.addCollider) {
                    ++EditorGUI.indentLevel;
                    emptyBrush.colliderType = (ColliderType)EditorGUILayout.EnumPopup(emptyBrush.colliderType);
                    --EditorGUI.indentLevel;
                }
                ExtraEditorGUI.TrailingTip(content);
            }
        }
    }
}
