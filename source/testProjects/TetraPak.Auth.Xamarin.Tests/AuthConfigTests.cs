using System;
using Xunit;

namespace TetraPak.Auth.Xamarin.Tests
{
    public class AuthConfigTests
    {
        [Fact]
        public void InitialState()
        {
            var config = getStartConfig();
            Assert.True(string.IsNullOrEmpty(config.Scope));
        }

        [Fact]
        public void AddScope()
        {
            var config = getStartConfig(" aaa ", " bbb ");
            Assert.Equal("aaa bbb", config.Scope);
        }
        
        [Fact]
        public void RemoveScope()
        {
            var config = getStartConfig(" aaa ", " bbb ");
            config.RemoveScope("bbb");
            Assert.Equal("aaa", config.Scope);
            config.RemoveScope("ccc");
            Assert.Equal("aaa", config.Scope);
            config.RemoveScope("aaa");
            Assert.True(string.IsNullOrEmpty(config.Scope));
        }

        static AuthConfig getStartConfig(params string[] scope)
        {
            return AuthConfig
                .Default(RuntimeEnvironment.Migration, "clientId", new Uri("test://redirect"))
                .WithScope(scope);
        }
    }
}