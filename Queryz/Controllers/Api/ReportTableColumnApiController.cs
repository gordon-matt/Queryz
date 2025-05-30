using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Queryz.Data.Entities;

namespace Queryz.Controllers.Api;

public class ReportTableColumnApiController : GenericODataController<ReportTableColumn, int>
{
    public ReportTableColumnApiController(IAuthorizationService authorizationService, IRepository<ReportTableColumn> repository)
        : base(authorizationService, repository)
    {
    }

    protected override int GetId(ReportTableColumn entity) => entity.Id;

    protected override void SetNewId(ReportTableColumn entity)
    {
    }
}