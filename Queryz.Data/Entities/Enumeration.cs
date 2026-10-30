namespace Queryz.Data.Entities;

public class Enumeration : BaseEntity<int>
{
    public required string Name { get; set; }

    /// <summary>JSON array of <c>{ id, name }</c>, the same shape the old wizard stored.</summary>
    public string Values { get; set; } = "[]";

    public bool IsBitFlags { get; set; }
}

public class EnumerationMap : IEntityTypeConfiguration<Enumeration>
{
    public void Configure(EntityTypeBuilder<Enumeration> builder)
    {
        builder.ToTable("Enumerations", Constants.Schemas.App);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(Constants.MaxLengths.Name).IsUnicode(true);
        builder.Property(m => m.Values).IsRequired().IsUnicode(true);
        builder.HasIndex(m => m.Name).IsUnique();
    }
}
