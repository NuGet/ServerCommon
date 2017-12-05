// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.Services.Validation;

namespace NuGet.Services.Errors
{
    /// <summary>
    /// A placeholder error when the specific error is unknown.
    /// </summary>
    public class UnknownError : ValidationError
    {
        public static UnknownError Instance = new UnknownError();

        private UnknownError()
        {
        }

        public override ValidationErrorCode ErrorCode => ValidationErrorCode.Unknown;

        public override string GetMessage() => "Package validation failed due to an unknown error.";
    }
}
