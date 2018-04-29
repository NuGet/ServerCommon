// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.KeyVault.Secret
{
    public class SimpleTokenizer : ITokenizer
    {
        private const string DefaultFrame = "$$";
        private const string DefaultSeparator = "|";
        private const string UrlEncodingTokenType = "urlencode";
        private const string RecursiveTokenType = "recurse";
        private readonly string _frame;
        private readonly string _separator;
        private readonly ISecretReader _secretReader;
        private readonly ISecretInjector _secretInjector;

        public SimpleTokenizer(ISecretReader secretReader, ISecretInjector secretInjector)
            : this(DefaultFrame, DefaultSeparator, secretReader, secretInjector)
        {
        }

        public SimpleTokenizer(string frame, string separator, ISecretReader secretReader, ISecretInjector secretInjector)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _separator = separator ?? throw new ArgumentNullException(nameof(separator));
            _secretReader = secretReader ?? throw new ArgumentNullException(nameof(secretReader));
            _secretInjector = secretInjector ?? throw new ArgumentNullException(nameof(secretInjector));
        }

        public IEnumerable<IToken> Tokenize(string input)
        {
            int verbatimStartIndex = 0;
            int foundIndex;
            bool insideFrame = false;
            int frameStartIndex = 0;

            while ((foundIndex = input.IndexOf(_frame, frameStartIndex, StringComparison.InvariantCulture)) >= 0)
            {
                if (insideFrame)
                {
                    var secretTokenValue = input.Substring(frameStartIndex, foundIndex - frameStartIndex);
                    if (!string.IsNullOrWhiteSpace(secretTokenValue))
                    {
                        var precedingVerbatimText = input.Substring(verbatimStartIndex, frameStartIndex - verbatimStartIndex - _frame.Length);
                        // keep whitespace in verbatim text
                        if (!string.IsNullOrEmpty(precedingVerbatimText))
                        {
                            yield return new VerbatimStringToken(precedingVerbatimText);
                        }
                        yield return CreateNonVerbatimToken(secretTokenValue);

                        insideFrame = false;
                        verbatimStartIndex = foundIndex + _frame.Length;
                        frameStartIndex = verbatimStartIndex;
                    }
                    else
                    {
                        // if what we found does not look like a secret name, advance the frame
                        // start index to current position
                        frameStartIndex = foundIndex + _frame.Length;
                    }
                }
                else
                {
                    frameStartIndex = foundIndex + _frame.Length;
                    insideFrame = true;
                }
            }
            var trailingVerbatimText = input.Substring(verbatimStartIndex);
            if (!string.IsNullOrEmpty(trailingVerbatimText))
            {
                yield return new VerbatimStringToken(trailingVerbatimText);
            }
        }

        private IToken CreateNonVerbatimToken(string name)
        {
            var separatorIndex = name.IndexOf(_separator);
            if (separatorIndex >= 0)
            {
                var tokenType = name.Substring(0, separatorIndex);
                var secretName = name.Substring(separatorIndex + 1);

                if (!string.IsNullOrWhiteSpace(tokenType))
                {
                    switch (tokenType)
                    {
                    case UrlEncodingTokenType:
                        return new UrlEncodingToken(secretName, _secretReader);
                    case RecursiveTokenType:
                        return new RecursiveToken(secretName, _secretReader, _secretInjector);
                    }
                }
            }
            return new SecretToken(name, _secretReader);
        }
    }
}
