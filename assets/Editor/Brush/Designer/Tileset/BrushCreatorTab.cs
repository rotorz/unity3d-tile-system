// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor.Internal
{
    internal class BrushCreatorTab : ITilesetDesignerTab
    {
        #region User Settings

        static BrushCreatorTab()
        {
            var settings = AssetSettingManagement.GetGroup("Designer.Tileset.BrushCreator");

            ExpandAutotileBrushes = settings.Fetch<bool>("ExpandAutotileBrushes", true);
            ExpandTilesetBrushes = settings.Fetch<bool>("ExpandTilesetBrushes", true);
        }


        /// <summary>
        /// User preference indicating whether "Autotile Brushes" section of default
        /// brush properties should be expanded.
        /// </summary>
        private static Setting<bool> ExpandAutotileBrushes { get; set; }
        /// <summary>
        /// User preference indicating whether "Tileset Brushes" section of default
        /// brush properties should be expanded.
        /// </summary>
        private static Setting<bool> ExpandTilesetBrushes { get; set; }

        #endregion


        private TilesetDesigner designer;

        public TilesetAssetRecord tilesetRecord;
        public Tileset tileset;

        public BrushCreatorTab(TilesetDesigner designer)
        {
            this.designer = designer;
        }

        public string Label {
            get { return TileLang.Text("Brush Creator"); }
        }

        public Vector2 ScrollPosition { get; set; }

        #region Brush creation parameters

        private TilesetBrushParams[] inputBrushParams;
        private TilesetBrushParams inputAutotileBrushParams;

        // Number of selected tiles.
        private int selectedTileCount;

        private void ResetBrushParams()
        {
            RotorzEditorGUI.ClearControlFocus();

            var brushRecords = this.tilesetRecord.BrushRecords;

            this.selectedTileCount = 0;

            this.inputAutotileBrushParams.Name = string.Empty;
            this.inputAutotileBrushParams.Count = 0;
            this.inputAutotileBrushParams.IsSelected = false;

            // Count the number of associated brushes.
            foreach (var record in brushRecords) {
                var tilesetBrush = record.Brush as TilesetBrush;
                if (tilesetBrush is AutotileBrush) {
                    ++this.inputAutotileBrushParams.Count;
                }
            }

            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].Name = string.Empty;
                this.inputBrushParams[i].Count = 0;
                this.inputBrushParams[i].IsSelected = false;

                // Count the number of associated brushes.
                foreach (var record in brushRecords) {
                    var tilesetBrush = record.Brush as TilesetBrush;
                    if (tilesetBrush == null || tilesetBrush is AutotileBrush) {
                        continue;
                    }

                    if (tilesetBrush.tileIndex == i) {
                        ++this.inputBrushParams[i].Count;
                    }
                }
            }
        }

        #endregion


        #region Brush Defaults

        private BrushVisibility inputBrushVisibility;
        private bool inputBrushStatic;
        private InheritYesNo inputBrushProcedural;

        private string inputBrushTag;
        private int inputBrushLayer;
        private int inputBrushCategoryId;

        private int inputBrushGroupNo;

        private bool inputBrushCreateEmptyContainer;
        private bool inputBrushAddCollider;
        private ColliderType inputBrushColliderType;
        private bool inputBrushAttachPrefabTick;
        private GameObject inputBrushAttachPrefab;
        private bool inputBrushApplyPrefabTransform;
        private ScaleMode inputBrushScaleMode;
        private Vector3 inputBrushCustomScale;

        private bool inputBrushSolidFlag;
        private int inputBrushFlagMask;

        private bool inputAutotileEdgeSolid;
        private bool inputAutotileEdgeCollider;
        private bool inputAutotileInnerSolid;
        private bool inputAutotileInnerCollider;
        private ColliderType inputAutotileColliderType;


        private void UseBrushDefaults()
        {
            this.inputBrushVisibility = BrushVisibility.Shown;
            this.inputBrushStatic = false;
            this.inputBrushProcedural = InheritYesNo.Inherit;

            this.inputBrushTag = "Untagged";
            this.inputBrushLayer = 0;
            this.inputBrushCategoryId = 0;

            this.inputBrushGroupNo = 0;

            this.inputBrushCreateEmptyContainer = false;
            this.inputBrushAddCollider = false;
            this.inputBrushAttachPrefabTick = false;
            this.inputBrushAttachPrefab = null;
            this.inputBrushApplyPrefabTransform = false;
            this.inputBrushScaleMode = ScaleMode.DontTouch;
            this.inputBrushCustomScale = Vector3.one;

            // Set default collider type based upon editor behavior mode.
            switch (EditorSettings.defaultBehaviorMode) {
                case EditorBehaviorMode.Mode2D:
                    this.inputBrushColliderType = ColliderType.BoxCollider2D;
                    break;
                case EditorBehaviorMode.Mode3D:
                    this.inputBrushColliderType = ColliderType.BoxCollider3D;
                    break;
            }
            this.inputAutotileColliderType = this.inputBrushColliderType;

            this.inputBrushSolidFlag = false;
            this.inputBrushFlagMask = 0;
        }

        #endregion


        #region GUI

        private static GUIStyle s_LabelStyle;
        private static GUIStyle s_ButtonStyle;

        public void OnNewTilesetRecord(TilesetAssetRecord record)
        {
            this.tilesetRecord = record;
            this.tileset = record.Tileset;

            this.inputBrushParams = new TilesetBrushParams[this.tileset.Rows * this.tileset.Columns];
            this.ResetBrushParams();
        }

        public void OnEnable()
        {
            this.UseBrushDefaults();
        }

        public void OnDisable()
        {
        }

        protected int viewColumns;
        protected int viewRows;

        private Vector2 scrollingBrushDefaults;

        private static readonly GUIContent[] _selectionToolbarLabels = new GUIContent[] {
            ControlContent.Basic(TileLang.ParticularText("Action|Select", "All")),
            ControlContent.Basic(TileLang.ParticularText("Action|Select", "None")),
            ControlContent.Basic(TileLang.ParticularText("Action|Select", "Invert")),
            ControlContent.Basic(TileLang.ParticularText("Action|Select", "Unused")),
        };

        public void OnFixedHeaderGUI()
        {
            if (s_LabelStyle == null) {
                var skin = GUI.skin;

                s_LabelStyle = new GUIStyle(skin.label);
                s_LabelStyle.stretchHeight = true;
                s_LabelStyle.padding = new RectOffset(0, 5, 1, 0);
                s_LabelStyle.alignment = TextAnchor.MiddleCenter;

                s_ButtonStyle = new GUIStyle(skin.button);
                s_ButtonStyle.padding = new RectOffset(7, 10, 0, 1);
                s_ButtonStyle.alignment = TextAnchor.MiddleCenter;
                s_ButtonStyle.fixedHeight = 22;
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            GUILayout.Label(TileLang.ParticularText("Action", "Select"), s_LabelStyle);

            switch (GUILayout.Toolbar(-1, _selectionToolbarLabels, GUILayout.Height(22))) {
                case 0:
                    this.OnSelectAllTiles();
                    GUIUtility.ExitGUI();
                    break;
                case 1:
                    this.OnSelectNoneTiles();
                    GUIUtility.ExitGUI();
                    break;
                case 2:
                    this.OnSelectInvertTiles();
                    GUIUtility.ExitGUI();
                    break;
                case 3:
                    this.OnSelectUnusedTiles();
                    GUIUtility.ExitGUI();
                    break;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Auto Name"), s_ButtonStyle)) {
                this.OnAutoName();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(TileLang.ParticularText("Action", "Clear Names"), s_ButtonStyle)) {
                this.OnClearNames();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();

            ExtraEditorGUI.Separator(marginTop: 5, marginBottom: 0);
        }

        public void OnGUI()
        {
            // Remove size of brush defaults panel.
            this.designer.viewPosition.width -= DesignerView.ExtendedPropertiesPanelWidth + 10;

            Rect position = GUILayoutUtility.GetRect(0, 1);
            position.x += 5;
            position.width -= 5;

            this.viewColumns = Mathf.Max(1, (int)this.designer.viewPosition.width / 213);
            this.viewRows = this.inputBrushParams.Length / this.viewColumns;
            if (this.inputBrushParams.Length % this.viewColumns != 0) {
                ++this.viewRows;
            }

            int wastedWidth = (int)this.designer.viewPosition.width - this.viewColumns * 212;

            float initialX = position.x;
            position.y += 4;
            position.width = 213 + wastedWidth / this.viewColumns;
            position.height = 48 + 8;

            // Begin to count the number of selected tiles.
            this.selectedTileCount = 0;

            this.DrawCreateAutotileBrushField(ref position, this.tileset as AutotileTileset);

            position.x = initialX;

            this.DrawCreateBrushFields(ref position, this.tileset);

            GUILayout.FlexibleSpace();
        }

        private static readonly int NameFieldControlHash = "EditorTextField".GetHashCode();

        /// <summary>
        /// Inactive text field which behaves like button.
        /// </summary>
        /// <param name="position">Position for button.</param>
        /// <returns>
        /// A value of <c>true</c> if clicked; otherwise <c>false</c>.
        /// </returns>
        private static bool ButtonNameField(Rect position)
        {
            int controlID = GUIUtility.GetControlID(NameFieldControlHash, FocusType.Keyboard);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        Event.current.Use();
                        return true;
                    }
                    break;

                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == controlID)
                        switch (Event.current.keyCode) {
                            case KeyCode.Space:
                            case KeyCode.Return:
                            case KeyCode.KeypadEnter:
                                Event.current.Use();
                                return true;
                        }
                    break;

                case EventType.Repaint:
                    GUI.color = new Color(1, 1, 1, 0.3f);
                    EditorStyles.textField.Draw(position, GUIContent.none, false, false, false, GUIUtility.keyboardControl == controlID);
                    GUI.color = Color.white;
                    break;
            }

            return false;
        }

        private void DrawCreateAutotileBrushField(ref Rect r, AutotileTileset tileset)
        {
            if (tileset == null) {
                return;
            }

            Color restoreBackgroundColor = GUI.backgroundColor;
            Color restoreColor = GUI.color;

            GUILayout.Space(r.height);
            ExtraEditorGUI.SeparatorLight();

            // Draw tile preview.
            if (tileset.rawTexture != null && Event.current.type == EventType.Repaint) {
                GUI.DrawTexture(new Rect(r.x, r.y, 48, 48), tileset.rawTexture);
            }

            EditorGUI.BeginChangeCheck();
            this.inputAutotileBrushParams.IsSelected = EditorGUI.ToggleLeft(new Rect(r.x + 48 + 5, r.y + 1, 70, 18), TileLang.ParticularText("Action", "Create"), this.inputAutotileBrushParams.IsSelected);
            if (EditorGUI.EndChangeCheck() && this.inputAutotileBrushParams.IsSelected) {
                EditorGUI.FocusTextInControl("AutotileNameField");
            }

            if (this.inputAutotileBrushParams.HasErrorMessage) {
                GUI.color = new Color(0.87f, 0, 0);
                GUI.Label(new Rect(r.x + 48 + 3, r.y + 25 + 10, r.width - 48 - 5, 18), this.inputAutotileBrushParams.ErrorMessage, EditorStyles.whiteMiniLabel);
                GUI.color = restoreColor;
            }

            GUI.SetNextControlName("AutotileNameField");
            Rect nameRect = new Rect(r.x + 48 + 5, r.y + 20, r.width - 48 - 10, 16);

            if (this.inputAutotileBrushParams.IsSelected) {
                // Count the number of selected tiles.
                ++this.selectedTileCount;

                if (this.inputAutotileBrushParams.HasErrorMessage) {
                    GUI.backgroundColor = new Color(1f, 0.9f, 0.9f);
                }

                this.inputAutotileBrushParams.Name = EditorGUI.TextField(nameRect, this.inputAutotileBrushParams.Name);

                GUI.backgroundColor = restoreBackgroundColor;
            }
            else {
                if (ButtonNameField(nameRect)) {
                    this.inputAutotileBrushParams.IsSelected = true;

                    EditorGUI.FocusTextInControl("AutotileNameField");
                    GUIUtility.ExitGUI();
                }
            }

            this.DrawBrushCountButton(new Rect(r.x + r.width - 40 - 5, r.y + 1, 40, 18), -1, this.inputAutotileBrushParams);

            r.y += r.height + 7;
        }

        private void DrawCreateBrushFields(ref Rect r, Tileset tileset)
        {
            var atlasTexture = tileset.AtlasTexture;

            float initialX = r.x;

            Color restoreBackgroundColor = GUI.backgroundColor;
            Color restoreColor = GUI.color;

            GUILayout.BeginVertical();
            GUILayout.Space(this.viewRows * r.height);

            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                if (i > 0 && i % this.viewColumns == 0) {
                    r.y += r.height;
                    r.x = initialX;
                }

                // Draw tile preview.
                if (atlasTexture != null && Event.current.type == EventType.Repaint) {
                    GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y, 48, 48), atlasTexture, tileset.CalculateTexCoords(i), true);
                }

                string controlName = "Tile_" + i;

                EditorGUI.BeginChangeCheck();
                bool newState = EditorGUI.ToggleLeft(new Rect(r.x + 48 + 5, r.y + 1, 70, 18), TileLang.ParticularText("Action", "Create"), this.inputBrushParams[i].IsSelected);
                if (EditorGUI.EndChangeCheck() && newState) {
                    EditorGUI.FocusTextInControl(controlName);
                }

                if (newState != this.inputBrushParams[i].IsSelected) {
                    this.inputBrushParams[i].IsSelected = newState;
                    this.inputBrushParams[i].ErrorMessage = null;
                }

                if (this.inputBrushParams[i].HasErrorMessage) {
                    GUI.color = new Color(0.87f, 0, 0);
                    GUI.Label(new Rect(r.x + 48 + 3, r.y + 25 + 10, r.width - 48 - 5, 18), this.inputBrushParams[i].ErrorMessage, EditorStyles.whiteMiniLabel);
                    GUI.color = restoreColor;
                }

                GUI.SetNextControlName(controlName);
                Rect nameRect = new Rect(r.x + 48 + 5, r.y + 20, r.width - 48 - 10, 16);

                if (this.inputBrushParams[i].IsSelected) {
                    // Count the number of selected tiles.
                    ++this.selectedTileCount;

                    if (this.inputBrushParams[i].HasErrorMessage) {
                        GUI.backgroundColor = new Color(1f, 0.9f, 0.9f);
                    }

                    this.inputBrushParams[i].Name = EditorGUI.TextField(nameRect, this.inputBrushParams[i].Name);

                    GUI.backgroundColor = restoreBackgroundColor;
                }
                else {
                    if (ButtonNameField(nameRect)) {
                        this.inputBrushParams[i].IsSelected = true;

                        EditorGUI.FocusTextInControl(controlName);
                        GUIUtility.ExitGUI();
                    }
                }

                this.DrawBrushCountButton(new Rect(r.x + r.width - 40 - 5, r.y + 1, 40, 18), i, this.inputBrushParams[i]);

                r.x += r.width;
            }

            GUILayout.EndVertical();
        }

        private void DrawBrushCountButton(Rect position, int tileIndex, TilesetBrushParams tile)
        {
            if (tile.Count == 0) {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                if (Event.current.type == EventType.Repaint) {
                    using (var tempContent = ControlContent.Basic("0", RotorzEditorStyles.Skin.ToolPaint)) {
                        RotorzEditorStyles.Instance.FlatButtonFake.Draw(position, tempContent, controlID);
                    }
                }
            }
            else if (tile.Count == 1) {
                using (var content = ControlContent.Basic(
                    "1",
                    RotorzEditorStyles.Skin.ToolPaint,
                    TileLang.ParticularText("Action", "Edit existing brush")
                )) {
                    if (RotorzEditorGUI.HoverButton(position, content)) {
                        foreach (var brushRecord in this.tilesetRecord.BrushRecords) {
                            var tilesetBrush = brushRecord.Brush as TilesetBrush;
                            if (tilesetBrush == null) {
                                continue;
                            }

                            bool isAutotile = tilesetBrush is AutotileBrush;
                            if ((tilesetBrush.tileIndex == tileIndex && !isAutotile) || (tileIndex == -1 && isAutotile)) {
                                ToolUtility.ShowBrushInDesigner(tilesetBrush);
                                break;
                            }
                        }
                        GUIUtility.ExitGUI();
                    }
                }
            }
            else {
                using (var content = ControlContent.Basic(
                    tile.Count.ToString(),
                    RotorzEditorStyles.Skin.ToolPaint,
                    TileLang.ParticularText("Action", "Edit existing brush")
                )) {
                    if (RotorzEditorGUI.HoverButton(position, content)) {
                        var menu = new EditorMenu();

                        foreach (var brushRecord in this.tilesetRecord.BrushRecords) {
                            var tilesetBrush = brushRecord.Brush as TilesetBrush;
                            if (tilesetBrush == null) {
                                continue;
                            }

                            bool isAutotile = tilesetBrush is AutotileBrush;
                            if ((tilesetBrush.tileIndex == tileIndex && !isAutotile) || (tileIndex == -1 && isAutotile)) {
                                string brushName = tilesetBrush.name;
                                menu.AddCommand(string.IsNullOrEmpty(brushName) ? " " : brushName)
                                    .Action(() => {
                                        ToolUtility.ShowBrushInDesigner(tilesetBrush);
                                    });
                            }
                        }

                        menu.ShowAsDropdown(position);
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        private void DrawBrushDefaultsGUI()
        {
            float restoreLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Visibility"),
                TileLang.Text("Allows brush to be hidden from user interfaces.")
            )) {
                this.inputBrushVisibility = (BrushVisibility)EditorGUILayout.EnumPopup(content, this.inputBrushVisibility);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Group #"),
                TileLang.Text("Logical group that brush belongs to. This is used when specifying more advanced coalescing rules.")
            )) {
                this.inputBrushGroupNo = EditorGUILayout.IntField(content, this.inputBrushGroupNo, GUI.skin.textField);
                ExtraEditorGUI.TrailingTip(content);
            }

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Tag")
            )) {
                this.inputBrushTag = EditorGUILayout.TagField(content, this.inputBrushTag);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Layer")
            )) {
                this.inputBrushLayer = EditorGUILayout.LayerField(content, this.inputBrushLayer);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Category")
            )) {
                this.inputBrushCategoryId = RotorzEditorGUI.BrushCategoryField(content, this.inputBrushCategoryId);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Static"),
                TileLang.Text("Static tiles can be combined when optimizing tile systems.")
            )) {
                this.inputBrushStatic = EditorGUILayout.Toggle(content, this.inputBrushStatic);
                ExtraEditorGUI.TrailingTip(content);
            }

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Always Add Container"),
                TileLang.Text("Add tile container object even when not needed by brush.")
            )) {
                this.inputBrushCreateEmptyContainer = EditorGUILayout.ToggleLeft(content, this.inputBrushCreateEmptyContainer);
                ExtraEditorGUI.TrailingTip(content);
            }

            if (this.inputBrushAttachPrefabTick) {
                ExtraEditorGUI.SeparatorLight();
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Attach Prefab"),
                TileLang.Text("Additional game objects can be painted by attaching a prefab.")
            )) {
                bool newAttachPrefabTick = EditorGUILayout.ToggleLeft(content, this.inputBrushAttachPrefabTick);
                if (!newAttachPrefabTick) {
                    ExtraEditorGUI.TrailingTip(content);
                }

                if (newAttachPrefabTick != this.inputBrushAttachPrefabTick) {
                    this.inputBrushAttachPrefabTick = newAttachPrefabTick;
                    // Should attachment be cleared?
                    if (!this.inputBrushAttachPrefabTick) {
                        this.inputBrushAttachPrefab = null;
                    }
                }
            }

            if (this.inputBrushAttachPrefabTick) {
                ++EditorGUI.indentLevel;

                this.inputBrushAttachPrefab = EditorGUILayout.ObjectField(this.inputBrushAttachPrefab, typeof(GameObject), false) as GameObject;

                GUILayout.Space(3);

                using (var content = ControlContent.WithTrailableTip(
                    TileLang.ParticularText("Property", "Apply Prefab Transform"),
                    TileLang.Text("Tick to use prefab transform to offset position, rotation and scale of painted tiles.")
                )) {
                    this.inputBrushApplyPrefabTransform = EditorGUILayout.ToggleLeft(content, this.inputBrushApplyPrefabTransform);
                    ExtraEditorGUI.TrailingTip(content);
                }

                using (var content = ControlContent.WithTrailableTip(
                    TileLang.ParticularText("Property", "Scale Mode"),
                    TileLang.Text("Specifies the way in which painted tiles should be scaled.")
                )) {
                    this.inputBrushScaleMode = (ScaleMode)EditorGUILayout.EnumPopup(content, this.inputBrushScaleMode);

                    if (this.inputBrushScaleMode == ScaleMode.Custom) {
                        // Cancel out label above vector field.
                        this.inputBrushCustomScale = EditorGUILayout.Vector3Field("", this.inputBrushCustomScale);
                        GUILayout.Space(-17);
                    }

                    ExtraEditorGUI.TrailingTip(content);
                }

                --EditorGUI.indentLevel;
            }

            GUILayout.Space(7);

            if (this.inputAutotileBrushParams.IsSelected && this.tileset is AutotileTileset) {
                ExpandAutotileBrushes.Value = RotorzEditorGUI.FoldoutSection(ExpandAutotileBrushes,
                    label: TileLang.Text("Autotile Brushes"),
                    callback: this.DrawBrushDefaultsGUI_AutotileBrushes
                );
            }

            ExpandTilesetBrushes.Value = RotorzEditorGUI.FoldoutSection(ExpandTilesetBrushes,
                label: TileLang.Text("Tileset Brushes"),
                callback: this.DrawBrushDefaultsGUI_TilesetBrushes
            );

            BrushDesignerView.ShowExtendedFlags = RotorzEditorGUI.FoldoutSection(BrushDesignerView.ShowExtendedFlags,
                label: TileLang.Text("Flags"),
                callback: this.DrawBrushDefaultsFlags
            );

            GUILayout.FlexibleSpace();

            EditorGUIUtility.labelWidth = restoreLabelWidth;
        }

        private void DrawBrushDefaultsGUI_TilesetBrushes()
        {
            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Procedural"),
                TileLang.Text("Allows individual atlas brushes to override property of tileset.")
            )) {
                this.inputBrushProcedural = (InheritYesNo)EditorGUILayout.EnumPopup(content, this.inputBrushProcedural);
                ExtraEditorGUI.TrailingTip(content);
            }

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Flaggable", "Solid Flag"),
                TileLang.Text("Solid flag can be used to assist with user defined collision detection or pathfinding.")
            )) {
                this.inputBrushSolidFlag = EditorGUILayout.ToggleLeft(content, this.inputBrushSolidFlag);
                ExtraEditorGUI.TrailingTip(content);
            }

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Add Collider"),
                TileLang.Text("Automatically adds box collider to painted tile.")
            )) {
                bool autoInitCollider;

                EditorGUI.BeginChangeCheck();
                this.inputBrushAddCollider = EditorGUILayout.ToggleLeft(content, this.inputBrushAddCollider);
                autoInitCollider = (EditorGUI.EndChangeCheck() && this.inputBrushAddCollider);

                if (this.inputBrushAddCollider) {
                    ++EditorGUI.indentLevel;
                    this.inputBrushColliderType = (ColliderType)EditorGUILayout.EnumPopup(this.inputBrushColliderType);
                    --EditorGUI.indentLevel;
                }
                ExtraEditorGUI.TrailingTip(content);

                if (autoInitCollider) {
                    this.inputBrushColliderType = BrushUtility.AutomaticColliderType;
                }
            }
        }

        private void DrawBrushDefaultsGUI_AutotileBrushes()
        {
            float restoreLabelWidth = EditorGUIUtility.labelWidth;
            float restoreFieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 1;
            EditorGUIUtility.fieldWidth = 50;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Edge Tiles"));

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Flaggable", "Solid Flag")
            )) {
                this.inputAutotileEdgeSolid = EditorGUILayout.ToggleLeft(content, this.inputAutotileEdgeSolid);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Add Collider"),
                TileLang.Text("Automatically adds box collider to painted tile.")
            )) {
                this.inputAutotileEdgeCollider = EditorGUILayout.ToggleLeft(content, this.inputAutotileEdgeCollider);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Inner Tiles"));

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Flaggable", "Solid Flag")
            )) {
                this.inputAutotileInnerSolid = EditorGUILayout.ToggleLeft(content, this.inputAutotileInnerSolid);
            }

            using (var content = ControlContent.Basic(
                TileLang.ParticularText("Property", "Add Collider"),
                TileLang.Text("Automatically adds box collider to painted tile.")
            )) {
                this.inputAutotileInnerCollider = EditorGUILayout.ToggleLeft(content, this.inputAutotileInnerCollider);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (this.inputAutotileEdgeCollider || this.inputAutotileInnerCollider) {
                ++EditorGUI.indentLevel;
                this.inputAutotileColliderType = (ColliderType)EditorGUILayout.EnumPopup(this.inputAutotileColliderType);
                --EditorGUI.indentLevel;
            }

            EditorGUIUtility.labelWidth = restoreLabelWidth;
            EditorGUIUtility.fieldWidth = restoreFieldWidth;

            if (ControlContent.TrailingTipsVisible) {
                ExtraEditorGUI.TrailingTip(TileLang.Text("Solid flag can be used to assist with user defined collision detection or pathfinding."));
            }
        }

        private void DrawBrushDefaultsFlags()
        {
            if (ControlContent.TrailingTipsVisible) {
                ExtraEditorGUI.TrailingTip(TileLang.Text("Flags can be used in custom scripts. Use of flags is entirely user defined!"));
            }

            string[] customFlagLabels = ProjectSettings.Instance.FlagLabels;

            // Calculate metrics for flag toggle control.
            var flagToggleStyle = EditorStyles.toggle;
            float flagToggleHeight = flagToggleStyle.CalcHeight(GUIContent.none, 0);

            Rect flagTogglePosition = EditorGUILayout.GetControlRect(false, (flagToggleHeight + 4) * 8);
            flagTogglePosition.width = flagTogglePosition.width / 2 - flagToggleStyle.margin.left;
            flagTogglePosition.height = flagToggleHeight;

            float resetY = flagTogglePosition.y;

            string defaultFlagLabel = TileLang.ParticularText("Flaggable", "Flag");
            string userFlagFormat = TileLang.ParticularText("Format|UserFlagLabel",
                /* i.e. '9: My Ninth Flag'
                   0: flag number
                   1: flag label */
                "{0:00}: {1}"
            );

            // Custom user flags 1 to 16.
            for (int flagNumber = 1; flagNumber <= 16; ++flagNumber) {
                if (flagNumber == 9) {
                    flagTogglePosition.x = flagTogglePosition.xMax + 2;
                    flagTogglePosition.y = resetY;
                }

                string flagLabel = !string.IsNullOrEmpty(customFlagLabels[flagNumber - 1])
                    ? customFlagLabels[flagNumber - 1]
                    : defaultFlagLabel;

                bool flagState = (this.inputBrushFlagMask & (1 << (flagNumber - 1))) != 0;
                bool newFlagState = EditorGUI.ToggleLeft(flagTogglePosition, string.Format(userFlagFormat, flagNumber, flagLabel), flagState);
                if (flagState != newFlagState) {
                    if (newFlagState) {
                        this.inputBrushFlagMask |= 1 << (flagNumber - 1);
                    }
                    else {
                        this.inputBrushFlagMask &= ~(1 << (flagNumber - 1));
                    }
                }

                flagTogglePosition.y = flagTogglePosition.yMax + 4;
            }

            Rect toolbarPosition = GUILayoutUtility.GetRect(0f, 20f);
            toolbarPosition.width -= 29 + 2;

            var quickSelectButtons = new GUIContent[] {
                ControlContent.Basic(TileLang.ParticularText("Action|Select", "All")),
                ControlContent.Basic(TileLang.ParticularText("Action|Select", "None")),
                ControlContent.Basic(TileLang.ParticularText("Action|Select", "Invert")),
            };

            switch (GUI.Toolbar(toolbarPosition, -1, quickSelectButtons)) {
                case 0:
                    this.inputBrushFlagMask |= 0xFFFF;
                    GUIUtility.ExitGUI();
                    break;
                case 1:
                    this.inputBrushFlagMask &= ~0xFFFF;
                    GUIUtility.ExitGUI();
                    break;
                case 2:
                    this.inputBrushFlagMask = (this.inputBrushFlagMask & ~0xFFFF) | (~this.inputBrushFlagMask & 0xFFFF);
                    GUIUtility.ExitGUI();
                    break;
            }

            toolbarPosition.x = toolbarPosition.xMax + 2;
            toolbarPosition.width = 29;

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.EditLabel,
                TileLang.ParticularText("Action", "Edit Flag Labels")
            )) {
                if (RotorzEditorGUI.HoverButton(toolbarPosition, content)) {
                    EditFlagLabelsWindow.ShowWindow(null);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private static GUIStyle s_CreateBrushesButtonStyle;

        public void OnSideGUI()
        {
            if (s_CreateBrushesButtonStyle == null) {
                s_CreateBrushesButtonStyle = new GUIStyle(ExtraEditorStyles.Instance.BigButtonPadded);
                s_CreateBrushesButtonStyle.fontSize = 12;
                s_CreateBrushesButtonStyle.padding.top = 11;
                s_CreateBrushesButtonStyle.padding.bottom = 12;
            }

            GUILayout.Space(-1);

            Rect position = EditorGUILayout.BeginVertical(GUILayout.Width(DesignerView.ExtendedPropertiesPanelWidth));
            GUILayout.Space(6);
            RotorzEditorGUI.Title(" " + TileLang.Text("Brush Properties"));

            this.scrollingBrushDefaults = EditorGUILayout.BeginScrollView(this.scrollingBrushDefaults, RotorzEditorStyles.Instance.PaddedScrollView);
            this.DrawBrushDefaultsGUI();
            EditorGUILayout.EndScrollView();

            ExtraEditorGUI.Separator(marginTop: 0, marginBottom: 0);
            ExtraEditorGUI.SeparatorLight(thickness: 3, marginTop: 0, marginBottom: 3);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(12);

                EditorGUI.BeginDisabledGroup(this.selectedTileCount <= 0);
                {
                    if (GUILayout.Button(TileLang.ParticularText("Action", "Create Brushes"), s_CreateBrushesButtonStyle)) {
                        this.OnCreateBrushes();
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(1);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(9);
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint) {
                RotorzEditorStyles.Instance.HorizontalSplitter.Draw(
                    new Rect(position.x, position.y, position.width - 6, position.height),
                    GUIContent.none,
                    false, false, false, false
                );
            }
        }

        #endregion


        #region Actions

        private void OnSelectAllTiles()
        {
            this.inputAutotileBrushParams.IsSelected = true;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].IsSelected = true;
            }
        }

        private void OnSelectNoneTiles()
        {
            this.inputAutotileBrushParams.IsSelected = false;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].IsSelected = false;
                this.inputBrushParams[i].ErrorMessage = null;
            }
        }

        private void OnSelectInvertTiles()
        {
            this.inputAutotileBrushParams.IsSelected = !this.inputAutotileBrushParams.IsSelected;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].IsSelected = !this.inputBrushParams[i].IsSelected;
                this.inputBrushParams[i].ErrorMessage = null;
            }
        }

        private void OnSelectUnusedTiles()
        {
            this.inputAutotileBrushParams.IsSelected = this.inputAutotileBrushParams.Count == 0;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].IsSelected = this.inputBrushParams[i].Count == 0;
                if (!this.inputBrushParams[i].IsSelected) {
                    this.inputBrushParams[i].ErrorMessage = null;
                }
            }
        }

        private void OnAutoName()
        {
            GUIUtility.keyboardControl = 0;

            string baseName = this.tileset.name;
            string newName = baseName;
            int counter = 1;

            // Ensure that brush name is unique!
            while (this.tilesetRecord.FindBrushByName(newName) != null) {
                newName = string.Format("{0} - {1:D2}", baseName, counter++);
            }

            this.inputAutotileBrushParams.Name = newName;

            int columns = this.tileset.Columns;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                baseName = string.Format("Tile {0}", i + 1);
                newName = baseName;
                counter = 1;

                // Ensure that brush name is unique!
                while (this.tilesetRecord.FindBrushByName(newName) != null) {
                    newName = string.Format("{0} - {1:D2}", baseName, counter++);
                }

                this.inputBrushParams[i].Name = newName;
                this.inputBrushParams[i].ErrorMessage = null;
            }
        }

        private void OnClearNames()
        {
            this.inputAutotileBrushParams.Name = string.Empty;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].Name = string.Empty;
                this.inputBrushParams[i].ErrorMessage = null;
            }
        }

        private void OnCreateBrushes()
        {
            RotorzEditorGUI.ClearControlFocus();

            if (!this.ValidateCreateBrushInputs()) {
                this.designer.Window.Focus();
                return;
            }

            var tileset = this.tilesetRecord.Tileset;

            var autotileTileset = tileset as AutotileTileset;
            if (autotileTileset != null && this.inputAutotileBrushParams.IsSelected) {
                var autotileBrush = ScriptableObject.CreateInstance<AutotileBrush>();
                autotileBrush.Initialize(autotileTileset);
                autotileBrush.procedural = InheritYesNo.Yes;
                this.CreateBrush(autotileBrush, true);
                autotileBrush.name = this.inputAutotileBrushParams.Name;

                autotileBrush.forceLegacySideways = false;

                autotileBrush.SolidFlag = this.inputAutotileEdgeSolid;
                autotileBrush.addCollider = this.inputAutotileEdgeCollider;

                autotileBrush.InnerSolidFlag = this.inputAutotileInnerSolid;
                autotileBrush.addInnerCollider = this.inputAutotileInnerCollider;

                autotileBrush.colliderType = this.inputAutotileColliderType;

                AssetDatabase.AddObjectToAsset(autotileBrush, tileset);
            }

            bool proceduralBrushes = this.inputBrushProcedural == InheritYesNo.Yes || (this.inputBrushProcedural == InheritYesNo.Inherit && tileset.procedural);
            bool dirtyMeshAsset = false;

            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                TilesetBrushParams brushParams = this.inputBrushParams[i];
                if (!brushParams.IsSelected) {
                    continue;
                }

                var tilesetBrush = BrushUtility.CreateTilesetBrush(brushParams.Name, tileset, i, this.inputBrushProcedural);
                if (BrushUtility.s_DirtyMesh) {
                    dirtyMeshAsset = true;
                }

                this.CreateBrush(tilesetBrush, proceduralBrushes);
            }

            if (dirtyMeshAsset) {
                // Import mesh asset if it has been modified!
                string meshAssetPath = AssetDatabase.GetAssetPath(tileset.tileMeshAsset);
                AssetDatabase.ImportAsset(meshAssetPath);
            }

            EditorUtility.SetDirty(tileset);

            // Ensure that changes are persisted immediately.
            AssetDatabase.SaveAssets();

            ToolUtility.RepaintBrushPalette();

            this.ResetBrushParams();
        }

        private bool ValidateCreateBrushInputs()
        {
            bool valid = true;
            int firstInvalidIndex = 0;

            // Clear previous validation errors.
            this.inputAutotileBrushParams.ErrorMessage = null;
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                this.inputBrushParams[i].ErrorMessage = null;
            }

            // Validate autotile brush?
            var autotileTileset = this.tileset as AutotileTileset;
            if (autotileTileset != null && this.inputAutotileBrushParams.IsSelected) {
                if (!this.ValidateAutotileBrush(ref this.inputAutotileBrushParams)) {
                    valid = false;
                }
            }

            // Validate atlas brushes.
            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                if (!this.inputBrushParams[i].IsSelected) {
                    continue;
                }

                if (!this.ValidateBrush(i, ref this.inputBrushParams[i])) {
                    if (valid) {
                        valid = false;
                        // Index of first invalid tile.
                        firstInvalidIndex = i;
                    }
                }
            }

            if (!valid) {
                // Scroll to first invalid tile.
                float scrollY = (firstInvalidIndex / this.viewColumns) * (48 + 8);
                if (autotileTileset != null && this.inputAutotileBrushParams.ErrorMessage == null) {
                    scrollY += 48 + 8 + 7;
                }

                this.designer.viewScrollPosition.y = scrollY;

                EditorUtility.DisplayDialog(
                    TileLang.ParticularText("Error", "One or more inputs were invalid"),
                    TileLang.Text("Please review inputs and any errors and try again"),
                    TileLang.ParticularText("Action", "OK")
                );
                return false;
            }

            // Are there still any brushes to create?
            if (autotileTileset != null && this.inputAutotileBrushParams.IsSelected) {
                return true;
            }

            for (int i = 0; i < this.inputBrushParams.Length; ++i) {
                if (this.inputBrushParams[i].IsSelected) {
                    return true;
                }
            }

            return false;
        }

        private bool ValidateBrush(int tileIndex, ref TilesetBrushParams brushParams)
        {
            brushParams.ErrorMessage = null;
            brushParams.Name = brushParams.Name.Trim();

            if (string.IsNullOrEmpty(brushParams.Name)) {
                brushParams.ErrorMessage = TileLang.ParticularText("Error", "Name was not specified");
                return false;
            }

            // Is name actually valid?
            if (!Regex.IsMatch(brushParams.Name, "^[A-Za-z0-9()][A-Za-z0-9\\-_ ()]*")) {
                brushParams.ErrorMessage = TileLang.ParticularText("Error", "Invalid name");
                return false;
            }

            // Do not check autotile brushes!
            if (tileIndex != -1) {
                // Does autotile brush already use this name?
                if (this.inputAutotileBrushParams.IsSelected && this.inputAutotileBrushParams.Name == brushParams.Name) {
                    brushParams.ErrorMessage = TileLang.ParticularText("Error", "Name was already specified");
                    return false;
                }

                // Does another prior brush already use this name?
                for (int i = 0; i < tileIndex; ++i) {
                    if (this.inputBrushParams[i].IsSelected && this.inputBrushParams[i].Name == brushParams.Name) {
                        brushParams.ErrorMessage = TileLang.ParticularText("Error", "Name was already specified");
                        return false;
                    }
                }
            }

            // Does an existing brush already have this name?
            foreach (var brushRecord in this.tilesetRecord.BrushRecords) {
                if (brushRecord.DisplayName == brushParams.Name) {
                    brushParams.ErrorMessage = TileLang.ParticularText("Error", "Name is already in use");
                    return false;
                }
            }

            return true;
        }

        private bool ValidateAutotileBrush(ref TilesetBrushParams brushParams)
        {
            if (!this.ValidateBrush(-1, ref brushParams)) {
                return false;
            }
            if (!brushParams.IsSelected) {
                return true;
            }
            return true;
        }

        private void CreateBrush(TilesetBrush tilesetBrush, bool procedural)
        {
            tilesetBrush.visibility = this.inputBrushVisibility;

            tilesetBrush.Static = this.inputBrushStatic;

            tilesetBrush.tag = this.inputBrushTag;
            tilesetBrush.layer = this.inputBrushLayer;
            tilesetBrush.CategoryId = this.inputBrushCategoryId;

            tilesetBrush.group = this.inputBrushGroupNo;

            if (procedural) {
                tilesetBrush.alwaysAddContainer = this.inputBrushCreateEmptyContainer;
            }

            tilesetBrush.addCollider = this.inputBrushAddCollider;
            tilesetBrush.colliderType = this.inputBrushColliderType;

            if (this.inputBrushAttachPrefabTick && this.inputBrushAttachPrefab != null) {
                tilesetBrush.attachPrefab = this.inputBrushAttachPrefab;
                tilesetBrush.applyPrefabTransform = this.inputBrushApplyPrefabTransform;
                tilesetBrush.scaleMode = this.inputBrushScaleMode;
                tilesetBrush.transformScale = this.inputBrushCustomScale;
            }

            tilesetBrush.TileFlags = this.inputBrushFlagMask;
            tilesetBrush.SolidFlag = this.inputBrushSolidFlag;
        }

        #endregion
    }
}
