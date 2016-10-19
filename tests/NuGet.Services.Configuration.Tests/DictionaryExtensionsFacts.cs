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
            new object[] {-1},
            new object[] {1259},    
            new object[] {DateTime.MinValue},
            new object[] {DateTime.MinValue.AddYears(100).AddDays(50).AddHours(5).AddSeconds(62)} 
        };

        [Theory]
        [MemberData(nameof(ValueData))]
        public void TryGetValueConvertsAndDoesNotThrow<T>(T value) where T : struct
        {
            // Arrange
            const string key = "key";
            const string notKey = "notAKey";
            IDictionary<string, string> dictionary = new Dictionary<string, string>
            {
                {key, value.ToString()}
            };

            // Act
            var valueFromDictionary = dictionary.TryGetValue<T>(key);
            var notFoundFromDictionary = dictionary.TryGetValue<T>(notKey);

            // Assert
            Assert.True(valueFromDictionary.HasValue);
            Assert.Equal(value, valueFromDictionary.Value);
            Assert.False(notFoundFromDictionary.HasValue);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetConvertsValueAndThrows<T>(T value) where T : struct
        {
            // Arrange
            const string key = "key";
            const string notKey = "notAKey";
            IDictionary<string, string> dictionary = new Dictionary<string, string>
            {
                {key, value.ToString()}
            };

            // Act
            var valueFromDictionary = dictionary.Get<T>(key);

            // Assert
            Assert.Equal(value, valueFromDictionary);
            Assert.Throws<KeyNotFoundException>(() => dictionary.Get<T>(notKey));
        }
    }
}
