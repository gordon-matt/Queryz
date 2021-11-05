using System.Collections.Generic;
using Extenso.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Queryz.Data.Domain
{
    public class DataSource : BaseEntity<int>
    {
        private ICollection<Report> reports;

        public string Name { get; set; }

        public DataProvider DataProvider { get; set; }

        public string ConnectionString { get; set; }

        public string CustomProperties { get; set; }

        public virtual ICollection<Report> Reports
        {
            get { return reports ?? (reports = new List<Report>()); }
            set { reports = value; }
        }
    }

    public class DataSourceMap : IEntityTypeConfiguration<DataSource>
    {
        public void Configure(EntityTypeBuilder<DataSource> builder)
        {
            builder.ToTable("Queryz_DataSources");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(128).IsUnicode(false);
            builder.Property(x => x.DataProvider).IsRequired();
            builder.Property(x => x.ConnectionString).IsRequired().HasMaxLength(255).IsUnicode(false);
            builder.Property(x => x.CustomProperties).HasMaxLength(512);
        }

        public bool IsEnabled => true;
    }
}