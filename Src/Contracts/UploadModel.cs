using System.ComponentModel.DataAnnotations;

namespace HansJuergenWeb.Contracts
{
    public class UploadModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Allergene")]
        public string Allergene { get; set; }
    }
}