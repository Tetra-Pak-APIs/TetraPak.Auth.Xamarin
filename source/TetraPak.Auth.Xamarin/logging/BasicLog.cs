using System;

namespace TetraPak.Auth.Xamarin.logging
{
    /// <summary>
    ///   Provides a very basic <see cref="ILog"/> implementation.
    /// </summary>
    /// <remarks>
    ///   Invoking the different log methods of this implementation simply
    ///   triggers the <see cref="Logged"/> event. This can be utilized to
    ///   dispatch the actual log entries to various logging solutions.
    /// </remarks>
    public class BasicLog : ILog
    {
        /// <summary>
        ///   This event gets fired for every new log entry added.
        /// </summary>
        public event EventHandler<TextLogEventArgs> Logged;

        /// <summary>
        ///   Logs a new message of <seealso cref="LogRank.Debug"/>.
        /// </summary>
        /// <param name="message">
        ///   The logged message.
        /// </param>
        public void Debug(string message)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Debug, message));
        }

        /// <summary>
        ///   Logs a new message of <seealso cref="LogRank.Error"/>.
        /// </summary>
        /// <param name="exception">
        ///   A logged <see cref="Exception"/>.
        /// </param>
        /// <param name="message">
        ///   The logged message.
        /// </param>
        public void Error(Exception exception, string message = null)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Error, message, exception));
        }

        /// <summary>
        ///   Logs a new message of <seealso cref="LogRank.Info"/>.
        /// </summary>
        /// <param name="message">
        ///   The logged message.
        /// </param>
        public void Info(string message)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Info, message));
        }
    }
}
