using System;

namespace MidnightLizard.Web.Identity.Models
{
    public class StandardErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}