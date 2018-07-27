using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Display name")]
        public string DisplayName { get; set; }
    }
}
