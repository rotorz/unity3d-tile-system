// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace Rotorz.Tile
{
    internal struct BoxBounds
    {
        /// <summary>
        /// Gets or sets error threshold when working with bounding boxes.
        /// </summary>
        public static float ErrorThreshold { get; set; }


        static BoxBounds()
        {
            ErrorThreshold = 0.01f;
        }


        public static BoxBounds FromBounds(Bounds bounds)
        {
            return new BoxBounds(bounds.min, bounds.max);
        }

        public static BoxBounds FromBounds(Vector3 center, Vector3 size)
        {
            var point1 = new Vector3(center.x - size.x / 2f, center.y - size.y / 2f, center.z - size.z / 2f);
            var point2 = new Vector3(point1.x + size.x, point1.y + size.y, point1.z + size.z);
            return new BoxBounds(point1, point2);
        }


        private Vector3 min;
        private Vector3 max;


        public Vector3 Min {
            get { return this.min; }
        }
        public Vector3 Max {
            get { return this.max; }
        }


        public BoxBounds(Vector3 point1, Vector3 point2)
        {
            this.min = Vector3.Min(point1, point2);
            this.max = Vector3.Max(point2, point1);
        }

        //public static readonly int[,] EdgeList = {
        //    /* 0: */ { 1, 2, 3 },
        //    /* 1: */ { 0, 4, 6 },
        //    /* 2: */ { 0, 4, 5 },
        //    /* 3: */ { 0, 5, 6 },
        //    /* 4: */ { 1, 2, 7 },
        //    /* 5: */ { 2, 3, 7 },
        //    /* 6: */ { 1, 3, 7 },
        //    /* 7: */ { 4, 5, 6 },
        //};

        public Vector3 this[int index] {
            get {
                switch (index) {
                    case 0:
                        return this.min;
                    case 1:
                        return new Vector3(this.min.x, this.min.y, this.max.z);
                    case 2:
                        return new Vector3(this.min.x, this.max.y, this.min.z);
                    case 3:
                        return new Vector3(this.max.x, this.min.y, this.min.z);
                    case 4:
                        return new Vector3(this.min.x, this.max.y, this.max.z);
                    case 5:
                        return new Vector3(this.max.x, this.max.y, this.min.z);
                    case 6:
                        return new Vector3(this.max.x, this.min.y, this.max.z);
                    case 7:
                        return this.max;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public void Set(Vector3 point1, Vector3 point2)
        {
            this.min = Vector3.Min(point1, point2);
            this.max = Vector3.Max(point2, point1);
        }

        private static bool Approx(Vector3 a, Vector3 b, float threshold)
        {
            a.x = a.x > b.x ? a.x - b.x : b.x - a.x;
            a.y = a.y > b.y ? a.y - b.y : b.y - a.y;
            a.z = a.z > b.z ? a.z - b.z : b.z - a.z;
            return a.x < threshold && a.y < threshold && a.z < threshold;
        }

        public bool Encapsulate(BoxBounds other)
        {
            int matchCount = 0;

            for (int i = 0; i < 8 && matchCount != 4; ++i) {
                var p = this[i];
                for (int j = 0; j < 8; ++j) {
                    var q = other[j];
                    if (Approx(p, q, ErrorThreshold)) {
                        ++matchCount;
                        break;
                    }
                }
            }

            if (matchCount != 4) {
                return false;
            }

            this.min = Vector3.Min(this.min, other.min);
            this.max = Vector3.Max(this.max, other.max);

            return true;
        }

        public Vector3 center {
            get {
                return Vector3.Lerp(this.min, this.max, 0.5f);
            }
        }

        public Vector3 size {
            get {
                return new Vector3(this.max.x - this.min.x, this.max.y - this.min.y, this.max.z - this.min.z);
            }
        }

        public Bounds ToBounds()
        {
            return new Bounds(this.center, this.size);
        }
    }
}
