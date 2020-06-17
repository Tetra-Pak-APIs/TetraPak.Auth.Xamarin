using System;
using authClient.viewModels;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.Auth.Xamarin;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.dependencies
{
    public static class Dependencies
    {
        static IServiceProvider s_services;

        public static IServiceProvider SetupDependencies(this App self)
        {
            var c = new ServiceCollection();

            c.AddSingleton<ILog, BasicLog>();
            c.AddSingleton<MainViewModel>();
            c.AddTransient<TokenVM>();
            c.AddSingleton<AuthCodeGrantVM>();
            c.AddTransient<TokensResultVM>();
            //c.AddTransient(p => (AuthApplication) "MIG; RKAGXch5BTAGbuyj24Se88Pl0NGKylss; testping://auth")
            c.AddTransient(p => (AuthApplication) "DEV; RKAGXch5BTAGbuyj24Se88Pl0NGKylss; testping://auth")
#if DEBUG
                // .WithLocalAuthority(
                //     new Uri("http://192.168.1.62:5000/oauth2/authorize"), new Uri("http://192.168.1.62:5000/oauth2/token")
                //     //new Uri("https://10.69.105.9:5001/oauth2/authorize"), new Uri("https://10.69.105.9:5001/oauth2/token")
                //     )
#endif
                ;
            c.AddSingleton(p => self.MainPage.Navigation);
            c.AddSingleton(p => s_services);

            return s_services = c.BuildServiceProvider();
        }
    }
}
