using System;
using TetraPak.Auth.Xamarin.logging;
using TetraPak.Auth.Xamarin.oidc;

namespace TetraPak.Auth.Xamarin
{
    /// <summary>
    ///   Provides a starting point for authorization.
    /// </summary>
    /// <remarks>
    ///   This class can be used to start authorization of your client.
    /// </remarks>
    public static class Authorization
    {
        static Func<IAuthorizingAppDelegate> s_appDelegate;

        /// <summary>
        ///   Intended for internal use only.
        /// </summary>
        public static bool IsAppDelegateRegistered => s_appDelegate != null;

        /// <summary>
        ///   Intended for internal use only.
        /// </summary>
        public static void RegisterAppDelegate(Func<IAuthorizingAppDelegate> appDelegate)
        {
            if (s_appDelegate != null)
                throw new InvalidOperationException("Authorizing App Delegate was already registered");

            s_appDelegate = appDelegate;
            TetraPakAuthenticator.Authorized += onAuthorized;
        }

        static async void onAuthorized(object sender, AuthResultEventArgs args)
        {
            await DiscoveryDocument.TryDownloadAndSetCurrent(args.Result.Value, true);
            // obsolete AuthScope.Discover(discoDocumentDownloaded.Value);
        }

        /// <summary>
        ///   Intended for internal use only.
        /// </summary>
        public static IAuthorizingAppDelegate GetAuthorizingAppDelegate()
        {
            return s_appDelegate?.Invoke();
        }

        /// <summary>
        ///   Resolves and returns a suitable authenticator.  
        /// </summary>
        /// <param name="config">
        ///   The app configuration (<see cref="AuthConfig"/>).
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger, for diagnostics purposes. 
        /// </param>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        public static IAuthenticator GetAuthenticator(AuthConfig config, ILog log = null) 
            => new TetraPakAuthenticator(config, log);

        /// <summary>
        ///   Resolves and returns a suitable authenticator. 
        /// </summary>
        /// <param name="application">
        ///   A <see cref="AuthApplication"/> descriptor value.
        ///   Can be passed as a string.
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger, for diagnostics purposes. 
        /// </param>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        public static IAuthenticator GetAuthenticator(AuthApplication application, ILog log = null)
            => GetAuthenticator(AuthConfig.Default(application), log);

        /// <summary>
        ///   Resolves and returns a suitable authenticator.
        /// </summary>
        /// <param name="clientId">
        ///   Specifies the client id (a.k.a. app id). 
        /// </param>
        /// <param name="redirectUri">
        ///   Specifies the redirect <see cref="Uri"/>.
        /// </param>
        /// <param name="environment">
        ///   (optional; default = <see cref="RuntimeEnvironment.Production"/>)<br/>
        ///   Specifies the targeted runtime environment. 
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A logger, for diagnostics purposes. 
        /// </param>
        /// <returns>
        ///   An authenticator (implements <see cref="IAuthenticator"/>).
        /// </returns>
        public static IAuthenticator GetAuthenticator(
            string clientId, Uri redirectUri, 
            RuntimeEnvironment environment = RuntimeEnvironment.Production, ILog log = null)
            => GetAuthenticator(AuthConfig.Default(environment, clientId, redirectUri), log);
    }
}
