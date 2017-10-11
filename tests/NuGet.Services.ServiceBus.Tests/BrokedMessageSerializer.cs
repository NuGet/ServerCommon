// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Moq;
using NuGet.Services.ServiceBus;
using Xunit;

namespace NuGet.Services.ServiceBus.Tests
{
    public class BrokedMessageSerializer
    {
        private const string SchemaVersionKey = "SchemaVersion";
        private const string TypeKey = "Type";

        private const string SchematizedTypeName = "SchematizedType";
        private const int SchemaVersion23 = 23;

        private const string JsonSerializedContent = "{\"A\":\"Hello World\"}";

        [SchemaVersion(23)]
        public class SchematizedType
        {
            public string A { get; set; }
        }

        public class UnSchematizedType { }

        public class TheConstructor : Base
        {
            [Fact]
            public void ThrowsIfSchemaDoesntHaveSchemaVersionAttribute()
            {
                Action runConstructor = () => new BrokedMessageSerializer<UnSchematizedType>();
                var exception = Assert.Throws<TypeInitializationException>(runConstructor);

                Assert.Equal(typeof(InvalidOperationException), exception.InnerException.GetType());
                Assert.Contains($"{SchematizedTypeName} must have exactly one {nameof(SchemaVersionAttribute)}", exception.InnerException.Message);
            }
        }

        public class TheSerializeMethod : Base
        {
            [Fact]
            public void ProducesExpectedMessage()
            {
                // Arrange
                var input = new SchematizedType { A = "Hello World" };

                // Act
                var output = _target.Serialize(input);

                // Assert
                Assert.Contains(SchemaVersionKey, output.Properties.Keys);
                Assert.Equal(SchemaVersion23, output.Properties[SchemaVersionKey]);
                Assert.Contains(TypeKey, output.Properties.Keys);
                Assert.Equal(SchematizedTypeName, output.Properties[TypeKey]);
                var body = output.GetBody();
                Assert.Equal(JsonSerializedContent, body);
            }
        }

        public class TheDeserializePackageValidationMessageDataMethod : Base
        {
            private const string TypeValue = "PackageValidationMessageData";

            [Fact]
            public void ProducesExpectedMessage()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();

                // Act
                var output = _target.Deserialize(brokeredMessage.Object);

                // Assert
                Assert.Equal("Hello World", output.A);
            }

            [Fact]
            public void RejectsInvalidType()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();
                brokeredMessage.Object.Properties[TypeKey] = "bad";

                // Act & Assert
                var exception = Assert.Throws<FormatException>(() =>
                    _target.Deserialize(brokeredMessage.Object));
                Assert.Contains($"The provided message should have {TypeKey} property '{SchematizedTypeName}'.", exception.Message);
            }

            [Fact]
            public void RejectsInvalidSchemaVersion()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();
                brokeredMessage.Object.Properties[SchemaVersionKey] = -1;

                // Act & Assert
                var exception = Assert.Throws<FormatException>(() =>
                    _target.Deserialize(brokeredMessage.Object));
                Assert.Contains($"The provided message should have {SchemaVersionKey} property '23'.", exception.Message);
            }

            [Fact]
            public void RejectsMissingType()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();
                brokeredMessage.Object.Properties.Remove(TypeKey);

                // Act & Assert
                var exception = Assert.Throws<FormatException>(() =>
                    _target.Deserialize(brokeredMessage.Object));
                Assert.Contains($"The provided message does not have a {TypeKey} property.", exception.Message);
            }

            [Fact]
            public void RejectsMissingSchemaVersion()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();
                brokeredMessage.Object.Properties.Remove(SchemaVersionKey);

                // Act & Assert
                var exception = Assert.Throws<FormatException>(() =>
                    _target.Deserialize(brokeredMessage.Object));
                Assert.Contains($"The provided message does not have a {SchemaVersionKey} property.", exception.Message);
            }

            [Fact]
            public void RejectsInvalidTypeType()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();
                brokeredMessage.Object.Properties[TypeKey] = -1;

                // Act & Assert
                var exception = Assert.Throws<FormatException>(() =>
                    _target.Deserialize(brokeredMessage.Object));
                Assert.Contains($"The provided message contains a {TypeKey} property that is not a string.", exception.Message);
            }

            [Fact]
            public void RejectsInvalidSchemaVersionType()
            {
                // Arrange
                var brokeredMessage = GetBrokeredMessage();
                brokeredMessage.Object.Properties[SchemaVersionKey] = "bad";

                // Act & Assert
                var exception = Assert.Throws<FormatException>(() =>
                    _target.Deserialize(brokeredMessage.Object));
                Assert.Contains($"The provided message contains a {SchemaVersionKey} property that is not an integer.", exception.Message);
            }

            private static Mock<IBrokeredMessage> GetBrokeredMessage()
            {
                var brokeredMessage = new Mock<IBrokeredMessage>();
                brokeredMessage
                    .Setup(x => x.GetBody())
                    .Returns(JsonSerializedContent);
                brokeredMessage
                    .Setup(x => x.Properties)
                    .Returns(new Dictionary<string, object>
                    {
                        { TypeKey, SchematizedTypeName },
                        { SchemaVersionKey, SchemaVersion23 }
                    });
                return brokeredMessage;
            }
        }

        public abstract class Base
        {
            protected readonly BrokedMessageSerializer<SchematizedType> _target;

            public Base()
            {
                _target = new BrokedMessageSerializer<SchematizedType>();
            }
        }
    }
}
