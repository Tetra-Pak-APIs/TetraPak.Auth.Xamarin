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
        ///   A collection of tokens (represented as <see cref="TokenInfo"/> objects) returned from the issuer.
        /// </summary>
        public TokenInfo[] Tokens { get; }

        /// <summary>
        ///   Gets the access token when successful.
        /// </summary>
        public string AccessToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.TokenValue;

        /// <summary>
        ///   Gets an optional refresh token when successful.
        /// </summary>
        public string RefreshToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.RefreshToken)?.TokenValue;

        /// <summary>
        ///   Gets an optional identity token when successful.
        /// </summary>
        public string IdToken => Tokens?.FirstOrDefault(i => i.Role == TokenRole.IdToken)?.TokenValue;

        /// <summary>
        ///   Gets any provided expiration time when successful.
        /// </summary>
        public DateTime? Expires => Tokens?.FirstOrDefault(i => i.Role == TokenRole.AccessToken)?.Expires;

        /// <summary>
        ///   Indicates whether the authorization is (still) valid.
        /// </summary>
        [Obsolete("The IsValid property should no longer be used. Please use the TokenInfo.IsValidAsync() method instead")]
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
        
        internal AuthResult(params TokenInfo[] tokens)
        {
            Tokens = tokens;
        }
    }

    /// <summary>
    ///   Carries an individual token and its meta data.
    /// </summary>
    public class TokenInfo
    {
        readonly ValidateTokenDelegate _validateTokenDelegate;
        bool _isValidatedByDelegate;

        /// <summary>
        ///   Gets the actual token as a <see cref="string"/> value.
        /// </summary>
        public string TokenValue { get; }

        /// <summary>
        ///   Gets the token role (see <see cref="TokenRole"/>).
        /// </summary>
        public TokenRole Role { get; }

        /// <summary>
        ///   Gets a expiration date/time, if available.
        /// </summary>
        public DateTime? Expires { get; }

        /// <summary>
        ///   Gets a value that indicates whether the token can be validated (other than just by its longevity).
        /// </summary>
        public bool IsValidatable => _validateTokenDelegate != null;

        /// <summary>
        ///   Validates the token and returns a value to indicate whether it is valid at this point. 
        /// </summary>
        public async Task<bool> IsValidAsync()
        {
            if (isTokenExpired())
                return false;

            if (_validateTokenDelegate is null || _isValidatedByDelegate)
                return true;

            var isValid = await _validateTokenDelegate(TokenValue);
            _isValidatedByDelegate = true;
            return isValid;
        }

        bool isTokenExpired() => Expires.HasValue && Expires.Value <= DateTime.Now;

        internal TokenInfo(string tokenValue, TokenRole role, DateTime? expires, ValidateTokenDelegate validateTokenDelegate)
        {
            TokenValue = tokenValue;
            Role = role;
            Expires = expires;
            _validateTokenDelegate = validateTokenDelegate;
        }
    }
    
    delegate Task<BoolValue<string>> ValidateTokenDelegate(string token);

}