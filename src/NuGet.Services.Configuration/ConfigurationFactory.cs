using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration
{
    /// <summary>
    /// <see cref="IConfigurationFactory"/> implementation that uses an <see cref="IConfigurationProvider"/> to inject configuration.
    /// </summary>
    public class ConfigurationFactory : IConfigurationFactory
    {
        private readonly IConfigurationProvider _configProvider;

        public ConfigurationFactory(IConfigurationProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public async Task<T> Get<T>() where T : Configuration, new()
        {
            var instance = new T();

            // Iterate over the properties and inject each with configuration.
            foreach (
                var property in
                TypeDescriptor.GetProperties(instance).Cast<PropertyDescriptor>().Where(p => !p.IsReadOnly))
            {
                await (Task) GetType()
                    .GetMethod(nameof(InjectPropertyWithConfiguration))
                    .MakeGenericMethod(typeof(T), property.PropertyType)
                    .Invoke(this, new object[] {instance, property});
            }

            return instance;
        }

        /// <summary>
        /// Injects a property of <param name="instance">instance</param> specified by <param name="property">a <see cref="PropertyDescriptor"/></param> with configuration.
        /// </summary>
        /// <typeparam name="T">Type of the instance to inject configuration into a property of.</typeparam>
        /// <typeparam name="TP">Type of the property to inject configuration into.</typeparam>
        /// <param name="instance">Instance to inject configuration into a property of.</param>
        /// <param name="property"><see cref="PropertyDescriptor"/> that describes the property to inject the configuration into.</param>
        /// <returns>A task that completes when the property has been injected into.</returns>
        public async Task InjectPropertyWithConfiguration<T, TP>(T instance, PropertyDescriptor property)
        {
            var settingName = string.IsNullOrEmpty(property.DisplayName) ? property.Name : property.DisplayName;

            TP value;

            if (property.Attributes.OfType<RequiredAttribute>().Any())
            {
                // If the property is required, use GetOrThrowAsync to access configuration.
                // It will throw if the configuration is not found or invalid.
                value = await _configProvider.GetOrThrowAsync<TP>(settingName);
            }
            else
            {
                var defaultValueAttribute = property.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                if (defaultValueAttribute != null)
                {
                    try
                    {
                        // Use the default value specified by the DefaultValueAttribute if it can be converted into the type of the property.
                        var defaultValue = (TP) (defaultValueAttribute.Value.GetType() == property.PropertyType ? defaultValueAttribute.Value : property.Converter.ConvertFrom(defaultValueAttribute.Value));
                        value = await _configProvider.GetOrDefaultAsync(settingName, defaultValue);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException($"Default value for {settingName} specified by DefaultValueAttribute is malformed ({defaultValueAttribute.Value ?? "null"})!");
                    }
                }
                else
                {
                    value = await _configProvider.GetOrDefaultAsync<TP>(settingName);
                }
            }

            property.SetValue(instance, value);
        }
    }
}
