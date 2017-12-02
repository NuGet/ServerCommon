// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using NuGet.Services.Validation;

namespace NuGet.Services.Errors
{
    public abstract class ValidationError : IValidationError
    {
        [JsonIgnore]
        public abstract ValidationErrorCode ErrorCode { get; }

        public abstract string GetMessage();

        public virtual string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
