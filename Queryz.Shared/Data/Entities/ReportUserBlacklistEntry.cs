namespace Queryz.Data.Entities;

public class ReportUserBlacklistEntry : IEntity
{
    public int ReportId { get; set; }

    public string UserId { get; set; }

    public virtual Report Report { get; set; }

    #region IEntity Members

    public object[] KeyValues => new object[] { ReportId, UserId };

    #endregion IEntity Members
}

public class ReportUserBlacklistEntryMap : IEntityTypeConfiguration<ReportUserBlacklistEntry>
{
    public void Configure(EntityTypeBuilder<ReportUserBlacklistEntry> builder)
    {
        builder.ToTable("ReportUserBlacklist", "qryz");
        builder.HasKey(x => new { x.ReportId, x.UserId });
        builder.Property(x => x.ReportId).IsRequired();
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(128).IsUnicode(true);

        builder.HasOne(x => x.Report).WithMany().HasForeignKey(x => x.ReportId);
        //builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }

    public bool IsEnabled => true;
}