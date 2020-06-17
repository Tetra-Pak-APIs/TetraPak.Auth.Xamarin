using System.Diagnostics;
using Xamarin.Forms;
using authClient.viewModels;

namespace authClient
{
    public partial class MainPage : ContentPage
    {
        Stopwatch _doubleTapTimer;
        const long DoubleTapTimespanMs = 400L;

        public MainPage()
        {
            InitializeComponent();
        }

        bool isDoubleTap()
        {
            if (_doubleTapTimer is null)
            {
                _doubleTapTimer = Stopwatch.StartNew();
                return false;
            }
            var elapsed = _doubleTapTimer.ElapsedMilliseconds;
            _doubleTapTimer = null;
            return elapsed < DoubleTapTimespanMs;
        }

        void onAuthorityLabelTapped(object sender, System.EventArgs e)
        {
            if (!(BindingContext is MainViewModel mainViewModel))
                return;

            if (isDoubleTap() && mainViewModel.AuthCodeGrant.ToggleIsLocalIdentityProvider.CanExecute(null))
                mainViewModel.AuthCodeGrant.ToggleIsLocalIdentityProvider.Execute(null);
        }
    }
}