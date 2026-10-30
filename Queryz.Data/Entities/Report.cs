namespace Queryz.Data.Entities;

/// <summary>
/// A saved SELECT. <see cref="RulesJson"/> is QueryKrafter rules; the SQL is generated from the
/// server's column list when the report runs, so a stored rule cannot name a column that was never offered.
/// </summary>
public class Report : BaseEntity<int>
{
    public required string Name { get; set; }

    public int GroupId { get; set; }

    public int DataSourceId { get; set; }

    public required string TableName { get; set; }

    public bool IsDistinct { get; set; }

    public int? RowLimit { get; set; }

    public bool Enabled { get; set; }

    public bool EmailEnabled { get; set; }

    public EnumerationHandling EnumerationHandling { get; set; }

    public string RulesJson { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }
}

public class ReportMap : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.Name).IsUnicode(true);
        builder.Property(m => m.TableName).IsRequired().HasMaxLength(Constants.MaxLengths.ColumnName).IsUnicode(false);
        builder.Property(m => m.RulesJson).IsRequired().IsUnicode(true);
        builder.Property(m => m.EmailEnabled).IsRequired();
        builder.Property(m => m.EnumerationHandling).IsRequired();
        builder.HasIndex(m => new { m.GroupId, m.Name });
    }
}
