using SlackLeadsBidder.Models;

namespace SlackLeadsBidder.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SlackLeadsBidder.Models.SlackLeadsBidderContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SlackLeadsBidder.Models.SlackLeadsBidderContext context)
        {
            TransactionType.SeedDb(context);
        }
    }
}
