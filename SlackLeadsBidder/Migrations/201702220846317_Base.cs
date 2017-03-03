namespace SlackLeadsBidder.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Base : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        AutoBid = c.Int(nullable: false),
                        NextBid = c.Int(nullable: false),
                        SlackId = c.String(nullable: false),
                        DailyAllowance = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Leads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Subject = c.String(nullable: false),
                        Body = c.String(nullable: false),
                        FromEmail = c.String(nullable: false),
                        FromName = c.String(nullable: false),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        LeadId = c.Int(),
                        AgentId = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        TransactionTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agents", t => t.AgentId)
                .ForeignKey("dbo.Leads", t => t.LeadId)
                .ForeignKey("dbo.TransactionTypes", t => t.TransactionTypeId)
                .Index(t => t.LeadId)
                .Index(t => t.AgentId)
                .Index(t => t.TransactionTypeId);
            
            CreateTable(
                "dbo.TransactionTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DescriptionText = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "TransactionTypeId", "dbo.TransactionTypes");
            DropForeignKey("dbo.Transactions", "LeadId", "dbo.Leads");
            DropForeignKey("dbo.Transactions", "AgentId", "dbo.Agents");
            DropIndex("dbo.Transactions", new[] { "TransactionTypeId" });
            DropIndex("dbo.Transactions", new[] { "AgentId" });
            DropIndex("dbo.Transactions", new[] { "LeadId" });
            DropTable("dbo.TransactionTypes");
            DropTable("dbo.Transactions");
            DropTable("dbo.Leads");
            DropTable("dbo.Agents");
        }
    }
}
