namespace SlackLeadsBidder.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Base2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Leads", "AuctionStarted", c => c.DateTime());
            AddColumn("dbo.Leads", "AuctionEnded", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Leads", "AuctionEnded");
            DropColumn("dbo.Leads", "AuctionStarted");
        }
    }
}
