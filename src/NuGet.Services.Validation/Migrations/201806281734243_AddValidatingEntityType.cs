namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddValidatingEntityType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PackageValidationSets", "ValidatingType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PackageValidationSets", "ValidatingType");
        }
    }
}
