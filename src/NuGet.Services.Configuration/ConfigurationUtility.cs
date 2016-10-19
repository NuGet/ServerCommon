// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace NuGet.Services.Configuration
{
    public static class ConfigurationUtility
    {
        public static T ConvertFromString<T>(string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                // This will throw a NotSupportedException if no conversion is possible.
                return (T)converter.ConvertFromString(value);
            }
            // If there is no converter, no conversion is possible, so throw a NotSupportedException.
            throw new NotSupportedException("No converter exists from string to " + typeof(T).Name + "!");
        }
    }
}
