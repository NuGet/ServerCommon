using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NuGet.Services.Incidents
{
    internal class IncidentList
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<Incident> Incidents { get; set; }

        [JsonProperty(PropertyName = "odata.nextLink")]
        public Uri NextLink { get; set; }
    }
}
