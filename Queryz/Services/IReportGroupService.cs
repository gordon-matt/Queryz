using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IReportGroupService : IGenericDataService<ReportGroup>
    {
    }

    public class ReportGroupService : GenericDataService<ReportGroup>, IReportGroupService
    {
        public ReportGroupService(IRepository<ReportGroup> repository)
            : base(repository)
        {
        }
    }
}