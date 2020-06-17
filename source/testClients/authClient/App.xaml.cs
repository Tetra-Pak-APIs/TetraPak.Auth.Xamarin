using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using authClient.dependencies;
using authClient.viewModels;
using TetraPak.Auth.Xamarin.logging;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace authClient
{
    public partial class App : Application
    {
        public ILog Log => Services.GetService<ILog>(); 

        public IServiceProvider Services { get; }

        public App()
        {
            Services = this.SetupDependencies();
            Log.Logged += onLogged;
            InitializeComponent();
            try
            {
                MainPage = new MainPage { BindingContext = Services.GetService<MainViewModel>() };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void onLogged(object sender, TextLogEventArgs e)
        {
            Debug.WriteLine(e.Rank == LogRank.Error
                ? $"[{e.Rank}] {e.Exception}{(e.Message != null ? $" {e.Message}" : "")}"
                : $"[{e.Rank}] {e.Message}");
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}