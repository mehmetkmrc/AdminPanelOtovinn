using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthManual.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string MailAddress { get; set; }
        public string Password { get; set; }
        public Guid RoleId { get; set; } // Add RoleId property

        public List<SelectListItem> RoleList { get; set; } // Add RoleList property

        public Role Role { get; set; }
        public Boolean EmailConfirmed { get; set; } = false;
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime Last_Login { get; set; }
		public string TimeZoneId { get; set; }
	}
}








