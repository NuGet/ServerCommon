// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Collections.Generic;

namespace NuGet.Services.Validation
{
    public class ValidationResult : IValidationResult
    {
        private static readonly IValidationResult notStarted = new ValidationResult(ValidationStatus.NotStarted);
        private static readonly IValidationResult incomplete = new ValidationResult(ValidationStatus.Incomplete);
        private static readonly IValidationResult succeeded = new ValidationResult(ValidationStatus.Succeeded);
        private static readonly IValidationResult failed = new ValidationResult(ValidationStatus.Failed);

        /// <summary>
        /// Represents a validation result that has not been started.
        /// </summary>
        public static IValidationResult NotStarted => notStarted;

        /// <summary>
        /// Represents a validation result that has started but not succeeded or failed yet.
        /// </summary>
        public static IValidationResult Incomplete => incomplete;

        /// <summary>
        /// A successful validation result.
        /// </summary>
        public static IValidationResult Succeeded => succeeded;

        /// <summary>
        /// A failed validation result with no errors.
        /// </summary>
        public static IValidationResult Failed => failed;

        /// <summary>
        /// Create a new validation result with the given status.
        /// </summary>
        /// <param name="status">The result's status.</param>
        public ValidationResult(ValidationStatus status)
        {
            Status = status;
            Errors = new string[0];
        }

        /// <summary>
        /// Create a new failed validation result with the given errors.
        /// </summary>
        /// <param name="errors">The errors that detail why the validation failed.</param>
        public ValidationResult(params string[] errors)
        {
            Status = ValidationStatus.Failed;
            Errors = errors ?? new string[0];
        }

        /// <summary>
        /// The status of the validation.
        /// </summary>
        public ValidationStatus Status { get; }

        /// <summary>
        /// The errors that were encountered if the validation failed.
        /// </summary>
        public IEnumerable<string> Errors { get; }
    }
}
