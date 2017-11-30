// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// A validation error that failed an asynchronous validation.
    /// </summary>
    public interface IValidationError
    {
        /// <summary>
        /// The code that classifies the error.
        /// </summary>
        ValidationErrorCode ErrorCode { get; }

        /// <summary>
        /// The values that can be used to format the error message.
        /// </summary>
        IReadOnlyDictionary<string, string> Arguments { get; }
    }
}