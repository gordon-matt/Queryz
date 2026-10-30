using Queryz.Data.MySql;
using Queryz.Data.Npgsql;
using Queryz.Data.Sql;
using Queryz.Data.Sqlite;
using QueryKrafter;

namespace Queryz.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static AuthProvider AddQueryz(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<QueryzOptions>? configure = null)
    {
        var options = new QueryzOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddScoped<IQueryzAuthorization, QueryzAuthorization>();
        services.AddHttpContextAccessor();
        services.AddQueryKrafter();
        services.AddQueryzDatabase(configuration);
        services.AddQueryzRepositories();
        services.AddQueryzServices();
        var authProvider = services.AddQueryzAuthentication(configuration);
        services.AddScoped<IShellNavigation, QueryzNavigation>();
        services.Configure<ForgeShellOptions>(shell =>
        {
            shell.LogoUrl ??= "/_content/Queryz/img/Logo.png";
            shell.MarkUrl ??= "/_content/Queryz/img/Icon.png";
        });
        return authProvider;
    }

    public static WebApplication MapQueryz(this WebApplication app)
    {
        app.MapReportExport();
        return app;
    }

    public static IServiceCollection AddQueryzDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string provider = DatabaseProviders.Resolve(
            configuration[DatabaseProviders.ConfigurationKey],
            DatabaseProviders.Sqlite,
            [.. DatabaseProviders.All]);

        return provider switch
        {
            DatabaseProviders.Sqlite => services.AddQueryzSqlite(configuration),
            DatabaseProviders.SqlServer => services.AddQueryzSqlServer(configuration),
            DatabaseProviders.Npgsql => services.AddQueryzNpgsql(configuration),
            DatabaseProviders.MySql => services.AddQueryzMySql(configuration),
            _ => throw new InvalidOperationException($"Unsupported {DatabaseProviders.ConfigurationKey} '{provider}'."),
        };
    }

    public static IServiceCollection AddQueryzRepositories(this IServiceCollection services)
    {
        services.AddEntityFrameworkRepository();
        return services;
    }

    public static AuthProvider AddQueryzAuthentication(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddNetForgeAuthentication<ApplicationUser, ApplicationRole, ApplicationDbContextBase>(
            configuration);
}
