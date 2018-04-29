// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using NuGet.Services.KeyVault.Secret;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class VerbatimStringTokenFacts
    {
        [Fact]
        public void ThrowsWhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VerbatimStringToken(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        [InlineData("abc")]
        [InlineData("containing space")]
        [InlineData("key=value;anotherkey=anothervalue")]
        public async Task EchoesWhateverWasPassedIn(string value)
        {
            var token = new VerbatimStringToken(value);

            var result = await token.ProcessAsync();
            Assert.Equal(value, result);
        }
    }
}
