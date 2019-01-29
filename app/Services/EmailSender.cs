using Microsoft.Extensions.Options;
using MidnightLizard.Web.Identity.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly AuthMessageSenderOptions Options;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(this.Options.SENDGRID_API_KEY);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(this.Options.IDENTITY_SERVICE_EMAIL, this.Options.IDENTITY_SERVICE_DISPLAY_NAME),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}
