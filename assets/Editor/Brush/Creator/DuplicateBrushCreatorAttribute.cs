// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Defines the group that a <see cref="BrushCreator"/> implementation is a part of.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class BrushCreatorGroupAttribute : Attribute
    {
        /// <summary>
        /// Gets the annotated group of a given type.
        /// </summary>
        /// <param name="brushCreatorType">The type of brush creator.</param>
        /// <returns>
        /// The group of the given type.
        /// </returns>
        public static BrushCreatorGroup GetAnnotatedGroupOfType(Type brushCreatorType)
        {
            var attribute = GetCustomAttribute(brushCreatorType, typeof(BrushCreatorGroupAttribute), true) as BrushCreatorGroupAttribute;
            return attribute != null
                ? attribute.Group
                : BrushCreatorGroup.Default;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BrushCreatorGroupAttribute"/> class.
        /// </summary>
        /// <param name="group">Identifies the group of the brush creator.</param>
        public BrushCreatorGroupAttribute(BrushCreatorGroup group)
        {
            this.Group = group;
        }


        /// <summary>
        /// Gets the associated group.
        /// </summary>
        public BrushCreatorGroup Group { get; private set; }
    }
}
