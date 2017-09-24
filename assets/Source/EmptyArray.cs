// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Generic repository of empty arrays.
    /// </summary>
    internal sealed class EmptyArray<T>
    {
        /// <summary>
        /// The empty array.
        /// </summary>
        public static readonly T[] Instance = new T[0];
    }
}
