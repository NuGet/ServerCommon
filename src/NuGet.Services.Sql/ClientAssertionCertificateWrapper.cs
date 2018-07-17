// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Sql
{
    public class ClientAssertionCertificateWrapper : IClientAssertionCertificate
    {
        private ClientAssertionCertificate Instance { get; }

        public ClientAssertionCertificateWrapper(ClientAssertionCertificate instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public string RawData
        {
            get
            {
                return Convert.ToBase64String(Instance.Certificate.RawData);
            }
        }
    }
}