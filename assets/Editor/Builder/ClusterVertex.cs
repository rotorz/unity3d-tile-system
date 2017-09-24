// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal struct ClusterVertex
    {
        public MeshData Data;
        public int Index;


        public ClusterVertex(MeshData data, int index)
        {
            this.Data = data;
            this.Index = index;
        }


        public Vector3 Position {
            get { return this.Data.Vertices[this.Index]; }
        }

        public Vector3 Normal {
            get { return this.Data.Normals[this.Index]; }
        }

        public Vector3 OriginalNormal {
            get { return this.Data.OriginalNormals[this.Index]; }
        }
    }
}
