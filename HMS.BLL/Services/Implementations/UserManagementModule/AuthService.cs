using HMS.DAL.Contracts;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.PatientEnums;
using HMS.DAL.Models.IdentityModule;
using HMS.DAL.Models.PatientModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.PatientModule;
using HMS.BLL.Shared.Common;
using HMS.BLL.Shared.Dtos.UserManagementDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HMS.BLL.ServicesAbstraction.Contracts;

namespace HMS.BLL.Services.Implementations.UserManagementModule
{
    public class AuthService(
        UserManager<ApplicationUser> _userManager,
        RoleManager<IdentityRole> _roleManager,
        IOptions<JwtOptions> _jwtOptions,
        IAuditService _auditService,
        IEmailService _emailService,
        IUnitOfWork _unitOfWork,
        ICacheService _cacheService) : IAuthService
    {
        private static readonly string[] ProtectedRoles = ["SuperAdmin", "HospitalAdmin"];

        public async Task<AuthResultDto> RegisterAsync(RegisterDto dto, string? callerRole)
        {
            var adminOnlyRoles = new[] { "SuperAdmin", "HospitalAdmin" };
            var staffRoles = new[] { "Doctor", "Nurse", "Receptionist" };
            var publicRoles = new[] { "Patient" };

            var selfRegistrationAllowed = publicRoles.Contains(dto.Role) ||
                (dto.Role == "Doctor" && dto.DoctorId.HasValue);

            if (callerRole is null && !selfRegistrationAllowed)
                throw new ForbiddenException(
                    "Self-registration is only allowed for the 'Patient' role, or for Doctors using their assigned Doctor ID.");

            if (adminOnlyRoles.Contains(dto.Role) && callerRole != "SuperAdmin")
                throw new ForbiddenException(
                    $"Only a SuperAdmin can assign the '{dto.Role}' role.");

            var isDoctorSelfRegistering = dto.Role == "Doctor" && dto.DoctorId.HasValue && callerRole is null;

            if (staffRoles.Contains(dto.Role)
                && !isDoctorSelfRegistering
                && callerRole != "SuperAdmin"
                && callerRole != "HospitalAdmin")
                throw new ForbiddenException(
                    $"Only SuperAdmin or HospitalAdmin can assign the '{dto.Role}' role.");

            if (dto.Role == "Doctor")
            {
                if (!dto.DoctorId.HasValue)
                    throw new ValidationException(["DoctorId is required for the Doctor role."]);

                var doctorExists = await _unitOfWork.GetRepository<Doctor, int>()
                    .GetByIdAsync(dto.DoctorId.Value);
                if (doctorExists is null)
                    throw new ValidationException([$"Doctor with ID {dto.DoctorId.Value} does not exist."]);

                if (_userManager.Users.Any(u => u.DoctorId == dto.DoctorId))
                    throw new ValidationException(["This DoctorId is already linked to another account."]);
            }

            // ── Patient Registration ─────────────────────────────────────────────
            if (dto.Role == "Patient")
            {
                if (dto.PatientInfo is null)
                    throw new ValidationException(["PatientInfo is required when registering as a Patient."]);

                // Duplicate NationalId check
                var existingNationalId = await _unitOfWork.GetRepository<Patient, int>()
                    .CountAsync(new PatientByNationalIdSpecification(dto.PatientInfo.NationalId));
                if (existingNationalId > 0)
                    throw new ConflictException(
                        $"A patient with NationalId '{dto.PatientInfo.NationalId}' already exists.");

                // Duplicate Email check on Patients table
                var existingPatientEmail = await _unitOfWork.GetRepository<Patient, int>()
                    .CountAsync(new PatientByEmailSpecification(dto.Email));
                if (existingPatientEmail > 0)
                    throw new ConflictException(
                        $"A patient with email '{dto.Email}' already exists.");

                // Duplicate Email check on Identity table
                var existingIdentityUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingIdentityUser is not null)
                    throw new ConflictException(
                        $"An account with email '{dto.Email}' already exists.");

                // Create Patient record
                var patient = new Patient
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Phone = dto.PatientInfo.Phone,
                    DateOfBirth = dto.PatientInfo.DateOfBirth,
                    Gender = dto.PatientInfo.Gender,
                    NationalId = dto.PatientInfo.NationalId,
                    Address = new Address
                    {
                        Street = dto.PatientInfo.Address?.Street ?? string.Empty,
                        City = dto.PatientInfo.Address?.City ?? string.Empty,
                        Country = dto.PatientInfo.Address?.Country ?? string.Empty,
                        PostalCode = dto.PatientInfo.Address?.PostalCode ?? string.Empty
                    },
                    RegistrationDate = DateTime.UtcNow,
                    Status = PatientStatus.Active,
                    MedicalRecordNumber = $"TEMP-{Guid.NewGuid():N}"
                };

                var patientRepo = _unitOfWork.GetRepository<Patient, int>();
                await patientRepo.AddAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                // Assign final MRN now that we have the DB-generated Id
                patient.MedicalRecordNumber = $"MRN{patient.Id:D6}";
                patientRepo.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                // Create Identity user — roll back patient on failure
                var patientUser = new ApplicationUser
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PatientId = patient.Id,
                };

