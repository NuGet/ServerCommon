using System;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Services.Incidents
{
    public class IncidentApiConfiguration
    {
        /// <summary>
        /// The base <see cref="Uri"/> of the NuGet incident API to access.
        /// </summary>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// The certificate to use to authenticate with the NuGet incident API.
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
    }
}
