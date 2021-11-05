using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Queryz.Data.Domain;

namespace Queryz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataSource> ReportBuilder_DataSources { get; set; }

        public DbSet<Enumeration> ReportBuilder_Enumerations { get; set; }

        public DbSet<ReportGroupRole> ReportBuilder_ReportGroupRoles { get; set; }

        public DbSet<ReportGroup> ReportBuilder_ReportGroups { get; set; }

        public DbSet<Report> ReportBuilder_Reports { get; set; }

        public DbSet<ReportSorting> ReportBuilder_ReportSortings { get; set; }

        public DbSet<ReportTableColumn> ReportBuilder_ReportTableColumns { get; set; }

        public DbSet<ReportTable> ReportBuilder_ReportTables { get; set; }

        public DbSet<ReportUserBlacklistEntry> ReportBuilder_ReportUserBlacklist { get; set; }

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
}