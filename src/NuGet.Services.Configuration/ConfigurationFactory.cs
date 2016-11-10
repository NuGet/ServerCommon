using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration
{
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

            // Iterate over the properties
            foreach (
                var property in
                TypeDescriptor.GetProperties(instance).Cast<PropertyDescriptor>().Where(p => !p.IsReadOnly))
            {
                await (Task) GetType()
                    .GetMethod(nameof(SetProperty))
                    .MakeGenericMethod(new [] {typeof(T), property.PropertyType})
                    .Invoke(this, new object[] {instance, property});
            }

            return instance;
        }

        public async Task SetProperty<T, TP>(T instance, PropertyDescriptor property)
        {
            var settingName = string.IsNullOrEmpty(property.DisplayName) ? property.Name : property.DisplayName;

            TP value;

            if (property.Attributes.OfType<RequiredAttribute>().Any())
            {
                value = await _configProvider.GetOrThrowAsync<TP>(settingName);
            }
            else
            {
                var defaultValueAttribute = property.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                if (defaultValueAttribute != null)
                {
                    try
                    {
                        var defaultValue = (TP) property.Converter.ConvertFrom(defaultValueAttribute.Value);
                        value = await _configProvider.GetOrDefaultAsync(settingName, defaultValue);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException($"Default value for {settingName} specified by DefaultValueAttribute is malformed ({defaultValueAttribute.Value})!");
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
