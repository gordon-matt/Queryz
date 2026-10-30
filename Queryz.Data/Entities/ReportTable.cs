namespace Queryz.Data.Entities;

/// <summary>
/// A table the report reads. The row with an empty <see cref="ParentTable"/> is the FROM table.
/// Every other row is a join back to another selected table.
/// </summary>
public class ReportTable : BaseEntity<int>
{
    public int ReportId { get; set; }

    public required string Name { get; set; }

    public int Ordinal { get; set; }

    public string ParentTable { get; set; } = string.Empty;

    public string PrimaryKeyColumn { get; set; } = string.Empty;

    public string ForeignKeyColumn { get; set; } = string.Empty;

    public ReportJoinType JoinType { get; set; }
}

public class ReportTableMap : IEntityTypeConfiguration<ReportTable>
{
    public void Configure(EntityTypeBuilder<ReportTable> builder)
    {
        builder.ToTable("ReportTables", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.ParentTable).HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.PrimaryKeyColumn).HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.ForeignKeyColumn).HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.JoinType).IsRequired();
        builder.HasIndex(m => new { m.ReportId, m.Ordinal });
    }
}
