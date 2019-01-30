using Microsoft.Extensions.Options;
using MidnightLizard.Web.Identity.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly AuthMessageSenderOptions options;
        private readonly Task<string> emailConfirmationTemplate;
        private readonly Task<string> passwordResetTemplate;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            var path = "./wwwroot/templates";
            this.options = optionsAccessor.Value;
            this.emailConfirmationTemplate = File.ReadAllTextAsync($"{path}/confirm-email.htm");
            this.passwordResetTemplate = File.ReadAllTextAsync($"{path}/reset-password.htm");
        }

        private Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(this.options.SENDGRID_API_KEY);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(
                    this.options.IDENTITY_SERVICE_EMAIL,
                    this.options.IDENTITY_SERVICE_DISPLAY_NAME),
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

        public async Task SendEmailConfirmationRequestAsync(string userDispalyName, string userEmail, string callbackLink)
        {
            var html = HtmlEncoder.Default;
            var message = (await this.emailConfirmationTemplate)
                .Replace("{user:dispaly-name}", html.Encode(userDispalyName ?? "dear user"))
                .Replace("{user:email}", html.Encode(userEmail ?? "hidden"))
                .Replace("{callback:link}", html.Encode(callbackLink));

            await this.SendEmailAsync(userEmail, "Confirm your email", message);
        }

        public async Task SendPasswordResetRequestAsync(string userDispalyName, string userEmail, string callbackLink)
        {
            var html = HtmlEncoder.Default;
            var message = (await this.passwordResetTemplate)
                .Replace("{user:dispaly-name}", html.Encode(userDispalyName))
                .Replace("{user:email}", html.Encode(userEmail))
                .Replace("{callback:link}", html.Encode(callbackLink));

            await this.SendEmailAsync(userEmail, "Reset Password", message);
        }
    }
}
