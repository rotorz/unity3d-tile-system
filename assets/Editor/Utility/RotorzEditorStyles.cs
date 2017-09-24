// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Styles for editor user interfaces.
    /// </summary>
    internal sealed class RotorzEditorStyles : EditorSingletonScriptableObject
    {
        private static RotorzEditorStyles s_Instance;
        private static SkinInfo s_Skin;


        /// <summary>
        /// Gets the one-and-only <see cref="RotorzEditorStyles"/> instance.
        /// </summary>
        public static RotorzEditorStyles Instance {
            get {
                EditorSingletonUtility.GetAssetInstance<RotorzEditorStyles>(ref s_Instance);
                return s_Instance;
            }
        }

        /// <summary>
        /// Gets the current skin.
        /// </summary>
        public static SkinInfo Skin {
            get {
                if (s_Skin == null) {
                    s_Skin = EditorGUIUtility.isProSkin ? Instance.darkSkin : Instance.lightSkin;
                }
                return s_Skin;
            }
        }


        [SerializeField]
        private SkinInfo darkSkin = new SkinInfo();
        [SerializeField]
        private SkinInfo lightSkin = new SkinInfo();


        public static GUILayoutOption ContractWidth = GUILayout.ExpandWidth(false);

        public static Color SelectedHighlightColor = new Color32(61, 128, 223, 255);
        public static Color SelectedHighlightStrongColor = new Color32(0, 128, 255, 255);

        public GUIStyle InspectorBigTitle { get; private set; }

        public GUIStyle Box { get; private set; }
        public GUIStyle ListBox { get; private set; }
        public GUIStyle OutlinedCornerBox { get; private set; }
        public GUIStyle VariationBox { get; private set; }
        public GUIStyle DropTargetBackground { get; private set; }
        public GUIStyle StatusWindow { get; private set; }

        public GUIStyle ButtonWide { get; private set; }
        public GUIStyle Separator { get; private set; }
        public GUIStyle FlatToggle { get; private set; }

        public GUIStyle HorizontalSplitter { get; private set; }

        public GUIStyle BrushField { get; private set; }
        public GUIStyle PreviewLabel { get; private set; }
        public GUIStyle SelectedPreviewLabel { get; private set; }
        public GUIStyle WhiteWordWrappedMiniLabel { get; private set; }
        public GUIStyle TitleLabel { get; private set; }
        public GUIStyle BoldLabel { get; private set; }
        public GUIStyle TabBackground { get; private set; }
        public GUIStyle Tab { get; private set; }
        public GUIStyle MiniCenteredLabel { get; private set; }
        public GUIStyle SmallRemoveButton { get; private set; }
        public GUIStyle LabelMiddleLeft { get; private set; }

        public GUIStyle MiniSliderBorder { get; private set; }
        public Color MiniSliderFillColor { get; private set; }
        public Color MiniSliderEmptyColor { get; private set; }
        public Color MiniSliderMarkerColor { get; private set; }

        public GUIStyle SmallCloseButton { get; private set; }
        public GUIStyle SearchTextField { get; private set; }
        public GUIStyle SearchCancelButton { get; private set; }
        public GUIStyle SearchCancelButtonEmpty { get; private set; }
        public GUIStyle TextFieldRoundEdge { get; private set; }
        public GUIStyle TextFieldRoundEdgeCancelButton { get; private set; }
        public GUIStyle TextFieldRoundEdgeCancelButtonEmpty { get; private set; }
        public GUIStyle TransparentTextField { get; private set; }

        /*
        public GUIStyle ToolbarSearchTextField = "ToolbarSeachTextField";
        public GUIStyle ToolbarSearchCancelButton = "ToolbarSeachCancelButton";
        public GUIStyle ToolbarSearchCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
        */

        public GUIStyle SmallButton { get; private set; }
        public GUIStyle SmallFlatButton { get; private set; }
        public GUIStyle SmallFlatButtonFake { get; private set; }
        public GUIStyle FlatButton { get; private set; }
        public GUIStyle FlatButtonNoMargin { get; private set; }
        public GUIStyle FlatButtonFake { get; private set; }
        public GUIStyle HistoryNavButton { get; private set; }
        public GUIStyle ToolButton { get; private set; }

        public GUIStyle ListSectionElement { get; private set; }
        public GUIStyle ListLargeElement { get; private set; }

        public GUIStyle FoldoutTitle { get; private set; }
        public GUIStyle FoldoutSectionPadded { get; private set; }
        public GUIStyle InspectorSectionPadded { get; private set; }
        public GUIStyle ExtendedPropertiesLeader { get; private set; }

        public GUIStyle SelectedBrushPreviewBox { get; private set; }

        public GUIStyle ButtonPaddedExtra { get; private set; }
        public GUIStyle ToolbarButtonPadded { get; private set; }
        public GUIStyle ToolbarButtonPaddedExtra { get; private set; }
        public GUIStyle ToolbarButtonNoStretch { get; private set; }

        public GUIStyle ListViewBox { get; private set; }
        public GUIStyle ListViewButton { get; private set; }
        public GUIStyle ListViewIconButton { get; private set; }
        public GUIStyle ListViewExtaButton { get; private set; }

        public GUIStyle BrushVisibilityControl { get; private set; }

        public GUIStyle TransparentBox { get; private set; }
        public GUIStyle OrientationBox { get; private set; }

        public GUIStyle Tooltip { get; private set; }

        public GUIStyle WindowGreyBorder { get; private set; }

        public GUIStyle ExtendedProperties_TitleShown { get; private set; }
        public GUIStyle ExtendedProperties_TitleHidden { get; private set; }
        public GUIStyle ExtendedProperties_HSplit { get; private set; }
        public GUIStyle ExtendedProperties_ScrollView { get; private set; }

        public GUIStyle PaddedScrollView { get; private set; }


        protected override void OnInitialize()
        {
            GUISkin skin = GUI.skin;

            this.InspectorBigTitle = "In BigTitle";

            this.Box = new GUIStyle(skin.box);
            this.Box.fixedHeight = this.Box.fixedWidth = 0;
            this.Box.margin = new RectOffset(2, 2, 2, 2);

            this.ListBox = new GUIStyle();
            this.ListBox.normal.background = Skin.ListBox;
            this.ListBox.border = new RectOffset(4, 4, 4, 4);

            this.OutlinedCornerBox = new GUIStyle();
            this.OutlinedCornerBox.normal.background = Skin.OutlinedCornerBox;
            this.OutlinedCornerBox.border = new RectOffset(13, 13, 13, 13);

            this.VariationBox = new GUIStyle();
            this.VariationBox.normal.background = Skin.VariationBox;
            this.VariationBox.border = new RectOffset(1, 1, 1, 1);

            this.DropTargetBackground = new GUIStyle();
            this.DropTargetBackground.normal.background = Skin.DropTargetBackground;
            this.DropTargetBackground.border = new RectOffset(2, 2, 2, 2);

            this.StatusWindow = new GUIStyle();
            this.StatusWindow.normal.background = Skin.StatusWindowBackground;
            this.StatusWindow.normal.textColor = new Color32(192, 192, 192, 255);
            this.StatusWindow.fontStyle = FontStyle.Bold;
            this.StatusWindow.alignment = TextAnchor.UpperCenter;
            this.StatusWindow.border = new RectOffset(1, 8, 20, 1);
            this.StatusWindow.padding = new RectOffset(5, 5, 19, 5);
            this.StatusWindow.overflow = new RectOffset(8, 8, 4, 12);
            this.StatusWindow.clipping = TextClipping.Clip;
            this.StatusWindow.contentOffset = new Vector2(0, -18);
            this.StatusWindow.richText = true;

            this.ButtonWide = new GUIStyle(skin.button);
            this.ButtonWide.padding.left = 34;
            this.ButtonWide.padding.right = 35;
            this.ButtonWide.stretchWidth = false;

            this.PreviewLabel = new GUIStyle(EditorStyles.miniLabel);
            this.PreviewLabel.wordWrap = true;
            this.PreviewLabel.alignment = TextAnchor.MiddleCenter;
            this.PreviewLabel.normal.textColor = new Color32(245, 245, 245, 255);

            this.SelectedPreviewLabel = new GUIStyle(this.PreviewLabel);
            this.SelectedPreviewLabel.normal.textColor = Color.white;

            this.WhiteWordWrappedMiniLabel = new GUIStyle(EditorStyles.whiteMiniLabel);
            this.WhiteWordWrappedMiniLabel.wordWrap = true;

            this.TitleLabel = new GUIStyle(EditorStyles.whiteLabel);
            this.TitleLabel.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.65f, 0.65f, 0.65f)
                : new Color(0.39f, 0.39f, 0.39f);
            this.TitleLabel.fontSize = 18;
            this.TitleLabel.fontStyle = FontStyle.Bold;

            this.BoldLabel = new GUIStyle(EditorStyles.label);
            this.BoldLabel.fontStyle = FontStyle.Bold;

            this.TabBackground = new GUIStyle();
            this.TabBackground.normal.background = Skin.TabBackground;
            this.TabBackground.fixedHeight = 24;

            this.Tab = new GUIStyle();
            this.Tab.normal.background = Skin.TabBackground;
            this.Tab.onNormal.background = Skin.Tab;
            this.Tab.border = new RectOffset(9, 9, 7, 0);
            this.Tab.stretchWidth = false;
            this.Tab.fixedHeight = 24;
            this.Tab.fontSize = 12;
            this.Tab.padding = new RectOffset(20, 20 + 2, 6, 0);

            if (EditorGUIUtility.isProSkin) {
                this.Tab.normal.textColor = new Color32(215, 215, 215, 255);
                this.Tab.onNormal.textColor = new Color(0.9f, 0.9f, 0.9f);
            }
            else {
                this.Tab.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            }

            this.MiniCenteredLabel = new GUIStyle(EditorStyles.miniLabel);
            this.MiniCenteredLabel.alignment = TextAnchor.MiddleCenter;

            this.SmallRemoveButton = new GUIStyle();
            this.SmallRemoveButton.fixedWidth = 18;
            this.SmallRemoveButton.fixedHeight = 18;
            this.SmallRemoveButton.normal.background = Skin.SmallRemoveButtonNormal;
            this.SmallRemoveButton.active.background = Skin.SmallRemoveButtonActive;

            this.LabelMiddleLeft = new GUIStyle(skin.label);
            this.LabelMiddleLeft.alignment = TextAnchor.MiddleLeft;
            this.LabelMiddleLeft.stretchWidth = false;
            this.LabelMiddleLeft.padding.top = 3;

            this.MiniSliderBorder = new GUIStyle();
            this.MiniSliderBorder.normal.background = Skin.MiniSliderBorder;
            this.MiniSliderBorder.border = new RectOffset(3, 3, 3, 3);
            if (EditorGUIUtility.isProSkin) {
                this.MiniSliderFillColor = new Color32(67, 67, 67, 255);
                this.MiniSliderEmptyColor = new Color32(29, 29, 29, 255);
                this.MiniSliderMarkerColor = new Color32(49, 49, 49, 255);
            }
            else {
                this.MiniSliderFillColor = new Color32(208, 208, 208, 255);
                this.MiniSliderEmptyColor = new Color32(100, 100, 100, 255);
                this.MiniSliderMarkerColor = new Color32(142, 142, 142, 255);
            }

            this.SmallCloseButton = skin.FindStyle("WinBtnClose");
            if (this.SmallCloseButton == null) {
                this.SmallCloseButton = skin.FindStyle("WinBtnCloseWin");
            }

            this.SearchTextField = skin.FindStyle("SearchTextField");
            this.SearchCancelButton = skin.FindStyle("SearchCancelButton");
            this.SearchCancelButtonEmpty = skin.FindStyle("SearchCancelButtonEmpty");

            this.TextFieldRoundEdge = new GUIStyle(EditorStyles.textField);
            this.TextFieldRoundEdge.border = new RectOffset(9, 1, 18, 0);
            this.TextFieldRoundEdge.margin = new RectOffset(this.SearchTextField.margin.left, this.SearchTextField.margin.right, this.SearchTextField.margin.top + 1, this.SearchTextField.margin.bottom);
            this.TextFieldRoundEdge.padding = new RectOffset(8, this.SearchTextField.padding.right, this.SearchTextField.padding.top, this.SearchTextField.padding.bottom);
            this.TextFieldRoundEdge.fixedHeight = 18;
            this.TextFieldRoundEdge.imagePosition = ImagePosition.ImageLeft;
            this.TextFieldRoundEdge.normal.background = Skin.TextPartRound;
            this.TextFieldRoundEdge.active.background = Skin.TextPartRound;
            this.TextFieldRoundEdge.active.textColor = this.TextFieldRoundEdge.normal.textColor;
            this.TextFieldRoundEdge.focused.background = Skin.TextPartRound;

            this.TextFieldRoundEdgeCancelButton = new GUIStyle(this.SearchCancelButton);
            this.TextFieldRoundEdgeCancelButton.margin.top += 1;
            this.TextFieldRoundEdgeCancelButtonEmpty = new GUIStyle(this.SearchCancelButtonEmpty);
            this.TextFieldRoundEdgeCancelButtonEmpty.margin.top += 1;

            this.TransparentTextField = new GUIStyle(EditorStyles.whiteLabel);
            this.TransparentTextField.normal.textColor = EditorStyles.textField.normal.textColor;

            this.Separator = new GUIStyle();
            this.Separator.normal.background = EditorGUIUtility.whiteTexture;
            this.Separator.stretchWidth = true;

            this.FlatToggle = new GUIStyle();
            this.FlatToggle.fixedHeight = 21;
            this.FlatToggle.border = new RectOffset(1, 17, 0, 0);
            this.FlatToggle.alignment = TextAnchor.MiddleLeft;
            this.FlatToggle.margin = new RectOffset(0, 0, 0, 5);
            this.FlatToggle.padding = new RectOffset(5, 20, 0, 2);
            if (EditorGUIUtility.isProSkin) {
                this.FlatToggle.normal.textColor = new Color32(150, 150, 150, 255);
                this.FlatToggle.active.textColor = new Color32(210, 210, 210, 255);
            }
            else {
                this.FlatToggle.normal.textColor = new Color32(35, 35, 35, 255);
                this.FlatToggle.active.textColor = new Color32(255, 255, 255, 255);
            }
            this.FlatToggle.onNormal.textColor = this.FlatToggle.normal.textColor;
            this.FlatToggle.onActive.textColor = this.FlatToggle.active.textColor;
            this.FlatToggle.normal.background = Skin.FlatToggleOff;
            this.FlatToggle.onNormal.background = Skin.FlatToggleOn;
            this.FlatToggle.active.background = Skin.FlatToggleOffActive;
            this.FlatToggle.onActive.background = Skin.FlatToggleOnActive;

            this.HorizontalSplitter = new GUIStyle();
            this.HorizontalSplitter.normal.background = Skin.HorizontalSplitThick;
            this.HorizontalSplitter.stretchHeight = true;
            this.HorizontalSplitter.fixedWidth = 6;

            GUIStyle hiLabel = skin.FindStyle("Hi Label");

            this.ListSectionElement = new GUIStyle(skin.label);
            this.ListSectionElement.alignment = TextAnchor.MiddleRight;
            this.ListSectionElement.padding = new RectOffset(0, 10, 0, 0);
            this.ListSectionElement.onNormal.background = hiLabel.onActive.background;
            this.ListSectionElement.onNormal.textColor = Color.white;
            this.ListSectionElement.fontSize = 12;

            this.ListLargeElement = new GUIStyle(skin.label);
            this.ListLargeElement.margin = new RectOffset();
            this.ListLargeElement.padding = new RectOffset(5, 5, 0, 2);
            this.ListLargeElement.alignment = TextAnchor.MiddleLeft;
            this.ListLargeElement.onNormal.background = hiLabel.onActive.background;
            this.ListLargeElement.onNormal.textColor = Color.white;

            this.BrushField = new GUIStyle("ObjectField");
            this.BrushField.font = EditorStyles.miniFont;
            this.BrushField.fontSize = 11;
            this.BrushField.stretchWidth = true;
            this.BrushField.padding = new RectOffset(2, 18 + 2, -2, 0);
            this.BrushField.fixedHeight = 16;
            this.BrushField.clipping = TextClipping.Clip;

            this.FoldoutTitle = new GUIStyle();
            this.FoldoutTitle.normal.background = Skin.ToggleTitleOff;
            this.FoldoutTitle.onNormal.background = Skin.ToggleTitleOn;
            this.FoldoutTitle.focused.background = Skin.ToggleTitleOff;
            this.FoldoutTitle.onFocused.background = Skin.ToggleTitleOn;
            this.FoldoutTitle.active.background = Skin.ToggleTitleOn;
            this.FoldoutTitle.onActive.background = Skin.ToggleTitleOn;
            this.FoldoutTitle.border = new RectOffset(23, 7, 26, 0);
            this.FoldoutTitle.padding = new RectOffset(27, 0, 5, 7);
            this.FoldoutTitle.fontSize = 12;
            this.FoldoutTitle.stretchWidth = true;
            this.FoldoutTitle.alignment = TextAnchor.MiddleLeft;
            this.FoldoutTitle.fixedHeight = 26;

            if (EditorGUIUtility.isProSkin) {
                this.FoldoutTitle.normal.textColor = new Color32(210, 210, 210, 255);
                this.FoldoutTitle.onNormal.textColor = new Color32(230, 230, 230, 255);
                this.FoldoutTitle.focused.textColor = new Color32(210, 210, 210, 255);
                this.FoldoutTitle.onFocused.textColor = new Color32(230, 230, 230, 255);
                this.FoldoutTitle.active.textColor = new Color32(230, 230, 230, 255);
                this.FoldoutTitle.onActive.textColor = new Color32(230, 230, 230, 255);
            }

            this.FoldoutSectionPadded = new GUIStyle();
            this.FoldoutSectionPadded.padding = new RectOffset(5, 5, 5, 5);

            this.InspectorSectionPadded = new GUIStyle();
            this.InspectorSectionPadded.padding = new RectOffset(8, 8, 5, 5);

            this.ExtendedPropertiesLeader = new GUIStyle();
            this.ExtendedPropertiesLeader.padding = new RectOffset(5, 5, 0, 7);

            this.SmallButton = new GUIStyle(skin.button);
            this.SmallButton.normal.background = Skin.SmallButtonNormal;
            this.SmallButton.hover.background = Skin.SmallButtonHover;
            this.SmallButton.active.background = Skin.SmallButtonActive;
            this.SmallButton.border = new RectOffset(5, 5, 5, 5);
            this.SmallButton.padding.top = 0;
            this.SmallButton.padding.bottom = 0;
            this.SmallButton.padding.left = 2;
            this.SmallButton.padding.right = 2;

            this.SmallFlatButton = new GUIStyle(this.SmallButton);
            this.SmallFlatButton.normal = skin.label.normal;

            this.SmallFlatButtonFake = new GUIStyle(this.SmallFlatButton);
            this.SmallFlatButtonFake.hover = skin.label.normal;

            this.FlatButton = new GUIStyle(skin.button);
            this.FlatButton.normal = skin.label.normal;
            this.FlatButton.hover = this.SmallButton.hover;
            this.FlatButton.active = this.SmallButton.active;

            this.FlatButtonNoMargin = new GUIStyle(this.FlatButton);
            this.FlatButtonNoMargin.margin = new RectOffset();

            this.FlatButtonFake = new GUIStyle(skin.button);
            this.FlatButtonFake.normal = skin.label.normal;

            this.HistoryNavButton = new GUIStyle(this.FlatButton);
            this.HistoryNavButton.margin = new RectOffset(1, 0, 0, 0);
            this.HistoryNavButton.padding = new RectOffset(6, 6, 0, 0);
            this.HistoryNavButton.fixedHeight = 21;
            this.HistoryNavButton.stretchWidth = false;

            this.ToolButton = new GUIStyle(GUI.skin.button);
            this.ToolButton.margin = new RectOffset(2, 2, 2, 2);
            this.ToolButton.padding = new RectOffset(0, 0, 0, 1);
            this.ToolButton.fixedHeight = 27;
            this.ToolButton.fontSize = 10;
            this.ToolButton.alignment = TextAnchor.MiddleCenter;
            this.ToolButton.normal.background = null;
            this.ToolButton.hover = this.FlatButton.hover;
            this.ToolButton.active = this.FlatButton.active;

            this.SelectedBrushPreviewBox = new GUIStyle(skin.box);
            this.SelectedBrushPreviewBox.fixedWidth = 42;
            this.SelectedBrushPreviewBox.fixedHeight = 42;
            this.SelectedBrushPreviewBox.padding = new RectOffset(2, 2, 2, 2);

            this.ButtonPaddedExtra = new GUIStyle(skin.button);
            this.ButtonPaddedExtra.padding.left = 25;
            this.ButtonPaddedExtra.padding.right = 25;
            this.ButtonPaddedExtra.stretchWidth = false;

            this.ToolbarButtonPadded = new GUIStyle(EditorStyles.toolbarButton);
            this.ToolbarButtonPadded.padding = new RectOffset(10, 10, 0, 0);

            this.ToolbarButtonPaddedExtra = new GUIStyle(EditorStyles.toolbarButton);
            this.ToolbarButtonPaddedExtra.padding = new RectOffset(20, 20, 0, 0);

            this.ToolbarButtonNoStretch = new GUIStyle(EditorStyles.toolbarButton);
            this.ToolbarButtonNoStretch.stretchWidth = false;

            this.ListViewBox = new GUIStyle();
            this.ListViewBox.border = new RectOffset(0, 0, 2, 2);
            this.ListViewBox.normal.background = Skin.BrushListBackground;

            this.ListViewIconButton = new GUIStyle(skin.button);
            this.ListViewIconButton.clipping = TextClipping.Clip;
            this.ListViewIconButton.alignment = TextAnchor.MiddleCenter;
            this.ListViewIconButton.wordWrap = true;
            this.ListViewIconButton.margin = new RectOffset(4, 4, 3, 3);
            this.ListViewIconButton.padding = new RectOffset(6, 6, 2, 3);
            this.ListViewIconButton.border = new RectOffset(5, 5, 5, 5);
            this.ListViewIconButton.overflow = new RectOffset(0, 0, 0, 1);
            this.ListViewIconButton.normal.background = Skin.IconButtonNormal;
            this.ListViewIconButton.onNormal.background = Skin.IconButtonOn;
            this.ListViewIconButton.active.background = Skin.IconButtonActive;
            this.ListViewIconButton.onActive.background = Skin.IconButtonOnActive;
            if (EditorGUIUtility.isProSkin) {
                this.ListViewIconButton.normal.textColor = new Color32(232, 232, 232, 255);
                this.ListViewIconButton.active.textColor = new Color32(30, 30, 30, 255);
                this.ListViewIconButton.onNormal.textColor = new Color32(19, 19, 19, 255);
                this.ListViewIconButton.onActive.textColor = new Color32(220, 220, 220, 255);
            }
            else {
                this.ListViewIconButton.onNormal.textColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            this.ListViewExtaButton = new GUIStyle(this.ListViewIconButton);
            this.ListViewExtaButton.border = new RectOffset(5, 5, 5, 5);
            this.ListViewExtaButton.onNormal.background = Skin.ListButtonOn;
            this.ListViewExtaButton.onActive.background = Skin.ListButtonOnActive;
            this.ListViewExtaButton.onNormal.textColor = Color.white;
            this.ListViewExtaButton.onActive.textColor = Color.white;
            if (EditorGUIUtility.isProSkin) {
                this.ListViewExtaButton.normal.background = this.SmallButton.normal.background;
                this.ListViewExtaButton.normal.textColor = new Color32(232, 232, 232, 255);
                this.ListViewExtaButton.active.background = this.SmallButton.active.background;
                this.ListViewExtaButton.active.textColor = new Color32(190, 190, 190, 255);
            }
            else {
                this.ListViewExtaButton.normal.background = Skin.ListButtonNormal;
                this.ListViewExtaButton.normal.textColor = new Color32(40, 40, 40, 255);
                this.ListViewExtaButton.active.background = Skin.ListButtonActive;
            }

            this.ListViewButton = new GUIStyle(this.ListViewExtaButton);
            this.ListViewButton.alignment = TextAnchor.MiddleLeft;
            this.ListViewButton.padding = new RectOffset(59, 0, 0, 3);
            if (EditorGUIUtility.isProSkin) {
                this.ListViewButton.normal.background = this.SmallButton.normal.background;
                this.ListViewButton.normal.textColor = new Color32(220, 220, 220, 255);
                this.ListViewButton.active.textColor = new Color32(190, 190, 190, 255);
                this.ListViewButton.onNormal.textColor = Color.white;
                this.ListViewButton.onActive.textColor = Color.white;
            }

            this.ListViewExtaButton.hover = this.ListViewExtaButton.normal;
            this.ListViewButton.hover = this.ListViewButton.normal;

            this.BrushVisibilityControl = new GUIStyle(EditorStyles.popup);
            this.BrushVisibilityControl.margin = new RectOffset(0, 5, 5, 0);
            this.BrushVisibilityControl.padding = new RectOffset(5, 0, 0, 0);
            this.BrushVisibilityControl.stretchWidth = false;

            this.TransparentBox = new GUIStyle();
            this.TransparentBox.border = new RectOffset(1, 1, 1, 1);
            this.TransparentBox.normal.background = Skin.TransparentBox;

            this.OrientationBox = new GUIStyle();
            this.OrientationBox.border = new RectOffset(2, 2, 2, 2);
            this.OrientationBox.normal.background = Skin.OrientationBox;

            this.Tooltip = new GUIStyle();
            this.Tooltip.padding = new RectOffset(6, 6, 3, 4);
            this.Tooltip.border = new RectOffset(3, 4, 3, 3);
            this.Tooltip.normal.background = Skin.Tooltip;
            this.Tooltip.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color32(20, 20, 20, 255)
                : new Color32(255, 255, 255, 255);
            this.Tooltip.alignment = TextAnchor.MiddleCenter;

            this.ExtendedProperties_TitleShown = new GUIStyle();
            this.ExtendedProperties_TitleShown.border = new RectOffset(1, 18, 16, 1);
            this.ExtendedProperties_TitleShown.padding = new RectOffset(6, 0, 2, 1);
            this.ExtendedProperties_TitleShown.onNormal.background = Skin.ExtendedProperties_TitleShown;
            this.ExtendedProperties_TitleShown.onActive.background = Skin.ExtendedProperties_TitleShownPressed;

            if (EditorGUIUtility.isProSkin) {
                this.ExtendedProperties_TitleShown.onNormal.textColor = new Color32(190, 190, 190, 255);
                this.ExtendedProperties_TitleShown.onActive.textColor = new Color32(190, 190, 190, 255);
            }
            else {
                this.ExtendedProperties_TitleShown.onNormal.textColor = Color.white;
                this.ExtendedProperties_TitleShown.onActive.textColor = Color.white;
            }

            this.ExtendedProperties_TitleHidden = new GUIStyle();
            this.ExtendedProperties_TitleHidden.fixedWidth = 21;
            this.ExtendedProperties_TitleHidden.stretchHeight = true;
            this.ExtendedProperties_TitleHidden.border = new RectOffset(1, 20, 16, 1);
            this.ExtendedProperties_TitleHidden.padding = new RectOffset(0, 2, 17, 0);
            this.ExtendedProperties_TitleHidden.normal.background = Skin.ExtendedProperties_TitleHidden;
            this.ExtendedProperties_TitleHidden.normal.textColor = Color.white;
            this.ExtendedProperties_TitleHidden.active.background = Skin.ExtendedProperties_TitleHiddenPressed;
            this.ExtendedProperties_TitleHidden.active.textColor = Color.white;

            this.ExtendedProperties_HSplit = new GUIStyle();
            this.ExtendedProperties_HSplit.normal.background = Skin.HorizontalSplitThick;
            this.ExtendedProperties_HSplit.stretchHeight = true;
            this.ExtendedProperties_HSplit.fixedWidth = 6;

            this.PaddedScrollView = new GUIStyle(skin.scrollView);
            this.PaddedScrollView.padding = new RectOffset(10, 6, 9, 10);

            this.ExtendedProperties_ScrollView = new GUIStyle(skin.scrollView);
            this.ExtendedProperties_ScrollView.padding = new RectOffset(8, 2, 5, 10);

            this.WindowGreyBorder = "grey_border";
        }


        [System.Serializable]
        public sealed class SkinInfo
        {
            [System.NonSerialized]
            private Dictionary<int, Texture2D> invertedTextureCache = new Dictionary<int, Texture2D>();


            [SerializeField]
            private Texture2D texAddFindOrientation = null;
            [SerializeField]
            private Texture2D texAddVariationOverlay = null;
            [SerializeField]
            private Texture2D texAutotileBasicIcon = null;
            [SerializeField]
            private Texture2D texAutotileBasicPreview = null;
            [SerializeField]
            private Texture2D texAutotileExtendedIcon = null;
            [SerializeField]
            private Texture2D texAutotileExtendedPreview = null;
            [SerializeField]
            private Texture2D texBadge = null;
            [SerializeField]
            private Texture2D texBrushListBackground = null;
            [SerializeField]
            private Texture2D texBrushRound = null;
            [SerializeField]
            private Texture2D texBrushSquare = null;
            [SerializeField]
            private Texture2D texCaution = null;
            [SerializeField]
            private Texture2D texCentralize = null;
            [SerializeField]
            private Texture2D texCentralizeUsed = null;
            [SerializeField]
            private Texture2D texChunkToggle = null;
            [SerializeField]
            private Texture2D texContextHelp = null;
            [SerializeField]
            private Texture2D texContextMenu = null;
            [SerializeField]
            private Texture2D texCursor_Brush = null;
            [SerializeField]
            private Texture2D texCursor_Cycle = null;
            [SerializeField]
            private Texture2D texCursor_Fill = null;
            [SerializeField]
            private Texture2D texCursor_Line_Mac = null;
            [SerializeField]
            private Texture2D texCursor_Line_Win = null;
            [SerializeField]
            private Texture2D texCursor_Picker = null;
            [SerializeField]
            private Texture2D texCursor_Plop = null;
            [SerializeField]
            private Texture2D texCursor_PlopCycle = null;
            [SerializeField]
            private Texture2D texCursor_Rectangle_Mac = null;
            [SerializeField]
            private Texture2D texCursor_Rectangle_Win = null;
            [SerializeField]
            private Texture2D texCursor_Spray = null;
            [SerializeField]
            private Texture2D texDefaultWindowIcon = null;
            [SerializeField]
            private Texture2D texDownArrow = null;
            [SerializeField]
            private Texture2D texDropTargetBackground = null;
            [SerializeField]
            private Texture2D texDroplet = null;
            [SerializeField]
            private Texture2D texEditLabel = null;
            [SerializeField]
            private Texture2D texEmptyPreview = null;
            [SerializeField]
            private Texture2D texExtendedProperties_TitleHidden = null;
            [SerializeField]
            private Texture2D texExtendedProperties_TitleHiddenPressed = null;
            [SerializeField]
            private Texture2D texExtendedProperties_TitleShown = null;
            [SerializeField]
            private Texture2D texExtendedProperties_TitleShownPressed = null;
            [SerializeField]
            private Texture2D texEyeOpen = null;
            [SerializeField]
            private Texture2D texEyeShut = null;
            [SerializeField]
            private Texture2D texFallbackBrushPreview = null;
            [SerializeField]
            private Texture2D texFilterIcon = null;
            [SerializeField]
            private Texture2D texFlatToggleOff = null;
            [SerializeField]
            private Texture2D texFlatToggleOffActive = null;
            [SerializeField]
            private Texture2D texFlatToggleOn = null;
            [SerializeField]
            private Texture2D texFlatToggleOnActive = null;
            [SerializeField]
            private Texture2D texGearButton = null;
            [SerializeField]
            private Texture2D texGearTool = null;
            [SerializeField]
            private Texture2D texGotoTarget = null;
            [SerializeField]
            private Texture2D texGotoTileset = null;
            [SerializeField]
            private Texture2D texGridToggle = null;
            [SerializeField]
            private Texture2D texHorizontalSplitThick = null;
            [SerializeField]
            private Texture2D texIconButtonActive = null;
            [SerializeField]
            private Texture2D texIconButtonNormal = null;
            [SerializeField]
            private Texture2D texIconButtonOn = null;
            [SerializeField]
            private Texture2D texIconButtonOnActive = null;
            [SerializeField]
            private Texture2D texIcon_PresetTileSystem = null;
            [SerializeField]
            private Texture2D texLeftArrow = null;
            [SerializeField]
            private Texture2D texLinkFields = null;
            [SerializeField]
            private Texture2D texListBox = null;
            [SerializeField]
            private Texture2D texListButtonActive = null;
            [SerializeField]
            private Texture2D texListButtonNormal = null;
            [SerializeField]
            private Texture2D texListButtonOn = null;
            [SerializeField]
            private Texture2D texListButtonOnActive = null;
            [SerializeField]
            private Texture2D texListView_Icons = null;
            [SerializeField]
            private Texture2D texListView_List = null;
            [SerializeField]
            private Texture2D texLock = null;
            [SerializeField]
            private Texture2D texLockActive = null;
            [SerializeField]
            private Texture2D texMappingArrow = null;
            [SerializeField]
            private Texture2D texMenuButton = null;
            [SerializeField]
            private Texture2D texMiniSliderBorder = null;
            [SerializeField]
            private Texture2D texMouseLeft = null;
            [SerializeField]
            private Texture2D texMouseRight = null;
            [SerializeField]
            private Texture2D texOrientationBox = null;
            [SerializeField]
            private Texture2D texOutlinedCornerBox = null;
            [SerializeField]
            private Texture2D texOverlay_Alias = null;
            [SerializeField]
            private Texture2D texOverlay_Brush = null;
            [SerializeField]
            private Texture2D texPickBrush = null;
            [SerializeField]
            private Texture2D texPickPrefab = null;
            [SerializeField]
            private Texture2D texRandomize = null;
            [SerializeField]
            private Texture2D texRecentHistory = null;
            [SerializeField]
            private Texture2D texRectangleFill = null;
            [SerializeField]
            private Texture2D texRectangleOutline = null;
            [SerializeField]
            private Texture2D texRectanglePaintAround = null;
            [SerializeField]
            private Texture2D texRefreshIcon = null;
            [SerializeField]
            private Texture2D texRightArrow = null;
            [SerializeField]
            private Texture2D texRotationSelector = null;
            [SerializeField]
            private Texture2D texSmallButtonActive = null;
            [SerializeField]
            private Texture2D texSmallButtonHover = null;
            [SerializeField]
            private Texture2D texSmallButtonNormal = null;
            [SerializeField]
            private Texture2D texSmallGearButton = null;
            [SerializeField]
            private Texture2D texSmallRemoveButtonActive = null;
            [SerializeField]
            private Texture2D texSmallRemoveButtonNormal = null;
            [SerializeField]
            private Texture2D texSnapCells = null;
            [SerializeField]
            private Texture2D texSnapFreeX = null;
            [SerializeField]
            private Texture2D texSnapFreeY = null;
            [SerializeField]
            private Texture2D texSnapPoints = null;
            [SerializeField]
            private Texture2D texSoftClipping = null;
            [SerializeField]
            private Texture2D texSortAsc = null;
            [SerializeField]
            private Texture2D texSortDesc = null;
            [SerializeField]
            private Texture2D texStatusWindowBackground = null;
            [SerializeField]
            private Texture2D texSwitchPrimarySecondary = null;
            [SerializeField]
            private Texture2D texTab = null;
            [SerializeField]
            private Texture2D texTabBackground = null;
            [SerializeField]
            private Texture2D texTextPartRound = null;
            [SerializeField]
            private Texture2D texToggleRotationalSymmetry = null;
            [SerializeField]
            private Texture2D texToggleRotationalSymmetryOn = null;
            [SerializeField]
            private Texture2D texToggleTitleOff = null;
            [SerializeField]
            private Texture2D texToggleTitleOn = null;
            [SerializeField]
            private Texture2D texToolCycle = null;
            [SerializeField]
            private Texture2D texToolFill = null;
            [SerializeField]
            private Texture2D texToolLine = null;
            [SerializeField]
            private Texture2D texToolPaint = null;
            [SerializeField]
            private Texture2D texToolPicker = null;
            [SerializeField]
            private Texture2D texToolPlop = null;
            [SerializeField]
            private Texture2D texToolRectangle = null;
            [SerializeField]
            private Texture2D texToolSpray = null;
            [SerializeField]
            private Texture2D texTooltip = null;
            [SerializeField]
            private Texture2D texTooltipArrow = null;
            [SerializeField]
            private Texture2D texTransparentBox = null;
            [SerializeField]
            private Texture2D texTrim = null;
            [SerializeField]
            private Texture2D texVariationBox = null;
            [SerializeField]
            private Texture2D texVariationOffsetSelector = null;
            [SerializeField]
            private Texture2D texVendorBadge = null;
            [SerializeField]
            private Texture2D texZoomIcon = null;


            public Texture2D AddFindOrientation {
                get { return this.texAddFindOrientation; }
            }
            public Texture2D AddVariationOverlay {
                get { return this.texAddVariationOverlay; }
            }
            public Texture2D AutotileBasicIcon {
                get { return this.texAutotileBasicIcon; }
            }
            public Texture2D AutotileBasicPreview {
                get { return this.texAutotileBasicPreview; }
            }
            public Texture2D AutotileExtendedIcon {
                get { return this.texAutotileExtendedIcon; }
            }
            public Texture2D AutotileExtendedPreview {
                get { return this.texAutotileExtendedPreview; }
            }
            public Texture2D Badge {
                get { return this.texBadge; }
            }
            public Texture2D BrushListBackground {
                get { return this.texBrushListBackground; }
            }
            public Texture2D BrushRound {
                get { return this.texBrushRound; }
            }
            public Texture2D BrushSquare {
                get { return this.texBrushSquare; }
            }
            public Texture2D Caution {
                get { return this.texCaution; }
            }
            public Texture2D Centralize {
                get { return this.texCentralize; }
            }
            public Texture2D CentralizeUsed {
                get { return this.texCentralizeUsed; }
            }
            public Texture2D ChunkToggle {
                get { return this.texChunkToggle; }
            }
            public Texture2D ContextHelp {
                get { return this.texContextHelp; }
            }
            public Texture2D ContextMenu {
                get { return this.texContextMenu; }
            }
            public Texture2D Cursor_Brush {
                get { return this.texCursor_Brush; }
            }
            public Texture2D Cursor_Cycle {
                get { return this.texCursor_Cycle; }
            }
            public Texture2D Cursor_Fill {
                get { return this.texCursor_Fill; }
            }
            public Texture2D Cursor_Line_Mac {
                get { return this.texCursor_Line_Mac; }
            }
            public Texture2D Cursor_Line_Win {
                get { return this.texCursor_Line_Win; }
            }
            public Texture2D Cursor_Picker {
                get { return this.texCursor_Picker; }
            }
            public Texture2D Cursor_Plop {
                get { return this.texCursor_Plop; }
            }
            public Texture2D Cursor_PlopCycle {
                get { return this.texCursor_PlopCycle; }
            }
            public Texture2D Cursor_Rectangle_Mac {
                get { return this.texCursor_Rectangle_Mac; }
            }
            public Texture2D Cursor_Rectangle_Win {
                get { return this.texCursor_Rectangle_Win; }
            }
            public Texture2D Cursor_Spray {
                get { return this.texCursor_Spray; }
            }
            public Texture2D DefaultWindowIcon {
                get { return this.texDefaultWindowIcon; }
            }
            public Texture2D DownArrow {
                get { return this.texDownArrow; }
            }
            public Texture2D DropTargetBackground {
                get { return this.texDropTargetBackground; }
            }
            public Texture2D Droplet {
                get { return this.texDroplet; }
            }
            public Texture2D EditLabel {
                get { return this.texEditLabel; }
            }
            public Texture2D EmptyPreview {
                get { return this.texEmptyPreview; }
            }
            public Texture2D ExtendedProperties_TitleHidden {
                get { return this.texExtendedProperties_TitleHidden; }
            }
            public Texture2D ExtendedProperties_TitleHiddenPressed {
                get { return this.texExtendedProperties_TitleHiddenPressed; }
            }
            public Texture2D ExtendedProperties_TitleShown {
                get { return this.texExtendedProperties_TitleShown; }
            }
            public Texture2D ExtendedProperties_TitleShownPressed {
                get { return this.texExtendedProperties_TitleShownPressed; }
            }
            public Texture2D EyeOpen {
                get { return this.texEyeOpen; }
            }
            public Texture2D EyeShut {
                get { return this.texEyeShut; }
            }
            public Texture2D FallbackBrushPreview {
                get { return this.texFallbackBrushPreview; }
            }
            public Texture2D FilterIcon {
                get { return this.texFilterIcon; }
            }
            public Texture2D FlatToggleOff {
                get { return this.texFlatToggleOff; }
            }
            public Texture2D FlatToggleOffActive {
                get { return this.texFlatToggleOffActive; }
            }
            public Texture2D FlatToggleOn {
                get { return this.texFlatToggleOn; }
            }
            public Texture2D FlatToggleOnActive {
                get { return this.texFlatToggleOnActive; }
            }
            public Texture2D GearButton {
                get { return this.texGearButton; }
            }
            public Texture2D GearTool {
                get { return this.texGearTool; }
            }
            public Texture2D GotoTarget {
                get { return this.texGotoTarget; }
            }
            public Texture2D GotoTileset {
                get { return this.texGotoTileset; }
            }
            public Texture2D GridToggle {
                get { return this.texGridToggle; }
            }
            public Texture2D HorizontalSplitThick {
                get { return this.texHorizontalSplitThick; }
            }
            public Texture2D IconButtonActive {
                get { return this.texIconButtonActive; }
            }
            public Texture2D IconButtonNormal {
                get { return this.texIconButtonNormal; }
            }
            public Texture2D IconButtonOn {
                get { return this.texIconButtonOn; }
            }
            public Texture2D IconButtonOnActive {
                get { return this.texIconButtonOnActive; }
            }
            public Texture2D Icon_PresetTileSystem {
                get { return this.texIcon_PresetTileSystem; }
            }
            public Texture2D LeftArrow {
                get { return this.texLeftArrow; }
            }
            public Texture2D LinkFields {
                get { return this.texLinkFields; }
            }
            public Texture2D ListBox {
                get { return this.texListBox; }
            }
            public Texture2D ListButtonActive {
                get { return this.texListButtonActive; }
            }
            public Texture2D ListButtonNormal {
                get { return this.texListButtonNormal; }
            }
            public Texture2D ListButtonOn {
                get { return this.texListButtonOn; }
            }
            public Texture2D ListButtonOnActive {
                get { return this.texListButtonOnActive; }
            }
            public Texture2D ListView_Icons {
                get { return this.texListView_Icons; }
            }
            public Texture2D ListView_List {
                get { return this.texListView_List; }
            }
            public Texture2D Lock {
                get { return this.texLock; }
            }
            public Texture2D LockActive {
                get { return this.texLockActive; }
            }
            public Texture2D MappingArrow {
                get { return this.texMappingArrow; }
            }
            public Texture2D MenuButton {
                get { return this.texMenuButton; }
            }
            public Texture2D MiniSliderBorder {
                get { return this.texMiniSliderBorder; }
            }
            public Texture2D MouseLeft {
                get { return this.texMouseLeft; }
            }
            public Texture2D MouseRight {
                get { return this.texMouseRight; }
            }
            public Texture2D OrientationBox {
                get { return this.texOrientationBox; }
            }
            public Texture2D OutlinedCornerBox {
                get { return this.texOutlinedCornerBox; }
            }
            public Texture2D Overlay_Alias {
                get { return this.texOverlay_Alias; }
            }
            public Texture2D Overlay_Brush {
                get { return this.texOverlay_Brush; }
            }
            public Texture2D PickBrush {
                get { return this.texPickBrush; }
            }
            public Texture2D PickPrefab {
                get { return this.texPickPrefab; }
            }
            public Texture2D Randomize {
                get { return this.texRandomize; }
            }
            public Texture2D RecentHistory {
                get { return this.texRecentHistory; }
            }
            public Texture2D RectangleFill {
                get { return this.texRectangleFill; }
            }
            public Texture2D RectangleOutline {
                get { return this.texRectangleOutline; }
            }
            public Texture2D RectanglePaintAround {
                get { return this.texRectanglePaintAround; }
            }
            public Texture2D RefreshIcon {
                get { return this.texRefreshIcon; }
            }
            public Texture2D RightArrow {
                get { return this.texRightArrow; }
            }
            public Texture2D RotationSelector {
                get { return this.texRotationSelector; }
            }
            public Texture2D SmallButtonActive {
                get { return this.texSmallButtonActive; }
            }
            public Texture2D SmallButtonHover {
                get { return this.texSmallButtonHover; }
            }
            public Texture2D SmallButtonNormal {
                get { return this.texSmallButtonNormal; }
            }
            public Texture2D SmallGearButton {
                get { return this.texSmallGearButton; }
            }
            public Texture2D SmallRemoveButtonActive {
                get { return this.texSmallRemoveButtonActive; }
            }
            public Texture2D SmallRemoveButtonNormal {
                get { return this.texSmallRemoveButtonNormal; }
            }
            public Texture2D SnapCells {
                get { return this.texSnapCells; }
            }
            public Texture2D SnapFreeX {
                get { return this.texSnapFreeX; }
            }
            public Texture2D SnapFreeY {
                get { return this.texSnapFreeY; }
            }
            public Texture2D SnapPoints {
                get { return this.texSnapPoints; }
            }
            public Texture2D SoftClipping {
                get { return this.texSoftClipping; }
            }
            public Texture2D SortAsc {
                get { return this.texSortAsc; }
            }
            public Texture2D SortDesc {
                get { return this.texSortDesc; }
            }
            public Texture2D StatusWindowBackground {
                get { return this.texStatusWindowBackground; }
            }
            public Texture2D SwitchPrimarySecondary {
                get { return this.texSwitchPrimarySecondary; }
            }
            public Texture2D Tab {
                get { return this.texTab; }
            }
            public Texture2D TabBackground {
                get { return this.texTabBackground; }
            }
            public Texture2D TextPartRound {
                get { return this.texTextPartRound; }
            }
            public Texture2D ToggleRotationalSymmetry {
                get { return this.texToggleRotationalSymmetry; }
            }
            public Texture2D ToggleRotationalSymmetryOn {
                get { return this.texToggleRotationalSymmetryOn; }
            }
            public Texture2D ToggleTitleOff {
                get { return this.texToggleTitleOff; }
            }
            public Texture2D ToggleTitleOn {
                get { return this.texToggleTitleOn; }
            }
            public Texture2D ToolCycle {
                get { return this.texToolCycle; }
            }
            public Texture2D ToolFill {
                get { return this.texToolFill; }
            }
            public Texture2D ToolLine {
                get { return this.texToolLine; }
            }
            public Texture2D ToolPaint {
                get { return this.texToolPaint; }
            }
            public Texture2D ToolPicker {
                get { return this.texToolPicker; }
            }
            public Texture2D ToolPlop {
                get { return this.texToolPlop; }
            }
            public Texture2D ToolRectangle {
                get { return this.texToolRectangle; }
            }
            public Texture2D ToolSpray {
                get { return this.texToolSpray; }
            }
            public Texture2D Tooltip {
                get { return this.texTooltip; }
            }
            public Texture2D TooltipArrow {
                get { return this.texTooltipArrow; }
            }
            public Texture2D TransparentBox {
                get { return this.texTransparentBox; }
            }
            public Texture2D Trim {
                get { return this.texTrim; }
            }
            public Texture2D VariationBox {
                get { return this.texVariationBox; }
            }
            public Texture2D VariationOffsetSelector {
                get { return this.texVariationOffsetSelector; }
            }
            public Texture2D VendorBadge {
                get { return this.texVendorBadge; }
            }
            public Texture2D ZoomIcon {
                get { return this.texZoomIcon; }
            }


            public Texture2D GetInverted(Texture2D texture)
            {
                Texture2D invertedTexture;
                if (!this.invertedTextureCache.TryGetValue(texture.GetInstanceID(), out invertedTexture)) {
                    invertedTexture = TextureUtility.Invert(texture);
                    this.invertedTextureCache[texture.GetInstanceID()] = invertedTexture;
                }
                return invertedTexture;
            }

            public Texture2D GetActive(Texture2D texture, bool active)
            {
                return active ? this.GetInverted(texture) : texture;
            }
        }
    }
}
