using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MidnightLizard.Web.Identity.Configuration
{
    public static class Certificate
    {
        private static readonly string _certificatePath = "/etc/secret/certificate";
        private static readonly string _certificatePassPath = "/etc/secret/password";
        public static X509Certificate2 Get(IConfiguration configuration, ILogger logger)
        {
            if (!File.Exists(_certificatePath))
            {
                return null;
            }
            try
            {
                return new X509Certificate2(_certificatePath, File.ReadAllText(_certificatePassPath),
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create signin certificate. Developer signin certiticate will be used.");
                return null;
            }
        }
    }
}
