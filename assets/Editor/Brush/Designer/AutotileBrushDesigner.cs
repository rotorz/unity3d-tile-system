// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Designer for <see cref="AutotileBrush"/> brushes.
    /// </summary>
    internal class AutotileBrushDesigner : TilesetBrushDesigner
    {
        /// <summary>
        /// Gets autotile brush that is being edited.
        /// </summary>
        public AutotileBrush AutotileBrush { get; private set; }


        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.AutotileBrush = Brush as AutotileBrush;
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            base.OnGUI();
        }

        /// <inheritdoc/>
        protected override void DrawTilesetBrushInfo()
        {
            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "From Tileset"),
                this.TilesetRecord.DisplayName
            );
            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "Layout"),
                this.AutotileBrush.Layout.ToString()
            );

            EditorGUILayout.LabelField(
                TileLang.ParticularText("Property", "Inner Joins"),
                (this.AutotileBrush.Tileset != null)
                    ? TileLang.FormatYesNoStatus(this.AutotileBrush.Tileset.HasInnerJoins)
                    : TileLang.ParticularText("Status|Unknown", "?")
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

        /// <inheritdoc/>
        protected override void DrawTilesetBrushInfoActions()
        {
            // Do not inherit actions of tileset brush designer because autotile brushes
            // cannot be non-procedural.
        }

        /// <inheritdoc/>
        public override void OnExtendedPropertiesGUI()
        {
            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Always Add Container"),
                TileLang.Text("Add tile container object even when not needed by brush.")
            )) {
                if (this.AutotileBrush.IsProcedural) {
                    this.AutotileBrush.alwaysAddContainer = EditorGUILayout.ToggleLeft(content, this.AutotileBrush.alwaysAddContainer);
                    ExtraEditorGUI.TrailingTip(content);

                    if (this.brushAttachPrefabTick) {
                        ExtraEditorGUI.SeparatorLight();
                    }
                }
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
                        TilesetBrush.attachPrefab = null;
                    }
                }
            }

            if (this.brushAttachPrefabTick) {
                ++EditorGUI.indentLevel;

                this.TilesetBrush.attachPrefab = EditorGUILayout.ObjectField(TilesetBrush.attachPrefab, typeof(GameObject), false) as GameObject;
                GUILayout.Space(3);
                this.OnExtendedGUI_ScaleMode();

                --EditorGUI.indentLevel;
            }

            ExtraEditorGUI.SeparatorLight(marginBottom: 2);

            float restoreLabelWidth = EditorGUIUtility.labelWidth;
            float restoreFieldWidth = EditorGUIUtility.fieldWidth;

            bool autoInitCollider = false;

            EditorGUIUtility.labelWidth = 70;
            EditorGUIUtility.fieldWidth = 1;

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Edge Tiles"));

                    using (var content = ControlContent.Basic(
                        TileLang.ParticularText("Flaggable", "Solid Flag"),
                        TileLang.Text("Solid flag can be used to assist with user defined collision detection or pathfinding.")
                    )) {
                        this.AutotileBrush.SolidFlag = EditorGUILayout.ToggleLeft(content, this.AutotileBrush.SolidFlag);
                    }

                    using (var content = ControlContent.Basic(
                        TileLang.ParticularText("Property", "Add Collider"),
                        TileLang.Text("Automatically adds box collider to painted tile.")
                    )) {
                        EditorGUI.BeginChangeCheck();
                        this.AutotileBrush.addCollider = EditorGUILayout.ToggleLeft(content, this.AutotileBrush.addCollider);
                        autoInitCollider |= (EditorGUI.EndChangeCheck() && this.AutotileBrush.addCollider);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Inner Tiles"));

                    using (var content = ControlContent.Basic(
                        TileLang.ParticularText("Flaggable", "Solid Flag"),
                        TileLang.Text("Solid flag can be used to assist with user defined collision detection or pathfinding.")
                    )) {
                        this.AutotileBrush.InnerSolidFlag = EditorGUILayout.ToggleLeft(content, this.AutotileBrush.InnerSolidFlag);
                    }

                    using (var content = ControlContent.Basic(
                        TileLang.ParticularText("Property", "Add Collider"),
                        TileLang.Text("Automatically adds box collider to painted tile.")
                    )) {
                        EditorGUI.BeginChangeCheck();
                        this.AutotileBrush.addInnerCollider = EditorGUILayout.ToggleLeft(content, this.AutotileBrush.addInnerCollider);
                        autoInitCollider |= (EditorGUI.EndChangeCheck() && this.AutotileBrush.addInnerCollider);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            if (autoInitCollider) {
                this.AutotileBrush.colliderType = BrushUtility.AutomaticColliderType;
            }

            if (this.AutotileBrush.addCollider || this.AutotileBrush.addInnerCollider) {
                ++EditorGUI.indentLevel;
                this.AutotileBrush.colliderType = (ColliderType)EditorGUILayout.EnumPopup(this.AutotileBrush.colliderType);
                --EditorGUI.indentLevel;
            }

            EditorGUIUtility.labelWidth = restoreLabelWidth;
            EditorGUIUtility.fieldWidth = restoreFieldWidth;

            if (ControlContent.TrailingTipsVisible) {
                ExtraEditorGUI.TrailingTip(TileLang.Text("Edge and Inner 'solid' flag can be used for custom collision detection. Avoid inner colliders where possible."));
            }
        }

        /// <inheritdoc/>
        protected internal override void EndExtendedProperties()
        {
            ShowExtendedOrientation = RotorzEditorGUI.FoldoutSection(ShowExtendedOrientation,
                label: TileLang.Text("Automatic Orientations"),
                callback: this.OnExtendedGUI_Coalescing
            );

            base.EndExtendedProperties();
        }
    }
}
