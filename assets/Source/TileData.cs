// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rotorz.Tile
{
    /// <summary>
    /// Data that describes a painted tile.
    /// </summary>
    [Serializable]
    public sealed class TileData
    {
        /// <summary>
        /// Indicates tile is empty.
        /// </summary>
        private const int FLAG_NOT_EMPTY = 1 << 16;
        /// <summary>
        /// Indicates tile is linked to a game object.
        /// </summary>
        private const int FLAG_GO_LINK = 1 << 17;
        /// <summary>
        /// Indicates tile must be force refreshed upon completing bulk edit.
        /// </summary>
        private const int FLAG_DIRTY = 1 << 18;
        /// <summary>
        /// Indicates tile is solid.
        /// </summary>
        internal const int FLAG_SOLID = 1 << 19;
        /// <summary>
        /// Indicates tile is generated procedurally.
        /// </summary>
        private const int FLAG_PROCEDURAL = 1 << 20;

        /// <summary>
        /// 2-bit value to identify actual rotation of tile.
        /// </summary>
        private const int FLAG_VALUE_ROTATION = (1 << 21) | (1 << 22);
        /// <summary>
        /// 2-bit value to identify painted rotation of tile.
        /// </summary>
        private const int FLAG_VALUE_PAINTED_ROTATION = (1 << 23) | (1 << 24);

        private const int COMPARE_FLAG_MASK = ~(FLAG_NOT_EMPTY | FLAG_GO_LINK | FLAG_DIRTY);


        /// <summary>
        /// Mask of flags for internal use
        /// </summary>
        [SerializeField, FormerlySerializedAs("_flags")]
        internal int flags;

        /// <summary>
        /// The game object that occupies tile.
        /// </summary>
        /// <remarks>
        /// <para>Field will contain <c>null</c> when no game object is associated
        /// with tile. Here are some scenarios:</para>
        /// <list type="bullet">
        ///   <item>Tile was painted using a tileset brush that has no associated game
        ///   object. There is no need to create game objects when no components are
        ///   needed.</item>
        ///   <item>Game object has been destroyed (either at runtime or in editor)</item>
        /// </list>
        /// </remarks>
        public GameObject gameObject;

        /// <summary>
        /// Bit representation of tile orientation.
        /// </summary>
        /// <remarks>
        /// <para>Defaults to 000-00-000 for tiles painted using brushes that do not
        /// support orientations.</para>
        /// </remarks>
        public byte orientationMask;
        /// <summary>
        /// Zero-based index of the tile variation.
        /// </summary>
        public byte variationIndex;

        /// <summary>
        /// Zero-based index of tile in tileset.
        /// </summary>
        /// <remarks>
        /// <para>A value of -1 if tile was not painted using a tileset brush.</para>
        /// </remarks>
        public int tilesetIndex = -1;

        /// <summary>
        /// The associated tileset asset.
        /// </summary>
        /// <remarks>
        /// <para>A value of <c>null</c> if no tileset is associated with tile.</para>
        /// </remarks>
        public Tileset tileset;
        /// <summary>
        /// Brush that was used to paint tile.
        /// </summary>
        public Brush brush;


        /// <summary>
        /// Gets or sets a value indicating whether tile is empty.
        /// </summary>
        /// <remarks>
        /// <para>Used internally to workaround serialization restrictions that are
        /// imposed by Unity. Unity replaces <c>null</c> references with empty objects,
        /// this value can be used to determine whether the tile is populated.</para>
        /// <para><see cref="TileSystem.GetTile(int, int)"/> will return <c>null</c>
        /// if tile is marked as empty.</para>
        /// <para>This property may become deprecated in the future if Unity provides
        /// control over the way objects are serialized.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if empty; otherwise <c>false</c>.
        /// </value>
        public bool Empty {
            get { return this.flags == 0; }
            set {
                this.flags = value
                    ? (this.flags & ~FLAG_NOT_EMPTY)
                    : (this.flags | FLAG_NOT_EMPTY)
                    ;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="TileData"/> was linked to a game object.
        /// </summary>
        /// <value>
        /// A value of <c>true</c> if has game object; otherwise <c>false</c>.
        /// </value>
        public bool HasGameObject {
            get { return (this.flags & FLAG_GO_LINK) != 0; }
            internal set {
                this.flags = value
                    ? (this.flags | FLAG_GO_LINK)
                    : (this.flags & ~FLAG_GO_LINK)
                    ;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TileData"/> is dirty.
        /// </summary>
        /// <remarks>
        /// <para>Tile can be marked as dirty so that it can be force refreshed later.
        /// For example, tiles that are painted during a bulk edit are marked as dirty so
        /// that they can be generated later.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if dirty; otherwise <c>false</c>.
        /// </value>
        /// <seealso cref="TileSystem.BeginBulkEdit"/>
        /// <seealso cref="TileSystem.EndBulkEdit"/>
        public bool Dirty {
            get { return (this.flags & FLAG_DIRTY) != 0; }
            set {
                this.flags = value
                    ? (this.flags | FLAG_DIRTY)
                    : (this.flags & ~FLAG_DIRTY)
                    ;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TileData"/> is
        /// generated procedurally.
        /// </summary>
        public bool Procedural {
            get { return (this.flags & FLAG_PROCEDURAL) != 0; }
            set {
                this.flags = value
                    ? (this.flags | FLAG_PROCEDURAL)
                    : (this.flags & ~FLAG_PROCEDURAL)
                    ;
            }
        }

        /// <summary>
        /// Gets or sets rotation which was used to paint tile. This will typically be zero.
        /// </summary>
        /// <value>
        /// <para>Zero-based index of simple rotation (0 to 3 inclusive):</para>
        /// <list type="bullet">
        /// <item>0 = 0�</item>
        /// <item>1 = 90�</item>
        /// <item>2 = 180�</item>
        /// <item>3 = 270�</item>
        /// </list>
        /// </value>
        /// <seealso cref="Rotation"/>
        public int PaintedRotation {
            get { return (this.flags & FLAG_VALUE_PAINTED_ROTATION) >> 23; }
            set {
                this.flags = (this.flags & ~FLAG_VALUE_PAINTED_ROTATION) | (value << 23);
            }
        }

        /// <summary>
        /// Gets or sets actual rotation of tile. The value of this property will differ from
        /// <see cref="PaintedRotation"/> if additional rotation occurred.
        /// </summary>
        /// <remarks>
        /// <para>Values are mapped as follows:</para>
        /// <list type="bullet">
        /// <item>0 = 0�</item>
        /// <item>1 = 90�</item>
        /// <item>2 = 180�</item>
        /// <item>3 = 270�</item>
        /// </list>
        /// </remarks>
        /// <seealso cref="PaintedRotation"/>
        public int Rotation {
            get { return (this.flags & FLAG_VALUE_ROTATION) >> 21; }
            set {
                this.flags = (this.flags & ~FLAG_VALUE_ROTATION) | (value << 21);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TileData"/> is solid.
        /// </summary>
        /// <remarks>
        /// <para>Tile can be marked as solid to assist manual tile based collision
        /// detection or pathfinding in custom user scripts.</para>
        /// </remarks>
        /// <value>
        /// A value of <c>true</c> if solid; otherwise <c>false</c>.
        /// </value>
        /// <seealso cref="TileSystem.TileTraceSolid">TileSystem.TileTraceSolid</seealso>
        public bool SolidFlag {
            get { return (this.flags & FLAG_SOLID) != 0; }
            set {
                this.flags = value
                    ? (this.flags | FLAG_SOLID)
                    : (this.flags & ~FLAG_SOLID)
                    ;
            }
        }

        /// <summary>
        /// Gets or sets bit mask that represents up to 16 user flags.
        /// </summary>
        /// <remarks>
        /// <para>It is generally adviced to use <see cref="GetUserFlag"/> or <see cref="SetUserFlag"/>
        /// in place of this property. However, this method can be useful when implementing
        /// custom serialization logic.</para>
        /// </remarks>
        /// <example>
        ///
        /// <para>Check if flag 2 is set:</para>
        /// <code language="csharp"><![CDATA[
        /// // Check if flag 2 is set.
        /// int flags = tile.UserFlags;
        /// if ((flags & (1 << 2) != 0) {
        ///     // Flag 2 is set!
        /// }
        /// ]]></code>
        ///
        /// <para>Unset flag 3:</para>
        /// <code language="csharp"><![CDATA[
        /// int flags = tile.UserFlags;
        /// flags &= ~(1 << 3);
        /// tile.UserFlags = flags;
        /// ]]></code>
        ///
        /// <para>Set flag 4:</para>
        /// <code language="csharp"><![CDATA[
        /// int flags = tile.UserFlags;
        /// flags |= (1 << 4);
        /// tile.UserFlags = flags;
        /// ]]></code>
        ///
        /// </example>
        /// <seealso cref="GetUserFlag"/>
        /// <seealso cref="SetUserFlag"/>
        public int UserFlags {
            get { return (this.flags & 0xFFFF); }
            set {
                this.flags = (this.flags & ~0xFFFF) | (value & 0xFFFF);
            }
        }

        /// <summary>
        /// Gets a value indicating whether game object is missing for this <see cref="TileData"/>.
        /// </summary>
        /// <value>
        /// A value of <c>true</c> if game object missing; otherwise <c>false</c>.
        /// </value>
        public bool IsGameObjectMissing {
            get { return this.HasGameObject && this.gameObject == null; }
        }

        /// <summary>
        /// Gets name of the attached game object.
        /// </summary>
        /// <value>
        /// Name of game object or <c>null</c> when no game object is attached.
        /// </value>
        public string Name {
            get { return this.gameObject != null ? this.gameObject.name : null; }
        }

        /// <summary>
        /// Gets the oriented brush.
        /// </summary>
        /// <value>
        /// The oriented brush.
        /// </value>
        public OrientedBrush OrientedBrush {
            get { return this.brush as OrientedBrush; }
        }

        /// <summary>
        /// Gets the alias brush.
        /// </summary>
        /// <value>
        /// The alias brush.
        /// </value>
        public AliasBrush AliasBrush {
            get { return this.brush as AliasBrush; }
        }

        /// <summary>
        /// Gets the tileset brush.
        /// </summary>
        /// <value>
        /// The tileset brush.
        /// </value>
        public TilesetBrush TilesetBrush {
            get { return this.brush as TilesetBrush; }
        }

        /// <summary>
        /// Gets the autotile tileset brush.
        /// </summary>
        /// <value>
        /// The autotile brush.
        /// </value>
        public AutotileBrush AutotileBrush {
            get { return this.brush as AutotileBrush; }
        }


        /// <summary>
        /// Gets the state of a custom user flag.
        /// </summary>
        /// <param name="flag">Number of flag (1-16)</param>
        /// <returns>
        /// A value of <c>true</c> if flag is on; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="SetUserFlag(int, bool)"/>
        /// <seealso cref="ToggleUserFlag(int)"/>
        public bool GetUserFlag(int flag)
        {
            return (this.flags & (1 << --flag)) != 0;
        }

        /// <summary>
        /// Sets the state of a custom user flag.
        /// </summary>
        /// <param name="flag">Number of flag (1-16)</param>
        /// <param name="on">Specify <c>true</c> to set flag, or <c>false</c> to unset flag.</param>
        /// <seealso cref="GetUserFlag(int)"/>
        /// <seealso cref="ToggleUserFlag(int)"/>
        public void SetUserFlag(int flag, bool on)
        {
            if (on) {
                this.flags |= (1 << --flag);
            }
            else {
                this.flags &= ~(1 << --flag);
            }
        }

        /// <summary>
        /// Toggles the state of a custom user flag.
        /// </summary>
        /// <param name="flag">Number of flag (1-16)</param>
        /// <seealso cref="ToggleUserFlag(int)"/>
        /// <seealso cref="GetUserFlag(int)"/>
        /// <seealso cref="SetUserFlag(int, bool)"/>
        public void ToggleUserFlag(int flag)
        {
            this.SetUserFlag(flag, !this.GetUserFlag(flag));
        }

        /// <summary>
        /// Clear tile data.
        /// </summary>
        /// <remarks>
        /// <para>Should not be used to erase tiles (see <see cref="TileSystem.EraseTile(int, int)"/>).</para>
        /// </remarks>
        public void Clear()
        {
            this.flags = 0;

            this.gameObject = null;

            this.variationIndex = 0;
            this.orientationMask = 0;

            this.tileset = null;
            this.tilesetIndex = -1;

            this.brush = null;
        }

        /// <summary>
        /// Set tile data by copying from another <see cref="TileData"/> instance.
        /// </summary>
        /// <intro>
        /// <para>It is generally advisable to use <see cref="TileSystem.SetTileFrom(TileIndex, TileData)">TileSystem.SetTileFrom</see>
        /// instead since that will erase game objects which are associated with former
        /// tile data.</para>
        /// </intro>
        /// <param name="other">The other tile data instance.</param>
        /// <exception cref="System.NullReferenceException">
        /// If <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public void SetFrom(TileData other)
        {
            this.flags = other.flags;

            this.gameObject = other.gameObject;

            this.orientationMask = other.orientationMask;
            this.variationIndex = other.variationIndex;

            this.tileset = other.tileset;
            this.tilesetIndex = other.tilesetIndex;

            this.brush = other.brush;
        }

        #region Equality

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the
        /// current <see cref="TileData"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current
        /// <see cref="TileData"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="TileData"/>; otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) {
                return false;
            }

            var tile = obj as TileData;
            if (tile == null) {
                return false;
            }

            return this.brush == tile.brush
                && this.orientationMask == tile.orientationMask
                && this.variationIndex == tile.variationIndex
                && this.tilesetIndex == tile.tilesetIndex
                && this.tileset == tile.tileset
                && (this.flags & COMPARE_FLAG_MASK) == (tile.flags & COMPARE_FLAG_MASK)
                ;
        }

        /// <summary>
        /// Determines whether the specified <see cref="TileData"/> is equal to the
        /// current <see cref="TileData"/>.
        /// </summary>
        /// <param name="tile">The <see cref="TileData"/> to compare with the current
        /// <see cref="TileData"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="TileData"/> is equal to the current
        /// <see cref="TileData"/>; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(TileData tile)
        {
            if (tile == null) {
                return false;
            }

            return this.brush == tile.brush
                && this.orientationMask == tile.orientationMask
                && this.variationIndex == tile.variationIndex
                && this.tilesetIndex == tile.tilesetIndex
                && this.tileset == tile.tileset
                && (this.flags & COMPARE_FLAG_MASK) == (tile.flags & COMPARE_FLAG_MASK)
                ;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="TileData"/> object.
        /// </summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms
        /// and data structures such as a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int hash = (this.orientationMask ^ this.variationIndex) ^ this.tilesetIndex;
            if (this.brush != null) {
                hash = this.brush.GetHashCode() ^ hash;
            }
            if (this.tileset != null) {
                hash = this.tileset.GetHashCode() ^ hash;
            }
            return hash;
        }

        #endregion
    }
}
