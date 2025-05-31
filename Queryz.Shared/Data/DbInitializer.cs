using Queryz.Data.Entities;

namespace Queryz.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        await EnsureRoleExistsAsync(roleManager, SharedConstants.Roles.Administrators);
        await EnsureRoleExistsAsync(roleManager, SharedConstants.Roles.ReportBuilderEditors);
        await EnsureRoleExistsAsync(roleManager, SharedConstants.Roles.ReportBuilderUsers);

        await EnsureUserExistsAsync(userManager, "admin@queryz.com", "Admin@123!", SharedConstants.Roles.Administrators);
        await EnsureUserExistsAsync(userManager, "editor@queryz.com", "Editor@123!", SharedConstants.Roles.ReportBuilderEditors);
        await EnsureUserExistsAsync(userManager, "user@queryz.com", "User@123!", SharedConstants.Roles.ReportBuilderUsers);
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