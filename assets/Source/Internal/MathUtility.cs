// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Runtime.InteropServices;
using UnityEngine;

namespace Rotorz.Tile.Internal
{
    /// <summary>
    /// Math utility functions.
    /// </summary>
    public static class MathUtility
    {
        public static readonly Quaternion AngleAxis_180_Up = Quaternion.AngleAxis(180f, Vector3.up);
        public static readonly Quaternion AngleAxis_90_Left = Quaternion.AngleAxis(90f, Vector3.left);

        public static readonly Matrix4x4 RotateUpAxisBy180Matrix = Matrix4x4.TRS(Vector3.zero, AngleAxis_180_Up, Vector3.one);

        /// <summary>
        /// Vector with the value (-1, -1, -1)
        /// </summary>
        public static readonly Vector3 MinusOneVector = new Vector3(-1f, -1f, -1f);

        /// <summary>
        /// Identity quaternion.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
        /// </remarks>
        public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
        /// <summary>
        /// Identity matrix.
        /// </summary>
        /// <remarks>
        /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
        /// </remarks>
        public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;


        #region Integers

        /// <summary>
        /// Calculate modulo division; this behaves as one would expect for negative
        /// values (unlike the built-in % operator).
        /// </summary>
        public static int Mod(int number, int divisor)
        {
            return (number % divisor + divisor) % divisor;
        }

        #endregion


        #region Floating Point

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public int i;
            [FieldOffset(0)]
            public float f;
        }

        /// <summary>
        /// Returns the next representable floating point number in sequence after the
        /// one specified.
        /// </summary>
        /// <param name="number">Input floating point number.</param>
        /// <returns>
        /// The next floating point number.
        /// </returns>
        public static float NextAfter(float number)
        {
            if (float.IsNaN(number)) {
                return number;
            }

            FloatIntUnion u;
            u.i = 0;
            u.f = number;  // shut up the compiler

            ++u.i;
            return u.f;
        }

        /// <summary>
        /// Returns the next representable floating point number after the one specified.
        /// </summary>
        /// <param name="number">Input floating point number.</param>
        /// <param name="n">Nth representable number after.</param>
        /// <returns>
        /// The next floating point number.
        /// </returns>
        public static float NextAfter(float number, int n)
        {
            if (float.IsNaN(number)) {
                return number;
            }

            FloatIntUnion u;
            u.i = 0;
            u.f = number;  // shut up the compiler

            u.i += n;
            return u.f;
        }

        #endregion


        #region Vector Utility

        /// <summary>
        /// Get inverse of input vector (1/x, 1/y, 1/z, 1/w).
        /// </summary>
        /// <param name="vector">The input vector.</param>
        /// <returns>
        /// The reciprocal.
        /// </returns>
        public static Vector4 Inverse(Vector4 vector)
        {
            return new Vector4 {
                x = 1f / vector.x,
                y = 1f / vector.y,
                z = 1f / vector.z,
                w = 1f / vector.w
            };
        }

        /// <summary>
        /// Get inverse of input vector (1/x, 1/y, 1/z).
        /// </summary>
        /// <param name="vector">The input vector.</param>
        /// <returns>
        /// The reciprocal.
        /// </returns>
        public static Vector3 Inverse(Vector3 vector)
        {
            return new Vector3 {
                x = 1f / vector.x,
                y = 1f / vector.y,
                z = 1f / vector.z
            };
        }

        /// <summary>
        /// Get inverse of input vector (1/x, 1/y).
        /// </summary>
        /// <param name="vector">The input vector.</param>
        /// <returns>
        /// The reciprocal.
        /// </returns>
        public static Vector2 Inverse(Vector2 vector)
        {
            return new Vector2 {
                x = 1f / vector.x,
                y = 1f / vector.y
            };
        }

        #endregion


        #region Matrix Decomposition

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        #endregion


