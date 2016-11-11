using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration
{
    public interface IConfigurationFactory
    {
        Task<T> Get<T>() where T : Configuration, new();
    }
}
