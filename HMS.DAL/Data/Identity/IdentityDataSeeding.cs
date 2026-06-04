using HMS.DAL.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HMS.DAL.Data.Identity
{
    public class IdentityDataSeeding(
        RoleManager<IdentityRole> _roleManager,
        UserManager<ApplicationUser> _userManager,
        IdentityHospitalDbContext _identityContext)
    {
        public async Task SeedAsync()
        {
            // Apply any pending migrations for the Identity DB
            var pending = await _identityContext.Database.GetPendingMigrationsAsync();
            if (pending.Any())
                await _identityContext.Database.MigrateAsync();

            // Seed the 6 roles
            string[] roles = ["SuperAdmin", "HospitalAdmin", "Doctor", "Nurse", "Patient", "Receptionist"];
            foreach (var role in roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            // Seed default SuperAdmin
            if (!_userManager.Users.Any())
            {
                var admin = new ApplicationUser
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = "superadmin",
                    Email = "admin@hospital.com",
                    IsEmailVerified = true,
                };
                await _userManager.CreateAsync(admin, "P@ssw0rd!");
                await _userManager.AddToRoleAsync(admin, "SuperAdmin");
            }
        }
    }
}
