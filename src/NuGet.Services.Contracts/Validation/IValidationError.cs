// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Validation
{
    /// <summary>
    /// A validation error that failed an asynchronous validation.
    /// </summary>
    public interface IValidationError
    {
        /// <summary>
        /// The code that classifies this error.
        /// </summary>
        ValidationErrorCode ErrorCode { get; }

        /// <summary>
        /// Serialize the contents of this validation error.
        /// </summary>
        /// <returns>A string containing this error's serialized contents, excluding the error code.</returns>
        string Serialize();

        /// <summary>
        /// Get a message that describes this error. Used to display this error to end users.
        /// </summary>
        /// <returns>A well formatted message that describes this error.</returns>
        string GetMessage();
    }
}