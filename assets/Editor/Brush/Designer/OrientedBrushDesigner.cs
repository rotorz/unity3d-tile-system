// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Designer for <see cref="OrientedBrush"/> brushes.
    /// </summary>
    /// <remarks>
    /// <para>Custom designers can be derived for specialized types of oriented
    /// brushes. Be sure to invoke base functionality when overriding methods.</para>
    /// </remarks>
    /// <example>
    /// <para>Register custom oriented brush designer:</para>
    /// <code language="csharp"><![CDATA[
    /// [InitializeOnLoad]
    /// public class MySpecialOrientedBrushDesigner : OrientedDesigner
    /// {
    ///     static MySpecialOrientedBrushDesigner()
    ///     {
    ///         BrushUtility.RegisterDescriptor<
    ///             // Custom brush that needs an editor
    ///             MySpecialOrientedBrush,
    ///             // Primary designer for instances of brush
    ///             MySpecialOrientedBrushDesigner,
    ///             // Alias designer for alias instances of brush
    ///             AliasBrushDesigner
    ///         >();
    ///     }
    ///
    ///
    ///     public override void OnGUI()
    ///     {
    ///         // Add custom GUI before regular oriented brush GUI
    ///         if (GUILayout.Button("Do Something")) {
    ///         }
    ///
    ///         base.OnGUI();
    ///
    ///         // Add custom GUI after regular oriented brush GUI
    ///         if (GUILayout.Button("Do Something Else")) {
    ///         }
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public class OrientedBrushDesigner : BrushDesignerView
    {
        /// <summary>
        /// Gets the oriented brush that is being edited.
        /// </summary>
        public OrientedBrush OrientedBrush { get; private set; }


        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.OrientedBrush = Brush as OrientedBrush;
        }


        /// <summary>
        /// Position of the "Define or Find Orientation" tool button in header area.
        /// </summary>
        private Rect buttonPositionDefineOrFindOrientation;


        /// <inheritdoc/>
        public override void DrawSecondaryMenuButton(Rect position)
        {
            if (Event.current.type == EventType.Repaint) {
                this.buttonPositionDefineOrFindOrientation = ExtraEditorGUI.GUIToScreenRect(position);
            }

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.AddFindOrientation,
                TileLang.FormatActionWithShortcut(
                    TileLang.ParticularText("Action", "Define or Find Orientation"), "F3"
                )
            )) {
                if (DefineOrientationWindow.OwnerWindow == this.Window) {
                    RotorzEditorGUI.StickyHoverButton(position, content);
                }
                else {
                    if (RotorzEditorGUI.ImmediateHoverButton(position, content)) {
                        this.ShowDefineOrFindOrientationPopup();
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            // Permit shortcut key "F3".
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F3) {
                Event.current.Use();
                this.ShowDefineOrFindOrientationPopup();
                GUIUtility.ExitGUI();
            }

            GUILayoutUtility.GetRect(0, 3);

            this.Section_Orientations();
            GUILayoutUtility.GetRect(0, 7);

            this.Section_MaterialMapper();
            GUILayoutUtility.GetRect(0, 7);
        }

        /// <inheritdoc/>
        public override void OnExtendedPropertiesGUI()
        {
            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Override Brush Transforms"),
                TileLang.Text("Overrides transform of nested brushes.")
            )) {
                this.Brush.overrideTransforms = EditorGUILayout.ToggleLeft(content, this.Brush.overrideTransforms);
                ExtraEditorGUI.TrailingTip(content);
            }

            base.OnExtendedPropertiesGUI();
        }

        /// <inheritdoc/>
        protected internal override void EndExtendedProperties()
        {
            ShowExtendedOrientation = RotorzEditorGUI.FoldoutSection(ShowExtendedOrientation,
                label: TileLang.Text("Automatic Orientations"),
                callback: this.OnExtendedGUI_AutomaticOrientation
            );

            base.EndExtendedProperties();
        }

        private void OnExtendedGUI_AutomaticOrientation()
        {
            this.OnExtendedGUI_Coalescing();

            ExtraEditorGUI.SeparatorLight();

            using (var content = ControlContent.WithTrailableTip(
                TileLang.ParticularText("Property", "Fallback"),
                TileLang.Text("Fallback to use when orientation is unavailable.")
            )) {
                FallbackMode guiInputFallbackMode = (FallbackMode)EditorGUILayout.EnumPopup(content, this.OrientedBrush.FallbackMode);
                if (this.OrientedBrush.FallbackMode != guiInputFallbackMode) {
                    this.OrientedBrush.FallbackMode = guiInputFallbackMode;
                }
                ExtraEditorGUI.TrailingTip(content);
            }
        }


        #region Variation Validation

        /// <summary>
        /// Validate new variation object and get error message on failure.
        /// </summary>
        /// <param name="variation">The new variation object.</param>
        /// <returns>
        /// A value of <c>null</c> if variation was valid; otherwise an error message.
        /// </returns>
        /// <seealso cref="ValidateNewVariation"/>
        /// <seealso cref="ValidateNewVariationWithUserNotification"/>
        private string ValidateNewVariationWithMessage(Object variation)
        {
            // Ensure that valid variation type was specified.
            if (!(variation is GameObject || variation is Brush)) {
                return TileLang.ParticularText("Error", "Invalid variation type.");
            }

            // Display warning if attempting to add model prefab.
            if (PrefabUtility.GetPrefabType(variation) == PrefabType.ModelPrefab) {
                return TileLang.ParticularText("Error", "Cannot add model to orientation; create a prefab instead.");
            }

            // Prevent nesting of non-physical brushes.
            var variationBrush = variation as Brush;
            if (variationBrush != null) {
                var targetBrush = variationBrush;

                // Get the target brush (i.e. the brush or target of alias).
                var variationAliasBrush = variationBrush as AliasBrush;
                if (variationAliasBrush != null) {
                    targetBrush = variationAliasBrush.target;
                }

                // Cannot nest oriented brushes!
                if (targetBrush.PerformsAutomaticOrientation) {
                    return string.Format(
                        /* 0: name of the thing that cannot be nested */
                        TileLang.ParticularText("Error", "{0} cannot be nested within an oriented brush."),
                        ObjectNames.NicifyVariableName(targetBrush.GetType().Name)
                    );
                }
            }

            return null;
        }

        /// <summary>
        /// Validates new variation object.
        /// </summary>
        /// <param name="variation">The new variation object.</param>
        /// <returns>
        /// A value of <c>true</c> if variation was valid; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="ValidateNewVariationWithUserNotification"/>
        private bool ValidateNewVariation(Object variation)
        {
            return this.ValidateNewVariationWithMessage(variation) == null;
        }

        /// <summary>
        /// Validates whether variation can be added to orientation. Error message is
        /// presented to user upon failure.
        /// </summary>
        /// <param name="variation">The variation object.</param>
        /// <returns>
        /// A value of <c>true</c> if variation can be added; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="ValidateNewVariation"/>
        private bool ValidateNewVariationWithUserNotification(Object variation)
        {
            string error = this.ValidateNewVariationWithMessage(variation);
            if (error != null) {
                this.Window.ShowNotification(new GUIContent(error));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether variation can be inserted into orientation.
        /// </summary>
        /// <param name="target">Target orientation.</param>
        /// <param name="variation">The variation object.</param>
        /// <returns>
        /// A value of <c>true</c> if variation can be inserted; otherwise <c>false</c>.
        /// </returns>
        private bool CanInsertVariation(BrushOrientation target, Object variation)
        {
            return variation != null && (target == s_AnchorOrientation || target.IndexOfVariation(variation) == -1);
        }

        #endregion


        #region Orientations

        /// <summary>
        /// Width (in pixels) of area to left of orientation track. This area includes
        /// the 3x3 orientation icon.
        /// </summary>
        private const int OrientationLeftWidth = 127;
        /// <summary>
        /// Width (in pixels) of "variation picker" button strip.
        /// </summary>
        private const int OrientationRightWidth = 35;
        /// <summary>
        /// Height (in pixels) of orientation track.
        /// </summary>
        private const int OrientationTrackHeight = 97 + 11;

        /// <summary>
        /// Thickness (in pixels) of "dead zone" between orientations.
        /// </summary>
        /// <remarks>
        /// <para>This helps to avoid flickering when variation is being dragged between
        /// orientations since "dead zone" allows mouse movement without registering new
        /// target orientation. This essentially introduces a small delay.</para>
        /// </remarks>
        private const int DeadZone = 2;


        private bool isDoubleClickEvent;
        private bool ignoreNextDoubleClick;


        /// <summary>
        /// Render orientations for editing.
        /// </summary>
        /// <example>
        /// Display orientations section in custom oriented brush designer.
        /// <code language="csharp"><![CDATA[
        /// public class CustomOrientedBrushDesigner : OrientedBrushDesigner
        /// {
        ///     public override void OnGUI()
        ///     {
        ///         this.Section_Orientations();
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        protected void Section_Orientations()
        {
            this.isDoubleClickEvent = false;
            if (Event.current.type == EventType.MouseDown) {
                this.isDoubleClickEvent = Event.current.clickCount == 2 && !this.ignoreNextDoubleClick;
                this.ignoreNextDoubleClick = false;
            }

            Rect position = this.GetOrientationSectionRect(this.OrientedBrush.Orientations);
            position.x += viewScrollPosition.x;

            // Ensure that list of pending variations is clear.
            this.pendingVariations.Clear();
            // Update list of variations that are being dragged.
            this.CheckDragAndDropInsertion(position);

            this.DrawOrientationListBackground(position);
            this.DrawOrientationListForeground(position);
        }

        /// <summary>
        /// Get position for entire orientations section.
        /// </summary>
        /// <param name="orientations">Collection of brush orientations.</param>
        /// <returns>
        /// The position in space of window.
        /// </returns>
        private Rect GetOrientationSectionRect(IEnumerable<BrushOrientation> orientations)
        {
            float maxHeight = 1;
            float maxWidth = viewPosition.width;

            float width;

            foreach (var orientation in orientations) {
                if (!IsVisibleOrientation(orientation)) {
                    continue;
                }

                maxHeight += OrientationTrackHeight;

                width = OrientationLeftWidth + OrientationRightWidth + orientation.VariationCount * VariationHorizontalDelta + VariationHorizontalDelta / 2;
                if (width > maxWidth) {
                    maxWidth = width;
                }
            }

            return GUILayoutUtility.GetRect(maxWidth, maxHeight);
        }

        /// <summary>
        /// Determines whether an orientation should be listed in user interface.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <returns>
        /// A value of <c>true</c> if orientation is to be shown; otherwise <c>false</c>.
        /// </returns>
        private static bool IsVisibleOrientation(BrushOrientation orientation)
        {
            return orientation != null && orientation.Rotation == 0;
        }

        /// <summary>
        /// Finds the nth visible orientation.
        /// </summary>
        /// <param name="nthIndex">Zero-based index of nth visible orientation.</param>
        /// <returns>
        /// The orientation or a value of <c>null</c> if not found.
        /// </returns>
        private BrushOrientation FindNthVisibleOrientation(int nthIndex)
        {
            var orientations = this.OrientedBrush.Orientations;
            for (int i = 0; i < orientations.Count; ++i) {
                var orientation = orientations[i];
                if (!IsVisibleOrientation(orientation)) {
                    continue;
                }

                if (nthIndex-- == 0) {
                    return orientation;
                }
            }
            return null;
        }


        /// <summary>
        /// Cache of orientation track control IDs which is updated for each GUI event.
        /// </summary>
        private static List<int> s_OrientationTrackControlIDs = new List<int>();


        /// <summary>
        /// Draw background of list of orientation tracks.
        /// </summary>
        /// <param name="position">Position of list.</param>
        private void DrawOrientationListBackground(Rect position)
        {
            var orientations = this.OrientedBrush.Orientations;
            var lastOrientation = orientations.Last();

            float visibleVariationListWidth = viewPosition.width - OrientationLeftWidth;

            // Draw background of variation lists.
            if (Event.current.type == EventType.Repaint) {
                Rect backgroundPosition = position;
                backgroundPosition.x += OrientationLeftWidth;
                backgroundPosition.width = visibleVariationListWidth + 1;
                RotorzEditorStyles.Instance.ListBox.Draw(backgroundPosition, GUIContent.none, false, false, false, false);
            }

            Rect orientationPosition = new Rect(position.x, position.y + 1, position.width, OrientationTrackHeight);

            s_OrientationTrackControlIDs.Clear();

            for (int i = 0; i < orientations.Count; ++i) {
                var orientation = orientations[i];
                if (!IsVisibleOrientation(orientation)) {
                    continue;
                }

                int orientationControlID = GUIUtility.GetControlID(FocusType.Passive);
                s_OrientationTrackControlIDs.Add(orientationControlID);

                // Draw background of orientation.
                this.DrawOrientationTrackBackground(orientationControlID, new Rect(orientationPosition.x, orientationPosition.y, orientationPosition.width, OrientationTrackHeight - 1), orientation);

                // Draw splitter between each orientation.
                if (!ReferenceEquals(orientation, lastOrientation)) {
                    ExtraEditorGUI.SeparatorLight(new Rect(orientationPosition.x, orientationPosition.y + OrientationTrackHeight - 1, OrientationLeftWidth, 1));
                    ExtraEditorGUI.Separator(new Rect(orientationPosition.x + OrientationLeftWidth + 1, orientationPosition.y + OrientationTrackHeight - 1, visibleVariationListWidth - 2, 1));
                }

                // Offset to next orientation.
                orientationPosition.y += OrientationTrackHeight;
            }
        }

        /// <summary>
        /// Draw foreground of list of orientation tracks.
        /// </summary>
        /// <param name="position">Position of list.</param>
        private void DrawOrientationListForeground(Rect position)
        {
            int listControlID = GUIUtility.GetControlID(FocusType.Passive);
            var orientations = this.OrientedBrush.Orientations;

            Color restoreColor = GUI.color;

            // Draw each track of variations.
            position.x += OrientationLeftWidth + 2;
            position.y += 2;
            position.width = viewPosition.width - OrientationLeftWidth - OrientationRightWidth - 1;
            position.height -= 4;

            GUI.BeginGroup(position);

            if (s_AnchorOrientation != null) {
                switch (Event.current.GetTypeForControl(listControlID)) {
                    case EventType.MouseDown:
                        if (IsTrackingReorderVariation(listControlID)) {
                            // Cancel drag when other mouse button is pressed.
                            this.StopTrackingReorderVariation();
                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseUp:
                        if (IsTrackingReorderVariation(listControlID)) {
                            // Use combination ctrl+drag to copy variation to target orientation.
                            if (Event.current.control) {
                                this.AcceptDragCopyVariation();
                            }
                            else {
                                this.AcceptDragReorderVariation();
                            }

                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (IsTrackingReorderVariation(listControlID) && s_AnchorOrientation != null) {
                            Vector2 mousePosition = Event.current.mousePosition;
                            s_FloatingVariationPosition.x = mousePosition.x + s_AnchorMouseOffset.x;
                            s_FloatingVariationPosition.y = mousePosition.y + s_AnchorMouseOffset.y;

                            // Hot spot is center point of variation which is being dragged in space of
                            // orientation list group.
                            int hotSpotX = Mathf.RoundToInt(s_FloatingVariationPosition.center.x);
                            int hotSpotY = Mathf.RoundToInt((int)s_FloatingVariationPosition.center.y);

                            // Determine which orientation is being targetted when reordering variations.
                            int targetIndex = (hotSpotY - 1) / OrientationTrackHeight;
                            if (targetIndex >= s_OrientationTrackControlIDs.Count) {
                                targetIndex = s_OrientationTrackControlIDs.Count - 1;
                            }

                            // Force active orientation when mouse pointer is below.
                            bool forceLastOrientation = (targetIndex + 1 == s_OrientationTrackControlIDs.Count);

                            // "Dead Zone" between orientations to reduce flicker.
                            int orientationTop = 1 + targetIndex * OrientationTrackHeight;
                            if (forceLastOrientation || hotSpotY >= orientationTop + DeadZone && hotSpotY < orientationTop + OrientationTrackHeight - DeadZone) {
                                var targetOrientation = this.FindNthVisibleOrientation(targetIndex);
                                if (targetOrientation == null) {
                                    targetOrientation = orientations[orientations.Count - 1];
                                }

                                if (s_TargetOrientation != targetOrientation) {
                                    s_TargetOrientation = targetOrientation;

                                    // Figure out insertion index of variation.
                                    if (targetOrientation != null) {
                                        s_TargetIndex = (hotSpotX - 5) / VariationHorizontalDelta;
                                        int variationRight = 5 + (s_TargetIndex * VariationHorizontalDelta) + VariationHorizontalDelta;
                                        if (hotSpotX > variationRight || (targetOrientation == s_AnchorOrientation && s_TargetIndex > s_AnchorIndex)) {
                                            ++s_TargetIndex;
                                        }

                                        s_TargetIndex = Mathf.Clamp(s_TargetIndex, 0, targetOrientation.VariationCount);
                                    }
                                }
                            }
                        }
                        break;

                    case EventType.KeyDown:
                        if (IsTrackingReorderVariation(listControlID) && Event.current.keyCode == KeyCode.Escape) {
                            this.StopTrackingReorderVariation();
                            Event.current.Use();
                        }
                        break;
                }
            }

            Rect orientationPosition = new Rect(5 - viewScrollPosition.x, -1, position.width, OrientationTrackHeight);
            int nthIndex = 0;

            for (int i = 0; i < orientations.Count; ++i) {
                var orientation = orientations[i];
                if (!IsVisibleOrientation(orientation)) {
                    continue;
                }

                int orientationControlID = s_OrientationTrackControlIDs[nthIndex++];

                Rect variationListPosition = orientationPosition;
                variationListPosition.y += 6;

                // Draw each variation of orientation.
                Rect variationPosition = new Rect(variationListPosition.x, variationListPosition.y + 6, VariationSize, VariationSize);
                variationListPosition.width = orientation.VariationCount * VariationHorizontalDelta + 1;
                variationPosition.x = this.DrawReorderableVariationList(variationListPosition, listControlID, orientationControlID, orientation);

                // Do variations exceed visible view?
                if (variationPosition.x + 10 >= position.width && Event.current.type == EventType.Repaint) {
                    // Inset shading slightly so that it does not overlap border drop target.
                    int verticalOffset = (orientationControlID == DragAndDrop.activeControlID ? 1 : 0);

                    GUI.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp(position.width - variationPosition.x, 0, 10) / 10f);
                    GUI.DrawTexture(new Rect(position.width - 20, orientationPosition.y + verticalOffset, 20, orientationPosition.height - 1 - verticalOffset * 3), RotorzEditorStyles.Skin.SoftClipping);
                    GUI.color = restoreColor;
                }

                // Offset to next orientation.
                orientationPosition.y += OrientationTrackHeight;
            }

            if (s_AnchorOrientation != null) {
                this.DrawFloatingVariation();
            }

            if (IsTrackingReorderVariation(listControlID)) {
                // Force repaint to occur so that dragging rectangle is visible.
                if (Event.current.GetTypeForControl(listControlID) == EventType.MouseDrag) {
                    Event.current.Use();
                }
            }

            GUI.EndGroup();
        }

        /// <summary>
        /// Draw background of orientation track.
        /// </summary>
        /// <remarks>
        /// <para>Width of supplied position will be wider than view when horizontal
        /// scrolling is available. See <c>viewPosition.width</c> for actual displayable
        /// width.</para>
        /// </remarks>
        /// <param name="orientationControlID">Unique ID of orientation.</param>
        /// <param name="position">Position of orientation row within window.</param>
        /// <param name="orientation">The orientation.</param>
        /// <seealso cref="DrawReorderableVariationList"/>
        /// <seealso cref="DrawVariationPickerButtons"/>
        private void DrawOrientationTrackBackground(int orientationControlID, Rect position, BrushOrientation orientation)
        {
            // Draw highlighted background if this is the drop target!
            if (orientationControlID == DragAndDrop.activeControlID && Event.current.type == EventType.Repaint) {
                float visibleVariationListWidth = viewPosition.width - OrientationLeftWidth;

                Rect dropTargetPosition = position;
                dropTargetPosition.x += OrientationLeftWidth - 1;
                dropTargetPosition.y -= 1;
                dropTargetPosition.width = visibleVariationListWidth;
                dropTargetPosition.height += 1;

                RotorzEditorStyles.Instance.DropTargetBackground.Draw(dropTargetPosition, GUIContent.none, false, false, false, false);
            }

            // Draw "Use as default" toggle.
            using (var content = ControlContent.Basic(
                "", TileLang.ParticularText("Action", "Set as default for fallback")
            )) {
                bool isDefaultOrientation = this.OrientedBrush.DefaultOrientationMask == orientation.Mask;
                Rect togglePosition = new Rect(position.x + 6, position.y + 39, 12, 12);
                if (GUI.Toggle(togglePosition, isDefaultOrientation, content, EditorStyles.radioButton)) {
                    // Use as default orientation if not already the default orientation!
                    if (!isDefaultOrientation) {
                        Undo.RecordObject(this.OrientedBrush, TileLang.ParticularText("Action", "Set as default for fallback"));
                        this.OrientedBrush.DefaultOrientationMask = orientation.Mask;

                        // Only need to refresh preview if "alone" orientation is missing!
                        if (this.OrientedBrush.FindOrientation(0) == null) {
                            BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
                        }
                    }
                }
            }

            this.DrawOrientationIcon(position.x + 28, position.y + 10, orientation);

            // Present button to remove orientation, but not for default orientation!
            using (var content = ControlContent.Basic(
                "", TileLang.ParticularText("Action", "Remove Orientation")
            )) {
                if (orientation.Mask != this.OrientedBrush.DefaultOrientationMask) {
                    Rect buttonPosition = new Rect(position.x + OrientationLeftWidth - 17, position.y + 4, 13, 13);
                    if (RotorzEditorGUI.HoverButton(buttonPosition, content, RotorzEditorStyles.Instance.SmallCloseButton)) {
                        this.RemoveOrientation(orientation);
                        GUIUtility.ExitGUI();
                    }
                }
            }

            this.CheckDropInsertVariations(position, orientationControlID, orientation);

            Rect sidebarPosition = new Rect(viewScrollPosition.x + viewPosition.width - 33, position.y, 30, position.height);
            this.DrawVariationPickerButtons(orientationControlID, sidebarPosition, orientation);

            this.AcceptPendingVariations(orientation);
        }


        /// <summary>
        /// ID of picker control for whom selection of a particular object is to be ignored.
        /// </summary>
        /// <seealso cref="s_IgnorePickerObject"/>
        private static int s_IgnorePickerControlID;
        /// <summary>
        /// A recently picked object which should be temporarily ignored to avoid
        /// multiple insertions when object is double-clicked in user interface.
        /// Object is only ignored for the associated picker control.
        /// </summary>
        /// <seealso cref="s_IgnorePickerControlID"/>
        private static Object s_IgnorePickerObject;


        /// <summary>
        /// Draw buttons to right of orientation allowing user to pick tile prefabs and
        /// nestable brushes as new variations.
        /// </summary>
        /// <param name="orientationControlID">Unique control ID of orientation.</param>
        /// <param name="position">Position of button strip.</param>
        /// <param name="orientation">Orientation.</param>
        private void DrawVariationPickerButtons(int orientationControlID, Rect position, BrushOrientation orientation)
        {
            int pickerControlID = GUIUtility.GetControlID(FocusType.Passive);
            Object pickedObject = null;

            if (Event.current.type == EventType.ExecuteCommand) {
                switch (Event.current.commandName) {
                    case "ObjectSelectorUpdated":
                        if (pickerControlID == EditorGUIUtility.GetObjectPickerControlID()) {
                            pickedObject = EditorGUIUtility.GetObjectPickerObject();
                            Event.current.Use();
                        }
                        break;

                    case "Rotorz.TileSystem.BrushPickerUpdated":
                        if (pickerControlID == RotorzEditorGUI.BrushPickerControlID) {
                            pickedObject = RotorzEditorGUI.BrushPickerSelectedBrush;
                            Event.current.Use();
                        }
                        break;
                }
            }

            if (pickedObject != null) {
                if (pickerControlID != s_IgnorePickerControlID || pickedObject != s_IgnorePickerObject) {
                    s_IgnorePickerControlID = pickerControlID;
                    s_IgnorePickerObject = pickedObject;

                    // Append variation to end of orientation.
                    s_TargetIndex = orientation.VariationCount;
                    this.AddPendingVariation(orientation, pickedObject);
                }
            }

            var buttonStyle = (orientationControlID == DragAndDrop.activeControlID)
                ? RotorzEditorStyles.Instance.SmallFlatButtonFake
                : RotorzEditorStyles.Instance.SmallFlatButton;

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.PickBrush,
                TileLang.ParticularText("Action", "Pick Nestable Brush")
            )) {
                if (RotorzEditorGUI.HoverButton(new Rect(position.x, position.yMax - 3 - 25 - 25, 30, 25), content, buttonStyle)) {
                    s_IgnorePickerControlID = 0;
                    RotorzEditorGUI.ShowBrushPicker(null, false, true, pickerControlID);
                    GUIUtility.ExitGUI();
                }
            }

            using (var content = ControlContent.Basic(
                RotorzEditorStyles.Skin.PickPrefab,
                TileLang.ParticularText("Action", "Pick Prefab")
            )) {
                if (RotorzEditorGUI.HoverButton(new Rect(position.x, position.yMax - 3 - 25, 30, 25), content, buttonStyle)) {
                    s_IgnorePickerControlID = 0;
                    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, null, pickerControlID);
                    GUIUtility.ExitGUI();
                }
            }
        }

        /// <summary>
        /// Draw icon that represents orientation.
        /// </summary>
        /// <param name="left">Left position within window.</param>
        /// <param name="top">Top position within window.</param>
        /// <param name="orientation">The orientation.</param>
        private void DrawOrientationIcon(float left, float top, BrushOrientation orientation)
        {
            string name = OrientationUtility.NameFromMask(orientation.Mask);

            Color restoreBackground = GUI.backgroundColor;
            Rect position = new Rect(left, top, 24, 24);

            Color activeColor = orientation.HasRotationalSymmetry ? new Color(0, 40, 255) : Color.green;

            for (int i = 0, j = 0; i < 9; ++i) {
                if (i % 3 == 0 && i != 0) {
                    position.x = left;
                    position.y += 26;
                }

                // Draw center box?
                if (i == 4) {
                    GUI.backgroundColor = Color.black;
                }
                else {
                    GUI.backgroundColor = name[j] == '1' ? activeColor : Color.white;
                    ++j;
                }

                GUI.Box(position, GUIContent.none, RotorzEditorStyles.Instance.OrientationBox);

                position.x += 26;
            }

            GUI.backgroundColor = restoreBackground;
        }

        #endregion


        #region Reorderable Variations

        /// <summary>
        /// Space (in pixels) between variations horizontally.
        /// </summary>
        private const int VariationHorizontalDelta = 92;
        /// <summary>
        /// Width and height (in pixels) of variation preview.
        /// </summary>
        private const int VariationSize = 84;

        /// <summary>
        /// Position of mouse upon anchoring item for drag.
        /// </summary>
        private static Vector2 s_AnchorMouseOffset;
        /// <summary>
        /// The anchored orientation.
        /// </summary>
        private static BrushOrientation s_AnchorOrientation;
        /// <summary>
        /// The targeted orientation.
        /// </summary>
        private static BrushOrientation s_TargetOrientation;
        /// <summary>
        /// Zero-based index of anchored variation.
        /// </summary>
        private static int s_AnchorIndex = -1;
        /// <summary>
        /// Zero-based index of insertion variation for reordering.
        /// </summary>
        private static int s_TargetIndex = -1;

        /// <summary>
        /// Begin tracking drag and drop within list.
        /// </summary>
        /// <param name="listControlID">Unique control ID of orientations list control.</param>
        /// <param name="orientation">The associated orientation.</param>
        /// <param name="itemIndex">Zero-based index of item which is going to be dragged.</param>
        private void BeginTrackingReorderVariation(int listControlID, BrushOrientation orientation, int itemIndex)
        {
            GUIUtility.hotControl = listControlID;
            GUIUtility.keyboardControl = 0;
            s_AnchorOrientation = orientation;
            s_TargetOrientation = orientation;
            s_AnchorIndex = itemIndex;
            s_TargetIndex = itemIndex;
        }

        /// <summary>
        /// Stop tracking drag and drop.
        /// </summary>
        private void StopTrackingReorderVariation()
        {
            GUIUtility.hotControl = 0;
            s_AnchorOrientation = null;
            s_TargetOrientation = null;
            s_AnchorIndex = -1;
            s_TargetIndex = -1;
        }

        /// <summary>
        /// Gets a value indicating whether variation in specified list is currently being tracked.
        /// </summary>
        /// <param name="listControlID">Unique control ID of orientations list control.</param>
        /// <returns>
        /// A value of <c>true</c> if item is being tracked; otherwise <c>false</c>.
        /// </returns>
        private static bool IsTrackingReorderVariation(int listControlID)
        {
            return GUIUtility.hotControl == listControlID;
        }

        /// <summary>
        /// Accept reordering of variations.
        /// </summary>
        private void AcceptDragReorderVariation()
        {
            try {
                // No orientation was anchored to reorder variations, just bail.
                if (s_AnchorOrientation == null) {
                    return;
                }

                int sourceIndex = s_AnchorIndex;
                var variation = s_AnchorOrientation.GetVariation(sourceIndex);

                // It is possible that orientation was dragged outside bounds of orientations list.
                if (s_TargetOrientation != null) {
                    if (!this.ValidateNewVariationWithUserNotification(variation)) {
                        return;
                    }
                    s_TargetIndex = Mathf.Clamp(s_TargetIndex, 0, s_TargetOrientation.VariationCount + 1);
                }
                else {
                    s_TargetIndex = -1;
                }

                if (!this.CanInsertVariation(s_TargetOrientation, variation)) {
                    return;
                }

                if (s_TargetOrientation != s_AnchorOrientation || (s_TargetIndex != s_AnchorIndex && s_TargetIndex != s_AnchorIndex + 1)) {
                    int destIndex = s_TargetIndex;
                    if (s_TargetOrientation == s_AnchorOrientation && destIndex > sourceIndex) {
                        --destIndex;
                    }

                    Undo.RecordObject(this.OrientedBrush, s_TargetOrientation != null
                        ? TileLang.ParticularText("Action", "Reorder Variation")
                        : TileLang.ParticularText("Action", "Remove Variation"));

                    // Actually adjust ordering of variations.
                    int sourceWeight = s_AnchorOrientation.GetVariationWeight(sourceIndex);
                    s_AnchorOrientation.RemoveVariation(sourceIndex);
                    // Has item been dragged out for removal?
                    if (s_TargetOrientation != null) {
                        s_TargetOrientation.InsertVariation(destIndex, variation, sourceWeight);
                    }

                    // Synchronize changes with other "connected" orientations.
                    this.OrientedBrush.SyncGroupedVariations(s_AnchorOrientation.Mask);
                    if (s_TargetOrientation != null && s_TargetOrientation != s_AnchorOrientation) {
                        this.OrientedBrush.SyncGroupedVariations(s_TargetOrientation.Mask);
                    }

                    EditorUtility.SetDirty(this.OrientedBrush);

                    // Regenerate brush preview?
                    if ((s_AnchorOrientation.Mask == 0 && sourceIndex == 0) || (destIndex == 0 && (s_TargetOrientation != null && s_TargetOrientation.Mask == 0 || this.OrientedBrush.FindOrientation(0) == null))) {
                        BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
                    }
                }
            }
            finally {
                this.StopTrackingReorderVariation();
            }
        }

        /// <summary>
        /// Accept ctrl+drag and copy of variation.
        /// </summary>
        private void AcceptDragCopyVariation()
        {
            try {
                // No orientation was anchored to reorder variations, just bail.
                // Also, cannot copy to the same orientation!
                if (s_AnchorOrientation == null || s_TargetOrientation == null || s_TargetOrientation == s_AnchorOrientation) {
                    return;
                }

                int sourceIndex = s_AnchorIndex;
                var variation = s_AnchorOrientation.GetVariation(sourceIndex);
                int variationWeight = s_AnchorOrientation.GetVariationWeight(sourceIndex);

                if (!this.ValidateNewVariationWithUserNotification(variation)) {
                    return;
                }
                if (!this.CanInsertVariation(s_TargetOrientation, variation)) {
                    return;
                }

                int destIndex = s_TargetIndex;
                if (s_TargetOrientation == s_AnchorOrientation && destIndex > sourceIndex) {
                    --destIndex;
                }

                Undo.RecordObject(this.OrientedBrush, TileLang.ParticularText("Action", "Copy Variation"));

                // Actually adjust ordering of variations.
                s_TargetOrientation.InsertVariation(destIndex, variation, variationWeight);

                // Synchronize changes with other "connected" orientations.
                this.OrientedBrush.SyncGroupedVariations(s_TargetOrientation.Mask);

                EditorUtility.SetDirty(this.OrientedBrush);

                // Regenerate brush preview?
                if (destIndex == 0 && (s_TargetOrientation.Mask == 0 || this.OrientedBrush.FindOrientation(0) == null)) {
                    BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
                }
            }
            finally {
                this.StopTrackingReorderVariation();
            }
        }


        /// <summary>
        /// Position of floating variation which is being dragged.
        /// </summary>
        private static Rect s_FloatingVariationPosition;


        /// <summary>
        /// Draw reorderable list of variations for an orientation.
        /// </summary>
        /// <param name="position">Position of variation list within orientation track.</param>
        /// <param name="listControlID">Unique control ID of orientations list control.</param>
        /// <param name="orientationControlID">Unique control ID of orientation.</param>
        /// <param name="orientation">The orientation.</param>
        /// <returns>
        /// X position of trailing variation.
        /// </returns>
        private float DrawReorderableVariationList(Rect position, int listControlID, int orientationControlID, BrushOrientation orientation)
        {
            // Get local copy of event information for efficiency.
            EventType eventType = Event.current.GetTypeForControl(listControlID);
            Vector2 mousePosition = Event.current.mousePosition;

            bool isCopyMode = Event.current.control;
            bool isCopyTarget = (isCopyMode && s_AnchorOrientation == s_TargetOrientation);
            bool isReorderingTarget = !isCopyTarget && (orientation == s_TargetOrientation && this.CanInsertVariation(orientation, s_AnchorOrientation.GetVariation(s_AnchorIndex)));
            bool isDragAndDropTarget = (DragAndDrop.activeControlID == orientationControlID);

            int newTargetIndex = s_TargetIndex;

            if (isReorderingTarget && eventType == EventType.MouseDrag) {
                // Reset target index and adjust when looping through list items.
                if (mousePosition.x < position.x) {
                    newTargetIndex = 0;
                }
                //else if (mousePosition.x >= position.xMax) {
                //    newTargetIndex = orientation.variations.Length;
                //}
            }

            // Draw list items!
            Rect variationPosition = new Rect(position.x, position.y, VariationSize, VariationSize);
            // Assume that target slot is at end of list.
            Rect targetSlotPosition = variationPosition;
            targetSlotPosition.x = position.xMax - 1;

            float lastMidPoint = 0f;
            float lastWidth = 0f;

            if (isReorderingTarget && orientation == s_AnchorOrientation) {
                targetSlotPosition.x -= VariationHorizontalDelta;
            }

            if (isDragAndDropTarget && Event.current.type == EventType.Repaint) {
                s_FloatingVariationPosition.x = mousePosition.x - VariationHorizontalDelta / 2;
                s_FloatingVariationPosition.y = position.y;
                s_FloatingVariationPosition.width = VariationHorizontalDelta;
                s_FloatingVariationPosition.height = VariationSize;

                // Make append insertion slightly easier for long lists.
                // i.e. consistent with forme 'append' behaviour.
                if (mousePosition.x > -(VariationHorizontalDelta / 2)) {
                    s_TargetIndex = Mathf.Clamp(((int)mousePosition.x - 5) / VariationHorizontalDelta, 0, orientation.VariationCount);
                }
                else {
                    s_TargetIndex = orientation.VariationCount;
                }
            }

            int count = orientation.VariationCount;
            for (int i = 0; i < count; ++i) {
                // Consider both drag reordering plus drag and drop insertion.
                if (s_AnchorOrientation != null || isDragAndDropTarget) {
                    // Does this represent the target index?
                    if (i == s_TargetIndex && (isReorderingTarget || isDragAndDropTarget)) {
                        targetSlotPosition.x = variationPosition.x;
                        variationPosition.x += s_FloatingVariationPosition.width;
                    }

                    if (orientation == s_AnchorOrientation) {
                        // Do not draw item if it is currently being dragged (unless ctrl key is held).
                        // Draw later so that it is shown in front of other controls.
                        if (i == s_AnchorIndex && !isCopyMode) {
                            continue;
                        }
                    }

                    lastMidPoint = variationPosition.x - lastWidth / 2f;
                }

                // Update position for current item.
                lastWidth = variationPosition.width;

                if (isReorderingTarget && eventType == EventType.MouseDrag || isDragAndDropTarget) {
                    float midpoint = variationPosition.x + VariationHorizontalDelta / 2f;
                    if (s_TargetIndex < i) {
                        if (s_FloatingVariationPosition.xMax > lastMidPoint && s_FloatingVariationPosition.xMax < midpoint) {
                            newTargetIndex = i;
                        }
                    }
                    else if (s_TargetIndex > i) {
                        if (s_FloatingVariationPosition.x > lastMidPoint && s_FloatingVariationPosition.x < midpoint) {
                            newTargetIndex = i;
                        }
                    }
                }

                // Draw list item.
                this.DrawVariation(variationPosition, orientation.GetVariation(i));
                this.DrawRemoveVariationButton(variationPosition, i, orientation);

                // Did list count change (i.e. item removed)?
                if (orientation.VariationCount < count) {
                    // We assume that it was this item which was removed, so --i allows us
                    // to process the next item as usual.
                    count = orientation.VariationCount;
                    --i;
                    continue;
                }

                // Draw slider control below variation for randomization weighting.
                this.DrawVariationWeightSlider(variationPosition, orientation, i);

                // Event has already been used, skip to next item.
                if (GUI.enabled && Event.current.type == EventType.MouseDown && variationPosition.Contains(mousePosition)) {
                    // Remove input focus from control before attempting a context click or drag.
                    GUIUtility.keyboardControl = 0;

                    if (Event.current.button == 0 && Event.current.clickCount == 1) {
                        s_FloatingVariationPosition = variationPosition;
                        s_FloatingVariationPosition.width = VariationHorizontalDelta;

                        this.BeginTrackingReorderVariation(listControlID, orientation, i);
                        s_AnchorMouseOffset.x = variationPosition.x - mousePosition.x;
                        s_AnchorMouseOffset.y = variationPosition.y - mousePosition.y;
                        s_TargetIndex = i;

                        Event.current.Use();
                    }
                }

                variationPosition.x += VariationHorizontalDelta;
            }

            if (isReorderingTarget) {
                if (s_TargetIndex < orientation.VariationCount && s_FloatingVariationPosition.center.x > variationPosition.x - VariationHorizontalDelta) {
                    newTargetIndex = orientation.VariationCount;
                }
            }

            if (eventType == EventType.Repaint) {
                if (isDragAndDropTarget || isReorderingTarget) {
                    // Draw outline for insertion position.
                    RotorzEditorStyles.Instance.OutlinedCornerBox.Draw(targetSlotPosition, GUIContent.none, false, false, false, false);
                }
                else {
                    bool isPreviewEmpty = (!isCopyMode && orientation.VariationCount == 1 && orientation == s_AnchorOrientation && s_TargetOrientation != s_AnchorOrientation);
                    if (orientation.VariationCount == 0 || isPreviewEmpty) {
                        // Show placeholder label when empty and not inserting item.
                        Rect labelRect = new Rect(3, position.y - 7, viewPosition.width, OrientationTrackHeight - 4);
                        RotorzEditorStyles.Instance.LabelMiddleLeft.Draw(labelRect, TileLang.Text("Drop prefab or nestable brush here"), false, false, false, false);
                    }
                }
            }

            // Item which is being dragged should be shown on top of other controls!
            if (isReorderingTarget) {
                lastMidPoint = variationPosition.x + lastWidth / 2f;

                if (eventType == EventType.MouseDrag) {
                    if (s_FloatingVariationPosition.xMax >= lastMidPoint) {
                        newTargetIndex = count;
                    }
                    s_TargetIndex = newTargetIndex;
                }
            }

            return variationPosition.x;
        }

        /// <summary>
        /// Draw floating variation which is currently being reordered by user.
        /// </summary>
        private void DrawFloatingVariation()
        {
            Rect position = s_FloatingVariationPosition;
            position.x += 2;
            position.y += 2;
            position.width = VariationSize - 4;
            position.height = VariationSize - 4;

            // Floating item should appear semi-transparent.
            Color restore = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);

            // Present actual control.
            this.DrawVariation(position, s_AnchorOrientation.GetVariation(s_AnchorIndex));

            GUI.color = restore;

            // Display overlay when ctrl key is pressed whilst dragging variation.
            if (Event.current.control && Event.current.type == EventType.Repaint) {
                Rect overlayPosition = new Rect(
                    position.x + position.width - 18 - 6,
                    position.y - 18 + 6,
                    36,
                    36
                );
                GUI.DrawTexture(overlayPosition, RotorzEditorStyles.Skin.AddVariationOverlay);
            }
        }

        #endregion


        #region Variations

        /// <summary>
        /// Draw variation within orientation.
        /// </summary>
        /// <param name="position">Position of variation.</param>
        /// <param name="variation">Variation prefab or nested brush.</param>
        private void DrawVariation(Rect position, Object variation)
        {
            // Prepare tooltip!
            string hoverTip = (Event.current.rawType == EventType.MouseMove)
                ? AssetDatabase.GetAssetPath(variation)
                : "";
            /*int controlID = */
            RotorzEditorGUI.GetHoverControlID(position, hoverTip);

            if (Event.current.type == EventType.Repaint) {
                RotorzEditorStyles.Instance.VariationBox.Draw(position, GUIContent.none, false, false, false, false);

                Rect thumbnailPosition = new Rect(position.x + 1, position.y + 1, position.width - 2, position.height - 2);
                if (ExtraEditorGUI.VisibleRect.Overlaps(thumbnailPosition)) {
                    this.DrawVariationThumbnail(thumbnailPosition, variation);
                }
            }
            else if (this.isDoubleClickEvent && position.Contains(Event.current.mousePosition)) {
                this.isDoubleClickEvent = false;
                Event.current.Use();

                Brush brushVariation = variation as Brush;
                if (brushVariation != null) {
                    // Only allow non-master brushes to be opened into brush designer.
                    BrushAssetRecord record = BrushDatabase.Instance.FindRecord(brushVariation);
                    if (!record.IsMaster) {
                        this.Window.SelectedObject = brushVariation;

                        // Only reveal brush in brushes palette when designer window is not locked.
                        //!TODO: Can this be moved into `Window.SelectedObject`?
                        if (!this.Window.IsLocked) {
                            ToolUtility.RevealBrush(brushVariation, false);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                else {
                    EditorGUIUtility.PingObject(variation);
                }
            }
        }

        /// <summary>
        /// Draw button to remove variation from orientation.
        /// </summary>
        /// <param name="position">Position of variation.</param>
        /// <param name="variationIndex">Zero-based index of variation.</param>
        /// <param name="orientation">The orientation.</param>
        private void DrawRemoveVariationButton(Rect position, int variationIndex, BrushOrientation orientation)
        {
            if (GUI.Button(new Rect(position.xMax - 14, position.y - 4, 18, 18), GUIContent.none, RotorzEditorStyles.Instance.SmallRemoveButton)) {
                Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Remove Variation"));

                // Remove variation!
                orientation.RemoveVariation(variationIndex);
                this.OrientedBrush.SyncGroupedVariations(orientation.Mask);

                // Refresh preview if this was the default variation.
                if (variationIndex == 0 && orientation.Mask == this.OrientedBrush.DefaultOrientationMask) {
                    BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
                }

                this.ignoreNextDoubleClick = true;

                // Refresh display.
                this.Repaint();
            }
        }

        /// <summary>
        /// Draw thumbnail for variation in orientation.
        /// </summary>
        /// <param name="position">Position of thumbnail.</param>
        /// <param name="variation">Variation prefab or nested brush.</param>
        private void DrawVariationThumbnail(Rect position, Object variation)
        {
            var brushVariation = variation as Brush;
            if (brushVariation != null) {
                // Draw thumbnail for nested brush.
                BrushAssetRecord record = BrushDatabase.Instance.FindRecord(brushVariation);
                if (record != null) {
                    if (!RotorzEditorGUI.DrawBrushPreviewHelper(position, record, false)) {
                        // Fallback, just draw label.
                        using (var tempContent = ControlContent.Basic(record.DisplayName)) {
                            RotorzEditorStyles.Instance.PreviewLabel.Draw(position, tempContent, 0);
                        }
                    }

                    GUI.DrawTexture(new Rect(position.x - 1, position.y + position.height - 11, 12, 12), RotorzEditorStyles.Skin.Overlay_Brush);
                }
                else {
                    using (var tempContent = ControlContent.Basic("?")) {
                        RotorzEditorStyles.Instance.PreviewLabel.Draw(position, tempContent, 0);
                    }
                }
            }
            else {
                // Draw thumbnail for nested prefab.
                var previewTexture = AssetPreviewCache.GetAssetPreview(variation);
                if (previewTexture != null) {
                    GUI.DrawTexture(position, previewTexture);
                }
            }
        }

        private void DrawVariationWeightSlider(Rect variationPosition, BrushOrientation orientation, int variationIndex)
        {
            Rect sliderPosition = new Rect {
                x = variationPosition.x + 1,
                y = variationPosition.yMax + 4,
                width = variationPosition.width - 3,
                height = 7
            };

            EditorGUI.BeginChangeCheck();

            int newWeight = RotorzEditorGUI.MiniSlider(sliderPosition, orientation.GetVariationWeight(variationIndex), 0, 100);

            if (EditorGUI.EndChangeCheck()) {
                // Make it easier to select the middle mark!
                if (newWeight >= 49 && newWeight <= 51) {
                    newWeight = 50;
                }

                // If user is holding control, apply change to same variation for all orientations.
                if (Event.current.control) {
                    foreach (var o in this.OrientedBrush.Orientations) {
                        this.AdjustWeightForVariation(o, variationIndex, newWeight);
                    }
                }
                else {
                    this.AdjustWeightForVariation(orientation, variationIndex, newWeight);
                }
            }
        }

        private void AdjustWeightForVariation(BrushOrientation orientation, int variationIndex, int newWeight)
        {
            if (variationIndex >= orientation.VariationCount) {
                return;
            }

            if (newWeight != orientation.GetVariationWeight(variationIndex)) {
                Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Adjust Variation Weight"));
                orientation.SetVariationWeight(variationIndex, newWeight);
                EditorUtility.SetDirty(this.Brush);
            }
        }

        #endregion


        #region Drag & Drop Variation Insertion

#pragma warning disable 414

        // Whilst at the time of writing the following field `isTrackingDragAndDrop` is
        // assigned but not used, it might be useful in the future...

        /// <summary>
        /// Indicates whether drag and drop is being tracked for orientation list area.
        /// </summary>
        private bool isTrackingDragAndDrop;

#pragma warning restore 414

        /// <summary>
        /// Indicates whether user is dragging objects into orientation list area.
        /// </summary>
        private bool isDraggingObjectsIntoList;
        /// <summary>
        /// List of potential variations which are being dragged.
        /// </summary>
        private List<Object> draggingVariations = new List<Object>();


        /// <summary>
        /// Begin tracking potential drag and drop insertion.
        /// </summary>
        private void BeginTrackingDragAndDropInsertion()
        {
            // Has user just dragged one or more items into orientation list?
            this.isTrackingDragAndDrop = true;
            // Clear any prior list of dragged variations.
            this.draggingVariations.Clear();

            Object[] draggedObjects = DragAndDrop.objectReferences;
            for (int i = 0; i < draggedObjects.Length; ++i) {
                var obj = draggedObjects[i];

                // Ensure that root object of a prefab is added!
                if (obj is GameObject) {
                    // Can only add prefab game objects!
                    if (PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab) {
                        continue;
                    }
                    obj = PrefabUtility.FindPrefabRoot(obj as GameObject);
                }

                // Only consider valid variations; otherwise simply ignore!
                if (this.ValidateNewVariation(obj)) {
                    this.draggingVariations.Add(obj);
                }
            }

            this.isDraggingObjectsIntoList = (this.draggingVariations.Count > 0);
        }

        /// <summary>
        /// Stop tracking to drag of potential variation objects.
        /// </summary>
        private void StopTrackingDragAndDropInsertion()
        {
            this.isTrackingDragAndDrop = false;
            this.isDraggingObjectsIntoList = false;
            this.draggingVariations.Clear();
        }

        /// <summary>
        /// Check to see whether objects are being dragged into orientation list.
        /// </summary>
        /// <param name="orientationSectionPosition">Position of orientation section.</param>
        private void CheckDragAndDropInsertion(Rect orientationSectionPosition)
        {
            if (this.isDraggingObjectsIntoList) {
                // Has user stopped dragging items into orientation list?
                if (DragAndDrop.objectReferences.Length == 0 || !orientationSectionPosition.Contains(Event.current.mousePosition)) {
                    this.StopTrackingDragAndDropInsertion();
                }
            }
            else if (DragAndDrop.objectReferences.Length > 0) {
                this.BeginTrackingDragAndDropInsertion();
            }
        }

        /// <summary>
        /// Checks to see if dragged variations have been dropped into orientation.
        /// </summary>
        /// <param name="position">Position of orientation track.</param>
        /// <param name="orientationControlID">Unique ID of orientation.</param>
        /// <param name="orientation">The orientation.</param>
        private void CheckDropInsertVariations(Rect position, int orientationControlID, BrushOrientation orientation)
        {
            if (!this.isDraggingObjectsIntoList) {
                return;
            }

            // Does mouse pointer overlap orientation track?
            if (!position.Contains(Event.current.mousePosition)) {
                return;
            }

            switch (Event.current.GetTypeForControl(orientationControlID)) {
                case EventType.DragUpdated:
                    DragAndDrop.activeControlID = orientationControlID;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    break;

                case EventType.DragPerform:
                    try {
                        // Only accept valid variations when performing drag and drop.
                        foreach (var variation in this.draggingVariations) {
                            this.AddPendingVariation(orientation, variation);
                        }
                    }
                    finally {
                        this.StopTrackingDragAndDropInsertion();

                        DragAndDrop.AcceptDrag();
                        DragAndDrop.visualMode = DragAndDropVisualMode.None;
                        Event.current.Use();
                    }
                    break;
            }
        }

        #endregion


        #region Pending Variations

        /// <summary>
        /// List of variations pending to be added to active orientation.
        /// </summary>
        private List<Object> pendingVariations = new List<Object>();


        /// <summary>
        /// Add variation to list of variations pending to be added to active orientation.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <param name="variation">Prefab or nestable brush.</param>
        private void AddPendingVariation(BrushOrientation orientation, Object variation)
        {
            if (variation == null) {
                return;
            }

            if (orientation.IndexOfVariation(variation) != -1) {
                this.Window.ShowNotification(new GUIContent(TileLang.Text("Orientation already contains this variation.")));
                return;
            }

            if (this.ValidateNewVariation(variation)) {
                this.pendingVariations.Add(variation);
            }
        }

        /// <summary>
        /// Accept any variations which are pending to be added to orientation.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        private void AcceptPendingVariations(BrushOrientation orientation)
        {
            if (this.pendingVariations.Count == 0) {
                return;
            }

            // Sort variations into alphabetical order for consistent insertion order.
            this.pendingVariations.Sort((a, b) => a.name.CompareTo(b.name));

            int insertIndex = s_TargetIndex;
            try {
                for (int i = 0; i < this.pendingVariations.Count; ++i) {
                    this.InsertNewVariation(orientation, insertIndex++, this.pendingVariations[i]);
                }
            }
            catch (Exception ex) {
                Debug.LogError(string.Format("Exception thrown when adding {0} variations.\n{1}", this.pendingVariations.Count, ex.Message));
            }

            this.pendingVariations.Clear();
            this.draggingVariations.Clear();

            //!Experiment: Removed to see if this is still needed!
            //GUIUtility.ExitGUI();
        }

        #endregion


        #region Methods

        /// <summary>
        /// Displays popup allowing user to specify an orientation to find or define.
        /// </summary>
        /// <remarks>
        /// <para>Interface is shown directly below the "Define or Find Orientation"
        /// button in header area of designer.</para>
        /// </remarks>
        protected void ShowDefineOrFindOrientationPopup()
        {
            DefineOrientationWindow.ShowAsDropDown(
                buttonRect: this.buttonPositionDefineOrFindOrientation,
                title: TileLang.ParticularText("Action", "Define or Find Orientation"),
                callback: this.OnDefineOrFindOrientation
            );
        }

        /// <summary>
        /// Handles selection of orientation from "Define or Find Orientation" window.
        /// </summary>
        /// <param name="mask">Orientation mask.</param>
        /// <param name="rotationalSymmetry">Indicates whether orientation will have rotation symmetry.</param>
        private void OnDefineOrFindOrientation(int mask, bool rotationalSymmetry)
        {
            if (rotationalSymmetry) {
                mask = OrientationUtility.FirstMaskWithRotationalSymmetry(mask);
            }

            var existingOrientation = this.OrientedBrush.FindOrientation(mask);
            bool makeDefault = false;

            // If orientation already exists, allow user to replace!
            if (rotationalSymmetry && existingOrientation != null && !existingOrientation.HasRotationalSymmetry) {
                if (!EditorUtility.DisplayDialog(
                    TileLang.Text("Orientation(s) already exist"),
                    TileLang.Text("One or more orientations already exist which share selected rotational symmetry.\n\nWould you like to remove these and insert a blank orientation?"),
                    TileLang.ParticularText("Action", "Yes"),
                    TileLang.ParticularText("Action", "Cancel")
                )) {
                    return;
                }

                Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Define Orientation"));

                int[] masks = OrientationUtility.GetMasksWithRotationalSymmetry(mask);
                for (int i = 0; i < masks.Length; ++i) {
                    // We might need to promote new orientation to the default orientation!
                    if (masks[i] == this.OrientedBrush.DefaultOrientationMask) {
                        makeDefault = true;
                    }

                    this.OrientedBrush.RemoveOrientation(masks[i]);
                }

                existingOrientation = null;
            }

            // Define orientation if it has not already been defined.
            if (existingOrientation == null) {
                Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Define Orientation"));

                this.OrientedBrush.AddOrientation(mask, rotationalSymmetry);
                if (makeDefault) {
                    this.OrientedBrush.DefaultOrientationMask = mask;
                }

                // Need to refresh brush preview?
                if (mask == 0) {
                    BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
                }

                EditorUtility.SetDirty(this.OrientedBrush);
            }
            else if (existingOrientation.HasRotationalSymmetry) {
                // We need to find first orientation in group.
                mask = OrientationUtility.FirstMaskWithRotationalSymmetry(mask);
            }

            // Scroll to orientation.
            int j = 0;
            foreach (var orientation in this.OrientedBrush.Orientations) {
                // Skip missing orientations and grouped orientations.
                if (!IsVisibleOrientation(orientation)) {
                    continue;
                }

                if (orientation.Mask == mask) {
                    viewScrollPosition.y = OrientationTrackHeight * j;
                    break;
                }
                ++j;
            }

            this.SetDirty();
        }

        /// <summary>
        /// Insert new variation into orientation.
        /// </summary>
        /// <param name="orientation">Orientation to add to.</param>
        /// <param name="insertIndex">Zero-based index for new variation in variations
        /// collection. This value is automatically clamped if out of range.</param>
        /// <param name="variation">The new variation.</param>
        protected void InsertNewVariation(BrushOrientation orientation, int insertIndex, Object variation)
        {
            if (!this.ValidateNewVariationWithUserNotification(variation)) {
                return;
            }

            insertIndex = Mathf.Clamp(insertIndex, 0, orientation.VariationCount);

            // Can only insert saved assets!
            if (!EditorUtility.IsPersistent(variation)) {
                return;
            }

            Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Add Variation"));

            // Insert the new variation!
            orientation.InsertVariation(insertIndex, variation);
            //!TODO: The following is not efficient when multiple variations are being added.
            this.OrientedBrush.SyncGroupedVariations(orientation.Mask);

            this.SetDirty();

            // Refresh preview?
            if (insertIndex == 0 && (orientation.Mask == 0 || (orientation.Mask == this.OrientedBrush.DefaultOrientationMask && this.OrientedBrush.FindOrientation(0) == null))) {
                BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
            }
        }

        /// <summary>
        /// Remove orientation from brush.
        /// </summary>
        /// <remarks>
        /// <para>This method presents user with a warning dialog allowing them to cancel
        /// removal of orientation.</para>
        /// </remarks>
        /// <param name="orientation">Orientation to remove from.</param>
        protected void RemoveOrientation(BrushOrientation orientation)
        {
            if (!EditorUtility.DisplayDialog(
                TileLang.ParticularText("Action", "Remove Orientation"),
                TileLang.Text("Do you really want to delete this orientation?"),
                TileLang.ParticularText("Action", "Yes"),
                TileLang.ParticularText("Action", "No")
            )) {
                return;
            }

            Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Remove Orientation"));

            bool needToRefreshPreview = (orientation.Mask == 0);

            // Use another default orientation?
            if (this.OrientedBrush.DefaultOrientationMask == orientation.Mask) {
                // Assume "Center Alone" by default!
                this.OrientedBrush.DefaultOrientationMask = 0;
                needToRefreshPreview = true;
            }

            // Remove orientation from object.
            this.OrientedBrush.RemoveOrientation(orientation.Mask);

            if (needToRefreshPreview) {
                BrushUtility.RefreshPreviewIncludingDependencies(this.Brush);
            }

            // Changes have been made!
            this.SetDirty();
        }

        #endregion
    }
}
