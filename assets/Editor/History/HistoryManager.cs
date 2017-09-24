// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rotorz.Tile.Editor
{
    /// <summary>
    /// History manager can be used to implement navigation history or even to implement
    /// a custom undo/redo system.
    /// </summary>
    public sealed class HistoryManager
    {
        /// <summary>
        /// Represents a state in selection history.
        /// </summary>
        /// <example>
        /// <para>The following source code demonstrates how to define a class for
        /// a custom history state:</para>
        /// <code language="csharp"><![CDATA[
        /// public class CustomState : SelectionHistory.State
        /// {
        ///     public bool someCustomData;
        ///
        ///
        ///     public CustomState(IHistoryObject selected)
        ///         : base(selected)
        ///     {
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public class State
        {
            /// <summary>
            /// Initialize new <see cref="State"/> instance for a selected object.
            /// </summary>
            /// <param name="selected">Selected object.</param>
            public State(IHistoryObject selected)
            {
                this.Object = selected;
            }


            /// <summary>
            /// Gets selected object.
            /// </summary>
            public IHistoryObject Object { get; private set; }
        }


        private int maximumHistoryCount = 20;
        private int maximumRecentCount = 10;

        private List<State> backList;
        private List<State> forwardList;
        private List<IHistoryObject> recentList;


        /// <summary>
        /// Gets context of history manager.
        /// </summary>
        public IHistoryManagerContext Context { get; private set; }


        /// <summary>
        /// Gets a value indicating whether history manager is navigating forward
        /// or backward by one state. This can be used to avoid adding existing
        /// state into back list.
        /// </summary>
        public bool IsNavigating { get; private set; }

        /// <summary>
        /// Gets a read-only collection of recently selected objects.
        /// </summary>
        public ReadOnlyCollection<IHistoryObject> Recent { get; private set; }

        /// <summary>
        /// Gets or sets maximum number of states to keep in history.
        /// </summary>
        /// <remarks>
        /// <para>A value of zero indicates that there is no maximum count.</para>
        /// </remarks>
        public int MaximumHistoryCount {
            get { return this.maximumHistoryCount; }
            set {
                if (value != this.maximumHistoryCount) {
                    this.maximumHistoryCount = value;
                    this.ClearExcess();
                }
            }
        }
        /// <summary>
        /// Gets or sets maximum number of recent entries.
        /// </summary>
        /// <remarks>
        /// <para>A value of zero indicates that there is no maximum count.</para>
        /// </remarks>
        public int MaximumRecentCount {
            get { return this.maximumRecentCount; }
            set {
                if (value != this.maximumRecentCount) {
                    this.maximumRecentCount = value;
                    this.ClearExcess();
                }
            }
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="HistoryManager"/> class.
        /// </summary>
        /// <param name="context">Context of history manager.</param>
        internal HistoryManager(IHistoryManagerContext context)
        {
            this.Context = context;

            this.backList = new List<State>(this.MaximumHistoryCount + 1);
            this.forwardList = new List<State>(this.MaximumHistoryCount + 1);
            this.recentList = new List<IHistoryObject>(this.MaximumRecentCount + 1);
            this.Recent = new ReadOnlyCollection<IHistoryObject>(this.recentList);
        }

        /// <summary>
        /// Clear excess states from history when maximum limits have been reached.
        /// </summary>
        private void ClearExcess()
        {
            if (this.MaximumHistoryCount == 0) {
                return;
            }

            if (this.backList.Count > this.MaximumHistoryCount) {
                this.backList.RemoveRange(0, this.backList.Count - this.MaximumHistoryCount);
            }
            if (this.forwardList.Count > this.MaximumHistoryCount) {
                this.forwardList.RemoveRange(0, this.forwardList.Count - this.MaximumHistoryCount);
            }
        }

        /// <summary>
        /// Add object to recently accessed list.
        /// </summary>
        /// <param name="recent">Recent object.</param>
        public void AddToRecent(IHistoryObject recent)
        {
            if (this.recentList.Count > 0 && ReferenceEquals(this.recentList[0], recent)) {
                return;
            }

            // Remove selection from recent history list to ensure that it is not listed
            // multiple times. Place at top of list for most recent history.
            this.recentList.Remove(recent);
            this.recentList.Insert(0, recent);

            // Limit number of entries to 10.
            if (this.recentList.Count > this.MaximumRecentCount) {
                this.recentList.RemoveRange(this.MaximumRecentCount, this.recentList.Count - this.MaximumRecentCount);
            }
        }

        /// <summary>
        /// Advance to next state in history.
        /// </summary>
        /// <remarks>
        /// <para>Will not advance to next state in history when <see cref="IsNavigating"/>
        /// is <c>true</c> since previous navigation has not yet completed. This is a
        /// safety mechanism to avoid infinite loops.</para>
        /// </remarks>
        public void Advance()
        {
            // Do not attempt to advance whilst lock to prevent adding back/forward state
            // into the back list!
            if (this.IsNavigating) {
                return;
            }

            State state = this.Context.UpdateCurrentState();
            if (state != null) {
                this.backList.Add(state);
                this.forwardList.Clear();

                // Remove entries that have become invalid.
                this.Cleanup();

                this.ClearExcess();

                this.AddToRecent(state.Object);
            }
        }


        /// <summary>
        /// Gets the next back state.
        /// </summary>
        /// <remarks>
        /// <para>Gets a value of <c>null</c> when <see cref="CanGoBack"/> is <c>false</c>.</para>
        /// </remarks>
        /// <seealso cref="CanGoBack"/>
        public State PeekBack {
            get { return this.CanGoBack ? this.backList.Last() : null; }
        }

        /// <summary>
        /// Gets the next forward state.
        /// </summary>
        /// <remarks>
        /// <para>Gets a value of <c>null</c> when <see cref="CanGoForward"/> is <c>false</c>.</para>
        /// </remarks>
        /// <seealso cref="CanGoForward"/>
        public State PeekForward {
            get { return this.CanGoForward ? this.forwardList[0] : null; }
        }

        /// <summary>
        /// Gets a value indicating whether it is possible to navigate backward by
        /// at least one state in history.
        /// </summary>
        /// <seealso cref="PeekBack"/>
        public bool CanGoBack {
            get { return this.backList.Count > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether it is possible to navigate forward by
        /// at least one state in history.
        /// </summary>
        /// <seealso cref="PeekForward"/>
        public bool CanGoForward {
            get { return this.forwardList.Count > 0; }
        }


        /// <summary>
        /// Go back to most recent state in navigation history.
        /// </summary>
        /// <remarks>
        /// <para>Nothing happens when there are no states.</para>
        /// </remarks>
        public void GoBack()
        {
            this.Cleanup();

            if (!this.CanGoBack) {
                return;
            }

            this.IsNavigating = true;
            try {
                // Update current state and add to forward list.
                State current = this.Context.UpdateCurrentState();
                if (current != null) {
                    this.forwardList.Insert(0, current);
                }

                current = this.backList.Last();
                this.backList.RemoveAt(this.backList.Count - 1);

                this.Context.OnNavigateBack(current);
            }
            finally {
                this.IsNavigating = false;
            }
        }

        /// <summary>
        /// Go forward to most recent state in navigation history.
        /// </summary>
        /// <remarks>
        /// <para>Nothing happens when there are no states.</para>
        /// </remarks>
        public void GoForward()
        {
            this.Cleanup();

            if (!this.CanGoForward) {
                return;
            }

            this.IsNavigating = true;
            try {
                // Update current state and add to back list.
                State current = this.Context.UpdateCurrentState();
                if (current != null) {
                    this.backList.Add(current);
                }

                current = this.forwardList[0];
                this.forwardList.RemoveAt(0);

                this.Context.OnNavigateForward(current);
            }
            finally {
                this.IsNavigating = false;
            }
        }

        /// <summary>
        /// Removes references to objects that have been destroyed.
        /// </summary>
        public void Cleanup()
        {
            this.backList.RemoveAll(state => !state.Object.Exists);
            this.forwardList.RemoveAll(state => !state.Object.Exists);
            this.recentList.RemoveAll(state => !state.Exists);
        }

        /// <summary>
        /// Clear all recent history.
        /// </summary>
        public void Clear()
        {
            this.backList.Clear();
            this.forwardList.Clear();
            this.recentList.Clear();
        }
    }
}
