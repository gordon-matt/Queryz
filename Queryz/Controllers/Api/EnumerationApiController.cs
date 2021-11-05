using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Controllers.Api
{
    public class EnumerationApiController : GenericODataController<Enumeration, int>
    {
        public EnumerationApiController(IRepository<Enumeration> repository)
            : base(repository)
        {
        }

        protected override int GetId(Enumeration entity) => entity.Id;

        protected override void SetNewId(Enumeration entity)
        {
        }
    }
}