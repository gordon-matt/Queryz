namespace Queryz.Data.Entities;

public class ReportColumn : BaseEntity<int>
{
    public int ReportId { get; set; }

    public required string Name { get; set; }

    public string Alias { get; set; } = string.Empty;

    public int Ordinal { get; set; }

    public bool IsLiteral { get; set; }

    public bool IsHidden { get; set; }

    public string Format { get; set; } = string.Empty;

    public int? EnumerationId { get; set; }

    public string TransformFunction { get; set; } = string.Empty;
}

public class ReportColumnMap : IEntityTypeConfiguration<ReportColumn>
{
    public void Configure(EntityTypeBuilder<ReportColumn> builder)
    {
        builder.ToTable("ReportColumns", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.Alias).HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(true);
        builder.Property(m => m.Format).HasMaxLength(Constants.MaxLengths.Format).IsUnicode(true);
        builder.Property(m => m.TransformFunction).HasMaxLength(Constants.MaxLengths.Name).IsUnicode(false);
        builder.HasIndex(m => new { m.ReportId, m.Ordinal });
    }
}
