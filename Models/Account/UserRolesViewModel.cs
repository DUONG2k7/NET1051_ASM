using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Account
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleSelectionVM> Roles { get; set; } = new List<RoleSelectionVM>();
    }
    public class RoleSelectionVM
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
