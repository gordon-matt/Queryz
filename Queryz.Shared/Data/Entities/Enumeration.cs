namespace Queryz.Data.Entities;

public class Enumeration : BaseEntity<int>
{
    private ICollection<ReportTableColumn> columns;

    public string Name { get; set; }

    public string Values { get; set; }

    public bool IsBitFlags { get; set; }

    public virtual ICollection<ReportTableColumn> Columns
    {
        get => columns ??= []; set => columns = value;
    }
}

public class EnumerationMap : IEntityTypeConfiguration<Enumeration>
{
    public void Configure(EntityTypeBuilder<Enumeration> builder)
    {
        builder.ToTable("Enumerations", "qryz");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128).IsUnicode(false);
        builder.Property(x => x.Values).IsRequired().IsUnicode(true);
        builder.Property(x => x.IsBitFlags).IsRequired();
    }

    public bool IsEnabled => true;
}