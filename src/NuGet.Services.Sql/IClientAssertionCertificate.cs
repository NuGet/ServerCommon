// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Sql
{
    public interface IClientAssertionCertificate
    {
        ClientAssertionCertificate Instance { get; }

        string GetRawData();
    }
}
