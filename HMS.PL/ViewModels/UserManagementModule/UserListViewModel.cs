namespace HMS.PL.ViewModels.UserManagementModule
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
    }

    public class UserIndexPageViewModel
    {
        public IEnumerable<UserListViewModel> Users { get; set; } = [];
        public string? SearchQuery { get; set; }
        public string? RoleFilter { get; set; }
        public bool? LockedFilter { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
