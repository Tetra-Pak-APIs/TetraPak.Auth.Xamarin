using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Windows.Input;
using authClient.views;
using Xamarin.Forms;
using TetraPak.Auth.Xamarin;
using TetraPak.Auth.Xamarin.common;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.viewModels
{
    /// <summary>
    ///   A Auth Code grant view model that uses Xamarin.Auth.
    /// </summary>
    public class AuthCodeGrantVM : ViewModel
    {
        AuthConfig _config;
        string _message;
        AuthResult _authorization;
        bool _isUsingCustomAuth;
        bool _isInternalValueChange;
        RuntimeEnvironment _environment;
        bool _isLogAvailable;

        IAuthenticator Authenticator => TetraPak.Auth.Xamarin.Authorization.GetAuthenticator(_config, Log);

        /// <summary>
        ///   Gets or sets the Client Id (a.k.a. "App Id")
        /// </summary>
        [ValidatedValue(PlaceholderValue = "Please specify the Client ID", IsRequired = true)]
        public StringVM ClientId { get; private set; }

        // obsolete (we no longer support client secret in native clients)
        ///// <summary>
        /////   Gets or sets a Client Secret (a.k.a. "App Secret")
        ///// </summary>
        //[ValidatedValue(PlaceholderValue = "Optionally, specify a client secret")]
        //public StringVM ClientSecret { get; private set; }

        /// <summary>
        ///   Gets or sets the runtime environment.
        /// </summary>
        public RuntimeEnvironment Environment
        {
            get => _environment;
            set
            {
                _environment = value;
                _config = AuthConfig.Default(new AuthApplication(_config.ClientId, _config.RedirectUri, value))
                    .AssignFrom(_config);
            }
        }

        /// <summary>
        ///   Gets or sets the scope for the requested token.
        /// </summary>
        [ValidatedValue(PlaceholderValue = "An optional scope to be requested")]
        public StringVM Scope { get; private set; }

        /// <summary>
        ///   Gets or sets the URL of the OAuth2-enabled Authority. 
        /// </summary>
        [ValidatedValue(PlaceholderValue = "Please specify the Authority (URL)", IsRequired = true)]
        public AbsoluteUriVM AuthorityUrl { get; private set; }

        /// <summary>
        ///   Gets or sets the URL of the OAuth2-enabled Token Issuer. 
        /// </summary>
        [ValidatedValue(PlaceholderValue = "Please specify the Token Issuer (URL)", IsRequired = true)]
        public AbsoluteUriVM TokenIssuerUrl { get; private set; }

        public AuthResult Authorization
        {
            get => _authorization;
            set
            {
                SetValue(ref _authorization, value);
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        public bool IsAuthorized => Authorization?.Tokens?.Any() ?? false;

        public bool IsLogAvailable
        {
            get => _isLogAvailable;
            set => SetValue(ref _isLogAvailable, value);
        } 

        /// <summary>
        ///   Gets or sets the redirect URI
        ///   Please remember to register as a Uri Scheme with your app
        ///   (see https://xamarinhelp.com/uri-scheme/).
        /// </summary>
        [ValidatedValue(PlaceholderValue = "Optionally, specify a redirect URL")]
        public AbsoluteUriVM RedirectUrl { get; private set; }
        
        public bool IsStateUsed
        {
            get => _config.IsStateUsed;
            set => setConfigValue(value);
        }

        public bool IsPkceUsed
        {
            get => _config.IsPkceUsed;
            set => setConfigValue(value);
        }

        public bool IsCaching
        {
            get => _config.IsCaching;
            set
            {
                setConfigValue(value);
                if (!IsCaching)
                    TokensResult.Clear();
            }
        }
        
        public bool IsRequestingUserId
        {
            get => _config.IsRequestingUserId;
            set => setConfigValue(value);
        } 

        [ValidatedValue(PlaceholderValue = "Paste a refresh token here to test renewing")]
        
        public TokensResultVM TokensResult { get; }

        public string Message
        {
            get => _message;
            set => SetValue(ref _message, value);
        }

#if DEBUG
        /*
        /// <summary>
        ///   Specifies whether to use a local (mock) identity provider
        ///   rather than the "real" one.
        /// </summary>
        public bool IsLocalIdentityProvider
        {
            get => _config.IsTargetingLocalAuthority;
            set
            {
                _config.IsTargetingLocalAuthority = value;
                AuthorityUrl.Value = _config.Authority.AbsoluteUri;
                TokenIssuerUrl.Value = _config.TokenIssuer.AbsoluteUri;
            }
        }
        */
#endif

        public bool IsUsingCustomAuth
        {
            get => _isUsingCustomAuth;
            set => SetValue(ref _isUsingCustomAuth, value);
        }

        public ICommand AuthorizeCommand { get; }
        
        public ICommand AuthorizeSilentlyCommand { get; }

        /* obsolete
        public ICommand ToggleIsLocalIdentityProvider { get; }
        */
        
        public ICommand DeleteAccessTokenCommand { get; }

        public ICommand ViewLogCommand { get; }

        void setConfigValue(object value, [CallerMemberName] string propertyName = null)
        {
            var p = _config.GetType().GetProperty(propertyName);
            p.SetValue(_config, value);
        }
        
        async Task onAuthorize(bool silently)
        {
            var authorized = silently
                ? await Authenticator.GetAccessTokenSilentlyAsync()
                : await Authenticator.GetAccessTokenAsync();

            IsLogAvailable = true;

            if (authorized)
            {
                // success ...
                LogInfo($"AUTHORIZED! Access token = {authorized.Value.AccessToken}");
                setTokensResult(authorized);
                Authorization = authorized.Value;
            }
            else
            {
                // failure ...
                setFailureResult(authorized);
                LogInfo($"Authorization failed with message: \"{authorized.Message}\"");
            }
        }

        void setTokensResult(BoolValue<AuthResult> authResult)
        {
            TokensResult.Clear();
            foreach (var tokenInfo in authResult.Value.Tokens)
            {
                var res = resolveCaptionAndValidation(tokenInfo);
                var commandCaption = tokenInfo.IsValidatable ? "VALIDATE" : null; 
                var icon = tokenInfo.IsValidatable ? Theme.IconValidate : null;
                var command = tokenInfo.IsValidatable 
                    ? new Command(async vm =>
                    {
                        ((TokenVM) vm).IsTokenValid = await tokenInfo.IsValidAsync();
                    }) 
                    : null;
                TokensResult.AddToken(res.tokenCaption, tokenInfo.TokenValue, commandCaption, icon, command, res.isUnvalidated);
            }
        }

        static (string tokenCaption, bool isUnvalidated) resolveCaptionAndValidation(TokenInfo token)
        {
            switch (token.Role)
            {
                case TokenRole.AccessToken:
                    return ("Access", false);

                case TokenRole.RefreshToken:
                    return ("Refresh", false);

                case TokenRole.IdToken:
                    return ("ID", true);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void setFailureResult(BoolValue<AuthResult> authResult)
        {
            Message = authResult.Message;
        }

        internal override void NotifyChildValueChanged(ViewModel viewModel, string valueName, object oldValue, object newValue)
        {
            if (_isInternalValueChange)
                return;

            base.NotifyChildValueChanged(viewModel, valueName, oldValue, newValue);
            switch (valueName)
            {
                case nameof(AuthorityUrl):
                    _config.Authority = new Uri((string)newValue);
                    break;

                case nameof(TokenIssuerUrl):
                    _config.TokenIssuer = new Uri((string)newValue);
                    break;

                case nameof(RedirectUrl):
                    _config.RedirectUri = string.IsNullOrEmpty(newValue as string) ? null : new Uri((string)newValue);
                    break;

                case nameof(ClientId):
                    _config.ClientId = (string)newValue;
                    break;

                //case nameof(ClientSecret): obsolete (we no longer support client secret in native clients)
                //    _config.ClientSecret = (string)newValue;
                //    break;

                case nameof(Scope):
                    _config.Scope = (string)newValue;
                    break;

                case nameof(IsStateUsed):
                    _config.IsStateUsed = (bool)newValue;
                    break;

                case nameof(IsPkceUsed):
                    _config.IsPkceUsed = (bool)newValue;
                    break;

                case nameof(IsCaching):
                    _config.IsCaching = (bool)newValue;
                    break;

            }
        }

        async Task onDeleteAccessTokenAsync()
        {
            TokensResult.Remove("Access");
            await _config.TokenCache.RemoveAccessTokenAsync();
        }

        async Task onViewLog()
        {
            var navigation = Services.GetService<INavigation>();
            await navigation.PushAsync(new LogPage(new LogVM(Services, Log)));
        }

        void initializeValues(AuthConfig config)
        {
            _isInternalValueChange = true;
            _isUsingCustomAuth = true;

            AuthorityUrl.Value = config.Authority?.AbsoluteUri;
            TokenIssuerUrl.Value = config.TokenIssuer?.AbsoluteUri;
            RedirectUrl.Value = config.RedirectUri?.AbsoluteUri;
            ClientId.Value = config.ClientId;
            //ClientSecret.Value = config.ClientSecret; obsolete (we no longer support client secret in native clients)
            Scope.Value = config.Scope;
            IsStateUsed = config.IsStateUsed;
            IsPkceUsed = config.IsPkceUsed;
            IsCaching = config.IsCaching;

            if (IsCaching)
                initializeTokensFromCache(config.TokenCache);

            _isInternalValueChange = false;
        }

        async Task initializeTokensFromCache(TokenCache tokenCache)
        {
            var authResult = await tokenCache.TryGetAsync();
            if (authResult)
            {
                Authorization = authResult.Value;
                setTokensResult(authResult);
            }
        }

        public AuthCodeGrantVM(AuthApplication application, IServiceProvider services, ILog log) : base(services, log)
        {
            _environment = application.Environment;
            _config = AuthConfig.Default(application);
            TokensResult = Services.GetService<TokensResultVM>();
            AuthorizeCommand = new Command(async () => await onAuthorize(false));
            AuthorizeSilentlyCommand = new Command(async () => await onAuthorize(true), () => IsCaching);
            DeleteAccessTokenCommand = new Command(async () => await onDeleteAccessTokenAsync());
            ViewLogCommand = new Command(async () => await onViewLog());
#if DEBUG
            /*
            ToggleIsLocalIdentityProvider = new Command(() => IsLocalIdentityProvider = !IsLocalIdentityProvider);
            */
#endif
            initializeValues(_config);
        }

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidatedValueAttribute : Attribute
    {
        public object PlaceholderValue { get; set; }
        public bool IsRequired { get; set; }
    }

    static class AuthConfigExtensions
    {
        public static AuthConfig AssignFrom(this AuthConfig self, AuthConfig source)
        {
            var props = self.GetType().GetProperties();
            foreach (var p in props)
            {
                if (!p.CanWrite || !p.CanRead || p.Name == nameof(AuthConfig.Authority) || p.Name == nameof(AuthConfig.TokenIssuer))
                    continue;

                var value = p.GetValue(source);
                p.SetValue(self, value);
            }

            return self;
        }
    }
}