using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.UserManagementModule
{
    public class ForceResetPasswordViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required, MinLength(8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
