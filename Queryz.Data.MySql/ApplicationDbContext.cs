namespace Queryz.Data.MySql;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : ApplicationDbContextBase(options)
{
}
