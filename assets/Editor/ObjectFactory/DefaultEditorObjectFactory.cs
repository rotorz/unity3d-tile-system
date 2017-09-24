// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// The default object factory that is used to create and destroy tile
    /// game objects at design time when using the Unity editor.
    /// </summary>
    /// <remarks>
    /// <para>Objects are created using <c>PrefabUtility.InstantiatePrefab</c>
    /// and are destroyed using <c>Object.DestroyImmediate</c>.</para>
    /// <para>A custom implementation should also use <c>PrefabUtility.InstantiatePrefab</c>
    /// when instantiating prefabs so that instances are properly linked to
    /// their prefab counterparts. When destroying objects from editor scripts
    /// it is always necessary to use <c>Object.DestroyImmediate</c>.</para>
    /// </remarks>
    [InitializeOnLoad]
    public sealed class DefaultEditorObjectFactory : IObjectFactory
    {
        private static DefaultEditorObjectFactory s_Default;
        private static IObjectFactory s_Current;


        static DefaultEditorObjectFactory()
        {
            s_Default = new DefaultEditorObjectFactory();
        }


        /// <summary>
        /// Gets or sets the current factory for creating and destroying
        /// tile game objects in editor.
        /// </summary>
        /// <example>
        /// <para>Activating a custom editor object factory:</para>
        /// <code language="csharp"><![CDATA[
        /// [InitializeOnLoad]
        /// public class CustomEditorObjectFactory : IObjectFactory
        /// {
        ///     static CustomEditorObjectFactory()
        ///     {
        ///         // Activate the object factory asap!
        ///         DefaultEditorObjectFactory.Current = new CustomEditorObjectFactory();
        ///     }
        ///
        ///
        ///     // Implement IObjectFactory here...
        /// }
        /// ]]></code>
        /// </example>
        public static IObjectFactory Current {
            get {
                // Restore the default factory if runtime factory is missing.
                if (s_Current == null) {
                    s_Current = s_Default;
                }
                return s_Current;
            }
            set {
                s_Current = value;
            }
        }


        /// <inheritdoc/>
        public GameObject InstantiatePrefab(GameObject prefab, IObjectFactoryContext context)
        {
            return PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }

        /// <inheritdoc/>
        public void DestroyObject(GameObject go, IObjectFactoryContext context)
        {
            Object.DestroyImmediate(go);
        }
    }
}
