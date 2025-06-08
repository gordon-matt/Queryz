using Microsoft.Extensions.Configuration;

namespace Queryz.Data;

public class QueryzDbContextFactory<TUser, TRole> : IDbContextFactory
    where TUser : IdentityUser
    where TRole : IdentityRole
{
    private readonly IConfiguration configuration;

    public QueryzDbContextFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    private DbContextOptions<QueryzDbContext<TUser, TRole>> options;

    private DbContextOptions<QueryzDbContext<TUser, TRole>> Options
    {
        get
        {
            if (options == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<QueryzDbContext<TUser, TRole>>();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options = optionsBuilder.Options;
            }
            return options;
        }
    }

    public DbContext GetContext() => new QueryzDbContext<TUser, TRole>(Options);

    public DbContext GetContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QueryzDbContext<TUser, TRole>>();
        optionsBuilder.UseSqlServer(connectionString);
        return new QueryzDbContext<TUser, TRole>(optionsBuilder.Options);
    }
}