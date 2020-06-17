using System;
using TetraPak.Auth.Xamarin.common;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.viewModels
{
    /// <summary>
    ///   An absolute URI value that works well in view models.
    /// </summary>
    public class AbsoluteUriVM : StringVM
    {
        public Uri UriValue => IsInvalid ? null : new Uri(Value);

        protected override BoolValue<string> OnValidateValue(string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
                return base.OnValidateValue(value);

            return !Uri.TryCreate(value, UriKind.Absolute, out _) 
                ? BoolValue<string>.Fail("Invalid absolute URI") 
                : BoolValue<string>.Success(value);
        }
        
        public AbsoluteUriVM(string valueName, ViewModel parent, IServiceProvider services, ILog log)
            : base(valueName, parent, services, log)
        {
        }
    }
}