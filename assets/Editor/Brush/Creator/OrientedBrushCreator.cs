// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Oriented brush creator interface.
    /// </summary>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    public sealed class OrientedBrushCreator : BrushCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrientedBrushCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public OrientedBrushCreator(IBrushCreatorContext context)
            : base(context)
        {
        }


        /// <inheritdoc/>
        public override string Name {
            get { return TileLang.ParticularText("BrushCreator|TabLabel", "Oriented"); }
        }

        /// <inheritdoc/>
        public override string Title {
            get { return TileLang.Text("Create new oriented brush"); }
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            GUILayout.Label(
                TileLang.Text("Simple brushes can be created by adding one or more tile variations to the default orientation. Additional orientations can be defined so that tiles are painted based upon neighboring tiles."),
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(10f);

            this.DrawBrushNameField();
        }

        /// <inheritdoc/>
        public override void OnButtonCreate()
        {
            string brushName = this.Context.GetSharedProperty(BrushCreatorSharedPropertyKeys.BrushName, "");

            if (!this.ValidateInputs(brushName)) {
                return;
            }

            this.CreateOrientedBrush(brushName);

            this.Context.Close();
        }


        private bool ValidateInputs(string brushName)
        {
            return this.ValidateUniqueAssetName(brushName);
        }

        private void CreateOrientedBrush(string brushName)
        {
            var newOrientedBrush = BrushUtility.CreateOrientedBrush(brushName);
            ToolUtility.ShowBrushInDesigner(newOrientedBrush);

            ToolUtility.RepaintBrushPalette();
        }
    }
}
