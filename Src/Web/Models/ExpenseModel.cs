using System.ComponentModel.DataAnnotations;

namespace WebHJ.Models
{
    public class ExpenseModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Expense Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Amount")]
        public string Amount { get; set; }
    }
}