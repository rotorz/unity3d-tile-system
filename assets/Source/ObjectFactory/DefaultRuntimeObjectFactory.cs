// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// The default object factory that is used to create and destroy tile game objects at
    /// runtime.
    /// </summary>
    /// <remarks>
    /// <para>Objects are created using <c>Object.Instantiate</c> and are destroyed using
    /// <c>Object.Destroy</c>.</para>
    /// <para>A custom object factory can be created by providing a custom implementation
    /// of <see cref="IObjectFactory"/> which can then be activated by assigning an
    /// instance to <see cref="DefaultRuntimeObjectFactory.Current"/>.</para>
    /// </remarks>
    public sealed class DefaultRuntimeObjectFactory : IObjectFactory
    {
        private static DefaultRuntimeObjectFactory s_Default;

        static DefaultRuntimeObjectFactory()
        {
            s_Default = new DefaultRuntimeObjectFactory();
            s_RuntimeCurrent = s_Default;
        }


        private static IObjectFactory s_RuntimeCurrent;


        /// <summary>
        /// Gets or sets the current factory for creating and destroying tile game objects
        /// at runtime.
        /// </summary>
        /// <example>
        /// <para>Activating a custom object factory:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomObjectFactory : MonoBehaviour, IObjectFactory
        /// {
        ///     private void Awake()
        ///     {
        ///         // Activate the object factory asap!
        ///         DefaultRuntimeObjectFactory.Current = this;
        ///     }
        ///
        ///     // Implement IObjectFactory here...
        /// }
        /// ]]></code>
        /// </example>
        public static IObjectFactory Current {
            get {
                // Restore the default factory if runtime factory is missing.
                if (s_RuntimeCurrent == null) {
                    s_RuntimeCurrent = s_Default;
                }
                return s_RuntimeCurrent;
            }
            set {
                s_RuntimeCurrent = value;
            }
        }


        /// <inheritdoc/>
        public GameObject InstantiatePrefab(GameObject prefab, IObjectFactoryContext context)
        {
            return Object.Instantiate(prefab) as GameObject;
        }

        /// <inheritdoc/>
        public void DestroyObject(GameObject go, IObjectFactoryContext context)
        {
            Object.Destroy(go);
        }
    }
}
