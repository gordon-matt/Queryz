namespace Queryz.Data.Entities;

public class DataSource : BaseEntity<int>
{
    private ICollection<Report> reports;

    public string Name { get; set; }

    public DataProvider DataProvider { get; set; }

    public string ConnectionString { get; set; }

    public string CustomProperties { get; set; }

    public virtual ICollection<Report> Reports
    {
        get => reports ??= []; set => reports = value;
    }
}

public class DataSourceMap : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("DataSources", "qryz");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128).IsUnicode(false);
        builder.Property(x => x.DataProvider).IsRequired();
        builder.Property(x => x.ConnectionString).IsRequired().HasMaxLength(255).IsUnicode(false);
        builder.Property(x => x.CustomProperties).HasMaxLength(512);
    }

    public bool IsEnabled => true;
}