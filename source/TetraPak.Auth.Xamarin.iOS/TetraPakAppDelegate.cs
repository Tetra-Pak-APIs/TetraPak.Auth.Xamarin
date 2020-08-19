﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using SafariServices;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.iOS
{
    public class TetraPakAppDelegate : FormsApplicationDelegate, IAuthorizingAppDelegate
    {
        readonly List<Uri> _expectedRedirectUris = new List<Uri>();
        static TetraPakAppDelegate s_current;

        static UIViewController RootVC => UIApplication.SharedApplication.Delegate.GetWindow().RootViewController;

        public void ActivateApp()
        {
            // todo No need to activate iOS apps?
        }

        public Task OpenInDefaultBrowserAsync(Uri uri) => OpenInDefaultBrowserAsync(uri, null);

        public Task OpenInDefaultBrowserAsync(Uri uri, Uri redirectUri)
        {
            if (redirectUri != null)
            {
                _expectedRedirectUris.Add(redirectUri);
            }

            var tcs = new TaskCompletionSource<bool>();
            var safariVC = new SFSafariViewController(new NSUrl(uri.AbsoluteUri));
            RootVC.PresentViewController(safariVC, true, () => tcs.SetResult(true));
            return tcs.Task;
        }

        public static bool OnOpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (s_current is null)
            {
                var t = typeof(TetraPakAppDelegate);
                throw new InvalidOperationException($"{t} is not initialized. In your AppDelegate, please call static method {t}.{nameof(OnFinishedLaunching)}.");
            }
            return s_current.OpenUrl(app, url, options);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        // note parameters are retained for possible future compatiblity purposes (Jonas Rembratt 20-03-06)
        public static void OnFinishedLaunching(UIApplication app, NSDictionary options)
        {
            s_current = new TetraPakAppDelegate();
        }
#pragma warning restore IDE0060 // Remove unused parameter

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (isExpectedRedirectUri(url))
            {
                RootVC.DismissViewController(true, null);
            }
            var authCallbackHandler = DependencyService.Get<IAuthCallbackHandler>();
            authCallbackHandler?.HandleUrlCallback(new Uri(url.ToString()));
            return true;
        }

        bool isExpectedRedirectUri(NSUrl url, bool remove = true)
        {
            var uri = new Uri(url.ToString());
            var expected = _expectedRedirectUris.FirstOrDefault(i => i.EqualsBasePath(uri));
            if (expected is null)
                return false;

            if (remove)
            {
                _expectedRedirectUris.Remove(expected);
            }
            return true;
        }

        public TetraPakAppDelegate()
        {
            Authorization.RegisterAppDelegate(() => this);
        }
    }
}
