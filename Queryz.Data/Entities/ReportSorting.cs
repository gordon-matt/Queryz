namespace Queryz.Data.Entities;

public class ReportSorting : BaseEntity<int>
{
    public int ReportId { get; set; }

    public required string ColumnName { get; set; }

    public ReportSortDirection SortDirection { get; set; }

    public int Ordinal { get; set; }
}

public class ReportSortingMap : IEntityTypeConfiguration<ReportSorting>
{
    public void Configure(EntityTypeBuilder<ReportSorting> builder)
    {
        builder.ToTable("ReportSortings", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ColumnName).IsRequired().HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.SortDirection).IsRequired();
        builder.HasIndex(m => new { m.ReportId, m.Ordinal });
    }
}
