// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Designer for <see cref="AliasBrush"/> brushes that transparently adapts to use the
    /// <see cref="AliasBrushDesigner"/> as specified by brush descriptor.
    /// </summary>
    internal sealed class AliasBrushDesignerAdaptor : BrushDesignerView
    {
        /// <summary>
        /// The alias brush that is being edited.
        /// </summary>
        private AliasBrush aliasBrush;
        /// <summary>
        /// Target of alias brush.
        /// </summary>
        private Brush targetBrush;

        /// <summary>
        /// The target brush designer as specified by brush descriptor.
        /// </summary>
        private AliasBrushDesigner targetDesigner;
        /// <summary>
        /// Indicates whether target brush supports aliases.
        /// </summary>
        private bool targetSupportsAliases;


        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.aliasBrush = this.Brush as AliasBrush;
            this.RefreshTargetDesigner();
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            if (this.targetDesigner != null) {
                this.targetDesigner.OnDisable();
            }

            base.OnDisable();
        }

        /// <inheritdoc/>
        public override void DrawSecondaryMenuButton(Rect position)
        {
            if (this.targetDesigner != null) {
                this.targetDesigner.DrawSecondaryMenuButton(position);
            }
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            // Update target?
            if (this.targetBrush != this.aliasBrush.target) {
                this.RefreshTargetDesigner();
            }

            if (!this.targetSupportsAliases) {
                EditorGUILayout.HelpBox("No alias designer was registered for '" + this.targetBrush.GetType().FullName + "'", MessageType.Warning);
            }

            if (this.targetDesigner != null) {
                this.targetDesigner.viewPosition = this.viewPosition;
                this.targetDesigner.viewScrollPosition = this.viewScrollPosition;

                this.targetDesigner.OnGUI();

                this.viewScrollPosition = this.targetDesigner.viewScrollPosition;
            }
        }

        /// <inheritdoc/>
        protected internal override void BeginExtendedProperties()
        {
            this.targetDesigner.BeginExtendedProperties();
        }

        /// <inheritdoc/>
        protected internal override void EndExtendedProperties()
        {
            this.targetDesigner.EndExtendedProperties();
        }

        /// <inheritdoc/>
        public override void OnExtendedPropertiesGUI()
        {
            if (this.targetDesigner != null) {
                this.targetDesigner.OnExtendedPropertiesGUI();
            }
        }


        private void RefreshTargetDesigner()
        {
            // Clear previous designer.
            if (this.targetDesigner != null) {
                this.targetDesigner.OnDisable();
                this.targetDesigner = null;
            }

            // Attach to new target.
            this.targetBrush = this.aliasBrush.target;

            BrushDescriptor brushDescriptor = null;
            this.targetSupportsAliases = true;

            if (this.targetBrush != null) {
                brushDescriptor = BrushUtility.GetDescriptor(this.targetBrush.GetType());
                if (brushDescriptor == null || !brushDescriptor.SupportsAliases) {
                    this.targetSupportsAliases = false;
                }
            }
            else {
                // Basic alias brush implementation.
                brushDescriptor = BrushUtility.GetDescriptor<Brush>();
            }

            this.targetDesigner = brushDescriptor.CreateAliasDesigner(this.aliasBrush);
            this.targetDesigner.Window = this.Window;
            this.targetDesigner.OnEnable();
        }
    }
}
