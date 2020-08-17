using System;
using System.Linq;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;

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
        ///   Gets an optional id token when successful.
        /// </summary>
        public string IdToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.IdToken)?.Token;

        /// <summary>
        ///   Gets any provided expiration time when successful.
        /// </summary>
        public DateTime? Expires => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Expires;

        /// <summary>
        ///   Indicates whether the authorization is (still) valid for all tokens.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Tokens?.All(i => IsValid) ?? true;

                /* obsolete
                var accessToken = Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken);
                if (accessToken is null)
                    return false;

                return !accessToken.Expires.HasValue || accessToken.Expires.Value > DateTime.Now;
                */
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
        readonly string _token;
        readonly ValidateTokenDelegate _validateTokenDelegate;
        readonly bool _isAutoValidating;
        bool _isValid;
        bool _isValidated;

        /// <summary>
        ///   Gets the actual token as a <see cref="string"/> value.
        /// </summary>
        public string Token
        {
            get => validatedTokenFromDelegateAsync().Result;
        }

        /// <summary>
        ///   Gets the token role (see <see cref="TokenRole"/>).
        /// </summary>
        public TokenRole Role { get; }

        /// <summary>
        ///   Gets a expiration date/time, if available.
        /// </summary>
        public DateTime? Expires { get; }

        /// <summary>
        ///   Gets a value that indicates whether the token is currently valid.
        /// </summary>
        public bool IsValid => _validateTokenDelegate is null 
            ? validateOnExpiration() 
            : isValidTokenFromDelegateAsync().Result;

        bool validateOnExpiration()
        {
            if (!Expires.HasValue || DateTime.Now <= Expires.Value) 
                return true;

            _isValid = false;
            _isValidated = true;
            return false;
        }

        async Task<bool> isValidTokenFromDelegateAsync()
        {
            if (_isValidated)
                return _isValid && validateOnExpiration();

            var isValid = await _validateTokenDelegate(_token);
            _isValidated = true;
            _isValid = isValid;
            return _isValid;
        }

        async Task<string> validatedTokenFromDelegateAsync()
        {
            if (!_isAutoValidating)
                return _token;

            await isValidTokenFromDelegateAsync();
            return _token;
        }

        internal TokenResult(string token, TokenRole role, DateTime? expires, bool isAutoValidating)
        {
            _token = token;
            Role = role;
            Expires = expires;
            _isAutoValidating = isAutoValidating;
        }

        internal TokenResult(string token, TokenRole role, DateTime? expires, ValidateTokenDelegate validateTokenDelegate)
        {
            _token = token;
            Role = role;
            Expires = expires;
            _isAutoValidating = validateTokenDelegate != null;
            _validateTokenDelegate = validateTokenDelegate;
        }

    }

    internal delegate Task<BoolValue<string>> ValidateTokenDelegate(string token);
}