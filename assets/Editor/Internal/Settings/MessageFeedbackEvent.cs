// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;

namespace Rotorz.Settings
{
    /// <summary>
    /// Identifies type of message feedback.
    /// </summary>
    internal enum MessageFeedbackType
    {
        /// <summary>
        /// General information.
        /// </summary>
        Information,
        /// <summary>
        /// Warning message.
        /// </summary>
        Warning,
        /// <summary>
        /// Error message.
        /// </summary>
        Error
    }


    /// <summary>
    /// Arguments passed to <see cref="EventHandler{MessageFeedbackEventArgs}"/> when
    /// messages are logged.
    /// </summary>
    internal sealed class MessageFeedbackEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize <see cref="MessageFeedbackEventArgs"/> instance.
        /// </summary>
        /// <param name="feedbackType">Type of message.</param>
        /// <param name="message">Message.</param>
        /// <param name="exception">Associated exception or a value of <c>null</c>
        /// if not applicable.</param>
        public MessageFeedbackEventArgs(MessageFeedbackType feedbackType, string message, Exception exception)
        {
            this.FeedbackType = feedbackType;
            this.Message = message;
            this.Exception = exception;
        }


        /// <summary>
        /// Gets type of message.
        /// </summary>
        public MessageFeedbackType FeedbackType { get; private set; }
        /// <summary>
        /// Gets message.
        /// </summary>
        public string Message { get; private set; }
        /// <summary>
        /// Gets associated exception or a value of <c>null</c> if not applicable.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}
