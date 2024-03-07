using System.ComponentModel.DataAnnotations;

namespace AuthManual.Models
{
    public class LoginViewModel
    {
        [EmailAddress]
        public string MailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}
