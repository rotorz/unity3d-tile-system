// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Duplicate brush creator interface.
    /// </summary>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    [BrushCreatorGroup(BrushCreatorGroup.Duplication)]
    public sealed class DuplicateBrushCreator : BrushCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateBrushCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public DuplicateBrushCreator(IBrushCreatorContext context)
            : base(context)
        {
        }


        /// <inheritdoc/>
        public override string Name {
            get { return TileLang.ParticularText("BrushCreator|TabLabel", "Duplicate"); }
        }

        /// <inheritdoc/>
        public override string Title {
            get { return TileLang.Text("Create duplicate of an existing brush"); }
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            GUILayout.Space(10f);

            this.DrawBrushNameField();

            GUILayout.Space(10f);

            ExtraEditorGUI.AbovePrefixLabel(TileLang.Text("Select target brush to create duplicate from:"));

            var targetBrush = this.Context.GetSharedProperty<Brush>(BrushCreatorSharedPropertyKeys.TargetBrush);
            targetBrush = RotorzEditorGUI.BrushField(targetBrush);
            this.Context.SetSharedProperty(BrushCreatorSharedPropertyKeys.TargetBrush, targetBrush);
        }

        /// <inheritdoc/>
        public override void OnButtonCreate()
        {
            string brushName = this.Context.GetSharedProperty(BrushCreatorSharedPropertyKeys.BrushName, "");
            var targetBrush = this.Context.GetSharedProperty<Brush>(BrushCreatorSharedPropertyKeys.TargetBrush);

            if (!this.ValidateInputs(brushName, targetBrush)) {
                return;
            }

            this.CreateDuplicateBrush(brushName, targetBrush);

            this.Context.Close();
        }


        private bool ValidateInputs(string brushName, Brush targetBrush)
        {
            //!TODO: Can this be improved? What about other custom assets that contain
            //       one or more other brushes?

            var targetTilesetBrush = targetBrush as TilesetBrush;
            if (targetTilesetBrush != null) {
                // Validate name within scope of tileset.
                var tilesetRecord = BrushDatabase.Instance.FindTilesetRecord(targetTilesetBrush.Tileset);
                if (tilesetRecord != null && !tilesetRecord.IsNameUnique(brushName, null)) {
                    EditorUtility.DisplayDialog(
                        TileLang.Text("Asset already exists"),
                        TileLang.Text("Tileset already contains a brush with that name."),
                        TileLang.ParticularText("Action", "OK")
                    );
                    return false;
                }
            }
            else if (!this.ValidateUniqueAssetName(brushName)) {
                return false;
            }

            if (targetBrush == null) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Target brush not specified"),
                    TileLang.Text("Select the brush to duplicate."),
                    TileLang.ParticularText("Action", "Close")
                );
                return false;
            }

            return true;
        }

        private void CreateDuplicateBrush(string brushName, Brush targetBrush)
        {
            var newDuplicateBrush = BrushUtility.DuplicateBrush(brushName, targetBrush);
            ToolUtility.ShowBrushInDesigner(newDuplicateBrush);

            ToolUtility.RepaintBrushPalette();
        }
    }
}
