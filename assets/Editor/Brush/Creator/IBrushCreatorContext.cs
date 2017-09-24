// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// The context of a <see cref="BrushCreator"/> instance.
    /// </summary>
    public interface IBrushCreatorContext : IRepaintableUI, ICloseableUI, IFocusableUI
    {
        /// <summary>
        /// Gets the name of the "Asset Name" control (for instance, "Brush Name" or
        /// "Tileset Name").
        /// </summary>
        /// <remarks>
        /// <para>If the <see cref="BrushCreator"/> implementation should use this to
        /// identify the IMGUI control when the interface allows the user to enter a
        /// brush name.</para>
        /// </remarks>
        string PrimaryAssetNameControlName { get; }

        /// <summary>
        /// Gets the collection of properties that are shared across <see cref="BrushCreator"/>
        /// implementations. This is useful for retaining values when switching between
        /// interfaces.
        /// </summary>
        /// <remarks>
        /// <para>Refer to <see cref="BrushCreatorSharedPropertyKeys"/> for the built-in
        /// shared property keys.</para>
        /// <para>DO NOT use square brackets around custom shared property key names.
        /// This is a convention used only for the built-in shared properties to avoid
        /// clashes if new built-in's are added in the future.</para>
        /// </remarks>
        IDictionary<string, object> SharedProperties { get; }
    }
}
