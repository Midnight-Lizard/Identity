using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace MidnightLizard.Web.Identity.Configuration
{
    public static class Certificate
    {
        public static X509Certificate2 Get(IConfiguration configuration)
        {
            var certPass = configuration.GetValue<string>("IDENTITY_SERVER_SIGNIN_CERTIFICATE_PASSWORD");
            var certData = configuration.GetValue<string>("IDENTITY_SERVER_SIGNIN_CERTIFICATE");
            if (string.IsNullOrEmpty(certData) || string.IsNullOrEmpty(certPass))
            {
                return null;
            }
            try
            {
                return new X509Certificate2(Convert.FromBase64String(certData), certPass,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            }
            catch
            {
                return null;
            }
        }
    }
}
