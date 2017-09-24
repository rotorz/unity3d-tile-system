// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Alias brush creator interface.
    /// </summary>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    [BrushCreatorGroup(BrushCreatorGroup.Duplication)]
    public sealed class AliasBrushCreator : BrushCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasBrushCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public AliasBrushCreator(IBrushCreatorContext context)
            : base(context)
        {
        }


        /// <inheritdoc/>
        public override string Name {
            get { return TileLang.ParticularText("BrushCreator|TabLabel", "Alias"); }
        }

        /// <inheritdoc/>
        public override string Title {
            get { return TileLang.Text("Create new alias brush"); }
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            GUILayout.Label(TileLang.Text("Create new brush that is based upon an existing brush to override properties and materials."), EditorStyles.wordWrappedLabel);
            GUILayout.Space(10f);

            this.DrawBrushNameField();

            GUILayout.Space(10f);

            ExtraEditorGUI.AbovePrefixLabel(TileLang.Text("Select target brush to create an alias of:"));
            var targetBrush = this.Context.GetSharedProperty<Brush>(BrushCreatorSharedPropertyKeys.TargetBrush);
            targetBrush = RotorzEditorGUI.BrushField(targetBrush, false);
            this.Context.SetSharedProperty(BrushCreatorSharedPropertyKeys.TargetBrush, targetBrush);

            RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Note: You cannot create an alias of another alias brush."));
        }

        /// <inheritdoc/>
        public override void OnButtonCreate()
        {
            string brushName = this.Context.GetSharedProperty(BrushCreatorSharedPropertyKeys.BrushName, "");
            var targetBrush = this.Context.GetSharedProperty<Brush>(BrushCreatorSharedPropertyKeys.TargetBrush);

            if (!this.ValidateInputs(brushName, targetBrush)) {
                return;
            }

            this.CreateAliasBrush(brushName, targetBrush);

            this.Context.Close();
        }


        private bool ValidateInputs(string brushName, Brush targetBrush)
        {
            if (!this.ValidateUniqueAssetName(brushName)) {
                return false;
            }

            if (targetBrush == null) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Target brush was not specified"),
                    TileLang.Text("Select the brush that you would like to create an alias of."),
                    TileLang.ParticularText("Action", "Close")
                );
                return false;
            }

            var targetBrushDescriptor = BrushUtility.GetDescriptor(targetBrush.GetType());
            if (targetBrushDescriptor == null || !targetBrushDescriptor.SupportsAliases) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Unable to create alias brush"),
                    string.Format(
                        /* 0: class of target brush */
                        TileLang.Text("No alias designer was registered for '{0}'"),
                        targetBrush.GetType().FullName
                    ),
                    TileLang.ParticularText("Action", "Close")
                );
                return false;
            }

            return true;
        }

        private void CreateAliasBrush(string brushName, Brush targetBrush)
        {
            var newAliasBrush = BrushUtility.CreateAliasBrush(brushName, targetBrush);
            ToolUtility.ShowBrushInDesigner(newAliasBrush);

            ToolUtility.RepaintBrushPalette();
        }
    }
}
