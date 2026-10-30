namespace Queryz.Data.Entities;

/// <summary>A user who must not run this report. The old wizard called this the user blacklist.</summary>
public class ReportUserBlacklistEntry : BaseEntity<int>
{
    public int ReportId { get; set; }

    public required string UserId { get; set; }
}

public class ReportUserBlacklistEntryMap : IEntityTypeConfiguration<ReportUserBlacklistEntry>
{
    public void Configure(EntityTypeBuilder<ReportUserBlacklistEntry> builder)
    {
        builder.ToTable("ReportUserBlacklist", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.UserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.HasIndex(m => new { m.ReportId, m.UserId }).IsUnique();
    }
}
