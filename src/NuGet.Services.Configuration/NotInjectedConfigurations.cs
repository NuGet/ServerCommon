// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.Configuration
{
    public static class NotInjectedConfigurations
    {
        public static readonly HashSet<string> Keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "GalleryDb:ConnectionString",
            "ValidationDb:ConnectionString",
            "SupportRequestDb:ConnectionString",
            "StatisticsDb:ConnectionString",
            };
    }
}
