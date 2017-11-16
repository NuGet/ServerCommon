namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddParentCertificatesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "signature.ParentCertificates",
                c => new
                    {
                        Key = c.Long(nullable: false, identity: true),
                        EndCertificateKey = c.Long(nullable: false),
                        Thumbprint = c.String(nullable: false, maxLength: 20, fixedLength: true, unicode: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("signature.Certificates", t => t.EndCertificateKey, cascadeDelete: true)
                .Index(t => new { t.EndCertificateKey, t.Thumbprint }, name: "IX_ParentCertificates_EndCertificateKeyThumbprint");
            
        }
        
        public override void Down()
        {
            DropForeignKey("signature.ParentCertificates", "EndCertificateKey", "signature.Certificates");
            DropIndex("signature.ParentCertificates", "IX_ParentCertificates_EndCertificateKeyThumbprint");
            DropTable("signature.ParentCertificates");
        }
    }
}
