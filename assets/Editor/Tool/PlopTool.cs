// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using Rotorz.Settings;
using Rotorz.Tile.Internal;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Tool for plopping tiles onto the plane of a tile system without actually adding
    /// them to the tile data structure.
    /// </summary>
    /// <remarks>
    /// <para>Automatically adds <see cref="PlopInstance"/> component to plopped tiles
    /// allowing them to be erased.</para>
    /// </remarks>
    public class PlopTool : PaintToolBase
    {
        private readonly string label = TileLang.FormatActionWithShortcut(
            TileLang.ParticularText("Tool Name", "Plop"), "P"
        );


        #region Tool Information

        /// <inheritdoc/>
        public override string Label {
            get { return this.label; }
        }

        /// <inheritdoc/>
        public override Texture2D IconNormal {
            get { return RotorzEditorStyles.Skin.ToolPlop; }
        }

        /// <inheritdoc/>
        public override Texture2D IconActive {
            get { return RotorzEditorStyles.Skin.GetInverted(RotorzEditorStyles.Skin.ToolPlop); }
        }

        /// <inheritdoc/>
        public override CursorInfo Cursor {
            get {
                CursorInfo cursor = ToolCursors.Plop;

                // Show 'Plop Cycle' cursor if active plop has multiple variations.
                if (!this.allowOverpaint && ToolUtility.ActivePlop != null
                        && ToolUtility.SelectedBrush == ToolUtility.ActivePlop.Brush && PlopUtility.CanPlopWithBrush(ToolUtility.SelectedBrush)
                        && PlopUtility.CountPlopVariations(ToolUtility.ActiveTileSystem, ToolUtility.ActivePlop) > 1) {
                    cursor = ToolCursors.PlopCycle;
                }

                return cursor;
            }
        }

        #endregion


        #region Tool Interaction

        private Vector2 localMousePoint;
        private bool allowOverpaint;

        /// <inheritdoc/>
        public override void OnRefreshToolEvent(ToolEvent e, IToolContext context)
        {
            base.OnRefreshToolEvent(e, context);

            // Update local point of mouse within tile system.
            this.localMousePoint = e.MousePointerLocalPoint;
            if (this.SnapAxisX.Alignment == SnapAlignment.Cells) {
                this.localMousePoint.x -= this.SnapAxisX.Resolve(context.TileSystem.CellSize.x) / 2f;
            }
            if (this.SnapAxisY.Alignment == SnapAlignment.Cells) {
                this.localMousePoint.y += this.SnapAxisY.Resolve(context.TileSystem.CellSize.y) / 2f;
            }

            // "Disable cycle function"
            this.allowOverpaint = this.DisableCycleFunction | Event.current.control;

            if (Event.current.isMouse) {
                var go = HandleUtility.PickGameObject(Event.current.mousePosition, true);
                ToolUtility.ActivePlop = (go != null)
                    ? go.GetComponentInParent<PlopInstance>()
                    : null;

                // "Interact with active system only"
                if (this.InteractWithActiveSystemOnly && ToolUtility.ActivePlop != null && ToolUtility.ActivePlop.Owner != context.TileSystem) {
                    ToolUtility.ActivePlop = null;
                }
            }
        }

        /// <inheritdoc/>
        public override void OnTool(ToolEvent e, IToolContext context)
        {
            switch (e.Type) {
                case EventType.MouseDown:
                    this.OnPaint(e, context);
                    break;
            }
        }

        /// <summary>
        /// Raised by <see cref="OnTool"/> to perform painting upon pressing left or right mouse button.
        /// </summary>
        /// <param name="e">Tool event data.</param>
        /// <param name="context">Context that tool is being used in.</param>
        protected virtual void OnPaint(ToolEvent e, IToolContext context)
        {
            var brush = e.IsLeftButtonPressed
                ? ToolUtility.SelectedBrush
                : ToolUtility.SelectedBrushSecondary;

            // Like with the regular paint tool, null brush is the eraser!
            if (brush != null && !PlopUtility.CanPlopWithBrush(brush)) {
                return;
            }

            if (brush == null) {
                if (ToolUtility.ActivePlop != null) {
                    PlopUtility.ErasePlop(ToolUtility.ActivePlop);
                    ToolUtility.ActivePlop = null;
                }
            }
            else {
                if (!this.allowOverpaint && ToolUtility.ActivePlop != null) {
                    // Cycle to next variation if replacing plop with same brush.
                    int nextVariation = ToolUtility.ActivePlop.VariationIndex;
                    if (brush == ToolUtility.ActivePlop.Brush) {
                        ++nextVariation;
                    }

                    ToolUtility.ActivePlop = PlopUtility.CyclePlop(context.TileSystem, ToolUtility.ActivePlop, brush, ToolUtility.Rotation, nextVariation);
                }
                else {
                    var args = this.GetPaintingArgs(brush);
                    if (args.variation == Brush.RANDOM_VARIATION) {
                        args.variation = this.PreRandomizeVariation(brush, 0);
                        this.RandomizeVariationShift();
                    }
                    int nextVariation = args.ResolveVariation(0);

                    var plop = PlopUtility.PaintPlop(context.TileSystem, this.ApplySnapping(this.localMousePoint), brush, ToolUtility.Rotation, nextVariation);
                    ToolUtility.ActivePlop = plop;
                    ToolUtility.PreviouslyPlopped = plop;
                }
            }
        }

        #endregion


        #region Tool Options

        /// <inheritdoc/>
        protected override void PrepareOptions(ISettingStore store)
        {
            base.PrepareOptions(store);

            this.settingSnapAxisX = store.Fetch<SnapAxis>("SnapAxisX", null,
                filter: (value) => value ?? new SnapAxis()
            );
            this.settingSnapAxisY = store.Fetch<SnapAxis>("SnapAxisY", null,
                filter: (value) => value ?? new SnapAxis()
            );
            this.settingLinkSnapAxis = store.Fetch<bool>("LinkSnapAxis", true);

            this.settingPlopLocation = store.Fetch<Location>("PlopLocation", Location.GroupInsideTileSystem);
            this.settingPlopGroupName = store.Fetch<string>("PlopGroupName", "Plops");

            this.settingDisableCycleFunction = store.Fetch<bool>("DisableCycleFunction", false);
            this.settingInteractWithActiveSystemOnly = store.Fetch<bool>("InteractWithActiveSystemOnly", true);
            this.settingHideWireframeOutline = store.Fetch<bool>("HideWireframeOutline", false);
        }


        private Setting<SnapAxis> settingSnapAxisX;
        private Setting<SnapAxis> settingSnapAxisY;
        private Setting<bool> settingLinkSnapAxis;


        internal SnapAxis SnapAxisX {
            get { return this.settingSnapAxisX.Value; }
        }

        internal SnapAxis SnapAxisY {
            get { return this.settingSnapAxisY.Value; }
        }

        internal bool LinkSnapAxis {
            get { return this.settingLinkSnapAxis.Value; }
            set { this.settingLinkSnapAxis.Value = value; }
        }

        internal Vector3 ApplySnapping(Vector3 point)
        {
            Vector2 cellSize = ToolUtility.ActiveTileSystem.CellSize;
            point.x = this.SnapAxisX.ApplySnapping(point.x, cellSize.x, false);
            point.y = this.SnapAxisY.ApplySnapping(point.y, cellSize.y, true);
            return point;
        }


        /// <summary>
        /// Indicates location for painted tile objects.
        /// </summary>
        public enum Location
        {
            /// <summary>
            /// Groups within empty "Plops" object inside tile system.
            /// </summary>
            GroupInsideTileSystem,
            /// <summary>
            /// Make immediate child of tile system.
            /// </summary>
            ChildOfTileSystem,
            /// <summary>
            /// Simply place within root top-level of scene.
            /// </summary>
            SceneRoot,
        }

        private Setting<Location> settingPlopLocation;
        private Setting<string> settingPlopGroupName;

        /// <summary>
        /// Gets or sets location for 'plopped' objects (where they are parented).
        /// </summary>
        public Location PlopLocation {
            get { return this.settingPlopLocation.Value; }
            set { this.settingPlopLocation.Value = value; }
        }

        /// <summary>
        /// Gets or sets name of game object for which to group plops inside the active
        /// tile system when <see cref="PlopLocation"/> is set to a value of <see cref="Location.GroupInsideTileSystem"/>.
        /// </summary>
        public string PlopGroupName {
            get { return this.settingPlopGroupName.Value; }
            set { this.settingPlopGroupName.Value = value; }
        }

        private Setting<bool> settingDisableCycleFunction;
        private Setting<bool> settingInteractWithActiveSystemOnly;
        private Setting<bool> settingHideWireframeOutline;

        /// <summary>
        /// Gets or sets whether cycle function of plop tool should be disabled thus
        /// making it possible to rapidly plop overlapping tiles without having to hold
        /// the <b>Ctrl</b> key.
        /// </summary>
        public bool DisableCycleFunction {
            get { return this.settingDisableCycleFunction.Value; }
            set { this.settingDisableCycleFunction.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether tool should only be able to cycle and erase plops that
        /// are associated with the active tile system.
        /// </summary>
        public bool InteractWithActiveSystemOnly {
            get { return this.settingInteractWithActiveSystemOnly.Value; }
            set { this.settingInteractWithActiveSystemOnly.Value = value; }
        }

        /// <summary>
        /// Gets or sets whether wireframe outline should be shown around immediate
        /// preview or whether to highlight an existing plop.
        /// </summary>
        /// <remarks>
        /// <para>Wireframe outline is always shown for brushes where immediate previews
        /// have been disabled.</para>
        /// </remarks>
        public bool HideWireframeOutline {
            get { return this.settingHideWireframeOutline.Value; }
            set { this.settingHideWireframeOutline.Value = value; }
        }

        #endregion


        #region Tool Options Interface

        /// <inheritdoc/>
        public override void OnToolOptionsGUI()
        {
            GUILayout.BeginHorizontal();
            {
                this.DrawStandardOptionsGUI();

                GUILayout.FlexibleSpace();

                GUILayout.Space(5);

                float spacingFieldHeight = RotorzEditorStyles.Instance.TextFieldRoundEdge.fixedHeight;
                float toggleWidth = 13;

                Rect spacingFieldPosition = GUILayoutUtility.GetRect(110 + toggleWidth, 0);
                spacingFieldPosition.y -= 5;
                spacingFieldPosition.width -= toggleWidth;
                spacingFieldPosition.height = spacingFieldHeight;
                SpacingCoordinateField(spacingFieldPosition, "X", this.SnapAxisX);

                Rect linkFieldsPosition = spacingFieldPosition;
                linkFieldsPosition.x = spacingFieldPosition.xMax;
                linkFieldsPosition.y += 7;

                if (this.LinkSnapAxis) {
                    this.SnapAxisY.SetFrom(this.SnapAxisX);
                }

                EditorGUI.BeginDisabledGroup(this.LinkSnapAxis);
                spacingFieldPosition.y += spacingFieldPosition.height + 1;
                SpacingCoordinateField(spacingFieldPosition, "Y", this.SnapAxisY);
                EditorGUI.EndDisabledGroup();

                this.LinkSnapAxis = LinkFields(linkFieldsPosition, this.LinkSnapAxis);

                GUILayout.Space(3);
            }
            GUILayout.EndHorizontal();

            ExtraEditorGUI.SeparatorLight();
        }

        /// <inheritdoc/>
        public override void OnAdvancedToolOptionsGUI()
        {
            this.PlopLocation = (Location)EditorGUILayout.EnumPopup(TileLang.ParticularText("Property", "Location"), this.PlopLocation);
            ++EditorGUI.indentLevel;
            switch (this.PlopLocation) {
                case Location.GroupInsideTileSystem:
                    this.PlopGroupName = EditorGUILayout.TextField(TileLang.ParticularText("Property", "Group Name"), this.PlopGroupName);
                    break;
            }
            --EditorGUI.indentLevel;

            ExtraEditorGUI.SeparatorLight();

            // Repaint scene views when options are changed so that handles updated.
            EditorGUI.BeginChangeCheck();

            this.DisableCycleFunction = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Disable cycle function"), this.DisableCycleFunction);
            this.InteractWithActiveSystemOnly = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Interact with active system only"), this.InteractWithActiveSystemOnly);
            this.HideWireframeOutline = EditorGUILayout.ToggleLeft(TileLang.ParticularText("Property", "Hide wireframe outline"), this.HideWireframeOutline);

            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }
        }

        #endregion


        #region Scene View

        /// <inheritdoc/>
        public override void OnDrawGizmos(TileSystem system)
        {
            // We need to populate properties of `IBrushContext` for preview generation.
            var brush = (PreviousToolEvent != null && PreviousToolEvent.IsRightButtonPressed)
                ? ToolUtility.SelectedBrushSecondary
                : ToolUtility.SelectedBrush;

            if (!PlopUtility.CanPlopWithBrush(brush)) {
                return;
            }

            // Do not draw immediate preview when mouse is positioned over a plop
            // unless overpainting is permitted.
            if (!this.allowOverpaint && ToolUtility.ActivePlop != null) {
                return;
            }

            // Offset preview against mouse position.
            Vector3 placementPoint = PlopUtility.PositionFromPlopPoint(system, this.ApplySnapping(this.localMousePoint));
            ImmediatePreviewUtility.Matrix = system.transform.localToWorldMatrix * MathUtility.TranslationMatrix(placementPoint);

            this._fakeContext.TileSystem = system;
            this._fakeContext.Brush = brush;

            // Pretend to paint tile so that we can see its data beforehand!
            var previewTile = ImmediatePreviewUtility.GetPreviewTileData(this._fakeContext, brush, ToolUtility.Rotation);
            // Plop tool does not support orientations.
            previewTile.orientationMask = 0;

            var args = GetPaintingArgs(brush);
            if (args.variation == Brush.RANDOM_VARIATION) {
                args.variation = this.PreRandomizeVariation(brush, 0);
            }
            previewTile.variationIndex = (byte)args.ResolveVariation(0);

            brush.OnDrawImmediatePreview(this._fakeContext, previewTile, ImmediatePreviewUtility.PreviewMaterial, brush);
        }

        /// <inheritdoc/>
        public override void OnSceneGUI(ToolEvent e, IToolContext context)
        {
            if (!IsEditorNearestControl) {
                return;
            }

            // "Hide wireframe outline"
            if (this.HideWireframeOutline) {
                bool willErase = (ToolUtility.SelectedBrush == null && ToolUtility.ActivePlop != null);
                bool willCycle = (!this.allowOverpaint && ToolUtility.ActivePlop != null && PlopUtility.CanPlopWithBrush(ToolUtility.SelectedBrush));
                bool disableImmediatePreview = (ToolUtility.SelectedBrush != null && ToolUtility.SelectedBrush.disableImmediatePreview);
                if (!willErase && !willCycle && !disableImmediatePreview) {
                    return;
                }
            }

            // Outline plop with wire cube!
            Vector3 wirePoint = (!this.allowOverpaint && ToolUtility.ActivePlop != null)
                ? ToolUtility.ActivePlop.PlopPoint
                : this.ApplySnapping(this.localMousePoint);

            Vector3 snapCellSize = context.TileSystem.CellSize;
            snapCellSize.x = this.SnapAxisX.Resolve(snapCellSize.x);
            snapCellSize.y = this.SnapAxisY.Resolve(snapCellSize.y);

            ToolHandleUtility.DrawWireBox(wirePoint, snapCellSize);
        }

        #endregion


        #region Fake Implementation of IBrushContext

        private FakeBrushContext _fakeContext = new FakeBrushContext();


        /// <summary>
        /// Fake context for previewing tile data.
        /// </summary>
        private class FakeBrushContext : IBrushContext
        {
            /// <inheritdoc/>
            public Brush Brush { get; set; }

            /// <inheritdoc/>
            public int Column {
                get { return 0; }
            }
            /// <inheritdoc/>
            public int Row {
                get { return 0; }
            }

            /// <inheritdoc/>
            public TileSystem TileSystem { get; set; }
        }

        #endregion


        #region Custom Field: SpacingCoordinateField

        private static readonly int s_SpacingCoordinateFieldHint = "EditorTextField".GetHashCode();

        private static SnapAxis s_DropDownSnapAxis;

        private static void SpacingCoordinateField(Rect position, string label, SnapAxis axis)
        {
            var fieldStyle = RotorzEditorStyles.Instance.TextFieldRoundEdge;
            var textStyle = RotorzEditorStyles.Instance.TransparentTextField;
            var buttonStyle = RotorzEditorStyles.Instance.TextFieldRoundEdgeCancelButtonEmpty;

            int controlID = EditorGUIUtility.GetControlID(s_SpacingCoordinateFieldHint, FocusType.Passive);
            int realTextControlID = controlID + 1;

            // Display prefix label?
            if (!string.IsNullOrEmpty(label)) {
                if (Event.current.type == EventType.Repaint) {
                    EditorStyles.label.Draw(new Rect(position.x, position.y, 12, position.height), label, false, false, false, false);
                }
                position.x += 12;
                position.width -= 12;
            }

            // Display popup field for alignment selection.
            Rect alignmentPosition = position;
            alignmentPosition.y -= 1;
            alignmentPosition.width = 33;
            alignmentPosition.height += 1;

            using (var content = AlignmentContent(label, axis)) {
                if (EditorInternalUtility.DropdownMenu(alignmentPosition, content, RotorzEditorStyles.Instance.FlatButton)) {
                    DropDownAlignment(alignmentPosition, axis);
                }
            }

            position.x += alignmentPosition.width;
            position.width -= alignmentPosition.width;

            EditorGUI.BeginDisabledGroup(axis.Alignment == SnapAlignment.Free);
            {
                // Add small amount of padding after control.
                Rect textPosition = position;
                textPosition.width -= 3;

                if (axis.Alignment == SnapAlignment.Free) {
                    // Draw background for text field.
                    if (Event.current.type == EventType.Repaint) {
                        position.width -= buttonStyle.fixedWidth;

                        fieldStyle.Draw(position, false, false, false, false);

                        // Draw end of text field control.
                        position.x += position.width;
                        position.width = buttonStyle.fixedWidth;
                        position.height = buttonStyle.fixedHeight;

                        buttonStyle.Draw(position, false, false, false, false);
                    }
                }
                else {
                    using (var fieldPrefixContent = ControlContent.Basic(
                        labelText: axis.GridType == SnapGridType.Fraction ? "1/" : " ",
                        image: RotorzEditorStyles.Skin.DownArrow
                    )) {
                        // Draw background for text field.
                        if (Event.current.type == EventType.Repaint) {
                            position.width -= buttonStyle.fixedWidth;

                            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.black : new Color(0f, 0f, 0f, 0.5f);
                            fieldStyle.Draw(position, fieldPrefixContent, realTextControlID);
                            GUI.contentColor = Color.white;

                            // Draw end of text field control.
                            position.x += position.width;
                            position.width = buttonStyle.fixedWidth;
                            position.height = buttonStyle.fixedHeight;

                            buttonStyle.Draw(position, false, false, false, false);
                        }

                        float prefixWidth = fieldStyle.CalcSize(fieldPrefixContent).x;

                        if (Event.current.GetTypeForControl(controlID) == EventType.MouseDown) {
                            Rect popupPosition = textPosition;
                            popupPosition.width = prefixWidth;
                            if (popupPosition.Contains(Event.current.mousePosition)) {
                                Event.current.Use();
                                DropDownSpacingCoordinate(popupPosition, axis);
                            }
                        }

                        // Draw actual text field.
                        textPosition.x += prefixWidth;
                        textPosition.y += 1;
                        textPosition.width -= prefixWidth;

                        switch (axis.GridType) {
                            default:
                            case SnapGridType.Fraction:
                                axis.SetFraction(EditorGUI.IntField(textPosition, axis.FractionDenominator, textStyle));
                                break;

                            case SnapGridType.Custom:
                                axis.SetCustomSize(EditorGUI.FloatField(textPosition, axis.CustomSize, textStyle));
                                break;
                        }
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private static ControlContent AlignmentContent(string label, SnapAxis axis)
        {
            Texture2D icon;
            string tip;

            switch (axis.Alignment) {
                default:
                case SnapAlignment.Points:
                    icon = RotorzEditorStyles.Skin.SnapPoints;
                    tip = TileLang.ParticularText("SnapAlignment", "Points");
                    break;

                case SnapAlignment.Cells:
                    icon = RotorzEditorStyles.Skin.SnapCells;
                    tip = TileLang.ParticularText("SnapAlignment", "Cells");
                    break;

                case SnapAlignment.Free:
                    icon = label == "X" ? RotorzEditorStyles.Skin.SnapFreeX : RotorzEditorStyles.Skin.SnapFreeY;
                    tip = TileLang.ParticularText("SnapAlignment", "Free");
                    break;
            }

            return ControlContent.Basic(icon, tip);
        }

        private static void DropDownAlignment(Rect position, SnapAxis axis)
        {
            s_DropDownSnapAxis = axis;

            var menu = new EditorMenu();

            menu.AddCommand(TileLang.ParticularText("SnapAlignment", "Free"))
                .Checked(axis.Alignment == SnapAlignment.Free)
                .Action(DoSelectSnapAlignment, SnapAlignment.Free);

            menu.AddCommand(TileLang.ParticularText("SnapAlignment", "Points"))
                .Checked(axis.Alignment == SnapAlignment.Points)
                .Action(DoSelectSnapAlignment, SnapAlignment.Points);

            menu.AddCommand(TileLang.ParticularText("SnapAlignment", "Cells"))
                .Checked(axis.Alignment == SnapAlignment.Cells)
                .Action(DoSelectSnapAlignment, SnapAlignment.Cells);

            menu.ShowAsDropdown(position);
        }

        private static void DoSelectSnapAlignment(SnapAlignment alignment)
        {
            s_DropDownSnapAxis.Alignment = alignment;
        }

        private static void DropDownSpacingCoordinate(Rect position, SnapAxis axis)
        {
            s_DropDownSnapAxis = axis;

            var menu = new EditorMenu();

            menu.AddCommand(TileLang.Text("Fraction of Cell Size"))
                .Checked(axis.GridType == SnapGridType.Fraction)
                .Action(DoSelectSpacingType, SnapGridType.Fraction);

            menu.AddCommand(TileLang.Text("Custom Size"))
                .Checked(axis.GridType == SnapGridType.Custom)
                .Action(DoSelectSpacingType, SnapGridType.Custom);

            menu.AddSeparator();

            foreach (int fractionValue in new int[] { 1, 2, 4, 8, 16 }) {
                menu.AddCommand(string.Format("{0} \u2044 {1}", 1, fractionValue))
                    .Action(s_DropDownSnapAxis.SetFraction, fractionValue);
            }

            menu.AddSeparator();

            foreach (float decimalValue in new float[] { 0.1f, 0.25f, 0.5f, 1f, 2f }) {
                menu.AddCommand(string.Format("{0}", decimalValue))
                    .Action(s_DropDownSnapAxis.SetCustomSize, decimalValue);
            }

            menu.ShowAsDropdown(position);
        }

        private static void DoSelectSpacingType(SnapGridType gridType)
        {
            s_DropDownSnapAxis.GridType = gridType;
        }

        #endregion


        #region Custom Field: Link Fields

        private static bool LinkFields(Rect position, bool linked)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(Event.current.mousePosition)) {
                        linked = !linked;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    // Ensure that size of this control is correct.
                    position.width = 13;
                    if (position.height > 21) {
                        position.y -= (position.height - 21) / 2f;
                    }
                    position.height = 21;

                    GUI.DrawTextureWithTexCoords(position, RotorzEditorStyles.Skin.LinkFields, new Rect(linked ? 13f / 26f : 0, 0, 13f / 26f, 1f));
                    break;
            }

            return linked;
        }

        #endregion
    }
}
