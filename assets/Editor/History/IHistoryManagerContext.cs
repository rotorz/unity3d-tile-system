// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// Context of a history manager instance.
    /// </summary>
    public interface IHistoryManagerContext
    {
        /// <summary>
        /// Finalize current state ready for history.
        /// </summary>
        /// <remarks>
        /// <para>Return a value of <c>null</c> if there is no current state.</para>
        /// </remarks>
        /// <returns>
        /// Current state.
        /// </returns>
        HistoryManager.State UpdateCurrentState();


        /// <summary>
        /// Invoked upon navigating back one state in history.
        /// </summary>
        /// <param name="state">New histoty state.</param>
        void OnNavigateBack(HistoryManager.State state);

        /// <summary>
        /// Invoked upon navigating forward one state in history.
        /// </summary>
        /// <param name="state">New histoty state.</param>
        void OnNavigateForward(HistoryManager.State state);
    }
}
