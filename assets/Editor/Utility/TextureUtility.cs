// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility functions for manipulating textures.
    /// </summary>
    internal static class TextureUtility
    {
        /// <summary>
        /// Create new texture by copying and inverting pixels from input texture.
        /// </summary>
        /// <param name="input">Input texture.</param>
        /// <returns>
        /// A new <see cref="Texture2D"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="input"/> is <c>null</c>.
        /// </exception>
        public static Texture2D Invert(Texture2D input)
        {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            Texture2D inverted = new Texture2D(input.width, input.height, input.format, false, true);
            inverted.hideFlags = HideFlags.HideAndDontSave;

            for (int ix = 0; ix < input.width; ++ix) {
                for (int iy = 0; iy < input.height; ++iy) {
                    Color c = input.GetPixel(ix, iy);
                    c.r = 1f - c.r;
                    c.g = 1f - c.g;
                    c.b = 1f - c.b;
                    inverted.SetPixel(ix, iy, c);
                }
            }
            inverted.Apply();

            return inverted;
        }

        /// <summary>
        /// Invert color of pixels of input texture.
        /// </summary>
        /// <param name="input">Input texture.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="input"/> is <c>null</c>.
        /// </exception>
        public static void InvertOriginal(Texture2D input)
        {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            for (int ix = 0; ix < input.width; ++ix) {
                for (int iy = 0; iy < input.height; ++iy) {
                    Color c = input.GetPixel(ix, iy);
                    c.r = 1f - c.r;
                    c.g = 1f - c.g;
                    c.b = 1f - c.b;
                    input.SetPixel(ix, iy, c);
                }
            }
            input.Apply();
        }
    }
}
