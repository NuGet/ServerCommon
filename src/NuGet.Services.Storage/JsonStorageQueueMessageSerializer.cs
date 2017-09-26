// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Implementation of <see cref="StorageQueueMessageSerializer{T}"/> that uses <see cref="JsonConvert"/> to serialize and deserialize.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonStorageQueueMessageSerializer<T> : StorageQueueMessageSerializer<T>
    {
        private JsonSerializerSettings _settings;

        public override string Serialize(T contents)
        {
            return JsonConvert.SerializeObject(contents, _settings);
        }

        public override T Deserialize(string contents)
        {
            return JsonConvert.DeserializeObject<T>(contents, _settings);
        }

        public JsonStorageQueueMessageSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }
    }
}