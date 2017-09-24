// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games.UnityEditorExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Base class for custom brush creator interfaces.
    /// </summary>
    /// <intro>
    /// <para>Custom brush creator interfaces can be defined and registered to provide
    /// users with custom sections in the "Create Brush" window.
    /// For information regarding the creation of brushes please refer to
    /// <a href="https://github.com/rotorz/unity3d-tile-system/wiki/Brushes">Brushes</a>
    /// section of the user guide.</para>
    /// </intro>
    /// <example>
    /// <para>Skeleton of a custom brush creator:</para>
    /// <code language="csharp"><![CDATA[
    /// using Rotorz.Tile.Editor;
    /// using UnityEditor;
    /// using UnityEngine;
    ///
    /// [InitializeOnLoad]
    /// public class MagicBrushCreator : BrushCreator
    /// {
    ///     static MagicBrushCreator()
    ///     {
    ///         Register<MagicBrushCreator>();
    ///     }
    ///
    ///
    ///     public MagicBrushCreator(EditorWindow window)
    ///         : base(window)
    ///     {
    ///     }
    ///
    ///
    ///     public override string Name {
    ///         get { return "Magic"; }
    ///     }
    ///
    ///     public override string Title {
    ///         get { return "Create new magic brush"; }
    ///     }
    ///
    ///     protected override void OnGUI()
    ///     {
    ///         // Present user interface to define new brush...
    ///         GUILayout.Label("Place controls here...");
    ///     }
    ///
    ///     protected override void OnButtonCreate()
    ///     {
    ///         // Validate inputs and create new brush...
    ///     }
    /// }
    /// ]]></code>
    /// <para>The above source code will present something like the following:</para>
    /// <para><img src="../art/MagicBrushCreator.png" alt="Brush creator window with 'Magic' example brush creator section selected."/></para>
    /// </example>
    /// <seealso cref="BrushCreator.Register{T}"/>
    /// <seealso cref="BrushCreator.Unregister{T}"/>
    /// <seealso cref="BrushCreatorGroupAttribute"/>
    [BrushCreatorGroup(BrushCreatorGroup.Default)]
    public abstract class BrushCreator
    {
        static BrushCreator()
        {
            s_Register = new List<Type>();

            // Register default creator types.
            Register<OrientedBrushCreator>();
            Register<TilesetCreator>();
            Register<AutotileCreator>();
            Register<EmptyBrushCreator>();

            // Register "Duplicate" style brush creator types.
            Register<AliasBrushCreator>();
            Register<DuplicateBrushCreator>();
        }


        #region Register

        internal static List<Type> s_Register;

        /// <summary>
        /// Register custom brush creator interface.
        /// </summary>
        /// <remarks>
        /// <para>Custom brush creator interfaces can only be registered once.</para>
        /// </remarks>
        /// <typeparam name="T">Type of brush creator.</typeparam>
        public static void Register<T>() where T : BrushCreator
        {
            if (!s_Register.Contains(typeof(T))) {
                s_Register.Add(typeof(T));
            }
        }

        private static bool Unregister(Type ty)
        {
            int i = s_Register.IndexOf(ty);
            if (i != -1) {
                s_Register.RemoveAt(i);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unregister brush creator interface.
        /// </summary>
        /// <typeparam name="T">Type of brush creator.</typeparam>
        /// <returns>
        /// A value of <c>true</c> when creator type was unregistered; otherwise <c>false</c>
        /// if creator was not previously registered.
        /// </returns>
        public static bool Unregister<T>() where T : BrushCreator
        {
            return Unregister(typeof(T));
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="BrushCreator"/> class.
        /// </summary>
        /// <param name="context">The context of the creator.</param>
        public BrushCreator(IBrushCreatorContext context)
        {
            this.Context = context;
        }


        /// <summary>
        /// Gets the context of the <see cref="BrushCreator"/>.
        /// </summary>
        public IBrushCreatorContext Context { get; private set; }


        /// <summary>
        /// Gets name of brush creator.
        /// </summary>
        /// <remarks>
        /// <para>Name is used for tab in brush creator window.</para>
        /// </remarks>
        public abstract string Name { get; }

        /// <summary>
        /// Gets title of brush creator.
        /// </summary>
        /// <remarks>
        /// <para>Title is presented at top of brush creator window when brush
        /// creator is active.</para>
        /// </remarks>
        public abstract string Title { get; }


        /// <summary>
        /// <see cref="OnEnable"/> is called when brush creator is first initialized.
        /// </summary>
        /// <remarks>
        /// <para>This method should be overridden to initialize brush creator.</para>
        /// </remarks>
        public virtual void OnEnable()
        {
        }

        /// <summary>
        /// <see cref="OnDisable"/> is called when brush creator is no longer required.
        /// </summary>
        /// <remarks>
        /// <para>This method should be overridden to tidy up brush creator before it is
        /// destroyed.</para>
        /// </remarks>
        public virtual void OnDisable()
        {
        }

        /// <summary>
        /// <see cref="OnShown"/> is called each time brush creator is shown.
        /// </summary>
        /// <remarks>
        /// <para>Brush creator is shown when tab is selected in brush creator window.</para>
        /// </remarks>
        public virtual void OnShown()
        {
        }

        /// <summary>
        /// <see cref="OnHidden"/> is called each time brush creator is hidden.
        /// </summary>
        /// <remarks>
        /// <para>Brush creator is shown when another tab is selected in brush creator
        /// window.</para>
        /// </remarks>
        public virtual void OnHidden()
        {
        }

        /// <summary>
        /// <see cref="OnGUI"/> is called for rendering and handling GUI events.
        /// </summary>
        /// <remarks>
        /// <para><see cref="OnGUI"/> will be called multiple times during a frame; once
        /// per event. See <a href="http://docs.unity3d.com/Documentation/ScriptReference/Event.html">Event</a>
        /// for more information about GUI events.</para>
        /// </remarks>
        public abstract void OnGUI();

        /// <summary>
        /// <see cref="OnButtonGUI"/> is called to handle brush creator buttons.
        /// </summary>
        /// <remarks>
        /// <para>This method can be overridden to provide additional buttons or to
        /// replace buttons entirely.</para>
        /// </remarks>
        /// <example>
        /// <para>Add custom button to brush creator user interface:</para>
        /// <code language="csharp"><![CDATA[
        /// public override void OnButtonGUI()
        /// {
        ///     if (GUILayout.BigButton("Reset", ExtraEditorStyles.Instance.BigButtonPadded)) {
        ///         this.OnResetButton();
        ///     }
        ///
        ///     // Display default buttons as normal.
        ///     base.OnButtonGUI();
        /// }
        /// ]]></code>
        /// </example>
        public virtual void OnButtonGUI()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TileLang.ParticularText("Action", "Create"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.OnButtonCreate();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(3f);

            if (GUILayout.Button(TileLang.ParticularText("Action", "Cancel"), ExtraEditorStyles.Instance.BigButtonPadded)) {
                this.OnButtonCancel();
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(5f);
        }

        /// <summary>
        /// <see cref="OnButtonCreate"/> is called when "Create" button is clicked.
        /// </summary>
        /// <remarks>
        /// <para>Brush creators provide their own implementation of this method to
        /// undertake the creation of brush asset(s).</para>
        /// </remarks>
        public virtual void OnButtonCreate()
        {
        }

        /// <summary>
        /// <see cref="OnButtonCreate"/> is called when "Cancel" button is clicked.
        /// </summary>
        /// <remarks>
        /// <para>Custom cancel button can reuse this functionality.</para>
        /// </remarks>
        public virtual void OnButtonCancel()
        {
            this.Context.Close();
        }


        /// <summary>
        /// Checks that specified asset name is valid.
        /// </summary>
        /// <remarks>
        /// <para>Error message dialog is shown if asset name is not valid.</para>
        /// </remarks>
        /// <param name="name">Name of asset.</param>
        /// <returns>
        /// A value of <c>true</c> when specified asset name is valid; otherwise <c>false</c>.
        /// </returns>
        protected bool ValidateAssetName(string name)
        {
            // Ensure that a name has been specified.
            if (!Regex.IsMatch(name, @"^[A-Za-z0-9()][A-Za-z0-9\-_ ()]*$")) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Invalid name for asset"),
                    TileLang.Text("Can only use alphanumeric characters (A-Z a-z 0-9), hyphens (-), underscores (_) and spaces.\n\nName must begin with an alphanumeric character."),
                    TileLang.ParticularText("Action", "Close")
                );
                this.Context.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks that specified asset name is both valid and unique.
        /// </summary>
        /// <remarks>
        /// <para>Error message dialog is shown if asset name is not valid or unique.</para>
        /// </remarks>
        /// <param name="name">Name of asset.</param>
        /// <returns>
        /// A value of <c>true</c> when specified asset name is valid; otherwise <c>false</c>.
        /// </returns>
        protected bool ValidateUniqueAssetName(string name)
        {
            if (!this.ValidateAssetName(name)) {
                return false;
            }

            // Ensure that asset path does not already exist.
            string assetPath = BrushUtility.GetBrushAssetPath() + name + ".asset";
            if (File.Exists(assetPath)) {
                EditorUtility.DisplayDialog(
                    TileLang.Text("Asset already exists"),
                    TileLang.Text("Please specify unique name for asset."),
                    TileLang.ParticularText("Action", "OK")
                );
                return false;
            }

            return true;
        }


        /// <summary>
        /// Draws the "Brush Name" input field.
        /// </summary>
        /// <remarks>
        /// <para>User input is a shared property that is stored under the key
        /// <see cref="BrushCreatorSharedPropertyKeys.BrushName"/>.</para>
        /// </remarks>
        protected void DrawBrushNameField()
        {
            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Name:"));
            GUI.SetNextControlName(this.Context.PrimaryAssetNameControlName);

            string brushName = this.Context.GetSharedProperty(BrushCreatorSharedPropertyKeys.BrushName, "");
            brushName = EditorGUILayout.TextField(brushName).Trim();
            this.Context.SetSharedProperty(BrushCreatorSharedPropertyKeys.BrushName, brushName);

            RotorzEditorGUI.MiniFieldDescription(TileLang.Text("Must start with alphanumeric character (A-Z a-z 0-9) and can contain hyphens (-), underscores (_) and spaces."));
        }

        /// <summary>
        /// Draws the "Tileset Name" input field.
        /// </summary>
        /// <remarks>
        /// <para>User input is a shared property that is stored under the key
        /// <see cref="BrushCreatorSharedPropertyKeys.TilesetName"/>.</para>
        /// </remarks>
        protected void DrawTilesetNameField()
        {
            ExtraEditorGUI.AbovePrefixLabel(TileLang.ParticularText("Property", "Tileset Name:"));
            GUI.SetNextControlName(this.Context.PrimaryAssetNameControlName);

            string tilesetName = this.Context.GetSharedProperty(BrushCreatorSharedPropertyKeys.TilesetName, "");
            tilesetName = EditorGUILayout.TextField(tilesetName).Trim();
            this.Context.SetSharedProperty(BrushCreatorSharedPropertyKeys.TilesetName, tilesetName);
        }
    }
}
