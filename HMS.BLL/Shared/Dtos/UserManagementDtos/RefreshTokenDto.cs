using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.UserManagementDtos
{
    public record RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;


    }
}
