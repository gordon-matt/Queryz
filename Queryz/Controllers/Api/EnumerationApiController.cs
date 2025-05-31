namespace Queryz.Controllers.Api;

public class EnumerationApiController : GenericODataController<Enumeration, int>
{
    public EnumerationApiController(IAuthorizationService authorizationService, IRepository<Enumeration> repository)
        : base(authorizationService, repository)
    {
    }

    protected override int GetId(Enumeration entity) => entity.Id;

    protected override void SetNewId(Enumeration entity)
    {
    }
}