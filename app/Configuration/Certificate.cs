using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IO;

namespace MidnightLizard.Web.Identity.Configuration
{
    public static class Certificate
    {
        private static readonly string _certificatePath = "/etc/secret/certificate";
        public static X509Certificate2 Get(IConfiguration configuration)
        {
            var certPass = configuration.GetValue<string>("IDENTITY_SERVER_SIGNIN_CERTIFICATE_PASSWORD");
            if (!File.Exists(_certificatePath))
            {
                return null;
            }
            try
            {
                return new X509Certificate2(_certificatePath, certPass,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            }
            catch
            {
                return null;
            }
        }
    }
}
