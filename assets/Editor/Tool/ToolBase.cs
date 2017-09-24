// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings;
using Rotorz.Tile.Internal;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Base class for a tool that can interact with the active tile system.
    /// Tools must inherit this class for compatability with the tile system
    /// tool manager. For an example of registering a custom tool please refer
    /// to <see cref="Rotorz.Tile.Editor.ToolManager.RegisterTool">ToolManager.RegisterTool&lt;T&gt;</see>.
    /// </summary>
    /// <intro>
    /// <para>For information regarding the usage of tools please refer to
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Tools">Tools</a>
    /// section of user guide.</para>
    /// </intro>
    /// <remarks>
    /// <para>Tools cannot interact with locked tile systems.</para>
    /// </remarks>
    /// <seealso cref="PaintToolBase"/>
    public abstract class ToolBase
    {
        /// <summary>
        /// Gets or sets reference to previous tool event object.
        /// </summary>
        /// <exclude/>
        public static ToolEvent PreviousToolEvent { get; set; }

        /// <summary>
        /// Gets a value indicating whether user is interacting with the tile system
        /// editor or with another control which is nearer within the scene view.
        /// </summary>
        /// <remarks>
        /// <para>This property is often useful when overriding <see cref="OnSceneGUI(ToolEvent, IToolContext)"/>
        /// to avoid blocking use of the scene view rotation gadget:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void OnSceneGUI(ToolEvent e, IToolContext context) {
        ///     if (!IsEditorNearestControl) {
        ///         return;
        ///     }
        ///
        ///     // ...
        /// }
        /// ]]></code>
        /// </remarks>
        public static bool IsEditorNearestControl { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether Cmd (OS X) or Ctrl (Windows) is pressed.
        /// </summary>
        internal static bool IsCommandKeyPressed {
            get {
                return Application.platform == RuntimePlatform.OSXEditor
                    ? Event.current.command
                    : Event.current.control;
            }
        }


        // Variables indicate whether `OnToolOptionsGUI` and/or `OnAdvancedToolOptionsGUI`
        // have been overridden thus implying whether shown.
        private bool hasToolOptionsGUI;
        private bool hasAdvancedToolOptionsGUI;


        /// <summary>
        /// Initialize new instance of <see cref="ToolBase"/>.
        /// </summary>
        public ToolBase()
        {
            // Does this tool implement an custom options interfaces?
            this.hasToolOptionsGUI = this.GetType().GetMethod("OnToolOptionsGUI").DeclaringType != typeof(ToolBase);
            this.hasAdvancedToolOptionsGUI = this.GetType().GetMethod("OnAdvancedToolOptionsGUI").DeclaringType != typeof(ToolBase);
        }


        #region Tool Options

        internal void PrepareSettings(IDynamicSettingGroup group)
        {
            this.Options = group;

            this.settingShowAdvancedOptionsGUI = group.Fetch<bool>("ShowAdvancedOptionsGUI", false);

            this.PrepareOptions(group);
        }

        /// <summary>
        /// Invoked allowing tool to prepare options from the provided setting store.
        /// </summary>
        /// <remarks>
        /// <para>It is important to inherit the initialization of options from inherited
        /// tool class by including <c>base.PrepareOptions(store)</c>.</para>
        /// <para>Setting classes are contained within the <see cref="Rotorz.Settings"/>
        /// namespaces which will need to be included at start of custom tool scripts
        /// when overriding this method:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Settings;
        /// ]]></code>
        /// </remarks>
        /// <example>
        /// <para>Prepare and present boolean option for a custom tool:</para>
        /// <code language="csharp"><![CDATA[
        /// private Setting<bool> settingEnableFoo;
        ///
        ///
        /// public bool EnableFoo {
        ///     get { return this.settingEnableFoo.Value; }
        ///     set { this.settingEnableFoo.Value = value; }
        /// }
        ///
        ///
        /// protected override void PrepareOptions(ISettingStore store)
        /// {
        ///     base.PrepareOptions(store);
        ///     this.settingEnableFoo = store.Fetch<bool>("EnableFoo", true);
        /// }
        ///
        /// public override void OnAdvancedToolOptionsGUI()
        /// {
        ///     EnableFoo = EditorGUILayout.ToggleLeft("Enable Foo", EnableFoo);
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="store">Setting store.</param>
        protected virtual void PrepareOptions(ISettingStore store)
        {
        }

        /// <summary>
        /// Gets group of tool specific options.
        /// </summary>
        internal ISettingGroup Options { get; private set; }

        internal bool _visible = true;

        /// <summary>
        /// Gets or sets whether tool should be shown in tool palette window.
        /// </summary>
        public bool Visible {
            get { return this._visible; }
            set { ToolManagementSettings.HideTool(this, !value); }
        }

        private Setting<bool> settingShowAdvancedOptionsGUI;

        /// <summary>
        /// Gets or sets whether advanced section of options interface is shown for tool.
        /// </summary>
        public bool ShowAdvancedOptionsGUI {
            get { return this.settingShowAdvancedOptionsGUI.Value; }
            set { this.settingShowAdvancedOptionsGUI.Value = value; }
        }

        #endregion


        #region Tool Information

        /// <summary>
        /// Gets label for tool selection button.
        /// </summary>
        public abstract string Label { get; }

        /// <summary>
        /// Gets icon for normal tool state.
        /// </summary>
        /// <remarks>
        /// <para>Return 22x22 icon texture or a value of <c>null</c> to assume default label
        /// instead. This property must not generate the icon texture each time it
        /// is accessed; instead a local copy should be cached for future calls.</para>
        /// </remarks>
        /// <example>
        /// <para>Use custom icon for tool:</para>
        /// <code language="csharp"><![CDATA[
        /// using Rotorz.Tile;
        /// using Rotorz.Tile.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// [InitializeOnLoad]
        /// public class MagicTool : ToolBase
        /// {
        ///     private static Texture2D s_IconNormal;
        ///     private static Texture2D s_IconActive;
        ///
        ///
        ///     static MagicTool()
        ///     {
        ///         ToolManager.Instance.RegisterTool<MagicTool>();
        ///
        ///         // Load icon that is normally shown for tool
        ///         s_IconNormal = Resources.LoadAssetAtPath(
        ///             "Assets/Editor/MagicToolIcon_Normal.png",
        ///             typeof(Texture2D)
        ///         ) as Texture2D;
        ///
        ///         // Load icon for active tool (usually inverted colors)
        ///         s_IconActive = Resources.LoadAssetAtPath(
        ///             "Assets/Editor/MagicToolIcon_Active.png",
        ///             typeof(Texture2D)
        ///         ) as Texture2D;
        ///     }
        ///
        ///
        ///     public override Texture2D IconNormal {
        ///         get { return s_IconNormal; }
        ///     }
        ///
        ///     public override Texture2D IconActive {
        ///         get { return s_IconActive; }
        ///     }
        ///
        ///
        ///     // Remainder of custom tool implementation...
        /// }
        /// ]]></code>
        /// </example>
        /// <seealso cref="IconActive"/>
        public virtual Texture2D IconNormal {
            get { return null; }
        }

        /// <summary>
        /// Gets icon for active tool state.
        /// </summary>
        /// <remarks>
        /// <para>Return 22x22 icon texture or a value of <c>null</c> to assume value of <see cref="IconNormal"/>
        /// instead. This property must not generate the icon texture each time it
        /// is accessed; instead a local copy should be cached for future calls.</para>
        /// </remarks>
        /// <inheritdoc cref="IconNormal" select="example"/>
        /// <seealso cref="IconNormal"/>
        public virtual Texture2D IconActive {
            get { return null; }
        }

        /// <summary>
        /// Gets custom cursor to use when tool is active.
        /// </summary>
        /// <remarks>
        /// <para>When using custom cursors:</para>
        /// <list type="bullet">
        /// <item>Width and height should be 32 pixels.</item>
        /// <item>This property must not generate the cursor texture each time it is accessed;
        /// instead a local copy should be cached for future calls.</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <para>Use one of the built-in Unity cursors:</para>
        /// <code language="csharp"><![CDATA[
        /// public override CursorInfo Cursor {
        ///     get {
        ///         // CursorInfo is a struct thus no GC allocation occurs:
        ///         var cursor = new CursorInfo();
        ///         cursor.type = MouseCursor.MoveArrow;
        ///         return cursor;
        ///     }
        /// }
        /// ]]></code>
        /// <para>Use one of the provided cursors:</para>
        /// <code language="csharp"><![CDATA[
        /// public override CursorInfo Cursor {
        ///     get { return ToolCursors.Paint; }
        /// }
        /// ]]></code>
        /// <para>Use a custom cursor texture:</para>
        /// <code language="csharp"><![CDATA[
        /// private CursorInfo customCursor;
        ///
        ///
        /// // Prepare custom cursor using tool constructor.
        /// public MagicTool()
        /// {
        ///     var cursorTexture = Resources.LoadAssetAtPath(
        ///         'Assets/Editor/MagicToolCursor.png',
        ///         Texture2D
        ///     );
        ///     this.customCursor = new CursorInfo(cursorTexture, 0, 0);
        /// }
        ///
        ///
        /// public override CursorInfo Cursor {
        ///     get { return this.customCursor; }
        /// }
        /// ]]></code>
        /// </example>
        /// <seealso cref="ToolCursors"/>
        public virtual CursorInfo Cursor {
            get { return default(CursorInfo); }
        }

        #endregion


        #region Tool Interaction

        /// <summary>
        /// Pre-filter mouse position (in local space of active tile system) ready for
        /// handling tool event. This is useful for switching between grid cell and point
        /// alignment for even nozzle sizes.
        /// </summary>
        /// <param name="localPoint">Local point of mouse on tile system plane.</param>
        /// <returns>
        /// Modified local point.
        /// </returns>
        public virtual Vector3 PreFilterLocalPoint(Vector3 localPoint)
        {
            return localPoint;
        }

        /// <summary>
        /// Gets a value indicating whether target point should be constrained when tool has
        /// been anchored. For example, holding shift to contains to straight lines or uniform
        /// rectangles.
        /// </summary>
        /// <remarks>
        /// <para>Yields a value of <c>true</c> when target point should be constrained.</para>
        /// </remarks>
        protected virtual bool IsTargetPointConstrained {
            get { return Event.current.shift; }
        }

        /// <summary>
        /// Raised when tool becomes active.
        /// </summary>
        public virtual void OnEnable()
        {
        }

        /// <summary>
        /// Raised when tool becomes inactive.
        /// </summary>
        public virtual void OnDisable()
        {
        }

        /// <summary>
        /// Raised allowing tool to adjust tool event before interacting with tool or
        /// handling scene view GUI events.
        /// </summary>
        /// <example>
        /// <para>The line tool overrides this method allowing the user to constrain
        /// to straight horizontal/vertical lines when the shift key is held:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void FilterToolEvent(ToolEvent e, IToolContext context)
        /// {
        ///     if (Event.current.shift) {
        ///         var targetIndex = e.tileIndex;
        ///
        ///         // Determine whether to constrain horizontally or vertically.
        ///         int lineRowCount = Mathf.Abs(targetIndex.row - this.anchorIndex.row);
        ///         int lineColumnCount = Mathf.Abs(targetIndex.column - this.anchorIndex.column);
        ///         if (lineRowCount < lineColumnCount) {
        ///             targetIndex.row = this.anchorIndex.row;
        ///         }
        ///         else {
        ///             targetIndex.column = this.anchorIndex.column;
        ///         }
        ///
        ///         e.tileIndex = targetIndex;
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="e">Event data.</param>
        /// <param name="context">Context of tool usage.</param>
        public virtual void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
        }

        /// <summary>
        /// Raised when tool is being used.
        /// </summary>
        /// <remarks>
        /// <para>This event occurs when the left or right mouse button is pressed.</para>
        /// <para>Additional information regarding the current event can be determined using
        /// <c>UnityEditor.Event.current</c>.</para>
        /// </remarks>
        /// <example>
        /// <para>The following example demonstrates how to implement a tool which paints
        /// a single tile when either the left or right mouse button is clicked.</para>
        /// <code language="csharp"><![CDATA[
        /// public override void OnTool(ToolEvent e, IToolContext context)
        /// {
        ///     if (e.type == EventType.MouseDown) {
        ///         // Switch between primary and secondary brush depending
        ///         // upon whether left or right button was used
        ///         var brush = e.leftButton
        ///             ? ToolUtility.selectedBrush
        ///             : ToolUtility.selectedBrushSecondary;
        ///
        ///         // Paint or erase a single tile
        ///         if (brush != null) {
        ///             brush.Paint(context.TileSystem, e.tileIndex);
        ///         }
        ///         else {
        ///             context.TileSystem.EraseTile(e.tileIndex);
        ///         }
        ///
        ///         context.TileSystem.RefreshSurroundingTiles(e.tileIndex);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="e">Event data.</param>
        /// <param name="context">Context of tool usage.</param>
        public abstract void OnTool(ToolEvent e, IToolContext context);

        /// <summary>
        /// Raised when tool is being used but is inactive.
        /// </summary>
        /// <remarks>
        /// <para>This event occurs when the left or right mouse button is <b>not</b> pressed.</para>
        /// <para>Additional information regarding the current event can be determined using
        /// <c>UnityEditor.Event.current</c>.</para>
        /// </remarks>
        /// <param name="e">Event data.</param>
        /// <param name="context">Context of tool usage.</param>
        public virtual void OnToolInactive(ToolEvent e, IToolContext context)
        {
        }

        #endregion


        #region Scene View

        /// <summary>
        /// Gets indicator that should be drawn to represent tool nozzle.
        /// </summary>
        /// <remarks>
        /// <para>This method is called by <see cref="DrawNozzleIndicator"/> to determine
        /// which nozzle indicator should be used for the given context. For example, the
        /// wireframe indicator is sometimes more appropriate for 3D tiles whilst the flat
        /// indicator is better for 2D tiles.</para>
        /// <para>The outcome of this method can be customized by providing a specialized
        /// implementation. This can be useful to dynamically choose the indicator most
        /// relevant for an existing tile.</para>
        /// </remarks>
        /// <example>
        /// <para>The following example demonstrates how to pick an indicator based upon
        /// an existing tile. The method begins by assuming the user preferred nozzle
        /// indicator and proceeds to automatically pick between flat and wireframe when
        /// specified based upon the brush that was used to paint the existing tile.</para>
        /// <code language="csharp"><![CDATA[
        /// protected override NozzleIndicator GetNozzleIndicator(TileSystem system, int row, int column, BrushNozzle nozzle)
        /// {
        ///     NozzleIndicator mode = PreferredNozzleIndicator;
        ///
        ///     // Would user prefer tool to automatically pick indicator?
        ///     if (mode == NozzleIndicator.Automatic) {
        ///         mode = NozzleIndicator.Flat;
        ///
        ///         // Lookup the existing tile and switch to wireframe mode if
        ///         // associated brush indicates to do so.
        ///         TileData tile = system.GetTile(row, column);
        ///         if (tile != null && tile.brush != null && tile.brush.UseWireIndicatorInEditor) {
        ///             mode = NozzleIndicator.Wireframe;
        ///         }
        ///     }
        ///
        ///     return mode;
        /// }
        /// ]]></code>
        /// </example>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile.</param>
        /// <param name="nozzle">Type of brush nozzle.</param>
        protected virtual NozzleIndicator GetNozzleIndicator(TileSystem system, TileIndex index, BrushNozzle nozzle)
        {
            NozzleIndicator mode = RtsPreferences.ToolPreferredNozzleIndicator;

            if (mode == NozzleIndicator.Automatic) {
                mode = NozzleIndicator.Flat;

                if (ToolUtility.SelectedBrush != null) {
                    if (ToolUtility.SelectedBrush.UseWireIndicatorInEditor) {
                        mode = NozzleIndicator.Wireframe;
                    }
                }
                else {
                    // Determine based upon active tile (i.e. when using eraser).
                    var tile = system.GetTile(index);
                    if (tile != null && tile.brush != null && tile.brush.UseWireIndicatorInEditor) {
                        mode = NozzleIndicator.Wireframe;
                    }
                }
            }

            return mode;
        }

        /// <summary>
        /// Draw nozzle indicator to provide user with feedback by indicating position
        /// of active tile in scene view.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation of this function honors preferred nozzle
        /// indicator where possible. Flat indicator is drawn to represent larger areas of
        /// tiles when a radius larger than zero is specified regardless of the preferred
        /// nozzle indicator.</para>
        /// <para>This function is typically invoked automatically when processing scene
        /// GUI events (unless of course <see cref="OnSceneGUI"/> has been overridden).</para>
        /// </remarks>
        /// <param name="system">Tile system.</param>
        /// <param name="index">Index of tile.</param>
        /// <param name="nozzle">Type of brush nozzle.</param>
        /// <param name="nozzleSize">Size of nozzle where <c>1</c> is a single tile.</param>
        /// <seealso cref="GetNozzleIndicator"/>
        protected virtual void DrawNozzleIndicator(TileSystem system, TileIndex index, BrushNozzle nozzle, int nozzleSize)
        {
            nozzleSize = Mathf.Max(1, nozzleSize);

            // Temporarily scale handles matrix by cell size of tile system.
            Matrix4x4 restoreMatrix = Handles.matrix;
            Matrix4x4 tempMatrix = restoreMatrix;
            MathUtility.MultiplyMatrixByScale(ref tempMatrix, system.CellSize);
            Handles.matrix = tempMatrix;

            // Calculate position to draw indicator (in local space of tile system).
            Vector3 position = new Vector3(index.column + 0.5f, -index.row - 0.5f);

            // Always use flat indicator for larger nozzle sizes.
            NozzleIndicator mode = (nozzleSize > 1)
                ? NozzleIndicator.Flat
                : this.GetNozzleIndicator(system, index, nozzle);

            if (mode == NozzleIndicator.Flat) {
                // Always use square nozzle indicator when radius only permits a single
                // tile to be painted!
                if (nozzleSize == 1) {
                    nozzle = BrushNozzle.Square;
                }

                --nozzleSize;

                float indicatorRadius;

                switch (nozzle) {
                    default:
                    case BrushNozzle.Round:
                        indicatorRadius = (float)(nozzleSize / 2) + 0.5f;
                        break;

                    case BrushNozzle.Square:
                        indicatorRadius = (float)nozzleSize / 2f + 0.5f;

                        // Offset indicator for odd sizes.
                        if ((nozzleSize & ~1) != nozzleSize) {
                            position.x += 0.5f;
                            position.y -= 0.5f;
                        }
                        break;
                }

                // Illustrate size of brush radius.
                ToolHandleUtility.DrawNozzleIndicatorSmoothRadius(position, nozzle, indicatorRadius);
            }
            else {
                // Simple 3D wire cube.
                ToolHandleUtility.DrawWireCube(position);
            }

            Handles.matrix = restoreMatrix;
        }

        /// <summary>
        /// Raised when handling scene view GUI events.
        /// </summary>
        /// <remarks>
        /// <para>The primary purpose of this method is to draw helper objects into
        /// scene views making it easier to interact with tile systems. Custom tool
        /// processing logic should be placed into one of the following methods
        /// where possible:</para>
        /// <list type="bullet">
        ///     <item><see cref="OnRefreshToolEvent"/></item>
        ///     <item><see cref="OnTool"/></item>
        ///     <item><see cref="OnToolInactive"/></item>
        /// </list>
        /// <para>This method will be invoked multiple times when handling different
        /// GUI events within a scene view. This method is called from the context of
        /// the tile system editor (see <a href="http://docs.unity3d.com/Documentation/ScriptReference/Editor.OnSceneGUI.html">Editor.OnSceneGUI</a>).</para>
        /// <para>Here is the default implementation of this method:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void OnSceneGUI(ToolEvent e, IToolContext context)
        /// {
        ///     if (!IsEditorNearestControl) {
        ///         return;
        ///     }
        ///     this.DrawNozzleIndicator(context.TileSystem, e.tileIndex, BrushNozzle.Square, 1);
        /// }
        /// ]]></code>
        /// </remarks>
        /// <param name="e">Event data.</param>
        /// <param name="context">Context of tool usage.</param>
        public virtual void OnSceneGUI(ToolEvent e, IToolContext context)
        {
            if (!IsEditorNearestControl) {
                return;
            }

            // Outline plop with wire cube!
            if (ToolUtility.ActivePlop != null) {
                ToolHandleUtility.DrawWireBox(ToolUtility.ActivePlop.PlopPoint, context.TileSystem.CellSize);
                return;
            }

            this.DrawNozzleIndicator(context.TileSystem, e.MousePointerTileIndex, BrushNozzle.Square, 1);
        }

        /// <summary>
        /// Raised to draw custom gizmos within scene view.
        /// </summary>
        /// <remarks>
        /// <para>This method is invoked from the context of <a href="http://docs.unity3d.com/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmos.html">MonoBehaviour.OnDrawGizmos</a>
        /// of the active tile system. Custom gizmos should be drawn using the <a href="http://docs.unity3d.com/Documentation/ScriptReference/Gizmos.html">Gizmos</a>
        /// class.</para>
        /// </remarks>
        /// <param name="system">Active tile system.</param>
        public virtual void OnDrawGizmos(TileSystem system)
        {
        }

        #endregion


        #region Tool Options Interface

        /// <summary>
        /// Raised when checking for keyboard shortcuts when this is the active tool. This method
        /// occurs within the context of <c>OnGUI</c> and thus can use <c>Event.current</c>.
        /// </summary>
        /// <remarks>
        /// <para>This method is invoked for locked tile systems so that tool options can still be
        /// configured. This method <strong>must not</strong> alter a locked tile system (see <see cref="TileSystem.Locked">TileSystem.Locked</see>).</para>
        /// </remarks>
        public virtual void OnCheckKeyboardShortcuts()
        {
        }

        /// <summary>
        /// Raised to draw options GUI for custom tools.
        /// </summary>
        /// <remarks>
        /// <para>This method will be invoked multiple times when handling different
        /// GUI events. Please refer to the Unity documentation for further information
        /// regarding custom GUIs: <a href="http://docs.unity3d.com/Documentation/Components/gui-ExtendingEditor.html">Extending the Editor</a>.</para>
        /// <para>Horizontal and vertical scroll bars will be shown automatically if your
        /// custom tool options interface exceeds window size.</para>
        /// </remarks>
        /// <example>
        /// <para>The following source code demonstrates how to define a user interface
        /// with two buttons.</para>
        /// <para><img src="../art/custom_OnToolOptionsGUI.png" alt="Example of custom tool options GUI"/></para>
        /// <code language="csharp"><![CDATA[
        /// public override void OnToolOptionsGUI()
        /// {
        ///     GUILayout.BeginHorizontal();
        ///     {
        ///         GUILayout.Label("Special Action: ");
        ///
        ///         if (GUILayout.Button("A")) {
        ///             // Do something!
        ///         }
        ///
        ///         if (GUILayout.Button("B")) {
        ///             // Do something!
        ///         }
        ///     }
        ///     GUILayout.EndHorizontal();
        /// }
        /// ]]></code>
        /// </example>
        public virtual void OnToolOptionsGUI()
        {
            if (!this.HasAdvancedToolOptionsGUI) {
                GUILayout.Label(TileLang.Text("Tool has no parameters"), EditorStyles.miniLabel);
            }
        }

        /// <summary>
        /// Gets a value indicating whether tool has options to show.
        /// </summary>
        /// <seealso cref="OnToolOptionsGUI"/>
        internal virtual bool HasToolOptionsGUI {
            get { return this.hasToolOptionsGUI; }
        }

        /// <summary>
        /// Gets a value indicating whether tool has advanced options to show.
        /// </summary>
        /// <remarks>
        /// <para>This property yields a value of <c>true</c> when <see cref="OnAdvancedToolOptionsGUI"/>
        /// has been overridden.</para>
        /// </remarks>
        /// <seealso cref="OnAdvancedToolOptionsGUI"/>
        public bool HasAdvancedToolOptionsGUI {
            get { return this.hasAdvancedToolOptionsGUI; }
        }

        /// <summary>
        /// Raised to draw advanced options GUI for custom tools.
        /// </summary>
        /// <remarks>
        /// <para>This method will be invoked multiple times when handling different
        /// GUI events. Please refer to the Unity documentation for further information
        /// regarding custom GUIs: <a href="http://docs.unity3d.com/Documentation/Components/gui-ExtendingEditor.html">Extending the Editor</a>.</para>
        /// <para>Horizontal and vertical scroll bars will be shown automatically if your
        /// custom tool options interface exceeds window size.</para>
        /// </remarks>
        /// <seealso cref="HasAdvancedToolOptionsGUI"/>
        public virtual void OnAdvancedToolOptionsGUI()
        {
        }

        #endregion


        #region Status Area

        /// <summary>
        /// Raised to draw content of status panel.
        /// </summary>
        internal void OnStatusPanelGUI()
        {
            Color restoreColor = GUI.color;
            GUI.color = new Color(0.8f, 0.8f, 0.8f);

            if (ToolUtility.ActivePlop != null) {
                this.DrawPlopStatusGUI();
            }
            else if (ToolManager.Instance.CurrentTool is PlopTool) {
                this.DrawPlopPointStatusGUI();
            }
            else {
                this.DrawStandardStatusGUI();
            }

            GUI.color = restoreColor;
        }

        private void DrawStandardStatusGUI()
        {
            var activeTileSystem = ToolUtility.ActiveTileSystem;
            TileIndex activeTileIndex = ToolUtility.ActiveTileIndex;
            TileData activeTile = ToolUtility.ActiveTile;

            string statusLabel = string.Format(
                /* 0: zero-based index of row
                   1: zero-based index of column */
                TileLang.Text("Row: {0,5}    Column: {1,5}"),
                activeTileIndex.row, activeTileIndex.column
            );

            Rect outRect = new Rect(5, 20, 205, 17);
            GUI.Label(outRect, statusLabel, EditorStyles.whiteLabel);

            outRect.x = 203;
            outRect.width = 190;

            statusLabel = "-";
            if (activeTile != null) {
                if (activeTile.brush != null) {
                    if (activeTile.tileset != null) {
                        statusLabel = string.Format(
                            /* 0: name of brush
                               1: zero-based index of associated tile in tileset */
                            TileLang.Text("{0} : {1}"),
                            activeTile.brush.name, activeTile.tilesetIndex
                        );
                    }
                    else {
                        statusLabel = activeTile.brush.name;
                    }
                }
                else if (activeTile.tileset != null) {
                    statusLabel = string.Format(
                        /* 0: name of tileset
                           1: zero-based index of tile in tileset */
                        TileLang.Text("{1} from {0}"),
                        activeTile.tileset.name, activeTile.tilesetIndex
                    );
                }
            }
            GUI.Label(outRect, statusLabel, EditorStyles.whiteLabel);

            if (activeTile != null && Event.current.type == EventType.Repaint) {
                int actualOrientation = activeTile.orientationMask;

                // Get oriented brush component of tile.
                var orientedBrush = activeTile.OrientedBrush;
                if (activeTile.brush is AliasBrush) {
                    orientedBrush = activeTile.AliasBrush.target as OrientedBrush;
                }
                // Determine actual orientation of active tile.
                if (orientedBrush != null) {
                    actualOrientation = OrientationUtility.DetermineTileOrientation(activeTileSystem, activeTileIndex, activeTile.brush, activeTile.PaintedRotation);
                }

                Color restoreBackground = GUI.backgroundColor;
                Color activeColor = activeTile.PaintedRotation != 0 ? new Color(0, 40, 255) : Color.green;

                outRect.x = 180;
                outRect.y = 19;
                outRect.width = 5;
                outRect.height = 5;

                int bit = 1;
                for (int i = 0; i < 9; ++i) {
                    if (i == 4) {
                        // Draw center box!
                        GUI.backgroundColor = Color.black;
                        RotorzEditorStyles.Instance.TransparentBox.Draw(outRect, GUIContent.none, false, false, false, false);

                        if (activeTile.PaintedRotation != 0) {
                            GUI.backgroundColor = activeColor;
                            RotorzEditorStyles.Instance.TransparentBox.Draw(new Rect(outRect.x + 1, outRect.y + 1, 3, 3), GUIContent.none, false, false, false, false);
                        }
                    }
                    else {
                        // Green  : Matching part of orientation
                        // Cyan   : Matching part of orientation with rotation applied
                        // Red    : Surplus part of orientation
                        // Gray   : Missing part of orientation

                        if ((actualOrientation & bit) != (activeTile.orientationMask & bit)) {
                            // Orientation of tile is invalid.
                            GUI.backgroundColor = (activeTile.orientationMask & bit) != 0 ? Color.red : Color.gray;
                        }
                        else {
                            // Orientation of tile is valid.
                            GUI.backgroundColor = (activeTile.orientationMask & bit) != 0 ? activeColor : Color.white;
                        }

                        RotorzEditorStyles.Instance.TransparentBox.Draw(outRect, GUIContent.none, false, false, false, false);

                        bit <<= 1;
                    }

                    if (i % 3 == 2) {
                        outRect.x = 180;
                        outRect.y += 6;
                    }
                    else {
                        outRect.x += 6;
                    }
                }

                GUI.backgroundColor = restoreBackground;
            }
        }

        private void DrawPlopStatusGUI()
        {
            if (ToolUtility.ActivePlop == null) {
                return;
            }

            Vector3 plopPosition = ToolUtility.ActivePlop.PlopPoint;
            GUI.Label(new Rect(5, 20, 205, 17), string.Format("X: {0,5:0.0}    Y: {1,5:0.0}", plopPosition.x, plopPosition.y), EditorStyles.whiteLabel);

            if (ToolUtility.ActivePlop.Brush != null) {
                GUI.Label(new Rect(203, 20, 190, 17), ToolUtility.ActivePlop.Brush.name, EditorStyles.whiteLabel);
            }
            else {
                GUI.Label(new Rect(203, 20, 190, 17), ToolUtility.ActivePlop.name + " (Plop)", EditorStyles.whiteLabel);
            }

            if (Event.current.type == EventType.Repaint) {
                GUI.DrawTexture(new Rect(183, 19, 13, 21), RotorzEditorStyles.Skin.Droplet);
            }
        }

        private void DrawPlopPointStatusGUI()
        {
            Vector3 plopPoint = (this as PlopTool).ApplySnapping(PreviousToolEvent.MousePointerLocalPoint);
            GUI.Label(new Rect(5, 20, 205, 17), string.Format("X: {0,5:0.0}    Y: {1,5:0.0}", plopPoint.x, plopPoint.y), EditorStyles.whiteLabel);

            if (Event.current.type == EventType.Repaint) {
                GUI.DrawTexture(new Rect(183, 19, 13, 21), RotorzEditorStyles.Skin.Droplet);
            }
        }

        #endregion
    }
}
