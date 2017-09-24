// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    internal sealed class AboutWindow : RotorzWindow
    {
        #region Window Management

        public static void ShowWindow()
        {
            GetUtilityWindow<AboutWindow>();
        }

        #endregion


        [NonSerialized]
        private string versionString;

        private GUIStyle labelTitleStyle;
        private GUIStyle labelVersionStyle;
        private GUIStyle labelLowerRightStyle;
        private GUIStyle hyperlinkStyle;


        /// <inheritdoc/>
        protected override void DoEnable()
        {
            this.titleContent = new GUIContent(string.Format(
                /* 0: name of product */
                TileLang.Text("About {0}"),
                ProductInfo.Name
            ));
            this.InitialSize = this.minSize = this.maxSize = new Vector2(471, 224);
            this.CenterWhenFirstShown = CenterMode.Always;

            this.versionString = string.Format(
                /* 0: main version string; for instance, "1.2.3"
                   1: special version name; for instance, "ALPHA 1"
                   2: hash of the commit; for instance, "7435f844caee0ec925d4497d81c36265a6615e91" */
                TileLang.Text("Version {0} {1}\n\nCommit {2}"),
                ProductInfo.Version, ProductInfo.Release, ProductInfo.CommitHash
            );

            GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            GUIStyle defaultLabelStyle = skin.label;

            Color labelTextColor;
            Color labelDarkTextColor;

            if (EditorGUIUtility.isProSkin) {
                labelTextColor = new Color(0.9f, 0.9f, 0.9f);
                labelDarkTextColor = new Color(0.25f, 0.25f, 0.25f);
            }
            else {
                labelTextColor = new Color(0.25f, 0.25f, 0.25f);
                labelDarkTextColor = new Color(0.2f, 0.2f, 0.2f);
            }

            this.labelTitleStyle = new GUIStyle(defaultLabelStyle);
            this.labelTitleStyle.fontSize = 26;
            this.labelTitleStyle.normal.textColor = labelDarkTextColor;

            this.labelVersionStyle = new GUIStyle(defaultLabelStyle);
            this.labelVersionStyle.normal.textColor = labelDarkTextColor;

            this.labelLowerRightStyle = new GUIStyle(defaultLabelStyle);
            this.labelLowerRightStyle.alignment = TextAnchor.LowerRight;
            this.labelLowerRightStyle.padding = new RectOffset(0, 10, 0, 22);
            this.labelLowerRightStyle.normal.textColor = labelTextColor;

            this.hyperlinkStyle = new GUIStyle(defaultLabelStyle);
            this.hyperlinkStyle.stretchWidth = false;
            this.hyperlinkStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(18f / 255f, 174f / 255f, 1f)
                : Color.blue;
        }

        /// <inheritdoc/>
        protected override void DoGUI()
        {
            Event e = Event.current;
            Rect position;

            GUILayout.Space(10);

            // Draw header area.
            using (var titleContent = ControlContent.Basic(ProductInfo.Name)) {
                position = GUILayoutUtility.GetRect(1, 100);
                if (e.type == EventType.Repaint) {
                    Rect headerBackgroundPosition = new Rect(position.x - 2, position.y - 12, position.width + 4, position.height);

                    if (EditorGUIUtility.isProSkin) {
                        GUI.DrawTexture(headerBackgroundPosition, EditorGUIUtility.whiteTexture);
                    }
                    else {
                        RotorzEditorStyles.Instance.TransparentBox.Draw(headerBackgroundPosition, GUIContent.none, false, false, false, false);
                    }

                    Texture2D texAssetBadge = RotorzEditorStyles.Skin.Badge;
                    GUI.DrawTexture(new Rect(position.x + position.width - texAssetBadge.width - 6, position.y - 3, texAssetBadge.width, texAssetBadge.height), texAssetBadge);
                }

                position.x += 10;
                position.width -= 10;

                GUI.Label(position, titleContent, this.labelTitleStyle);

                Rect versionLabelPosition = new Rect(position.x, position.y + this.labelTitleStyle.CalcHeight(titleContent, position.width), position.width, 42);
                GUI.Label(versionLabelPosition, this.versionString, this.labelVersionStyle);

                GUILayout.BeginHorizontal();
                this.AddLink(TileLang.ParticularText("Online", "Repository"), "https://github.com/rotorz/unity3d-tile-system", "https://github.com/rotorz/unity3d-tile-system");
                GUILayout.EndHorizontal();
            }

            ExtraEditorGUI.SeparatorLight(marginTop: 17, marginBottom: 7);

            // Draw footer area.
            var vendorBadge = RotorzEditorStyles.Skin.VendorBadge;
            position = GUILayoutUtility.GetRect(1, vendorBadge.height + 5);
            if (e.type == EventType.Repaint) {
                GUI.DrawTexture(new Rect(position.x + 7, position.y - 2, vendorBadge.width, vendorBadge.height), vendorBadge);

                position.y -= 4;
                this.labelLowerRightStyle.Draw(position, "©2011-2017 Rotorz Limited. All rights reserved.", false, false, false, false);
            }
        }

        private void AddLink(string title, string caption, string href)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(title);
            if (GUILayout.Button(caption, this.hyperlinkStyle)) {
                Help.BrowseURL(href);
                GUIUtility.ExitGUI();
            }

            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.EndVertical();
        }
    }
}
