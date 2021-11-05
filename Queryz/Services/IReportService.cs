using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Services
{
    public interface IReportService : IGenericDataService<Report>
    {
    }

    public class ReportService : GenericDataService<Report>, IReportService
    {
        public ReportService(IRepository<Report> repository)
            : base(repository)
        {
        }
    }
}