// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using NuGet.Services.Validation;
using Xunit;

namespace NuGet.Services.Errors.Tests
{
    public class ValidationErrorFacts
    {
        [Fact]
        public void TheErrorCodeTypesPropertyValuesAllExtendValidationError()
        {
            Assert.True(ValidationError.ErrorCodeTypes.Values.All(t => t.IsSubclassOf(typeof(ValidationError))));
        }

        [Fact]
        public void TheSerializeMethodShouldSerializeAllPublicPropertiesExceptTheErrorCode()
        {
            // Arrange
            var signedError = new PackageIsSignedError("Hello.World", "1.3.4");
            var result = signedError.Serialize();

            // Assert
            Assert.Equal("{\"PackageId\":\"Hello.World\",\"PackageVersion\":\"1.3.4\"}", result);
        }

        public class TheDeserializeMethod
        {
            [Fact]
            public void PackageIsSignedDeserialization()
            {
                // Arrange & Act
                var validationError = CreatePackageValidationError(ValidationErrorCode.PackageIsSignedError, "{\"PackageId\":\"Hello.World\",\"PackageVersion\":\"1.3.4\"}");
                var result = ValidationError.FromPackageValidationError(validationError) as PackageIsSignedError;

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
    }
}
