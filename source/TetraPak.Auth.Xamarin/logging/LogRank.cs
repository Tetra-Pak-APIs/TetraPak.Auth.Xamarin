
namespace TetraPak.Auth.Xamarin.logging
{
    /// <summary>
    ///   Used by the (<see cref="ILog"/> based) logging mechanism
    ///   to classify log entries.
    /// </summary>
    public enum LogRank
    {
        /// <summary>
        ///   The lowest (most detailed) log rank.
        /// </summary>
        Debug,

        /// <summary>
        ///   Logs "normal" operations.
        /// </summary>
        Info,

        /// <summary>
        ///   Denotes a logged exception.
        /// </summary>
        Error
    }
}
