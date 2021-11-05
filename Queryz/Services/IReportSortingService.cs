using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IReportSortingService : IGenericDataService<ReportSorting>
    {
    }

    public class ReportSortingService : GenericDataService<ReportSorting>, IReportSortingService
    {
        public ReportSortingService(IRepository<ReportSorting> repository)
            : base(repository)
        {
        }
    }
}