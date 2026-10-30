using Microsoft.AspNetCore.Identity;

namespace Queryz.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        AuthProvider authProvider,
        IConfiguration configuration,
        ILogger logger)
    {
        var context = services.GetRequiredService<ApplicationDbContextBase>();

        if (context.Database.IsRelational())
        {
            logger.LogInformation("Applying database migrations");
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (authProvider is AuthProvider.Identity or AuthProvider.Keycloak)
        {
            await SeedRolesAsync(services, logger);
        }

        if (authProvider == AuthProvider.Identity)
        {
            await SeedAdministratorAsync(services, configuration, logger);
        }
    }

    private static async Task SeedRolesAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetService<RoleManager<ApplicationRole>>();
        if (roleManager is null)
        {
            return;
        }

        var roles = new (string Name, string Description)[]
        {
            (WellKnownRoles.Administrator, "Manages the instance"),
            (WellKnownRoles.User, "Runs and edits reports"),
        };

        foreach (var (name, description) in roles)
        {
            if (await roleManager.RoleExistsAsync(name))
            {
                continue;
            }

            await roleManager.CreateAsync(new ApplicationRole { Name = name, Description = description });
            logger.LogInformation("Created role {Role}", name);
        }
    }

    private static async Task SeedAdministratorAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var userManager = services.GetService<UserManager<ApplicationUser>>();
        if (userManager is null)
        {
            return;
        }

        var existing = await userManager.GetUsersInRoleAsync(WellKnownRoles.Administrator);
        if (existing.Count > 0)
        {
            return;
        }

        string? password = configuration["Seed:Admin:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("No administrator exists and Seed:Admin:Password is not set.");
            return;
        }

        string email = configuration["Seed:Admin:Email"] ?? "admin@queryz.local";
        string userName = configuration["Seed:Admin:UserName"] is { Length: > 0 } configuredName
            ? configuredName
            : email;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Administrator",
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            logger.LogError(
                "Could not create the seed administrator: {Errors}",
                string.Join("; ", created.Errors.Select(error => error.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, WellKnownRoles.Administrator);
        await userManager.AddToRoleAsync(user, WellKnownRoles.User);
        logger.LogWarning("Seed administrator {UserName} was created from configuration. Change its password.", userName);
    }
}
