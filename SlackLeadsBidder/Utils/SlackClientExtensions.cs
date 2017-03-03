using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Slack.Webhooks;

namespace SlackLeadsBidder.Utils
{
    public static class SlackClientExtensions
    {

        public static void PostError(this SlackClient client, string message)
        {
            client.Post(new SlackMessage()
            {
                Attachments = new List<SlackAttachment>()
                {
                    new SlackAttachment()
                    {
                        Text = $"Failed:{message}",
                        Color = "danger",
                    }
                }
            });
        }

    }
}