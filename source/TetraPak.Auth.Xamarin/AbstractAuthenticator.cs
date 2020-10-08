using System;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;
using TetraPak.Auth.Xamarin.logging;

namespace TetraPak.Auth.Xamarin
{
    public abstract class AbstractAuthenticator : IAuthenticator
    {
        /// <summary>
        ///   Gets a logger (if any).
        /// </summary>
        /// <seealso cref="LogInfo"/>
        /// <seealso cref="LogDebug"/>
        /// <seealso cref="LogError"/>
        protected ILog Log { get; }

        /// <summary>
        ///   Gets the configuration.
        /// </summary>
        protected AuthConfig Config { get; }

        /// <summary>
        ///   Gets a default cache key (the client Id).
        /// </summary>
        protected string CacheKey => $"{Config.Authority.Host}::{Config.ClientId}";

        /// <summary>
        ///   Logs a message of <see cref="LogRank.Info"/>.
        /// </summary>
        protected void LogInfo(string message) => Log?.Info(message);

        /// <summary>
        ///   Logs a message of <see cref="LogRank.Debug"/>.
        /// </summary>
        protected void LogDebug(string message) => Log?.Debug(message);

        /// <summary>
        ///   Logs an error an an optional message (of <see cref="LogRank.Error"/>).
        /// </summary>
        protected void LogError(Exception exception, string message) => Log?.Error(exception, message);

        /// <summary>
        ///   Gets a value indicating whether token can be persisted.
        /// </summary>
        /// <seealso cref="AuthConfig.TokenCache"/>
        public bool IsCaching => Config.IsCaching;
        
        /// <inheritdoc />
        public abstract Task<BoolValue<AuthResult>> GetAccessTokenAsync(bool allowCached = true);
        
        /// <inheritdoc />
        public abstract Task<BoolValue<AuthResult>> GetAccessTokenSilentlyAsync();

        /// <summary>
        ///   Initializes the authenticator.
        /// </summary>
        protected AbstractAuthenticator(AuthConfig config, ILog log = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Log = log;
        }
    }
}