                var patientUserResult = await _userManager.CreateAsync(patientUser, dto.Password);
                if (!patientUserResult.Succeeded)
                {
                    patientRepo.Delete(patient);
                    await _unitOfWork.SaveChangesAsync();
                    throw new ValidationException(patientUserResult.Errors.Select(e => e.Description));
                }

                if (!await _roleManager.RoleExistsAsync(dto.Role))
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));

                await _userManager.AddToRoleAsync(patientUser, dto.Role);

                var patientRawToken = Guid.NewGuid().ToString();
                patientUser.EmailVerificationTokenHash = HashToken(patientRawToken);
                patientUser.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
                await _userManager.UpdateAsync(patientUser);

                try { await _emailService.SendVerificationEmailAsync(patientUser.Email!, patientRawToken); }
                catch { }

                return BuildAuthResult(patientUser, [], null, null, patientRawToken);
            }

            // ── All Other Roles (Doctor, Nurse, Receptionist, HospitalAdmin, SuperAdmin) ──
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new ValidationException(result.Errors.Select(e => e.Description));

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            var rawToken = Guid.NewGuid().ToString();
            user.EmailVerificationTokenHash = HashToken(rawToken);
            user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(user);

            try { await _emailService.SendVerificationEmailAsync(user.Email!, rawToken); }
            catch { }

            return BuildAuthResult(user, [], null, null, rawToken);
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                       ?? throw new UnauthorizedException("Invalid email or password.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                throw new AccountLockedException(user.LockoutEnd.Value);

            var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordOk)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                await _userManager.UpdateAsync(user);
                await _auditService.LogAsync(user.Id, "LOGIN_FAILED");
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.IsEmailVerified)
                throw new EmailNotVerifiedException();

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateJwt(user, roles);
            var (rawRefresh, hashRefresh) = GenerateRefreshToken();
            user.RefreshTokenHash = hashRefresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenDays);

            await _userManager.UpdateAsync(user);
            await _auditService.LogAsync(user.Id, "LOGIN_SUCCESS");

            return BuildAuthResult(user, roles, accessToken, rawRefresh);
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshTokenHash == hash)
                       ?? throw new UnauthorizedException("Invalid refresh token.");

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token has expired.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                throw new AccountLockedException(user.LockoutEnd.Value);

            var (rawRefresh, hashRefresh) = GenerateRefreshToken();
            user.RefreshTokenHash = hashRefresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenDays);
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateJwt(user, roles);

            return BuildAuthResult(user, roles, accessToken, rawRefresh);
        }

        public async Task LogoutAsync(string userId, string accessToken)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);

            var opts = _jwtOptions.Value;
            var expiry = TimeSpan.FromMinutes(opts.AccessTokenMinutes);
            await _cacheService.SetCacheValueAsync($"blacklist:{accessToken}", "revoked", expiry);

            await _auditService.LogAsync(userId, "LOGOUT");
        }

        public async Task VerifyEmailAsync(string token)
        {
            var hash = HashToken(token);
            var user = _userManager.Users
                           .FirstOrDefault(u => u.EmailVerificationTokenHash == hash)
                       ?? throw new UnauthorizedException("Invalid or expired verification token.");

            if (user.EmailVerificationExpiry < DateTime.UtcNow)
                throw new UnauthorizedException("Verification token has expired.");

            user.IsEmailVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationExpiry = null;
            await _userManager.UpdateAsync(user);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return;

            var rawToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenHash = HashToken(rawToken);
            user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);
            await _userManager.UpdateAsync(user);

            await _emailService.SendPasswordResetEmailAsync(email, rawToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var hash = HashToken(dto.Token);
            var user = _userManager.Users
                           .FirstOrDefault(u => u.PasswordResetTokenHash == hash)
                       ?? throw new UnauthorizedException("Invalid or expired reset token.");

            if (user.PasswordResetExpiry < DateTime.UtcNow)
                throw new UnauthorizedException("Reset token has expired.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
            if (!result.Succeeded)
                throw new ValidationException(result.Errors.Select(e => e.Description));

            user.PasswordResetTokenHash = null;
            user.PasswordResetExpiry = null;
            await _userManager.UpdateAsync(user);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private string GenerateJwt(ApplicationUser user, IList<string> roles)
        {
            var opts = _jwtOptions.Value;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email,          user.Email!),
                new(ClaimTypes.Name,           user.FullName),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            if (user.DoctorId.HasValue)
                claims.Add(new Claim("doctor_id", user.DoctorId.Value.ToString()));
            if (user.PatientId.HasValue)
                claims.Add(new Claim("patient_id", user.PatientId.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: opts.Issuer,
                audience: opts.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(opts.AccessTokenMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static (string raw, string hash) GenerateRefreshToken()
        {
            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (raw, HashToken(raw));
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        private AuthResultDto BuildAuthResult(
            ApplicationUser user, IList<string> roles,
            string? accessToken, string? refreshToken,
            string? emailVerificationToken = null)
        {
            return new AuthResultDto
            {
                AccessToken = accessToken ?? string.Empty,
                RefreshToken = refreshToken ?? string.Empty,
                ExpiresIn = _jwtOptions.Value.AccessTokenMinutes * 60,
                EmailVerificationToken = emailVerificationToken,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Roles = [.. roles],
                }
            };
        }
    }
}