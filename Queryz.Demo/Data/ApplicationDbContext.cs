using Microsoft.EntityFrameworkCore;
using Queryz.Data;
using Queryz.Demo.Data.Entities;

namespace Queryz.Demo.Data;

public class ApplicationDbContext : QueryzDbContext<ApplicationUser, ApplicationRole>
{
    public ApplicationDbContext(DbContextOptions<QueryzDbContext<ApplicationUser, ApplicationRole>> options)
        : base(options)
    {
    }
}