// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.WindowsAzure.Storage.Queue;

namespace NuGet.Services.Storage
{
    internal class AzureStorageQueueMessage : IStorageQueueMessage
    {
        public string Contents { get; }

        internal CloudQueueMessage Message { get; }

        internal AzureStorageQueueMessage(CloudQueueMessage message)
        {
            Message = message;
            Contents = Message.AsString;
        }

        internal AzureStorageQueueMessage(string contents)
        {
            Message = new CloudQueueMessage(contents);
            Contents = contents;
        }
    }
}