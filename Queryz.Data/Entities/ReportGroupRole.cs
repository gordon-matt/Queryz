namespace Queryz.Data.Entities;

public class ReportGroupRole : BaseEntity<int>
{
    public int ReportGroupId { get; set; }

    public required string RoleId { get; set; }
}

public class ReportGroupRoleMap : IEntityTypeConfiguration<ReportGroupRole>
{
    public void Configure(EntityTypeBuilder<ReportGroupRole> builder)
    {
        builder.ToTable("ReportGroupRoles", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.RoleId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.HasIndex(m => new { m.ReportGroupId, m.RoleId }).IsUnique();
    }
}
