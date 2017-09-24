// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// This attribute can be applied to brush properties in custom scripts to specify
    /// whether alias or master brushes can be selected when using brush picker interface
    /// via the inspector.
    /// </summary>
    /// <example>
    /// <para>The following shows how to prevent selection of alias brushes:</para>
    /// <code language="csharp"><![CDATA[
    /// using Rotorz.Tile;
    /// using UnityEngine;
    ///
    /// public class ExampleBehaviour : MonoBehaviour
    /// {
    ///     [BrushProperty(false)]
    ///     public Brush someBrush;
    /// }
    /// ]]></code>
    /// </example>
    public sealed class BrushPropertyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrushPropertyAttribute"/> class.
        /// </summary>
        /// <param name="allowAlias">Indicates whether alias brushes can be selected.</param>
        /// <param name="allowMaster">Indicates whether master brushes can be selected.</param>
        public BrushPropertyAttribute(bool allowAlias = true, bool allowMaster = true)
        {
            this.AllowAlias = allowAlias;
            this.AllowMaster = allowMaster;
        }


        /// <summary>
        /// Gets a value indicating whether alias brushes can be selected.
        /// </summary>
        public bool AllowAlias { get; private set; }
        /// <summary>
        /// Gets a value indicating whether master brushes can be selected.
        /// </summary>
        public bool AllowMaster { get; private set; }
    }
}
