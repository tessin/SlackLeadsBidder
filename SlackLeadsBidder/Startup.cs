using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Owin;
using Owin;
using SlackLeadsBidder.Services;

[assembly: OwinStartup(typeof(SlackLeadsBidder.Startup))]

namespace SlackLeadsBidder
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            RecurringJob.AddOrUpdate(() => SlackLeadsBidderService.CreditDailyAllowances(), Cron.Daily(0));
            RecurringJob.AddOrUpdate(() => SlackLeadsBidderService.CreateAuction(), Cron.Minutely());
            RecurringJob.AddOrUpdate(() => SlackLeadsBidderService.EndAuction(), Cron.Minutely());
        }
    }
}
