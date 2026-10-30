namespace Queryz.Data.Entities;

public class DataSource : BaseEntity<int>
{
    public required string Name { get; set; }

    public ReportProvider Provider { get; set; }

    public required string ConnectionString { get; set; }
}

public class DataSourceMap : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("DataSources", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.Name).IsUnicode(true);
        builder.Property(m => m.Provider).IsRequired();
        builder.Property(m => m.ConnectionString).IsRequired().HasMaxLength(Constants.MaxLengths.ConnectionString).IsUnicode(false);
        builder.HasIndex(m => m.Name).IsUnique();
    }
}
