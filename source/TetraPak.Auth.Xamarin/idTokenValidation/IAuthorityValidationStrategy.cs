using System;
using System.Collections.Generic;
using System.Text;

namespace TetraPak.Auth.Xamarin.idTokenValidation
{
    /// <summary>
    /// Authority validation strategy.
    /// </summary>
    public interface IAuthorityValidationStrategy
    {
        /// <summary>
        /// Validate issuer name found in Discovery Document.
        /// </summary>
        /// <param name="expectedAuthority">Authority expected.</param>
        /// <param name="issuerName">Authority declared in Discovery Document.</param>
        /// <returns></returns>
        AuthorityValidationResult IsIssuerNameValid(string issuerName, string expectedAuthority);

        /// <summary>
        /// Validate end point found in Discovery Document.
        /// </summary>
        /// <param name="expectedAuthority">Authority expected.</param>
        /// <param name="endpoint">Endpoint declared in Discovery Document.</param>
        /// <returns></returns>
        AuthorityValidationResult IsEndpointValid(string endpoint, IEnumerable<string> expectedAuthority);
    }
}
