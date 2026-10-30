using Queryz.Data.MySql;
using Queryz.Data.Npgsql;
using Queryz.Data.Sql;
using Queryz.Data.Sqlite;

namespace Queryz.Infrastructure;

public static class ServiceCollectionExtensions
{
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
