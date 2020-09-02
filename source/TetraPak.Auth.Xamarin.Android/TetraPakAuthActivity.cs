using System;
using Android.App;
using Android.Content;
using Android.OS;
using TetraPak.Auth.Xamarin.Android;
using TetraPak.Auth.Xamarin.logging;
using Xamarin.Forms;

[assembly: Dependency(typeof(TetraPakAuthActivity))]

namespace TetraPak.Auth.Xamarin.Android
{
    // [Activity(Label = "TetraPakAuthActivity")]
    // [IntentFilter(
    //         actions: new[] { Intent.ActionView },
    //         Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //         DataSchemes = new[] { "testping" },
    //         DataPath = "/auth"
    //         )]
    public class TetraPakAuthActivity : Activity
    {
        public static void OnCreate(Activity activity, Intent intent, bool finish = true)
        {
            global::Android.Net.Uri androidUri = intent.Data;
            var uri = new Uri(androidUri.ToString());
            var log = DependencyService.Get<ILog>();
            log.Debug($"OnCreate: Uri={uri}");
            Authorization.GetAuthorizingAppDelegate().ActivateApp();
            var authCallbackHandler = DependencyService.Get<IAuthCallbackHandler>();
            authCallbackHandler?.HandleUrlCallback(uri);
            if (finish)
            {
                activity.Finish();
            }
        }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Android.Net.Uri androidUri = Intent.Data;
            var uri = new Uri(androidUri.ToString());
            var log = DependencyService.Get<ILog>();
            log.Debug($"OnCreate: Uri={uri}");
            Authorization.GetAuthorizingAppDelegate().ActivateApp();
            var authCallbackHandler = DependencyService.Get<IAuthCallbackHandler>();
            authCallbackHandler?.HandleUrlCallback(uri);
            Finish();
        }
    }
}
