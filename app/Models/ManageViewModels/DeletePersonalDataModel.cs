using System.ComponentModel.DataAnnotations;

namespace MidnightLizard.Web.Identity.Models.ManageViewModels
{
    public class DeletePersonalDataModel
    {
        public bool RequirePassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
