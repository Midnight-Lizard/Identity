using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Services
{
    public interface IEmailSender
    {
        //Task SendEmailAsync(string email, string subject, string message);

        Task SendEmailConfirmationRequestAsync(string userDispalyName, string userEmail, string callbackLink);
        Task SendPasswordResetRequestAsync(string userDispalyName, string userEmail, string callbackLink);
    }
}
