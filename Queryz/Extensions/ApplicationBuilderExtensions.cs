//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;

//namespace Queryz.Extensions;

//public static class ApplicationBuilderExtensions
//{
//    public static async Task<IApplicationBuilder> UseQueryzAsync(this IApplicationBuilder app)
//    {
//        var context = app.ApplicationServices.GetRequiredService<ApplicationDbContext>();
//        var userManager = app.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>();
//        var roleManager = app.ApplicationServices.GetRequiredService<RoleManager<ApplicationRole>>();

//        await DbInitializer.InitializeAsync(context, userManager, roleManager);

//        return app;
//    }
//}