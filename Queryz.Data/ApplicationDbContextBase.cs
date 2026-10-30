using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Queryz.Data;

/// <summary>
/// Provider-agnostic context. Each <c>Queryz.Data.{Provider}</c> project subclasses this and
/// owns its own migration history, because EF bakes provider-specific column types into migrations.
/// </summary>
public abstract class ApplicationDbContextBase
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    protected ApplicationDbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<DataSource> DataSources => Set<DataSource>();

    public DbSet<ReportGroup> ReportGroups => Set<ReportGroup>();

    public DbSet<Report> Reports => Set<Report>();

    public DbSet<ReportColumn> ReportColumns => Set<ReportColumn>();

    public DbSet<ReportTable> ReportTables => Set<ReportTable>();

    public DbSet<ReportSorting> ReportSortings => Set<ReportSorting>();

    public DbSet<Enumeration> Enumerations => Set<Enumeration>();

    public DbSet<ReportGroupRole> ReportGroupRoles => Set<ReportGroupRole>();

    public DbSet<ReportUserBlacklistEntry> ReportUserBlacklist => Set<ReportUserBlacklistEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Constants.Schemas.App);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContextBase).Assembly);
        ConfigureIdentityTables(builder);
    }

    protected static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().ToTable("Users", Constants.Schemas.App);
        builder.Entity<ApplicationRole>().ToTable("Roles", Constants.Schemas.App);
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", Constants.Schemas.App);
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", Constants.Schemas.App);
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", Constants.Schemas.App);
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", Constants.Schemas.App);
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", Constants.Schemas.App);
    }
}
