// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace NuGet.Services.Storage
{
    public abstract class Storage : IStorage
    {
        protected readonly ILogger<Storage> Logger;

        public Storage(Uri baseAddress, ILogger<Storage> logger)
        {
            string s = baseAddress.OriginalString.TrimEnd('/') + '/';
            BaseAddress = new Uri(s);
            Logger = logger;
        }

        public override string ToString()
        {
            return BaseAddress.ToString();
        }

        protected abstract Task OnSave(Uri resourceUri, StorageContent content, CancellationToken cancellationToken);
        protected abstract Task OnSaveIfETag(Uri resourceUri, StorageContent content, string eTag, CancellationToken cancellationToken);
        protected abstract Task<StorageContent> OnLoad(Uri resourceUri, CancellationToken cancellationToken);
        protected abstract Task OnDelete(Uri resourceUri, CancellationToken cancellationToken);
        protected abstract Task OnDeleteIfETag(Uri resourceUri, string eTag, CancellationToken cancellationToken);

        public async Task Save(Uri resourceUri, StorageContent content, CancellationToken cancellationToken)
        {
            SaveCount++;

            TraceMethod(nameof(Save), resourceUri);

            try
            {
                await OnSave(resourceUri, content, cancellationToken);
            }
            catch (Exception e)
            {
                throw TraceException(nameof(Save), resourceUri, e);
            }
        }

        public async Task SaveIfETag(Uri resourceUri, StorageContent content, string eTag, CancellationToken cancellationToken)
        {
            SaveCount++;

            TraceMethod(nameof(SaveIfETag), resourceUri);

            try
            {
                await OnSaveIfETag(resourceUri, content, eTag, cancellationToken);
            }
            catch (Exception e)
            {
                throw TraceException(nameof(SaveIfETag), resourceUri, e);
            }
        }

        public async Task<StorageContent> Load(Uri resourceUri, CancellationToken cancellationToken)
        {
            LoadCount++;

            TraceMethod(nameof(Load), resourceUri);

            try
            {
                return await OnLoad(resourceUri, cancellationToken);
            }
            catch (Exception e)
            {
                throw TraceException(nameof(Load), resourceUri, e);
            }
        }

        public async Task Delete(Uri resourceUri, CancellationToken cancellationToken)
        {
            DeleteCount++;

            TraceMethod(nameof(Delete), resourceUri);

            try
            {
                await OnDelete(resourceUri, cancellationToken);
            }
            catch (StorageException e)
            {
                WebException webException = e.InnerException as WebException;
                if (webException != null)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)webException.Response).StatusCode;
                    if (statusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                throw TraceException(nameof(Delete), resourceUri, e);
            }
        }

        public async Task DeleteIfETag(Uri resourceUri, string eTag, CancellationToken cancellationToken)
        {
            DeleteCount++;

            TraceMethod(nameof(DeleteIfETag), resourceUri);

            try
            {
                await OnDeleteIfETag(resourceUri, eTag, cancellationToken);
            }
            catch (StorageException e)
            {
                WebException webException = e.InnerException as WebException;
                if (webException != null)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)webException.Response).StatusCode;
                    if (statusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                throw TraceException(nameof(DeleteIfETag), resourceUri, e);
            }
        }

        public async Task<string> LoadString(Uri resourceUri, CancellationToken cancellationToken)
        {
            StorageContent content = await Load(resourceUri, cancellationToken);
            if (content == null)
            {
                return null;
            }
            else
            {
                using (Stream stream = content.GetContentStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public Uri BaseAddress { get; protected set; }
        public abstract bool Exists(string fileName);
        public abstract Task<IEnumerable<StorageListItem>> List(CancellationToken cancellationToken);

        public bool Verbose
        {
            get;
            set;
        }

        public int SaveCount
        {
            get;
            protected set;
        }

        public int LoadCount
        {
            get;
            protected set;
        }

        public int DeleteCount
        {
            get;
            protected set;
        }

        public void ResetStatistics()
        {
            SaveCount = 0;
            LoadCount = 0;
            DeleteCount = 0;
        }

        public Uri ResolveUri(string relativeUri)
        {
            return new Uri(BaseAddress, relativeUri);
        }

        protected string GetName(Uri uri)
        {
            var address = Uri.UnescapeDataString(BaseAddress.GetLeftPart(UriPartial.Path));
            if (!address.EndsWith("/"))
            {
                address += "/";
            }
            var uriString = uri.ToString();

            int baseAddressLength = address.Length;

            var name = uriString.Substring(baseAddressLength);
            if (name.Contains("#"))
            {
                name = name.Substring(0, name.IndexOf("#"));
            }
            return name;
        }

        protected Uri GetUri(string name)
        {
            string address = BaseAddress.ToString();
            if (!address.EndsWith("/"))
            {
                address += "/";
            }
            address += name.Replace("\\", "/").TrimStart('/');

            return new Uri(address);
        }

        protected void TraceMethod(string method, Uri resourceUri)
        {
            if (Verbose)
            {
                Logger.LogInformation("{Method} {ResourceUri}", method, resourceUri);
            }
        }

        protected Exception TraceException(string method, Uri resourceUri, Exception exception)
        {
            string message = String.Format("{Method} EXCEPTION: {0} {1}", method, resourceUri, exception.Message);
            Logger.LogError("{Method} EXCEPTION: {ResourceUri} {Exception}", method, resourceUri, exception);
            return new Exception(message, exception);
        }
    }
}