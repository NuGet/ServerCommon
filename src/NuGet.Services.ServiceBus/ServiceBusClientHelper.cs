// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace NuGet.Services.ServiceBus
{
    internal static class ServiceBusClientHelper
    {
        public static ServiceBusClient GetServiceBusClient(string connectionString, string managedIdentityClientId)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return connectionString.Contains(Constants.SharedAccessKeytoken)
                ? new ServiceBusClient(connectionString)
                : new ServiceBusClient(connectionString, new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = managedIdentityClientId
                }));
        }

        /// <summary>
        /// Source: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample08_Interop.md#sending-a-message-using-azuremessagingservicebus-that-will-be-received-with-windowsazureservicebus
        /// </summary>
        public static byte[] SerializeXmlDataContract<T>(T body)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    serializer.WriteObject(writer, body);
                    writer.Flush();
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Source: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample08_Interop.md#receiving-a-message-using-azuremessagingservice-that-was-sent-with-windowsazureservicebus
        /// </summary>
        public static T DeserializeXmlDataContract<T>(BinaryData body)
        {
            var deserializer = new DataContractSerializer(typeof(T));
            using (var stream = body.ToStream())
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return (T)deserializer.ReadObject(reader);
            }
        }
    }
}
