namespace Queryz.Controllers.Api;

public class ReportSortingApiController : GenericODataController<ReportSorting, int>
{
    public ReportSortingApiController(IAuthorizationService authorizationService, IRepository<ReportSorting> repository)
        : base(authorizationService, repository)
    {
    }

    protected override int GetId(ReportSorting entity) => entity.Id;

    protected override void SetNewId(ReportSorting entity)
    {
    }
}