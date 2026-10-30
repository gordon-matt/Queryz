using Extenso.Data.Entity;
using LogVue.Data.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Queryz.Data.MySql;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddQueryzMySql(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    options.UseInMemoryDatabase("QueryzDb");
                }
                else
                {
                    options.UseMySQL(connectionString, mySql =>
                        mySql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
                }
            });

            services.AddScoped<ApplicationDbContextBase>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddSingleton<IDbContextFactory, ApplicationDbContextFactory>();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddLogVueMySql(connectionString);
            }

            return services;
        }
    }
}
