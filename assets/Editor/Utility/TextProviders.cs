// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// A delegate which extracts text from the given context object. This is useful when
    /// text is expensive to generate (for example, CPU performance, memory allocations, etc)
    /// where text is often not needed.
    /// </summary>
    /// <example>
    /// <para>Hover tip controls use this delegate to defer generation of string until tooltip
    /// is actually shown on-screen. This avoids wastefully allocating strings for every
    /// control for each GUI event:</para>
    /// <code language="csharp"><![CDATA[
    /// int hoverControlID = RotorzEditorGUI.GetHoverControlID(
    ///     position    : position,
    ///     tipProvider : TextProviders.FromObjectName,
    ///     tipContext  : someObject
    /// );
    /// ]]></code>
    /// </example>
    /// <param name="context">Some context object.</param>
    /// <returns>
    /// This method may return a string instance or a value of <c>null</c>.
    /// </returns>
    /// <seealso cref="TextProviders"/>
    internal delegate string TextProvider(object context);


    /// <summary>
    /// Some standard text provider implementations.
    /// </summary>
    internal static class TextProviders
    {
        /// <summary>
        /// Text provider which simply returns context object as a string.
        /// </summary>
        public static readonly TextProvider FromString = (context) => {
            return context as string;
        };

        /// <summary>
        /// Text provider which returns name of the specified <c>UnityEngine.Object</c> instance.
        /// </summary>
        public static readonly TextProvider FromObjectName = (context) => {
            var obj = context as Object;
            return obj != null ? obj.name : null;
        };

        /// <summary>
        /// Text provider which returns asset path the specified <c>UnityEngine.Object</c> instance.
        /// </summary>
        public static readonly TextProvider FromAssetPath = (context) => {
            var asset = context as Object;
            return asset != null ? AssetDatabase.GetAssetPath(asset) : null;
        };

        /// <summary>
        /// Text provider which returns display name of specified <see cref="BrushAssetRecord"/> instance.
        /// </summary>
        public static readonly TextProvider FromBrushAssetRecordDisplayName = (context) => {
            var record = context as BrushAssetRecord;
            return record != null ? record.DisplayName : null;
        };
    }
}
