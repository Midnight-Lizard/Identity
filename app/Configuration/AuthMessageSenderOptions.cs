using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Configuration
{
    public class AuthMessageSenderOptions
    {
        public string SENDGRID_API_KEY { get; set; }
        public string IDENTITY_SERVICE_EMAIL { get; set; }
        public string IDENTITY_SERVICE_DISPLAY_NAME { get; set; }
    }
}
