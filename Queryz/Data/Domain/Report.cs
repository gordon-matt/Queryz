using System;
using System.Collections.Generic;
using Extenso.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Queryz.Data.Domain
{
    public class Report : BaseEntity<int>
    {
        private ICollection<ReportTable> tables;
        private ICollection<ReportTableColumn> columns;
        private ICollection<ReportSorting> sortings;

        public string Name { get; set; }

        public int GroupId { get; set; }

        public int DataSourceId { get; set; }

        public DateTime LastModifiedUtc { get; set; }

        public bool IsDistinct { get; set; }

        public int? RowLimit { get; set; }

        public string Filters { get; set; }

        public EnumerationHandling EnumerationHandling { get; set; }

        public bool Enabled { get; set; }

        public bool EmailEnabled { get; set; }

        #region Navigation Properties

        public virtual ReportGroup Group { get; set; }

        public virtual DataSource DataSource { get; set; }

        public virtual ICollection<ReportTable> Tables
        {
            get { return tables ?? (tables = new List<ReportTable>()); }
            set { tables = value; }
        }

        public virtual ICollection<ReportTableColumn> Columns
        {
            get { return columns ?? (columns = new List<ReportTableColumn>()); }
            set { columns = value; }
        }

        public virtual ICollection<ReportSorting> Sortings
        {
            get { return sortings ?? (sortings = new List<ReportSorting>()); }
            set { sortings = value; }
        }

        #endregion Navigation Properties
    }

    public enum EnumerationHandling : byte
    {
        ShowText = 0,
        ShowNumericValue = 1,
        ShowBoth = 2
    }

    public class ReportMap : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Queryz_Reports");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(128).IsUnicode(false);
            builder.Property(x => x.GroupId).IsRequired();
            builder.Property(x => x.DataSourceId).IsRequired();
            builder.Property(x => x.LastModifiedUtc).IsRequired();
            builder.Property(x => x.IsDistinct).IsRequired();
            builder.Property(x => x.Filters).IsUnicode(true);
            builder.Property(x => x.EnumerationHandling).IsRequired();
            builder.Property(x => x.Enabled).IsRequired();
            builder.Property(x => x.EmailEnabled).IsRequired();

            builder.HasOne(x => x.Group).WithMany(x => x.Reports).HasForeignKey(x => x.GroupId);
            builder.HasOne(x => x.DataSource).WithMany(x => x.Reports).HasForeignKey(x => x.DataSourceId);

            builder.HasMany(x => x.Columns).WithOne(x => x.Report).HasForeignKey(x => x.ReportId);
            builder.HasMany(x => x.Tables).WithOne(x => x.Report).HasForeignKey(x => x.ReportId);
            builder.HasMany(x => x.Sortings).WithOne(x => x.Report).HasForeignKey(x => x.ReportId);
        }

        public bool IsEnabled => true;
    }
}