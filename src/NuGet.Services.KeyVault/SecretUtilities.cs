// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public static class SecretUtilities
    {
        public static ICollection<string> GetSecretNames(string input, string frame)
        {
            var secretNames = new HashSet<string>();

            int startIndex = 0;
            int foundIndex;
            bool insideFrame = false;

            do
            {
                foundIndex = input.IndexOf(frame, startIndex, StringComparison.InvariantCulture);

                if (insideFrame && foundIndex > 0)
                {
                    var secret = input.Substring(startIndex, foundIndex - startIndex);
                    if (!string.IsNullOrWhiteSpace(secret))
                    {
                        secretNames.Add(secret);
                    }

                    insideFrame = false;
                }
                else
                {
                    insideFrame = true;
                }

                startIndex = foundIndex + frame.Length;

            } while (foundIndex >= 0);

            return secretNames;
        }
    }
}
