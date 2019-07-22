namespace Invenio.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReportDateOfInspection : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Report", "DateOfInspection", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Report", "DateOfInspection");
        }
    }
}
