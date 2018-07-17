// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace NuGet.Services.Sql.Tests
{
    public class MockAccessTokenCache : AccessTokenCache
    {
        public const string DefaultCertificateData = "certificateData";

        public int AcquireTokenCount { get; private set; }

        public AccessTokenCacheValue InitialValue { get; private set; }

        public AccessTokenCacheValue[] MockValues { get; }

        public MockAccessTokenCache(
            string initialCertData = DefaultCertificateData,
            string certData = DefaultCertificateData,
            IAuthenticationResult initialValue = null,
            params IAuthenticationResult[] mockTokens)
        {
            var clientCertificate = new Mock<IClientAssertionCertificate>();
            clientCertificate.Setup(c => c.RawData).Returns(certData);

            InitialValue = (initialValue == null)
                ? null
                : new AccessTokenCacheValue(clientCertificate.Object, initialValue);

            MockValues = mockTokens.Select(t => new AccessTokenCacheValue(clientCertificate.Object, t)).ToArray();
        }

        protected override bool TryGetValue(
            AzureSqlConnectionStringBuilder connectionString,
            out AccessTokenCacheValue accessToken)
        {
            var result = InitialValue;
            if (result != null)
            {
                InitialValue = null;
                accessToken = result;
                return true;
            }

            return base.TryGetValue(connectionString, out accessToken);
        }

        protected override Task<AccessTokenCacheValue> AcquireAccessTokenAsync(
            AzureSqlConnectionStringBuilder connectionString,
            string clientCertificateData)
        {
            try
            {
                if (MockValues.Length == 0)
                {
                    return null;
                }

                var tokenIndex = Math.Max(AcquireTokenCount, MockValues.Length - 1);
                return Task.FromResult(MockValues[tokenIndex]);
            }
            finally
            {
                AcquireTokenCount++;
            }
        }

        public static IAuthenticationResult CreateMockAccessToken(string value, DateTimeOffset expiresOn)
        {
            var mockToken = new Mock<IAuthenticationResult>();
            mockToken.Setup(x => x.AccessToken).Returns(value);
            mockToken.Setup(x => x.ExpiresOn).Returns(expiresOn);
            return mockToken.Object;
        }
    }
}
