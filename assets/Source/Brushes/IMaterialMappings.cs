// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Interface that can be implemented by brushes for material mapping capabilities.
    /// </summary>
    /// <remarks>
    /// <para>User interface for material mapping is automatically shown for brushes that
    /// implement this interface.</para>
    /// </remarks>
    /// <example>
    /// <para>It is important to ensure that mapping fields are properly serialized:</para>
    /// <code language="csharp"><![CDATA[
    /// using Rotorz.Tile;
    /// using UnityEngine;
    ///
    /// public class MyMagicBrush : Brush, IMaterialMappings
    /// {
    ///     // Serialize mapping fields.
    ///     [SerializeField]
    ///     private Material[] materialMappingFrom;
    ///     [SerializeField]
    ///     private Material[] materialMappingTo;
    ///
    ///
    ///     // Exposed properties for material mapping implementation.
    ///     public Material[] MaterialMappingFrom {
    ///         get { return this.materialMappingFrom; }
    ///         set { this.materialMappingFrom = value; }
    ///     }
    ///
    ///     public Material[] MaterialMappingTo {
    ///         get { return this.materialMappingTo; }
    ///         set { this.materialMappingTo = value; }
    ///     }
    ///
    ///
    ///     // Brush implementation...
    /// }
    /// ]]></code>
    /// </example>
    public interface IMaterialMappings
    {
        /// <summary>
        /// Gets or sets list of materials to map from.
        /// </summary>
        /// <remarks>
        /// <para>Each material in this list must be paired with another in <see cref="MaterialMappingTo"/>
        /// so that brushes can remap the materials of painted tiles.</para>
        /// <para>Any changes that are made to this field will not be applied to previously
        /// painted tiles unless they are force refreshed.</para>
        /// </remarks>
        Material[] MaterialMappingFrom { get; set; }

        /// <summary>
        /// Gets or sets list of materials to map to.
        /// </summary>
        /// <remarks>
        /// <para>Each material in this list must be paired with another in <see cref="MaterialMappingFrom"/>
        /// so that brushes can remap the materials of painted tiles.</para>
        /// <para>Any changes that are made to this field will not be applied to previously
        /// painted tiles unless they are force refreshed.</para>
        /// </remarks>
        Material[] MaterialMappingTo { get; set; }
    }
}