        #region Matrix Composition

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>
        /// The translation transform matrix.
        /// </returns>
        public static Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = IdentityMatrix;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }

        #endregion


        #region Matrix Utilities

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }

        /// <summary>
        /// Form scale matrix from vector and then multiply scale matrix with input matrix
        /// (matrix = scaleMatrix * matrix).
        /// </summary>
        /// <param name="matrix">Matrix for which to apply scale.</param>
        /// <param name="scale">The scale vector.</param>
        public static void MultiplyScaleByMatrix(ref Matrix4x4 matrix, Vector3 scale)
        {
            matrix.m00 *= scale.x;
            matrix.m01 *= scale.x;
            matrix.m02 *= scale.x;
            matrix.m03 *= scale.x;
            matrix.m10 *= scale.y;
            matrix.m11 *= scale.y;
            matrix.m12 *= scale.y;
            matrix.m13 *= scale.y;
            matrix.m20 *= scale.z;
            matrix.m21 *= scale.z;
            matrix.m22 *= scale.z;
            matrix.m23 *= scale.z;
        }

        /// <summary>
        /// Form scale matrix from vector and then multiply matrix with scale matrix
        /// (matrix = matrix * scaleMatrix).
        /// </summary>
        /// <param name="matrix">Matrix for which to apply scale.</param>
        /// <param name="scale">The scale vector.</param>
        public static void MultiplyMatrixByScale(ref Matrix4x4 matrix, Vector3 scale)
        {
            matrix.m00 *= scale.x;
            matrix.m01 *= scale.y;
            matrix.m02 *= scale.z;
            matrix.m10 *= scale.x;
            matrix.m11 *= scale.y;
            matrix.m12 *= scale.z;
            matrix.m20 *= scale.x;
            matrix.m21 *= scale.y;
            matrix.m22 *= scale.z;
            matrix.m30 *= scale.x;
            matrix.m31 *= scale.y;
            matrix.m32 *= scale.z;
        }

        #endregion


        #region Tile Bounds

        /// <summary>
        /// Get minimum and maximum bounds from anchor and target tile indices.
        /// </summary>
        /// <param name="anchor">Anchor index.</param>
        /// <param name="target">Target index.</param>
        /// <param name="min">Minimum tile index.</param>
        /// <param name="max">Maximum tile index.</param>
        /// <param name="uniform">Indicates whether bounds should be uniformly sized.</param>
        public static void GetRectangleBounds(TileIndex anchor, TileIndex target, out TileIndex min, out TileIndex max, bool uniform)
        {
            if (uniform) {
                int size = Mathf.Max(Mathf.Abs(target.row - anchor.row), Mathf.Abs(target.column - anchor.column));
                if (target.row >= anchor.row) {
                    target.row = anchor.row + size;
                }
                else {
                    target.row = anchor.row - size;
                }

                if (target.column >= anchor.column) {
                    target.column = anchor.column + size;
                }
                else {
                    target.column = anchor.column - size;
                }
            }

            min.row = Mathf.Min(anchor.row, target.row);
            min.column = Mathf.Min(anchor.column, target.column);

            max.row = Mathf.Max(anchor.row, target.row);
            max.column = Mathf.Max(anchor.column, target.column);
        }

        /// <inheritdoc cref="GetRectangleBounds(TileIndex, TileIndex, out TileIndex, out TileIndex, bool)"/>
        public static void GetRectangleBounds(TileIndex anchor, TileIndex target, out TileIndex min, out TileIndex max)
        {
            GetRectangleBounds(anchor, target, out min, out max, false);
        }

        /// <summary>
        /// Get minimum and maximum bounds from anchor and target tile indices clamped within
        /// bounds of tile system.
        /// </summary>
        /// <param name="system">Tile system.</param>
        /// <param name="anchor">Anchor index.</param>
        /// <param name="target">Target index.</param>
        /// <param name="min">Minimum tile index.</param>
        /// <param name="max">Maximum tile index.</param>
        /// <param name="uniform">Indicates whether bounds should be uniformly sized.</param>
        public static void GetRectangleBoundsClamp(TileSystem system, TileIndex anchor, TileIndex target, out TileIndex min, out TileIndex max, bool uniform)
        {
            GetRectangleBounds(anchor, target, out min, out max, uniform);

            if (min.row < 0) {
                min.row = 0;
            }
            if (min.column < 0) {
                min.column = 0;
            }
            if (max.row >= system.RowCount) {
                max.row = system.RowCount - 1;
            }
            if (max.column >= system.ColumnCount) {
                max.column = system.ColumnCount - 1;
            }
        }

        /// <inheritdoc cref="GetRectangleBoundsClamp(TileSystem, TileIndex, TileIndex, out TileIndex, out TileIndex, bool)"/>
        public static void GetRectangleBoundsClamp(TileSystem system, TileIndex anchor, TileIndex target, out TileIndex min, out TileIndex max)
        {
            GetRectangleBoundsClamp(system, anchor, target, out min, out max, false);
        }

        #endregion
    }
}
