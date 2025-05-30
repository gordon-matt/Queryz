namespace Queryz.Data.Entities;

public class ReportSorting : BaseEntity<int>
{
    public int ReportId { get; set; }

    public string ColumnName { get; set; }

    public SortDirection SortDirection { get; set; }

    public byte Ordinal { get; set; }

    public virtual Report Report { get; set; }
}

public class ReportSortingMap : IEntityTypeConfiguration<ReportSorting>
{
    public void Configure(EntityTypeBuilder<ReportSorting> builder)
    {
        builder.ToTable("ReportSortings", "qryz");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ColumnName).IsRequired().HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.SortDirection).IsRequired();
        builder.Property(x => x.Ordinal).IsRequired();
        builder.Property(x => x.ReportId).IsRequired();

        builder.HasOne(x => x.Report).WithMany(x => x.Sortings).HasForeignKey(x => x.ReportId);
    }

    public bool IsEnabled => true;
}