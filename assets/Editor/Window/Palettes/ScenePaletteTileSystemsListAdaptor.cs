// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.Collections;
using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    internal sealed class ScenePaletteTileSystemsListAdaptor : IReorderableListAdaptor, IReorderableListDropTarget
    {
        private const float DragThresholdInPixels = 6;

        private EditorWindow parentWindow;
        private List<ScenePaletteEntry> entries;
        private Rect[] itemPositions;

        private Vector2 mouseDownPosition;


        public ScenePaletteTileSystemsListAdaptor(EditorWindow parentWindow, List<ScenePaletteEntry> entries)
        {
            this.parentWindow = parentWindow;
            this.entries = entries;

            this.CanBeginEditingNameOnMouseUp = true;
        }


        public int Count {
            get { return this.entries.Count; }
        }

        public bool CanDrag(int index)
        {
            return !this.entries[index].IsHeader;
        }


        public void BeginGUI()
        {
            if (this.IsEditingName) {
                // Accept new name if clicking outside of text field.
                if (Event.current.type == EventType.MouseDown && !this.renameFieldRect.Contains(Event.current.mousePosition)) {
                    this.EndEditingName(true);
                    this.RepaintParentWindow();
                    GUIUtility.ExitGUI();
                }
                // Cancel rename when undo/redo is performed.
                if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed") {
                    this.EndEditingName(false);
                    this.RepaintParentWindow();
                    GUIUtility.ExitGUI();
                }
            }
            else {
                // Do not process keyboard input during rename mode.
                this.OnKeyboardGUI();
            }
        }

        public void EndGUI()
        {
        }

        public void DrawItemBackground(Rect position, int index)
        {
            var entry = this.entries[index];

            if (!entry.IsHeader && entry.TileSystem == ToolUtility.ActiveTileSystem) {
                Color restoreColor = GUI.color;
                GUI.color = ReorderableListStyles.Instance.HorizontalLineColor;
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);
                GUI.color = restoreColor;
            }
        }

        private bool _wantsToBeginEditingName;

        public void DrawItem(Rect position, int index)
        {
            var entry = this.entries[index];
            if (entry.IsHeader) {
                this.DrawItem_Header(position, index, entry.Scene);
            }
            else {
                this.DrawItem_TileSystem(position, index, entry.TileSystem);
            }
        }

        private void DrawItem_Header(Rect position, int index, Scene scene)
        {
            int horizontalOffset = 3;
            int verticalReduction = 2;
            int verticalOffset = 3 + verticalReduction;

            position.x -= horizontalOffset;
            position.y += verticalOffset;
            position.width += horizontalOffset * 2;
            position.height -= verticalReduction;


            string label = scene.name;

            // Make label bold when this is the active scene.
            if (scene == EditorSceneManager.GetActiveScene()) {
                label = string.Format("<b>{0}</b>", label);
            }

            // Highlight with asterisk if the scene has been modified in some way.
            if (scene.isDirty) {
                label += "*";
            }


            GUI.Box(position, label, ReorderableListStyles.Instance.Title);
        }

        private void DrawItem_TileSystem(Rect position, int index, TileSystem system)
        {
            bool isActiveSystem = system == ToolUtility.ActiveTileSystem;

            int itemControlID = GUIUtility.GetControlID(FocusType.Passive);

            Rect totalPosition = ReorderableListGUI.CurrentItemTotalPosition;

            if (this.itemPositions == null || this.itemPositions.Length != this.entries.Count) {
                this.itemPositions = new Rect[this.entries.Count];
            }
            this.itemPositions[index] = ReorderableListGUI.CurrentItemTotalPosition;

            Rect eyeButtonPosition = new Rect(position.x, totalPosition.y + (totalPosition.height - 18) / 2f, 21, 18);
            int eyeButtonControlID = GUIUtility.GetControlID(FocusType.Passive);

            Rect totalLabelPosition = new Rect(position.x, totalPosition.y, totalPosition.xMax - position.x, totalPosition.height);
            totalLabelPosition.x += eyeButtonPosition.width + 3;
            totalLabelPosition.width -= eyeButtonPosition.width + 3;

            Rect labelPosition = totalLabelPosition;
            labelPosition.height -= 1;

            var eventType = Event.current.GetTypeForControl(itemControlID);
            if (eventType == EventType.Repaint) {
                GUI.DrawTexture(eyeButtonPosition, system.gameObject.activeInHierarchy ? RotorzEditorStyles.Skin.EyeOpen : RotorzEditorStyles.Skin.EyeShut);

                string systemName = this.IsEditingName && this.renameIndex == index ? "" : system.name;
                RotorzEditorStyles.Instance.ListLargeElement.Draw(labelPosition, systemName, false, false, isActiveSystem, false);

                if (system.Locked) {
                    Rect lockIconPosition = new Rect(labelPosition.xMax - 28, labelPosition.y + 5, 12, 15);
                    GUI.DrawTexture(lockIconPosition, isActiveSystem ? RotorzEditorStyles.Skin.LockActive : RotorzEditorStyles.Skin.Lock);
                }
            }

            if (isActiveSystem) {
                this.RenameField(labelPosition, system);
            }

            if (this.IsEditingName) {
                return;
            }

            switch (eventType) {
                case EventType.MouseDown:
                    if (Event.current.button == 0) {
                        if (eyeButtonPosition.Contains(Event.current.mousePosition)) {
                            this._wantsToBeginEditingName = false;

                            Undo.RecordObject(system.gameObject, TileLang.ParticularText("Action", "Toggle Visibility"));
                            system.gameObject.SetActive(!system.gameObject.activeSelf);
                            Event.current.Use();
                            break;
                        }

                        bool wasTileSystemAlreadySelected = isActiveSystem || (Selection.activeGameObject == system.gameObject && Selection.objects.Length == 1);
                        if (!wasTileSystemAlreadySelected) {
                            if (Event.current.clickCount == 1 && totalPosition.Contains(Event.current.mousePosition)) {
                                // Ensure that tile system is selected.
                                ToolUtility.SelectTileSystem(system);
                            }
                        }

                        if (totalLabelPosition.Contains(Event.current.mousePosition)) {
                            this._wantsToBeginEditingName = false;

                            if (Event.current.clickCount == 2) {
                                Selection.objects = new Object[] { system.gameObject };
                                SceneView.lastActiveSceneView.FrameSelected();
                            }
                            else {
                                this.mouseDownPosition = Event.current.mousePosition;

                                // Should game object of tile system be selected?
                                if (wasTileSystemAlreadySelected) {
                                    this._wantsToBeginEditingName = this.CanBeginEditingNameOnMouseUp;
                                }

                                GUIUtility.hotControl = itemControlID;
                                GUIUtility.keyboardControl = 0;
                            }

                            Event.current.Use();
                        }
                    }
                    else if (Event.current.button == 1) {
                        if (totalPosition.Contains(Event.current.mousePosition)) {
                            ToolUtility.SelectTileSystem(system);
                            Event.current.Use();

                            GUIUtility.hotControl = itemControlID;
                            GUIUtility.keyboardControl = 0;
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == itemControlID && Event.current.button == 0) {
                        if (Vector2.Distance(this.mouseDownPosition, Event.current.mousePosition) >= DragThresholdInPixels) {
                            GUIUtility.hotControl = 0;
                            this.StartDrag(system);
                        }
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == itemControlID || GUIUtility.hotControl == eyeButtonControlID) {
                        if (totalPosition.Contains(Event.current.mousePosition)) {
                            if (Event.current.button == 0 && GUIUtility.hotControl == itemControlID) {
                                // Consider entering rename mode?
                                if (isActiveSystem && this._wantsToBeginEditingName) {
                                    CallbackSchedulerUtility.SetTimeout(() => {
                                        if (this._wantsToBeginEditingName) {
                                            this.BeginEditingName(system);
                                        }
                                    }, 0.5);
                                }
                            }
                            else if (Event.current.button == 1) {
                                this.ShowContextMenu(system);
                            }
                        }

                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;
            }
        }

        public float GetItemHeight(int index)
        {
            return 23f;
        }

        public void Move(int sourceIndex, int destIndex)
        {
            if (sourceIndex == destIndex) {
                return;
            }

            var movedEntry = this.entries[sourceIndex];
            if (movedEntry.IsHeader) {
                return;
            }

            //!TODO: Fix/adjust scene order header offset here?
            int offsetIndex = 1 + this.entries.FindIndex(entry => entry.IsHeader && entry.Scene == movedEntry.Scene);
            destIndex -= offsetIndex;

            // Adjust scene order of tile systems.
            Undo.RecordObject(movedEntry.TileSystem, TileLang.ParticularText("Action", "Reorder Tile Systems"));
            foreach (var entry in this.entries) {
                if (entry.IsHeader) {
                    continue;
                }
                if (entry.Scene != movedEntry.Scene) {
                    continue;
                }
                if (entry.TileSystem.sceneOrder >= destIndex) {
                    Undo.RecordObject(entry.TileSystem, TileLang.ParticularText("Action", "Reorder Tile Systems"));
                    ++entry.TileSystem.sceneOrder;
                }
            }
            movedEntry.TileSystem.sceneOrder = destIndex;

            EditorTileSystemUtility.ApplySceneOrders(
                from x in this.entries
                where !x.IsHeader
                orderby x.SceneOrder
                select x.TileSystem
            );
        }


        #region IReorderableListAdaptor - Non-Implemented Members

        void IReorderableListAdaptor.Insert(int index)
        {
            throw new NotImplementedException();
        }

        void IReorderableListAdaptor.Add()
        {
            throw new NotImplementedException();
        }

        void IReorderableListAdaptor.Duplicate(int index)
        {
            throw new NotImplementedException();
        }

        bool IReorderableListAdaptor.CanRemove(int index)
        {
            return false;
        }

        void IReorderableListAdaptor.Remove(int index)
        {
            throw new NotImplementedException();
        }

        void IReorderableListAdaptor.Clear()
        {
            throw new NotImplementedException();
        }

        #endregion


        private TileSystem DraggedTileSystem {
            get { return DragAndDrop.objectReferences.FirstOrDefault() as TileSystem; }
        }

        public bool CanDropInsert(int insertionIndex)
        {
            if (!ExtraEditorGUI.VisibleRect.Contains(Event.current.mousePosition)) {
                return false;
            }

            var tileSystem = this.DraggedTileSystem;
            return tileSystem != null && !EditorUtility.IsPersistent(tileSystem);
        }

        public void ProcessDropInsertion(int insertionIndex)
        {
            if (Event.current.type == EventType.DragPerform) {
                var draggedTileSystem = this.DraggedTileSystem;

                // Can only rearrange if tile system was indeed selected!
                if (draggedTileSystem != null) {
                    // Is the tile system included in this list?
                    int draggedIndex = this.entries.FindIndex(entry => entry.TileSystem == draggedTileSystem);
                    if (draggedIndex != -1) {
                        this.Move(draggedIndex, insertionIndex);
                    }
                }
            }
        }

        public Rect GetItemPosition(TileSystem system)
        {
            if (this.itemPositions != null) {
                int index = this.entries.FindIndex(entry => entry.TileSystem == system);
                if ((uint)index < this.itemPositions.Length) {
                    return this.itemPositions[index];
                }
            }
            return default(Rect);
        }


        private void StartDrag(TileSystem system)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new Object[] { system };
            DragAndDrop.paths = new string[0];
            DragAndDrop.StartDrag(TileLang.FormatDragObjectTitle(system.name, typeof(TileSystem).Name));
        }


        //!TODO: The following should be refactored so that the tile system is passed in
        //       as the context object rather than using a closure.
        private EditorMenu BuildContextMenu(TileSystem system)
        {
            var contextMenu = new EditorMenu();

            contextMenu.AddCommand(TileLang.ParticularText("Action", "Inspect"))
                .Action(() => {
                    EditorInternalUtility.FocusInspectorWindow();
                });

            contextMenu.AddSeparator();

            contextMenu.AddCommand(TileLang.ParticularText("Action", "Rename"))
                .Action(() => {
                    this.BeginEditingName(system);
                });

            contextMenu.AddCommand(TileLang.ParticularText("Action", "Lock"))
                .Checked(system.Locked)
                .Action(() => {
                    Undo.RecordObject(system, system.Locked
                        ? TileLang.ParticularText("Action", "Unlock Tile System")
                        : TileLang.ParticularText("Action", "Lock Tile System"));
                    system.Locked = !system.Locked;
                    EditorUtility.SetDirty(system);
                    ToolUtility.RepaintScenePalette();
                });

            contextMenu.AddSeparator();

            contextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Refresh Tiles")))
                .Enabled(!system.Locked)
                .Action(() => {
                    TileSystemCommands.Command_Refresh(system);
                });

            contextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Repair Tiles")))
                .Enabled(!system.Locked)
                .Action(() => {
                    TileSystemCommands.Command_Repair(system);
                });

            contextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Clear Tiles")))
                .Enabled(!system.Locked)
                .Action(() => {
                    TileSystemCommands.Command_Clear(system);
                });

            //contextMenu.AddSeparator();

            //contextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Refresh Plops")))
            //    .Enabled(!system.Locked)
            //    .Action(() => {
            //        TileSystemCommands.Command_RefreshPlops(system);
            //    });

            //contextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Clear Plops")))
            //    .Enabled(!system.Locked)
            //    .Action(() => {
            //        TileSystemCommands.Command_ClearPlops(system);
            //    });

            contextMenu.AddSeparator();

            contextMenu.AddCommand(TileLang.ParticularText("Action", "Delete"))
                .Enabled(!system.Locked)
                .Action(() => {
                    Undo.DestroyObjectImmediate(system.gameObject);
                });

            contextMenu.AddSeparator();

            contextMenu.AddCommand(TileLang.OpensWindow(TileLang.ParticularText("Action", "Build Prefab")))
                .Action(() => {
                    TileSystemCommands.Command_BuildPrefab(system);
                });

            return contextMenu;
        }

        private void ShowContextMenu(TileSystem system)
        {
            var menu = this.BuildContextMenu(system);
            menu.ShowAsContext();
        }


        private void OnKeyboardGUI()
        {
            if (Event.current.GetTypeForControl(ReorderableListControl.CurrentListControlID) != EventType.KeyDown) {
                return;
            }

            int index = this.entries.FindIndex(entry => entry.TileSystem == ToolUtility.ActiveTileSystem);
            int newIndex = index;

            switch (Event.current.keyCode) {
                case KeyCode.UpArrow:
                    if (index == -1) {
                        newIndex = this.entries.Count - 1;
                    }
                    else {
                        newIndex = Mathf.Max(0, newIndex - 1);
                    }
                    break;

                case KeyCode.DownArrow:
                    if (index == -1) {
                        newIndex = 0;
                    }
                    else {
                        newIndex = Mathf.Min(this.entries.Count - 1, newIndex + 1);
                    }
                    break;

                case KeyCode.Home:
                    newIndex = 0;
                    break;

                case KeyCode.End:
                    newIndex = this.entries.Count - 1;
                    break;

                case KeyCode.Delete:
                    if (ToolUtility.ActiveTileSystem != null && !ToolUtility.ActiveTileSystem.Locked) {
                        Undo.DestroyObjectImmediate(ToolUtility.ActiveTileSystem.gameObject);
                        this.RepaintParentWindow();
                        Event.current.Use();
                    }
                    break;
            }

            // Alter active tile system?
            if (newIndex != index) {
                if (this.entries.Count == 0) {
                    ToolUtility.SelectTileSystem(null);
                }
                else {
                    if (newIndex < 0) {
                        newIndex = this.entries.Count - 1;
                    }
                    else if (newIndex >= this.entries.Count) {
                        newIndex = 0;
                    }

                    ToolUtility.SelectTileSystem(this.entries[newIndex].TileSystem);
                }

                Event.current.Use();
            }
        }


        private void RepaintParentWindow()
        {
            this.parentWindow.Repaint();
        }


        #region Rename Field

        private int renameIndex;
        private Rect renameFieldRect;
        private string renameFieldText;


        public bool IsEditingName { get; private set; }
        public bool CanBeginEditingNameOnMouseUp { get; set; }


        public void BeginEditingName(TileSystem system)
        {
            this.renameIndex = this.entries.FindIndex(entry => entry.TileSystem == system);
            if (this.renameIndex == -1) {
                Debug.LogWarning(string.Format("Cannot rename tile system '{0}'.", system != null ? system.name : "(null)"));
                return;
            }

            this.IsEditingName = true;
            this.renameFieldText = system.name;

            this.RepaintParentWindow();
        }

        public void EndEditingName(bool accept)
        {
            if (!this.IsEditingName) {
                throw new InvalidOperationException("Not actually renaming a tile system!");
            }

            if (accept) {
                var tileSystem = this.entries[this.renameIndex].TileSystem;
                string currentName = tileSystem.gameObject.name;
                if (!string.IsNullOrEmpty(this.renameFieldText) && this.renameFieldText != currentName) {
                    string actionDescription = string.Format(
                        /* 0: current name of the tile system */
                        TileLang.ParticularText("Action", "Rename {0}"),
                        currentName
                    );
                    Undo.RecordObject(tileSystem.gameObject, actionDescription);
                    tileSystem.gameObject.name = this.renameFieldText;
                }
            }

            this.IsEditingName = false;

            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
        }

        private void RenameField(Rect position, TileSystem system)
        {
            Event current = Event.current;

            if (!this.IsEditingName) {
                if (current.type == EventType.KeyDown && (((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && Application.platform == RuntimePlatform.OSXEditor) || (current.keyCode == KeyCode.F2 && Application.platform == RuntimePlatform.WindowsEditor))) {
                    this.BeginEditingName(system);
                    current.Use();
                }
                else {
                    return;
                }
            }

            switch (current.type) {
                case EventType.KeyDown:
                    switch (current.keyCode) {
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            this.EndEditingName(true);
                            current.Use();
                            GUIUtility.ExitGUI();
                            break;

                        case KeyCode.Escape:
                            this.EndEditingName(false);
                            current.Use();
                            break;
                    }
                    break;

                case EventType.ScrollWheel:
                    // Prevent scrolling!
                    current.Use();
                    break;
            }

            float fieldHeight = EditorStyles.textField.CalcHeight(GUIContent.none, position.width);
            float offsetY = (position.height - fieldHeight) / 2f;
            this.renameFieldRect = new Rect(position.x + 3, position.y + offsetY, position.width - 6, fieldHeight);

            GUI.SetNextControlName("ScenePaletteWindowRenameField");
            this.renameFieldText = EditorGUI.TextField(this.renameFieldRect, this.renameFieldText);

            if (GUI.GetNameOfFocusedControl() != "ScenePaletteWindowRenameField") {
                EditorGUI.FocusTextInControl("ScenePaletteWindowRenameField");
                this.RepaintParentWindow();
            }
        }

        #endregion
    }
}
