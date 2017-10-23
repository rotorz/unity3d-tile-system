// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Designer for <see cref="TilesetBrush"/> brushes.
    /// </summary>
    public class TilesetBrushDesigner : BrushDesignerView
    {
        /// <summary>
        /// Gets the tileset brush that is being edited.
        /// </summary>
        public TilesetBrush TilesetBrush { get; private set; }

        /// <summary>
        /// Gets the brush database record for the tileset that is being edited.
        /// </summary>
        protected TilesetAssetRecord TilesetRecord { get; private set; }


        /// <summary>
        /// Indicates if brush prefab is to be attached.
        /// </summary>
        internal bool brushAttachPrefabTick;


        #region Messages and Events

        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.TilesetBrush = Brush as TilesetBrush;
            this.TilesetRecord = BrushDatabase.Instance.FindTilesetRecord(TilesetBrush.Tileset);

            this.brushAttachPrefabTick = (this.TilesetBrush.attachPrefab != null);
        }

        /// <inheritdoc/>
        public override void DrawSecondaryMenuButton(Rect position)
        {
            EditorGUI.BeginDisabledGroup(this.TilesetBrush.Tileset == null);

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.GotoTileset,
                TileLang.FormatActionWithShortcut(
                    TileLang.ParticularText("Action", "Goto Tileset"), "F3"
                )
            )) {
                if (RotorzEditorGUI.HoverButton(position, content)) {
                    this.OnViewTileset();
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            // Permit shortcut key "F3"
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F3) {
                Event.current.Use();
                this.OnViewTileset();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            this.DrawLargeTilePreview();

            this.DrawTilesetBrushInfoSection();

            GUILayout.EndHorizontal();
            ExtraEditorGUI.SeparatorLight(marginTop: 10, thickness: 3);
        }

        private void DrawLargeTilePreview()
        {
            Rect previewPosition = GUILayoutUtility.GetRect(160, 160, 150, 150, RotorzEditorStyles.Instance.Box);

            // Draw background box for larger tile preview.
            previewPosition.width -= 10;
            if (Event.current.type == EventType.Repaint) {
                RotorzEditorStyles.Instance.Box.Draw(previewPosition, GUIContent.none, 0);
            }

            // Draw tile preview.
            previewPosition.x += 2;
            previewPosition.y += 2;
            previewPosition.width -= 4;
            previewPosition.height -= 4;

            BrushAssetRecord record = BrushDatabase.Instance.FindRecord(this.Brush);
            RotorzEditorGUI.DrawBrushPreview(previewPosition, record);
        }

        private void DrawTilesetBrushInfoSection()
        {
            var tileset = this.TilesetBrush.Tileset;

            GUILayout.BeginVertical();

            if (this.TilesetRecord == null) {
                GUILayout.Label(TileLang.Text("Error: No tileset associated with brush."));
                return;
            }

            float restoreLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.Space(10);

            this.DrawTilesetBrushInfo();

            GUILayout.Space(10);

            this.DrawTilesetBrushInfoActions();

            EditorGUIUtility.labelWidth = restoreLabelWidth;

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws information about the tileset brush.
        /// </summary>
        protected virtual void DrawTilesetBrushInfo()
        {
            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "From Tileset"),
                this.TilesetRecord.DisplayName
            );
            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "Tile Index"),
                this.TilesetBrush.tileIndex.ToString()
            );

            GUILayout.Space(10);

            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "Tile Width"),
                TileLang.FormatPixelMetric(this.TilesetBrush.Tileset.TileWidth)
            );
            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "Tile Height"),
                TileLang.FormatPixelMetric(this.TilesetBrush.Tileset.TileHeight)
            );
        }

        protected virtual void DrawTilesetBrushInfoActions()
        {
            this.DrawProceduralField(this.TilesetBrush, this.TilesetBrush.Tileset);

            if (this.TilesetBrush.Tileset.GetTileMesh(this.TilesetBrush.tileIndex) != null) {
                RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Non-procedural mesh has been generated."));
            }
            else if (!this.TilesetBrush.IsProcedural) {
                EditorGUILayout.HelpBox(TileLang.Text("Non-procedural mesh asset is missing."), MessageType.Warning, true);
                if (GUILayout.Button(TileLang.ParticularText("Action", "Repair"))) {
                    // Ensure that required procedural mesh exists!
                    if (BrushUtility.EnsureTilesetMeshExists(this.TilesetBrush.Tileset, this.TilesetBrush.tileIndex)) {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.TilesetBrush.Tileset.tileMeshAsset));
                    }

                    GUIUtility.ExitGUI();
                }
            }
        }

        private void DrawProceduralField(TilesetBrush brush, Tileset tileset)
        {
            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Procedural"),
                TileLang.Text("Allows individual atlas brushes to override property of tileset.")
            )) {
                // Need to make width of "Procedural" popup shorter.
                GUILayout.BeginHorizontal(GUILayout.Width(200));
                InheritYesNo newProcedural = (InheritYesNo)EditorGUILayout.EnumPopup(content, brush.procedural);
                if (newProcedural != brush.procedural) {
                    brush.procedural = newProcedural;

                    if (!brush.IsProcedural) {
                        // Ensure that required procedural mesh exists!
                        if (BrushUtility.EnsureTilesetMeshExists(tileset, brush.tileIndex)) {
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tileset.tileMeshAsset));
                        }
                    }
                }

                Rect position = GUILayoutUtility.GetLastRect();
                GUI.Label(new Rect(position.x + position.width, position.y, 100, position.height), "= " + (brush.IsProcedural ? TileLang.Text("Procedural") : TileLang.Text("Non-Procedural")), EditorStyles.miniLabel);

                GUILayout.EndHorizontal();

                ExtraEditorGUI.TrailingTip(content);
            }
        }

        /// <inheritdoc/>
        public override void OnExtendedPropertiesGUI()
        {
            bool autoInitCollider;

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Add Collider"),
                TileLang.Text("Automatically adds box collider to painted tile.")
            )) {
                EditorGUI.BeginChangeCheck();
                this.TilesetBrush.addCollider = EditorGUILayout.ToggleLeft(content, this.TilesetBrush.addCollider);
                autoInitCollider = (EditorGUI.EndChangeCheck() && this.TilesetBrush.addCollider);
                if (this.TilesetBrush.addCollider) {
                    ++EditorGUI.indentLevel;
                    this.TilesetBrush.colliderType = (ColliderType)EditorGUILayout.EnumPopup(this.TilesetBrush.colliderType);
                    --EditorGUI.indentLevel;
                }
                ExtraEditorGUI.TrailingTip(content);
            }

            if (autoInitCollider) {
                this.TilesetBrush.colliderType = BrushUtility.AutomaticColliderType;
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Always Add Container"),
                TileLang.Text("Add tile container object even when not needed by brush.")
            )) {
                if (this.TilesetBrush.IsProcedural) {
                    this.TilesetBrush.alwaysAddContainer = EditorGUILayout.ToggleLeft(content, this.TilesetBrush.alwaysAddContainer);
                    ExtraEditorGUI.TrailingTip(content);
                }
            }

            if (this.brushAttachPrefabTick) {
                ExtraEditorGUI.SeparatorLight();
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Attach Prefab"),
                TileLang.Text("Additional game objects can be painted by attaching a prefab.")
            )) {
                bool newAttachPrefabTick = EditorGUILayout.ToggleLeft(content, this.brushAttachPrefabTick);
                if (!newAttachPrefabTick) {
                    ExtraEditorGUI.TrailingTip(content);
                }

                // Has state of prefab tick changed?
                if (newAttachPrefabTick != this.brushAttachPrefabTick) {
                    this.brushAttachPrefabTick = newAttachPrefabTick;
                    // Should attachment be cleared?
                    if (!this.brushAttachPrefabTick) {
                        this.TilesetBrush.attachPrefab = null;
                    }
                }

                if (this.brushAttachPrefabTick) {
                    ++EditorGUI.indentLevel;

                    this.TilesetBrush.attachPrefab = EditorGUILayout.ObjectField(this.TilesetBrush.attachPrefab, typeof(GameObject), false) as GameObject;
                    GUILayout.Space(2);
                    this.OnExtendedGUI_ScaleMode();

                    --EditorGUI.indentLevel;
                }
            }
        }

        #endregion


        #region Button / Action Handlers

        /// <summary>
        /// Occurs when "View Tileset" is clicked.
        /// </summary>
        protected virtual void OnViewTileset()
        {
            var designerWindow = Window as DesignerWindow;
            if (designerWindow == null) {
                return;
            }

            // Put tileset into edit mode.
            designerWindow.SelectedObject = this.TilesetBrush.Tileset;
        }

        #endregion
    }
}
