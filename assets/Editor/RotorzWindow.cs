// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Base class for custom Unity editor windows.
    /// </summary>
    public abstract class RotorzWindow : EditorWindow, IRepaintableUI, IFocusableUI, ICloseableUI
    {
        /// <summary>
        /// Indicates whether window should be centered when first shown.
        /// </summary>
        public enum CenterMode
        {
            /// <summary>
            /// Do not automatically center window.
            /// </summary>
            No = 0,
            /// <summary>
            /// Automatically center window upon first being shown.
            /// </summary>
            Once,
            /// <summary>
            /// Always automatically center window upon being shown.
            /// </summary>
            Always,
        }


        /// <summary>
        /// Provides efficient access to active window instances.
        /// </summary>
        private static Dictionary<Type, RotorzWindow> s_Instances = new Dictionary<Type, RotorzWindow>();

        internal static T GetInstance<T>() where T : RotorzWindow
        {
            return s_Instances.ContainsKey(typeof(T))
                ? (T)s_Instances[typeof(T)]
                : null;
        }


        private static bool s_ShouldAssignCenterOnEnable;
        private static CenterMode s_AssignCenterOnEnable;


        /// <summary>
        /// Get utility window instance and create if not already shown.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="RotorzWindow"/> to get.</typeparam>
        /// <param name="title">Title text for window.</param>
        /// <param name="focus">Indicates whether window should be focused.</param>
        /// <returns>
        /// The <see cref="RotorzWindow"/> instance.
        /// </returns>
        internal static T GetUtilityWindow<T>(string title, bool focus) where T : RotorzWindow
        {
            try {
                s_ShouldAssignCenterOnEnable = true;
                s_AssignCenterOnEnable = RtsPreferences.AlwaysCenterUtilityWindows
                    ? CenterMode.Always
                    : CenterMode.Once;
                return GetWindow<T>(true, title, focus);
            }
            finally {
                s_ShouldAssignCenterOnEnable = false;
            }
        }

        /// <inheritdoc cref="GetUtilityWindow{T}(string, bool)"/>
        internal static T GetUtilityWindow<T>(string title) where T : RotorzWindow
        {
            return GetUtilityWindow<T>(title, true);
        }

        /// <inheritdoc cref="GetUtilityWindow{T}(string, bool)"/>
        internal static T GetUtilityWindow<T>(bool focus) where T : RotorzWindow
        {
            return GetUtilityWindow<T>(null, focus);
        }

        /// <inheritdoc cref="GetUtilityWindow{T}(string, bool)"/>
        internal static T GetUtilityWindow<T>() where T : RotorzWindow
        {
            return GetUtilityWindow<T>(null, true);
        }

        /// <summary>
        /// Repaint window of specified type if window is shown.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="RotorzWindow"/> to repaint.</typeparam>
        internal static void RepaintIfShown<T>() where T : RotorzWindow
        {
            var window = GetInstance<T>();
            if (window != null) {
                window.Repaint();
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RotorzWindow"/> class.
        /// </summary>
        public RotorzWindow()
        {
        }


        /// <summary>
        /// Gets or sets whether window should be centered upon first being shown.
        /// </summary>
        public CenterMode CenterWhenFirstShown { get; set; }

        /// <summary>
        /// Gets or sets initial size of window.
        /// </summary>
        public Vector2 InitialSize { get; set; }


        #region Messages and Events

        private void OnEnable()
        {
            s_Instances[this.GetType()] = this;

            if (s_ShouldAssignCenterOnEnable) {
                s_ShouldAssignCenterOnEnable = false;
                this.CenterWhenFirstShown = s_AssignCenterOnEnable;
            }

            this.DoEnable();

            // Trigger repaint of window since existing windows no longer seem to get
            // repainted after an assembly reload occurs in Unity 5.3.
            this.Repaint();
        }

        private void OnDisable()
        {
            this.DoDisable();
        }

        private void OnDestroy()
        {
            s_Instances[this.GetType()] = null;
            this.DoDestroy();
        }

        #endregion

        [NonSerialized]
        private bool hasInitializedPosition;

        private void InitializePosition()
        {
            this.hasInitializedPosition = true;

            string prefsKey = GetType().FullName + ".HasShownOnce";

            if (this.CenterWhenFirstShown == CenterMode.Always || !EditorPrefs.GetBool(prefsKey)) {
                EditorPrefs.SetBool(prefsKey, true);

                Vector2 size = this.InitialSize;
                if (size.x > 30 && size.y > 30) {
                    Rect newPosition = position;

                    if (this.CenterWhenFirstShown != CenterMode.No) {
                        newPosition.x = (Screen.currentResolution.width - size.x) / 2;
                        newPosition.y = (Screen.currentResolution.height - size.y) / 2;
                    }

                    newPosition.width = size.x;
                    newPosition.height = size.y;

                    this.position = newPosition;
                }
            }
        }

        private void OnGUI()
        {
            if (!this.hasInitializedPosition) {
                this.InitializePosition();
            }

            this.titleContent.image = RotorzEditorStyles.Skin.DefaultWindowIcon;

            this.DoGUI();
        }


        /// <summary>
        /// Replacement for <c>OnEnable</c> which can be overridden as needed.
        /// </summary>
        protected virtual void DoEnable()
        {
        }

        /// <summary>
        /// Replacement for <c>OnDisable</c> which can be overridden as needed.
        /// </summary>
        protected virtual void DoDisable()
        {
        }

        /// <summary>
        /// Replacement for <c>OnDestroy</c> which can be overridden as needed.
        /// </summary>
        protected virtual void DoDestroy()
        {
        }

        /// <summary>
        /// Replacement for <c>OnGUI</c> which can be overridden as needed.
        /// </summary>
        protected virtual void DoGUI()
        {
        }
    }
}
