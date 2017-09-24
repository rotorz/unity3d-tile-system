// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Describes a kind of brush.
    /// </summary>
    /// <remarks>
    /// <para>Each kind of brush has its own <see cref="BrushDesignerView"/> which provides
    /// an appropriate user interface for the brush designer window. It is also possible
    /// to provide custom user interfaces when editing aliases of specific kinds of brush.</para>
    /// </remarks>
    public class BrushDescriptor
    {
        /// <summary>
        /// Gets <see cref="System.Type"/> of described brush.
        /// </summary>
        public Type BrushType { get; private set; }

        /// <summary>
        /// Gets editor <see cref="System.Type"/> for described type of brush.
        /// </summary>
        public Type DesignerType { get; private set; }

        /// <summary>
        /// Gets editor <see cref="System.Type"/> for alises of described type of brush.
        /// </summary>
        public Type AliasDesignerType { get; private set; }


        /// <summary>
        /// Gets a value indicating whether alias brushes can be created for
        /// described type of brush.
        /// </summary>
        public bool SupportsAliases {
            get { return !ReferenceEquals(this.AliasDesignerType, null); }
        }

        /// <summary>
        /// Gets display name for brush type.
        /// </summary>
        public string DisplayName {
            get {
                return ObjectNames.NicifyVariableName(this.BrushType.Name);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BrushDescriptor"/> class
        /// </summary>
        /// <param name="brushType">Type of described brush.</param>
        /// <param name="brushDesignerType">Designer type for described brush.</param>
        /// <param name="brushAliasDesignerType">Designer type for aliases of described brush.</param>
        public BrushDescriptor(Type brushType, Type brushDesignerType, Type brushAliasDesignerType)
        {
            this.BrushType = brushType;
            this.DesignerType = brushDesignerType;
            this.AliasDesignerType = brushAliasDesignerType;
        }


        /// <summary>
        /// Create designer for editing a brush of the described kind of brush.
        /// </summary>
        /// <param name="brush">Brush instance to edit.</param>
        /// <returns>
        /// The <see cref="BrushDesignerView"/>.
        /// </returns>
        public BrushDesignerView CreateDesigner(Brush brush)
        {
            if (ReferenceEquals(this.DesignerType, null)) {
                return null;
            }

            var brushDesigner = Activator.CreateInstance(this.DesignerType) as BrushDesignerView;
            brushDesigner.Brush = brush;

            return brushDesigner;
        }

        /// <summary>
        /// Create designer for editing an alias brush that targets a brush of the
        /// described kind.
        /// </summary>
        /// <remarks>
        /// <para>Each brush can define their own alias brush designer which is used to
        /// design alias brushes that target them.</para>
        /// </remarks>
        /// <param name="brush">Brush instance to edit.</param>
        /// <returns>
        /// The <see cref="AliasBrushDesigner"/>.
        /// </returns>
        internal AliasBrushDesigner CreateAliasDesigner(AliasBrush brush)
        {
            if (ReferenceEquals(this.AliasDesignerType, null)) {
                return null;
            }

            var aliasBrushDesigner = Activator.CreateInstance(this.AliasDesignerType) as AliasBrushDesigner;
            aliasBrushDesigner.Brush = brush;

            return aliasBrushDesigner;
        }

        /// <summary>
        /// Gets a value indicating whether the specified brush supports preview cache.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// A value of <c>true</c> if possible; otherwise <c>false</c>.
        /// </returns>
        public virtual bool CanHavePreviewCache(Brush brush)
        {
            return false;
        }

        /// <summary>
        /// Draw brush preview to GUI.
        /// </summary>
        /// <param name="output">Output position of brush preview.</param>
        /// <param name="record">The brush record.</param>
        /// <param name="selected">Indicates if preview is highlighted.</param>
        /// <returns>
        /// A value of <c>true</c> indicates if preview was drawn; otherwise <c>false</c>
        /// indicates that caller should assume default.
        /// </returns>
        protected internal virtual bool DrawPreview(Rect output, BrushAssetRecord record, bool selected)
        {
            return false;
        }

        /// <summary>
        /// Duplicate a brush that is described by this descriptor.
        /// </summary>
        /// <param name="name">Name for new brush.</param>
        /// <param name="record">Record for brush that is to be duplicated.</param>
        public virtual Brush DuplicateBrush(string name, BrushAssetRecord record)
        {
            // Duplicate entire asset because brush IS the main asset.
            if (record.MainAsset == record.Brush) {
                string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(BrushUtility.GetBrushAssetPath() + name + ".asset");
                if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(record.MainAsset), newAssetPath)) {
                    return null;
                }

                // Load the new asset.
                AssetDatabase.ImportAsset(newAssetPath);
                return AssetDatabase.LoadMainAssetAtPath(newAssetPath) as Brush;
            }

            return null;
        }

        /// <summary>
        /// Delete a brush that is described by this descriptor.
        /// </summary>
        /// <param name="record">Record for brush that is to be deleted.</param>
        /// <returns>
        /// A value of <c>true</c> if brush was deleted; otherwise <c>false</c>.
        /// </returns>
        public virtual bool DeleteBrush(BrushAssetRecord record)
        {
            // Delete entire asset if brush IS the main asset.
            if (record.MainAsset == record.Brush) {
                if (AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(record.MainAsset))) {
                    BrushDatabase.Instance.ClearMissingRecords();
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
