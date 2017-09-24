// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Default inspector for brush assets.
    /// </summary>
    [CustomEditor(typeof(Brush), true)]
    public class BrushEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Indicates whether editor has been initialized.
        /// </summary>
        private bool hasInitialized;
        /// <summary>
        /// Indicates whether brush asset is accessible via brush database.
        /// </summary>
        private bool hasRecord;


        protected override void OnHeaderGUI()
        {
            if (!this.hasInitialized) {
                this.hasInitialized = true;

                // Find out whether brush asset is accessible via brush database.
                var record = BrushDatabase.Instance.FindRecord(target as Brush);
                this.hasRecord = (record != null);
            }

            if (this.hasRecord) {
                var brush = target as Brush;

                GUILayout.BeginHorizontal(RotorzEditorStyles.Instance.InspectorBigTitle);

                Rect previewPosition = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(64), GUILayout.Height(65));
                if (Event.current.type == EventType.Repaint) {
                    RotorzEditorStyles.Instance.Box.Draw(new Rect(previewPosition.x - 2, previewPosition.y - 2, 68, 68), GUIContent.none, false, false, false, false);
                    previewPosition = new Rect(previewPosition.x, previewPosition.y, 64, 64);
                    RotorzEditorGUI.DrawBrushPreview(previewPosition, brush);
                }

                GUILayout.BeginVertical();
                {
                    GUILayout.Label(brush.name, EditorStyles.largeLabel);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        var tilesetBrush = brush as TilesetBrush;
                        if (tilesetBrush != null) {
                            EditorGUI.BeginDisabledGroup(tilesetBrush.Tileset == null);
                            if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Goto Tileset")), EditorStyles.miniButton)) {
                                ToolUtility.ShowTilesetInDesigner(tilesetBrush.Tileset);
                                GUIUtility.ExitGUI();
                            }
                            EditorGUI.EndDisabledGroup();
                        }

                        if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Show in Designer")), EditorStyles.miniButton)) {
                            ToolUtility.ShowBrushInDesigner(brush);
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            else {
                // Nope, assume default header!
                base.OnHeaderGUI();
            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(TileLang.Text("Please use designer to edit brush."));
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var brush = target as Brush;

            var brushDescriptor = BrushUtility.GetDescriptor(brush.GetType());
            if (brushDescriptor != null && brushDescriptor.CanHavePreviewCache(brush)) {
                return BrushUtility.CreateBrushPreview(target as Brush, width, height);
            }
            else {
                return null;
            }
        }
    }
}
