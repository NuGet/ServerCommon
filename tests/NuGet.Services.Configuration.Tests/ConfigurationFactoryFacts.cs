// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Xunit;

namespace NuGet.Services.Configuration.Tests
{
    public class ConfigurationFactoryFacts
    {
        /// <summary>
        /// Represents the data associated with a property.
        /// Used to construct classes dynamically to test the <see cref="ConfigurationFactory"/> with.
        /// </summary>
        public class ConfigurationTuple
        {
            public ConfigurationTuple(Type type, bool required, object expectedValue, object defaultValue = null,
                string configKey = null, string configKeyPrefix = null)
            {
                Type = type;
                Required = required;
                ExpectedValue = expectedValue;
                DefaultValue = defaultValue;
                ConfigKey = configKey;
                ConfigKeyPrefix = configKeyPrefix;
            }

            /// <summary>
            /// If not null, a <see cref="ConfigurationKeyAttribute"/> will be added to the property with this value as the key.
            /// </summary>
            public string ConfigKey { get; set; }

            /// <summary>
            /// If not null, a <see cref="ConfigurationKeyPrefixAttribute"/> will be added to the property with this value as the prefix.
            /// </summary>
            public string ConfigKeyPrefix { get; set; }

            /// <summary>
            /// The type of the property.
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// If true, a <see cref="RequiredAttribute"/> will be added to the property.
            /// </summary>
            public bool Required { get; set; }

            /// <summary>
            /// If not null, the value to be injected into the property by the <see cref="ConfigurationFactory"/>.
            /// If null, the default value will be injected into the property by the <see cref="ConfigurationFactory"/>.
            /// </summary>
            public object ExpectedValue { get; set; }

            /// <summary>
            /// If not null, a <see cref="DefaultValueAttribute"/> will be added to this property with this value as the default.
            /// If null, <code>default(Type)</code> will be used as the default.
            /// </summary>
            public object DefaultValue { get; set; }
        }

        /// <summary>
        /// Dummy class that has no conversion into any type.
        /// Used to test that the <see cref="ConfigurationFactory"/> fails correctly when given incorrect types.
        /// </summary>
        public class NoConversionFromThisClass
        {
        }

