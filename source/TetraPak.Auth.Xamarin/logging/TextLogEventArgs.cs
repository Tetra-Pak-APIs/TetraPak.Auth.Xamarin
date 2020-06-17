using System;

namespace TetraPak.Auth.Xamarin.logging
{
    /// <summary>
    ///   Describes a log entry.
    /// </summary>
    public class TextLogEventArgs : EventArgs
    {
        /// <summary>
        ///   Gets the log entry rank.
        /// </summary>
        public LogRank Rank { get; }

        /// <summary>
        ///   Gets the log entry message (if any).
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///   Gets a log entry error (if any).
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///   Initializes the event arguments.
        /// </summary>
        public TextLogEventArgs(LogRank rank, string message, Exception exception = null)
        {
            Rank = rank;
            Message = message;
            Exception = exception;
        }
    }
}
