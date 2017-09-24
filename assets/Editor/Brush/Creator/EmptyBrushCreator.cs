// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Empty brush creator interface.
    /// </summary>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    public sealed class EmptyBrushCreator : BrushCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyBrushCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public EmptyBrushCreator(IBrushCreatorContext context)
            : base(context)
        {
        }


        /// <inheritdoc/>
        public override string Name {
            get { return TileLang.ParticularText("BrushCreator|TabLabel", "Empty"); }
        }

        /// <inheritdoc/>
        public override string Title {
            get { return TileLang.Text("Create new empty brush"); }
        }


        /// <inheritdoc/>
        public override void OnGUI()
        {
            GUILayout.Label(TileLang.Text("Create brush whose tiles have no visual representation."), EditorStyles.wordWrappedLabel);
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

            this.CreateEmptyBrush(brushName);

            this.Context.Close();
        }


        private bool ValidateInputs(string brushName)
        {
            return this.ValidateUniqueAssetName(brushName);
        }

        private void CreateEmptyBrush(string brushName)
        {
            var newEmptyBrush = BrushUtility.CreateEmptyBrush(brushName);
            ToolUtility.ShowBrushInDesigner(newEmptyBrush);

            ToolUtility.RepaintBrushPalette();
        }
    }
}
