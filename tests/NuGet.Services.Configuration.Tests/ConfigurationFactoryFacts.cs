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
        public const string DynamicTypeName = "TestConfiguration";
        public static int DynamicTypeCount = 0;

        /// <summary>
        /// Represents the data associated with a property.
        /// Used to construct classes dynamically to test the <see cref="ConfigurationFactory"/> with.
        /// </summary>
        public class ConfigurationTuple
        {
            public ConfigurationTuple(Type type, bool required, object expectedValue, object defaultValue = null)
            {
                Type = type;
                Required = required;
                ExpectedValue = expectedValue;
                DefaultValue = defaultValue;
            }

            public Type Type { get; set; }
            public bool Required { get; set; }
            public object ExpectedValue { get; set; }
            public object DefaultValue { get; set; }
        }

        /// <summary>
        /// Test data for the <see cref="ConfigurationFactory"/> tests.
        /// </summary>
        public static IEnumerable<object[]> ConfigurationFactoryTestData => new[]
        {
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringProperty",
                        new ConfigurationTuple(typeof(string), false, "i am a string")
                    }
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {"stringPropertyRequired", new ConfigurationTuple(typeof(string), true, "i am still a string")}
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithDefault",
                        new ConfigurationTuple(typeof(string), false, null, "default string value")
                    }
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyWithDefaultDefined",
                        new ConfigurationTuple(typeof(string), false, "yet again i am a string", "default string value")
                    }
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {
                        "stringPropertyRequiredWithDefaultDefined",
                        new ConfigurationTuple(typeof(string), true, "string forever", "default string value")
                    }
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {"stringPropertyRequiredMissing", new ConfigurationTuple(typeof(string), true, null)}
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {"stringPropertyMissing", new ConfigurationTuple(typeof(string), false, null)}
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {"stringPropertyRequiredEmpty", new ConfigurationTuple(typeof(string), true, "")}
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {"intProperty", new ConfigurationTuple(typeof(int), false, 101)}
                }
            },
            new object[]
            {
                new Dictionary<string, ConfigurationTuple>
                {
                    {"intProperty", new ConfigurationTuple(typeof(int), false, 44)},
                    {"boolProperty", new ConfigurationTuple(typeof(bool), true, true)}
                }
            }
        };

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
                    new TestConfigurationProvider(typeMap.ToDictionary(tuple => tuple.Key,
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

                if (configTuple.Required &&
                    (configTuple.ExpectedValue == null ||
                     (configTuple.ExpectedValue is string && string.IsNullOrEmpty((string) configTuple.ExpectedValue))))
                {
                    // Acquiring the configuration will fail if a required attribute is not specified.
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
                    Assert.Equal(configTuple.ExpectedValue ?? configTuple.DefaultValue, GetFromProperty(config, configPair.Key));
                }
            }
            else
            {
                // This will throw a TargetInvocationException because we are using Reflection in getConfig.
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
            var propertyBuilder = typeBuilder.DefineProperty(name, propertyAttributes, configTuple.Type, null);

            if (configTuple.DefaultValue != null)
            {
                propertyBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(typeof(DefaultValueAttribute).GetConstructor(new[] { configTuple.Type }),
                        new[] { configTuple.DefaultValue }));
            }

            if (configTuple.Required)
            {
                propertyBuilder.SetCustomAttribute(
                    new CustomAttributeBuilder(typeof(RequiredAttribute).GetConstructor(Type.EmptyTypes),
                        new object[] { }));
            }

            // Create the field that the property will get and set.
            var fieldBuilder = typeBuilder.DefineField($"_{name}", configTuple.Type, FieldAttributes.Private);

            // Create the get method for the property that will return the field.
            var getMethod = typeBuilder.DefineMethod($"get_{name}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, configTuple.Type, null);

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
