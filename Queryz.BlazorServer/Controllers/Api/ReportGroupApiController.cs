using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.BlazorServer.Controllers.Api
{
    public class ReportGroupApiController : GenericODataController<ReportGroup, int>
    {
        public ReportGroupApiController(IRepository<ReportGroup> repository)
            : base(repository)
        {
        }

        protected override int GetId(ReportGroup entity)
        {
            return entity.Id;
        }

        protected override void SetNewId(ReportGroup entity)
        {
        }
    }
}