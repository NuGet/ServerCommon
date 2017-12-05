// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGet.Services.Validation.Errors
{
    public abstract class ValidationError : IValidationError
    {
        /// <summary>
        /// The map of error codes to the type that represents the error. The types MUST extend <see cref="ValidationError"/>.
        /// </summary>
        public static readonly IReadOnlyDictionary<ValidationErrorCode, Type> ErrorCodeTypes = new Dictionary<ValidationErrorCode, Type>
        {
            { ValidationErrorCode.PackageIsSignedError, GetErrorType<PackageIsSignedError>() },
        };

        /// <summary>
        /// Deserialize an error code and data string into a <see cref="ValidationError"/>.
        /// </summary>
        /// <param name="errorCode">The error code that the error represents.</param>
        /// <param name="data">The error's serialized data, as serialized by <see cref="Serialize"/>.</param>
        /// <returns>An error object that can be used to display an error message to users.</returns>
        public static ValidationError Deserialize(ValidationErrorCode errorCode, string data)
        {
            if (!ErrorCodeTypes.TryGetValue(errorCode, out Type deserializationType))
            {
                return new UnknownError();
            }

            var result = JsonConvert.DeserializeObject(data, deserializationType);

            if (result == null)
            {
                throw new ArgumentException($"Cannot deserialize to {deserializationType} as the data is malformed", nameof(data));
            }

            return (ValidationError)result;
        }

        /// <summary>
        /// Get the <see cref="Type"/> of a <see cref="ValidationError"/>. Used to populate <see cref="ErrorCodeTypes"/>.
        /// </summary>
        /// <typeparam name="T">The compile-time type whose runtime type should be fetched.</typeparam>
        /// <returns>The error's runtime type.</returns>
        private static Type GetErrorType<T>() where T : ValidationError => typeof(T);

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
        /// Serialize this error into a string, excluding the error code.
        /// </summary>
        /// <returns>A serialized version of this validation error.</returns>
        public virtual string Serialize() => JsonConvert.SerializeObject(this);
    }
}
