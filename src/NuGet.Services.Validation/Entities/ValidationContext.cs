// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The Entity Framework database context for validation entities.
    /// </summary>
    [DbConfigurationType(typeof(EntitiesConfiguration))]
    public class ValidationEntitiesContext : DbContext
    {
        private const string SignatureSchema = "signature";

        private const string PackageValidationSetsValidationTrackingId = "IX_PackageValidationSets_ValidationTrackingId";
        private const string PackageValidationSetsPackageKeyIndex = "IX_PackageValidationSets_PackageKey";
        private const string PackageValidationSetsPackageIdPackageVersionIndex = "IX_PackageValidationSets_PackageId_PackageNormalizedVersion";

        private const string PackagesPackageIdPackageVersionIndex = "IX_Packages_PackageId_PackageNormalizedVersion";

        private const string PackageSignaturesPackageKeyIndex = "IX_PackageSignatures_PackageKey";
        private const string PackageSignaturesStatusIndex = "IX_PackageSignatures_Status";

        private const string CertificatesThumbprintIndex = "IX_Certificates_Thumbprint";

        static ValidationEntitiesContext()
        {
            // Don't run migrations, ever!
            Database.SetInitializer<ValidationEntitiesContext>(null);
        }

        public IDbSet<PackageValidationSet> PackageValidationSets { get; set; }
        public IDbSet<PackageValidation> PackageValidations { get; set; }

        public ValidationEntitiesContext() : this("Validation.SqlServer")
        {
        }

        public ValidationEntitiesContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PackageValidationSet>()
                .HasKey(pvs => pvs.Key);

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.ValidationTrackingId)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackageValidationSetsValidationTrackingId)
                        {
                            IsUnique = true
                        }
                    }));

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.PackageKey)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackageValidationSetsPackageKeyIndex)
                    }));

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.PackageId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackageValidationSetsPackageIdPackageVersionIndex, 1)
                    }));

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.PackageNormalizedVersion)
                .HasMaxLength(64)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackageValidationSetsPackageIdPackageVersionIndex, 2)
                    }));

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.Created)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PackageValidationSet>()
                .Property(pvs => pvs.Updated)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PackageValidation>()
                .HasKey(pv => pv.Key);

            modelBuilder.Entity<PackageValidation>()
                .Property(pv => pv.Key)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<PackageValidation>()
                .Property(pv => pv.Type)
                .HasMaxLength(255)
                .HasColumnType("varchar")
                .IsRequired();

            modelBuilder.Entity<PackageValidation>()
                .Property(pv => pv.Started)
                .IsOptional()
                .HasColumnType("datetime2");

            modelBuilder.Entity<PackageValidation>()
                .Property(pv => pv.ValidationStatusTimestamp)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PackageValidation>()
                .Property(pv => pv.RowVersion)
                .IsRowVersion();

            RegisterPackageSigningEntities(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void RegisterPackageSigningEntities(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Package>()
                .ToTable(nameof(Package), SignatureSchema)
                .HasKey(p => p.PackageKey);

            modelBuilder.Entity<Package>()
                .Property(p => p.PackageKey)
                .IsRequired();

            modelBuilder.Entity<Package>()
                .Property(p => p.PackageId)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackagesPackageIdPackageVersionIndex, 1)
                    }));

            modelBuilder.Entity<Package>()
                .Property(p => p.PackageNormalizedVersion)
                .HasMaxLength(64)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackagesPackageIdPackageVersionIndex, 2)
                    }));

            modelBuilder.Entity<Package>()
                .HasMany(p => p.PackageSignatures)
                .WithRequired(s => s.Package)
                .HasForeignKey(s => s.PackageKey)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PackageSignature>()
                .ToTable(nameof(PackageSignature), SignatureSchema)
                .HasKey(s => s.Key);

            modelBuilder.Entity<PackageSignature>()
                .Property(s => s.Key)
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<PackageSignature>()
                .Property(s => s.PackageKey)
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackageSignaturesPackageKeyIndex)
                    }));

            modelBuilder.Entity<PackageSignature>()
                .Property(s => s.CreatedAt)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<PackageSignature>()
                .Property(s => s.Status)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(PackageSignaturesStatusIndex)
                    }));

            modelBuilder.Entity<PackageSignature>()
                .Property(s => s.SignedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PackageSignature>()
                .Property(s => s.CreatedAt)
                .HasColumnType("datetime2");

            modelBuilder.Entity<PackageSignature>()
                .HasMany(s => s.Certificates)
                .WithMany(c => c.PackageSignatures)
                .Map(m =>
                {
                    m.MapLeftKey("PackageSignatureKey");
                    m.MapRightKey("CertificateKey");
                    m.ToTable("PackageSignatureCertificates", SignatureSchema);
                });

            modelBuilder.Entity<Certificate>()
                .ToTable(nameof(Certificate), SignatureSchema)
                .HasKey(c => c.Key);

            modelBuilder.Entity<Certificate>()
                .Property(c => c.Key)
                .IsRequired()
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Certificate>()
                .Property(c => c.Thumbprint)
                .HasMaxLength(64)
                .HasColumnType("varbinary")
                .IsRequired()
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute(CertificatesThumbprintIndex)
                        {
                            IsUnique = true,
                        }
                    }));

            modelBuilder.Entity<Certificate>()
                .Property(c => c.StatusUpdateTime)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Certificate>()
                .Property(c => c.NextStatusUpdateTime)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Certificate>()
                .Property(c => c.LastVerificationTime)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Certificate>()
                .Property(c => c.RevocationTime)
                .HasColumnType("datetime2");
        }
    }
}
