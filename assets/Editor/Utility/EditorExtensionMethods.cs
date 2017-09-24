// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    internal static class EditorExtensionMethods
    {
        public static int CountSubstrings(this string str, char value)
        {
            int count = 0, index = 0;
            index = str.IndexOf(value, index);
            while (index != -1) {
                ++count;
                index = str.IndexOf(value, index + 1);
            }
            return count;
        }

        public static int CountSubstrings(this string str, string value)
        {
            int count = 0, index = 0;
            index = str.IndexOf(value, index);
            while (index != -1) {
                ++count;
                index = str.IndexOf(value, index + 1);
            }
            return count;
        }

        public static string ReplaceLast(this string str, string value, string newValue)
        {
            int lastIndex = str.LastIndexOf(value);
            return str.Substring(0, lastIndex) + newValue + str.Substring(lastIndex + value.Length);
        }
    }
}
