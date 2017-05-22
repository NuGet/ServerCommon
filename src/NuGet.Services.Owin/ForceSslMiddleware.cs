// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace NuGet.Services.Owin
{
    public class ForceSslMiddleware : OwinMiddleware
    {
        private readonly int _sslPort;
        private readonly List<Regex> _exclusionPathPatterns;

        public ForceSslMiddleware(OwinMiddleware next, int sslPort)
            : this(next, sslPort, Enumerable.Empty<Regex>())
        {
        }

        public ForceSslMiddleware(
            OwinMiddleware next, 
            int sslPort, 
            IEnumerable<Regex> exclusionPathPatterns)
            : base(next ?? throw new ArgumentNullException(nameof(next)))
        {
            _sslPort = sslPort;
            _exclusionPathPatterns = exclusionPathPatterns.ToList();
        }

        public override async Task Invoke(IOwinContext context)
        {
            bool shouldPassThrough = context.Request.IsSecure 
                || (context.Request.Path.HasValue && IsExcludedPath(context.Request.Path.Value));
            if (shouldPassThrough)
            {
                await Next.Invoke(context);
            }
            else
            {
                if (IsAllowedMethod(context.Request.Method))
                {
                    context.Response.Redirect(new UriBuilder(context.Request.Uri)
                    {
                        Scheme = Uri.UriSchemeHttps,
                        Port = _sslPort
                    }.Uri.AbsoluteUri);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ReasonPhrase = "SSL Required";
                }
            }
        }

        private bool IsExcludedPath(string path)
        {
            return _exclusionPathPatterns.Any(p => p.IsMatch(path));
        }

        private static bool IsAllowedMethod(string method)
        {
            return method == HttpMethod.Get.Method || method == HttpMethod.Head.Method;
        }
    }
}
