// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Rotorz.Settings
{
    internal static class PathUtility
    {
        private static readonly Regex s_InvalidPathCharacter = new Regex("[" + Regex.Escape(Path.GetInvalidPathChars().ToString()) + "]");
        private static readonly string s_DirectorySeparator = Path.DirectorySeparatorChar.ToString();


        private static bool ValidatePath(string fileName)
        {
            return !s_InvalidPathCharacter.IsMatch(fileName);
        }


        public static string Combine(params string[] paths)
        {
            foreach (var path in paths) {
                if (string.IsNullOrEmpty(path)) {
                    throw new ArgumentException("Argument is null or is an empty string.");
                }
                if (!ValidatePath(path)) {
                    throw new ArgumentException("Argument contains one or more characters which are not permitted in valid paths.");
                }
            }
            return string.Join(s_DirectorySeparator, paths);
        }
    }
}
