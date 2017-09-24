// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile
{
    /// <summary>
    /// Provides internal helper functionality.
    /// </summary>
    /// <remarks>
    /// <para>Implementation can be replaced as needed. The tile system editor switches
    /// to a different implementation for integration with Unity editor.</para>
    /// </remarks>
    public class InternalUtility
    {
        #region Singleton Pattern

        /// <summary>
        /// The instance.
        /// </summary>
        public static InternalUtility Instance = new InternalUtility();

        // Prevent instantiation!
        protected InternalUtility()
        {
        }

        #endregion


        #region Primary Interface

        /// <summary>
        /// Destroy the specified instance using <c>UnityEngine.Object.Destroy</c> when
        /// game is playing; otherwise destroys object using <c>UnityEngine.Object.DestroyImmediate</c>.
        /// </summary>
        /// <remarks>
        /// <para>This function should be to destroy game objects or components.</para>
        /// </remarks>
        /// <param name="obj">Object to destroy.</param>
        public static void Destroy(Object obj)
        {
            if (obj == null) {
                return;
            }

            if (Application.isPlaying) {
                Object.Destroy(obj);
            }
            else {
                Object.DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Destroy the specified instance immediately using <c>UnityEngine.Object.DestroyImmediate</c>
        /// in editor; otherwise destroys instance using <c>UnityEngine.Object.Destroy</c>.
        /// </summary>
        /// <remarks>
        /// <para>This function should be used to destroy generated meshes and materials.</para>
        /// </remarks>
        /// <param name="obj">Object to destroy.</param>
        public static void DestroyImmediate(Object obj)
        {
            if (obj == null) {
                return;
            }

            if (Application.isEditor) {
                Object.DestroyImmediate(obj);
            }
            else {
                Object.Destroy(obj);
            }
        }

        /// <summary>
        /// Hides wireframe of mesh of selected object when in editor.
        /// </summary>
        /// <param name="go">Game object.</param>
        public static void HideEditorWireframe(GameObject go)
        {
            Instance.HideEditorWireframeImpl(go);
        }

        /// <summary>
        /// Gets a value indicating whether empty chunks should be removed.
        /// </summary>
        public static bool ShouldEraseEmptyChunks(TileSystem system)
        {
            switch (Instance.eraseEmptyChunks) {
                default:
                case 0: // EraseEmptyChunksPreference.Yes
                    return !Application.isPlaying || system.hintEraseEmptyChunks;
                case 1: // EraseEmptyChunksPreference.No
                    return false;
                case 2: // EraseEmptyChunksPreference.PerTileSystem
                    return system.hintEraseEmptyChunks;
            }
        }

        #endregion


        #region Default Implementation

        // Always use `EraseEmptyChunkPreference.PerTileSystem` for runtime!
        public int eraseEmptyChunks = 2;

        public virtual void HideEditorWireframeImpl(GameObject go) { }

        #endregion


        #region Object Factory Resolution

        /// <summary>
        /// Resolves the current object factory used to instantiate tiles.
        /// </summary>
        /// <returns>
        /// The <see cref="IObjectFactory"/> instance.
        /// </returns>
        public virtual IObjectFactory ResolveObjectFactory()
        {
            return DefaultRuntimeObjectFactory.Current;
        }

        #endregion


        #region Progress Handlers

        /// <summary>
        /// Indicates if progress handler should be ignored.
        /// </summary>
        public static bool EnableProgressHandler = true;

        /// <summary>
        /// Clear progress feedback.
        /// </summary>
        public static void ClearProgress()
        {
            Instance.ClearProgressImpl();
        }

        /// <summary>
        /// Handle progress feedback.
        /// </summary>
        /// <param name="title">Progress title.</param>
        /// <param name="message">Progress status message.</param>
        /// <param name="progress">Percentage of progress.</param>
        public static void ProgressHandler(string title, string message, float progress)
        {
            if (EnableProgressHandler) {
                Instance.ProgressHandlerImpl(title, message, progress);
            }
        }

        /// <summary>
        /// Handle progress feedback.
        /// </summary>
        /// <param name="title">Progress title.</param>
        /// <param name="message">Progress status message.</param>
        /// <param name="progress">Percentage of progress.</param>
        /// <returns>
        /// A value of <c>true</c> to indicate that task should be cancelled; otherwise <c>false</c>.
        /// </returns>
        public static bool CancelableProgressHandler(string title, string message, float progress)
        {
            if (EnableProgressHandler) {
                return Instance.CancelableProgressHandlerImpl(title, message, progress);
            }
            return false;
        }

        protected virtual void ClearProgressImpl()
        {
        }

        protected virtual void ProgressHandlerImpl(string title, string message, float progress)
        {
        }

        protected virtual bool CancelableProgressHandlerImpl(string title, string message, float progress)
        {
            return false;
        }

        #endregion


        #region Unity Hackarounds

        // This hack is needed to workaround a bug (Case #677357) where usage of lhs == rhs
        // doesn't always work!! Fails to work upon switching from play mode to edit mode.
        // Unity closed this issue as "by design" despite the fact that the following logic
        // makes very little sense:
        //   objectReferenceA == objectReferenceB (true)
        //   objectReferenceA == null (false)
        //   objectReferenceB == null (true)
        public static bool AreSameUnityObjects(Object lhs, Object rhs)
        {
            // Lhs and Rhs are the same if Unity's overloaded == operator says so AND
            // either both are null or neither are null.
            return lhs == rhs && !(lhs == null ^ rhs == null);
        }

        #endregion
    }
}
