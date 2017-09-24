// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using System;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal enum SnapAlignment
    {
        Free,
        Points,
        Cells,
    }


    internal enum SnapGridType
    {
        Fraction,
        Custom,
    }


    internal sealed class SnapAxis : IDirtyableObject
    {
        public SnapAxis()
        {
            this.ResetToDefaultValues();
        }


        private SnapAlignment alignment;

        [SettingProperty]
        public SnapAlignment Alignment {
            get { return this.alignment; }
            set {
                if (value != this.alignment) {
                    this.alignment = value;
                    this.IsDirty = true;
                }
            }
        }

        private SnapGridType gridType;
        private int fractionDenominator;
        private float customSize;

        [SettingProperty]
        public SnapGridType GridType {
            get { return this.gridType; }
            set {
                if (value != this.gridType) {
                    this.gridType = value;
                    this.IsDirty = true;
                }
            }
        }

        [SettingProperty]
        public int FractionDenominator {
            get { return this.fractionDenominator; }
            set {
                if (value != this.fractionDenominator) {
                    this.fractionDenominator = value;
                    this.IsDirty = true;
                }
            }
        }

        [SettingProperty]
        public float CustomSize {
            get { return this.customSize; }
            set {
                if (value != this.customSize) {
                    this.customSize = value;
                    this.IsDirty = true;
                }
            }
        }

        public void ResetToDefaultValues()
        {
            this.Alignment = SnapAlignment.Points;

            this.GridType = SnapGridType.Fraction;
            this.FractionDenominator = 4;
            this.CustomSize = 0.5f;
        }

        public void SetFraction(int denominator)
        {
            this.GridType = SnapGridType.Fraction;
            this.FractionDenominator = Mathf.Max(1, denominator);
        }

        public void SetCustomSize(float size)
        {
            this.GridType = SnapGridType.Custom;
            this.CustomSize = Mathf.Max(0.0001f, size);
        }

        public void SetFrom(SnapAxis other)
        {
            if (other == null) {
                throw new ArgumentNullException("other");
            }

            this.Alignment = other.Alignment;

            this.GridType = other.GridType;
            this.FractionDenominator = other.FractionDenominator;
            this.CustomSize = other.CustomSize;
        }

        public float Resolve(float cellSize)
        {
            switch (this.GridType) {
                default:
                case SnapGridType.Fraction:
                    return cellSize * (1f / (float)this.FractionDenominator);

                case SnapGridType.Custom:
                    return this.CustomSize;
            }
        }

        public float ApplySnapping(float point, float cellSize, bool invert)
        {
            if (this.Alignment == SnapAlignment.Free) {
                return point;
            }

            float spacing = this.Resolve(cellSize);

            point = Mathf.Round(point / spacing) * spacing;

            if (this.Alignment == SnapAlignment.Cells) {
                float direction = invert ? -1f : 1f;
                point += (spacing / 2f) * direction;
            }

            return point;
        }


        #region IDirtyableObject Members

        /// <inheritdoc/>
        public bool IsDirty { get; private set; }

        /// <inheritdoc/>
        void IDirtyableObject.MarkClean()
        {
            this.IsDirty = false;
        }

        #endregion
    }
}
