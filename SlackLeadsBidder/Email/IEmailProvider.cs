using System.Net.Mail;

namespace SlackLeadsBidder.Email
{
    public interface IEmailProvider
    {

        IEmailDelivery Prepare(string name, MailMessage message);

    }
}
