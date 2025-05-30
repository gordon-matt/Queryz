namespace Queryz.Data.Entities;

public class ReportTableColumn : BaseEntity<int>
{
    public int ReportId { get; set; }

    public string Name { get; set; }

    public string Alias { get; set; }

    public int Ordinal { get; set; }

    public bool IsLiteral { get; set; }

    public bool IsForeignKey { get; set; }

    // If the "Name" property refers to a foreign key column, then "IsForeignKey" should be true and this
    //  property ("DisplayColumn") should be used for getting a column from the parent table to be
    //  used for display.
    //  TODO: Implement...
    public string DisplayColumn { get; set; }

    public int? EnumerationId { get; set; }

    public string TransformFunction { get; set; }

    public string Format { get; set; }

    public bool IsHidden { get; set; }

    public virtual Report Report { get; set; }

    public virtual Enumeration Enumeration { get; set; }
}

public class ReportTableColumnMap : IEntityTypeConfiguration<ReportTableColumn>
{
    public void Configure(EntityTypeBuilder<ReportTableColumn> builder)
    {
        builder.ToTable("ReportTableColumns", "qryz");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReportId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.Alias).HasMaxLength(255).IsUnicode(true);
        builder.Property(x => x.Ordinal).IsRequired();
        builder.Property(x => x.IsLiteral).IsRequired();
        builder.Property(x => x.IsForeignKey).IsRequired();
        builder.Property(x => x.DisplayColumn).HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.TransformFunction).HasMaxLength(128).IsUnicode(false);
        builder.Property(x => x.Format).HasMaxLength(128).IsUnicode(true);
        builder.Property(x => x.IsHidden).IsRequired();

        builder.HasOne(x => x.Report).WithMany(x => x.Columns).HasForeignKey(x => x.ReportId);
        builder.HasOne(x => x.Enumeration).WithMany(x => x.Columns).HasForeignKey(x => x.EnumerationId);
    }

    public bool IsEnabled => true;
}