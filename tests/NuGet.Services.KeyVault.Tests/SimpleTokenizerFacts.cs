// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NuGet.Services.KeyVault.Secret;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class SimpleTokenizerFacts
    {
        public static IEnumerable<object[]> InputToTokenMap => new []
        {
            new object[]{ "", Tokens() },
            new object[]{ "a", Tokens(Verbatim("a")) },
            new object[]{ "aa", Tokens(Verbatim("aa")) },
            new object[]{ "aaa", Tokens(Verbatim("aaa")) },
            new object[]{ "a b", Tokens(Verbatim("a b")) },
            new object[]{ "a bb", Tokens(Verbatim("a bb")) },
            new object[]{ "a bbb", Tokens(Verbatim("a bbb")) },
            new object[]{ "aa b", Tokens(Verbatim("aa b")) },
            new object[]{ "aa bb", Tokens(Verbatim("aa bb")) },
            new object[]{ "aa bbb", Tokens(Verbatim("aa bbb")) },
            new object[]{ "aaa b", Tokens(Verbatim("aaa b")) },
            new object[]{ "aaa bb", Tokens(Verbatim("aaa bb")) },
            new object[]{ "aaa bbb", Tokens(Verbatim("aaa bbb")) },
            new object[]{ "$$a$$", Tokens(Secret("a")) },
            new object[]{ "$$$a$$", Tokens(Secret("$a")) },                 // yes, $a is the secret name
            new object[]{ "$$$a$$$", Tokens(Secret("$a"), Verbatim("$")) }, // same as above
            new object[]{ "$$aa$$", Tokens(Secret("aa")) },
            new object[]{ "$$aaa$$", Tokens(Secret("aaa")) },
            new object[]{ "$$ $$a$$", Tokens(Verbatim("$$ "), Secret("a")) },
            new object[]{ "a$$b$$", Tokens(Verbatim("a"), Secret("b")) },
            new object[]{ "a$$bb$$", Tokens(Verbatim("a"), Secret("bb")) },
            new object[]{ "a$$bbb$$", Tokens(Verbatim("a"), Secret("bbb")) },
            new object[]{ "aa$$b$$", Tokens(Verbatim("aa"), Secret("b")) },
            new object[]{ "aa$$bb$$", Tokens(Verbatim("aa"), Secret("bb")) },
            new object[]{ "aa$$bbb$$", Tokens(Verbatim("aa"), Secret("bbb")) },
            new object[]{ "aaa$$b$$", Tokens(Verbatim("aaa"), Secret("b")) },
            new object[]{ "aaa$$bb$$", Tokens(Verbatim("aaa"), Secret("bb")) },
            new object[]{ "aaa$$bbb$$", Tokens(Verbatim("aaa"), Secret("bbb")) },
            new object[]{ "$$a$$$$$b$$", Tokens(Secret("a"), Secret("$b")) },
            new object[]{ "$$a$$$$b$$", Tokens(Secret("a"), Secret("b")) },
            new object[]{ "$$a$$$$bb$$", Tokens(Secret("a"), Secret("bb")) },
            new object[]{ "$$a$$$$bbb$$", Tokens(Secret("a"), Secret("bbb")) },
            new object[]{ "$$aa$$$$b$$", Tokens(Secret("aa"), Secret("b")) },
            new object[]{ "$$aa$$$$bb$$", Tokens(Secret("aa"), Secret("bb")) },
            new object[]{ "$$aa$$$$bbb$$", Tokens(Secret("aa"), Secret("bbb")) },
            new object[]{ "$$aaa$$$$b$$", Tokens(Secret("aaa"), Secret("b")) },
            new object[]{ "$$aaa$$$$bb$$", Tokens(Secret("aaa"), Secret("bb")) },
            new object[]{ "$$aaa$$$$bbb$$", Tokens(Secret("aaa"), Secret("bbb")) },
            new object[]{ "aaa$$bbb", Tokens(Verbatim("aaa$$bbb")) },
            new object[]{ "aaa$$bbb$$ccc", Tokens(Verbatim("aaa"), Secret("bbb"), Verbatim("ccc")) },
            new object[]{ "aaa$$bbb$$ccc$$ddd$$", Tokens(Verbatim("aaa"), Secret("bbb"), Verbatim("ccc"), Secret("ddd")) },
            new object[]{ "$", Tokens(Verbatim("$")) },
            new object[]{ "$$", Tokens(Verbatim("$$")) },
            new object[]{ "$$$", Tokens(Verbatim("$$$")) },
            new object[]{ "$$$$", Tokens(Verbatim("$$$$")) },
            new object[]{ "$$$$$", Tokens(Verbatim("$$$$$")) },
            new object[]{ "$$$$$$", Tokens(Verbatim("$$$$$$")) },
            new object[]{ "Server=tcp:myserver.database.windows.net,1433;Database=myDB;User ID=$$secret1$$;Password=$$secret2$$;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                Tokens(
                    Verbatim("Server=tcp:myserver.database.windows.net,1433;Database=myDB;User ID="),
                    Secret("secret1"),
                    Verbatim(";Password="),
                    Secret("secret2"),
                    Verbatim(";Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))},
        };

        [Theory]
        [MemberData(nameof(InputToTokenMap))]
        public void ProducesCorrectTokenSequences(string inputString, IEnumerable<BaseToken> expectedTokens)
        {
            var response = _subject.Tokenize(inputString).OfType<BaseToken>().ToList();
            Assert.Equal(expectedTokens, response, new BaseTokenEqualityComparer());
        }

        private static Mock<ISecretReader> _secretReader = new Mock<ISecretReader>();
        private SimpleTokenizer _subject;

        public SimpleTokenizerFacts()
        {
            _subject = new SimpleTokenizer("$$", _secretReader.Object);
        }

        private static BaseToken[] Tokens(params BaseToken[] tokens)
            => tokens;

        private static VerbatimStringToken Verbatim(string value)
            => new VerbatimStringToken(value);

        private static SecretToken Secret(string value)
            => new SecretToken(value, _secretReader.Object);

        private class BaseTokenEqualityComparer : IEqualityComparer<BaseToken>
        {
            public bool Equals(BaseToken x, BaseToken y)
                => x.GetType() == y.GetType() && x.Value == y.Value;

            public int GetHashCode(BaseToken obj)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
