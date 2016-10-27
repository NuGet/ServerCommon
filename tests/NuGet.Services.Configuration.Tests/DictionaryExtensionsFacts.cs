// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace NuGet.Services.Configuration.Tests
{
    public class DictionaryExtensionsFacts
    {
        public static IEnumerable<object[]> ValueData => new[]
        {
            new object[] {true},
            new object[] {false},
            new object[] {"hello"},
            new object[] {"123456789"},
            new object[] {-1},
            new object[] {1259},    
            new object[] {DateTime.MinValue},
            new object[] {DateTime.MinValue.AddYears(100).AddDays(50).AddHours(5).AddSeconds(62)} 
        };

        private struct NoConversionFromStringToThisStruct
        {
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetOrNullConvertsAndDoesNotThrow<T>(T value)
        {
            // Arrange
            const string key = "key";
            const string notKey = "notAKey";
            IDictionary<string, string> dictionary = new Dictionary<string, string>
            {
                {key, value.ToString()}
            };

            // Act
            var valueFromDictionary = dictionary.GetOrDefault<T>(key);
            var notFoundFromDictionary = dictionary.GetOrDefault<T>(notKey);
            var notSupportedFromDictionary = dictionary.GetOrDefault<NoConversionFromStringToThisStruct>(key);

            // Assert
            Assert.Equal(value, valueFromDictionary);
            Assert.Equal(default(T), notFoundFromDictionary);
            Assert.Equal(default(NoConversionFromStringToThisStruct), notSupportedFromDictionary);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetOrThrowConvertsValueAndThrows<T>(T value)
        {
            // Arrange
            const string key = "key";
            const string notKey = "notAKey";
            IDictionary<string, string> dictionary = new Dictionary<string, string>
            {
                {key, value.ToString()}
            };

            // Act
            var valueFromDictionary = dictionary.GetOrThrow<T>(key);

            // Assert
            Assert.Equal(value, valueFromDictionary);
            Assert.Throws<KeyNotFoundException>(() => dictionary.GetOrThrow<T>(notKey));
            Assert.Throws<NotSupportedException>(() => dictionary.GetOrThrow<NoConversionFromStringToThisStruct>(key));
        }
    }
}
