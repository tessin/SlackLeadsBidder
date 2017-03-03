using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Models;
using SlackLeadsBidder.Config;

namespace SlackLeadsBidder.Email.Providers
{
    public class MandrillProvider : IEmailProvider
    {

        public IEmailDelivery Prepare(string name, MailMessage message)
        {
            MandrillApi api = new MandrillApi(SlackLeadsBidderSettings.Settings.MandrillApiKey);

            EmailMessage _message = new EmailMessage()
            {
                Html = message.Body,
                Subject = message.Subject,
                FromEmail = message.From.Address,
                FromName = message.From.DisplayName,
                //TrackClicks = true,
                //TrackOpens = true,
                Tags = new[] { name },
                //AutoText = true
            };

            if (message.Bcc.Any())
            {
                _message.BccAddress = message.Bcc.First().Address;
            }

            return new MandrillDelivery(api, _message);
        }

    }
}
