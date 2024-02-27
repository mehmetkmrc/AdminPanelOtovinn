using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthManual.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required] 
        public string Name { get; set; }

        [NotMapped]
        public string? RoleId { get; set; }
        [NotMapped]
        public string? Role { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
        public Boolean EmailConfirmed { get; set; } = false;
    }
}
