#if DEBUG
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.debugging
{
    // todo maybe(?) consider improving the Auth simulator to truly simulate the actual flow, including flipping to an "external web page"
    // todo consider improving the the Auth simulator to also support retrieving user information
    public class AuthSimulator
    {
        public static bool IsSimulating
        {
            get
            {
#if SIMULATED_AUTH
                    return true;
#else
                    return false;
#endif
            }
        }

        public static TimeSpan AccessTokenLongevity { get; set; }

        public static TimeSpan IdTokenLongevity { get; set; }

        public static async Task<BoolValue<AuthResult>> TryGetSimulatedAccessTokenAsync(AuthConfig config, string cacheKey)
        {
            if (!IsSimulating)
                return BoolValue<AuthResult>.Fail();

            return await GetAccessTokenAsync(config, cacheKey);
        }

        public static async Task<BoolValue<AuthResult>> GetAccessTokenAsync(AuthConfig config, string cacheKey)
        {
            var accessToken = new TokenInfo(new RandomString(), TokenRole.AccessToken, DateTime.Now.Add(AccessTokenLongevity), null);
            var refreshToken = new TokenInfo(new RandomString(), TokenRole.RefreshToken, null, null);
            if (!config.IsRequestingUserId)
                return await config.CacheAsync(new AuthResult(null, null, accessToken, refreshToken), cacheKey);
            
            var idToken = new TokenInfo(simulatedJwtToken(), TokenRole.IdToken, DateTime.Now.Add(IdTokenLongevity), onValidateSimulatedIdToken);
            return await config.CacheAsync(new AuthResult(null, null, accessToken, refreshToken, idToken), cacheKey);
        }

        public static async Task<BoolValue<AuthResult>> TryGetSimulatedRenewedAccessTokenAsync(string refreshToken, AuthConfig config, string cacheKey)
        {
            var canBeRefreshed = await config.TryGetFromRefreshTokenAsync(refreshToken);
            if (!canBeRefreshed)
                return BoolValue<AuthResult>.Fail($"Invalid refresh token");
                
            var accessToken = new TokenInfo(new RandomString(), TokenRole.AccessToken, DateTime.Now.Add(AccessTokenLongevity), null);
            return await config.CacheAsync(new AuthResult(null, null, accessToken), cacheKey);
        }

        static string simulatedJwtToken()
        {
            var key = new RandomString();
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("name", "Simulated-Client"),
                new Claim("email", "simulated@client.net")
            };
            var jwtSecurityToken = new JwtSecurityToken(
                typeof(AuthSimulator).FullName,
                Assembly.GetEntryAssembly()?.FullName ?? "simulated-client",
                claims,
                null,
                DateTime.Now.Add(IdTokenLongevity),
                credentials);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.WriteToken(jwtSecurityToken);
            return jwtToken;
        }

        static Task<BoolValue<string>> onValidateSimulatedIdToken(string token) => Task.FromResult(BoolValue<string>.Success(token));

        static AuthSimulator()
        {
            AccessTokenLongevity = TimeSpan.FromMinutes(5);
            IdTokenLongevity = AccessTokenLongevity;
        }
    }

    static class AuthConfigExtensions
    {
        static readonly Dictionary<string,AuthResult> s_authResults = new Dictionary<string, AuthResult>();

        public static async Task<BoolValue<AuthResult>> CacheAsync(this AuthConfig config, AuthResult authResult, string cacheKey)
        {
            if (config.IsCaching)
            {
                await config.TokenCache.AddAsync(cacheKey, authResult);
            }

            var refreshToken = authResult.Tokens.FirstOrDefault(i => i.Role == TokenRole.RefreshToken);
            if (refreshToken != null)
                s_authResults.Add(refreshToken.TokenValue, authResult);
            
            return BoolValue<AuthResult>.Success(authResult);
        }
        
        public static Task<BoolValue<AuthResult>> TryGetFromRefreshTokenAsync(this AuthConfig config, string refreshToken)
        {
            if (!s_authResults.TryGetValue(refreshToken, out var authResult))
                return Task.FromResult<BoolValue<AuthResult>>(BoolValue<AuthResult>.Fail());
            
            s_authResults.Remove(refreshToken);
            return Task.FromResult(BoolValue<AuthResult>.Success(authResult));
        }
    }
}
#endif