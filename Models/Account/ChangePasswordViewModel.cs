using System.ComponentModel.DataAnnotations;

namespace ASM_1.Models.Account
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; } = string.Empty;


        [Required]
        [StringLength(100, ErrorMessage = "{0} phải tối thiểu {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận không trùng khớp.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
