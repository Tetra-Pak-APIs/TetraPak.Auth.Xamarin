using TetraPak.Auth.Xamarin.common;

namespace TetraPak.Auth.Xamarin.idTokenValidation
{
    static class InternalStringExtensions
    {
        public static string RemoveTrailingSlash(this string self) => self.EnsureNotEndsWith("/");
    }
}