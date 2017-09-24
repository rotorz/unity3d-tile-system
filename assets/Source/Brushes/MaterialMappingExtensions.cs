// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Extension methods for <see cref="IMaterialMappings"/> interface.
    /// </summary>
    public static class MaterialMappingExtensions
    {
        /// <summary>
        /// Remap specified source material to the target material.
        /// </summary>
        /// <param name="mappings">Material mappings.</param>
        /// <param name="from">Source material.</param>
        /// <returns>
        /// The remapped material; or source material when no mapping was found or no
        /// target material was specified.
        /// </returns>
        public static Material RemapMaterial(this IMaterialMappings mappings, Material from)
        {
            if (mappings.MaterialMappingFrom == null || mappings.MaterialMappingTo == null) {
                return from;
            }

            int index = Array.IndexOf(mappings.MaterialMappingFrom, from);
            return index >= 0 && index < mappings.MaterialMappingTo.Length
                ? mappings.MaterialMappingTo[index]
                : from;
        }
    }
}
