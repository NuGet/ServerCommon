// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Test.Server;
using Test.Utility;

namespace NuGet.Services.Killswitch.Tests
{
    // Based off of https://github.com/NuGet/NuGet.Client/blob/dev/test/TestUtilities/Test.Utility/Signing/SigningTestServer.cs
    public class KillswitchTestServer
    {
        public const string DefaultResponse = "[]";

        private readonly HttpListener _listener;
        private bool _isDisposed;

        private KillswitchTestServer(HttpListener listener, Uri url)
        {
            _listener = listener;

            Url = url;
            Response = DefaultResponse;
        }

        public Uri Url { get; }

        public string Response { get; set; }

        public static Task<KillswitchTestServer> CreateAsync()
        {
            var portReserver = new PortReserver();

            return portReserver.ExecuteAsync(CreateInternalAsync, CancellationToken.None);
        }

        private static Task<KillswitchTestServer> CreateInternalAsync(int port, CancellationToken token)
        {
            var url = new Uri($"http://127.0.0.1:{port}/");
            var httpListener = new HttpListener();

            httpListener.IgnoreWriteExceptions = true;
            httpListener.Prefixes.Add(url.OriginalString);
            httpListener.Start();

            var server = new KillswitchTestServer(httpListener, url);

            using (var taskStartedEvent = new ManualResetEventSlim())
            {
                Task.Factory.StartNew(() => server.HandleRequest(taskStartedEvent, token));

                taskStartedEvent.Wait(token);
            }

            return Task.FromResult(server);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _listener.Stop();
                _listener.Abort();

                GC.SuppressFinalize(this);

                _isDisposed = true;
            }
        }

        private void HandleRequest(ManualResetEventSlim mutex, CancellationToken cancellationToken)
        {
            mutex.Set();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = _listener.GetContext();

                    byte[] responseBuffer = Encoding.UTF8.GetBytes(Response);

                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength64 = responseBuffer.Length;

                    using (var writer = new BinaryWriter(context.Response.OutputStream))
                    {
                        writer.Write(responseBuffer);
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == ErrorConstants.ERROR_OPERATION_ABORTED ||
                        ex.ErrorCode == ErrorConstants.ERROR_INVALID_HANDLE ||
                        ex.ErrorCode == ErrorConstants.ERROR_INVALID_FUNCTION ||
                        RuntimeEnvironmentHelper.IsMono && ex.ErrorCode == ErrorConstants.ERROR_OPERATION_ABORTED_MONO)
                    {
                        return;
                    }

                    Console.WriteLine($"Unexpected error code: {ex.ErrorCode}. Exception: {ex}");

                    throw;
                }
            }
        }
    }
}
