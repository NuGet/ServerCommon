namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddValidationErrors : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PackageValidationErrors",
                c => new
                    {
                        Key = c.Long(nullable: false, identity: true),
                        PackageValidationSetKey = c.Long(nullable: false),
                        Message = c.String(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.PackageValidationSets", t => t.PackageValidationSetKey, cascadeDelete: true)
                .Index(t => t.PackageValidationSetKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PackageValidationErrors", "PackageValidationSetKey", "dbo.PackageValidationSets");
            DropIndex("dbo.PackageValidationErrors", new[] { "PackageValidationSetKey" });
            DropTable("dbo.PackageValidationErrors");
        }
    }
}
