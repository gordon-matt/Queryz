using Queryz.Data.Entities;

namespace Queryz.Services;

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