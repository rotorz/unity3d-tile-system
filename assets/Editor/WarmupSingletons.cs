// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine.Assertions;

namespace Rotorz.Tile.Editor
{
    [InitializeOnLoad]
    internal static class WarmupSingletons
    {
        static WarmupSingletons()
        {
            Assert.IsNotNull(ProjectSettings.Instance);
            Assert.IsNotNull(ProjectSettings.Instance.BrushesFolderRelativePath);
        }
    }
}
