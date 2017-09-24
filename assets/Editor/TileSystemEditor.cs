// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Provides editor functionality for tile systems.
    /// </summary>
    /// <seealso cref="TileSystemInspector"/>
    [CustomEditor(typeof(TileSystem)), CanEditMultipleObjects]
    internal sealed class TileSystemEditor : UnityEditor.Editor, IToolContext
    {
        private static FieldInfo s_fiTools_ButtonDown;

        static TileSystemEditor()
        {
            // Hack to workaround tool selector error.
            s_fiTools_ButtonDown = typeof(Tools).GetField("s_ButtonDown", BindingFlags.NonPublic | BindingFlags.Static);
        }


        private TileSystemInspector inspector;
        private ToolEvent toolEvent;


        #region IToolContext - Implementation

        /// <inheritdoc/>
        ToolManager IToolContext.ToolManager {
            get { return ToolManager.Instance; }
        }

        /// <inheritdoc/>
        ToolBase IToolContext.Tool {
            get { return ToolManager.Instance.CurrentTool; }
        }

        /// <inheritdoc/>
        TileSystem IToolContext.TileSystem {
            get { return ToolUtility.ActiveTileSystem; }
        }

        public object EditorEditorSceneManager { get; private set; }

        #endregion


        #region Messages and Events

        private void OnEnable()
        {
            var targetSystem = target as TileSystem;

            // Mark active target as the active tile system?
            //  - Must be part of current user selection!
            //  - Must not reside within an asset.
            if (Selection.activeGameObject == targetSystem.gameObject && Selection.instanceIDs.Length == 1) {
                if (!EditorUtility.IsPersistent(this.target) && ToolUtility.ActiveTileSystem != targetSystem) {
                    ToolUtility.ActiveTileSystem = targetSystem;

                    // Hide selected wireframe if tool is active.
                    if (ToolManager.Instance.CurrentTool != null) {
                        ToolUtility.HideSelectedWireframe(targetSystem);
                    }
                }
            }
        }

        #endregion


        #region Inspector GUI

        public override void OnInspectorGUI()
        {
            if (this.inspector == null) {
                this.inspector = new TileSystemInspector(this);
            }

            bool restoreWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = false;

            this.inspector.OnGUI();

            EditorGUIUtility.wideMode = restoreWideMode;
        }

        public override bool UseDefaultMargins()
        {
            // Do not assume default margins for inspector.
            return false;
        }

        #endregion


        #region Scene GUI

        private int sceneControlID = -1;

        // Tool Hack: Must correct state of View Tool when using middle or right mouse buttons.
        private void OnPreSceneGUI()
        {
            // This is not the active tile system, bail!!
            if (this.target != ToolUtility.ActiveTileSystem) {
                return;
            }

            this.sceneControlID = GUIUtility.GetControlID(FocusType.Passive);
            if (s_fiTools_ButtonDown == null) {
                return;
            }

            if (ToolManager.Instance.CurrentTool != null) {
                var e = Event.current;

                if (e.GetTypeForControl(this.sceneControlID) == EventType.MouseDown) {
                    if (e.button == 1 && !e.alt && (!e.control || e.shift)) {
                        s_fiTools_ButtonDown.SetValue(null, 0);
                    }
                }
            }
        }

        private void OnSceneGUI()
        {
            var tileSystem = this.target as TileSystem;

            // This is not the active tile system, bail!!
            if (tileSystem != ToolUtility.ActiveTileSystem) {
                return;
            }
            // Skip for non-instance prefabs.
            if (PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab) {
                return;
            }

            // Tool Hack: Just in case OnPreSceneGUI is not supported in the future.
            if (this.sceneControlID == -1) {
                this.sceneControlID = GUIUtility.GetControlID(FocusType.Passive);
            }

            // Do not update the value of `ToolBase.IsEditorNearestControl` during layout
            // events since Unity 4.6.0f1 seems to have introduced a bug whereby the
            // value of `HandleUtility.nearestControl` is temporarily incorrect.
            if (Event.current.type != EventType.Layout) {
                // Determine whether tile system editor is the nearest control to the mouse pointer.
                // We can use this to avoid overuling other controls in the scene view (like the
                // viewing angle gadget in upper-right corner).
                ToolBase.IsEditorNearestControl = (HandleUtility.nearestControl == this.sceneControlID || GUIUtility.hotControl == this.sceneControlID);
            }

            // Should tool event data be initialized?
            if (this.toolEvent == null) {
                this.toolEvent = new ToolEvent();
                this.UpdateToolEventFromCursor(Event.current.mousePosition);
            }

            ToolManager.Instance.CheckForKeyboardShortcut();

            var tool = ToolManager.Instance.CurrentTool;

            if (tool != null) {
                this.DrawStatusPanel();
            }

            // Prevent editing if tile system is not editable!
            if (!tileSystem.IsEditable) {
                return;
            }

            EventType eventType = Event.current.GetTypeForControl(this.sceneControlID);

            switch (eventType) {
                case EventType.Ignore:
                case EventType.Used:
                    return;

                case EventType.Layout:
                    if (tool != null) {
                        // Prevent regular object selection when tool is active.
                        HandleUtility.AddDefaultControl(this.sceneControlID);
                    }
                    break;
            }

            // Need to record whether this is a mouse event prior to tracking mouse
            // input since "MouseDrag" and "MouseUp" will otherwise not be properly
            // detected within `DoTool` since `GUIUtility.hotControl` will have been
            // reassigned to a value of 0.
            bool isMouseEvent = (ToolBase.IsEditorNearestControl && Event.current.IsMouseForControl(this.sceneControlID));
            bool wasSceneActiveControl = (GUIUtility.hotControl == this.sceneControlID);

            ToolBase.PreviousToolEvent = this.toolEvent;
            this.toolEvent.Type = eventType;
            this.toolEvent.WasLeftButtonPressed = this.toolEvent.IsLeftButtonPressed;
            this.toolEvent.WasRightButtonPressed = this.toolEvent.IsRightButtonPressed;

            this.UpdateCursor();

            if (isMouseEvent) {
                this.TrackMouseInput();
            }

            // Do not proceed when a different tool is anchored (with mouse drag)!
            if (tool != null && (s_AnchorTool == null || s_AnchorTool == tool)) {
                this.DoTool(tool, isMouseEvent, wasSceneActiveControl);
            }
        }

        private void UpdateCursor()
        {
            if (RtsPreferences.DisableCustomCursors || !ToolBase.IsEditorNearestControl || ToolUtility.ActiveTileSystem.Locked) {
                return;
            }

            // Do not attempt to show custom cursor whilst alt key is being held because
            // this causes Unity to flash between the custom cursor and Unity's view cursor.
            if (Event.current.alt) {
                return;
            }

            var tool = ToolManager.Instance.CurrentTool;
            if (tool == null) {
                return;
            }

            if (Application.platform == RuntimePlatform.OSXEditor) {
                // Force basic Unity cursor to workaround bug (Case 605226) on OS X.
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.Arrow, this.sceneControlID);
            }

            if (GUIUtility.hotControl == 0 || GUIUtility.hotControl == this.sceneControlID) {
                CursorInfo cursor = tool.Cursor;

                if (cursor.Type == MouseCursor.CustomCursor) {
                    // Can only show custom cursor if custom texture was specified.
                    if (cursor.Texture != null) {
                        Cursor.SetCursor(cursor.Texture, cursor.Hotspot, CursorMode.Auto);
                    }
                    else {
                        return;
                    }
                }

                // Do not bother defining cursor rectangle if arrow cursor has been specified
                // since this is the default anyhow.
                if (cursor.Type != MouseCursor.Arrow) {
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), cursor.Type, this.sceneControlID);
                }
            }
        }

        private static ToolBase s_AnchorTool;

        private void TrackMouseInput()
        {
            Event e = Event.current;

            // Refresh status of left/right mouse button when mouse is not being dragged
            // or stop dragging when middle mouse button is pressed.
            if (e.button > 1 || this.toolEvent.Type == EventType.MouseMove) {
                this.toolEvent.IsRightButtonPressed = this.toolEvent.IsLeftButtonPressed = false;
            }

            switch (this.toolEvent.Type) {
                case EventType.MouseDown:
                    if (!this.UpdateToolEventFromCursor(e.mousePosition)) {
                        break;
                    }

                    if (ToolManager.Instance.CurrentTool != null && GUIUtility.hotControl == 0) {
                        this.toolEvent.IsRightButtonPressed = this.toolEvent.IsLeftButtonPressed = false;

                        // Note: Do not allow use of control or shift with right button
                        //       because otherwise it would not be possible to pan and
                        //       zoom viewport!

                        if ((e.button == 0 || e.button == 1) && !e.alt && (!e.control || e.shift || e.button == 0)) {
                            if (e.button == 0) {
                                this.toolEvent.IsLeftButtonPressed = true;
                            }
                            else if (e.button == 1) {
                                this.toolEvent.IsRightButtonPressed = true;
                            }

                            GUIUtility.hotControl = this.sceneControlID;
                            s_AnchorTool = ToolManager.Instance.CurrentTool;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    this.toolEvent.IsRightButtonPressed = this.toolEvent.IsLeftButtonPressed = false;
                    if (GUIUtility.hotControl == this.sceneControlID) {
                        GUIUtility.hotControl = 0;
                        s_AnchorTool = null;
                    }
                    break;

                case EventType.MouseMove:
                case EventType.MouseDrag:
                    this.UpdateToolEventFromCursor(e.mousePosition);
                    break;
            }
        }

        private void DoTool(ToolBase tool, bool isMouseEvent, bool wasSceneActiveControl)
        {
            var tileSystem = target as TileSystem;

            this.toolEvent.MousePointerTileIndex = this.toolEvent.LastMousePointerTileIndex;

            if (!EditorApplication.isPlaying) {
                // Mark the current scene as being dirty.
                EditorSceneManager.MarkSceneDirty(tileSystem.gameObject.scene);
            }

            // Allow tool to manipulate event data if desired.
            tool.OnRefreshToolEvent(this.toolEvent, this);
            ToolUtility.ActiveTileIndex = this.toolEvent.MousePointerTileIndex;

            // Use currently selected tool!
            this.DoToolSceneGUI(tool);

            if (isMouseEvent) {
                // Tools cannot interact with locked tile systems!
                if (!tileSystem.Locked) {
                    // Allow tool to respond to all mouse events.
                    bool isSceneActiveControl = GUIUtility.hotControl == this.sceneControlID;
                    if (wasSceneActiveControl || isSceneActiveControl) {
                        tool.OnTool(this.toolEvent, this);

                        switch (this.toolEvent.Type) {
                            case EventType.MouseDown:
                            case EventType.MouseDrag:
                                if (isSceneActiveControl) {
                                    Event.current.Use();
                                }
                                break;

                            case EventType.MouseUp:
                                if (wasSceneActiveControl) {
                                    tool.OnToolInactive(this.toolEvent, this);
                                    Event.current.Use();
                                }
                                break;
                        }
                    }
                    else {
                        tool.OnToolInactive(this.toolEvent, this);
                    }
                }

                // Force redraw in scene views.
                SceneView.RepaintAll();
                // Force redraw in game views.
                EditorInternalUtility.RepaintAllGameViews();
            }
        }

        private void DoToolSceneGUI(ToolBase tool)
        {
            var tileSystem = target as TileSystem;

            // Toggle preview material using control key.
            this.CheckSwitchImmediatePreviewMaterial();

            ToolUtility.CheckToolKeyboardShortcuts();

            // Preserve current state of handles.
            Matrix4x4 originalMatrix = Handles.matrix;
            Color restoreHandleColor = Handles.color;

            // Place handles within local space of tile system.
            Handles.matrix = tileSystem.transform.localToWorldMatrix;

            // Tools cannot interact with locked tile systems!
            if (!tileSystem.Locked) {
                tool.OnSceneGUI(this.toolEvent, this);
            }
            else {
                Vector3 activeCenter = Vector3.zero;
                activeCenter.x += this.toolEvent.MousePointerTileIndex.column * tileSystem.CellSize.x + tileSystem.CellSize.x / 2f;
                activeCenter.y -= this.toolEvent.MousePointerTileIndex.row * tileSystem.CellSize.y + tileSystem.CellSize.y / 2f;

                ToolHandleUtility.DrawWireBox(activeCenter, tileSystem.CellSize);
            }

            // Restore former state of handles.
            Handles.matrix = originalMatrix;
            Handles.color = restoreHandleColor;
        }

        private bool UpdateToolEventFromCursor(Vector2 mousePosition)
        {
            var tileSystem = target as TileSystem;

            // Find mouse position in 3D space.
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            // Store screen position of mouse pointer.
            this.toolEvent.MousePointerScreenPoint = mousePosition;

            Plane systemPlane = new Plane(tileSystem.transform.forward, tileSystem.transform.position);

            // Calculate point where mouse ray intersect tile system plane.
            float distanceToPlane = 0f;
            if (!systemPlane.Raycast(mouseRay, out distanceToPlane)) {
                return false;
            }

            Vector3 cellSize = tileSystem.CellSize;

            // Calculate world position of cursor in local space of tile system.
            Vector3 worldPoint = mouseRay.GetPoint(distanceToPlane);
            Vector3 localPoint = tileSystem.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPoint);

            // Allow tool to pre-filter local point.
            // For instance, to switch alignment to grid points rather than cells.
            var tool = ToolManager.Instance.CurrentTool;
            if (tool != null) {
                localPoint = tool.PreFilterLocalPoint(localPoint);
            }

            this.toolEvent.MousePointerLocalPoint = localPoint;

            // Calculate position within grid.
            this.toolEvent.LastMousePointerTileIndex = new TileIndex(
                row: Mathf.Clamp((int)(-localPoint.y / cellSize.y), 0, tileSystem.RowCount - 1),
                column: Mathf.Clamp((int)(localPoint.x / cellSize.x), 0, tileSystem.ColumnCount - 1)
            );

            // We keep track of the last known mouse tile index so that it can be restored
            // before passing `ToolEvent` to the active tool. This is needed for non-mouse
            // events since tools are free to mutate `ToolUtility.tileIndex`.

            return true;
        }

        private void CheckSwitchImmediatePreviewMaterial()
        {
            bool isSeeThrough = Event.current.control
                ? !RtsPreferences.ToolImmediatePreviewsSeeThrough
                : RtsPreferences.ToolImmediatePreviewsSeeThrough;

            if (isSeeThrough != ImmediatePreviewUtility.IsSeeThroughPreviewMaterial) {
                ImmediatePreviewUtility.IsSeeThroughPreviewMaterial = isSeeThrough;
                this.Repaint();
            }
        }

        private GUI.WindowFunction _statusPanelWindowFunction = (windowID) => {
            if (ToolManager.Instance.CurrentTool != null) {
                ToolManager.Instance.CurrentTool.OnStatusPanelGUI();
            }
        };

        private void DrawStatusPanel()
        {
            var tileSystem = target as TileSystem;

            Handles.BeginGUI();

            if (tileSystem.IsEditable) {
                string title = tileSystem.name;
                if (tileSystem.Locked) {
                    title += " <color=yellow><b>(" + TileLang.ParticularText("Status", "Locked") + ")</b></color>";
                }

                Rect position = new Rect(0, Screen.height - 21 - 42, 380, 42);
                GUI.Window(0, position, this._statusPanelWindowFunction, title, RotorzEditorStyles.Instance.StatusWindow);
                GUI.UnfocusWindow();
            }
            else {
                Rect position = new Rect(5, Screen.height - 42 - 40, 380, 40);
                EditorGUI.HelpBox(position, TileLang.Text("Tile system has been built and can no longer be edited."), MessageType.Warning);
            }

            Handles.EndGUI();
        }

        #endregion


        #region Gizmos

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        private static void OnDrawGizmosPickable(TileSystem system, GizmoType gizmoType)
        {
            if (!Selection.Contains(system.gameObject)) {
                DrawTileSystemGizmosNotSelected(system);
                DrawTileSystemHandle(system);
            }
        }

        [DrawGizmo(GizmoType.NonSelected)]
        private static void OnDrawGizmos(TileSystem system, GizmoType gizmoType)
        {
            if (ToolUtility.ActiveTileSystem == system && RtsPreferences.ShowActiveTileSystem) {
                DrawTileSystemGizmosSelected(system, 1.0f);
            }
        }

        private static void DrawTileSystemHandle(TileSystem system)
        {
            // Apply rotation to gizmos.
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = system.transform.localToWorldMatrix;

            Vector3 tileSize = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 boundsSize = new Vector3(tileSize.x * 3, tileSize.y * 3, 0f);

            // Prepare cursor to draw rows.
            Vector3 cursor = Vector3.zero;
            Vector3 cursorEnd = Vector3.zero;

            Gizmos.color = new Color(0.35f, 0.35f, 0.35f, 0.3f);
            Gizmos.DrawCube(boundsSize / 2f + new Vector3(0f, 0f, 0.01f), boundsSize);
            Gizmos.color = new Color(0.35f, 0.35f, 0.35f, 0.5f);

            cursorEnd.x += 3 * tileSize.x;

            // Draw rows.
            for (int rowIndex = 0; rowIndex < 4; ++rowIndex) {
                Gizmos.DrawLine(cursor, cursorEnd);
                cursor.y += tileSize.y;
                cursorEnd.y += tileSize.y;
            }

            // Prepare cursor to draw columns.
            cursorEnd = cursor = Vector3.zero;
            cursorEnd.y += 3 * tileSize.y;

            // Draw columns.
            for (int columnIndex = 0; columnIndex < 4; ++columnIndex) {
                Gizmos.DrawLine(cursor, cursorEnd);
                cursor.x += tileSize.x;
                cursorEnd.x += tileSize.x;
            }

            // Restore original gizmos matrix.
            Gizmos.matrix = originalMatrix;
        }

        [DrawGizmo(GizmoType.Active)]
        private static void OnDrawGizmosSelected(TileSystem system, GizmoType gizmoType)
        {
            DrawTileSystemGizmosSelected(system, 2.0f);

            // Do not display gizmos when tile system is locked, this would be confusing!
            if (system.Locked) {
                return;
            }

            if (!RtsPreferences.ToolImmediatePreviews) {
                return;
            }

            ToolBase currentTool = ToolManager.Instance.CurrentTool;
            if (currentTool != null && ToolBase.IsEditorNearestControl) {
                ImmediatePreviewUtility.PreviewMaterial.color = RtsPreferences.ToolImmediatePreviewsTintColor;
                currentTool.OnDrawGizmos(system);
            }
        }

        private static void DrawTileSystemGizmosNotSelected(TileSystem system)
        {
            // Apply rotation to gizmos.
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = system.transform.localToWorldMatrix;

            Gizmos.color = new Color(0.35f, 0.35f, 0.35f, 0.5f);

            // Prepare cursor to draw rows.
            Vector3 cellSize = system.CellSize;
            Vector3 boundsSize = new Vector3(cellSize.x * system.ColumnCount, -cellSize.y * system.RowCount, 0f);

            // Prepare cursor to draw rows.
            Vector3 cursor = Vector3.zero;
            Vector3 cursorEnd = Vector3.zero;
            cursorEnd.x += 1 * boundsSize.x;

            // Draw rows.
            for (int rowIndex = 0; rowIndex < 2; ++rowIndex) {
                Gizmos.DrawLine(cursor, cursorEnd);
                cursor.y += boundsSize.y;
                cursorEnd.y += boundsSize.y;
            }

            // Prepare cursor to draw columns.
            cursorEnd = cursor = Vector3.zero;
            cursorEnd.y += 1 * boundsSize.y;

            // Draw columns.
            for (int columnIndex = 0; columnIndex < 2; ++columnIndex) {
                Gizmos.DrawLine(cursor, cursorEnd);
                cursor.x += boundsSize.x;
                cursorEnd.x += boundsSize.x;
            }

            // Restore original gizmos matrix.
            Gizmos.matrix = originalMatrix;
        }

        private static void DrawTileSystemGizmosSelected(TileSystem system, float alphaFactor)
        {
            Vector3 cellSize = system.CellSize;

            // Apply rotation to gizmos.
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = system.transform.localToWorldMatrix;

            if (RtsPreferences.ShowGrid) {
                Vector3 boundsSize = new Vector3(cellSize.x * system.ColumnCount, -cellSize.y * system.RowCount, 0.00f);

                bool showChunks = (RtsPreferences.ShowChunks && system.ChunkWidth != 0 && system.ChunkHeight != 0);

                Color background = RtsPreferences.BackgroundGridColor;
                Color minor = RtsPreferences.MinorGridColor;
                Color major = RtsPreferences.MajorGridColor;
                Color chunk = RtsPreferences.ChunkGridColor;

                background.a = Mathf.Min(background.a * alphaFactor, 1.0f);
                minor.a = Mathf.Min(minor.a * alphaFactor, 1.0f);
                major.a = Mathf.Min(major.a * alphaFactor, 1.0f);
                chunk.a = Mathf.Min(chunk.a * alphaFactor, 1.0f);

                Gizmos.color = background;
                Gizmos.DrawCube(boundsSize / 2f + new Vector3(0f, 0f, 0.01f), boundsSize);

                // Prepare cursor to draw rows.
                Vector3 cursor = Vector3.zero;
                Vector3 cursorEnd = Vector3.zero;
                cursorEnd.x += system.ColumnCount * cellSize.x;

                // Draw rows.
                for (int rowIndex = 0; rowIndex <= system.RowCount; ++rowIndex) {
                    Gizmos.color = (showChunks && rowIndex % system.ChunkHeight == 0)
                        ? chunk
                        : (
                            rowIndex % 10 == 0
                                ? major
                                : minor
                        );

                    Gizmos.DrawLine(cursor, cursorEnd);
                    cursor.y -= cellSize.y;
                    cursorEnd.y -= cellSize.y;
                }

                // Prepare cursor to draw columns.
                cursorEnd = cursor = Vector3.zero;
                cursorEnd.y -= system.RowCount * cellSize.y;

                // Draw columns.
                for (int columnIndex = 0; columnIndex <= system.ColumnCount; ++columnIndex) {
                    Gizmos.color = (showChunks && columnIndex % system.ChunkWidth == 0)
                        ? chunk
                        : (
                            columnIndex % 10 == 0
                                ? major
                                : minor
                        );

                    Gizmos.DrawLine(cursor, cursorEnd);
                    cursor.x += cellSize.x;
                    cursorEnd.x += cellSize.x;
                }
            }

            // Restore original gizmos matrix.
            Gizmos.matrix = originalMatrix;
        }

        #endregion
    }
}
