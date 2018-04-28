// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.KeyVault.Secret
{
    public class SimpleTokenizer : ITokenizer
    {
        private const string DefaultFrame = "$$";

        private readonly string _frame;
        private readonly ISecretReader _secretReader;

        public SimpleTokenizer(string frame, ISecretReader secretReader)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _secretReader = secretReader ?? throw new ArgumentNullException(nameof(secretReader));
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
                        yield return new SecretToken(secretTokenValue, _secretReader);

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
    }
}
