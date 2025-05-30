using Queryz.Data.Entities;

namespace Queryz.Services;

public interface IReportTableColumnService : IGenericDataService<ReportTableColumn>
{
}

public class ReportTableColumnService : GenericDataService<ReportTableColumn>, IReportTableColumnService
{
    public ReportTableColumnService(IRepository<ReportTableColumn> repository)
        : base(repository)
    {
    }
}