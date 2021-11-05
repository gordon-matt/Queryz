using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IReportTableColumnService : IGenericDataService<ReportTableColumn>
    {
    }

    public class ReportTableColumnService : GenericDataService<ReportTableColumn>, IReportTableColumnService
    {
        public ReportTableColumnService(IRepository<ReportTableColumn> repository)
            : base(repository)
        {
        }
    }
}