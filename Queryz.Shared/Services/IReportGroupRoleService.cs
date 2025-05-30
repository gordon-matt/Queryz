using Queryz.Data.Entities;

namespace Queryz.Services;

public interface IReportGroupRoleService : IGenericDataService<ReportGroupRole>
{
}

public class ReportGroupRoleService : GenericDataService<ReportGroupRole>, IReportGroupRoleService
{
    public ReportGroupRoleService(IRepository<ReportGroupRole> repository)
        : base(repository)
    {
    }
}