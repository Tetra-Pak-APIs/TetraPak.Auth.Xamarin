using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;
using TetraPak.Auth.Xamarin.logging;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   Represents the result of an authorization operation.
    /// </summary>
    public class AuthResult
    {
        readonly ILog _log;
        UserInfo _userInfo;
        UserInfoLoader _userInfoLoader;
        readonly AuthConfig _authConfig;

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

        public async Task<BoolValue<object>> TryGetUserInfo(string type)
        {
            _log?.Debug("[GET USER INFO BEGIN]");

            if (AccessToken is null)
            {
                _log?.Warning("Cannot get user information without a valid access token");
                return BoolValue<object>.Fail();
            }

            var key = type.ToLowerInvariant();
            if (_userInfo != null)
                return _userInfo.TryGet(key, out object value)
                    ? BoolValue<object>.Success(value)
                    : BoolValue<object>.Fail();

            _userInfoLoader ??= new UserInfoLoader(AccessToken, _authConfig, _log);
            _userInfo = await _userInfoLoader.AwaitDownloadedAsync();
            return await TryGetUserInfo(type);
        }

        public async Task<BoolValue<string[]>> TeyGetUserInfoTypes()
        {
            if (AccessToken is null)
            {
                _log?.Warning("Cannot get user information without a valid access token");
                return BoolValue<string[]>.Fail();
            }

            try
            {
                if (_userInfo is { }) 
                    return BoolValue<string[]>.Success(_userInfo.Types);

                _userInfoLoader ??= new UserInfoLoader(AccessToken, _authConfig, _log);
                _userInfo = await _userInfoLoader.AwaitDownloadedAsync();

                return BoolValue<string[]>.Success(_userInfo.Types);
            }
            catch (Exception ex)
            {
                _log?.Error(ex);
                return BoolValue<string[]>.Fail(ex.Message, ex);
            }
        }

        internal AuthResult(AuthConfig config, ILog log, params TokenInfo[] tokens)
        {
            _authConfig = config;
            _log = log;
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

    class UserInfoLoader
    {
        readonly TaskCompletionSource<UserInfo> _tcs;
        readonly string _accessToken;
        readonly ILog _log;

        void downloadAsync(Uri userInfoUri)
        {
            Task.Run(async () =>
            {

                var request = (HttpWebRequest)WebRequest.Create(userInfoUri);
                request.Method = "GET";
                request.Accept = "*/*";
                request.Headers.Add($"Authorization: Bearer {_accessToken}");

                _log?.DebugWebRequest(request, null);

                try
                {
                    var response = await request.GetResponseAsync();
                    var responseStream = response.GetResponseStream()
                                         ?? throw new Exception("Unexpected error: No response when requesting token.");

                    using (var r = new StreamReader(responseStream))
                    {
                        var text = await r.ReadToEndAsync();

                        _log?.DebugWebResponse(response as HttpWebResponse, text);

                        var dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(text);
                        _tcs.SetResult(new UserInfo(dictionary));
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error(ex);
                    _tcs.SetException(ex);
                }
                finally
                {
                    _log?.Debug("[GET USER INFO END]");
                }
            });
        }

        public Task<UserInfo> AwaitDownloadedAsync() => _tcs.Task;

        public UserInfoLoader(string accessToken, AuthConfig config, ILog log)
        {
            _accessToken = accessToken;
            _log = log;
            _tcs = new TaskCompletionSource<UserInfo>();
            downloadAsync(config.UserInfoUri);
        }
    }

    public class UserInfo
    {
        readonly IDictionary<string, object> _dictionary;

        public string[] Types => _dictionary.Keys.ToArray();

        public bool TryGet<T>(string type, out T value)
        {
            var key = type.ToLowerInvariant();
            if (!_dictionary.TryGetValue(key, out var obj))
            {
                value = default;
                return false;
            }
            if (obj is T typedValue)
            {
                value = typedValue;
                return true;
            }

            // todo Cast from Json Token to requested value.
            // todo Also replace Json Token with converted value to avoid converting twice
            throw new NotImplementedException();
        }

        public UserInfo(IDictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }
    }


    public static class UserInfoTypes
    {
        public const string Subject = "sub";
        public const string UserId = Subject;
        public const string Name = "name";
        public const string FamilyName = "family_name";
        public const string Email = "email";
        public const string Domain = "domain";
    }

    delegate Task<BoolValue<string>> ValidateTokenDelegate(string token);

}