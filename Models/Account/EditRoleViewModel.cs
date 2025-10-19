using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Account
{
    public class EditRoleViewModel
    {
        [Required]
        public string RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        public string RoleName { get; set; }
    }
}
