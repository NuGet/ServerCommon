namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageValidationErrors : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PackageValidationErrors",
                c => new
                    {
                        Key = c.Long(nullable: false, identity: true),
                        PackageValidationKey = c.Guid(nullable: false),
                        ErrorCode = c.Int(nullable: false),
                        Data = c.String(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.PackageValidations", t => t.PackageValidationKey, cascadeDelete: true)
                .Index(t => t.PackageValidationKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PackageValidationErrors", "PackageValidationKey", "dbo.PackageValidations");
            DropIndex("dbo.PackageValidationErrors", new[] { "PackageValidationKey" });
            DropTable("dbo.PackageValidationErrors");
        }
    }
}
