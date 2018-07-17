// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Sql
{
    public class AccessTokenCacheValue
    {
        public AccessTokenCacheValue(ClientAssertionCertificate certificate, AuthenticationResult authenticationResult)
        {
            ClientAssertionCertificate = new ClientAssertionCertificateWrapper(certificate);
            AuthenticationResult = new AuthenticationResultWrapper(authenticationResult);
        }

        public AccessTokenCacheValue(IClientAssertionCertificate certificate, IAuthenticationResult authenticationResult)
        {
            ClientAssertionCertificate = certificate;
            AuthenticationResult = authenticationResult;
        }

        public IClientAssertionCertificate ClientAssertionCertificate { get; }

        public IAuthenticationResult AuthenticationResult { get; }
    }
}