using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;

namespace SlackLeadsBidder.Email.Providers
{
    public class MandrillDelivery : IEmailDelivery
    {

        private readonly MandrillApi _api;

        private readonly EmailMessage _message;

        private List<EmailAttachment> _attachments;

        public MandrillDelivery(MandrillApi api, EmailMessage message)
        {
            _api = api;
            _message = message;
        }

        public IEmailDelivery AddRecipient(string email)
        {
            _message.To = (_message.To ?? new List<EmailAddress>()).Concat(new[] { new EmailAddress(email) });
            return this;
        }

        public IEmailDelivery AddAttachment(string base64Data, string fileName, string fileType)
        {
            _attachments = _attachments ?? new List<EmailAttachment>();
            _attachments.Add(new EmailAttachment()
            {
                Content = base64Data,
                Name = fileName,
                Type = fileType
            });
            return this;
        }

        public List<EmailDeliveryResult> Send()
        {
            _message.Attachments = _attachments;

            var results = Task.Run(() => _api.SendMessage(new SendMessageRequest(_message))).Result;

            return results.Select(e => new EmailDeliveryResult()
            {
                Email = e.Email,
                Success = e.Status == EmailResultStatus.Sent || e.Status == EmailResultStatus.Queued || e.Status == EmailResultStatus.Scheduled,
                ProviderResult = e.RejectReason,
                ProviderId = e.Id
            }).ToList();
        }

    }
}
