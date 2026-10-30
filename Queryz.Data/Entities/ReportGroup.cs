namespace Queryz.Data.Entities;

public class ReportGroup : BaseEntity<int>
{
    public required string Name { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;
}

public class ReportGroupMap : IEntityTypeConfiguration<ReportGroup>
{
    public void Configure(EntityTypeBuilder<ReportGroup> builder)
    {
        builder.ToTable("ReportGroups", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.Name).IsUnicode(true);
        builder.Property(m => m.TimeZoneId).HasMaxLength(Constants.MaxLengths.TimeZoneId).IsUnicode(false);
        builder.HasIndex(m => m.Name).IsUnique();
    }
}
