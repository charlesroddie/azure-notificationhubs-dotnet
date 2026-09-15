using System;
using System.Threading;
using Microsoft.Azure.NotificationHubs.Auth;
using Xunit;

namespace Microsoft.Azure.NotificationHubs.Tests
{
    public class TokenProviderTests
    {
        class CountingTokenProvider : TokenProvider
        {
            public int Generated;

            public CountingTokenProvider(TimeSpan cacheExpirationTime) : base(cacheExpirationTime) { }

            protected override string GenerateToken(string appliesTo) => $"{appliesTo}{Interlocked.Increment(ref Generated)}";
        }

        [Fact]
        public void TokenIsCachedUntilExpiry()
        {
            var provider = new CountingTokenProvider(TimeSpan.FromMilliseconds(200));

            var first = provider.GetToken("https://ns.servicebus.windows.net/hub/registrations/1");
            Assert.Equal(first, provider.GetToken("https://ns.servicebus.windows.net/hub/registrations/1"));

            Thread.Sleep(300);

            Assert.NotEqual(first, provider.GetToken("https://ns.servicebus.windows.net/hub/registrations/1"));
            Assert.Equal(2, provider.Generated);
        }
    }
}