        /// <summary>
        /// Test data for the <see cref="ConfigurationFactory"/> tests.
        /// </summary>
        public static IEnumerable<object[]> ConfigurationFactoryTestData => new[]
        {
            new object[]
            {
                // Succeeds
                // Tests base case
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringProperty",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: "i am a string")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests ConfigurationKeyAttribute
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithCustomKey",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: "i have a custom name!",
                            configKey: "customConfig")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests ConfigurationKeyPrefixAttribute
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithPrefix",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: "i have a cool prefix!",
                            configKeyPrefix: "coolbeans:")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests ConfigurationKeyAttribute and ConfigurationKeyPrefixAttribute together
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithPrefixAndCustomKey",
                        new ConfigurationTuple(typeof(string), required: false,
                            expectedValue: "i have a cool prefix and a cool name!", configKey: "customConfig",
                            configKeyPrefix: "coolbeans:")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests required
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyRequired",
                        new ConfigurationTuple(typeof(string), required: true, expectedValue: "i am still a string")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests default value
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithDefault",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: null,
                            defaultValue: "default string value")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests default value with actual value provided
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithDefaultDefined",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: "yet again i am a string",
                            defaultValue: "default string value")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests required and default together
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyRequiredWithDefaultDefined",
                        new ConfigurationTuple(typeof(string), required: true, expectedValue: "string forever",
                            defaultValue: "default string value")
                    }
                }
            },
            new object[]
            {
                // Fails because required configuration is missing
                // Tests required
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyRequiredMissing",
                        new ConfigurationTuple(typeof(string), required: true, expectedValue: null)
                    }
                }
            },
            new object[]
            {
                // Succeeds because missing but not required
                // Tests default without provided default
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyMissing",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: null)
                    }
                }
            },
            new object[]
            {
                // Fails because empty string is equivalent to null with regards to configuration
                // (the IConfigurationProvider throws ArgumentNullException)
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyRequiredEmpty",
                        new ConfigurationTuple(typeof(string), required: true, expectedValue: "")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests conversion into types other than string
                new Dictionary<string, ConfigurationTuple>
                {
                    {"intProperty", new ConfigurationTuple(typeof(int), required: false, expectedValue: 101)}
                }
            },
            new object[]
            {
                // Succeeds
                // Tests multiple properties in a class
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringProperty1",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: "thing 1")
                    },
                    {
                        "stringProperty2",
                        new ConfigurationTuple(typeof(string), required: false, expectedValue: "thing 2")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests multiple properties and types in a class
                new Dictionary<string, ConfigurationTuple>
                {
                    {"intProperty", new ConfigurationTuple(typeof(int), required: false, expectedValue: 44)},
                    {"requiredBoolProperty", new ConfigurationTuple(typeof(bool), required: true, expectedValue: true)}
                }
            },
            new object[]
            {
                // Succeeds
                // Tests multiple properties and types in a class and custom key name
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "doublePropertyWithCustomKey",
                        new ConfigurationTuple(typeof(double), required: false, expectedValue: 99.999,
                            configKey: "coolIntProperty")
                    },
                    {
                        "requiredDatetimePropertyWithPrefix",
                        new ConfigurationTuple(typeof(bool), required: true, expectedValue: DateTime.MinValue,
                            configKeyPrefix: "coolProperty.")
                    },
                    {
                        "intPropertyWithDefaultAndCustomKeyAndPrefix",
                        new ConfigurationTuple(typeof(int), required: false, expectedValue: 503, defaultValue: 200,
                            configKey: "ResponseCode", configKeyPrefix: "coolProperty:")
                    }
                }
            },
            new object[]
            {
                // Succeeds
                // Tests defaults with multiple properties and types in a class
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "doublePropertyWithDefault",
                        new ConfigurationTuple(typeof(double), required: false, expectedValue: 1.1, defaultValue: 3.14)
                    },
                    {
                        "boolPropertyMissingWithDefault",
                        new ConfigurationTuple(typeof(bool), required: false, expectedValue: null, defaultValue: true)
                    }
                }
            },
            new object[]
            {
                // Fails
                // Tests that the configuration provided must have the same type as the property
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "invalidBoolProperty",
                        new ConfigurationTuple(typeof(double), required: false,
                            expectedValue: new NoConversionFromThisClass())
                    }
                }
            },
            new object[]
            {
                // Fails
                // Tests that the default provided must be convertible to the property.
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "boolPropertyWithInvalidDefault",
                        new ConfigurationTuple(typeof(double), required: false, expectedValue: null,
                            defaultValue: "can't convert this to double")
                    }
                }
            }
        };

        /// <summary>
        /// Returns true if the object is convertible to type.
        /// </summary>
        /// <param name="value">The object to test.</param>
        /// <param name="type">Type to test.</param>
        /// <returns>True if the <param name="value">object</param> is convertible to <param name="type">type</param>, false otherwise.</returns>
        private static bool IsValueValid(object value, Type type)
        {
            if (value == null)
            {
                // Null is always valid because it will use the default.
                return true;
            }

            if (value.GetType() == type)
            {
                // TypeConverters sometimes throw when converting a type to the same type.
                return true;
            }

            // Attempt to convert the value.
            // If it fails, the value is not valid.
            try
            {
                TypeDescriptor.GetConverter(type).ConvertFrom(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tests that the <see cref="ConfigurationFactory"/> can handle a subclass <see cref="Configuration"/> specified by <param name="typeMap">typeMap</param>.
        /// </summary>
        /// <param name="typeMap">Used to construct a subclass of <see cref="Configuration"/>.</param>
        [Theory]
        [MemberData(nameof(ConfigurationFactoryTestData))]
        public void CorrectlyHandlesTypes(IDictionary<string, ConfigurationTuple> typeMap)
        {
            // Arrange
            IConfigurationFactory configFactory =
                new ConfigurationFactory(
                    new TestConfigurationProvider(
                        typeMap.ToDictionary(
                            tuple => tuple.Value.ConfigKeyPrefix + (tuple.Value.ConfigKey ?? tuple.Key),
                            tuple => tuple.Value.ExpectedValue)));

            var type = CreateTypeFromConfiguration(typeMap);

            // Act
            var getConfig = new Func<object>(() => GetType()
                .GetMethod(nameof(GetConfig))
                .MakeGenericMethod(type)
                .Invoke(this, new object[] {configFactory}));

            var willSucceed = true;
            foreach (var configPair in typeMap)
            {
                var configTuple = configPair.Value;

                // True if the expected value is null or if it is an empty string.
                var expectedValueIsMissing =
                    configTuple.ExpectedValue == null ||
                    (configTuple.ExpectedValue is string && string.IsNullOrEmpty((string) configTuple.ExpectedValue));

                var isExpectedValueValid = IsValueValid(configTuple.ExpectedValue, configTuple.Type);
                var isDefaultValueValid = IsValueValid(configTuple.DefaultValue, configTuple.Type);

                if ((configTuple.Required && expectedValueIsMissing) ||
                    !isExpectedValueValid ||
                    !isDefaultValueValid ||
                    configTuple.ConfigKey == string.Empty)
                {
                    // Acquiring the configuration will fail if a required attribute does not have an expected value.
                    // It will also fail if the expected value or default value cannot be converted into the type of the property.
                    // A null or empty configuration key will also fail.
                    willSucceed = false;
                    break;
                }
            }

            // Assert
            if (willSucceed)
            {
                var config = getConfig();
                foreach (var configPair in typeMap)
                {
                    var configTuple = configPair.Value;
                    Assert.Equal(configTuple.ExpectedValue ?? configTuple.DefaultValue,
                        GetFromProperty(config, configPair.Key));
                }
            }
            else
            {
                // This will throw a TargetInvocationException instead of the exception thrown by ConfigurationFactory because we are using Reflection in getConfig.
                Assert.Throws<TargetInvocationException>(getConfig);
            }
        }

        public T GetConfig<T>(IConfigurationFactory configFactory) where T : Configuration, new()
        {
            return configFactory.Get<T>().Result;
        }

        private static object GetFromProperty(object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName).GetMethod.Invoke(instance, null);
        }

        private static Type CreateTypeFromConfiguration(IDictionary<string, ConfigurationTuple> typeMap)
        {
            var typeBuilder = CreateTypeBuilder();
            foreach (var property in typeMap)
            {
                AddProperty(typeBuilder, property.Key, property.Value);
            }

            return typeBuilder.CreateType();
        }

        private const string DynamicTypeName = "TestConfiguration";
        private static int DynamicTypeCount = 0;

        private static TypeBuilder CreateTypeBuilder()
        {
            var assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(typeof(ConfigurationFactoryFacts).Module.FullyQualifiedName);
            return moduleBuilder.DefineType($"{DynamicTypeName}{DynamicTypeCount++}", TypeAttributes.Public | TypeAttributes.Class,
                typeof(Configuration));
        }

        /// <summary>
        /// Dynamically adds a property to a <see cref="TypeBuilder"/>.
        /// </summary>
        /// <param name="typeBuilder">The <see cref="TypeBuilder"/> to add the property to.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="configTuple">Specifies how to construct the property.</param>
        private static void AddProperty(TypeBuilder typeBuilder, string name, ConfigurationTuple configTuple)
        {
            // Create the property attribute.
            var propertyAttributes = configTuple.DefaultValue != null ? PropertyAttributes.HasDefault : PropertyAttributes.None;
            var propertyBuilder = typeBuilder.DefineProperty(name, propertyAttributes, configTuple.Type, parameterTypes: null);

            if (configTuple.Required)
            {
                propertyBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(typeof(RequiredAttribute).GetConstructor(Type.EmptyTypes),
                        new object[] { }));
            }

            if (configTuple.DefaultValue != null)
            {
                propertyBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(typeof(DefaultValueAttribute).GetConstructor(new[] { configTuple.DefaultValue.GetType() }),
                        new[] { configTuple.DefaultValue }));
            }

            if (configTuple.ConfigKey != null)
            {
                propertyBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        typeof(ConfigurationKeyAttribute).GetConstructor(new[] { typeof(string) }),
                        new object[] { configTuple.ConfigKey }));
            }

            if (configTuple.ConfigKeyPrefix != null)
            {
                propertyBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        typeof(ConfigurationKeyPrefixAttribute).GetConstructor(new[] { typeof(string) }),
                        new object[] { configTuple.ConfigKeyPrefix }));
            }

            // Create the field that the property will get and set.
            var fieldBuilder = typeBuilder.DefineField($"_{name}", configTuple.Type, FieldAttributes.Private);

            // Create the get method for the property that will return the field.
            var getMethod = typeBuilder.DefineMethod($"get_{name}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, configTuple.Type, parameterTypes: null);

            var getMethodIl = getMethod.GetILGenerator();
            getMethodIl.Emit(OpCodes.Ldarg_0);
            getMethodIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethod);

            // Create the set method for the property that will set the field.
            var setMethod = typeBuilder.DefineMethod($"set_{name}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(void),
                new[] { configTuple.Type });

            var setMethodIl = setMethod.GetILGenerator();
            setMethodIl.Emit(OpCodes.Ldarg_0);
            setMethodIl.Emit(OpCodes.Ldarg_1);
            setMethodIl.Emit(OpCodes.Stfld, fieldBuilder);
            setMethodIl.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethod);
        }
    }
}
