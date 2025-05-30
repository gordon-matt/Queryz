namespace Queryz.Data.Entities;

public class ReportTable : BaseEntity<int>
{
    public int ReportId { get; set; }

    public string Name { get; set; }

    public string ParentTable { get; set; }

    public string PrimaryKeyColumn { get; set; }

    public string ForeignKeyColumn { get; set; }

    public JoinType JoinType { get; set; }

    public virtual Report Report { get; set; }

    public bool IsEmpty => string.IsNullOrEmpty(ParentTable) &&
                string.IsNullOrEmpty(PrimaryKeyColumn) &&
                string.IsNullOrEmpty(ForeignKeyColumn);
}

public class ReportTableMap : IEntityTypeConfiguration<ReportTable>
{
    public void Configure(EntityTypeBuilder<ReportTable> builder)
    {
        builder.ToTable("ReportTables", "qryz");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.ParentTable).HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.PrimaryKeyColumn).HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.ForeignKeyColumn).HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.JoinType).IsRequired();

        builder.HasOne(x => x.Report).WithMany(x => x.Tables).HasForeignKey(x => x.ReportId);

        builder.Ignore(x => x.IsEmpty);
    }

    public bool IsEnabled => true;
}