// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Provides internal helper functionality.
    /// </summary>
    [InitializeOnLoad]
    internal sealed class EditorInternalUtility : InternalUtility
    {
        static EditorInternalUtility()
        {
            InternalUtility.Instance = new EditorInternalUtility();

            InitRepaintGameViewsDelegate();

            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        internal enum HoverTipState
        {
            NotShown = 0,
            SkipFirst,
            ReadyToShow,
            Shown,
            ReadyToHide
        }

        internal static EditorWindow HoverWindow { get; set; }
        internal static HoverTipState HoverTipStage { get; set; }
        internal static double ReadyToHideTime { get; set; }

        private static void OnEditorUpdate()
        {
            // Repaint previous hover window if mouse is no longer hovering over it.
            if (HoverWindow != EditorWindow.mouseOverWindow && HoverWindow != null) {
                HoverWindow.Repaint();
                HoverWindow = null;
            }

            // Disable hover tip if it has been 1 second since it was ready to hide.
            if (HoverTipStage == HoverTipState.ReadyToHide) {
                if (EditorApplication.timeSinceStartup - ReadyToHideTime > 1.0) {
                    HoverTipStage = HoverTipState.NotShown;
                }
            }

            // Note: This will only occur for Windows!
            if (s_QueueDropDownWindow != null) {
                s_QueueDropDownWindow.SendEvent(s_QueueDropDownEvent);
                s_QueueDropDownWindow = null;
                s_QueueDropDownEvent = null;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (!(InternalUtility.Instance is EditorInternalUtility)) {
                InternalUtility.Instance = new EditorInternalUtility();
            }
        }


        #region Dropdown Menu Hack

        private static EditorWindow s_QueueDropDownWindow;
        private static Event s_QueueDropDownEvent;

        private static GUIContent s_TempMenuContent = new GUIContent();

        public static bool DropdownMenu(Rect position, GUIContent content, GUIStyle style)
        {
            int controlID = RotorzEditorGUI.GetHoverControlID(position, content.tooltip);

            if (Application.platform == RuntimePlatform.WindowsEditor) {
                switch (Event.current.GetTypeForControl(controlID)) {
                    case EventType.MouseDown:
                        if (position.Contains(Event.current.mousePosition)) {
                            GUIUtility.hotControl = controlID;
                            Event.current.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlID && Event.current.button == 42) {
                            GUIUtility.hotControl = 0;
                            return true;
                        }
                        break;

                    case EventType.Repaint:
                        s_TempMenuContent.text = content.text;
                        s_TempMenuContent.image = content.image;
                        style.Draw(position, s_TempMenuContent, controlID);

                        if (GUIUtility.hotControl == controlID) {
                            s_QueueDropDownWindow = EditorWindow.focusedWindow;// window;
                            s_QueueDropDownEvent = new Event();
                            s_QueueDropDownEvent.type = EventType.MouseDrag;
                            s_QueueDropDownEvent.button = 42;
                        }
                        break;
                }
            }
            else {
                switch (Event.current.GetTypeForControl(controlID)) {
                    case EventType.MouseDown:
                        if (position.Contains(Event.current.mousePosition)) {
                            Event.current.Use();
                            return true;
                        }
                        break;

                    case EventType.Repaint:
                        s_TempMenuContent.text = content.text;
                        s_TempMenuContent.image = content.image;
                        style.Draw(position, s_TempMenuContent, controlID);
                        break;
                }
            }

            return false;
        }

        public static bool DropdownMenu(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect position = GUILayoutUtility.GetRect(content, style, options);
            return DropdownMenu(position, content, style);
        }

        #endregion


        #region IO

        /// <summary>
        /// Ensures that asset folder exists.
        /// </summary>
        /// <param name="assetPath">Asset path starting with 'Assets/'.</param>
        internal static void EnsureAssetFolderExists(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("Asset path must begin with 'Assets/'.");
            }

            string systemPath = Application.dataPath;
            string parentPath = "Assets";

            string[] fragments = assetPath.Split('\\', '/');
            for (int i = 1; i < fragments.Length; ++i) {
                if (fragments[i] == "") {
                    break;
                }

                systemPath = Path.Combine(systemPath, fragments[i]);
                if (!Directory.Exists(systemPath)) {
                    AssetDatabase.CreateFolder(parentPath, fragments[i]);
                }

                parentPath += "/" + fragments[i];
            }
        }

        #endregion


        #region Asset Folders

        public static void EnsureThatAssetFolderExists(string folderAssetPath)
        {
            if (folderAssetPath == null) {
                throw new ArgumentNullException("folderAssetPath");
            }
            if (folderAssetPath == "") {
                throw new ArgumentException("Cannot be empty string.", "folderAssetPath");
            }

            // No point in even attempting to create the 'Assets' folder...
            if (folderAssetPath.Equals("Assets", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            string absoluteDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), folderAssetPath);
            if (!Directory.Exists(absoluteDirectoryPath)) {
                Directory.CreateDirectory(absoluteDirectoryPath);
            }
        }

        #endregion


        #region Texture Assets

        public static Texture2D SavePngAsset(string assetPath, Texture2D texture)
        {
            File.WriteAllBytes(
                Directory.GetCurrentDirectory() + "/" + assetPath,
                texture.EncodeToPNG()
            );

            AssetDatabase.ImportAsset(assetPath);

            var textureImporter = TextureImporter.GetAtPath(assetPath) as TextureImporter;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(assetPath);

            return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
        }


        #region GetImageSize :: Hack (Not Used - Faster)
        /*
        /// <summary>
        /// Get actual size of texture asset image.
        /// </summary>
        /// <param name="asset">Texture asset.</param>
        /// <param name="width">Outputs image width or zero when not retrieved.</param>
        /// <param name="height">Outputs image height or zero when not retrieved.</param>
        /// <returns>
        /// A value of <c>true</c> if image size was retrieved; otherwise <c>false</c>.
        /// </returns>
        public static bool GetImageSize(Texture2D asset, out int width, out int height)
        {
            if (asset != null) {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null) {
                    object[] args = new object[2] { 0, 0 };
                    var mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                    mi.Invoke(importer, args);

                    width = (int)args[0];
                    height = (int)args[1];

                    return true;
                }
            }

            height = width = 0;
            return false;
        }
        */
        #endregion


        #region GetImageSize :: Read Headers Manually (Slower)
        //*
        /// <summary>
        /// Get actual size of texture asset image.
        /// </summary>
        /// <remarks>
        /// <para>The following file formats are supported:</para>
        /// <list type="bullet">
        ///    <item>PSD</item>
        ///    <item>PNG</item>
        ///    <item>TIFF</item>
        ///    <item>JPG</item>
        ///    <item>TGA</item>
        ///    <item>GIF</item>
        ///    <item>BMP</item>
        ///    <item>IFF</item>
        /// </list>
        /// </remarks>
        /// <param name="asset">Texture asset.</param>
        /// <param name="width">Outputs image width or zero when not retrieved.</param>
        /// <param name="height">Outputs image height or zero when not retrieved.</param>
        /// <returns>
        /// A value of <c>true</c> if image size was retrieved; otherwise <c>false</c>.
        /// </returns>
        public static bool GetImageSize(Texture2D asset, out int width, out int height)
        {
            if (asset != null) {
                if (AssetDatabase.IsMainAsset(asset)) {
                    string assetPath = AssetDatabase.GetAssetPath(asset);
                    return GetImageSize(assetPath, out width, out height);
                }
                else {
                    width = asset.width;
                    height = asset.height;
                    return true;
                }
            }

            height = width = 0;
            return false;
        }

        /// <summary>
        /// Get width and height of image file.
        /// </summary>
        /// <remarks>
        /// <para>The following file formats are supported:</para>
        /// <list type="bullet">
        ///    <item>PSD</item>
        ///    <item>PNG</item>
        ///    <item>TIFF</item>
        ///    <item>JPG</item>
        ///    <item>TGA</item>
        ///    <item>GIF</item>
        ///    <item>BMP</item>
        ///    <item>IFF</item>
        /// </list>
        /// </remarks>
        /// <param name="fileName">Absolute path to image file.</param>
        /// <param name="width">Outputs width of image in pixels.</param>
        /// <param name="height">Outputs height of image in pixels.</param>
        /// <returns>
        /// A value of <c>true</c> when image size was determined; otherwise a value of <c>false</c>.
        /// </returns>
        public static bool GetImageSize(string fileName, out int width, out int height)
        {
            width = height = -1;

            // PSD, TIFF, JPG, TGA, PNG, GIF, BMP, IFF

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                int c1 = fs.ReadByte();
                int c2 = fs.ReadByte();
                int c3 = fs.ReadByte();
                int c4 = fs.ReadByte();

                if (c1 == 'G' && c2 == 'I' && c3 == 'F') {                      // GIF
                    fs.Seek(3 + 3, SeekOrigin.Begin);
                    width = ReadInt(fs, 2, false);
                    height = ReadInt(fs, 2, false);
                    return true;
                }
                else if (c1 == 0xFF && c2 == 0xD8) {                            // JPG
                    fs.Seek(3, SeekOrigin.Begin);
                    while (c3 == 255) {
                        int marker = fs.ReadByte();
                        int len = ReadInt(fs, 2, true);
                        if (marker == 192 || marker == 193 || marker == 194) {
                            fs.Seek(1, SeekOrigin.Current);
                            height = ReadInt(fs, 2, true);
                            width = ReadInt(fs, 2, true);
                            return true;
                        }
                        fs.Seek(len - 2, SeekOrigin.Current);
                        c3 = fs.ReadByte();
                    }
                }
                else if (c1 == 137 && c2 == 80 && c3 == 78) {                   // PNG
                    fs.Seek(3 + 15, SeekOrigin.Begin);
                    width = ReadInt(fs, 2, true);
                    fs.Seek(2, SeekOrigin.Current);
                    height = ReadInt(fs, 2, true);
                    return true;
                }
                else if (c1 == 66 && c2 == 77) {                                // BMP
                    fs.Seek(3 + 15, SeekOrigin.Begin);
                    width = ReadInt(fs, 2, false);
                    fs.Seek(2, SeekOrigin.Current);
                    height = ReadInt(fs, 2, false);
                    return true;
                }
                else if (c1 == '8' && c2 == 'B' && c3 == 'P' && c4 == 'S') {    // PSD
                    int ver = ReadInt(fs, 2, true);
                    if (ver == 1) {
                        fs.Seek(6 + 2, SeekOrigin.Current);
                        height = ReadInt(fs, 4, true);
                        width = ReadInt(fs, 4, true);
                        return true;
                    }
                }
                else if (fileName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)) {  // TGA
                    fs.Seek(12, SeekOrigin.Begin);
                    width = ReadInt(fs, 2, false);
                    height = ReadInt(fs, 2, false);
                    return true;
                }
                else if ((c1 == 'M' && c2 == 'M' && c3 == 0 && c4 == 42) || (c1 == 'I' && c2 == 'I' && c3 == 42 && c4 == 0)) {  // TIFF
                    bool bigEndian = c1 == 'M';
                    int ifd = ReadInt(fs, 4, bigEndian);
                    fs.Seek(ifd - 8, SeekOrigin.Current);
                    int entries = ReadInt(fs, 2, bigEndian);
                    for (int i = 0; i < entries; ++i) {
                        int tag = ReadInt(fs, 2, bigEndian);
                        int fieldType = ReadInt(fs, 2, bigEndian);
#pragma warning disable 219
                        int count = ReadInt(fs, 4, bigEndian);
#pragma warning restore 219
                        int valOffset;

                        if (fieldType == 3 || fieldType == 8) {
                            valOffset = ReadInt(fs, 2, bigEndian);
                            fs.Seek(2, SeekOrigin.Current);
                        }
                        else {
                            valOffset = ReadInt(fs, 4, bigEndian);
                        }

                        if (tag == 256) {
                            width = valOffset;
                        }
                        else if (tag == 257) {
                            height = valOffset;
                        }

                        if (width != -1 && height != -1) {
                            return true;
                        }
                    }
                }
                else if (c1 == 'F' && c2 == 'O' && c3 == 'R' && c4 == '4') {    // Maya IFF
                    int length = ReadInt(fs, 4, true);
                    if (fs.ReadByte() == 'C' && fs.ReadByte() == 'I' && fs.ReadByte() == 'M' && fs.ReadByte() == 'G') {
                        while (fs.CanRead) {
                            if (fs.ReadByte() == 'T' && fs.ReadByte() == 'B' && fs.ReadByte() == 'H' && fs.ReadByte() == 'D') {
                                fs.Seek(4, SeekOrigin.Current);

                                width = ReadInt(fs, 4, true);
                                height = ReadInt(fs, 4, true);
                                return true;
                            }

                            // Skip to next chunk.
                            length = ReadInt(fs, 4, true);
                            fs.Seek(length, SeekOrigin.Current);
                        }
                    }
                }
                else if (c1 == 'F' && c2 == 'O' && c3 == 'R' && c4 == 'M') {    // Amiga IFF
                    int length = ReadInt(fs, 4, true);
                    if (fs.ReadByte() == 'I' && fs.ReadByte() == 'L' && fs.ReadByte() == 'B' && fs.ReadByte() == 'M') {
                        while (fs.CanRead) {
                            if (fs.ReadByte() == 'B' && fs.ReadByte() == 'M' && fs.ReadByte() == 'H' && fs.ReadByte() == 'D') {
                                fs.Seek(4, SeekOrigin.Current);

                                width = ReadInt(fs, 2, true);
                                height = ReadInt(fs, 2, true);
                                return true;
                            }

                            // Skip to next chunk.
                            length = ReadInt(fs, 4, true);
                            fs.Seek(length, SeekOrigin.Current);
                        }
                    }
                }
            }

            width = height = 0;

            return false;
        }

        private static int ReadInt(FileStream fs, int bytes, bool bigEndian)
        {
            int result = 0;
            int sv = bigEndian ? ((bytes - 1) * 8) : 0;
            int cnt = bigEndian ? -8 : 8;

            for (int i = 0; i < bytes; ++i) {
                result |= fs.ReadByte() << sv;
                sv += cnt;
            }

            return result;
        }

        //*/
        #endregion


        /// <summary>
        /// Load uncompressed variation of specified texture asset.
        /// </summary>
        /// <remarks>
        /// <para>Please ensure that returned texture is destroyed when it is no longer
        /// needed. It's <see cref="Object.hideFlags"/> property is set to <see cref="HideFlags.DontSave"/>.</para>
        /// </remarks>
        /// <param name="textureAsset">Texture asset.</param>
        /// <returns>
        /// Uncompressed and unfiltered texture without mip-maps.
        /// </returns>
        public static Texture2D LoadTextureUncompressed(Texture2D textureAsset)
        {
            if (textureAsset == null) {
                return null;
            }

            string assetPath = AssetDatabase.GetAssetPath(textureAsset);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) {
                return null;
            }

            var originalSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(originalSettings);

            // Temporarily change texture importer settings.
            Texture2D uncompressed;
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            AssetDatabase.ImportAsset(assetPath);

            // Load and duplicate texture.
            uncompressed = Object.Instantiate(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D))) as Texture2D;
            uncompressed.hideFlags = HideFlags.HideAndDontSave;

            importer.SetTextureSettings(originalSettings);
            AssetDatabase.ImportAsset(assetPath);

            return uncompressed;

            /*string assetPath = Directory.GetCurrentDirectory() + "/" + AssetDatabase.GetAssetPath(textureAsset);
            Texture2D uncompressed = new Texture2D(textureAsset.width, textureAsset.height, TextureFormat.ARGB32, false);
            uncompressed.hideFlags = HideFlags.DontSave;

            using (FileStream fs = new FileStream(assetPath, FileMode.Open, FileAccess.Read)) {
                BinaryReader br = new BinaryReader(fs);
                uncompressed.LoadImage(br.ReadBytes((int)fs.Length));
                uncompressed.Apply(false, false);
            }

            return uncompressed;*/
        }

        #endregion


        #region Repaint All Game Views

        private delegate void RepaintAllGameViewsDelegate();

        private static RepaintAllGameViewsDelegate s_RepaintAllGameViews;

        public static void InitRepaintGameViewsDelegate()
        {
            string assemblyName = typeof(UnityEditor.EditorWindow).Assembly.FullName;

            Type gameViewClass = Type.GetType("UnityEditor.GameView, " + assemblyName, false);
            if (gameViewClass != null) {
                MethodInfo method = gameViewClass.GetMethod("RepaintAll", BindingFlags.Static | BindingFlags.Public);
                if (method != null) {
                    s_RepaintAllGameViews = (RepaintAllGameViewsDelegate)Delegate.CreateDelegate(typeof(RepaintAllGameViewsDelegate), method);
                }
            }
        }

        public static void RepaintAllGameViews()
        {
            if (s_RepaintAllGameViews != null) {
                s_RepaintAllGameViews();
            }
        }

        #endregion


        #region Window Management

        public static void FocusInspectorWindow()
        {
            // Display and focus the inspector window.
            string assemblyName = typeof(UnityEditor.EditorWindow).Assembly.FullName;
            Type inspectorWindowClass = Type.GetType("UnityEditor.InspectorWindow, " + assemblyName, false);
            if (inspectorWindowClass != null) {
                EditorWindow.GetWindow(inspectorWindowClass);
            }
        }

        #endregion


        public EditorInternalUtility()
        {
            this.eraseEmptyChunks = (int)RtsPreferences.EraseEmptyChunksPreference.Value;
        }

        public override void HideEditorWireframeImpl(GameObject go)
        {
            // Only attempt to hide wireframe when a tool is active.
            if (ToolManager.Instance.CurrentTool == null) {
                return;
            }

            // Remove wireframe to reduce clutter when painting.
            foreach (var renderer in go.GetComponentsInChildren<Renderer>()) {
                EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Hidden);
            }
        }


        #region Object Factory Resolution

        public override IObjectFactory ResolveObjectFactory()
        {
            return Application.isPlaying
                ? DefaultRuntimeObjectFactory.Current
                : DefaultEditorObjectFactory.Current;
        }

        #endregion


        #region Progress Handlers

        protected override void ClearProgressImpl()
        {
            EditorUtility.ClearProgressBar();
        }

        protected override void ProgressHandlerImpl(string title, string message, float progress)
        {
            EditorUtility.DisplayProgressBar(title, message, progress);
        }

        protected override bool CancelableProgressHandlerImpl(string title, string message, float progress)
        {
            return EditorUtility.DisplayCancelableProgressBar(title, message, progress);
        }

        #endregion
    }
}
