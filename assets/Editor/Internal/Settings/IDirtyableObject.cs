// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Settings
{
    /// <summary>
    /// Interface for an object indicating whether its state is dirty and thus
    /// should be updated when synchronization next occurs.
    /// </summary>
    /// <example>
    /// <para>Here is a simple example:</para>
    /// <code language="csharp"><![CDATA[
    /// public class ExampleClass : IDirtyableObject {
    ///     private int _someValue;
    ///     private bool _dirty;
    ///
    ///     public int SomeValue {
    ///         get { return _someValue; }
    ///         set {
    ///             if (value != _someValue) {
    ///                 _someValue = value;
    ///                 _dirty = true;
    ///             }
    ///         }
    ///     }
    ///
    ///     bool IDirtyableObject.IsDirty {
    ///         get { return _dirty; }
    ///     }
    ///     void IDirtyableObject.MarkClean() {
    ///         _dirty = false;
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    internal interface IDirtyableObject
    {
        /// <summary>
        /// Gets a value indicating whether state of object is dirty.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Mark object as clean.
        /// </summary>
        /// <remarks>
        /// <para>This method should be invoked when object is next synchronized.</para>
        /// </remarks>
        void MarkClean();
    }
}
