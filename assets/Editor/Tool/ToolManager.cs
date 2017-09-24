// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Delegate for tool changes.
    /// </summary>
    /// <param name="oldTool">
    /// Previously selected tool; or <c>null</c> if no tool was previously selected.
    /// </param>
    /// <param name="newTool">
    /// Newly selected tool; or <c>null</c> if no tool is now selected.
    /// </param>
    public delegate void ToolChangedDelegate(ToolBase oldTool, ToolBase newTool);


    /// <summary>
    /// Transparently switches between tile system tools and standard Unity tools.
    /// Tile system tools can be registered using <see cref="Rotorz.Tile.Editor.ToolManager.RegisterTool"/>
    /// which are then shown in the tool palette user interface.
    /// </summary>
    /// <remarks>
    /// <para>Custom tile system tools can be added by registering them via the
    /// tool manager. The provided tools can also be unregistered if they are
    /// not needed or if custom equivalents have been created.</para>
    /// </remarks>
    [InitializeOnLoad]
    public sealed class ToolManager
    {
        private static double s_LastUpdateTime;

        static ToolManager()
        {
            // Verify state of tool activation every 100ms.
            EditorApplication.update += () => {
                double timeNow = EditorApplication.timeSinceStartup;
                if (timeNow - s_LastUpdateTime >= 0.1) {
                    s_LastUpdateTime = timeNow;
                    Instance.VerifyState();
                }
            };
        }


        #region Singleton

        private static ToolManager s_Instance;

        /// <summary>
        /// Gets one and only tool manager instance.
        /// </summary>
        public static ToolManager Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = new ToolManager();
                    s_Instance.RegisterDefaultTools();
                }
                return s_Instance;
            }
        }

        #endregion


        #region Construction

        // Private constructor to prevent instantiation.
        private ToolManager()
        {
            this.toolsInOrderRegisteredReadOnly = new ReadOnlyCollection<ToolBase>(this.toolsInOrderRegistered);
            this.toolsInUserOrderReadOnly = new ReadOnlyCollection<ToolBase>(this.toolsInUserOrder);
        }


        private void RegisterDefaultTools()
        {
            this.RegisterTool<PaintTool>();
            this.RegisterTool<CycleTool>();
            this.RegisterTool<FillTool>();
            this.RegisterTool<PickerTool>();
            this.RegisterTool<LineTool>();
            this.RegisterTool<RectangleTool>();
            this.RegisterTool<SprayTool>();
            this.RegisterTool<PlopTool>();
        }

        #endregion


        #region Tool Selection

        private static ToolBase s_DefaultPaintTool;

        /// <summary>
        /// Gets or sets the default paint tool.
        /// </summary>
        public static ToolBase DefaultPaintTool {
            get {
                if (s_DefaultPaintTool == null) {
                    s_DefaultPaintTool = ToolManager.Instance.Find<PaintTool>();
                }
                return s_DefaultPaintTool;
            }
            set { s_DefaultPaintTool = value; }
        }


        /// <summary>
        /// The previous unity tool.
        /// </summary>
        private Tool previousUnityTool;
        /// <summary>
        /// The current tile system tool.
        /// </summary>
        private ToolBase currentTool;
        /// <summary>
        /// The previous tile system tool.
        /// </summary>
        private ToolBase previousTool;


        /// <summary>
        /// Occurs when current tool has changed.
        /// </summary>
        public event ToolChangedDelegate ToolChanged;


        /// <summary>
        /// Gets or sets current tool selection.
        /// </summary>
        /// <value>
        /// Reference to currently selected tool; otherwise a value of <c>null</c>
        /// if no tool is currently selected.
        /// </value>
        /// <seealso cref="SelectTool{T}()"/>
        public ToolBase CurrentTool {
            get { return this.currentTool; }
            set { this.SelectTool(value); }
        }
        /// <summary>
        /// Gets the previously selected tool.
        /// </summary>
        /// <value>
        /// Reference to previously selected tool or a value of <c>null</c>.
        /// </value>
        public ToolBase PreviousTool {
            get { return this.previousTool; }
        }


        /// <summary>
        /// Selects tool for use within custom system.
        /// </summary>
        /// <typeparam name="T">Type of tool.</typeparam>
        /// <returns>
        /// The <see cref="ToolBase"/> instance; otherwise a value of <c>null</c>
        /// if tool has not been registered.
        /// </returns>
        public ToolBase SelectTool<T>() where T : ToolBase
        {
            return this.CurrentTool = this.Find<T>();
        }

        /// <summary>
        /// Selects tool for use within custom system.
        /// </summary>
        /// <param name="tool">Tool that is to be selected. Specify `null` to
        /// revert to previous Unity tool.</param>
        /// <returns>
        /// Instance of tool that was selected.
        /// </returns>
        public ToolBase SelectTool(ToolBase tool)
        {
            if (tool != null) {
                this.AutoActivateTileSystem();

                if (ToolUtility.ActiveTileSystem == null) {
                    // Deselect tool because no tile system is selected!
                    tool = null;
                }
                else if (this.currentTool == null) {
                    // Reveal active tile system in scene palette when tool is first activated.
                    // Let's not keep scrolling to the active tile system each time the user
                    // selects another tool since this can be quite annoying!
                    ToolUtility.RevealTileSystem(ToolUtility.ActiveTileSystem, false);

                    // Automatically show tool palette upon activating tool if specified
                    // in user preferences.
                    if (RtsPreferences.AutoShowToolPalette) {
                        ToolUtility.ShowToolPalette(false);
                    }
                }
            }

            if (tool != null) {
                // Should current Unity tool be preserved whilst custom tool is used?
                if (UnityEditor.Tools.current != Tool.None) {
                    this.previousUnityTool = UnityEditor.Tools.current;
                    UnityEditor.Tools.current = Tool.None;
                }
            }
            else if (UnityEditor.Tools.current == Tool.None) {
                // Revert back to former Unity tool because tool has been deselected!
                UnityEditor.Tools.current = this.previousUnityTool;
            }

            // Will tool selection actually change?
            if (tool == this.currentTool) {
                return tool;
            }

            // Reset active plop reference to avoid issues when switching tools.
            ToolUtility.ActivePlop = null;

            if (this.currentTool != null) {
                this.previousTool = this.currentTool;
            }

            // Switch to specified tool.
            var oldTool = this.currentTool;
            this.currentTool = tool;

            if (oldTool != null) {
                oldTool.OnDisable();
            }

            if (tool != null) {
                if (oldTool == null) {
                    // No tool was active prior to using this method to select a tool.
                    this.BeginEditMode();
                }

                tool.OnEnable();
            }
            else if (oldTool != null) {
                // A tool was active prior to using this method to deselect tool.
                this.ExitEditMode();
            }

            // Raise event for tool change.
            if (this.ToolChanged != null) {
                try {
                    this.ToolChanged(oldTool, this.currentTool);
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            ToolUtility.RepaintToolPalette();

            // Brush palette only needs to be repainted when tool is first selected or deselected.
            if (oldTool == null || tool == null) {
                ToolUtility.RepaintBrushPalette();
            }

            SceneView.RepaintAll();

            return tool;
        }

        /// <summary>
        /// Automatically activate tile system from user selection if appropriate so that
        /// tool can be selected.
        /// </summary>
        private void AutoActivateTileSystem()
        {
            ToolUtility.SelectActiveOrParentTileSystem();

            if (ToolUtility.ActiveTileSystem == null) {
                // Okay, no tile system is actually selected.
                // Select first non-locked if possible; otherwise select first if possible.
                var tileSystems = ToolUtility.GetAllTileSystemsInScene();
                var firstTileSystem = tileSystems.FirstOrDefault(s => s != null && !s.Locked);
                if (firstTileSystem == null) {
                    firstTileSystem = tileSystems.FirstOrDefault(s => s != null);
                }

                if (firstTileSystem != null) {
                    ToolUtility.SelectTileSystem(firstTileSystem);
                }
                else {
                    return;
                }
            }

            // When using a tool it only makes sense to have one selected tile system.
            if (Selection.objects.Length > 1) {
                Selection.objects = new Object[] { ToolUtility.ActiveTileSystem.gameObject };
            }
        }

        /// <summary>
        /// Begin editing mode since tool has just been activated.
        /// </summary>
        private void BeginEditMode()
        {
            ToolUtility.HideSelectedWireframe(ToolUtility.ActiveTileSystem);
        }

        /// <summary>
        /// Exit editing mode since no tool is currently active.
        /// </summary>
        private void ExitEditMode()
        {
            ToolUtility.RestoreSelectedWireframe();
        }

        /// <summary>
        /// Determines whether tool is selected.
        /// </summary>
        /// <typeparam name="T">Type of tool.</typeparam>
        /// <returns>
        /// A value of <c>true</c> if tool is selected; otherwise a value of <c>false</c>.
        /// </returns>
        public bool IsSelected<T>() where T : ToolBase
        {
            return this.currentTool != null && this.currentTool.GetType() == typeof(T);
        }

        /// <summary>
        /// Verify state of tool selection.
        /// </summary>
        private void VerifyState()
        {
            // Verification typically occurs every 100ms.
            if (this.currentTool != null) {
                bool shouldDeselectTool = false;

                // The wrong number of objects are selected or selection does not contain
                // the active tile system?
                if (ToolUtility.ActiveTileSystem == null || Selection.activeGameObject != ToolUtility.ActiveTileSystem.gameObject || Selection.objects.Length != 1) {
                    shouldDeselectTool = true;
                }

                // Should we restore the current Unity tool?
                if (UnityEditor.Tools.current != Tool.None) {
                    shouldDeselectTool = true;
                }

                if (shouldDeselectTool) {
                    this.CurrentTool = null;
                }
            }
        }

        /// <summary>
        /// Checks for keyboard shortcut.
        /// </summary>
        /// <remarks>
        /// <para>Must be invoked from context of <c>OnGUI</c>, <c>OnSceneGUI</c> or <c>OnInspectorGUI</c>.</para>
        /// </remarks>
        internal void CheckForKeyboardShortcut()
        {
            var e = Event.current;

            // Switch tool using keyboard shortcut?
            if (e.type == EventType.KeyDown && !e.control && !e.shift && !e.alt) {
                switch (e.keyCode) {
                    case KeyCode.B:
                        this.CurrentTool = DefaultPaintTool;
                        break;
                    case KeyCode.X:
                        // Switch primary and secondary brushes.
                        Brush temp = ToolUtility.SelectedBrush;
                        ToolUtility.SelectedBrush = ToolUtility.SelectedBrushSecondary;
                        ToolUtility.SelectedBrushSecondary = temp;
                        break;
                    case KeyCode.G:
                        this.SelectTool<FillTool>();
                        break;
                    case KeyCode.I:
                        this.SelectTool<PickerTool>();
                        break;
                    case KeyCode.U:
                        this.SelectTool<RectangleTool>();
                        break;
                    case KeyCode.P:
                        this.SelectTool<PlopTool>();
                        break;

                    default:
                        return;
                }

                e.Use();
            }
        }

        #endregion


        #region Tool Management

        private Dictionary<Type, ToolBase> registeredTools = new Dictionary<Type, ToolBase>();

        private List<ToolBase> toolsInOrderRegistered = new List<ToolBase>();
        private ReadOnlyCollection<ToolBase> toolsInOrderRegisteredReadOnly;

        internal List<ToolBase> toolsInUserOrder = new List<ToolBase>();
        private ReadOnlyCollection<ToolBase> toolsInUserOrderReadOnly;


        /// <summary>
        /// Gets list of tools in order of registration.
        /// </summary>
        /// <value>
        /// Read-only list of registered tools.
        /// </value>
        internal IList<ToolBase> ToolsInOrderRegistered {
            get { return this.toolsInOrderRegisteredReadOnly; }
        }

        /// <summary>
        /// Gets ordered list of tools.
        /// </summary>
        /// <value>
        /// Read-only list of registered tools.
        /// </value>
        public IList<ToolBase> Tools {
            get { return this.toolsInUserOrderReadOnly; }
        }


        /// <summary>
        /// Register custom tool with tile system editor.
        /// </summary>
        /// <example>
        /// <para>Register custom tool by creating a custom editor script with a class
        /// that implements <see cref="ToolBase"/>. The custom tool must be registered
        /// with the <see cref="ToolManager"/> which can be achieved easily by adding
        /// the attribute <c>InitializeOnLoad</c> and a static initializer function.</para>
        /// <para>See <a href="http://unity3d.com/support/documentation/Manual/RunningEditorCodeOnLaunch.html">Running Editor Script Code on Launch</a>
        /// for more information regarding <c>InitializeOnLoad</c>.</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// [InitializeOnLoad]
        /// public class MagicTool : ToolBase
        /// {
        ///     static MagicTool()
        ///     {
        ///         ToolManager.Instance.RegisterTool<MagicTool>();
        ///     }
        ///
        ///
        ///     public override string Label {
        ///         get { return "Magic"; }
        ///     }
        ///
        ///
        ///     public override void OnTool(ToolEvent e, IToolContext context)
        ///     {
        ///         // Do something magical!
        ///     }
        ///
        ///     public override void OnToolInactive(ToolEvent e, IToolContext context)
        ///     {
        ///         // Do something magical!
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <returns>
        /// Instance of registered tool.
        /// </returns>
        /// <typeparam name="T">Type of tool</typeparam>
        public ToolBase RegisterTool<T>() where T : ToolBase, new()
        {
            // Has a tool already been registered with this name?
            if (this.registeredTools.ContainsKey(typeof(T))) {
                Debug.LogError("A tool has already been registered of type '" + typeof(T).FullName + "'");
                return null;
            }

            ToolBase tool = new T();

            // Prepare settings for tool.
            var group = AssetSettingManagement.GetGroup("Tool[" + typeof(T).FullName + "]");
            tool.PrepareSettings(group);
            group.Seal();

            this.registeredTools[typeof(T)] = tool;
            this.toolsInOrderRegistered.Add(tool);

            // Restore visibility of tool from user settings but bypass property
            // setter to avoid immediately marking tool ordering dirty.
            tool._visible = !ToolManagementSettings.IsToolHidden(tool);

            // Restore user preferred ordering of tools.
            ToolManagementSettings.LoadToolOrdering();

            return tool;
        }

        /// <summary>
        /// Unregisters the tool.
        /// </summary>
        /// <returns>
        /// Returns true to indicate that tool was unregistered, otherwise false.
        /// </returns>
        /// <typeparam name="T">Type of tool</typeparam>
        public bool UnregisterTool<T>() where T : ToolBase
        {
            ToolBase tool;
            if (this.registeredTools.TryGetValue(typeof(T), out tool)) {
                this.registeredTools.Remove(typeof(T));
                this.toolsInOrderRegistered.Remove(tool);
                this.toolsInUserOrder.Remove(tool);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find tool of specified type.
        /// </summary>
        /// <typeparam name="T">Type of tool.</typeparam>
        /// <returns>
        /// The <see cref="ToolBase"/> instance; otherwise a value of <c>null</c>
        /// if tool has not been registered.
        /// </returns>
        public T Find<T>() where T : ToolBase
        {
            ToolBase tool;
            this.registeredTools.TryGetValue(typeof(T), out tool);
            return tool as T;
        }

        #endregion
    }
}
