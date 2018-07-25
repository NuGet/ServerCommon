namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVSTSSymbolsServerRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VSTSSymbolsServerRequests",
                c => new
                    {
                        SymbolsKey = c.Int(nullable: false, identity: true),
                        RequestName = c.String(),
                        RequestStatusKey = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.SymbolsKey);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VSTSSymbolsServerRequests");
        }
    }
}
