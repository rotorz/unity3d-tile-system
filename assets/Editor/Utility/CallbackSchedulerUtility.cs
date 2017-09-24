// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rotorz.Tile.Editor
{
    [InitializeOnLoad]
    internal static class CallbackSchedulerUtility
    {
        private const double DelayBetweenScheduleChecksInSeconds = 0.1;
        private static double s_TimeOfLastScheduleCheck;
        private static Queue<int> s_ExecuteScheduleActionIDs = new Queue<int>();

        static CallbackSchedulerUtility()
        {
            EditorApplication.update += ProcessScheduledActions;
        }


        #region Id Management

        private static Stack<int> s_AvailableIDs = new Stack<int>();
        private static HashSet<int> s_BorrowedIDs = new HashSet<int>();
        private static int s_NextFreshID = 1;

        private static int GetNextID()
        {
            int nextID = s_AvailableIDs.Count != 0
                ? s_AvailableIDs.Pop()
                : s_NextFreshID++;

            s_BorrowedIDs.Add(nextID);

            return nextID;
        }

        private static void ReturnID(int id)
        {
            if (s_BorrowedIDs.Remove(id)) {
                s_AvailableIDs.Push(id);
            }
            else {
                Debug.LogWarning("Attempting to recycle an ID that is not actually being used!");
            }
        }

        #endregion


        #region Schedule

        private struct ScheduleAction
        {
            public Action Action;
            public double StartTimeStamp;
            public double IntervalInSeconds;
            public bool ExecuteOnce;
        }

        private static Dictionary<int, ScheduleAction> s_Schedule = new Dictionary<int, ScheduleAction>();

        private static int Schedule(Action action, double intervalInSeconds, bool executeOnce)
        {
            int id = GetNextID();

            ScheduleAction scheduleAction;
            scheduleAction.Action = action;
            scheduleAction.StartTimeStamp = EditorApplication.timeSinceStartup;
            scheduleAction.IntervalInSeconds = intervalInSeconds;
            scheduleAction.ExecuteOnce = executeOnce;

            s_Schedule[id] = scheduleAction;

            return id;
        }

        private static void ExecuteScheduledAction(int id)
        {
            var scheduleAction = s_Schedule[id];

            if (scheduleAction.ExecuteOnce) {
                Unschedule(id);
            }
            else {
                scheduleAction.StartTimeStamp = EditorApplication.timeSinceStartup;
                s_Schedule[id] = scheduleAction;
            }

            scheduleAction.Action();
        }

        private static void ProcessScheduledActions()
        {
            double timeStampNow = EditorApplication.timeSinceStartup;

            // Avoid excessively checking scheduled actions.
            if (timeStampNow - s_TimeOfLastScheduleCheck < DelayBetweenScheduleChecksInSeconds) {
                return;
            }
            s_TimeOfLastScheduleCheck = timeStampNow;

            // Queue each of the scheduled actions that are to be executed now to avoid
            // attempting to modify the schedule collection whilst enumerating it!
            s_ExecuteScheduleActionIDs.Clear();
            foreach (var entry in s_Schedule) {
                if (timeStampNow - entry.Value.StartTimeStamp >= entry.Value.IntervalInSeconds) {
                    s_ExecuteScheduleActionIDs.Enqueue(entry.Key);
                }
            }

            while (s_ExecuteScheduleActionIDs.Count != 0) {
                ExecuteScheduledAction(s_ExecuteScheduleActionIDs.Dequeue());
            }
        }

        #endregion


        public static int SetTimeout(Action action, double delayInSeconds)
        {
            return Schedule(action, delayInSeconds, true);
        }

        public static int SetInterval(Action action, double intervalInSeconds)
        {
            return Schedule(action, intervalInSeconds, false);
        }

        public static void Unschedule(int id)
        {
            if (s_Schedule.Remove(id)) {
                ReturnID(id);
            }
        }
    }
}
