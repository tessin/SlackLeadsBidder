using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web.Http;
using System.Web.Mvc;
using Slack.Webhooks;
using SlackLeadsBidder.Config;
using SlackLeadsBidder.Controllers.Requests;
using SlackLeadsBidder.Services;
using SlackLeadsBidder.Utils;

namespace SlackLeadsBidder.Controllers
{
    public class SlackController : ApiController
    {

        [System.Web.Http.HttpPost]
        public void Bid(SlackCommandRequest request)
        {
            var settings = SlackLeadsBidderSettings.Settings;

            var agentClient = new SlackClient(request.Response_Url);

            if (request.Token != settings.SlackCommandBidToken)
            {
                agentClient.PostError("Invalid token.");
            }

            int amount;

            if (!int.TryParse(request.Text, out amount) || amount < 0)
            {

                SlackLeadsBidderService.GetAgentNextBid(request.User_Name, agentClient);
            }
            else
            {
                SlackLeadsBidderService.SetAgentNextBid(request.User_Name, amount, agentClient);
            }
        }

        [System.Web.Http.HttpPost]
        public void Autobid(SlackCommandRequest request)
        {
            var settings = SlackLeadsBidderSettings.Settings;

            var agentClient = new SlackClient(request.Response_Url);

            if (request.Token != settings.SlackCommandAutoBidToken)
            {
                agentClient.PostError("Invalid token.");
            }

            int amount;

            if (!int.TryParse(request.Text, out amount) || amount < 0)
            {
                SlackLeadsBidderService.GetAgentAutoBid(request.User_Name, agentClient);
            }
            else
            {
                SlackLeadsBidderService.SetAgentAutoBid(request.User_Name, amount, agentClient);
            }
        }

        [System.Web.Http.HttpPost]
        public void Bids(SlackCommandRequest request)
        {
            var settings = SlackLeadsBidderSettings.Settings;

            var agentClient = new SlackClient(request.Response_Url);

            if (request.Token != settings.SlackCommandStatusToken)
            {
                agentClient.PostError("Invalid token.");
            }

            SlackLeadsBidderService.ShowBids(agentClient);
        }

        [System.Web.Http.HttpPost]
        public void Balance(SlackCommandRequest request)
        {
            var settings = SlackLeadsBidderSettings.Settings;

            var agentClient = new SlackClient(request.Response_Url);

            if (request.Token != settings.SlackCommandBalanceToken)
            {
                agentClient.PostError("Invalid token.");
            }

            SlackLeadsBidderService.ShowBalance(agentClient);
        }

    }
}
