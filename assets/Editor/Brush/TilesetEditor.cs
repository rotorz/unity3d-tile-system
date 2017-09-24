// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Default inspector for tileset assets.
    /// </summary>
    [CustomEditor(typeof(Tileset), true)]
    public class TilesetEditor : UnityEditor.Editor
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
                var record = BrushDatabase.Instance.FindTilesetRecord(target as Tileset);
                this.hasRecord = (record != null);
            }

            if (this.hasRecord) {
                var tileset = target as Tileset;

                GUILayout.BeginHorizontal(RotorzEditorStyles.Instance.InspectorBigTitle);

                Rect previewPosition = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(64), GUILayout.Height(65));
                if (Event.current.type == EventType.Repaint && ExtraEditorGUI.VisibleRect.Overlaps(previewPosition)) {
                    RotorzEditorStyles.Instance.Box.Draw(new Rect(previewPosition.x - 2, previewPosition.y - 2, 68, 68), GUIContent.none, false, false, false, false);
                    previewPosition = new Rect(previewPosition.x, previewPosition.y, 64, 64);

                    var previewAsset = tileset.AtlasTexture ?? target;
                    var tilesetPreviewTexture = AssetPreviewCache.GetAssetPreview(previewAsset);
                    if (!tilesetPreviewTexture) {
                        if (AssetPreview.IsLoadingAssetPreview(previewAsset.GetInstanceID())) {
                            this.Repaint();
                        }
                        tilesetPreviewTexture = AssetPreview.GetMiniThumbnail(previewAsset);
                    }

                    GUI.DrawTexture(previewPosition, tilesetPreviewTexture);
                }

                GUILayout.BeginVertical();
                {
                    GUILayout.Label(tileset.name, EditorStyles.largeLabel);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(TileLang.OpensWindow(TileLang.ParticularText("Action", "Show in Designer")), EditorStyles.miniButton)) {
                            ToolUtility.ShowTilesetInDesigner(tileset);
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
            GUILayout.Label(TileLang.Text("Please use designer to edit tileset."));
        }
    }
}
