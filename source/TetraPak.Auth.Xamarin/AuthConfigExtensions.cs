using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

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
        public static AuthConfig WithScope(this AuthConfig self, AuthScope scope)
        {
            self.Scope = scope;
            return self;
            /* obsolete
            var strings = self.Scope?.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var current = new HashSet<string>(strings);
            var sb = new StringBuilder(self.Scope);
            foreach (var identifier in scope.Where(i => !current.Contains(i)))
            {
                sb.Append($"{identifier.Trim()} ");
            }
            
            self.Scope = sb.ToString().Trim();
            return self;
            */
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
        ///   Adds one or more scope types (not already supported) to the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig AddScope(this AuthConfig self, params string[] scopeTypes)
        {
            var list = new List<string>(self.Scope.Items);
            foreach (var scope in scopeTypes)
            {
                if (!list.Any(i => i.Equals(scope, StringComparison.InvariantCultureIgnoreCase)))
                    list.Add(scope);
            }
            self.Scope = new AuthScope(list.ToArray());
            return self;
        }

        /// <summary>
        ///   Adds one or more scope types (not already supported) to the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig AddScope(this AuthConfig self, AuthScope scope) => self.AddScope(scope.Items);

        /// <summary>
        ///   Removes one or more scope types from the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig RemoveScope(this AuthConfig self, params string[] scopeTypes)
        {
            IEnumerable<string> list = new List<string>(self.Scope.Items);
            foreach (var type in scopeTypes)
            {
                var index = list.IndexOf(i => i.Equals(type, StringComparison.InvariantCultureIgnoreCase));
                if (index != -1)
                    ((List<string>)list).RemoveAt(index);
            }

            return self.WithScope(new AuthScope(list.ToArray()));
        }

        /// <summary>
        ///   Removes one or more scope types from the <see cref="AuthConfig.Scope"/> value.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        public static AuthConfig RemoveScope(this AuthConfig self, AuthScope scope) => self.RemoveScope(scope.Items);

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