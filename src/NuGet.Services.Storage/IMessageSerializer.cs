using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.Storage
{
    public interface IMessageSerializer<T>
    {
        string Serialize(T contents);

        T Deserialize(string contents);
    }
}
