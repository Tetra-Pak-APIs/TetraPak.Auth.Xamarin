﻿using System;
using TetraPak.Auth.Xamarin.logging;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   Used to configure a <see cref="TetraPakAuthenticator"/>.
    /// </summary>
    /// <remarks>
    ///   Used to configure a `IAuthenticator` used for acquiring access tokens.
    ///   The <b>TAX</b> system can create this configuration behind the scene if you describe your application
    ///   using the more simplified <see cref="AuthApplication"/> when invoking
    ///   <see cref="Authorization.GetAuthenticator(AuthConfig, ILog)"/>
    ///   but an `AuthConfig` allows for more custom configuration.
    /// </remarks>
    public class AuthConfig
    {
        const string DevelopmentAuthority = "https://api-dev.tetrapak.com/oauth2/authorize";
        const string MigrationAuthority = "https://api-mig.tetrapak.com/oauth2/authorize";
        const string ProductionAuthority = "https://api.tetrapak.com/oauth2/authorize";
        const string DevelopmentTokenIssuer = "https://api-dev.tetrapak.com/oauth2/token";
        const string MigrationTokenIssuer = "https://api-mig.tetrapak.com/oauth2/token";
        const string ProductionTokenIssuer = "https://api.tetrapak.com/oauth2/token";

        Uri _authority;
        Uri _tokenIssuer;
#if DEBUG
        Uri _localAuthority;
        Uri _localTokenIssuer;
#endif
        TokenCache _tokenCache;
        bool _isCaching;
        
        /// <summary>
        ///   Gets a value indicating whether user identity will be requested during the auth flow. 
        /// </summary>
        /// <seealso cref="IsAutoValidatingTokens"/>
        public bool IsRequestingIdToken { get; set; }

        /// <summary>
        ///   Gets or sets a value specifying whether tokens are automatically validated when
        ///   the client accesses them.
        /// </summary>
        /// <remarks>
        ///   When set, and the client attempts reading a token (such as <see cref="AuthResult.AccessToken"/> or
        ///   <see cref="AuthResult.IdToken"/>) the internal logic will ensure the token is being validated
        ///   for authenticity as well as longevity.
        /// </remarks>
        public bool IsAutoValidatingTokens { get; set; }

        /// <summary>
        ///   Gets or sets the <see cref="Uri"/> to the authority endpoint.
        /// </summary>
        public Uri Authority
        {
#if DEBUG
            get => IsTargetingLocalAuthority ? _localAuthority : _authority;
            set
            {
                if (IsTargetingLocalAuthority)
                    _localAuthority = value;
                else
                    _authority = value;
                _authority = value;
            }
#else
            get => _authority;
            set => _authority = value;
#endif
        }

        /// <summary>
        ///   Gets or sets the <see cref="Uri"/> to the token issuing endpoint.
        /// </summary>
        public Uri TokenIssuer
        {
#if DEBUG
            get => IsTargetingLocalAuthority ? _localTokenIssuer : _tokenIssuer;
            set
            {
                if (IsTargetingLocalAuthority)
                    _localTokenIssuer = value;
                else
                    _tokenIssuer = value;
            }
#else
            get => _tokenIssuer;
            set => _tokenIssuer = value;
#endif
        }

        /// <summary>
        ///   Gets or sets a redirect <see cref="Uri"/>, used for passing back an auth code.
        /// </summary>
        public Uri RedirectUri { get; set; }

        /// <summary>
        ///   Gets or sets the client id (a.k.a. "app id").
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        ///   Gets or sets a scope value when applicable.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use state in the auth code flow.
        /// </summary>
        public bool IsStateUsed { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the PKCE extension with the auth code flow.
        /// </summary>
        public bool IsPkceUsed { get; set; }

#if DEBUG
        public bool IsTargetingLocalAuthority { get; set; }
#endif

        /// <summary>
        ///   Gets or sets a value specifying whether authorizations (tokens)
        ///   should be cached. Default value is <c>true</c>.
        /// </summary>
        public bool IsCaching
        {
            get => _isCaching;
            set => setIsCaching(value);
        }

        async void setIsCaching(bool value)
        {
            _isCaching = value;
            if (!_isCaching)
                await _tokenCache.RemoveAsync(ClientId);
        }

        /// <summary>
        ///   Gets or sets a (custom) token cache. This value cannot be unassigned
        ///   and a default token cache will always be provided.
        /// </summary>
        public TokenCache TokenCache
        {
            get => _tokenCache;
            set => _tokenCache = value ?? _tokenCache;
        }

        /// <summary>
        ///   Creates a default OAuth configuration for a specified application.
        /// </summary>
        /// <param name="environment">
        ///   The targeted runtime environment.
        /// </param>
        /// <param name="clientId">
        ///   The application's client id (a.k.a. "app id").
        /// </param>
        /// <param name="redirectUri">
        ///     The application's redirect <see cref="Uri"/>.
        /// </param>
        /// <param name="platform">
        ///   (optional; default = <see cref="RuntimePlatform.Any"/>)<br/>
        ///   An intended runtime platform.
        /// </param>
        /// <returns>
        ///   A <see cref="AuthConfig"/> object with a default configuration for the specified application.
        /// </returns>
        public static AuthConfig Default(RuntimeEnvironment environment, string clientId, Uri redirectUri, RuntimePlatform platform = default)
            => Default(new AuthApplication(clientId, redirectUri, environment, platform));
        
        /// <summary>
        ///   Creates a default OAuth configuration for a specified application.
        /// </summary>
        /// <param name="application">
        ///   Describes the application to be authorized.
        /// </param>
        /// <returns>
        ///   A <see cref="AuthConfig"/> object with a default configuration for the specified application.
        /// </returns>
        public static AuthConfig Default(AuthApplication application)
        {
           return new AuthConfig(
                getAuthority(application.Environment),
                getTokenIssuer(application.Environment),
                application.RedirectUri,
                application.ClientId,
                "",
                true,
                true,
                true);
        }

        static Uri getAuthority(RuntimeEnvironment environment)
        {
            return environment switch
            {
                RuntimeEnvironment.Development => new Uri(DevelopmentAuthority),
                RuntimeEnvironment.Test => throw new NotSupportedException($"Unsupported environment: {environment}"),
                RuntimeEnvironment.Migration => new Uri(MigrationAuthority),
                RuntimeEnvironment.Production => new Uri(ProductionAuthority),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static Uri getTokenIssuer(RuntimeEnvironment environment)
        {
            return environment switch
            {
                RuntimeEnvironment.Development => new Uri(DevelopmentTokenIssuer),
                RuntimeEnvironment.Test => throw new NotSupportedException($"Unsupported environment: {environment}"),
                RuntimeEnvironment.Migration => new Uri(MigrationTokenIssuer),
                RuntimeEnvironment.Production => new Uri(ProductionTokenIssuer),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        ///   Flags the authorization request to also request user identity.
        ///   This is often required by APIs when making requests for data. 
        /// </summary>
        /// <param name="autoValidate">
        ///   Specifies whether the resulting id should automatically by validated.
        /// </param>
        /// <remarks>
        ///   This is mainly a clear-code way to ask for the <c>"openid"</c> scope.
        /// </remarks>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public AuthConfig WithIdToken(bool autoValidate = false)
        {
            IsRequestingIdToken = true;
            IsAutoValidatingTokens = autoValidate;
            return this;
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        ///   Initializes the configuration.
        /// </summary>
        public AuthConfig(
            Uri authority, 
            Uri tokenIssuer, 
            Uri redirectUri, 
            string clientId, 
            string scope = "", 
            bool isStateUsed = true, 
            bool isPkceUsed = true, 
            bool isCaching = true, 
            TokenCache tokenCache = null)
        {
            _tokenCache = tokenCache ?? (TokenCache)new TokenCache().WithDefaultKey(clientId);
            Authority = _authority = authority;
            TokenIssuer = _tokenIssuer = tokenIssuer;
            RedirectUri = redirectUri;
            ClientId = clientId;
            Scope = scope;
            IsStateUsed = isStateUsed;
            IsPkceUsed = isPkceUsed;
            IsCaching = isCaching;
        }
    }
}
