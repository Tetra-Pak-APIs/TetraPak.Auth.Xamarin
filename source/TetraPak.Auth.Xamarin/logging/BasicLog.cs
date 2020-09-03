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
        /// <inheritdoc />
        public event EventHandler<TextLogEventArgs> Logged;

        /// <inheritdoc />
        public QueryAsyncDelegate QueryAsync { get; set; }

        /// <inheritdoc />
        public void Debug(string message)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Debug, message));
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Warning, message));
        }

        /// <inheritdoc />
        public void Error(Exception exception, string message = null)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Error, message, exception));
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            Logged?.Invoke(this, new TextLogEventArgs(LogRank.Info, message));
        }
    }
}
