using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.UserManagementDtos
{
    public record ForgetPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256)]
        public string Email { get; init; } = string.Empty;

    }
}
