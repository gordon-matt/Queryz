using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Queryz.Data.Sqlite;

public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(
            "Data Source=queryz_design.db",
            sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
        optionsBuilder.ConfigureWarnings(w => w.Ignore(SqliteEventId.SchemaConfiguredWarning));
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
