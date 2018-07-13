﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Sql
{
    public interface IAuthenticationResult
    {
        string AccessToken { get; }

        DateTimeOffset ExpiresOn { get; }

        string TenantId { get; }

        string Authority { get; }
    }
}
