// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Moq;

namespace NuGet.Services.Sql.Tests
{
    public class MockAuthenticator : AzureSqlClientAuthenticator
    {
        public static readonly IAuthenticationResult ValidToken
            = GetAccessTokenMock("valid", DateTimeOffset.Now + TimeSpan.FromDays(1));

        public static readonly IAuthenticationResult ExpiredToken
            = GetAccessTokenMock("expired", DateTimeOffset.Now - TimeSpan.FromDays(0));

        public static readonly IAuthenticationResult NearExpiredToken
            = GetAccessTokenMock("near-expired", DateTimeOffset.Now + TimeSpan.FromMinutes(5));

        public IAuthenticationResult InitialToken { get; }

        public int AcquireTokenCounter { get; set; }

        public MockAuthenticator(
            string certificateData = "certificate",
            IAuthenticationResult initialResult = null)
            : base(certificateData)
        {
            AuthenticationResult = initialResult;
        }

        protected override Task<IAuthenticationResult> AcquireTokenFromContextAsync()
        {
            AcquireTokenCounter++;

            return Task.FromResult(ValidToken);
        }

        private static IAuthenticationResult GetAccessTokenMock(string token, DateTimeOffset expiresOn)
        {
            var mockToken = new Mock<IAuthenticationResult>();
            mockToken.Setup(m => m.AccessToken).Returns(token);
            mockToken.Setup(m => m.ExpiresOn).Returns(expiresOn);
            return mockToken.Object;
        }
    }
}
