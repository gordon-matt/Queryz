using Queryz.Data.Entities;

namespace Queryz.Services;

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