using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SlackLeadsBidder.Config
{
    public class SlackLeadsBidderSettings : ConfigurationSection
    {
        public static SlackLeadsBidderSettings Settings { get; } = ConfigurationManager.GetSection("slackLeadsBidder") as SlackLeadsBidderSettings;

        [ConfigurationProperty("slackOutgoingWebhook", IsRequired = true)]
        public string SlackOutgoingWebhook
        {
            get { return (string)this["slackOutgoingWebhook"]; }
            set { this["slackOutgoingWebhook"] = value; }
        }

        [ConfigurationProperty("slackCommandBidToken", IsRequired = true)]
        public string SlackCommandBidToken
        {
            get { return (string)this["slackCommandBidToken"]; }
            set { this["slackCommandBidToken"] = value; }
        }

        [ConfigurationProperty("slackCommandAutoBidToken", IsRequired = true)]
        public string SlackCommandAutoBidToken
        {
            get { return (string)this["slackCommandAutoBidToken"]; }
            set { this["slackCommandAutoBidToken"] = value; }
        }

        [ConfigurationProperty("slackCommandStatusToken", IsRequired = true)]
        public string SlackCommandStatusToken
        {
            get { return (string)this["slackCommandStatusToken"]; }
            set { this["slackCommandStatusToken"] = value; }
        }

        [ConfigurationProperty("slackCommandBalanceToken", IsRequired = true)]
        public string SlackCommandBalanceToken
        {
            get { return (string)this["slackCommandBalanceToken"]; }
            set { this["slackCommandBalanceToken"] = value; }
        }

        [ConfigurationProperty("auctionTimeInSeconds", IsRequired = true)]
        public int AuctionTimeInSeconds
        {
            get { return (int)this["auctionTimeInSeconds"]; }
            set { this["auctionTimeInSeconds"] = value; }
        }

        [ConfigurationProperty("mandrillApiKey", IsRequired = true)]
        public string MandrillApiKey
        {
            get { return (string)this["mandrillApiKey"]; }
            set { this["mandrillApiKey"] = value; }
        }

    }
}