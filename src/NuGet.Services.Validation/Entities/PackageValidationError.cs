// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// An error found by a <see cref="PackageValidation" /> that should be displayed to the user.
    /// </summary>
    public class PackageValidationError
    {
        /// <summary>
        /// The unique key that identifies this error.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The key to the <see cref="PackageValidation"/> that found this error.
        /// </summary>
        public Guid PackageValidationKey { get; set; }

        /// <summary>
        /// The code that this error represents. The NuGet Gallery should map this error
        /// to an actual error message using this code.
        /// </summary>
        public ValidationErrorCode ErrorCode { get; set; }

        /// <summary>
        /// The error message's serialized data. This is used to store <see cref="Data"/> in the
        /// database and is NOT intended to be used directly.
        /// </summary>
        [Column("Data")]
        public string DataAsJson {
            get => JsonConvert.SerializeObject(Data);

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Data = new Dictionary<string, string>();
                }
                else
                {
                    Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                }
            }
        }

        /// <summary>
        /// The <see cref="PackageValidation"/> that found this error.
        /// </summary>
        public PackageValidation PackageValidation { get; set; }

        /// <summary>
        /// The error message's data.
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Data { get; set; }
    }
}
