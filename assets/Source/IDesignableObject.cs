// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile
{
    /// <summary>
    /// Represents an object which can be selected in the designer window.
    /// </summary>
    public interface IDesignableObject : IHistoryObject
    {
        /// <summary>
        /// Gets user friendly name of designable object that is displayed in user interfaces.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets user friendly name of designable type.
        /// </summary>
        string DesignableType { get; }
    }
}
