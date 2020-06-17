using System.Threading.Tasks;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   Describes the contract for an authenticator implementation.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        ///   Attempts to acquire an access token.
        /// </summary>
        /// <param name="allowCached">
        ///   (optional; default = <c>true</c>)<br/>
        ///   Can be used to override global cache setting
        ///   value for this particular operation.
        /// </param>
        /// <returns>
        ///   A <seealso cref="BoolValue{T}"/> indicating success while also carrying
        ///   details for the authentication result, including access token.
        /// </returns>
        Task<BoolValue<AuthResult>> GetAccessTokenAsync(bool allowCached = true);

        /// <summary>
        ///   Attempts to acquire an access token "silently", automatically
        ///   using any persisted refresh token in the process.
        /// </summary>
        /// <returns>
        ///   A <seealso cref="BoolValue{T}"/> indicating success while also carrying
        ///   details for the authentication result, including access token.
        /// </returns>
        Task<BoolValue<AuthResult>> GetAccessTokenSilentlyAsync();
    }
}
