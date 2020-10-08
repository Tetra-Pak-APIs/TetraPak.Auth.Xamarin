using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;
using Xamarin.Forms;

namespace TetraPak.Auth.Xamarin.oidc
{
    // ReSharper disable once ClassNeverInstantiated.Global
    /// <summary>
    ///   A discovery document obtained from a well-known OIDC endpoint.  
    /// </summary>
    public class DiscoveryDocument
    {
        const string CacheKey = "taxDiscoveryDocument";

        /// <summary>
        ///   Gets the cached <see cref="DiscoveryDocument"/>, if available.
        /// </summary>
        /// <seealso cref="DownloadAsync"/>
        public static DiscoveryDocument Current { get; private set; }

        /// <summary>
        ///   Gets a value indicating when the document was last updated from the discovery endpoint.
        ///   The time is given as UTC.
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        ///   Get the token issuer host base URL. 
        /// </summary>
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        /// <summary>
        ///   Endpoint that validates all authorization requests.
        /// </summary>
        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }        
        
        /// <summary>
        ///   URL of the token endpoint. After a client has received an authorization code,
        ///   that code is presented to the token endpoint and exchanged for an identity token,
        ///   an access token, and a refresh token.
        /// </summary>
        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; }        
         
        /// <summary>
        ///   URL of the user information endpoint. 
        /// </summary>
        [JsonPropertyName("userinfo_endpoint")]
        public string UserInformationEndpoint { get; set; }        

        /// <summary>
        ///   URL of the JSON Web Key Set.
        ///   This set is a collection of JSON Web Keys, a standard method for representing cryptographic
        ///   keys in a JSON structure. For Hosted Login, that collection consists of the public keys used
        ///   to verify the signatures of the identity tokens issued by the authorization server.
        /// </summary>
        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; }

        /// <summary>
        ///   Specifies the way the authorization server responds after a user successfully authenticates.
        /// </summary>
        [JsonPropertyName("response_types_supported")]
        public IEnumerable<string> ResponseTypesSupported { get; set; }
        
        /// <summary>
        ///   JSON array containing a list of the supported Subject Identifier types. 
        /// </summary>
        [JsonPropertyName("subject_types_supported")]
        public IEnumerable<string> SubjectTypesSupported { get; set; }

        /// <summary>
        ///    JSON array containing a list of the scopes that the authorization server supports. 
        /// </summary>
        [JsonPropertyName("scopes_supported")]
        public IEnumerable<string> ScopesSupported { get; set; }

        /// <summary>
        ///   Specifies the different ways that a client can be granted an access token and, as a result,
        ///   can be given access to specific resources. 
        /// </summary>
        [JsonPropertyName("grant_types_supported")]
        public IEnumerable<string> GrantTypesSupported { get; set; }
        
        /// <summary>
        ///   JSON array consisting of all the JSON Web Signature algorithms that can be used for signing
        ///   JSON Web Tokens, such as SHA1 or SHA256. 
        /// </summary>
        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public IEnumerable<string> IdTokenSigningAlgValuesSupported { get; set; }

        /// <summary>
        ///   Downloads and returns the <seealso cref="DiscoveryDocument"/> found at a Url
        ///   resolved from the specified <paramref name="input"/>.
        /// </summary>
        /// <param name="input">
        ///   Either a URL for the well-known discovery endpoint or a (serialized) JWT token to be used for
        ///   resolving the URL.
        /// </param>
        /// <param name="refreshCurrent">
        ///   (optional; default = <c>false</c>)
        ///   Specifies whether to use the (cached) <seealso cref="Current"/> value or download
        ///   a new document and set it as the <seealso cref="Current"/> one.
        /// </param>
        /// <param name="cacheOnSuccess">
        ///   (optional; default = <c>false</c>)
        ///   Specifies whether to cache the downloaded <see cref="DiscoveryDocument"/> upon success.
        /// </param>
        /// <returns>
        ///   Please note that when there is a <seealso cref="Current"/> document already, passing a <c>false</c>
        ///   value will have the method simply return the <seealso cref="Current"/> one if assigned or, automatically,
        ///   download a new one and then set that as the <seealso cref="Current"/> document.
        /// </returns>
        public static async Task<BoolValue<DiscoveryDocument>> DownloadAsync(string input, bool refreshCurrent  = false, bool cacheOnSuccess = false)
        {
            if (Current is { } && !refreshCurrent)
                return BoolValue<DiscoveryDocument>.Success(Current);

            var resolvedEndpointUrl = DiscoveryEndpoint.TryResolveUrl(input);
            if (!resolvedEndpointUrl)
                return BoolValue<DiscoveryDocument>.Fail(resolvedEndpointUrl.Message, resolvedEndpointUrl.Exception);

            using var client = new HttpClient();
            using var message = new HttpRequestMessage(HttpMethod.Get, resolvedEndpointUrl.Value.Url);
            try
            {
                var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                    return BoolValue<DiscoveryDocument>.Fail($"Error connecting to {input}: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();
                var discoDocument = JsonSerializer.Deserialize<DiscoveryDocument>(content);
                discoDocument.LastUpdated = DateTime.UtcNow;
                Current = discoDocument;
                if (cacheOnSuccess)
                {
                    saveToCache(discoDocument);
                }
                return BoolValue<DiscoveryDocument>.Success(discoDocument);
            }
            catch (Exception ex)
            {
                return BoolValue<DiscoveryDocument>.Fail($"Error downloading discovery document from {input}: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///   Attempts loading a discovery document from the cache and returns a value to indicate the outcome.
        /// </summary>
        public static async Task<BoolValue<DiscoveryDocument>> TryLoadCached()
        {
            var cache = DependencyService.Get<ICache<DiscoveryDocument>>();
            if (cache is null)
                return BoolValue<DiscoveryDocument>.Fail();

            return await cache.TryGetAsync(CacheKey);
        }

        internal static async Task<BoolValue<DiscoveryDocument>> TryDownloadAndSetCurrent(AuthResult authResult, bool loadFromCacheOnFailure)
        {
            var idToken = authResult.IdToken;
            if (!string.IsNullOrEmpty(idToken))
            {
                var downloaded = await DownloadAsync(authResult.IdToken, true, true);
                if (downloaded)
                    return downloaded;
            }

            if (!loadFromCacheOnFailure) 
                return BoolValue<DiscoveryDocument>.Fail();
            
            var cached = await TryLoadCached();
            if (!cached)
                return BoolValue<DiscoveryDocument>.Fail();
             
            try
            {
                var discoDocument = cached.Value;
                if (Current is null || Current.LastUpdated < discoDocument.LastUpdated)
                    Current = discoDocument;

                return BoolValue<DiscoveryDocument>.Success(discoDocument);
            }
            catch (Exception ex)
            {
                return BoolValue<DiscoveryDocument>.Fail($"Failed while deserializing cached discovery document. {ex.Message}", ex);
            }
        }

        static async void saveToCache(DiscoveryDocument discoDocument)
        {
            var cache = DependencyService.Get<ICache<DiscoveryDocument>>();
            if (cache is { })
                await cache.AddAsync(CacheKey, discoDocument, true);
        }
    }
}