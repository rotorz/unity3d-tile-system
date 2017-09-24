// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Tile.Internal;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Utility drawing functions for tool handles for <c>OnSceneGUI</c>. These utility functions
    /// are intended for use within custom tool classes.
    /// </summary>
    /// <remarks>
    /// <para>The functionality provided by this utility class should only be used within the
    /// context of <c>OnSceneGUI</c> events. This class makes use of <c>UnityEditor.Handles</c>
    /// to draw within scene views.</para>
    /// </remarks>
    public static class ToolHandleUtility
    {
        #region Wireframe Box / Cube

        /// <summary>
        /// Draw wire box handle.
        /// </summary>
        /// <remarks>
        /// <para>Back edges are drawn with less opacity to make it easier to distinguish
        /// between front and back of box.</para>
        /// </remarks>
        /// <param name="position">Position to draw indicator (in local space of tile system).</param>
        /// <param name="size">Size of box.</param>
        /// <param name="color">Color of wireframe.</param>
        public static void DrawWireBox(Vector3 position, Vector3 size, Color color)
        {
            Vector3 halfSize = size * 0.5f;

            Vector3 min = position - halfSize;
            Vector3 max = position + halfSize;

            Handles.color = color;

            // Front
            Handles.DrawLine(min, new Vector3(min.x, max.y, min.z));
            Handles.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z));
            Handles.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z));
            Handles.DrawLine(new Vector3(max.x, min.y, min.z), min);

            // Edges
            Handles.color = new Color(color.r, color.g, color.b, color.a - 0.17f);
            Handles.DrawLine(min, new Vector3(min.x, min.y, max.z));
            Handles.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z));
            Handles.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z));
            Handles.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));

            // Back
            Handles.color = new Color(color.r, color.g, color.b, color.a - 0.3f);
            Handles.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z));
            Handles.DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z));
            Handles.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(max.x, min.y, max.z));
            Handles.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z));
        }

        /// <summary>
        /// Draw wire box handle using wireframe color from user preferences.
        /// </summary>
        /// <inheritdoc cref="DrawWireBox(Vector3, Vector3, Color)"/>
        public static void DrawWireBox(Vector3 position, Vector3 size)
        {
            DrawWireBox(position, size, RtsPreferences.ToolWireframeColor);
        }

        /// <summary>
        /// Draw wire cube handle one local unit in size.
        /// </summary>
        /// <inheritdoc cref="DrawWireBox(Vector3, Vector3, Color)"/>
        public static void DrawWireCube(Vector3 position, Color color)
        {
            DrawWireBox(position, Vector3.one, color);
        }

        /// <summary>
        /// Draw wire cube handle one local unit in size using wireframe color from user preferences.
        /// </summary>
        /// <inheritdoc cref="DrawWireBox(Vector3, Vector3, Color)"/>
        public static void DrawWireCube(Vector3 position)
        {
            DrawWireBox(position, Vector3.one, RtsPreferences.ToolWireframeColor);
        }

        #endregion


        #region Lines

        /// <summary>
        /// Draw line between two points with sphere cap at each end.
        /// </summary>
        /// <param name="point1">First point of line.</param>
        /// <param name="point2">Second point of line.</param>
        /// <param name="color">Color of line.</param>
        public static void DrawLineHandles(Vector3 point1, Vector3 point2, Color color)
        {
            var restoreColor = Handles.color;
            Handles.color = color;

            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlID);

            Handles.SphereHandleCap(controlID, point1, Quaternion.identity, 0.2f, eventType);
            Handles.SphereHandleCap(controlID, point2, Quaternion.identity, 0.2f, eventType);
            Handles.DrawLine(point1, point2);

            Handles.color = restoreColor;
        }

        /// <summary>
        /// Draw line between two points with sphere cap at each end. This overload assumes the
        /// user configurable "Wireframe" color preference.
        /// </summary>
        /// <inheritdoc cref="DrawLineHandles(Vector3, Vector3, Color)"/>
        public static void DrawLineHandles(Vector3 point1, Vector3 point2)
        {
            DrawLineHandles(point1, point2, RtsPreferences.ToolWireframeColor);
        }

        #endregion


        #region Rectangles

        private static Vector3[] s_RectangleVerts = new Vector3[4];

        /// <summary>
        /// Draw rectangle between anchor and target points.
        /// </summary>
        /// <param name="system">The tile system.</param>
        /// <param name="anchor">Anchor point of rectangle.</param>
        /// <param name="target">Target point of rectangle.</param>
        /// <param name="uniform">A value of <c>true</c> indicates that rectangle should be uniform (i.e. square).</param>
        /// <param name="fillCenter">A value of <c>false</c> indicates that only bordering tiles should be represented.</param>
        /// <param name="faceColor">Color to fill inner area of rectangle with.</param>
        /// <param name="outlineColor">Color to draw outline of rectangle with.</param>
        private static void DrawRectangleHelper(TileSystem system, TileIndex anchor, TileIndex target, bool uniform, bool fillCenter, Color faceColor, Color outlineColor)
        {
            TileIndex from, to;
            MathUtility.GetRectangleBounds(anchor, target, out from, out to, uniform);

            Vector3 cellSize = system.CellSize;

            // Vertices are transformed so 1 == tileSize.
            float anchorX = from.column * cellSize.x + 0f;
            float anchorY = -from.row * cellSize.y - 0f;
            float targetX = to.column * cellSize.x + cellSize.x;
            float targetY = -to.row * cellSize.y - cellSize.y;

            s_RectangleVerts[0] = new Vector3(anchorX, anchorY, 0);
            s_RectangleVerts[1] = new Vector3(targetX, anchorY, 0);
            s_RectangleVerts[2] = new Vector3(targetX, targetY, 0);
            s_RectangleVerts[3] = new Vector3(anchorX, targetY, 0);

            var restoreColor = Handles.color;
            Handles.color = Color.white;

            // Darken center area when "Fill Center" is disabled.
            // Note: Only do this when selected bounds span at least two tiles.
            if (!fillCenter && from != to) {
                Color transparent = new Color(0f, 0f, 0f, 0f);

                // Draw outer outline.
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, transparent, outlineColor);

                // Draw inner outline.
                s_RectangleVerts[0] = new Vector3(anchorX + cellSize.x, anchorY - cellSize.y, 0);
                s_RectangleVerts[1] = new Vector3(targetX - cellSize.x, anchorY - cellSize.y, 0);
                s_RectangleVerts[2] = new Vector3(targetX - cellSize.x, targetY + cellSize.y, 0);
                s_RectangleVerts[3] = new Vector3(anchorX + cellSize.x, targetY + cellSize.y, 0);
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, transparent, outlineColor);

                // Shader between outlines.
                s_RectangleVerts[0] = new Vector3(anchorX, anchorY, 0);
                s_RectangleVerts[1] = new Vector3(targetX, anchorY, 0);
                s_RectangleVerts[2] = new Vector3(targetX, anchorY - cellSize.y, 0);
                s_RectangleVerts[3] = new Vector3(anchorX, anchorY - cellSize.y, 0);
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, faceColor, transparent);

                s_RectangleVerts[0] = new Vector3(anchorX, anchorY - cellSize.y, 0);
                s_RectangleVerts[1] = new Vector3(anchorX + cellSize.x, anchorY - cellSize.y, 0);
                s_RectangleVerts[2] = new Vector3(anchorX + cellSize.x, targetY + cellSize.y, 0);
                s_RectangleVerts[3] = new Vector3(anchorX, targetY + cellSize.y, 0);
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, faceColor, transparent);

                s_RectangleVerts[0] = new Vector3(targetX - cellSize.x, anchorY - cellSize.y, 0);
                s_RectangleVerts[1] = new Vector3(targetX, anchorY - cellSize.y, 0);
                s_RectangleVerts[2] = new Vector3(targetX, targetY + cellSize.y, 0);
                s_RectangleVerts[3] = new Vector3(targetX - cellSize.x, targetY + cellSize.y, 0);
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, faceColor, transparent);

                s_RectangleVerts[0] = new Vector3(anchorX, targetY + cellSize.y, 0);
                s_RectangleVerts[1] = new Vector3(targetX, targetY + cellSize.y, 0);
                s_RectangleVerts[2] = new Vector3(targetX, targetY, 0);
                s_RectangleVerts[3] = new Vector3(anchorX, targetY, 0);
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, faceColor, transparent);
            }
            else {
                // Draw shaded outline.
                Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, faceColor, outlineColor);
            }

            // Draw small thumb square in upper left corner of selected bounds.
            float thumbSize = 0.2f;

            float temp;
            if (target.row < anchor.row) {
                temp = anchorY;
                anchorY = targetY + thumbSize;
                targetY = temp;
            }
            if (target.column < anchor.column) {
                temp = anchorX;
                anchorX = targetX - thumbSize;
                targetX = temp;
            }

            s_RectangleVerts[0] = new Vector3(anchorX, anchorY, 0);
            s_RectangleVerts[1] = new Vector3(anchorX + thumbSize, anchorY, 0);
            s_RectangleVerts[2] = new Vector3(anchorX + thumbSize, anchorY - thumbSize, 0);
            s_RectangleVerts[3] = new Vector3(anchorX, anchorY - thumbSize, 0);

            Handles.DrawSolidRectangleWithOutline(s_RectangleVerts, faceColor, faceColor);

            Handles.color = restoreColor;
        }

        /// <inheritdoc cref="DrawRectangleHelper(TileSystem, TileIndex, TileIndex, bool, bool, Color, Color)"/>
        public static void DrawRectangle(TileSystem system, TileIndex anchor, TileIndex target, bool uniform, Color faceColor, Color outlineColor)
        {
            DrawRectangleHelper(system, anchor, target, uniform, true, faceColor, outlineColor);
        }
        /// <inheritdoc cref="DrawRectangleHelper(TileSystem, TileIndex, TileIndex, bool, bool, Color, Color)"/>
        public static void DrawRectangle(TileSystem system, TileIndex anchor, TileIndex target, bool uniform)
        {
            DrawRectangleHelper(system, anchor, target, uniform, true, RtsPreferences.ToolShadedColor, RtsPreferences.ToolWireframeColor);
        }

        /// <summary>
        /// Draw rectangle between anchor and target points but only represent tiles which
        /// border the rectangle.
        /// </summary>
        /// <inheritdoc cref="DrawRectangleHelper(TileSystem, TileIndex, TileIndex, bool, bool, Color, Color)"/>
        public static void DrawRectangleBorder(TileSystem system, TileIndex anchor, TileIndex target, bool uniform, Color faceColor, Color outlineColor)
        {
            DrawRectangleHelper(system, anchor, target, uniform, false, faceColor, outlineColor);
        }
        /// <summary>
        /// Draw rectangle between anchor and target points but only represent tiles which
        /// border the rectangle.
        /// </summary>
        /// <inheritdoc cref="DrawRectangleHelper(TileSystem, TileIndex, TileIndex, bool, bool, Color, Color)"/>
        public static void DrawRectangleBorder(TileSystem system, TileIndex anchor, TileIndex target, bool uniform)
        {
            DrawRectangleHelper(system, anchor, target, uniform, false, RtsPreferences.ToolShadedColor, RtsPreferences.ToolWireframeColor);
        }

        #endregion


        #region Nozzle Indicator

        /// <summary>
        /// Vertex buffer which is used by <see cref="DrawNozzleIndicatorSmoothRadius(Vector3, BrushNozzle, float, Color, Color)"/>
        /// when drawing flat nozzle square.
        /// </summary>
        private static Vector3[] s_SquareNozzleVerts = new Vector3[4];

        /// <summary>
        /// Draw nozzle indicator to match specified radius.
        /// </summary>
        /// <param name="position">Position to draw indicator (in local space of tile system).</param>
        /// <param name="nozzle">Type of brush nozzle.</param>
        /// <param name="radius">Radius of nozzle (in local space of tile system).</param>
        /// <param name="faceColor">Color of shaded face.</param>
        /// <param name="outlineColor">Color of wire outline.</param>
        public static void DrawNozzleIndicatorSmoothRadius(Vector3 position, BrushNozzle nozzle, float radius, Color faceColor, Color outlineColor)
        {
            if (nozzle == BrushNozzle.Round) {
                Handles.color = faceColor;
                Handles.DrawSolidDisc(position, Vector3.forward, radius);
                Handles.color = outlineColor;
                Handles.DrawWireDisc(position, Vector3.forward, radius);
            }
            else {
                Handles.color = Color.white;

                s_SquareNozzleVerts[0] = new Vector3(position.x - radius, position.y + radius, 0);
                s_SquareNozzleVerts[1] = new Vector3(position.x + radius, position.y + radius, 0);
                s_SquareNozzleVerts[2] = new Vector3(position.x + radius, position.y - radius, 0);
                s_SquareNozzleVerts[3] = new Vector3(position.x - radius, position.y - radius, 0);

                Handles.DrawSolidRectangleWithOutline(s_SquareNozzleVerts, faceColor, outlineColor);
            }
        }

        /// <summary>
        /// Draw nozzle indicator to match specified radius using shading and wireframe
        /// color from user preferences.
        /// </summary>
        /// <inheritdoc cref="DrawNozzleIndicatorSmoothRadius(Vector3, BrushNozzle, float, Color, Color)"/>
        public static void DrawNozzleIndicatorSmoothRadius(Vector3 position, BrushNozzle nozzle, float radius)
        {
            DrawNozzleIndicatorSmoothRadius(position, nozzle, radius, RtsPreferences.ToolShadedColor, RtsPreferences.ToolWireframeColor);
        }

        #endregion
    }
}
