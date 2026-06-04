using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.UserManagementDtos
{
    public record LoginDto
    {
        [Required][EmailAddress]
        public string Email { get; init; } = string.Empty;
        [Required] 
        public string Password { get; init; } = string.Empty;
    }
}
