// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;

namespace Rotorz.Tile
{
    /// <summary>
    /// Two-dimensional index of tile in a tile system.
    /// </summary>
    [Serializable]
    public struct TileIndex : IEquatable<TileIndex>
    {
        /// <summary>
        /// Index of first tile in system.
        /// </summary>
        public static readonly TileIndex zero = new TileIndex();

        /// <summary>
        /// Represents an invalid tile index.
        /// </summary>
        public static readonly TileIndex invalid = new TileIndex(-1, -1);


        #region Equality Comparer

        private static readonly IEqualityComparer<TileIndex> s_EqualityComparer = new TileIndexEqualityComparer();


        /// <summary>
        /// Gets the <see cref="IEqualityComparer{T}"/> that should be passed to generic
        /// collections of <see cref="TileIndex"/> values instead of the default <see cref="EqualityComparer"/>
        /// implementation when deploying to platforms that require AOT.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="System.ExecutionEngineException"/> exception is thrown when
        /// attempting to construct a <see cref="HashSet{T}"/> or <see cref="Dictionary{TKey, TValue}"/>
        /// with <see cref="TileIndex"/> values on iOS platforms since <see cref="EqualityComparer"/>
        /// is unable to generate its comparer at runtime.</para>
        /// </remarks>
        /// <example>
        /// <para>Example construction of a <see cref="HashSet{T}"/> instance using
        /// <see cref="TileIndex.EqualityComparer"/>:</para>
        /// <code language="csharp"><![CDATA[
        /// var set = new HashSet<TileIndex>(TileIndex.EqualityComparer);
        /// ]]></code>
        /// </example>
        public static IEqualityComparer<TileIndex> EqualityComparer {
            get { return s_EqualityComparer; }
        }

        #endregion


        /// <summary>
        /// Zero-based row index.
        /// </summary>
        public int row;
        /// <summary>
        /// Zero-based column index.
        /// </summary>
        public int column;


        /// <summary>
        /// Initializes a new instance of the <see cref="Rotorz.Tile.TileIndex"/> struct.
        /// </summary>
        /// <param name="row">Zero-based row index.</param>
        /// <param name="column">Zero-based column index.</param>
        public TileIndex(int row, int column)
        {
            this.row = row;
            this.column = column;
        }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Rotorz.Tile.TileIndex"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="Rotorz.Tile.TileIndex"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("row: {0}, column: {1}", this.row, this.column);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the
        /// current <see cref="Rotorz.Tile.TileIndex"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current
        /// <see cref="Rotorz.Tile.TileIndex"/>.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified <see cref="System.Object"/> is equal
        /// to the current <see cref="Rotorz.Tile.TileIndex"/>; otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TileIndex)) {
                return false;
            }

            TileIndex other = (TileIndex)obj;
            return this.row == other.row & this.column == other.column;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Rotorz.Tile.TileIndex"/> is equal to
        /// the current <see cref="Rotorz.Tile.TileIndex"/>.
        /// </summary>
        /// <param name="other">The <see cref="Rotorz.Tile.TileIndex"/> to compare with the
        /// current <see cref="Rotorz.Tile.TileIndex"/>.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified <see cref="Rotorz.Tile.TileIndex"/> is
        /// equal to the current <see cref="Rotorz.Tile.TileIndex"/>; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(TileIndex other)
        {
            return this.row == other.row & this.column == other.column;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Rotorz.Tile.TileIndex"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms
        /// and data structures such as a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.row ^ this.column;
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="TileIndex"/> is equal to
        /// another specified <see cref="TileIndex"/>.
        /// </summary>
        /// <param name="lhs">The first <see cref="TileIndex"/> to compare.</param>
        /// <param name="rhs">The second <see cref="TileIndex"/> to compare.</param>
        /// <returns>
        /// A value of <c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise
        /// <c>false</c>.
        /// </returns>
        public static bool operator ==(TileIndex lhs, TileIndex rhs)
        {
            return lhs.row == rhs.row & lhs.column == rhs.column;
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="TileIndex"/> is not equal
        /// to another specified <see cref="TileIndex"/>.
        /// </summary>
        /// <param name="lhs">The first <see cref="TileIndex"/> to compare.</param>
        /// <param name="rhs">The second <see cref="TileIndex"/> to compare.</param>
        /// <returns>
        /// A value of <c>true</c> if <c>left</c> and <c>right</c> are not equal;
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator !=(TileIndex lhs, TileIndex rhs)
        {
            return lhs.row != rhs.row | lhs.column != rhs.column;
        }

        /// <summary>
        /// Add row and column indices of two <see cref="TileIndex"/> arguments.
        /// </summary>
        /// <param name="lhs">Left <see cref="TileIndex"/> operand.</param>
        /// <param name="rhs">Right <see cref="TileIndex"/> operand.</param>
        /// <returns>
        /// Sum of input <see cref="TileIndex"/> operands.
        /// </returns>
        public static TileIndex operator +(TileIndex lhs, TileIndex rhs)
        {
            lhs.row += rhs.row;
            lhs.column += rhs.column;
            return lhs;
        }

        /// <summary>
        /// Subtract row and column indices of two <see cref="TileIndex"/> arguments.
        /// </summary>
        /// <param name="lhs">Left <see cref="TileIndex"/> operand.</param>
        /// <param name="rhs">Right <see cref="TileIndex"/> operand.</param>
        /// <returns>
        /// Difference between input <see cref="TileIndex"/> operands.
        /// </returns>
        public static TileIndex operator -(TileIndex lhs, TileIndex rhs)
        {
            lhs.row -= rhs.row;
            lhs.column -= rhs.column;
            return lhs;
        }

        /// <summary>
        /// Negate row and column indices of <see cref="TileIndex"/> argument.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <returns>
        /// Negated value.
        /// </returns>
        public static TileIndex operator -(TileIndex value)
        {
            value.row = -value.row;
            value.column = -value.column;
            return value;
        }

    }


    /// <summary>
    /// An explicit implementation of <see cref="IEqualityComparer{TileIndex}"/> which
    /// should be used instead of <see cref="EqualityComparer.Default"/> when deploying
    /// to a platform that requires AOT.
    /// </summary>
    internal sealed class TileIndexEqualityComparer : IEqualityComparer<TileIndex>
    {
        public bool Equals(TileIndex x, TileIndex y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(TileIndex obj)
        {
            return obj.GetHashCode();
        }
    }
}
