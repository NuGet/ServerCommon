// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class SecretUtilitiesFacts
    {
        [Theory]
        [InlineData("no secrets", "$$", new string[0])]
        [InlineData("val1=$$secret1$$, val2=$$secret2$$", "$$", new[] { "secret1", "secret2" })]
        [InlineData("val1=$$secret1$$, val2=$$secret1$$", "$$", new[] { "secret1" })]
        [InlineData("val1=$$secret1$$, val2=$$secr", "$$", new[] { "secret1" })]
        [InlineData("val1=||secret1||, val2=$$secret2$$", "||", new[] { "secret1" })]
        [InlineData("val1=|secret1|, val2=$$secret2$$", "|", new[] { "secret1" })]
        [InlineData("val1=||secret1||, val2=$$secret2$$", "$$", new[] { "secret2" })]
        [InlineData("val1=$$$$, val2=$$secret2$$, val3=$$  $$", "$$", new[] { "secret2" })]
        [InlineData("val1=$$secret1$$, val2=$$secret2$$, val3=$$secret1$$", "$$", new[] { "secret1", "secret2" })]
        public void GetsSecrets(string input, string frame, IReadOnlyCollection<string> expectedList)
        {
            var result = SecretUtilities.GetSecretNames(input, frame);
            Assert.Equal(expectedList.Count(), result.Count);

            // verifying that there are no repetitions in the list
            Assert.Equal(result.Count, new HashSet<string>(result).Count);

            // both lists contain the same elements
            Assert.False(result.Except(expectedList).Any());
            Assert.False(expectedList.Except(result).Any());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void DoesNotAcceptEmptyFrame(string frame)
        {
            Assert.Throws<ArgumentException>("frame", () => SecretUtilities.GetSecretNames("some input", ""));
        }
    }
}
