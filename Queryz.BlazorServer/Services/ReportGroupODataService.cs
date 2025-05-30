using Queryz.Data.Entities;

namespace Queryz.BlazorServer.Services;

public class ReportGroupODataService : GenericODataService<ReportGroup>
{
    public ReportGroupODataService() : base("ReportGroupApi")
    {
    }
}