using System.Collections.Generic;
using Extenso.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Queryz.Data.Domain
{
    public class ReportGroup : BaseEntity<int>
    {
        private ICollection<Report> reports;

        //private ICollection<EmailReport> emailReports;
        private ICollection<ReportGroupRole> reportGroupRoles;

        public string Name { get; set; }

        public string TimeZoneId { get; set; }

        public virtual ICollection<Report> Reports
        {
            get { return reports ??= new List<Report>(); }
            set { reports = value; }
        }

        //public virtual ICollection<EmailReport> EmailReports
        //{
        //    get { return emailReports ??= new List<EmailReport>(); }
        //    set { emailReports = value; }
        //}

        public virtual ICollection<ReportGroupRole> ReportGroupRoles
        {
            get { return reportGroupRoles ??= new List<ReportGroupRole>(); }
            set { reportGroupRoles = value; }
        }
    }

    public class ReportGroupMap : IEntityTypeConfiguration<ReportGroup>
    {
        public void Configure(EntityTypeBuilder<ReportGroup> builder)
        {
            builder.ToTable("Queryz_ReportGroups");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255).IsUnicode(true);
            builder.Property(x => x.TimeZoneId).HasMaxLength(50).IsUnicode(false);
        }

        public bool IsEnabled => true;
    }
}