using Microsoft.Extensions.DependencyInjection;
using Queryz.Infrastructure;

namespace Queryz.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddQueryz<TUser, TRole>(this IServiceCollection services, string connectionString)
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        services.AddDbContext<QueryzDbContext<TUser, TRole>>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IDbContextFactory, QueryzDbContextFactory<TUser, TRole>>();
        services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>)); // Fixed CS7003: Use typeof() for unbound generics

        services.AddSingleton<IRazorViewRenderService, QueryzRazorViewRenderService>();
        services.AddSingleton<IODataRegistrar, ODataRegistrar>();

        services.AddTransient<IDataSourceService, DataSourceService>();
        services.AddTransient<IEnumerationService, EnumerationService>();
        services.AddTransient<IReportService, ReportService>();
        services.AddTransient<IReportTableService, ReportTableService>();
        services.AddTransient<IReportTableColumnService, ReportTableColumnService>();
        services.AddTransient<IReportGroupService, ReportGroupService>();
        services.AddTransient<IReportGroupRoleService, ReportGroupRoleService>();
        services.AddTransient<IReportSortingService, ReportSortingService>();
        services.AddTransient<IReportUserBlacklistService, ReportUserBlacklistService>();

        // Transform Functions
        services.AddSingleton<ITransformFunction, BeautifyJsonFunction>();
        services.AddSingleton<ITransformFunction, ConvertTimeZoneFunction>();

        services.AddTransient<IReportBuilderService, ReportBuilderService>();

        services.AddTransient<IUserService, UserService<TUser>>();
        services.AddTransient<IRoleService, RoleService<TRole>>();

        return services;
    }
}