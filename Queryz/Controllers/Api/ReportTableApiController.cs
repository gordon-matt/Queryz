using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Queryz.Data.Entities;

namespace Queryz.Controllers.Api;

public class ReportTableApiController : GenericODataController<ReportTable, int>
{
    public ReportTableApiController(IAuthorizationService authorizationService, IRepository<ReportTable> repository)
        : base(authorizationService, repository)
    {
    }

    protected override int GetId(ReportTable entity) => entity.Id;

    protected override void SetNewId(ReportTable entity)
    {
    }
}