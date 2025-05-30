using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using Queryz.Data.Entities;

namespace Queryz.BlazorServer.Controllers.Api;

public class ReportGroupApiController : GenericODataController<ReportGroup, int>
{
    public ReportGroupApiController(IAuthorizationService authorizationService, IRepository<ReportGroup> repository)
        : base(authorizationService, repository)
    {
    }

    protected override int GetId(ReportGroup entity) => entity.Id;

    protected override void SetNewId(ReportGroup entity)
    {
    }
}