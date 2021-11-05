using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Controllers.Api
{
    public class ReportTableApiController : GenericODataController<ReportTable, int>
    {
        public ReportTableApiController(IRepository<ReportTable> repository)
            : base(repository)
        {
        }

        protected override int GetId(ReportTable entity) => entity.Id;

        protected override void SetNewId(ReportTable entity)
        {
        }
    }
}