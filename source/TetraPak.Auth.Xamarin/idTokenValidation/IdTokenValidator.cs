using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.idTokenValidation
{
    public class IdTokenValidator
    {
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        
        public async Task<BoolValue<ClaimsPrincipal>> ValidateAsync(string idToken, JwtTokenValidationOptions options = null)
        {
            try
            {
                var jwtSecurityToken = new JwtSecurityToken(idToken);
                var discoveryEndpoint = getDiscoveryEndpoint(jwtSecurityToken);
                var discoveryDocument = await DiscoveryDocument.DownloadAsync(discoveryEndpoint.Url);
                if (!discoveryDocument)
                    return BoolValue<ClaimsPrincipal>.Fail(discoveryDocument.Message, discoveryDocument.Exception);

                var jwks = await JsonWebKeySet.DownloadAsync(discoveryDocument.Value.JwksUri);
                if (!jwks)
                    return BoolValue<ClaimsPrincipal>.Fail(jwks.Message, jwks.Exception);

                options ??= new JwtTokenValidationOptions();
                var parameters = options.ToTokenValidationParameters(jwtSecurityToken, discoveryDocument.Value, jwks.Value);
                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();
                var user = handler.ValidateToken(idToken, parameters, out _);
                return BoolValue<ClaimsPrincipal>.Success(user);
            }
            catch (Exception ex)
            {
                return BoolValue<ClaimsPrincipal>.Fail(ex.Message, ex);
            }
        }

        DiscoveryEndpoint getDiscoveryEndpoint(SecurityToken jwtSecurityToken)
        {
            var disco = DiscoveryEndpoint.ParseUrl(jwtSecurityToken.Issuer);
            return validate(disco, jwtSecurityToken.Issuer);
        }

        DiscoveryEndpoint validate(DiscoveryEndpoint discoveryEndpoint, string issuer)
        {
            if (!DiscoveryPolicy.ValidateIssuerName) 
                return discoveryEndpoint;
            
            if (DiscoveryPolicy.RequireHttps && !DiscoveryEndpoint.IsSecureScheme(new Uri(discoveryEndpoint.Url), DiscoveryPolicy))
                throw new InvalidOperationException($"Error connecting to {discoveryEndpoint.Url}. HTTPS required.");

            var strategy = DiscoveryPolicy.AuthorityValidationStrategy ?? DiscoveryPolicy.DefaultAuthorityValidationStrategy;
            var issuerValidationResult = strategy.IsIssuerNameValid(issuer, discoveryEndpoint.Authority);
            if (!issuerValidationResult.Success)
                throw  new InvalidOperationException($"Error connecting to {discoveryEndpoint.Url}. {issuerValidationResult.ErrorMessage}.");

            return discoveryEndpoint;
        }

        public IdTokenValidator() : this(new DiscoveryPolicy())
        {
        }
        
        public IdTokenValidator(DiscoveryPolicy discoveryPolicy)
        {
            DiscoveryPolicy = discoveryPolicy;
        }
    }
}