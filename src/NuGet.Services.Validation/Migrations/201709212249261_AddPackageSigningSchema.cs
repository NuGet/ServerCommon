namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageSigningSchema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "signature.Package",
                c => new
                    {
                        PackageKey = c.Int(nullable: false, identity: true),
                        PackageId = c.String(nullable: false, maxLength: 128),
                        PackageNormalizedVersion = c.String(nullable: false, maxLength: 64),
                        SignatureStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PackageKey)
                .Index(t => new { t.PackageId, t.PackageNormalizedVersion }, name: "IX_Packages_PackageId_PackageNormalizedVersion");
            
            CreateTable(
                "signature.PackageSignature",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        PackageKey = c.Int(nullable: false),
                        SignedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("signature.Package", t => t.PackageKey, cascadeDelete: true)
                .Index(t => t.PackageKey, name: "IX_PackageSignatures_PackageKey")
                .Index(t => t.Status, name: "IX_PackageSignatures_Status");
            
            CreateTable(
                "signature.Certificate",
                c => new
                    {
                        Key = c.Long(nullable: false, identity: true),
                        Thumbprint = c.Binary(nullable: false, maxLength: 64),
                        Status = c.Int(nullable: false),
                        StatusUpdateTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        NextStatusUpdateTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        LastVerificationTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        RevocationTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        ValidationFailures = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .Index(t => t.Thumbprint, unique: true, name: "IX_Certificates_Thumbprint");
            
            CreateTable(
                "signature.PackageSignatureCertificates",
                c => new
                    {
                        PackageSignatureKey = c.Int(nullable: false),
                        CertificateKey = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.PackageSignatureKey, t.CertificateKey })
                .ForeignKey("signature.PackageSignature", t => t.PackageSignatureKey, cascadeDelete: true)
                .ForeignKey("signature.Certificate", t => t.CertificateKey, cascadeDelete: true)
                .Index(t => t.PackageSignatureKey)
                .Index(t => t.CertificateKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("signature.PackageSignature", "PackageKey", "signature.Package");
            DropForeignKey("signature.PackageSignatureCertificates", "CertificateKey", "signature.Certificate");
            DropForeignKey("signature.PackageSignatureCertificates", "PackageSignatureKey", "signature.PackageSignature");
            DropIndex("signature.PackageSignatureCertificates", new[] { "CertificateKey" });
            DropIndex("signature.PackageSignatureCertificates", new[] { "PackageSignatureKey" });
            DropIndex("signature.Certificate", "IX_Certificates_Thumbprint");
            DropIndex("signature.PackageSignature", "IX_PackageSignatures_Status");
            DropIndex("signature.PackageSignature", "IX_PackageSignatures_PackageKey");
            DropIndex("signature.Package", "IX_Packages_PackageId_PackageNormalizedVersion");
            DropTable("signature.PackageSignatureCertificates");
            DropTable("signature.Certificate");
            DropTable("signature.PackageSignature");
            DropTable("signature.Package");
        }
    }
}
