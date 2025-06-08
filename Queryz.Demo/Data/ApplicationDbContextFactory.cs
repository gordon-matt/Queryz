using Extenso.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Queryz.Data;
using Queryz.Demo.Data.Entities;

namespace Queryz.Demo.Data;

public class ApplicationDbContextFactory : IDbContextFactory
{
    private readonly IConfiguration configuration;

    public ApplicationDbContextFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    private DbContextOptions<QueryzDbContext<ApplicationUser, ApplicationRole>> options;

    private DbContextOptions<QueryzDbContext<ApplicationUser, ApplicationRole>> Options
    {
        get
        {
            if (options == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<QueryzDbContext<ApplicationUser, ApplicationRole>>();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options = optionsBuilder.Options;
            }
            return options;
        }
    }

    public DbContext GetContext() => new ApplicationDbContext(Options);

    public DbContext GetContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QueryzDbContext<ApplicationUser, ApplicationRole>>();
        optionsBuilder.UseSqlServer(connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}