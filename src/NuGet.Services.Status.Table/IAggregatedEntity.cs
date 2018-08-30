using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.Status.Table
{
    public interface IAggregatedEntity<T> : ILinkedEntity<T>, IComponentAffectingEntity
        where T : IComponentAffectingEntity
    {
    }
}
