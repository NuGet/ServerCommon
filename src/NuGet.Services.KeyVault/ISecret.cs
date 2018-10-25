using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public interface ISecret
    {
        string Name { get; }
        string Value { get; }
        DateTime? Expiration { get; }
    }
}
