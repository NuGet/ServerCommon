﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NuGet.Services.Owin;

namespace Owin
{
    public static class ForceSslMiddlewareExtensions
    {
        public static IAppBuilder UseForceSsl(this IAppBuilder appBuilder)
        {
            return UseForceSsl(appBuilder, 443);
        }

        public static IAppBuilder UseForceSsl(this IAppBuilder appBuilder, int sslPort)
        {
            return appBuilder.Use<ForceSslMiddleware>(sslPort);
        }

        public static IAppBuilder UseForceSsl(this IAppBuilder appBuilder, int sslPort, IEnumerable<Regex> excludedPathPatterns)
        {
            return appBuilder.Use<ForceSslMiddleware>(sslPort, excludedPathPatterns);
        }
    }
}
