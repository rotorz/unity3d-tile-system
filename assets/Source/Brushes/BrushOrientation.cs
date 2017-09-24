// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Rotorz.Tile
{
    /// <summary>
    /// Describes one orientation of an oriented brush.
    /// </summary>
    /// <remarks>
    /// <para>Each orientation can reference one or more variations which can be used
    /// when painting tiles. Variations can be references to prefabs or nestable brushes.</para>
    /// </remarks>
    /// <seealso cref="OrientationUtility"/>
    [Serializable]
    public sealed class BrushOrientation
    {
        /// <summary>
        /// Default randomization weighting to assume when creating new variations if
        /// no weight has been explicitly specified.
        /// </summary>
        public const int DefaultVariationWeight = 50;


        [SerializeField]
        private int mask;

        [SerializeField, FormerlySerializedAs("_type")]
        internal bool type;
        [SerializeField, FormerlySerializedAs("_rotation")]
        internal int rotation;


        /// <summary>
        /// Gets bitmask representation of orientation.
        /// </summary>
        /// <remarks>
        /// <para>Each 3x3 orientation is represented using 8 bits rather than 9 since
        /// the middle bit is always 1.</para>
        /// </remarks>
        /// <example>
        /// <para>The following example aims to visualise the bit mask representation of
        /// an orientation:</para>
        /// <code language="csharp"><![CDATA[
        /// bool[,] orientation = {
        ///     { (mask & 1<<0) != 0, (mask & 1<<1) != 0, (mask & 1<<2) != 0 },
        ///     { (mask & 1<<3) != 0, true              , (mask & 1<<4) != 0 },
        ///     { (mask & 1<<5) != 0, (mask & 1<<6) != 0, (mask & 1<<7) != 0 },
        /// };
        /// ]]></code>
        /// </example>
        public int Mask {
            get { return this.mask; }
        }

        /// <summary>
        /// Gets a value indicating whether orientation is linked to other orientations
        /// with rotation symmetry.
        /// </summary>
        public bool HasRotationalSymmetry {
            get { return this.type; }
        }

        /// <summary>
        /// Gets index of rotation of orientation.
        /// </summary>
        /// <value>
        /// <para>Zero-based index of simple rotation (0 to 3 inclusive):</para>
        /// <list type="bullet">
        /// <item>0 = 0°</item>
        /// <item>1 = 90°</item>
        /// <item>2 = 180°</item>
        /// <item>3 = 270°</item>
        /// </list>
        /// </value>
        public int Rotation {
            get { return this.rotation; }
        }


        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushOrientation"/> class.
        /// </summary>
        public BrushOrientation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushOrientation"/> class.
        /// </summary>
        /// <param name="mask">Bitmask representation of orientation.</param>
        public BrushOrientation(int mask)
        {
            this.mask = mask;
        }

        #endregion


        #region Variations

        /// <summary>
        /// Array of variation prefabs and/or nestable brushes for orientation.
        /// </summary>
        /// <exclude/>
        [SerializeField, FormerlySerializedAs("_variations")]
        private Object[] variations = { };


        /// <summary>
        /// Gets count of variations within orientation.
        /// </summary>
        public int VariationCount {
            get { return this.variations.Length; }
        }


        /// <summary>
        /// Array of weight values which provide control over selection of variations
        /// when painting with "Randomize Variations".
        /// </summary>
        [SerializeField, FormerlySerializedAs("_variationWeights")]
        private int[] variationWeights = { };


        /// <summary>
        /// Gets array of weights which provide control over variation selection
        /// when painting tiles with randomly chosen variations.
        /// </summary>
        /// <remarks>
        /// <para>Array contains one entry per variation.</para>
        /// </remarks>
        private int[] VariationWeights {
            get {
                if (this.variationWeights.Length != this.variations.Length) {
                    int[] weights = new int[this.variations.Length];

                    // Copy values from previous array.
                    int i = 0, count = Mathf.Min(weights.Length, this.variationWeights.Length);
                    for (; i < count; ++i) {
                        weights[i] = this.variationWeights[i];
                    }

                    // Assume defaults for new values.
                    for (; i < weights.Length; ++i) {
                        weights[i] = DefaultVariationWeight;
                    }

                    this.variationWeights = weights;
                }
                return this.variationWeights;
            }
        }

        /// <summary>
        /// Get specific variation from orientation.
        /// </summary>
        /// <param name="index">Zero-based index of variation.</param>
        /// <returns>
        /// Reference to variation; or a value of <c>null</c> if associated asset is missing
        /// (for instance, if variation prefab or asset has been deleted by user).
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If specified index is out of range. You can verify whether an index is
        /// within range prior to invoking this method by checking <see cref="VariationCount"/>.
        /// </exception>
        public Object GetVariation(int index)
        {
            return this.variations[index];
        }

        /// <summary>
        /// Update specific veriation within orientation.
        /// </summary>
        /// <param name="index">Zero-based index of variation.</param>
        /// <param name="variation">The variation.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If specified index is out of range. You can verify whether an index is within
        /// range prior to invoking this method by checking <see cref="VariationCount"/>.
        /// </exception>
        public void SetVariation(int index, Object variation)
        {
            this.variations[index] = this.FilterVariation(variation);
        }

        /// <summary>
        /// Pre-filter variation instance and correct if possible.
        /// </summary>
        /// <remarks>
        /// <para>Returns reference to game object if component (for instance <c>Transform</c>)
        /// is incorrectly specified.</para>
        /// </remarks>
        /// <param name="variation">The variation.</param>
        /// <returns>
        /// The corrected variation reference.
        /// </returns>
        private Object FilterVariation(Object variation)
        {
            // If component is specified, assume associated game object instead!
            var component = variation as Component;
            if (component != null) {
                variation = component.gameObject;
            }

            return variation;
        }

        /// <summary>
        /// Find index of variation within orientation.
        /// </summary>
        /// <param name="variation">The variation.</param>
        /// <returns>
        /// Zero-based index of variation when found; otherwise a value of -1 indicating
        /// that the specified variation was not found.
        /// </returns>
        public int IndexOfVariation(Object variation)
        {
            variation = this.FilterVariation(variation);
            return Array.IndexOf(this.variations, variation);
        }

        /// <summary>
        /// Insert variation with custom weight at specific position within orientation.
        /// </summary>
        /// <remarks>
        /// <para>Tile prefabs and nestable brushes can be added as variations of an
        /// orientation. These can be randomly selected when painting tiles or manually
        /// selected by cycling through using the provided tools or API.</para>
        /// <para>Types of brush which can be nested:</para>
        /// <list type="bullet">
        ///    <item><see cref="EmptyBrush"/></item>
        ///    <item><see cref="TilesetBrush"/></item>
        /// </list>
        /// <para>Since oriented brushes cannot be nested within one another it is
        /// important to avoid adding them. You can determine whether a brush automatically
        /// orientates tiles by checking its property <see cref="Brush.PerformsAutomaticOrientation"/>:</para>
        /// <code language="csharp"><![CDATA[
        /// if (brush.PerformsAutomaticOrientation) {
        ///     throw new InvalidOperationException("Cannot nest oriented brushes.");
        /// }
        /// orientation.AddVariation(brush);
        /// ]]></code>
        /// <para>It is recommended to invoke <see cref="OrientedBrush.SyncGroupedVariations(int)"/>
        /// whenever inserting or removing variations so that variations are properly
        /// synchronized across grouped orientations. This is especially important when
        /// modifying variations for orientations that have rotational symmetry.</para>
        /// </remarks>
        /// <example>
        /// <para>Insert variation at start of orientation with a weight of 70 and synchronize
        /// with any grouped orientations:</para>
        /// <code language="csharp"><![CDATA[
        /// int mask = OrientationUtility.MaskFromName("00011111");
        /// var orientation = orientedBrush.FindOrientation(mask);
        ///
        /// // Add variation if distinct.
        /// if (orientation.IndexOfVariation(cubePrefab) == -1) {
        ///     orientation.InsertVariation(0, cubePrefab, 70);
        ///     orientedBrush.SyncGroupedVariations(mask);
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="index">Zero-based index of variation.</param>
        /// <param name="variation">Prefab root or nestable brush asset.</param>
        /// <param name="weight">Weight of variation for randomization. Value is automatically
        /// clamped within the range 0 to 100 (inclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If specified index is out of range. You can verify whether an index is within
        /// range prior to invoking this method by checking <see cref="VariationCount"/>.
        /// </exception>
        /// <seealso cref="AddVariation(Object)"/>
        /// <seealso cref="AddVariation(Object, int)"/>
        /// <seealso cref="InsertVariation(int, Object)"/>
        /// <seealso cref="InsertVariation(int, Object, int)"/>
        public void InsertVariation(int index, Object variation, int weight)
        {
            variation = this.FilterVariation(variation);

            var newVariations = new List<Object>(this.variations);
            newVariations.Insert(index, variation);
            var newWeights = new List<int>(this.VariationWeights);
            newWeights.Insert(index, weight);

            this.variations = newVariations.ToArray();
            this.variationWeights = newWeights.ToArray();
        }

        /// <summary>
        /// Insert variation at specific position within orientation.
        /// </summary>
        /// <example>
        /// <para>Insert variation at start of orientation and synchronize with any
        /// grouped orientations:</para>
        /// <code language="csharp"><![CDATA[
        /// int mask = OrientationUtility.MaskFromName("00011111");
        /// var orientation = orientedBrush.FindOrientation(mask);
        ///
        /// // Add variation if distinct.
        /// if (orientation.IndexOfVariation(cubePrefab) == -1) {
        ///     orientation.InsertVariation(0, cubePrefab);
        ///     orientedBrush.SyncGroupedVariations(mask);
        /// }
        /// ]]></code>
        /// </example>
        /// <inheritdoc cref="InsertVariation(int, Object, int)"/>
        public void InsertVariation(int index, Object variation)
        {
            this.InsertVariation(index, variation, DefaultVariationWeight);
        }

        /// <summary>
        /// Add variation with custom weight to orientation.
        /// </summary>
        /// <example>
        /// <para>Add variation with a custom weight of 70 to orientation and synchronize
        /// with any grouped orientations:</para>
        /// <code language="csharp"><![CDATA[
        /// int mask = OrientationUtility.MaskFromName("00011111");
        /// var orientation = orientedBrush.FindOrientation(mask);
        ///
        /// // Add variation if distinct.
        /// if (orientation.IndexOfVariation(cubePrefab) == -1) {
        ///     orientation.AddVariation(cubePrefab, 70);
        ///     orientedBrush.SyncGroupedVariations(mask);
        /// }
        /// ]]></code>
        /// </example>
        /// <inheritdoc cref="InsertVariation(int, Object, int)"/>
        public void AddVariation(Object variation, int weight)
        {
            this.InsertVariation(this.variations.Length, variation, weight);
        }

        /// <summary>
        /// Add variation to orientation.
        /// </summary>
        /// <example>
        /// <para>Add variation to orientation and synchronize with any grouped orientations:</para>
        /// <code language="csharp"><![CDATA[
        /// int mask = OrientationUtility.MaskFromName("00011111");
        /// var orientation = orientedBrush.FindOrientation(mask);
        ///
        /// // Add variation if distinct.
        /// if (orientation.IndexOfVariation(cubePrefab) == -1) {
        ///     orientation.AddVariation(cubePrefab);
        ///     orientedBrush.SyncGroupedVariations(mask);
        /// }
        /// ]]></code>
        /// </example>
        /// <inheritdoc cref="InsertVariation(int, Object, int)"/>
        public void AddVariation(Object variation)
        {
            this.InsertVariation(this.variations.Length, variation, DefaultVariationWeight);
        }

        /// <summary>
        /// Remove specific variation from orientation.
        /// </summary>
        /// <remarks>
        /// <para>It is recommended to invoke <see cref="OrientedBrush.SyncGroupedVariations(int)"/>
        /// whenever inserting or removing variations so that variations are properly
        /// synchronized across grouped orientations. This is especially important when
        /// modifying variations for orientations that have rotational symmetry.</para>
        /// </remarks>
        /// <example>
        /// <para>Remove first variation and synchronize with any grouped orientations:</para>
        /// <code language="csharp"><![CDATA[
        /// int mask = OrientationUtility.MaskFromName("00011111");
        /// var orientation = orientedBrush.FindOrientation(mask);
        ///
        /// if (orientation.VariationCount > 0) {
        ///     orientation.RemoveVariation(0);
        ///     orientedBrush.SyncGroupedVariations(mask);
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="index">Zero-based index of variation.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If specified index is out of range. You can verify whether an index is within
        /// range prior to invoking this method by checking <see cref="VariationCount"/>.
        /// </exception>
        public void RemoveVariation(int index)
        {
            var newVariations = new List<Object>(this.variations);
            newVariations.RemoveAt(index);
            var newWeights = new List<int>(this.VariationWeights);
            newWeights.RemoveAt(index);

            this.variations = newVariations.ToArray();
            this.variationWeights = newWeights.ToArray();
        }

        /// <summary>
        /// Remove all variations from orientation.
        /// </summary>
        /// <example>
        /// <para>Remove all variations and synchronize with any grouped orientations:</para>
        /// <code language="csharp"><![CDATA[
        /// int mask = OrientationUtility.MaskFromName("00011111");
        /// var orientation = orientedBrush.FindOrientation(mask);
        ///
        /// orientation.RemoveAllVariations();
        /// orientedBrush.SyncGroupedVariations(mask);
        /// ]]></code>
        /// </example>
        public void RemoveAllVariations()
        {
            if (this.VariationCount > 0) {
                this.variations = new Object[0];
                this.variationWeights = new int[0];
            }
        }

        /// <summary>
        /// Gets weight of the specified variation.
        /// </summary>
        /// <param name="index">Zero-based index of variation.</param>
        /// <returns>
        /// Weight of variation for randomization (0 to 100 inclusive) where a value of
        /// zero indicates that variation should be avoided if possible.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If specified index is out of range. You can verify whether an index is within
        /// range prior to invoking this method by checking <see cref="VariationCount"/>.
        /// </exception>
        /// <seealso cref="SetVariationWeight(int, int)"/>
        public int GetVariationWeight(int index)
        {
            return this.VariationWeights[index];
        }

        /// <summary>
        /// Set weight of the specified variation.
        /// </summary>
        /// <remarks>
        /// <para>Specify a weight of zero to avoid selection when variations are being
        /// randomized. A zero-weight variation will still be selected if there are no
        /// variations with a greater weight value.</para>
        /// </remarks>
        /// <param name="index">Zero-based index of variation.</param>
        /// <param name="weight">Weight of variation for randomization. Value is automatically
        /// clamped within the range 0 to 100 (inclusive).</param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If specified index is out of range. You can verify whether an index is within
        /// range prior to invoking this method by checking <see cref="VariationCount"/>.
        /// </exception>
        /// <seealso cref="GetVariationWeight(int)"/>
        public void SetVariationWeight(int index, int weight)
        {
            this.VariationWeights[index] = Mathf.Clamp(weight, 0, 100);
        }

        /// <summary>
        /// Synchronize variations from another orientation.
        /// </summary>
        /// <remarks>
        /// <para>Does nothing when attempting to synchronize variations from self.</para>
        /// </remarks>
        /// <param name="other">The other orientation.</param>
        /// <seealso cref="OrientedBrush.SyncGroupedVariations(int)"/>
        public void SyncVariationsFrom(BrushOrientation other)
        {
            if (other == this) {
                return;
            }

            this.variations = (Object[])other.variations.Clone();
            this.variationWeights = (int[])other.VariationWeights.Clone();
        }

        #endregion


        #region Randomly Picking Variations

        /// <summary>
        /// Pick random variation from orientation.
        /// </summary>
        /// <returns>
        /// Zero-based index of variation in sequence.
        /// </returns>
        public int PickRandomVariationIndex()
        {
            int[] weights = this.VariationWeights;

            // Find sum of all weights.
            int sumOfWeights = 0;
            foreach (int weight in weights) {
                sumOfWeights += weight;
            }

            // Now pick a random one!
            int random = Random.Range(0, sumOfWeights);
            int csum = 0;

            // If possible, avoid falling back to a variation which has a weight of 0.
            int fallbackIndex = 0;

            for (int i = 0; i < weights.Length; ++i) {
                int weight = weights[i];
                if (weight < 1) {
                    continue;
                }

                fallbackIndex = i;

                csum += weight;
                if (csum > random) {
                    return i;
                }
            }

            return fallbackIndex;
        }

        #endregion


        /// <summary>
        /// Gets orientation masks of grouped orientations. Resulting array always
        /// includes mask of this orientation.
        /// </summary>
        /// <example>
        /// <para>Retrieve array of orientation masks:</para>
        /// <code language="csharp"><![CDATA[
        /// // Add all four corner orientations using rotational symmetry.
        /// int cornerMask = OrientationUtility.MaskFromName("11010000");
        /// var orientation = orientedBrush.AddOrientation(cornerMask, true);
        ///
        /// // Get list of grouped orientation masks:
        /// int[] groupedMasks = orientation.GetGroupedOrientationMasks();
        ///
        /// // Resulting Array:
        /// // [0] = 11010000
        /// // [1] = 01101000
        /// // [2] = 00010110
        /// // [3] = 00001011
        /// ]]></code>
        ///
        /// <para>This also works for orientations which have not been grouped by
        /// rotational symmetry:</para>
        /// <code language="csharp"><![CDATA[
        /// // Add one corner orientation.
        /// int cornerMask = OrientationUtility.MaskFromName("11010000");
        /// var orientation = orientedBrush.AddOrientation(cornerMask);
        ///
        /// // Get list of grouped orientation masks:
        /// int[] groupedMasks = orientation.GetGroupedOrientationMasks();
        ///
        /// // Resulting Array:
        /// // [0] = 11010000
        /// ]]></code>
        /// </example>
        /// <returns>
        /// An array of orientation bitmasks.
        /// </returns>
        public int[] GetGroupedOrientationMasks()
        {
            return this.HasRotationalSymmetry
                ? OrientationUtility.GetMasksWithRotationalSymmetry(this.mask)
                : new int[] { this.mask }
                ;
        }
    }
}
