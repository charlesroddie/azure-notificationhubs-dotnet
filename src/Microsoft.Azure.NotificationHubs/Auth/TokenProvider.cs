//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Azure.NotificationHubs.Auth
{
    /// <summary>
    /// Represents a token provider.
    /// </summary>
    public abstract class TokenProvider : IDisposable
    {
        private const int PruneInterval = 100;

        private readonly ConcurrentDictionary<string, (string Token, DateTime ExpiresAtUtc)> _tokenCache = new ConcurrentDictionary<string, (string, DateTime)>();
        private readonly bool _cacheTokens;
        private readonly TimeSpan _cacheExpirationTime;
        private int _writesSincePrune;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProvider"/> class.
        /// </summary>
        /// /// <param name="cacheExpirationTime">Cache expiration time.</param>
        protected TokenProvider(TimeSpan cacheExpirationTime)
            : this(true, cacheExpirationTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProvider"/> class.
        /// </summary>
        /// <param name="cacheTokens">Cache tokens.</param><param name="cacheExpirationTime">Cache expiration time.</param>
        protected TokenProvider(bool cacheTokens, TimeSpan cacheExpirationTime)
        {
            _cacheTokens = cacheTokens && cacheExpirationTime > TimeSpan.Zero;
            _cacheExpirationTime = cacheExpirationTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Azure.NotificationHubs.TokenProvider" /> class.
        /// </summary>
        /// <param name="cacheTokens">if set to <c>true</c> [cache tokens].</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">cacheSize</exception>
        protected TokenProvider(bool cacheTokens)
        {
            _cacheTokens = cacheTokens;
        }

        /// <summary>
        /// When implemented, generates a token based upon the scope.
        /// </summary>
        /// <param name="appliesTo">The scope for the token.</param>
        /// <returns>The generated token based upon the scope.</returns>
        protected abstract string GenerateToken(string appliesTo);

        /// <summary>
        /// Gets whether the token provider strips query parameters.
        /// </summary>
        ///
        /// <returns>
        /// true if the token provider strips query parameters; otherwise, false.
        /// </returns>
        protected virtual bool StripQueryParameters => true;

        /// <summary>
        /// Asynchronously retrieves the token for the provider.
        /// </summary>
        ///
        /// <returns>
        /// The result of the asynchronous operation.
        /// </returns>
        /// <param name="appliesTo">The URI which the access token applies to.</param>
        /// <param name="action">The request action.</param>
        /// <param name="bypassCache">true to ignore existing token information in the cache; false to use the token information in the cache.</param>
        public string GetToken(string appliesTo, string action = "Manage", bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(appliesTo))
            {
                throw new ArgumentException(SRClient.NullAppliesTo);
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var normalizedAppliesTo = NormalizeAppliesTo(appliesTo);

            if(TryFetchFromCache(normalizedAppliesTo, action, bypassCache, out string token))
            {
                return token;
            }

            //If token is not found in cache, build signature and return
            token = GenerateToken(normalizedAppliesTo);

            //Add generated token to cache
            TrySetIntoCache(normalizedAppliesTo, action, bypassCache, token);

            return token;
        }

        private bool TryFetchFromCache(string appliesTo, string action, bool bypassCache, out string token)
        {
            token = null;
            if (bypassCache || !_cacheTokens || !_tokenCache.TryGetValue(BuildKey(appliesTo, action), out var entry) || entry.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return false;
            }

            token = entry.Token;
            return true;
        }

        private void TrySetIntoCache(string appliesTo, string action, bool bypassCache, string token)
        {
            if(!bypassCache && _cacheTokens)
            {
                var now = DateTime.UtcNow;
                _tokenCache[BuildKey(appliesTo, action)] = (token, now + _cacheExpirationTime);

                if (Interlocked.Increment(ref _writesSincePrune) >= PruneInterval)
                {
                    Interlocked.Exchange(ref _writesSincePrune, 0);
                    foreach (var entry in _tokenCache)
                    {
                        if (entry.Value.ExpiresAtUtc <= now)
                        {
                            _tokenCache.TryRemove(entry.Key, out _);
                        }
                    }
                }
            }
        }

        private string NormalizeAppliesTo(string appliesTo)
        {
            return NormalizeUri(appliesTo, "http", StripQueryParameters, stripPath: false, ensureTrailingSlash: true);
        }

        private string BuildKey(string appliesTo, string action)
        {
            return $"{appliesTo}_{action}";
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        public void Dispose()
        {
            _tokenCache.Clear();
        }

        private static string NormalizeUri(string uri, string scheme, bool stripQueryParameters = true, bool stripPath = false, bool ensureTrailingSlash = false)
        {
            UriBuilder uriBuilder = new UriBuilder(uri)
            {
                Scheme = scheme,
                Port = -1,
                Fragment = string.Empty,
                Password = string.Empty,
                UserName = string.Empty,
            };

            if (stripPath)
            {
                uriBuilder.Path = string.Empty;
            }

            if (stripQueryParameters)
            {
                uriBuilder.Query = string.Empty;
            }

            if (ensureTrailingSlash)
            {
                if (!uriBuilder.Path.EndsWith("/", StringComparison.Ordinal))
                {
                    uriBuilder.Path += "/";
                }
            }

            return uriBuilder.Uri.AbsoluteUri;
        }
    }
}
