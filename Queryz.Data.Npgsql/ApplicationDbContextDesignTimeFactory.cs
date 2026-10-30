using Microsoft.EntityFrameworkCore.Design;

namespace Queryz.Data.Npgsql;

public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=queryz_design;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
