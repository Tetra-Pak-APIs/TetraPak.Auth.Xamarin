using System;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.idTokenValidation
{
    /// <summary>
    /// Represents a URL to a discovery endpoint - parsed to separate the URL and authority
    /// </summary>
    public class DiscoveryEndpoint
    {
        const string WellKnownEndpoint = ".well-known/openid-configuration";

        /// <summary>
        /// Gets or sets the authority.
        /// </summary>
        /// <value>
        /// The authority.
        /// </value>
        public string Authority { get; }

        /// <summary>
        /// Gets or sets the discovery endpoint.
        /// </summary>
        /// <value>
        /// The discovery endpoint.
        /// </value>
        public string Url { get; }

        /// <summary>
        /// Parses a URL and turns it into authority and discovery endpoint URL.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Malformed URL
        /// </exception>
        public static DiscoveryEndpoint ParseUrl(string input)
        {   
            var success = Uri.TryCreate(input, UriKind.Absolute, out var uri);
            if (success == false)
            {
                throw new InvalidOperationException("Malformed URL");
            }

            if (!IsValidScheme(uri))
            {
                throw new InvalidOperationException("Malformed URL");
            }

            var url = input.RemoveTrailingSlash();

            return url.EndsWith(WellKnownEndpoint, StringComparison.OrdinalIgnoreCase)
                ? new DiscoveryEndpoint(url.Substring(0, url.Length - WellKnownEndpoint.Length - 1), url)
                : new DiscoveryEndpoint(url, $"{url.EnsureEndsWith("/")}{WellKnownEndpoint}");
        }

        /// <summary>
        /// Determines whether the URL uses http or https.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is valid scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidScheme(Uri url)
        {
            if (string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        ///   Determines whether uses a secure scheme according to the policy.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="policy">The policy.</param>
        /// <returns>
        ///   <c>true</c> if [is secure scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSecureScheme(Uri url, DiscoveryPolicy policy)
        {
            if (policy.RequireHttps != true) 
                return true;

            if (policy.AllowHttpOnLoopback != true)
                return string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
            
            var hostName = url.DnsSafeHost;
            foreach (var address in policy._loopbackAddresses)
            {
                if (string.Equals(hostName, address, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryEndpoint"/> class.
        /// </summary>
        /// <param name="authority">The authority.</param>
        /// <param name="url">The discovery endpoint URL.</param>
        public DiscoveryEndpoint(string authority, string url)
        {
            Authority = authority;
            Url = url;
        }
    }
}
