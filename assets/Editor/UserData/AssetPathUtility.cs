// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility functions for resolving asset and associated user data paths.
    /// </summary>
    internal static class AssetPathUtility
    {
        public static string ConvertToAssetsRelativePath(string assetPath)
        {
            if (assetPath == null) {
                throw new ArgumentNullException("assetPath");
            }
            if (!assetPath.StartsWith("Assets/")) {
                throw new ArgumentException(string.Format("Invalid asset path '{0}'.", assetPath), "assetPath");
            }

            return assetPath.Substring("Assets/".Length);
        }
    }
}
