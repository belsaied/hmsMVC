namespace HMS.BLL.Shared.Dtos.UserManagementDtos
{
    public record AuthResultDto
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public int ExpiresIn { get; init; }
        public UserInfoDto User { get; init; } = null!;
        public string? EmailVerificationToken { get; init; }
    }
}

