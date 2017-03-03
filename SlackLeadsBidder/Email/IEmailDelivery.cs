using System.Collections.Generic;

namespace SlackLeadsBidder.Email
{
    public interface IEmailDelivery
    {

        IEmailDelivery AddAttachment(string base64Data, string fileName, string fileType);

        IEmailDelivery AddRecipient(string email);

        List<EmailDeliveryResult> Send();

    }
}
