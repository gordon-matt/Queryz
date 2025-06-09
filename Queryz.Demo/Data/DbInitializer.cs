using Microsoft.AspNetCore.Identity;
using Queryz.Demo.Data.Entities;

namespace Queryz.Demo.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        await EnsureRoleExistsAsync(roleManager, QueryzConstants.Roles.Administrators);
        await EnsureRoleExistsAsync(roleManager, QueryzConstants.Roles.ReportBuilderEditors);
        await EnsureRoleExistsAsync(roleManager, QueryzConstants.Roles.ReportBuilderUsers);

        await EnsureUserExistsAsync(userManager, "admin@queryz.com", "Admin@123!", QueryzConstants.Roles.Administrators);
        await EnsureUserExistsAsync(userManager, "editor@queryz.com", "Editor@123!", QueryzConstants.Roles.ReportBuilderEditors);
        await EnsureUserExistsAsync(userManager, "user@queryz.com", "User@123!", QueryzConstants.Roles.ReportBuilderUsers);
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<ApplicationRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole(role));
        }
    }

    private static async Task EnsureUserExistsAsync(UserManager<ApplicationUser> userManager, string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, role);
            }
        }
    }
}