namespace SlackLeadsBidder.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Base1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Transactions", "Timestamp");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Transactions", "Timestamp", c => c.DateTime(nullable: false));
        }
    }
}
