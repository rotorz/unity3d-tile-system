// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Custom tool cursor textures which can be used in custom tools.
    /// </summary>
    public static class ToolCursors
    {
        static ToolCursors()
        {
            var skin = RotorzEditorStyles.Skin;
            bool isWin = (Application.platform == RuntimePlatform.WindowsEditor);

            Brush = new CursorInfo(skin.Cursor_Brush, 5, 5);
            Cycle = new CursorInfo(skin.Cursor_Cycle, 5, 5);
            Picker = new CursorInfo(skin.Cursor_Picker, 4, 17);
            Fill = new CursorInfo(skin.Cursor_Fill, 5, 14);
            Line = new CursorInfo(isWin ? skin.Cursor_Line_Win : skin.Cursor_Line_Mac, 5, 5);
            Rectangle = new CursorInfo(isWin ? skin.Cursor_Rectangle_Win : skin.Cursor_Rectangle_Mac, 5, 5);
            Spray = new CursorInfo(skin.Cursor_Spray, 4, 4);
            Plop = new CursorInfo(skin.Cursor_Plop, 6, 19);
            PlopCycle = new CursorInfo(skin.Cursor_PlopCycle, 6, 19);
        }


        /// <summary>
        /// Gets texture for 'Brush' cursor.
        /// </summary>
        public static CursorInfo Brush { get; private set; }

        /// <summary>
        /// Gets texture for 'Cycle' cursor.
        /// </summary>
        public static CursorInfo Cycle { get; private set; }

        /// <summary>
        /// Gets texture for 'Picker' cursor.
        /// </summary>
        public static CursorInfo Picker { get; private set; }

        /// <summary>
        /// Gets texture for 'Fill' cursor.
        /// </summary>
        public static CursorInfo Fill { get; private set; }

        /// <summary>
        /// Gets texture for 'Line' cursor.
        /// </summary>
        public static CursorInfo Line { get; private set; }

        /// <summary>
        /// Gets texture for 'Rectangle' cursor.
        /// </summary>
        public static CursorInfo Rectangle { get; private set; }

        /// <summary>
        /// Gets texture for 'Spray' cursor.
        /// </summary>
        public static CursorInfo Spray { get; private set; }

        /// <summary>
        /// Gets texture for 'Plop' cursor.
        /// </summary>
        public static CursorInfo Plop { get; private set; }

        /// <summary>
        /// Gets texture for 'Plop Cycle' cursor.
        /// </summary>
        public static CursorInfo PlopCycle { get; private set; }
    }
}
