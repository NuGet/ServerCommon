namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCvsOperationStateTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cvs.CvsOperationStates",
                c => new
                    {
                        Key = c.Long(nullable: false, identity: true),
                        ValidationStepId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        JobId = c.Long(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastUpdatedAt = c.DateTime(precision: 7, storeType: "datetime2"),
                        Violation = c.String(maxLength: 1024),
                        ContentPath = c.String(maxLength: 512),
                        FileId = c.String(maxLength: 64),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Key)
                .Index(t => t.ValidationStepId, unique: true, name: "IX_CvsOperationStates_ValidationStepIdIndex")
                .Index(t => new { t.Status, t.CreatedAt }, name: "IX_CvsOperationStates_ScanStatus_CreatedIndex");
            
        }
        
        public override void Down()
        {
            DropIndex("cvs.CvsOperationStates", "IX_CvsOperationStates_ScanStatus_CreatedIndex");
            DropIndex("cvs.CvsOperationStates", "IX_CvsOperationStates_ValidationStepIdIndex");
            DropTable("cvs.CvsOperationStates");
        }
    }
}
