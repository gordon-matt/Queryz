using Queryz.Data.Entities;

namespace Queryz.Services;

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