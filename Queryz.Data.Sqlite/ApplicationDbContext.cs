namespace Queryz.Data.Sqlite;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : ApplicationDbContextBase(options)
{
}
