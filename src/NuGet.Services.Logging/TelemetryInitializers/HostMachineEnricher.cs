// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Logging
{
    public class HostMachineEnricher : SupportPropertiesTelemetryInitializer
    {
        public HostMachineEnricher(string hostName)
            : base("HostName", hostName)
        {
        }
    }
}
