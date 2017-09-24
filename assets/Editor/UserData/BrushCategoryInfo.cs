// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile.Editor
{
    [Serializable]
    internal sealed class BrushCategoryInfo
    {
        [SerializeField, FormerlySerializedAs("_id")]
        private int id;
        [SerializeField, FormerlySerializedAs("_label")]
        private string label = "";


        internal BrushCategoryInfo()
        {
        }

        internal BrushCategoryInfo(int id)
        {
            this.id = id;
        }

        internal BrushCategoryInfo(int id, string label)
        {
            this.id = id;
            this.label = label ?? "";
        }


        public int Id {
            get { return this.id; }
            internal set { this.id = value; }
        }

        public string Label {
            get { return this.label; }
            internal set { this.label = value; }
        }
    }
}
