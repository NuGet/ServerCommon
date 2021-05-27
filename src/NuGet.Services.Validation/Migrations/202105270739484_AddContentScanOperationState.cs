namespace NuGet.Services.Validation
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddContentScanOperationState : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cvs.ContentScanOperationStates",
                c => new
                    {
                        Key = c.Long(nullable: false, identity: true),
                        ValidationStepId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        JobId = c.String(),
                        Type = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        PolledAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        FinishedAt = c.DateTime(precision: 7, storeType: "datetime2"),
                        ContentPath = c.String(maxLength: 512),
                        FileId = c.String(maxLength: 64),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Key)
                .Index(t => new { t.ValidationStepId, t.Type, t.Status }, name: "IX_ContentScanOperationStates_ValidationStepId_Type_StatusIndex")
                .Index(t => t.CreatedAt, name: "IX_ContentScanOperationStates_CreatedIndex");
        }
        
        public override void Down()
        {
            DropIndex("cvs.ContentScanOperationStates", "IX_ContentScanOperationStates_CreatedIndex");
            DropIndex("cvs.ContentScanOperationStates", "IX_ContentScanOperationStates_ValidationStepId_Type_StatusIndex");
            DropTable("cvs.ContentScanOperationStates");
        }
    }
}
