using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   Offers convenient extensions for the <see cref="AuthConfig"/> class.
    /// </summary>
    public static class AuthConfigExtensions
    {
        /// <summary>
        ///   Fluent API to assign the <see cref="AuthConfig.Authority"/>. 
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig WithAuthority(this AuthConfig self, Uri authorityUri)
        {
            self.Authority = authorityUri;
            return self;
        }

        /// <summary>
        ///   Fluent API to assign the <see cref="AuthConfig.TokenIssuer"/>. 
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig WithTokenIssuer(this AuthConfig self, Uri tokenIssuer)
        {
            self.TokenIssuer = tokenIssuer;
            return self;
        }

        /// <summary>
        ///   Fluent API to assign the two endpoint properties
        ///   (<see cref="AuthConfig.Authority"/> and <see cref="AuthConfig.TokenIssuer"/>). 
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig WithEndpoints(this AuthConfig self, Uri authorityUri, Uri tokenIssuer)
        {
            return self.WithAuthority(authorityUri).WithTokenIssuer(tokenIssuer);
        }

        /// <summary>
        ///   Adds one or more scope identifiers to the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig WithScope(this AuthConfig self, params string[] scope)
        {
            var strings = self.Scope?.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var current = new HashSet<string>(strings);
            var sb = new StringBuilder(self.Scope);
            foreach (var identifier in scope.Where(i => !current.Contains(i)))
            {
                sb.Append($"{identifier.Trim()} ");
            }
            
            self.Scope = sb.ToString().Trim();
            return self;
        }

        /// <summary>
        ///   Clears the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig ClearScope(this AuthConfig self)
        {
            self.Scope = null;
            return self;
        }

        /// <summary>
        ///   Removes one or more scope identifiers from the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig RemoveScope(this AuthConfig self, params string[] scope)
        {
            var strings = self.Scope?.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var current = new HashSet<string>(strings);
            var removing = new HashSet<string>(scope);
            var sb = new StringBuilder();
            foreach (var identifier in current.Where(identifier => !removing.Contains(identifier)))
            {
                sb.Append($"{identifier.Trim()} ");
            }
            
            self.Scope = sb.ToString().Trim();
            return self;
        }

        /// <summary>
        ///   Replaces the default <see cref="TokenCache"/> with a (custom) one.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig WithTokenCache(this AuthConfig self, TokenCache tokenCache)
        {
            self.TokenCache = tokenCache;
            return self;
        }
    }
}