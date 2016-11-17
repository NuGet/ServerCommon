// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Services.KeyVault
{
    public class Secret
    {
        public Secret(string name, string value, Dictionary<string, string> tags = null)
        {
            Name = name;
            Value = value;
            Tags = tags;
        }

        /// <summary>
        /// A unique identifier under which the secret can be accessed.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the secret.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// A set of key-value pairs that provide additional information about the secret.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
    }
}
