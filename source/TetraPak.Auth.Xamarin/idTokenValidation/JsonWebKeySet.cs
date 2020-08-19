using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.idTokenValidation
{
    public class JsonWebKeySet
    {
        //[JsonPropertyName("keys")]
        public IList<JsonWebKey> Keys { get; set; }

        public static async Task<BoolValue<JsonWebKeySet>> DownloadAsync(string url)
        {
            using var client = new HttpClient();
            using var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jwk-set+json"));
            try
            {
                var response = await client.SendAsync(message);
                if (!response.IsSuccessStatusCode)
                    return BoolValue<JsonWebKeySet>.Fail($"Error connecting to {url}: {response.ReasonPhrase}");

                var content = await response.Content.ReadAsStringAsync();
                var jsonWebKeySet = JsonConvert.DeserializeObject<JsonWebKeySet>(content);
                return BoolValue<JsonWebKeySet>.Success(jsonWebKeySet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}