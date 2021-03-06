﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.Auth.Xamarin.logging
{
    /// <summary>
    ///   Provides a basic logging mechanism to the package.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        ///   Triggered whenever a log entry gets added.
        /// </summary>
        event EventHandler<TextLogEventArgs> Logged;

        /// <summary>
        ///   Gets or sets a delegate used for querying the log.
        /// </summary>
        QueryAsyncDelegate QueryAsync { get; set; }

        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Debug"/>.
        /// </summary>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        void Debug(string message);

        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Info"/>.
        /// </summary>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        void Info(string message);

        /// <summary>
        ///   Adds a message of rank <see cref="LogRank.Warning"/>.
        /// </summary>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        void Warning(string message);

        /// <summary>
        ///   Adds an <see cref="Exception"/> and message of rank <see cref="LogRank.Error"/>.
        /// </summary>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be logged.
        /// </param>
        /// <param name="message">
        ///   A textual message to be logged.
        /// </param>
        void Error(Exception exception, string message = null);
    }

    public delegate Task<IEnumerable<ILogEntry>> QueryAsyncDelegate(params LogRank[] ranks);

    public interface ILogEntry
    {
        LogRank Rank { get; }

        DateTime Time { get; }

        string Message { get; }
    }
}
