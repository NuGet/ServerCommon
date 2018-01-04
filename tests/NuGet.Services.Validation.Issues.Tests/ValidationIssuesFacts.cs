// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace NuGet.Services.Validation.Issues.Tests
{
    public class ValidationIssuesFacts
    {
        private static string PackageIsSignedMessage => GetTestData("PackageIsSignedMessage.txt");

        [Fact]
        public void TheIssueCodeTypesPropertyValuesAllExtendValidationIssue()
        {
            Assert.True(ValidationIssue.IssueCodeTypes.Values.All(t => t.IsSubclassOf(typeof(ValidationIssue))));
        }

        public class TheSerializeMethod
        {
            [Fact]
            public void UnknownSerialization()
            {
                // Arrange
                var unknownIssue = new UnknownIssue();
                var result = unknownIssue.Serialize();

                // Assert
                Assert.Equal("{}", result);
            }

            [Fact]
            public void PackageIsSignedSerialization()
            {
                // Arrange
                var signedError = new PackageIsSigned();
                var result = signedError.Serialize();

                // Assert
                Assert.Equal("{}", result);
            }
        }

        public class TheDeserializeMethod
        {
            [Fact]
            public void UnknownDeserialization()
            {
                // Arrange & Act
                var validationIssue = CreatePackageValidationIssue(ValidationIssueCode.Unknown, "{}");
                var result = ValidationIssue.Deserialize(validationIssue.IssueCode, validationIssue.Data) as UnknownIssue;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(ValidationIssueCode.Unknown, result.IssueCode);
                Assert.Equal("Package validation failed due to an unknown error.", result.GetMessage());
            }

            [Fact]
            public void InvalidDeserialization()
            {
                // Arrange & Act & Assert
                var validationIssue = CreatePackageValidationIssue(ValidationIssueCode.PackageIsSigned, "HELLO THIS IS DOG");

                Assert.Throws<JsonReaderException>(() => ValidationIssue.Deserialize(validationIssue.IssueCode, validationIssue.Data));
            }

            [Fact]
            public void PackageIsSignedDeserialization()
            {
                // Arrange & Act
                var validationIssue = CreatePackageValidationIssue(ValidationIssueCode.PackageIsSigned, "{}");
                var result = ValidationIssue.Deserialize(validationIssue.IssueCode, validationIssue.Data) as PackageIsSigned;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(ValidationIssueCode.PackageIsSigned, result.IssueCode);
                Assert.Equal(PackageIsSignedMessage, result.GetMessage());
            }

            private PackageValidationIssue CreatePackageValidationIssue(ValidationIssueCode issueCode, string data)
            {
                return new PackageValidationIssue
                {
                    IssueCode = issueCode,
                    Data = data
                };
            }
        }

        private static string GetTestData(string fileName)
        {
            return File.ReadAllText(Path.Combine("Data", $"{fileName}"));
        }
    }
}
