using Microsoft.EntityFrameworkCore.Design;

namespace Queryz.Data.Sql;

public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=QueryzDesign;Trusted_Connection=True",
            sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
