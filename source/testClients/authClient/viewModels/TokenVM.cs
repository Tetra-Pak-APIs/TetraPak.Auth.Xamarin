using System;
using System.Windows.Input;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.viewModels
{
    public class TokenVM : ViewModel
    {
        string _tokenCaption;
        string _tokenValue;
        string _tokenCommandCaption;
        ICommand _command;

        public string TokenCaption
        {
            get => _tokenCaption;
            set => SetValue(ref _tokenCaption, value);
        }

        public string TokenValue
        {
            get => _tokenValue;
            set => SetValue(ref _tokenValue, value);
        }

        public string CommandCaption
        {
            get => _tokenCommandCaption;
            set
            {
                SetValue(ref _tokenCommandCaption, value);
                OnPropertyChanged(nameof(IsCommandVisible));
            }
        }

        public ICommand Command
        {
            get => _command;
            set
            {
                SetValue(ref _command, value);
                OnPropertyChanged(nameof(IsCommandVisible));
            }
        }

        public bool IsCommandVisible => Command != null && !string.IsNullOrEmpty(CommandCaption);

        public TokenVM(IServiceProvider services, ILog log) : base(services, log)
        {
        }
    }
}
