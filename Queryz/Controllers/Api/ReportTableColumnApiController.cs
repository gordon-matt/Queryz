using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Queryz.Data.Domain;

namespace Queryz.Controllers.Api
{
    public class ReportTableColumnApiController : GenericODataController<ReportTableColumn, int>
    {
        public ReportTableColumnApiController(IRepository<ReportTableColumn> repository)
            : base(repository)
        {
        }

        protected override int GetId(ReportTableColumn entity) => entity.Id;

        protected override void SetNewId(ReportTableColumn entity)
        {
        }
    }
}