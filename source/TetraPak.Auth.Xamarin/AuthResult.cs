using System;
using System.Linq;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   Represents the result of an authorization operation.
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        ///   A collection of tokens returned from the issuer.
        /// </summary>
        public TokenResult[] Tokens { get; }

        /// <summary>
        ///   Gets the access token when successful.
        /// </summary>
        public string AccessToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Token;

        /// <summary>
        ///   Gets an optional refresh token when successful.
        /// </summary>
        public string RefreshToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.RefreshToken)?.Token;

        /// <summary>
        ///   Gets any provided expiration time when successful.
        /// </summary>
        public DateTime? Expires => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Expires;

        /// <summary>
        ///   Indicates whether the authorization is (still) valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                var accessToken = Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken);
                if (accessToken is null)
                    return false;

                return !accessToken.Expires.HasValue || accessToken.Expires.Value > DateTime.Now;
            }
        }
        
        internal AuthResult(params TokenResult[] tokens)
        {
            Tokens = tokens;
        }
    }

    /// <summary>
    ///   Carries an individual token and its meta data.
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        ///   Gets the actual token as a <see cref="string"/> value.
        /// </summary>
        public string Token { get; }

        /// <summary>
        ///   Gets the token role (see <see cref="TokenRole"/>).
        /// </summary>
        public TokenRole Role { get; }

        /// <summary>
        ///   Gets a expiration date/time, if available.
        /// </summary>
        public DateTime? Expires { get; }

        internal TokenResult(string token, TokenRole role, DateTime? expires)
        {
            Token = token;
            Role = role;
            Expires = expires;
        }
    }
}