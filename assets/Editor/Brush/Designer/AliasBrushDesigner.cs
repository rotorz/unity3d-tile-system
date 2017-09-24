// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Designer for <see cref="AliasBrush"/> brushes.
    /// </summary>
    /// <remarks>
    /// <para>Each kind of brush can have two registered designers; one for editing brush
    /// instances, and another for editing aliases of its kind.</para>
    /// <para>This class can be extended to create custom alias brush designers. Custom
    /// alias brush designers must be registered using <see cref="BrushUtility.RegisterDescriptor"/>.</para>
    /// <para><strong>Advice: </strong> It is usually a good idea to inherit base
    /// functionality when overriding methods.</para>
    /// </remarks>
    /// <seealso cref="BrushDesignerView"/>
    /// <seealso cref="BrushUtility.RegisterDescriptor"/>
    public class AliasBrushDesigner : BrushDesignerView
    {
        private bool isTargetMasterBrush;


        /// <summary>
        /// Gets the alias brush that is being edited.
        /// </summary>
        public AliasBrush AliasBrush { get; private set; }


        /// <inheritdoc/>
        public override void OnEnable()
        {
            base.OnEnable();

            this.AliasBrush = this.Brush as AliasBrush;

            var targetBrushRecord = BrushDatabase.Instance.FindRecord(this.AliasBrush.target);
            if (targetBrushRecord != null) {
                this.isTargetMasterBrush = targetBrushRecord.IsMaster;
            }
        }

        /// <inheritdoc/>
        public override void DrawSecondaryMenuButton(Rect position)
        {
            var brushRecord = BrushDatabase.Instance.FindRecord(this.AliasBrush.target);
            EditorGUI.BeginDisabledGroup(brushRecord == null || brushRecord.IsMaster);
            {
                using (var content = ControlContent.Basic(
                    RotorzEditorStyles.Skin.GotoTarget,
                    TileLang.FormatActionWithShortcut(
                        TileLang.ParticularText("Action", "Goto Target Brush"), "F3"
                    )
                )) {
                    if (RotorzEditorGUI.HoverButton(position, content)) {
                        this.ShowTargetBrushInDesigner();
                        GUIUtility.ExitGUI();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            // Permit shortcut key "F3".
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F3) {
                Event.current.Use();
                this.ShowTargetBrushInDesigner();
                GUIUtility.ExitGUI();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginHorizontal(GUILayout.Width(114));
                {
                    // Draw preview.
                    string tooltipText = !this.isTargetMasterBrush ? TileLang.Text("Double-click to edit target...") : "";
                    using (var content = ControlContent.Basic("", tooltipText)) {
                        GUILayout.Box(content, GUILayout.Width(114), GUILayout.Height(114));
                        Rect previewRect = GUILayoutUtility.GetLastRect();
                        previewRect.x += 2;
                        previewRect.y += 2;
                        previewRect.width -= 4;
                        previewRect.height -= 4;
                        RotorzEditorGUI.DrawBrushPreview(previewRect, this.AliasBrush.target);

                        // Select target brush for editing upon double-clicking
                        Event e = Event.current;
                        if (e.isMouse && e.clickCount == 2 && previewRect.Contains(e.mousePosition) && !this.isTargetMasterBrush) {
                            this.ShowTargetBrushInDesigner();
                            GUIUtility.ExitGUI();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                {
                    GUILayout.Space(15);

                    ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Alias Target:"), RotorzEditorStyles.Instance.BoldLabel);

                    Brush newAliasTarget = RotorzEditorGUI.BrushField(this.AliasBrush.target, false);
                    if (newAliasTarget != this.AliasBrush.target) {
                        this.SetAliasTarget(newAliasTarget);
                    }

                    GUILayout.Space(5);

                    EditorGUI.BeginDisabledGroup(this.AliasBrush.target == null);
                    if (GUILayout.Button(TileLang.ParticularText("Action", "Revert To Target"), RotorzEditorStyles.Instance.ButtonWide)) {
                        if (EditorUtility.DisplayDialog(
                            TileLang.ParticularText("Dialog Title", "Confirmation"),
                            TileLang.Text("Revert properties of alias brush to target brush?"),
                            TileLang.ParticularText("Action", "Yes"),
                            TileLang.ParticularText("Action", "No")
                        )) {
                            Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Revert To Target"));
                            GUIUtility.keyboardControl = 0;
                            this.AliasBrush.RevertToTarget();
                        }
                        GUIUtility.ExitGUI();
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            // Do not display material mapper for 'tileset' targets.
            if (!(this.AliasBrush.target is TilesetBrush)) {
                this.Section_MaterialMapper();
            }
        }

        /// <inheritdoc/>
        protected internal override void EndExtendedProperties()
        {
            if (this.AliasBrush.target is OrientedBrush)
                OrientedBrushDesigner.ShowExtendedOrientation = RotorzEditorGUI.FoldoutSection(OrientedBrushDesigner.ShowExtendedOrientation,
                label: TileLang.Text("Automatic Orientations"),
                    callback: this.OnExtendedGUI_Coalescing
                );

            base.EndExtendedProperties();
        }


        private void ShowTargetBrushInDesigner()
        {
            ToolUtility.ShowBrushInDesigner(this.AliasBrush.target);
        }

        /// <summary>
        /// Specify brush that the edited brush is an alias of.
        /// </summary>
        /// <param name="brush">Target brush.</param>
        public void SetAliasTarget(Brush brush)
        {
            // Do not proceed if no changes have been made.
            if (brush == this.AliasBrush.target) {
                return;
            }

            Undo.RecordObject(this.Brush, TileLang.ParticularText("Action", "Set Target"));

            var brushDescriptor = (brush != null)
                ? BrushUtility.GetDescriptor(brush.GetType())
                : null;

            if (brush == this.AliasBrush || brush is AliasBrush) {
                // Brush cannot be an alias of itself or another alias.
                if (this.Window != null) {
                    this.Window.ShowNotification(new GUIContent(TileLang.Text("Cannot create alias of another alias brush.")));
                }
                this.AliasBrush.target = null;
            }
            else if (brush == null) {
                // No brush was specified, clear target.
                this.AliasBrush.target = null;
            }
            else if (brushDescriptor == null) {
                // Unknown target brush.
                if (this.Window != null) {
                    string targetBrushNicifiedName = ObjectNames.NicifyVariableName(brush.GetType().Name);
                    this.Window.ShowNotification(new GUIContent(string.Format(
                        /* 0: nicified name of the target brush class (i.e. 'Uber Oriented Brush') */
                        TileLang.Text("Cannot create alias of the unregistered brush '{0}'."),
                        targetBrushNicifiedName
                    )));
                }
                this.AliasBrush.target = null;
            }
            else if (!brushDescriptor.SupportsAliases) {
                // Brush does not support aliases.
                if (this.Window != null) {
                    this.Window.ShowNotification(new GUIContent(string.Format(
                        /* 0: name of the target brush (i.e. 'Grass Platform') */
                        TileLang.Text("Cannot create alias of '{0}'."),
                        brushDescriptor.DisplayName
                    )));
                }
                this.AliasBrush.target = null;
            }
            else {
                if (this.Window != null) {
                    this.Window.RemoveNotification();
                }

                // Update alias reference.
                this.AliasBrush.target = brush;
            }

            // Find out if target brush is a master brush.
            var targetBrushRecord = BrushDatabase.Instance.FindRecord(this.AliasBrush.target);
            if (targetBrushRecord != null) {
                this.isTargetMasterBrush = targetBrushRecord.IsMaster;
            }

            this.SetDirty();

            BrushUtility.RefreshPreview(this.Brush);
        }
    }
}
