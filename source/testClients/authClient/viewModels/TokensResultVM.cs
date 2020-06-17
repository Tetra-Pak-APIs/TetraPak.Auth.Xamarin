﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.viewModels
{
    public class TokensResultVM : ViewModel
    {
        public ObservableCollection<TokenVM> Tokens { get; }

        internal void AddToken(string tokenCaption, string token, string commandCaption = null, ICommand command = null)
        {
            var vm = Services.GetService<TokenVM>();
            vm.TokenCaption = tokenCaption;
            vm.TokenValue = token;
            vm.CommandCaption = commandCaption ?? vm.CommandCaption;
            vm.Command = command ?? vm.Command;
            Tokens.Add(vm);
        }

        internal void Clear()
        {
            Tokens.Clear();
        }
        
        internal void Remove(string tokenCaption)
        {
            var item = Tokens.FirstOrDefault(i => i.TokenCaption == tokenCaption);
            if (item != null)
                Tokens.Remove(item);
        }

        public TokensResultVM(IServiceProvider services, ILog log) : base(services, log)
        {
            Tokens = new ObservableCollection<TokenVM>();
        }
    }
}
