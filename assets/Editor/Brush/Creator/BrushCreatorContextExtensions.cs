// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Games;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Extension methods for the <see cref="IBrushCreatorContext"/> interface.
    /// </summary>
    public static class BrushCreatorContextExtensions
    {
        /// <summary>
        /// Sets the value of a shared property.
        /// </summary>
        /// <remarks>
        /// <para>Refer to <see cref="BrushCreatorSharedPropertyKeys"/> for the built-in
        /// shared property keys.</para>
        /// <para>DO NOT use square brackets around custom shared property key names.
        /// This is a convention used only for the built-in shared properties to avoid
        /// clashes if new built-in's are added in the future.</para>
        /// </remarks>
        /// <typeparam name="T">The type of value of interest.</typeparam>
        /// <param name="context">The <see cref="IBrushCreatorContext"/>.</param>
        /// <param name="key">Key of the shared property.</param>
        /// <param name="value">The value to assign.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="key"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="key"/> is empty or is not a string.
        /// </exception>
        public static void SetSharedProperty(this IBrushCreatorContext context, string key, object value)
        {
            ExceptionUtility.CheckExpectedStringArgument(key, "key");

            context.SharedProperties[key] = value;
        }

        /// <summary>
        /// Determines whether a value is currently defined for a given shared property.
        /// </summary>
        /// <typeparam name="T">The type of value of interest.</typeparam>
        /// <param name="context">The <see cref="IBrushCreatorContext"/>.</param>
        /// <param name="key">Key of the shared property.</param>
        /// <returns>
        /// A value of <see langref="true"/>  if the shared property is defined with a
        /// compatible type; otherwise, a value of <see langref="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="key"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="key"/> is empty or is not a string.
        /// </exception>
        public static bool IsSharedPropertyDefined<T>(this IBrushCreatorContext context, string key)
        {
            ExceptionUtility.CheckExpectedStringArgument(key, "key");

            object value;
            if (context.SharedProperties.TryGetValue(key, out value)) {
                return value != null && typeof(T).IsAssignableFrom(value.GetType());
            }
            return false;
        }

        /// <summary>
        /// Determines whether a value is currently defined for a given shared property.
        /// </summary>
        /// <param name="context">The <see cref="IBrushCreatorContext"/>.</param>
        /// <param name="key">Key of the shared property.</param>
        /// <returns>
        /// A value of <see langref="true"/>  if the shared property is defined;
        /// otherwise, a value of <see langref="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="key"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="key"/> is empty or is not a string.
        /// </exception>
        public static bool IsSharedPropertyDefined(this IBrushCreatorContext context, string key)
        {
            ExceptionUtility.CheckExpectedStringArgument(key, "key");

            object value;
            if (context.SharedProperties.TryGetValue(key, out value)) {
                return value != null;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of a shared property.
        /// </summary>
        /// <remarks>
        /// <para>Refer to <see cref="BrushCreatorSharedPropertyKeys"/> for the built-in
        /// shared property keys.</para>
        /// </remarks>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="context">The <see cref="IBrushCreatorContext"/>.</param>
        /// <param name="key">Key of the shared property.</param>
        /// <param name="defaultValue">The default value to assume if the shared property
        /// has not been defined yet.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="key"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If <paramref name="key"/> is empty or is not a string.
        /// </exception>
        public static T GetSharedProperty<T>(this IBrushCreatorContext context, string key, T defaultValue = default(T))
        {
            ExceptionUtility.CheckExpectedStringArgument(key, "key");

            object value;
            if (context.SharedProperties.TryGetValue(key, out value)) {
                if (value != null && typeof(T).IsAssignableFrom(value.GetType())) {
                    return (T)value;
                }
            }
            return defaultValue;
        }
    }
}
