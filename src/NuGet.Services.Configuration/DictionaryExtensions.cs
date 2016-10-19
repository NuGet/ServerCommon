// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Services.Configuration
{
    public static class DictionaryExtensions
    {
        public static string TryGetValue(this IDictionary<string, string> dictionary, string key)
        {
            string value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static T? TryGetValue<T>(this IDictionary<string, string> dictionary, string key) where T : struct
        {
            string valueString;
            if (!dictionary.TryGetValue(key, out valueString))
            {
                return null;
            }

            try
            {
                return ConfigurationUtility.ConvertFromString<T>(valueString);
            }
            catch
            {
                return null;
            }
        }

        public static T Get<T>(this IDictionary<string, string> dictionary, string key) where T : struct
        {
            return ConfigurationUtility.ConvertFromString<T>(dictionary[key]);
        }
    }
}
