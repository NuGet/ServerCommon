// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Threading.Tasks;

namespace NuGet.Services.Validation
{
    public interface IValidationEntitiesContext
    {
        IDbSet<PackageValidationSet> PackageValidationSets { get; set; }
        IDbSet<PackageValidation> PackageValidations { get; set; }
        IDbSet<ValidatorStatus> ValidatorStatuses { get; set; }

        IDbSet<PackageSigningState> PackageSigningStates { get; set; }
        IDbSet<PackageSignature> PackageSignatures { get; set; }
        IDbSet<Certificate> Certificates { get; set; }
        IDbSet<CertificateValidation> CertificateValidations { get; set; }

        Task<int> SaveChangesAsync();
    }
}
