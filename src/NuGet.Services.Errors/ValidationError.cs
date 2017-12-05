// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NuGet.Services.Validation;

namespace NuGet.Services.Errors
{
    public abstract class ValidationError : IValidationError
    {
        /// <summary>
        /// The map of error codes to the type that represents the error. The types MUST extend <see cref="ValidationError"/>.
        /// </summary>
        public static readonly Dictionary<ValidationErrorCode, Type> ErrorCodeTypes = new Dictionary<ValidationErrorCode, Type>
        {
            { ValidationErrorCode.PackageIsSignedError, typeof(PackageIsSignedError) },
        };

        /// <summary>
        /// The error code that this error represents.
        /// </summary>
        [JsonIgnore]
        public abstract ValidationErrorCode ErrorCode { get; }

        /// <summary>
        /// Get the message that describes this particular error.
        /// </summary>
        /// <returns>A well-formatted error message that describes this error.</returns>
        public abstract string GetMessage();

        /// <summary>
        /// Convert a <see cref="PackageValidationError"/> entity into a <see cref="ValidationError"/>.
        /// </summary>
        /// <param name="packageValidationError">The database entity that represents the error.</param>
        /// <returns>An error object that can be used to display an error message to users.</returns>
        public static ValidationError FromPackageValidationError(PackageValidationError packageValidationError)
        {
            if (!ErrorCodeTypes.TryGetValue(packageValidationError.ErrorCode, out Type deserializationType))
            {
                return UnknownError.Instance;
            }

            return (ValidationError)JsonConvert.DeserializeObject(packageValidationError.Data, deserializationType);
        }

        /// <summary>
        /// Serialize this error into a string, excluding the error code.
        /// </summary>
        /// <returns>A serialized version of this validation error.</returns>
        public virtual string Serialize() => JsonConvert.SerializeObject(this);
    }
}
