using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IReportTableService : IGenericDataService<ReportTable>
    {
    }

    public class ReportTableService : GenericDataService<ReportTable>, IReportTableService
    {
        public ReportTableService(IRepository<ReportTable> repository)
            : base(repository)
        {
        }
    }
}