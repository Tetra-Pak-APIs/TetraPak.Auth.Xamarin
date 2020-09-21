using System;
using TetraPak.Auth.Xamarin.common;
using TetraPak.Auth.Xamarin.logging;

namespace authClient.viewModels
{
    public class StringVM : ValidatedValueVM<string>
    {
        protected override BoolValue<string> OnValidateValue(string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value) && IsRequired)
                return BoolValue<string>.Fail("This value is required");

            return BoolValue<string>.Success(value);
        }

        public StringVM(string valueName, ViewModel parent, IServiceProvider services, ILog log)
            : base(valueName, parent, services, log)
        {
        }
    }
}