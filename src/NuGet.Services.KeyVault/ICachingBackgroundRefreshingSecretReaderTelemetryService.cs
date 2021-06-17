// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public interface ICachingBackgroundRefreshingSecretReaderTelemetryService
    {
        void TrackExpiredSecretRequested(string secretName);
        void TrackUnknownSecretRequested(string secretName);
        void TrackSecretRefreshFailure(string secretName);
        void TrackSecretRefreshed(string secretName);
        void TrackSecretRefreshIteration(bool success);
        void TrackBackgroundRefreshTaskLeakedException();
    }
}
