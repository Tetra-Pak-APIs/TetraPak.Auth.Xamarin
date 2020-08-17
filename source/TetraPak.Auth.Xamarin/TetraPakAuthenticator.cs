using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using TetraPak.Auth.Xamarin;
using TetraPak.Auth.Xamarin.common;
using TetraPak.Auth.Xamarin.idTokenValidation;
using TetraPak.Auth.Xamarin.logging;

[assembly: Dependency(typeof(TetraPakAuthenticator))]
[assembly: Dependency(typeof(TetraPakAuthCallbackHandler))]

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   A custom (Tetra Pak) implementation of the <see cref="IAuthenticator"/> contract.
    /// </summary>
    internal class TetraPakAuthenticator : AbstractAuthenticator
    {
        readonly TaskCompletionSource<BoolValue<Uri>> _authCodeTcs = new TaskCompletionSource<BoolValue<Uri>>();

        /// <inheritdoc />
        public override async Task<BoolValue<AuthResult>> GetAccessTokenAsync(bool allowCached = true)
        {
            if (allowCached)
            {
                var cached = await tryGetCachedAuthResultAsync();
                if (cached && cached.Value.AccessToken != null)
                    return cached;
            }
            try
            {
                LogDebug("---- START - Tetra Pak Code Grant Flow ----");
                return await acquireTokenAsyncUsingNativeWebUI();
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                LogDebug("---- END - Tetra Pak Code Grant Flow ----");
                return BoolValue<AuthResult>.Fail("Could not acquire an access token", ex);
            }
        }

        /// <inheritdoc />
        public override async Task<BoolValue<AuthResult>> GetAccessTokenSilentlyAsync()
        {
            if (!IsCaching)
                return await GetAccessTokenAsync();

            var cached = await tryGetCachedAuthResultAsync();
            if (!cached)
                return await GetAccessTokenAsync(false);

            if (cached.Value.AccessToken != null)
            {
                if (!cached.Value.Expires.HasValue || DateTime.Now < cached.Value.Expires.Value)
                    return BoolValue<AuthResult>.Success(cached.Value);
            }

            await removeFromCacheAsync();
            if (string.IsNullOrEmpty(cached.Value.RefreshToken))
                return await GetAccessTokenAsync();

            // access token has expired, try renew from refresh token is available ...
            LogDebug("---- START - Tetra Pak Refresh Token Flow ----");
            BoolValue<AuthResult> result;
            try
            {
                result = await acquireRenewedAccessTokenAsync(cached.Value.RefreshToken);
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message);
                return BoolValue<AuthResult>.Fail("Could not renew access token", ex);
            }
            finally
            {
                LogDebug("---- END - Tetra Pak Refresh Token Flow ----");
            }

            return result ? result : await GetAccessTokenAsync();
        }


        async Task<BoolValue<AuthResult>> acquireTokenAsyncUsingNativeWebUI()
        {
            LogDebug($"Listens for callbacks on {Config.RedirectUri} ...");

            var authAppDelegate = Authorization.GetAuthorizingAppDelegate();
            if (authAppDelegate is null)
                return BoolValue<AuthResult>.Fail($"Cannot obtain a {typeof(IAuthorizingAppDelegate)}.");

            var callbackHandler = (TetraPakAuthCallbackHandler)DependencyService.Get<IAuthCallbackHandler>();
            callbackHandler.NotifyUriCallback(onUriCallback);

            // make the call for auth code and await callback from redirect ...
            var authState = new AuthState(Config.IsStateUsed, Config.IsPkceUsed, Config.ClientId);
            var authRequest = buildAuthRequest(authState);
            
            LogDebug(authRequest);
            
            await authAppDelegate.OpenInDefaultBrowserAsync(new Uri(authRequest), Config.RedirectUri);
            var callback = await _authCodeTcs.Task.ConfigureAwait(false);

            LogDebug(callback.Value.ToString());

            // check the PKCE and get the access code ...
            var authCode = callback.Value.TryGetQueryValue("code").Value;
            var inState = callback.Value.TryGetQueryValue("state").Value;
            if (authState.IsUsed && inState != authState.State)
                return BoolValue<AuthResult>.Fail($"Returned state was invalid: \"{inState}\". Expected state: \"{authState.State}\"");

            return await getAccessCode(authCode, authState);

            void onUriCallback(Uri uri, out bool isHandled)
            {
                if (!uri.Scheme.Equals(Config.RedirectUri.Scheme) || !uri.Authority.Equals(Config.RedirectUri.Authority))
                {
                    isHandled = false;
                    return;
                }
                isHandled = true;
                _authCodeTcs.SetResult(BoolValue<Uri>.Success(uri));
            }
        }

        async Task<BoolValue<AuthResult>> getAccessCode(string authCode, AuthState authState)
        {
            var body = buildTokenRequestBody(authCode, authState);
            var uri = Config.TokenIssuer.AbsoluteUri;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var bodyData = Encoding.ASCII.GetBytes(body);
            request.ContentLength = bodyData.Length;
            var stream = request.GetRequestStream();
            await stream.WriteAsync(bodyData, 0, bodyData.Length);
            stream.Close();

            Log?.DebugWebRequest(request, body);
            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream() 
                             ?? throw new Exception("Unexpected error: No response when requesting token.");
                
                using (var r = new StreamReader(responseStream))
                {
                    var text = await r.ReadToEndAsync();
                    Log?.DebugWebResponse(response as HttpWebResponse, text);
                    return await buildAuthResultAsync(text);
                }
            }
            catch (WebException webException)
            {
                var response = (HttpWebResponse)webException.Response;
                return BoolValue<AuthResult>.Fail($"Server error: {(int)response.StatusCode} {response.StatusCode.ToString().SplitCamelCase()}");
            }
            catch (Exception ex)
            {
                return BoolValue<AuthResult>.Fail($"Unexpected Server error: {ex}");
            }
        }

        async Task<BoolValue<AuthResult>> acquireRenewedAccessTokenAsync(string refreshToken)
        {
            var body = makeRefreshTokenBody(refreshToken, Config.IsPkceUsed);
            var uri = Config.TokenIssuer;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var bodyData = Encoding.ASCII.GetBytes(body);
            request.ContentLength = bodyData.Length;
            var stream = request.GetRequestStream();
            await stream.WriteAsync(bodyData, 0, bodyData.Length);
            stream.Close();

            Log?.DebugWebRequest(request, body);
            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream() 
                                     ?? throw new Exception("Unexpected error: No response when requesting token.");
                
                using (var r = new StreamReader(responseStream))
                {
                    var text = await r.ReadToEndAsync();
                    Log.DebugWebResponse(response as HttpWebResponse, text);
                    return await buildAuthResultAsync(text);
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Failed request");
                return BoolValue<AuthResult>.Fail("Could not get a valid access token.", ex);
            }
        }

        string makeRefreshTokenBody(string refreshToken, bool includeClientId)
        {
            var sb = new StringBuilder();
            sb.Append("grant_type=refresh_token");
            sb.Append($"&refresh_token={refreshToken}");
            if (includeClientId)
            {
                sb.Append($"&client_id={Config.ClientId}");
            }
            return sb.ToString();
        }

        static async Task<BoolValue<string>> validateIdTokenAsync(string idToken)
        {
            var validator = new IdTokenValidator();
            var validated = await validator.ValidateAsync(idToken);
            return validated 
                ? BoolValue<string>.Success(idToken) 
                : BoolValue<string>.Fail(validated.Message, validated.Exception);
        }

        async Task<BoolValue<AuthResult>> buildAuthResultAsync(string responseText)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseText);
            if (!dict.TryGetValue("access_token", out var accessToken))
                return BoolValue<AuthResult>.Fail("Could not get a valid access token.");

            var tokens = new List<TokenResult>();
            var expires = dict.TryGetValue("expires_in", out var exp) && int.TryParse(exp, out var seconds)
                ? DateTime.Now.AddSeconds(seconds - 4)
                : (DateTime?)null;
            
            tokens.Add(new TokenResult(accessToken, TokenRole.AccessToken, expires, null));

            if (dict.TryGetValue("refresh_token", out var refreshToken))
            {
                tokens.Add(new TokenResult(refreshToken, TokenRole.RefreshToken, null, null));
            }

            if (!dict.TryGetValue("id_token", out var idToken)) 
                return await cacheAuthResultAsync(BoolValue<AuthResult>.Success(new AuthResult(tokens.ToArray())));

            /* obsolete
            var isIdTokenValidated = false;
            if (Config.IsAutoValidatingIdToken)
            {
                isIdTokenValidated = true;
                var idTokenValid = await validateIdTokenAsync(idToken);
                if (!idTokenValid)
                    return BoolValue<AuthResult>.Fail(idTokenValid.Message, idTokenValid.Exception);
            }
            */
            var validateTokenDelegate = Config.IsAutoValidatingTokens 
                ? validateIdTokenAsync 
                : (ValidateTokenDelegate) null;
            tokens.Add(new TokenResult(idToken, TokenRole.IdToken, null, validateTokenDelegate));
            return await cacheAuthResultAsync(BoolValue<AuthResult>.Success(new AuthResult(tokens.ToArray())));
        }
        
        async Task<BoolValue<AuthResult>> tryGetCachedAuthResultAsync()
        {
            if (!IsCaching)
                return BoolValue<AuthResult>.Fail();

            var cached = await Config.TokenCache.TryGetAsync(CacheKey);
            return cached ? cached : BoolValue<AuthResult>.Fail();
        }

        async Task<BoolValue<AuthResult>> cacheAuthResultAsync(BoolValue<AuthResult> authResult)
        {
            if (!IsCaching)
                return authResult;

            await Config.TokenCache.AddAsync(CacheKey, authResult.Value);
            return authResult;
        }

        async Task removeFromCacheAsync()
        {
            await Config.TokenCache.RemoveAsync(CacheKey);
        }

        string buildTokenRequestBody(string authCode, AuthState authState)
        {
            var sb = new StringBuilder();
            sb.Append("grant_type=authorization_code");
            sb.Append($"&code={authCode}");
            sb.Append($"&client_id={Config.ClientId}");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(Config.RedirectUri.AbsoluteUri)}");
            if (authState.Verifier != null)
                sb.Append($"&code_verifier={authState.Verifier}");

            return sb.ToString();
        }

        string buildAuthRequest(AuthState authState)
        {
            var sb = new StringBuilder();
            sb.Append($"{Config.Authority.AbsoluteUri}?response_type=code");
            sb.Append($"&redirect_uri={Uri.EscapeDataString(Config.RedirectUri.AbsoluteUri)}");
            sb.Append($"&client_id={Config.ClientId.Trim()}");

            if (Config.IsRequestingIdToken)
                Config.WithScope("openid");
                
            if (!string.IsNullOrEmpty(Config.Scope))
                sb.Append($"&scope={Config.Scope}");

            // state ...
            if (!authState.IsUsed)
                return sb.ToString();

            sb.Append($"&state={HttpUtility.UrlEncode(authState.State)}");
            if (!authState.IsPKCEUsed)
                return sb.ToString();

            sb.Append($"&code_challenge={authState.CodeChallenge}");
            sb.Append($"&code_challenge_method={authState.CodeChallengeMethod}");
            return sb.ToString();
        }

        /// <summary>
        ///   Initializes the authenticator.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A log to be used for logging internal operations and events-
        /// </param>
        public TetraPakAuthenticator(AuthConfig config, ILog log = null) : base(config, log)
        {
        }
    }
}
