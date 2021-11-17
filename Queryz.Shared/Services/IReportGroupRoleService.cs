using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
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
}