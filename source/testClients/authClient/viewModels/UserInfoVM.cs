using System;
using TetraPak.Auth.Xamarin;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.viewModels
{
    public class UserInfoVM : ViewModel
    {
        readonly AuthResult _authResult;

        async void loadUserInfoAsync()
        {
            var userInfoTypesResult = await _authResult.TeyGetUserInfoTypes();
        }
        
        public UserInfoVM(IServiceProvider services, AuthResult authResult, ILog log) : base(services, log)
        {
            _authResult = authResult;
            loadUserInfoAsync();
        }
    }
}