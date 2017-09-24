// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Settings
{
    /// <summary>
    /// Use this attribute to mark a property or a private field for serialization
    /// when composing a setting from a custom class or structure type.
    /// </summary>
    /// <example>
    /// <para>Define class with custom serialization for setting:</para>
    /// <code language="csharp"><![CDATA[
    /// public class CustomSettingType
    /// {
    ///     //
    ///     // Public fields are automatically serialized:
    ///     //
    ///     public bool shouldEnableSuperPowers;
    ///
    ///     //
    ///     // Serialize field for read-only name:
    ///     //
    ///     [SettingProperty("Name")]
    ///     private string _name;
    ///
    ///     public string Name {
    ///         get { return _name; }
    ///     }
    ///
    ///     //
    ///     // Serialize property for read-and-write:
    ///     //
    ///     private int _favouriteNumber = 42;
    ///
    ///     [SettingProperty]
    ///     public int FavouriteNumber {
    ///         get { return _favouriteNumber; }
    ///         set { _favouriteNumber = value; }
    ///     }
    ///
    ///     //
    ///     // Auto-properties can also be serialized:
    ///     //
    ///     [SettingProperty]
    ///     public int Score { get; private set; }
    /// }
    /// ]]></code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    internal sealed class SettingPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initialize new <see cref="SettingPropertyAttribute"/> instance and assume
        /// actual member name for serialization.
        /// </summary>
        public SettingPropertyAttribute()
        {
        }

        /// <summary>
        /// Initialize new <see cref="SettingPropertyAttribute"/> instance using custom
        /// property name instead of actual member name for serialization.
        /// </summary>
        /// <param name="propertyName">Custom property name for serialization.</param>
        public SettingPropertyAttribute(string propertyName)
        {
            this.Name = propertyName;
        }


        /// <summary>
        /// Gets custom name for field or property which will be used instead of
        /// actual member name.
        /// </summary>
        public string Name { get; private set; }
    }
}
