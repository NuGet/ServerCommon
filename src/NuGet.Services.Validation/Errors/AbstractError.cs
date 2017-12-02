// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace NuGet.Services.Validation
{
    public abstract class AbstractError
    {
        /// <summary>
        /// The code that identifies this error.
        /// </summary>
        [JsonIgnore]
        public abstract ValidationErrorCode ErrorCode { get; }

        /// <summary>
        /// The message that explains the error.
        /// </summary>
        [JsonIgnore]
        public abstract string ErrorMessage { get; }
    }
}