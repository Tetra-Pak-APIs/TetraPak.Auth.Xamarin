using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.idTokenValidation
{
    public class DiscoveryDocument 
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }        
        
        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; }        
         
        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; }

        [JsonPropertyName("response_types_supported")]
        public IEnumerable<string> ResponseTypesSupported { get; set; }
        
        [JsonPropertyName("subject_types_supported")]
        public IEnumerable<string> SubjectTypesSupported { get; set; }

        [JsonPropertyName("scopes_supported")]
        public IEnumerable<string> ScopesSupported { get; set; }

        [JsonPropertyName("grant_types_supported")]
        public IEnumerable<string> GrantTypesSupported { get; set; }
        
        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public IEnumerable<string> IdTokenSigningAlgValuesSupported { get; set; }

        public static async Task<BoolValue<DiscoveryDocument>> DownloadAsync(string url)
        {
            using var client = new HttpClient(); // { Timeout = TimeSpan.FromSeconds(5) };
            using var message = new HttpRequestMessage(HttpMethod.Get, url);
            try
            {
                var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                    return BoolValue<DiscoveryDocument>.Fail($"Error connecting to {url}: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();
                var discoDocument = JsonSerializer.Deserialize<DiscoveryDocument>(content);
                return BoolValue<DiscoveryDocument>.Success(discoDocument);
            }
            catch (Exception ex)
            {
                return BoolValue<DiscoveryDocument>.Fail($"Error downloading discovery document from {url}: {ex.Message}", ex);
            }
        }
    }
}