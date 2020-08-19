﻿using System;
using System.Diagnostics;
using Xamarin.Forms;
using authClient.viewModels;
using TetraPak.Auth.Xamarin;
using ValueChangedEventArgs = SegmentedControl.FormsPlugin.Abstractions.ValueChangedEventArgs;

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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (!(BindingContext is MainViewModel mainViewModel) || mainViewModel.AuthCodeGrant is null)
                return;

            switch (mainViewModel.AuthCodeGrant.Environment)
            {
                case RuntimeEnvironment.Production:
                    ScEnvironment.SelectedSegment = 0;
                    break;
                case RuntimeEnvironment.Migration:
                    ScEnvironment.SelectedSegment = 1;
                    break;
                case RuntimeEnvironment.Development:
                    ScEnvironment.SelectedSegment = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        /* obsolete
        void onAuthorityLabelTapped(object sender, System.EventArgs e)
        {
            if (!(BindingContext is MainViewModel mainViewModel))
                return;

            if (isDoubleTap() && mainViewModel.AuthCodeGrant.ToggleIsLocalIdentityProvider.CanExecute(null))
                mainViewModel.AuthCodeGrant.ToggleIsLocalIdentityProvider.Execute(null);
        }
        */
        
        void onEnvironmentChanged(object sender, ValueChangedEventArgs e)
        {
            if (!(BindingContext is MainViewModel mainViewModel))
                return;
            
            switch (e.NewValue)
            {
                case 0:
                    mainViewModel.AuthCodeGrant.Environment = RuntimeEnvironment.Development;
                    break;
                    
                case 1:
                    mainViewModel.AuthCodeGrant.Environment = RuntimeEnvironment.Migration;
                    break;
                    
                case 2:
                    mainViewModel.AuthCodeGrant.Environment = RuntimeEnvironment.Production;
                    break;
            }            
        }
    }
}