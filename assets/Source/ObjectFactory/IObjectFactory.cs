// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Interface for an object creator which can be implemented and utilised to override
    /// the way in which game objects are created and destroyed.
    /// </summary>
    /// <remarks>
    /// <para>You could implement a custom object factory to take control of the way
    /// objects are instantiated and destroyed. This might be useful in situations where
    /// you would like to implement some sort of pooling to recycle discarded objects
    /// instead of destroying them.</para>
    /// <para><a href="https://bitbucket.org/rotorz/rtspoolmanagerobjectfactory/overview">RtsPoolManagerObjectFactory</a>
    /// is an open-source script which adds support for the <a href="http://u3d.as/content/path-o-logical-games-llc/pool-manager">PoolManager</a>
    /// asset by Path-o-logical Games.</para>
    /// <para>These interfaces are useful for providing custom implementations at runtime,
    /// but it is also possible to customize the way in which objects are created and
    /// destroyed in the editor, though generally for different reasons. For example,
    /// implementing a custom object factory for the editor is a useful hook into the tile
    /// painting process.</para>
    /// <para>Custom object factories can be activated:</para>
    /// <list type="bullet">
    ///    <item><b>Runtime:</b> Assign instance to the <see cref="DefaultRuntimeObjectFactory.Current">DefaultRuntimeObjectFactory.Current</see> property.</item>
    ///    <item><b>Editor:</b> Assign instance to the <see cref="P:Rotorz.Tile.Editor.DefaultEditorObjectFactory.Current">DefaultEditorObjectFactory.Current</see> property.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para>The default runtime object factory is implemented as follows:</para>
    /// <code language="csharp"><![CDATA[
    /// using Rotorz.Tile;
    /// using UnityEngine;
    ///
    /// public sealed class DefaultRuntimeObjectFactory : IObjectFactory
    /// {
    ///     public GameObject InstantiatePrefab(GameObject prefab, IObjectFactoryContext context)
    ///     {
    ///         return Object.Instantiate(prefab) as GameObject;
    ///     }
    ///
    ///     public void DestroyObject(GameObject go, IObjectFactoryContext context)
    ///     {
    ///         Object.Destroy(go);
    ///     }
    /// }
    /// ]]></code>
    /// <para>The default design-time object factory is implemented as follows:</para>
    /// <code language="csharp"><![CDATA[
    /// using Rotorz.Tile;
    /// using UnityEngine;
    ///
    /// public sealed class DefaultEditorObjectFactory : IObjectFactory
    /// {
    ///     public GameObject InstantiatePrefab(GameObject prefab, IObjectFactoryContext context)
    ///     {
    ///         return PrefabUtility.InstantiatePrefab(prefab) as GameObject;
    ///     }
    ///
    ///     public void DestroyObject(GameObject go, IObjectFactoryContext context)
    ///     {
    ///         Object.DestroyImmediate(go);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    /// <seealso cref="DefaultRuntimeObjectFactory.Current">DefaultRuntimeObjectFactory.Current</seealso>
    /// <seealso cref="P:Rotorz.Tile.Editor.DefaultEditorObjectFactory.Current">DefaultEditorObjectFactory.Current</seealso>
    public interface IObjectFactory
    {
        /// <summary>
        /// Create object instance from prefab.
        /// </summary>
        /// <remarks>
        /// <para><b>Important:</b> The instantiated object must <b>NOT</b> have a parent
        /// object.</para>
        /// </remarks>
        /// <param name="prefab">The prefab.</param>
        /// <param name="context">An object describing the context of the prefab that is
        /// being instantiated. The context object is always specified, though contained
        /// members may be <c>null</c>.</param>
        /// <returns>
        /// Object instance or <c>null</c> if object could not be created.
        /// </returns>
        GameObject InstantiatePrefab(GameObject prefab, IObjectFactoryContext context);

        /// <summary>
        /// Destroy object instance.
        /// </summary>
        /// <remarks>
        /// <para>Object must be un-parented when recycling it:</para>
        /// <code language="csharp"><![CDATA[
        /// go.transform.SetParent(null, false);
        /// ]]></code>
        /// </remarks>
        /// <param name="go">The unwanted object.</param>
        /// <param name="context">An object describing the context of the game object
        /// that is being destroyed. The context object is always specified, though
        /// contained members may be <c>null</c>.</param>
        void DestroyObject(GameObject go, IObjectFactoryContext context);
    }
}
