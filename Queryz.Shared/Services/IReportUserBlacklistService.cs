using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IReportUserBlacklistService : IGenericDataService<ReportUserBlacklistEntry>
    {
    }

    public class ReportUserBlacklistService : GenericDataService<ReportUserBlacklistEntry>, IReportUserBlacklistService
    {
        public ReportUserBlacklistService(IRepository<ReportUserBlacklistEntry> repository)
            : base(repository)
        {
        }
    }
}