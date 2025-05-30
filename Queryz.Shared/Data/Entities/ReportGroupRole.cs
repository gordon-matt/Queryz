namespace Queryz.Data.Entities;

public class ReportGroupRole : IEntity
{
    public int ReportGroupId { get; set; }

    public string RoleId { get; set; }

    public virtual ReportGroup ReportGroup { get; set; }

    //public virtual ApplicationRole Role { get; set; }

    #region IEntity Members

    public object[] KeyValues => new object[] { ReportGroupId, RoleId };

    #endregion IEntity Members
}

public class ReportGroupRoleMap : IEntityTypeConfiguration<ReportGroupRole>
{
    public void Configure(EntityTypeBuilder<ReportGroupRole> builder)
    {
        builder.ToTable("ReportGroupRoles", "qryz");
        builder.HasKey(x => new { x.ReportGroupId, x.RoleId });
        builder.Property(x => x.ReportGroupId).IsRequired();
        builder.Property(x => x.RoleId).IsRequired().HasMaxLength(450).IsUnicode(true);

        builder.HasOne(x => x.ReportGroup).WithMany(x => x.ReportGroupRoles).HasForeignKey(x => x.ReportGroupId);
        //builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
    }

    public bool IsEnabled => true;
}