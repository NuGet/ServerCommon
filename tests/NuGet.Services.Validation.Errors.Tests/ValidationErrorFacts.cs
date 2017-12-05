// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace NuGet.Services.Validation.Errors.Tests
{
    public class ValidationErrorFacts
    {
        private static string PackageIsSignedSerializedError => GetSerializedTestData(ValidationErrorCode.PackageIsSignedError);

        [Fact]
        public void TheErrorCodeTypesPropertyValuesAllExtendValidationError()
        {
            Assert.True(ValidationError.ErrorCodeTypes.Values.All(t => t.IsSubclassOf(typeof(ValidationError))));
        }

        public class TheSerializeMethod
        {
            [Fact]
            public void UnknownSerialization()
            {
                // Arrange
                var error = new UnknownError();
                var result = error.Serialize();

                // Assert
                Assert.Equal("{}", result);
            }

            [Fact]
            public void PackageIsSignedSerialization()
            {
                // Arrange
                var signedError = new PackageIsSignedError("Hello.World", "1.3.4");
                var result = signedError.Serialize();

                // Assert
                Assert.Equal(PackageIsSignedSerializedError, result);
            }
        }

        public class TheDeserializeMethod
        {
            [Fact]
            public void UnknownDeserialization()
            {
                // Arrange & Act
                var validationError = CreatePackageValidationError(ValidationErrorCode.Unknown, "{}");
                var result = ValidationError.Deserialize(validationError.ErrorCode, validationError.Data) as UnknownError;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(ValidationErrorCode.Unknown, result.ErrorCode);
                Assert.Equal("Package validation failed due to an unknown error.", result.GetMessage());
            }

            [Fact]
            public void InvalidDeserialization()
            {
                // Arrange & Act & Assert
                var validationError = CreatePackageValidationError(ValidationErrorCode.PackageIsSignedError, "HELLO THIS IS DOG");

                Assert.Throws<JsonReaderException>(() => ValidationError.Deserialize(validationError.ErrorCode, validationError.Data));
            }

            [Fact]
            public void PackageIsSignedDeserialization()
            {
                // Arrange & Act
                var validationError = CreatePackageValidationError(ValidationErrorCode.PackageIsSignedError, PackageIsSignedSerializedError);
                var result = ValidationError.Deserialize(validationError.ErrorCode, validationError.Data) as PackageIsSignedError;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(ValidationErrorCode.PackageIsSignedError, result.ErrorCode);
                Assert.Equal("Hello.World", result.PackageId);
                Assert.Equal("1.3.4", result.PackageVersion);
                Assert.Equal("Package Hello.World 1.3.4 is signed.", result.GetMessage());
            }

            private PackageValidationError CreatePackageValidationError(ValidationErrorCode errorCode, string data)
            {
                return new PackageValidationError
                {
                    ErrorCode = errorCode,
                    Data = data
                };
            }
        }

        private static string GetSerializedTestData(ValidationErrorCode errorCode)
        {
            return File.ReadAllText(Path.Combine("Data", $"{errorCode}.json"));
        }
    }
}
