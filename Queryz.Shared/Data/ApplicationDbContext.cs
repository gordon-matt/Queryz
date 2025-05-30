using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Queryz.Data.Entities;

namespace Queryz.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataSource> DataSources { get; set; }

    public DbSet<Enumeration> Enumerations { get; set; }

    public DbSet<ReportGroupRole> ReportGroupRoles { get; set; }

    public DbSet<ReportGroup> ReportGroups { get; set; }

    public DbSet<Report> Reports { get; set; }

    public DbSet<ReportSorting> ReportSortings { get; set; }

    public DbSet<ReportTableColumn> ReportTableColumns { get; set; }

    public DbSet<ReportTable> ReportTables { get; set; }

    public DbSet<ReportUserBlacklistEntry> ReportUserBlacklist { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new DataSourceMap());
        modelBuilder.ApplyConfiguration(new EnumerationMap());
        modelBuilder.ApplyConfiguration(new ReportGroupRoleMap());
        modelBuilder.ApplyConfiguration(new ReportGroupMap());
        modelBuilder.ApplyConfiguration(new ReportMap());
        modelBuilder.ApplyConfiguration(new ReportSortingMap());
        modelBuilder.ApplyConfiguration(new ReportTableColumnMap());
        modelBuilder.ApplyConfiguration(new ReportTableMap());
        modelBuilder.ApplyConfiguration(new ReportUserBlacklistEntryMap());
    }
}