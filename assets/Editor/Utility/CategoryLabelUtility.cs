// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Text.RegularExpressions;

namespace Rotorz.Tile.Editor
{
    /// <exclude/>
    internal static class CategoryLabelUtility
    {
        public static string SanitizeCategoryLabel(string categoryLabel)
        {
            // Trim trailing slashes.
            categoryLabel = categoryLabel.Trim('/');

            // Ignore empty sub-categories.
            categoryLabel = Regex.Replace(categoryLabel, @"/+", "/");

            // Trim whitespace from sub-categories.
            var parts = categoryLabel.Split('/');
            for (int i = 0; i < parts.Length; ++i) {
                parts[i] = parts[i].Trim();
            }
            categoryLabel = string.Join("/", parts);

            return categoryLabel;
        }
    }
}
