namespace HMS.BLL.Shared.Dtos.UserManagementDtos
{
    public record UserInfoDto
    {
        public string Id { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string[] Roles { get; init; } = [];
    }
}
