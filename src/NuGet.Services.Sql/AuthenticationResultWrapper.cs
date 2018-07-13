// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Sql
{
    public class AuthenticationResultWrapper : IAuthenticationResult
    {
        private AuthenticationResult Instance { get; }

        public AuthenticationResultWrapper(AuthenticationResult instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public string AccessToken => Instance.AccessToken;

        public DateTimeOffset ExpiresOn => Instance.ExpiresOn;

        public string TenantId => Instance.TenantId;

        public string Authority => Instance.Authority;
    }
}
