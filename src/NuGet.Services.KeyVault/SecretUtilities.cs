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
        /// <summary>
        /// Searches the string for references to secrets.
        /// </summary>
        /// <param name="input">The string to search in.</param>
        /// <param name="frame">Symbols that indicate the beginning and the end of a secret reference. See <see cref="SecretInjector.DefaultFrame"/>.</param>
        /// <returns>The list of secret names found in <paramref name="input"/> in no particular order.</returns>
        public static ICollection<string> GetSecretNames(string input, string frame)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            if (string.IsNullOrWhiteSpace(frame))
            {
                throw new ArgumentException($"{nameof(frame)} must not be empty", nameof(frame));
            }

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
