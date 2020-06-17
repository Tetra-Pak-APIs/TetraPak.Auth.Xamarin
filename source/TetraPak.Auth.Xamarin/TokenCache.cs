using System;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;
using Xamarin.Essentials;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   A basic token cache that stores instances of <see cref="AuthResult"/>.
    /// </summary>
    public class TokenCache : MemoryCache<AuthResult>
    {
        readonly bool _isRefreshTokenPersisted;

        /// <summary>
        ///   Adds tokens (in an <see cref="AuthResult"/> object) to the cache.
        /// </summary>
        /// <param name="key">
        ///   Identifies the tokens (typically a client/app id).
        /// </param>
        /// <param name="tokens">
        ///   An <see cref="AuthResult"/> object containing the tokens.
        /// </param>
        /// <param name="expires">
        ///   (optional)<br/>
        ///   A time when the tokens expire. Please note that <see cref="AuthResult.Expires"/>
        ///   governs expiration of the access token themselves. This value simply controls
        ///   how long the result is being cached. Leave unassigned unless all tokens should expire at the
        ///   same time.
        /// </param>
        public override async Task AddAsync(string key, AuthResult tokens, DateTime? expires = null)
        {
            key = key ?? DefaultKey;
            tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            if (_isRefreshTokenPersisted)
            {
                await SecureStorage.SetAsync(key, tokens.RefreshToken);
            }

            await Task.FromResult(base.AddAsync(key, tokens, expires));
        }

        /// <summary>
        ///   Attempts fetching all tokens from the cache.
        /// </summary>
        /// <param name="key">
        ///   Identifies the tokens (typically a client/app id).
        /// </param>
        /// <returns>
        ///   An <see cref="AuthResult"/> item.
        /// </returns>
        public override async Task<BoolValue<AuthResult>> TryGetAsync(string key = null)
        {
            key = key ?? DefaultKey;
            var cached = await base.TryGetAsync(key);
            if (cached || !_isRefreshTokenPersisted)
                return cached;

            // look for persisted refresh token ...
            var refreshToken = await SecureStorage.GetAsync(key);
            return refreshToken is null
                ? cached
                : BoolValue<AuthResult>.Success(new AuthResult(cached.Value?.Tokens));
        }

        /// <summary>
        ///   Removes any access token from cache.
        /// </summary>
        /// <param name="key">
        ///   Identifies the token (typically a client/app id).
        /// </param>
        public async Task RemoveAccessTokenAsync(string key = null)
        {
            await base.RemoveAsync(key ?? DefaultKey);
        }

        /// <summary>
        ///   Removes any refresh token from cache.
        /// </summary>
        /// <param name="key">
        ///   Identifies the token (typically a client/app id).
        /// </param>
        public async Task RemoveRefreshTokenAsync(string key = null)
        {
            key = key ?? DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (await SecureStorage.GetAsync(key) != null)
            {
                SecureStorage.Remove(key);
            }
        }

        /// <summary>
        ///   Removes all tokens from cache.
        /// </summary>
        /// <param name="key">
        ///   Identifies the tokens (typically a client/app id).
        /// </param>
        public override async Task RemoveAsync(string key = null)
        {
            key = key ?? DefaultKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            await RemoveRefreshTokenAsync(key);
            await RemoveAccessTokenAsync(key);
        }

        /// <summary>
        ///   Initializes the token cache.
        /// </summary>
        /// <param name="isRefreshTokenPersisted">
        ///   Specifies whether refresh tokens should be persisted.
        /// </param>
        public TokenCache(bool isRefreshTokenPersisted = true)
        {
            _isRefreshTokenPersisted = isRefreshTokenPersisted;
        }
    }
}
