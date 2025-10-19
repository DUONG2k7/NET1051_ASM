using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Account
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Tên người dùng hoặc Email")]
        public string UsernameOrEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}
