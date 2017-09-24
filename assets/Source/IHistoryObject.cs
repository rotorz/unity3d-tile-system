// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Represents an object which can be recorded in history.
    /// </summary>
    public interface IHistoryObject
    {
        /// <summary>
        /// Gets name for object which can be shown in selection history listings.
        /// </summary>
        /// <remarks>
        /// <para>This is typically implemented as follows:</para>
        /// <code language="csharp"><![CDATA[
        /// string IHistoryObject.HistoryName {
        ///     get { return this.DisplayName; }
        /// }
        /// ]]></code>
        /// </remarks>
        string HistoryName { get; }

        /// <summary>
        /// Gets a value of <c>true</c> if object exists or a value of <c>false</c> if
        /// object has been destroyed.
        /// </summary>
        /// <remarks>
        /// <para>This property should always be implemented as follows for objects that
        /// are based upon <c>UnityEngine.Object</c>:</para>
        /// <code language="csharp"><![CDATA[
        /// bool IHistoryObject.Exists {
        ///     get { return this != null; }
        /// }
        /// ]]></code>
        /// </remarks>
        bool Exists { get; }
    }
}
